using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FontStashSharp;
using FontStashSharp.Interfaces;
using Veldrid;
using Veldrid.Sdl2;

namespace BRVBase
{
	public class FontTextureManager : ITexture2DManager
	{
		private readonly GraphicsDevice device;
		private readonly ResourceFactory factory;

		public FontTextureManager(GraphicsDevice device, ResourceFactory factory)
		{
			this.device = device;
			this.factory = factory;
		}

		public object CreateTexture(int width, int height)
		{
			Texture tex = factory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
			tex.Name = "Font Texture " + Guid.NewGuid().ToString();
			return tex;
		}

		public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
		{
			Texture castTexture = (Texture)texture;

			device.UpdateTexture(castTexture, data, (uint)bounds.X, (uint)bounds.Y, 0, (uint)bounds.Width, (uint)bounds.Height, 1, 0, 0);
		}
	}
}
