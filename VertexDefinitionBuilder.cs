using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public struct VertexDefinitionBuilder
	{
		private List<VertexElementDescription> elements;

		public VertexDefinitionBuilder(bool a)
		{
			elements = new List<VertexElementDescription>();
		}

		public VertexDefinitionBuilder Float2(string name)
		{
			elements.Add(new VertexElementDescription(name, VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate));

			return this;
		}

		public VertexDefinitionBuilder Float3(string name)
		{
			elements.Add(new VertexElementDescription(name, VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate));

			return this;
		}

		public VertexDefinitionBuilder Float4(string name)
		{
			elements.Add(new VertexElementDescription(name, VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate));

			return this;
		}

		public VertexLayoutDescription Build()
		{
			return new VertexLayoutDescription(elements.ToArray());
		}
	}
}
