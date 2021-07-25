using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class FontTextureRenderer : IFontStashRenderer
	{
		private readonly SpriteBatch batch;
		private readonly GraphicsDevice device;
		private readonly ResourceFactory factory;

		public ITexture2DManager TextureManager { get; private set; }

		public FontTextureRenderer(SpriteBatch batch, GraphicsDevice device, ResourceFactory factory)
		{
			this.batch = batch;
			this.device = device;
			this.factory = factory;

			TextureManager = new FontTextureManager(device, factory);
		}

		public void Draw(object texture, Vector2 pos, System.Drawing.Rectangle? src, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
		{
			TextureAndSampler tex = new TextureAndSampler((Texture)texture, device.PointSampler);

			Matrix3x2 mat = Matrix3x2.CreateTranslation(-origin) *
				Matrix3x2.CreateRotation(rotation) *
				Matrix3x2.CreateScale(scale) *
				Matrix3x2.CreateTranslation(pos);

			RgbaFloat colf = new RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
			System.Drawing.Rectangle rect1 = src.GetValueOrDefault(System.Drawing.Rectangle.Empty);
			Veldrid.Rectangle rect2 = new Veldrid.Rectangle(rect1.X, rect1.Y, rect1.Width, rect1.Height);
			batch.Draw(mat, tex, rect2, colf);
		}
	}
}
