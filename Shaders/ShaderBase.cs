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

		private Shader[] shaders;

		private ShaderUniformManager defaultUniforms;
		//private DeviceBuffer transformBuffer;

		private TextureAndSamplerSlot[] textureSlots;
		//Note that this does not represent what textures are actually bound on the gpu side.
		private TextureAndSampler[] boundTextures;

		private ResourceSet[] uniformSet;
		private ResourceLayout[] uniformLayout;
		private bool defaultUniformsDirty;
		private bool definedUniformsDirty;
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

			defaultUniforms = new ShaderUniformManager(factory, device, "Default", "Default", new Dictionary<string, ShaderUniformManager.UniformValidator>()
			{
				{ "ViewProj", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Model", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Tint", new ShaderUniformManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Vertex) }
			});

			defaultUniforms.Set("ViewProj", Matrix4x4.Identity, ShaderStages.Vertex);
			defaultUniforms.Set("Model", Matrix4x4.Identity, ShaderStages.Vertex);
		}

		public ShaderBase(GraphicsDevice device, ResourceFactory factory, TextureAndSamplerSlot[] slots)
		{
			this.device = device;
			this.factory = factory;

			shaders = LoadShaders();

			textureSlots = slots;

			boundTextures = new TextureAndSampler[slots.Length];

			defaultUniforms = new ShaderUniformManager(factory, device, "Default", "Default", new Dictionary<string, ShaderUniformManager.UniformValidator>()
			{
				{ "ViewProj", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Model", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
				{ "Tint", new ShaderUniformManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Vertex) }
			});

			defaultUniforms.Set("ViewProj", Matrix4x4.Identity, ShaderStages.Vertex);
			defaultUniforms.Set("Model", Matrix4x4.Identity, ShaderStages.Vertex);
		}

		public abstract Shader[] LoadShaders();

		//Unloads all shaders. They will be reloaded next time they are bound.
		public void ReloadShaders()
		{
			Util.DisposeShaders(shaders);
			shaders = LoadShaders();
		}

		public void SetViewProj(CommandList commandList, Matrix4x4 viewProj)
		{
			defaultUniforms.Set("ViewProj", viewProj, ShaderStages.Vertex, commandList);
			/*if (transformBuffer == null)
				CreateDefaultBuffer();
			commandList.UpdateBuffer(transformBuffer, 0, viewProj);

			defaultUniformsDirty = true;*/
		}

		public void SetModel(CommandList commandList, Matrix4x4 model)
		{
			defaultUniforms.Set("Model", model, ShaderStages.Vertex, commandList);
			/*if (transformBuffer == null)
				CreateDefaultBuffer();
			commandList.UpdateBuffer(transformBuffer, (uint)Marshal.SizeOf(typeof(Matrix4x4)), model);

			defaultUniformsDirty = true;*/
		}

		public void SetTint(CommandList commandList, RgbaFloat color)
		{
			defaultUniforms.Set("Tint", color, ShaderStages.Vertex);
			/*if (transformBuffer == null)
				CreateDefaultBuffer();
			commandList.UpdateBuffer(transformBuffer, (uint)Marshal.SizeOf(typeof(Matrix4x4)) * 2, color.ToVector4());

			defaultUniformsDirty = true;*/
		}

		/*private void CreateDefaultBuffer()
		{
			DeviceBufferBuilder builder = new DeviceBufferBuilder(factory).Mat4x4().Mat4x4().Vector4();
			transformBuffer = builder.Build();

			uint offset = 0;
			device.UpdateBuffer(transformBuffer, offset, Matrix4x4.Identity);
			offset += (uint)Marshal.SizeOf(typeof(Matrix4x4));
			device.UpdateBuffer(transformBuffer, offset, Matrix4x4.Identity);
			offset += (uint)Marshal.SizeOf(typeof(Matrix4x4));
			device.UpdateBuffer(transformBuffer, offset, Vector4.One);
		}*/

		public virtual void SetTexture(int index, TextureAndSampler texture)
		{
			if (boundTextures.Length < index)
				throw new Exception();

			boundTextures[index] = texture;

			defaultUniformsDirty = true;
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

		public ResourceLayout[] GetUniformLayout(List<ShaderUniformManager> uniforms)
		{
			ResourceLayout[] layout = new ResourceLayout[uniforms.Count];
			for (uint i = 0; i < uniforms.Count; i++)
            {
				layout[(int)i] = uniforms[(int)i].GetLayout();
            }

			return layout;

			/*if (uniformLayout == null)
			{
				uniformLayout = new ResourceLayout[2];
				uniformLayout[0] = CreateDefaultLayout();
				uniformLayout[1] = CreateUserDefinedResourceLayout();

				if (uniformLayout[1] == null)
				{
					var old = uniformLayout[0];
					uniformLayout = new ResourceLayout[1];
					uniformLayout[0] = old;
				}
			}

			return uniformLayout;*/
		}

		/*public ResourceLayout GetDefaultLayout()
		{
			return GetUniformLayout()[0];
		}

		public ResourceLayout GetUserDefinedLayout()
		{
			return GetUniformLayout()[1];
		}*/

		private ResourceLayout CreateDefaultLayout()
		{
			ResourceLayoutBuilder builder = new ResourceLayoutBuilder(factory).Uniform("Default", ShaderStages.Vertex);

			for (int i = 0; i < textureSlots.Length; i++)
			{
				builder = builder.Texture(textureSlots[i].texName, ShaderStages.Fragment)
					.Sampler(textureSlots[i].texName + "Sampler", ShaderStages.Fragment);
			}

			return builder.Build();
		}

		protected abstract ResourceLayout CreateUserDefinedResourceLayout();

		[Obsolete("Bind uniforms manually to avoid list allocation")]
		public void Bind(CommandList commandList, List<ShaderUniformManager> uniforms)
		{
			for (uint i = 0; i < uniforms.Count; i++)
            {
				uniforms[(int)i].Bind(commandList, i);
            }

			/*var sets = GetResourceSet();

			commandList.SetGraphicsResourceSet(0, sets[0]);
			if (sets[1] != null)
				commandList.SetGraphicsResourceSet(1, sets[1]);*/
		}

		public ResourceLayout[] GetLayout(Span<ShaderUniformManager> uniforms)
        {
			ResourceLayout[] layout = new ResourceLayout[uniforms.Length];

			for (int i = 0; i < uniforms.Length; i++)
            {
				layout[i] = uniforms[i].GetLayout();
            }

			return layout;
        }

		private ResourceSet[] GetResourceSet()
		{
			if (uniformSet == null)
				uniformSet = new ResourceSet[2];

			uniformSet[0] = defaultUniforms.GetSet();
			uniformSet[1] = CreateUserDefinedResourceSet();

			/*if (uniformSet == null)
			{
				uniformSet = new ResourceSet[2];
				defaultUniformsDirty = true;
				definedUniformsDirty = true;
			}
			
			if (defaultUniformsDirty)
			{
				bool failed = false;
				//TODO get rid of allocations
				List<BindableResource> resources = new List<BindableResource>();
				resources.Add(transformBuffer);
				for (int i = 0; i < boundTextures.Length; i++)
				{
					if (boundTextures[i].texture == null)
					{
						Console.WriteLine("Attempting to bind a null texture. All texture slots for a given program must be filled!");
						failed = true;
						break;
					}

					resources.Add(boundTextures[i].texture);
					resources.Add(boundTextures[i].sampler);
				}

				if (!failed)
				{
					uniformSet[0]?.Dispose();
					uniformSet[0] = factory.CreateResourceSet(new ResourceSetDescription(GetUniformLayout()[0], resources.ToArray()));
				}

				defaultUniformsDirty = false;
			}

			if (definedUniformsDirty)
			{
				uniformSet[1]?.Dispose();
				uniformSet[1] = CreateUserDefinedResourceSet();
				definedUniformsDirty = false;
			}*/

			return uniformSet;
		}

		//TODO: rename/remove, no longer creates since we handle that in ShaderUniformManager.
		protected abstract ResourceSet CreateUserDefinedResourceSet();

		protected void MarkDefaultUniformsDirty(bool immediateCache = false)
		{
			defaultUniformsDirty = true;

			if (immediateCache)
				GetResourceSet();
		}

		protected void MarkUserDefinedUniformsDirty(bool immediateCache = false)
		{
			definedUniformsDirty = true;

			if (immediateCache)
				GetResourceSet();
		}

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
					defaultUniforms.Dispose();
					for (int i = 0; i < shaders.Length; i++)
						shaders[i].Dispose();
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
