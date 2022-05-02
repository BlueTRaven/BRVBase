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
	public static class DefaultVertexDefinitions
	{
		public interface IVertexLayout
		{
			VertexLayoutDescription GetLayout();
		}

		public readonly struct VertexPosition
		{
			public readonly Vector2 position;

			public VertexPosition(Vector2 position)
			{
				this.position = position;
			}

			public const uint SIZE = 16;

			public static VertexLayoutDescription GetLayout()
			{
				return new VertexDefinitionBuilder(true).Float2("Position").Build();
			}
		}

		public readonly struct VertexPositionColor
		{
			public readonly Vector2 position;
			public readonly RgbaFloat color;

			public VertexPositionColor(Vector2 position, RgbaFloat color)
			{
				this.position = position;
				this.color = color;
			}

			public const uint SIZE = 24;

			public static VertexLayoutDescription GetLayout()
			{
				return new VertexDefinitionBuilder(true).Float2("Position").Float4("Color").Build();
			}
		}

		public readonly struct VertexPositionTextureColor
		{
			public readonly Vector2 position;
			public readonly Vector2 textureCoord;
			public readonly RgbaFloat color;

			public VertexPositionTextureColor(Vector2 position, Vector2 textureCoord, RgbaFloat color)
			{
				this.position = position;
				this.textureCoord = textureCoord;
				this.color = color;
			}

			public const uint SIZE = 36;

			public static VertexLayoutDescription GetLayout()
			{
				return new VertexDefinitionBuilder(true).Float2("Position").Float2("TexCoord").Float4("Color").Build();
			}
		}

		public readonly struct VertexPositionNormalTextureColor
		{
			public readonly Vector3 position;
			public readonly Vector3 normal;
			public readonly Vector2 textureCoord;
			public readonly RgbaFloat color;

			public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 textureCoord, RgbaFloat color)
			{
				this.position = position;
				this.normal = normal;
				this.textureCoord = textureCoord;
				this.color = color;
			}

			public const uint SIZE = 12 + 12 + 8 + 16;

			public static VertexLayoutDescription GetLayout()
			{
				return new VertexDefinitionBuilder(true).Float3("Position").Float3("Normal").Float2("TexCoord").Float4("Color").Build();
			}
		}
	}
}
