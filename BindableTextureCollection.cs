using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class BindableTextureCollection
	{
		private List<TextureAndSampler> textures = new List<TextureAndSampler>();
		private List<AssetHandle<TextureAndSampler>> textureHandles = new List<AssetHandle<TextureAndSampler>>();

		private int count;
		private TextureAndSampler first;
		private AssetHandle<TextureAndSampler> firstHandle;

		public int Count => count;

		public void Add(TextureAndSampler texture)
		{
			if (count == 0)
				first = texture;
			textures.Add(texture);
			count++;
		}

		public void Add(AssetHandle<TextureAndSampler> textureHandle)
		{
			//This could be a problem with hot reloading?
			if (count == 0)
				firstHandle = textureHandle;

			textureHandles.Add(textureHandle);
			count++;
		}

		public ref TextureAndSampler GetFirst()
		{
			if (count == 0)
				throw new Exception("Collection is empty - cannot get first element.");

			if (first.texture != null)
				return ref first;
			else return ref firstHandle.Get();
		}

		public void SetTextures(ShaderBase shader)
		{
			int lastTex = 0;
			for (int i = 0; i < textures.Count; i++)
			{
				shader.SetTexture(i, textures[i]);
				lastTex++;
			}

			//remaining in count should be lastTex to count
			for (int i = lastTex; i < count; i++)
			{
				int index = i - lastTex;

				shader.SetTexture(i, textureHandles[index].Get());
			}
		}
	}
}
