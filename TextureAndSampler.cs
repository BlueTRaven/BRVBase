using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public readonly struct TextureAndSampler : IAsset
	{
		public readonly Texture texture;
		public readonly Sampler sampler;

		public readonly int width;
		public readonly int height;

		public TextureAndSampler(TextureView texture, Sampler sampler)
		{
			this.texture = texture.Target;
			this.sampler = sampler;

			this.width = (int)texture.Target.Width;
			this.height = (int)texture.Target.Height;
		}

		public TextureAndSampler(Texture texture, Sampler sampler)
		{
			this.texture = texture;
			this.sampler = sampler;

			this.width = (int)texture.Width;
			this.height = (int)texture.Height;
		}

		public string GetName()
		{
			return texture.Name;
		}

		public static bool operator ==(TextureAndSampler first, TextureAndSampler second)
		{
			return first.texture == second.texture && first.sampler == second.sampler;
		}

		public static bool operator !=(TextureAndSampler first, TextureAndSampler second)
		{
			return first.texture != second.texture || first.sampler != second.sampler;
		}

		public override bool Equals(object obj)
		{
			if (obj is TextureAndSampler sampler)
				return sampler == this;
			else return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
