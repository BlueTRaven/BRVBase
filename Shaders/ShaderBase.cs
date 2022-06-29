using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public abstract class ShaderBase : IDisposable
	{
		public readonly struct TextureAndSamplerSlot
		{
			public readonly int slot;
			public readonly string texName;

			public TextureAndSamplerSlot(int slot, string texName)
			{
				this.slot = slot;
				this.texName = texName;
			}
		}

		protected static List<WeakReference<ShaderResourceManager[]>> trackedManagers = new List<WeakReference<ShaderResourceManager[]>>();

		private Shader[] shaders;

		private ShaderResourceManager[] defaultManagers;

		//private DeviceBuffer transformBuffer;

		private TextureAndSamplerSlot[] textureSlots;
		//Note that this does not represent what textures are actually bound on the gpu side.
		private TextureAndSampler[] boundTextures;

		private ResourceLayout[] resourceLayouts;
        protected readonly ResourceFactory factory;
		protected readonly GraphicsDevice device;

		protected readonly bool ModelMatrix;

        private bool disposedValue;

		public ShaderBase(GraphicsDevice device, ResourceFactory factory, int numTextures)
		{
			this.device = device;
			this.factory = factory;

			shaders = LoadShaders();

			textureSlots = new TextureAndSamplerSlot[numTextures];

			for (int i = 0; i < numTextures; i++)
			{
				textureSlots[i] = new TextureAndSamplerSlot(i, "Texture" + (i + 1));
			}

			boundTextures = new TextureAndSampler[numTextures];

			/*defaultUniforms = new ShaderUniformManager(factory, device, "Default", "Default", new Dictionary<string, ShaderResourceManager.UniformValidator>()
			{
				{ "ViewProj", new ShaderResourceManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Model", new ShaderResourceManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Tint", new ShaderResourceManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Vertex) }
			});

			defaultUniforms.Set("ViewProj", Matrix4x4.Identity, ShaderStages.Vertex);
			defaultUniforms.Set("Model", Matrix4x4.Identity, ShaderStages.Vertex);*/
		}

		public ShaderBase(GraphicsDevice device, ResourceFactory factory, TextureAndSamplerSlot[] slots)
		{
			this.device = device;
			this.factory = factory;

			shaders = LoadShaders();

			textureSlots = slots;

			boundTextures = new TextureAndSampler[slots.Length];

			defaultManagers = CreateResourceManagers();
			/*defaultUniforms = new ShaderUniformManager(factory, device, "Default", "Default", new Dictionary<string, ShaderResourceManager.UniformValidator>()
			{
				{ "ViewProj", new ShaderResourceManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Model", new ShaderResourceManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Tint", new ShaderResourceManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Vertex) }
			});

			defaultUniforms.Set("ViewProj", Matrix4x4.Identity, ShaderStages.Vertex);
			defaultUniforms.Set("Model", Matrix4x4.Identity, ShaderStages.Vertex);*/
		}

		public abstract Shader[] LoadShaders();

		//Unloads all shaders. They will be reloaded next time they are bound.
		public void ReloadShaders()
		{
			Util.DisposeShaders(shaders);
			shaders = LoadShaders();
		}

		public virtual void SetTexture(int index, TextureAndSampler texture, ShaderResourceManager uniformManager)
		{
			if (boundTextures.Length < index)
				throw new Exception();

			boundTextures[index] = texture;
		}

		public virtual VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionTextureColor.GetLayout();
		}

		public Shader[] GetShaders()
		{
			if (shaders == null)
				shaders = LoadShaders();

			return shaders;
		}

		//Helper function to bind all uniforms at the same time.
		//Use stackalloc!
		public void Bind(CommandList commandList, Span<ShaderResourceManager> resources)
		{
			for (uint i = 0; i < resources.Length; i++)
            {
				resources[(int)i].Bind(commandList, i);
            }
		}

		public void Bind(CommandList commandList, IList<ShaderResourceManager> resources)
		{
			for (uint i = 0; i < resources.Count; i++)
			{
				resources[(int)i].Bind(commandList, i);
			}
		}

		public ResourceLayout[] GetResourceLayouts()
        {
			if (resourceLayouts == null)
				resourceLayouts = CreateResourceLayouts();

			return resourceLayouts;
        }

		protected abstract ResourceLayout[] CreateResourceLayouts();

		public ShaderResourceManager[] GetDefaultResourceManagers()
        {
			return defaultManagers;
        }

		public ShaderResourceManager[] CreateResourceManagers()
        {
			ResourceLayout[] layouts = GetResourceLayouts();

			ShaderResourceManager[] managers = CreateResourceManagers(layouts);

			trackedManagers.Add(new WeakReference<ShaderResourceManager[]>(managers));

			return managers;
		}

		protected abstract ShaderResourceManager[] CreateResourceManagers(ResourceLayout[] layouts);

		public virtual string[] GetShaderNamesToWatchForChanges()
		{
			return null;
		}

		protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
					for (int i = 0; i < shaders.Length; i++)
						shaders[i].Dispose();
                }

				foreach (WeakReference<ShaderResourceManager[]> managerswf in trackedManagers)
                {
					if (managerswf.TryGetTarget(out ShaderResourceManager[] managers))
                    {
						for (int i = 0; i < managers.Length; i++)
							managers[i].Dispose();
                    }
                }

				//transformBuffer.Dispose();

				disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ShaderBase()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
