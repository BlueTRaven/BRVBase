using BRVBase.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace BRVBase
{
	public class Main : Game
	{
		public static Input Input;

		public static AssetManager AssetManager;

		public static SpriteBatch SpriteBatch;
		public static TextBatch TextBatch;

		public static Camera Camera;
		public static World World;
		public static Random Rand = new Random();

		public static ShaderError ErrorShader;
		private ShaderGeneric shader;
		private ShaderSolidColor shaderSolidColor;
		public static PipelineProgram BackbufferProgram;
		public static PipelineProgram IndirectProgram;
		public static PipelineProgram TextProgram;
		public static PipelineProgram SolidColorProgram;

		private Texture indirectBufferTex;
		private Framebuffer indirectBuffer;

		public static Debug Debug = new Debug(true);

		private double lastUpdateDelta;
		private int frames;
		private bool paused;

		public Main() : base()
		{
		}

		public override void LoadContent(GraphicsDevice device, ResourceFactory factory)
		{
			base.LoadContent(device, factory);

			AssetManager = new AssetManager(device, factory);

			indirectBufferTex = factory.CreateTexture(TextureDescription.Texture2D(Constants.INTERNAL_WIDTH, Constants.INTERNAL_HEIGHT, 1, 1,
				PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
			indirectBuffer = factory.CreateFramebuffer(new FramebufferDescription(null, indirectBufferTex));
			indirectBufferTex.Name = "Indirect FrameBuffer Tex";
			indirectBuffer.Name = "Indirect FrameBuffer";

			ErrorShader = new ShaderError(device, factory);

			shader = new ShaderGeneric(device, factory);
			shaderSolidColor = new ShaderSolidColor(device, factory);
			BackbufferProgram = new PipelineProgram(device.SwapchainFramebuffer, factory, shader);
			IndirectProgram = new PipelineProgram(indirectBuffer, factory, shader);
			TextProgram = new PipelineProgram(indirectBuffer, factory, new ShaderText(device, factory), BlendStateDescription.SingleAlphaBlend);
			SolidColorProgram = new PipelineProgram(indirectBuffer, factory, shaderSolidColor);
		}

		public override void Initialize(Sdl2Window window)
		{
			base.Initialize(window);

			Input = new Input(window);

			Camera = new Camera();
			World = new World(null, device);

			SpriteBatch = new SpriteBatch(device, factory);
			TextBatch = new TextBatch(device, factory);
		}

		public override void Update(DeltaTime delta)
		{
			base.Update(delta);

			if (Input.IsKeyDown(Key.Escape))
				Exit = true;

			if (Input.IsKeyJustPressed(Key.P))
				paused = !paused;

			Debug.Update(delta);

			if (!paused || Input.IsKeyJustPressed(Key.O))
				World.Update(delta);

			Input.Update();
			lastUpdateDelta = delta.Delta;
			frames++;
		}

		public override void Draw(DeltaTime delta)
		{
			base.Draw(delta);

			Debug.Draw(delta, frames, lastUpdateDelta);

			BackbufferProgram.Clear(device, new RgbaFloat(0, 0, 0, 0));
			//TextProgram.Clear(device, RgbaFloat.Black);
			IndirectProgram.Clear(device, Util.ColorFromInts(3, 8, 13));

			SpriteBatch.Begin(Camera.GetProjection() * Camera.GetMatrix(), IndirectProgram);
			TextBatch.Begin(Matrix4x4.CreateOrthographicOffCenter(0, Constants.INTERNAL_WIDTH, Constants.INTERNAL_HEIGHT, 0, 0, 100), TextProgram);
			World.Draw(delta);
			TextBatch.End();
			SpriteBatch.End();

			SpriteBatch.Begin(Matrix4x4.CreateOrthographicOffCenter(0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT, 0, 0, 100), BackbufferProgram);

			SpriteBatch.Draw(Matrix3x2.CreateScale(new Vector2(Constants.WINDOW_WIDTH / Constants.INTERNAL_WIDTH, Constants.WINDOW_HEIGHT / Constants.INTERNAL_HEIGHT)),
				new TextureAndSampler(indirectBufferTex, device.PointSampler), RgbaFloat.White);

			SpriteBatch.End();
			
			if (Debug.Enabled)
			{
				TextBatch.Begin(Matrix4x4.CreateOrthographicOffCenter(0, Constants.WINDOW_WIDTH, Constants.WINDOW_HEIGHT, 0, 0, 100), BackbufferProgram);
				var font = AssetManager.FontLoader.GetHandle("font_debug").Get().GetFont(18);
				Vector2 offset = Vector2.Zero;//new Vector2(8, Constants.INTERNAL_HEIGHT - (font.FontSize * 3));
				TextBatch.DrawString(font, "DT (Draw):" + string.Format("{0:0.000}", delta.Delta), offset, RgbaFloat.White);
				TextBatch.DrawString(font, "DT (Update):" + string.Format("{0:0.000}", lastUpdateDelta), offset + new Vector2(0, font.FontSize), RgbaFloat.White);

				if (delta.Now > 0)
				{
					TextBatch.DrawString(font, "FPS:" + string.Format("{0:0.000}", (float)frames / delta.Now), offset + new Vector2(0, font.FontSize * 2), RgbaFloat.White);
				}
				TextBatch.End();
			}
		}
	}
}
