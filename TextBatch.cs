using FontStashSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class TextBatch
	{
		private FontTextureRenderer renderer;
		private SpriteBatch spriteBatch;

		private bool begin;

		public TextBatch(GraphicsDevice device, ResourceFactory factory)
		{
			spriteBatch = new SpriteBatch(device, factory);
			renderer = new FontTextureRenderer(spriteBatch, device, factory);
		}

		public void Begin(Matrix4x4 viewProj, PipelineProgram program, RgbaFloat? clearColor = null)
		{
			if (begin)
			{
				throw new Exception();
			}

			begin = true;

			spriteBatch.Begin(viewProj, program, clearColor);
		}

		public void DrawString(DynamicSpriteFont font, string str, Vector2 position, RgbaFloat color)
		{
			System.Drawing.Color col = System.Drawing.Color.FromArgb((int)(color.A * 255), (int)(color.R * 255), (int)(color.G * 255), (int)(color.B * 255));
			font.DrawText(renderer, str, position, col);
		}

		public void End()
		{
			if (!begin)
			{
				//ERROR
				return;
			}

			spriteBatch.End();

			begin = false;
		}
	}
}
