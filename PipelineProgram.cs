using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class PipelineProgram
	{
		public readonly Framebuffer FrameBuffer;
		private readonly ResourceFactory factory;
		private ShaderBase shader;
		private bool useErrorShader;

		private Pipeline pipeline;

		private CommandList ourCommandList;
		private Fence fence;

		private BlendStateDescription? blendState;
		
		public PipelineProgram(Framebuffer frameBuffer, ResourceFactory factory, ShaderBase shader, BlendStateDescription? blendState = null)
		{
			this.FrameBuffer = frameBuffer;
			this.factory = factory;
			this.shader = shader;
			this.blendState = blendState;

			Main.AssetManager.ShaderLoader.OnAssetChanged += OnShaderAssetChanged;
		}

		public ShaderBase GetShader()
		{
			if (useErrorShader)
				return Main.ErrorShader;
			else return shader;
		}

		public void Clear(CommandList commandList, RgbaFloat clearColor, int index = 0)
		{
			commandList.ClearColorTarget((uint)index, clearColor);
		}

		public void Clear(GraphicsDevice device, RgbaFloat clearColor, int index = 0)
		{
			if (ourCommandList == null)
			{
				ourCommandList = factory.CreateCommandList();
				fence = factory.CreateFence(false);
			}

			ourCommandList.Begin();
			
			Bind(ourCommandList);
			Clear(ourCommandList, clearColor, index);

			ourCommandList.End();

			device.SubmitCommands(ourCommandList, fence);
		}

		public void Bind(CommandList commandList)
		{
			if (pipeline == null)
			{
				CreatePipeline();
			}

			commandList.SetFramebuffer(FrameBuffer);
			commandList.SetPipeline(pipeline);
		}

		public void BindShader(CommandList commandList)
		{
			GetShader().Bind(commandList);
		}

		private void CreatePipeline()
		{
			Shader[] shaders = GetShader().GetShaders();
			ResourceLayout[] layout = GetShader().GetUniformLayout();

			pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
			{
				BlendState = blendState.GetValueOrDefault(BlendStateDescription.SingleAlphaBlend),
				DepthStencilState = DepthStencilStateDescription.Disabled,
				RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
				PrimitiveTopology = PrimitiveTopology.TriangleList,
				ResourceLayouts = layout,
				ShaderSet = new ShaderSetDescription(new VertexLayoutDescription[] { DefaultVertexDefinitions.VertexPositionTextureColor.GetLayout() }, shaders),
				Outputs = FrameBuffer.OutputDescription
			});
		}

		private void OnShaderAssetChanged(string asset)
		{
			var shaders = shader.GetShaderNamesToWatchForChanges();
			
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
						}
						catch (Exception e)
						{
							Console.WriteLine(e.ToString());

							Console.WriteLine("\n----\nError loading shaders! Falling back to default texture.");

							useErrorShader = true;
						}

						CreatePipeline();

						break;
					}
				}
			}
		}
	}
}
