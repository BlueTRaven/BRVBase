using BRVBase.Services;
using BRVBase.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	//TODO: make IDisposable
	public class PipelineProgram : IDisposable
	{
		public static event Func<ShaderBase> GetErrorShaderEvent;
		public static Dictionary<string, string> CurrentShaderErrors = new Dictionary<string, string>();

		public readonly Framebuffer Framebuffer;
		private readonly ResourceFactory factory;
		private ShaderBase shader;
		private bool useErrorShader;

		private Pipeline pipelineTriangleTopology;
		private Pipeline pipelineLineTopology;

		private CommandList ourCommandList;
		private Fence fence;

		private BlendStateDescription? blendState;
		private DepthStencilStateDescription? depthStencilState;
		private RasterizerStateDescription? rasterizerState;
		private PrimitiveTopology? topology;
		private bool pipelineDirty;
		private Pipeline defaultPipeline;

        private bool disposedValue;

        public PipelineProgram(Framebuffer frameBuffer, ResourceFactory factory, ShaderBase shader)
		{
			this.Framebuffer = frameBuffer;
			this.factory = factory;
			this.shader = shader;

			ServiceManager.Instance.GetService<AssetManager>().ShaderLoader.OnAssetChanged += OnShaderAssetChanged;
		}

		public void SetPipelineState(BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null,
			PrimitiveTopology? topology = null)
		{
			if (blendState.HasValue)
				this.blendState = blendState;
			if (depthStencilState.HasValue)
				this.depthStencilState = depthStencilState;
			if (rasterizerState.HasValue)
			this.rasterizerState = rasterizerState;
			if (topology.HasValue && topology.Value != this.topology)
				this.topology = topology;

			pipelineDirty = true;
		}

		//Sets the current pipeline state as the default. Calling ResetPipelineState will reset the current pipeline state to this.
		public void SetDefaultPipelineState()
		{
			if (pipelineTriangleTopology == null || pipelineDirty)
			{
				CreatePipeline();
			}

			defaultPipeline = pipelineTriangleTopology;
		}

		//Whew, that's a mouthful.
		//If the default state has not already been set, this sets the default state to the current state.
		public void SetCurrentStateAsDefaultIfNotAlreadySet(Span<ShaderResourceManager> uniforms)
		{
			if (defaultPipeline != null)
				SetDefaultPipelineState();
		}

		//Resets the pipeline state to the default. Note that this does NOT affect shader parameters. These will need to be manually reset.
		public void ResetPipelineState()
		{
			pipelineTriangleTopology = defaultPipeline;
		}

		public ShaderBase GetShader()
		{
			if (useErrorShader)
			{
				ShaderBase shader = GetErrorShaderEvent?.Invoke();

				if (shader == null)
					Console.WriteLine("PipelineProgram.GetErrorShader should be assigned to! Shader hot-reloading will not work otherwise!");

				return shader;			
			}
			else return shader;
		}

		public Texture GetFrameBufferTexture(int id = 0)
		{
			return Framebuffer.ColorTargets[id].Target;
		}

		public Texture GetFramebufferDepthTexture()
        {
			return Framebuffer.DepthTarget?.Target;
        }

		//TODO: this shouldn't be in PipelineProgram
		//As different pipelines can use the same backbuffer
		public void Clear(CommandList commandList, RgbaFloat clearColor, int index = 0)
		{
			commandList.ClearColorTarget((uint)index, clearColor);
		}

		public void ClearAll(CommandList commandList, RgbaFloat clearColor)
        {
			for (int i = 0; i < Framebuffer.ColorTargets.Count; i++)
            {
				commandList.ClearColorTarget((uint)i, clearColor);
            }
        }

		public void ClearDepth(CommandList commandList, float clearValue = 0)
		{
			commandList.ClearDepthStencil(clearValue);
		}

		public void Clear(GraphicsDevice device, RgbaFloat clearColor, float clearDepth)
		{
			if (ourCommandList == null)
			{
				ourCommandList = factory.CreateCommandList();
				fence = factory.CreateFence(false);
			}

			ourCommandList.Begin();
			
			Bind(ourCommandList);

			for (int i = 0; i < this.Framebuffer.ColorTargets.Count; i++)
				Clear(ourCommandList, clearColor, i);
			if (this.Framebuffer.DepthTarget != null)
				ClearDepth(ourCommandList, clearDepth);

			ourCommandList.End();

			device.SubmitCommands(ourCommandList, fence);
		}

		public void Bind(CommandList commandList)
		{
			if (disposedValue)
				throw new ObjectDisposedException(this.ToString());
			if (Framebuffer.IsDisposed)
				throw new ObjectDisposedException(Framebuffer.ToString());

			if (pipelineTriangleTopology == null || pipelineDirty)
			{
				CreatePipeline();
			}

			if (pipelineTriangleTopology.IsDisposed)
				throw new ObjectDisposedException(pipelineTriangleTopology.ToString());

			commandList.SetFramebuffer(Framebuffer);
			commandList.SetPipeline(pipelineTriangleTopology);
		}

		private void CreatePipeline()
		{
			pipelineTriangleTopology?.Dispose();
			pipelineLineTopology?.Dispose();

			Shader[] shaders = GetShader().GetShaders();
			ResourceLayout[] layout = GetShader().GetResourceLayouts();

			pipelineTriangleTopology = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
			{
				BlendState = blendState.GetValueOrDefault(BlendStateDescription.SingleAlphaBlend),
				DepthStencilState = depthStencilState.GetValueOrDefault(DepthStencilStateDescription.Disabled),
				RasterizerState = rasterizerState.GetValueOrDefault(RasterizerStateDescription.Default),
				PrimitiveTopology = PrimitiveTopology.TriangleList,

				ResourceLayouts = layout,
				ShaderSet = new ShaderSetDescription(new VertexLayoutDescription[] { shader.GetVertexLayout() }, shaders),
				Outputs = Framebuffer.OutputDescription,
			});

			pipelineDirty = false;

			if (defaultPipeline != null)
				defaultPipeline = pipelineTriangleTopology;
		}

		private void OnShaderAssetChanged(string asset)
		{
			ShaderAttribute attr = Util.GetAttribute<ShaderAttribute>(shader);

			var shaders = attr.GetNamesArr();
			
			if (shaders != null)
			{
				foreach (string name in shaders)
				{
					if (asset == name)
					{
						try
						{
							shader.ReloadShaders();

							useErrorShader = false;

							if (CurrentShaderErrors.ContainsKey(asset))
								CurrentShaderErrors.Remove(asset);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.ToString());

							Console.WriteLine("\n----\nError loading shaders! Falling back to default texture.");

							if (CurrentShaderErrors.ContainsKey(asset))
								CurrentShaderErrors[asset] = e.ToString();
							else CurrentShaderErrors.Add(asset, e.ToString());

							useErrorShader = true;
						}

						pipelineDirty = true;
						//CreatePipeline();

						break;
					}
				}
			}
		}

        protected virtual void Dispose(bool disposing, bool withFramebuffer)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

				}

				ServiceManager.Instance.GetService<AssetManager>().ShaderLoader.OnAssetChanged -= OnShaderAssetChanged;

				//pipelines are lazy initialized so need a null check
				pipelineLineTopology?.Dispose();
				pipelineTriangleTopology?.Dispose();
				ourCommandList?.Dispose();
				fence?.Dispose();

				if (withFramebuffer)
                {
					foreach (var colorTarget in Framebuffer.ColorTargets) 
					{
						colorTarget.Target.Dispose(); 
					}

					Framebuffer.DepthTarget?.Target.Dispose();

					Framebuffer.Dispose();
                }

				disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PipelineProgram()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

		public bool IsDisposed()
        {
			return disposedValue;
        }

        public void Dispose()
        {
            Dispose(true, false);
            GC.SuppressFinalize(this);
        }

		public void DisposeWithFramebuffer()
        {
			Dispose(true, true);
			GC.SuppressFinalize(this);
		}
    }
}
