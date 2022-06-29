using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class AssetLoaderModelObj : AssetLoader<Model>
	{
		private struct ModelParseVertex
		{
			public int V;
			public int Vt;
			public int Vn;
		}

		private enum FaceType
		{
			Triangle,
			Multiple,
			Line
		}

		private struct ModelParseFace
		{
			public List<ModelParseVertex> Vertices;
			public List<uint> Indices;
			public FaceType Type;
		}

		private readonly ResourceFactory factory;
		private readonly GraphicsDevice device;

		public AssetLoaderModelObj(GraphicsDevice device, ResourceFactory factory) : base(new string[] { "./Assets/Models/" }, ".obj")
		{
			this.factory = factory;
			this.device = device;
		}

		protected override Model Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				List<Vector3> positions = new List<Vector3>();
				List<Vector2> texCoords = new List<Vector2>();
				List<Vector3> normals = new List<Vector3>();
				List<ModelParseFace> faces = new List<ModelParseFace>();

				string raw = File.ReadAllText(file.FullPath);

				var tokens = Parser.GetTokens(raw);

				int currentIndex = 0;
				Parser.Token currentToken = tokens[0];

				while (currentToken.type != Parser.Token.TokenType.EndOfFile)
				{
					if (currentToken.type == Parser.Token.TokenType.String)
					{
						if (currentToken.contents == "v")
						{
							//add vertex
							string xs = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string ys = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string zs = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							bool bx = float.TryParse(xs, out float x);
							bool by = float.TryParse(ys, out float y);
							bool bz = float.TryParse(zs, out float z);

							if (bx && by && bz)
								positions.Add(new Vector3(x, y, z));

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "vt")
						{
							//add texcoords
							string us = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string vs = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							bool bu = float.TryParse(us, out float u);
							bool bv = float.TryParse(vs, out float v);

							if (bu && bv)
								texCoords.Add(new Vector2(u, v));

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "vn")
						{
							//add vertex normals
							string xs = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string ys = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string zs = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							bool bx = float.TryParse(xs, out float x);
							bool by = float.TryParse(ys, out float y);
							bool bz = float.TryParse(zs, out float z);

							if (bx && by && bz)
								normals.Add(new Vector3(x, y, z));

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
						else if (currentToken.contents == "f")
						{
							ModelParseFace face = new ModelParseFace();
							face.Vertices = new List<ModelParseVertex>();
							face.Indices = new List<uint>();

							//we can expect a minimum of 3 vertices in a face.
							string a = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string b = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string c = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							//after that, we can have an arbitrarily large number of vertices in a face.

							face.Vertices.Add(ParseModelIndex(a));
							face.Vertices.Add(ParseModelIndex(b));
							face.Vertices.Add(ParseModelIndex(c));
							face.Indices.Add(0);
							face.Indices.Add(2);
							face.Indices.Add(1);
							face.Type = FaceType.Triangle;

							bool eol = false;
							bool eof = false;

							uint index = 3;

							string v = Parser.MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol, out eof);

							if (!eol)
								face.Type = FaceType.Multiple;

							while (!eol && !eof)
							{
								var vertex = ParseModelIndex(v);
								face.Vertices.Add(vertex);
								face.Indices.Add(index);
								face.Indices.Add(index - 1);
								face.Indices.Add(0);

								index++;

								v = Parser.MaybeExpectString(ref currentIndex, ref currentToken, tokens, out eol, out eof);
							}

							faces.Add(face);

							if (eof)
								break;
						}
						else if (currentToken.contents == "l")
						{
							ModelParseFace face = new ModelParseFace();
							face.Vertices = new List<ModelParseVertex>();
							face.Indices = new List<uint>();

							string a = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);
							string b = Parser.ExpectString(ref currentIndex, ref currentToken, tokens);

							face.Vertices.Add(ParseModelIndex(a));
							face.Vertices.Add(ParseModelIndex(b));
							face.Indices.Add(0);
							face.Indices.Add(1);
							face.Type = FaceType.Line;

							faces.Add(face);

							Parser.ExpectEndOfLine(ref currentIndex, ref currentToken, tokens);
						}
					}

					Parser.NextToken(ref currentIndex, ref currentToken, tokens);
				}

				List<DefaultVertexDefinitions.VertexPositionNormalTextureColor> vertices = new List<DefaultVertexDefinitions.VertexPositionNormalTextureColor>();
				List<uint> indices = new List<uint>();
				List<Model.ModelGroup> groups = null;
				//0, 16: stay on mode 0 for 16 vertices

				uint vertexOffset = 0;
				uint indexOffset = 0;
				uint num = 0;

				FaceType currentFaceType = faces[0].Type;
				PrimitiveTopology topology = PrimitiveTopology.TriangleList;

				foreach (ModelParseFace face in faces)
				{
					/*if (face.Type == FaceType.Line && currentFaceType != FaceType.Line || ((face.Type == FaceType.Triangle || face.Type == FaceType.Multiple) && currentFaceType == FaceType.Line))
					{
						if (groups == null)
							groups = new List<Model.ModelGroup>();

						groups.Add(new Model.ModelGroup()
						{
							Offset = indexOffset,
							Num = num,
							Topology = (face.Type == FaceType.Triangle || face.Type == FaceType.Multiple) ? PrimitiveTopology.TriangleList : PrimitiveTopology.LineList
						});

						currentFaceType = face.Type;
						num = 0;
					}*/

					if (face.Type == FaceType.Triangle)
					{
						AddIndex(vertices, positions, texCoords, normals, face.Vertices[0]);
						AddIndex(vertices, positions, texCoords, normals, face.Vertices[1]);
						AddIndex(vertices, positions, texCoords, normals, face.Vertices[2]);

						indices.Add(vertexOffset + face.Indices[0]);
						indices.Add(vertexOffset + face.Indices[1]);
						indices.Add(vertexOffset + face.Indices[2]);
					}

					if (face.Type == FaceType.Multiple)
					{
						for (int i = 0; i < face.Vertices.Count; i++)
						{
							AddIndex(vertices, positions, texCoords, normals, face.Vertices[i]);
						}

						for (int i = 0; i < face.Indices.Count; i++)
						{
							indices.Add(vertexOffset + face.Indices[i]);
						}
					}

					if (face.Type == FaceType.Line)
					{
						topology = PrimitiveTopology.LineList;

						AddIndex(vertices, positions, texCoords, normals, face.Vertices[0]);
						AddIndex(vertices, positions, texCoords, normals, face.Vertices[1]);

						indices.Add(vertexOffset + face.Indices[0]);
						indices.Add(vertexOffset + face.Indices[1]);
					}

					indexOffset += (uint)face.Indices.Count;
					vertexOffset += (uint)face.Vertices.Count;
					num += (uint)face.Indices.Count;
				}

				float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity;
				float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;

				foreach (var vertex in vertices)
				{
					if (vertex.position.X < minX)
						minX = vertex.position.X;
					if (vertex.position.Y < minY)
						minY = vertex.position.Y;
					if (vertex.position.Z < minZ)
						minZ = vertex.position.Z;

					if (vertex.position.X > maxX)
						maxX = vertex.position.X;
					if (vertex.position.Y > maxY)
						maxY = vertex.position.Y;
					if (vertex.position.Z > maxZ)
						maxZ = vertex.position.Z;
				}

				Model model = Model.Load(file.Name, factory, device, vertices, DefaultVertexDefinitions.VertexPositionNormalTextureColor.SIZE, 
					DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout(), indices, new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ)), topology: topology);

				return model;
			}

			return null;
		}

		private void AddIndex(List<DefaultVertexDefinitions.VertexPositionNormalTextureColor> vertices, List<Vector3> positions, List<Vector2> texCoords, List<Vector3> normals, ModelParseVertex i)
		{
			Vector3 aPos = positions[i.V - 1];
			Vector2 aTc = Vector2.Zero;
			Vector3 aNrm = Vector3.Zero;
			if (i.Vt > 0)
				aTc = texCoords[i.Vt - 1];
			if (i.Vn > 0)
				aNrm = normals[i.Vn - 1];

			vertices.Add(new DefaultVertexDefinitions.VertexPositionNormalTextureColor(aPos, aNrm, aTc, RgbaFloat.White));
		}

		private ModelParseVertex ParseModelIndex(string a)
		{
			//SLOW
			int numSlashes = a.Where(x => x == '/').Count();

			if (numSlashes == 2)
			{
				//Vertex, texcoord, and normal
				ModelParseVertex p = new ModelParseVertex();

				string[] split = a.Split('/');
				int.TryParse(split[0], out p.V);
				int.TryParse(split[1], out p.Vt);
				int.TryParse(split[2], out p.Vn);

				//if we have x//x instead of x/x/x, only vertex and normal are defined.
				//output of int.TryParse is 0 when invalid, so just set it to -1 since that's what I'm using for invalid.
				if (p.Vt == 0)
					p.Vt = -1;

				return p;
			}
			else if (numSlashes == 1)
			{
				//Vertex and texcoord
				ModelParseVertex p = new ModelParseVertex();

				string[] split = a.Split('/');
				int.TryParse(split[0], out p.V);
				int.TryParse(split[1], out p.Vt);
				p.Vn = -1;

				return p;
			}
			else if (numSlashes == 0)
			{
				//Only vertex
				ModelParseVertex p = new ModelParseVertex();

				int.TryParse(a, out p.V);
				p.Vt = -1;
				p.Vn = -1;

				return p;
			}
			else return new ModelParseVertex();
		}
	}
}
