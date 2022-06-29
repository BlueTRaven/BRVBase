using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class SpriteBatch
	{
		public struct SpriteBatchResources
        {
			public ShaderResourceManager ManagerWithViewProj;
			public ShaderResourceManager ManagerWithTextures;
			public IList<ShaderResourceManager> AllManagers;
        }

		public enum TextureType
		{
			OneTexture,
			TextureCollection
		}

		public struct InputInstance
		{
			public Matrix3x2 transform;
			public TextureType textureType;
			public BindableTextureCollection textureCollection;
			public TextureAndSampler texture;
			public Rectangle sourceRect;
			public RgbaFloat color;
		}

		private List<InputInstance> inputInstance = new List<InputInstance>();

		private TextureType currentTextureType;
		private TextureAndSampler currentTexture;
		private BindableTextureCollection currentTextureCollection;
		private int currentWidth;
		private int currentHeight;

		private DeviceBuffer vertexBuffer;
		private DeviceBuffer indexBuffer;
		private readonly ResourceFactory factory;
		private readonly GraphicsDevice device;
		private readonly CommandList commandList;
		private readonly Fence fence;

		private int currentVertexBufferSize;
		private int currentIndexBufferSize;
		private bool firstDraw = true;
		private bool begin;
		private Matrix4x4 viewProj;
		private PipelineProgram program;
		private SpriteBatchResources resources;
		private FastList<DefaultVertexDefinitions.VertexPositionTextureColor> vertices = new FastList<DefaultVertexDefinitions.VertexPositionTextureColor>();
		private FastList<uint> indices = new FastList<uint>();
		private uint lastIndex = 1;

		private static Vector2[] vertexTemplate = new Vector2[4]
		{
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(1, 1)
		};

		private static uint[] indexTemplate = new uint[6]
		{
			0, 1, 2,
			1, 3, 2
		};

		public SpriteBatch(GraphicsDevice device, ResourceFactory factory)
		{
			this.device = device;
			this.factory = factory;

			commandList = factory.CreateCommandList();
			fence = factory.CreateFence(false);
		}

		public void Begin(Matrix4x4 viewProj, PipelineProgram program, SpriteBatchResources resources, RgbaFloat? clearColor = null)
		{
			if (begin)
			{
				throw new Exception();
			}

			begin = true;
			lastIndex = 0;

			this.program = program;
			this.resources = resources;

			commandList.Begin();
			program.Bind(commandList);

			if (clearColor.HasValue)
			{
				commandList.ClearColorTarget(0, clearColor.GetValueOrDefault());
			}

			this.viewProj = viewProj;
			resources.ManagerWithViewProj.Set("ViewProj", viewProj, ShaderStages.Vertex, commandList);
			//program.GetShader().SetViewProj(commandList, viewProj);
		}

		public void Draw(Matrix3x2 transform, TextureAndSampler texture, RgbaFloat color)
		{
			Draw(transform, texture, new Rectangle(0, 0, texture.width, texture.height), color);
		}

		public void Draw(Matrix3x2 transform, TextureAndSampler texture, Rectangle sourceRect, RgbaFloat color)
		{
			inputInstance.Add(new InputInstance()
			{
				textureType = TextureType.OneTexture,
				transform = transform,
				texture = texture,
				sourceRect = sourceRect,
				color = color
			});
		}

		public void Draw(Matrix3x2 transform, BindableTextureCollection textureCollection, Rectangle sourceRect, RgbaFloat color)
		{
			inputInstance.Add(new InputInstance()
			{
				textureType = TextureType.TextureCollection,
				transform = transform,
				textureCollection = textureCollection,
				sourceRect = sourceRect,
				color = color
			});
		}

		private void AddVertex(int index, Vector2 uv, Vector2 size, in Matrix3x2 transform, RgbaFloat color, float depth = 0)
		{
			vertices.Add(new DefaultVertexDefinitions.VertexPositionTextureColor(
				Vector2.Transform(vertexTemplate[index] * size, transform),
				uv, color));
		}

		private void AddIndices()
		{
			indices.Add(lastIndex + indexTemplate[0]);
			indices.Add(lastIndex + indexTemplate[1]);
			indices.Add(lastIndex + indexTemplate[2]);
			indices.Add(lastIndex + indexTemplate[3]);
			indices.Add(lastIndex + indexTemplate[4]);
			indices.Add(lastIndex + indexTemplate[5]);

			lastIndex += 4;
		}

		public CommandList End(bool submitImmediately = true)
		{
			if (!begin)
			{
				//ERROR
				return null;
			}

			//It's valid to call begin then end immediately, without drawing anything.
			//Don't flush if that's the case.
			if (inputInstance.Count > 0)
			{
				ReallyDraw();

				if (submitImmediately)
				{
					commandList.End();
					device.SubmitCommands(commandList, fence);
				}
			}

			begin = false;

			return commandList;
		}

		public CommandList GetCommandList()
		{
			if (begin)
			{
				//ERROR
				Console.WriteLine("ERROR: Cannot call GetCommandList while begin is true. Call .End first.");

				return null;
			}
			return commandList;
		}

		private void ReallyDraw()
		{
			for (int i = 0; i < inputInstance.Count; i++)
			{
				InputInstance instance = inputInstance[i];

				//prevent the first flush since it will probably be empty.
				if (!firstDraw)
				{
					if (this.currentTextureType != instance.textureType)
						Flush();
					else if (currentTextureType == TextureType.OneTexture && this.currentTexture != instance.texture)
						Flush();
					else if (currentTextureType == TextureType.TextureCollection && this.currentTextureCollection != instance.textureCollection)
						Flush();
				}

				firstDraw = false;

				this.currentTextureType = instance.textureType;
				this.currentTexture = instance.texture;
				this.currentTextureCollection = instance.textureCollection;

				float width = 0;
				float height = 0;

				if (currentTextureType == TextureType.OneTexture)
				{
					width = currentTexture.width;
					height = currentTexture.height;
				}
				if (currentTextureType == TextureType.TextureCollection)
				{
					//TODO: always choosing the first might be a bad idea?
					width = currentTextureCollection.GetFirst().width;
					height = currentTextureCollection.GetFirst().height;
				}

				float minx = (float)instance.sourceRect.X / width;
				float maxx = minx + (float)instance.sourceRect.Width / width;
				float miny = (float)instance.sourceRect.Y / height;
				float maxy = miny + (float)instance.sourceRect.Height / height;

				Vector2 size = new Vector2(MathF.Abs(instance.sourceRect.Width), MathF.Abs(instance.sourceRect.Height));

				AddVertex(0, new Vector2(minx, miny), size, instance.transform, instance.color);
				AddVertex(1, new Vector2(maxx, miny), size, instance.transform, instance.color);
				AddVertex(2, new Vector2(minx, maxy), size, instance.transform, instance.color);
				AddVertex(3, new Vector2(maxx, maxy), size, instance.transform, instance.color);
				AddIndices();
			}

			Flush();

			inputInstance.Clear();
		}

		private unsafe void Flush()
		{
			if (vertices.Length <= 0 || indices.Length <= 0)
				return;

			if (currentTextureType == TextureType.OneTexture)
				resources.ManagerWithTextures.Set("Texture1", currentTexture, ShaderStages.Fragment, commandList);
			//program.GetShader().SetTexture(0, currentTexture);
			else if (currentTextureType == TextureType.TextureCollection)
				currentTextureCollection.SetTextures(resources.ManagerWithTextures, commandList);
			//currentTextureCollection.SetTextures(program.GetShader());

			program.GetShader().Bind(commandList, resources.AllManagers);
			//program.BindShader(commandList);

			if (vertexBuffer == null || indexBuffer == null || vertices.Buffer.Length > currentVertexBufferSize || indices.Buffer.Length > currentIndexBufferSize)
			{
				if (vertices.Buffer.Length > currentVertexBufferSize)
				{
					currentVertexBufferSize = vertices.Buffer.Length;
				}

				if (indices.Buffer.Length > currentIndexBufferSize)
				{
					currentIndexBufferSize = indices.Buffer.Length;
				}

				vertexBuffer?.Dispose();
				indexBuffer?.Dispose();
				vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(DefaultVertexDefinitions.VertexPositionTextureColor.SIZE * currentVertexBufferSize), BufferUsage.VertexBuffer));
				indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * currentIndexBufferSize), BufferUsage.IndexBuffer));
			}

			commandList.UpdateBuffer(vertexBuffer, 0, vertices.Buffer);
			commandList.UpdateBuffer(indexBuffer, 0, indices.Buffer);

			commandList.SetVertexBuffer(0, vertexBuffer);
			commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);

			commandList.DrawIndexed((uint)indices.Length);

			vertices.Clear();
			indices.Clear();

			lastIndex = 0;

			firstDraw = true;
			this.currentTextureType = TextureType.OneTexture;
			this.currentTexture = default;
			this.currentTextureCollection = null;
		}
	}
}
