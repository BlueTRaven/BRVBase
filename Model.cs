using BRVBase;
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
	public class Model : IAsset
	{
		public struct ModelGroup
		{
			public PrimitiveTopology Topology;
			public uint Num; //number of indices
			public uint Offset;
		}

		private readonly string name;

		private DeviceBuffer vertexBuffer;
		private DeviceBuffer indexBuffer;
		public readonly uint NumVertices;
		public readonly uint NumIndices;
		private readonly VertexLayoutDescription description;

		private List<ModelGroup> groups;

		public readonly BoundingBox BoundingBox;

		private Model(string name, DeviceBuffer vertexBuffer, uint numVertices, VertexLayoutDescription description, DeviceBuffer indexBuffer, uint numIndices, BoundingBox boundingBox)
		{
			this.name = name;
			this.vertexBuffer = vertexBuffer;
			this.indexBuffer = indexBuffer;
			this.NumVertices = numVertices;
			this.NumIndices = numIndices;
			BoundingBox = boundingBox;
			this.description = description;
		}

		public void Bind(CommandList commandList)
		{
			commandList.SetVertexBuffer(0, vertexBuffer);

			if (indexBuffer != null && NumIndices > 0)
				commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
		}

		public string GetName()
		{
			return name;
		}

		public static Model Load<T>(string name, ResourceFactory factory, GraphicsDevice device, List<T> vertices, uint vertexSize, VertexLayoutDescription description, 
			List<uint> indices, BoundingBox? boundingBox = null, List<ModelGroup> groups = null, PrimitiveTopology topology = PrimitiveTopology.TriangleList) where T : struct
		{
			DeviceBuffer vertexBuffer = factory.CreateBuffer(new BufferDescription(vertexSize * (uint)vertices.Count, BufferUsage.VertexBuffer));
			device.UpdateBuffer(vertexBuffer, 0, vertices.ToArray());

			DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)indices.Count, BufferUsage.IndexBuffer));
			device.UpdateBuffer(indexBuffer, 0, indices.ToArray());

			Model model = new Model(name, vertexBuffer, (uint)vertices.Count, description, indexBuffer, (uint)indices.Count, boundingBox.GetValueOrDefault(new BoundingBox())); ;

			if (groups == null)
			{
				model.groups = new List<ModelGroup>()
				{
					new()
					{
						Num = model.NumIndices,
						Offset = 0,
						Topology = topology
					}
				};
			}
			else model.groups = groups.ToList();

			return model;
		}
	
		public static Model PrimitiveCube(ResourceFactory factory, GraphicsDevice device, Vector3 position, float halfExtents)
		{
			string name = "primitive_cube_" + Guid.NewGuid().ToString();

			List<DefaultVertexDefinitions.VertexPositionNormalTextureColor> vertices = new List<DefaultVertexDefinitions.VertexPositionNormalTextureColor>();

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, halfExtents),
					new Vector3(0, 0, 1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, halfExtents),
					new Vector3(0, 0, 1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, halfExtents),
					new Vector3(0, 0, 1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, halfExtents),
					new Vector3(0, 0, 1), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexFBL);
				vertices.Add(vertexFTL);
				vertices.Add(vertexFTR);
				vertices.Add(vertexFBR);
			}

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, halfExtents),
					new Vector3(1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, halfExtents),
					new Vector3(1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, -halfExtents),
					new Vector3(1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, -halfExtents),
					new Vector3(1, 0, 0), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexFBR);
				vertices.Add(vertexFTR);
				vertices.Add(vertexBTR);
				vertices.Add(vertexBBR);
			}

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, -halfExtents),
					new Vector3(0, 0, -1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, -halfExtents),
					new Vector3(0, 0, -1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, -halfExtents),
					new Vector3(0, 0, -1), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, -halfExtents),
					new Vector3(0, 0, -1), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexBBR);
				vertices.Add(vertexBTR);
				vertices.Add(vertexBTL);
				vertices.Add(vertexBBL);
			}

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, halfExtents),
					new Vector3(-1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, -halfExtents),
					new Vector3(-1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, -halfExtents),
					new Vector3(-1, 0, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, halfExtents),
					new Vector3(-1, 0, 0), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexFBL);
				vertices.Add(vertexBBL);
				vertices.Add(vertexBTL);
				vertices.Add(vertexFTL);
			}

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, halfExtents),
					new Vector3(0, -1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, halfExtents, -halfExtents),
					new Vector3(0, -1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, -halfExtents),
					new Vector3(0, -1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFTR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, halfExtents, halfExtents),
					new Vector3(0, -1, 0), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexFTL);
				vertices.Add(vertexBTL);
				vertices.Add(vertexBTR);
				vertices.Add(vertexFTR);
			}

			{
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, halfExtents),
					new Vector3(0, 1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexFBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, halfExtents),
					new Vector3(0, 1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBR =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(halfExtents, -halfExtents, -halfExtents),
					new Vector3(0, 1, 0), new Vector2(0, 0), RgbaFloat.White);
				DefaultVertexDefinitions.VertexPositionNormalTextureColor vertexBBL =
					new DefaultVertexDefinitions.VertexPositionNormalTextureColor(position + new Vector3(-halfExtents, -halfExtents, -halfExtents),
					new Vector3(0, 1, 0), new Vector2(0, 0), RgbaFloat.White);

				vertices.Add(vertexFBL);
				vertices.Add(vertexFBR);
				vertices.Add(vertexBBR);
				vertices.Add(vertexBBL);
			}

			uint[] indices = new uint[]
			{
				0, 1, 2,
				2, 3, 0,

				4, 5, 6,
				6, 7, 4,

				8, 9, 10,
				10, 11, 8,

				12, 13, 14,
				14, 15, 12,

				16, 17, 18,
				18, 19, 16,

				20, 21, 22,
				22, 23, 20
			};

			DeviceBuffer buffer = factory.CreateBuffer(new BufferDescription(DefaultVertexDefinitions.VertexPositionNormalTextureColor.SIZE * (uint)vertices.Count, BufferUsage.VertexBuffer));
			device.UpdateBuffer(buffer, 0, vertices.ToArray());

			DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)indices.Length, BufferUsage.IndexBuffer));
			device.UpdateBuffer(indexBuffer, 0, indices);

			return new Model(name, buffer, (uint)vertices.Count, DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout(), indexBuffer, (uint)indices.Length, new BoundingBox());
		}

		public static Model PrimitiveFacingQuad(ResourceFactory factory, GraphicsDevice device, float width, float height)
		{
			DefaultVertexDefinitions.VertexPositionTextureColor[] vertices = new DefaultVertexDefinitions.VertexPositionTextureColor[4]
			{
				new DefaultVertexDefinitions.VertexPositionTextureColor(new Vector2(0, 0), new Vector2(0, 0), RgbaFloat.White),
				new DefaultVertexDefinitions.VertexPositionTextureColor(new Vector2(width, 0), new Vector2(1, 0), RgbaFloat.White),
				new DefaultVertexDefinitions.VertexPositionTextureColor(new Vector2(width, height), new Vector2(1, 1), RgbaFloat.White),
				new DefaultVertexDefinitions.VertexPositionTextureColor(new Vector2(0, height), new Vector2(0, 1), RgbaFloat.White),
			};

			uint[] indices = new uint[6]
			{
				0, 1, 2,
				2, 3, 0
			};

			DeviceBuffer vertexBuffer = factory.CreateBuffer(new BufferDescription(DefaultVertexDefinitions.VertexPositionTextureColor.SIZE * (uint)vertices.Length, BufferUsage.VertexBuffer));
			device.UpdateBuffer(vertexBuffer, 0, vertices);

			DeviceBuffer indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(uint) * (uint)indices.Length, BufferUsage.IndexBuffer));
			device.UpdateBuffer(indexBuffer, 0, indices);

			return new Model("primitive_facing_quad_" + Guid.NewGuid().ToString(), vertexBuffer, (uint)vertices.Length, 
				DefaultVertexDefinitions.VertexPositionTextureColor.GetLayout(), indexBuffer, (uint)indices.Length, new BoundingBox(new Vector3(0, 0, 0), new Vector3(width, height, 0)));
		}

		public static void Draw(Model model, CommandList commandList, Camera camera, PipelineProgram program, TextureAndSampler? texture, Matrix4x4? modelMat = null)
		{
			if (modelMat.HasValue)
				program.GetShader().SetModel(commandList, modelMat.Value);
			if (texture.HasValue)
				program.GetShader().SetTexture(0, texture.Value);

			if (camera != null)
				program.GetShader().SetViewProj(commandList, camera.GetView() * camera.GetProjection());

			if (model.indexBuffer == null)
			{
				program.Bind(commandList);
				program.BindShader(commandList);

				model.Bind(commandList);

				commandList.Draw(model.NumVertices);

				return;
			}

			/*foreach (var group in model.groups)
			{
				program.Bind(commandList, group.Topology == PrimitiveTopology.LineList);
				program.BindShader(commandList);

				model.Bind(commandList);

				int divisor = 3;

				if (group.Topology == PrimitiveTopology.TriangleList || group.Topology == PrimitiveTopology.TriangleStrip)
					divisor = 3;
				else if (group.Topology == PrimitiveTopology.LineList || group.Topology == PrimitiveTopology.LineStrip)
					divisor = 2;
				else if (group.Topology == PrimitiveTopology.PointList)
					divisor = 1;

				commandList.DrawIndexed(group.Num, 1, group.Offset, 0, 0);
			}*/

			program.Bind(commandList);
			program.BindShader(commandList);

			model.Bind(commandList);
			commandList.DrawIndexed((uint)model.NumIndices);
		}

		public static void DrawWireframe(Model model, Model cubeModel, CommandList commandList, Camera camera, PipelineProgram program, TextureAndSampler texture, Matrix4x4? modelMat = null)
		{
			program.Bind(commandList);
			if (modelMat.HasValue)
			{
				float scaleX = model.BoundingBox.PositionB.X - model.BoundingBox.PositionA.X;
				float scaleY = model.BoundingBox.PositionB.Y - model.BoundingBox.PositionA.Y;
				float scaleZ = model.BoundingBox.PositionB.Z - model.BoundingBox.PositionA.Z;

				Vector3 center = model.BoundingBox.GetCenter();
				program.GetShader().SetModel(commandList, Matrix4x4.CreateScale(scaleX / 2f, scaleY / 2f, scaleZ / 2f) * Matrix4x4.CreateTranslation(center) * modelMat.Value);
			}
			program.GetShader().SetTexture(0, texture);
			program.GetShader().SetViewProj(commandList, camera.GetView() * camera.GetProjection());
			program.BindShader(commandList);

			cubeModel.Bind(commandList);

			if (cubeModel.indexBuffer != null && cubeModel.NumIndices > 0)
				commandList.DrawIndexed((uint)cubeModel.NumIndices);
			else commandList.Draw(cubeModel.NumVertices);
		}

		public static void DrawFacingQuad(float width, float height, PipelineProgram program, TextureAndSampler texture)
		{

		}
	}
}
