using BRVBase.Services;
using ImGuiNET;
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
		private readonly TextBatch textBatch;
		private readonly PipelineProgram program;
		public bool Enabled;
		public bool ShaderInspectorEnabled;
		public bool FramebufferInspectorEnabled;
		public bool WireframeUsesDepth = true;
		public bool ShowDepthBuffer;

		private string notifyText;
		private float timeLeft;

		public Debug(TextBatch textBatch, PipelineProgram program, bool enabled)
		{
			this.textBatch = textBatch;
			
			this.program = program;
			this.Enabled = enabled;
		}

		public void DebugNotifyText(string text, float time)
		{
			this.notifyText = text;
			this.timeLeft = time;
		}

		public void Update(DeltaTime dt)
		{
			if (Input.IsKeyJustPressed(Key.F1))
				Enabled = !Enabled;

			timeLeft -= (float)dt.Delta;

			if (Enabled)
			{
			}
		}

		public void Draw(DeltaTime dt, int frames, double lastUpdateDelta)
		{
			if (Enabled)
			{
				if (ImGui.Begin("Debug"))
				{
					ImGui.Text("FPS:" + string.Format("{0:0.000}", (float)Runner.FramesPerSecond));
					ImGui.Text("DT (Draw):" + string.Format("{0:0.000}", dt.Delta));
					ImGui.Text("DT (Update):" + string.Format("{0:0.000}", lastUpdateDelta));
					ImGui.End();
				}

				if (PipelineProgram.CurrentShaderErrors.Count > 0)
				{
					if (ImGui.Begin("Shader errors")) {
						foreach ((string key, string value) in PipelineProgram.CurrentShaderErrors)
						{
							if (ImGui.CollapsingHeader(key))
							{
								ImGui.Text(value);
								ImGui.Separator();
							}
						}
						ImGui.End();
					}
				}

				/*textBatch.Begin(Matrix4x4.CreateOrthographicOffCenter(0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT, 0, 0, 100), program);
				var font = ServiceManager.Instance.GetService<AssetManager>().FontLoader.GetHandle("font_debug").Get().GetFont(18);
				Vector2 offset = Vector2.Zero;//new Vector2(8, Constants.INTERNAL_HEIGHT - (font.FontSize * 3));
				textBatch.DrawString(font, "DT (Draw):" + string.Format("{0:0.000}", dt.Delta), offset, RgbaFloat.White);
				textBatch.DrawString(font, "DT (Update):" + string.Format("{0:0.000}", lastUpdateDelta), offset + new Vector2(0, font.FontSize), RgbaFloat.White);

				if (dt.Now > 0)
				{
					textBatch.DrawString(font, "FPS:" + string.Format("{0:0.000}", (float)Runner.FramesPerSecond), offset + new Vector2(0, font.FontSize * 2), RgbaFloat.White);
				}

				if (timeLeft > 0)
				{
					float width = font.MeasureString(notifyText).X;

					textBatch.DrawString(font, notifyText, new Vector2((width / 2) + (Constants.WINDOW_WIDTH / 2), 0), RgbaFloat.White);
				}
				textBatch.End();*/
			}
		}
	}
}
