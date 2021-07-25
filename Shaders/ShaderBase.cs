using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public abstract class ShaderBase
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

		private DeviceBuffer transformBuffer;

		private TextureAndSamplerSlot[] textureSlots;
		//Note that this does not represent what textures are actually bound on the gpu side.
		private TextureAndSampler[] boundTextures;

		private ResourceSet[] uniformSet;
		private ResourceLayout[] uniformLayout;
		private bool defaultUniformsDirty;
		private bool definedUniformsDirty;

		//Does this shader store the uniform resolution?
		public readonly bool StoresResolution;

		protected readonly ResourceFactory factory;
		protected readonly GraphicsDevice device;

		protected VertexLayoutDescription vertexDefinition;

		public ShaderBase(GraphicsDevice device, ResourceFactory factory, int numTextures, bool storesResolution = false)
		{
			this.device = device;
			this.factory = factory;
			this.StoresResolution = storesResolution;

			shaders = LoadShaders();

			textureSlots = new TextureAndSamplerSlot[numTextures];

			for (int i = 0; i < numTextures; i++)
			{
				textureSlots[i] = new TextureAndSamplerSlot(i, "Texture" + (i + 1));
			}

			boundTextures = new TextureAndSampler[numTextures];

			vertexDefinition = GetVertexLayout();
		}

		public ShaderBase(GraphicsDevice device, ResourceFactory factory, TextureAndSamplerSlot[] slots)
		{
			this.device = device;
			this.factory = factory;

			shaders = LoadShaders();

			textureSlots = slots;

			boundTextures = new TextureAndSampler[slots.Length];

			vertexDefinition = GetVertexLayout();
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
			if (transformBuffer == null)
				CreateDefaultBuffer();
			commandList.UpdateBuffer(transformBuffer, 0, viewProj);

			defaultUniformsDirty = true;
		}

		public void SetResolution(CommandList commandList, Vector2 resolution)
		{
			if (!StoresResolution)
				throw new Exception("Cannot set resolution as the shader does not support it!");

			if (transformBuffer == null)
				CreateDefaultBuffer();

			commandList.UpdateBuffer(transformBuffer, 64, resolution);
		}

		private void CreateDefaultBuffer()
		{
			DeviceBufferBuilder builder = new DeviceBufferBuilder(factory).Mat4x4();
			if (StoresResolution)
				builder.Vector2();

			transformBuffer = builder.Build();
		}

		public virtual void SetTexture(int index, TextureAndSampler texture)
		{
			if (boundTextures.Length < index)
				throw new Exception();

			boundTextures[index] = texture;

			defaultUniformsDirty = true;
		}

		protected virtual VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionTextureColor.GetLayout();
		}

		public Shader[] GetShaders()
		{
			if (shaders == null)
				shaders = LoadShaders();

			return shaders;
		}

		public ResourceLayout[] GetUniformLayout()
		{
			if (uniformLayout == null)
			{
				uniformLayout = new ResourceLayout[2];
				uniformLayout[0] = CreateDefaultLayout();
				uniformLayout[1] = CreateUserDefinedResourceLayout();
			}

			return uniformLayout;
		}

		public ResourceLayout GetDefaultLayout()
		{
			return GetUniformLayout()[0];
		}

		public ResourceLayout GetUserDefinedLayout()
		{
			return GetUniformLayout()[1];
		}

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

		public void Bind(CommandList commandList)
		{
			var sets = GetResourceSet();

			commandList.SetGraphicsResourceSet(0, sets[0]);
			if (sets[1] != null)
				commandList.SetGraphicsResourceSet(1, sets[1]);
		}

		private ResourceSet[] GetResourceSet()
		{
			if (uniformSet == null)
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
					uniformSet[0] = factory.CreateResourceSet(new ResourceSetDescription(GetUniformLayout()[0], resources.ToArray()));

				defaultUniformsDirty = false;
			}

			if (definedUniformsDirty)
			{
				uniformSet[1] = CreateUserDefinedResourceSet();
				definedUniformsDirty = false;
			}

			return uniformSet;
		}

		protected abstract ResourceSet CreateUserDefinedResourceSet();

		protected void MarkDefaultUniformsDirty(bool immediateCache = false)
		{
			defaultUniformsDirty = true;

			if (immediateCache)
				GetResourceSet();
		}

		protected void MarkUserDefinedUniformsDirty(bool immediateCache = false)
		{
			if (immediateCache)
				GetResourceSet();
		}

		public virtual string[] GetShaderNamesToWatchForChanges()
		{
			return null;
		}
	}
}
