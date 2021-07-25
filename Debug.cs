using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class Debug
	{
		public bool Enabled;

		private string notifyText;
		private float timeLeft;

		public Debug(bool enabled)
		{
			this.Enabled = enabled;
		}

		public void DebugNotifyText(string text, float time)
		{
			this.notifyText = text;
			this.timeLeft = time;
		}

		public void Update(DeltaTime dt)
		{
			if (Main.Input.IsKeyJustPressed(Key.F1))
				Enabled = !Enabled;

			timeLeft -= (float)dt.Delta;
		}

		public void Draw(DeltaTime dt, int frames, double lastUpdateDelta)
		{
			if (Enabled)
			{
				Main.TextBatch.Begin(Matrix4x4.CreateOrthographicOffCenter(0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT, 0, 0, 100), Main.BackbufferProgram);
				var font = Main.AssetManager.FontLoader.GetHandle("font_debug").Get().GetFont(18);
				Vector2 offset = Vector2.Zero;//new Vector2(8, Constants.INTERNAL_HEIGHT - (font.FontSize * 3));
				Main.TextBatch.DrawString(font, "DT (Draw):" + string.Format("{0:0.000}", dt.Delta), offset, RgbaFloat.White);
				Main.TextBatch.DrawString(font, "DT (Update):" + string.Format("{0:0.000}", lastUpdateDelta), offset + new Vector2(0, font.FontSize), RgbaFloat.White);

				if (dt.Now > 0)
				{
					Main.TextBatch.DrawString(font, "FPS:" + string.Format("{0:0.000}", (float)Runner.FramesPerSecond), offset + new Vector2(0, font.FontSize * 2), RgbaFloat.White);
				}

				if (timeLeft > 0)
				{
					float width = font.MeasureString(notifyText).X;

					Main.TextBatch.DrawString(font, notifyText, new Vector2((width / 2) + (Constants.WINDOW_WIDTH / 2), 0), RgbaFloat.White);
				}
				Main.TextBatch.End();
			}
		}
	}
}
