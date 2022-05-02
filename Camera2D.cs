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
	public class Camera2D : Camera
	{
		public Vector2 offset;
		public float scale = 1;

		public Camera2D(int width, int height, Point viewport) : base(width, height, 0, 10000, viewport)
		{
			offset = new Vector2(width / 2f, height / 2f);
		}

		public override void Update(DeltaTime delta, bool canControlSelf)
		{
			float speed = 1;

			if (!ImGui.GetIO().WantCaptureKeyboard)
			{
				if (Input.IsKeyDown(Key.ShiftLeft))
					speed = 4;

				if (Input.IsKeyDown(Key.W))
					Position.Y -= speed;
				if (Input.IsKeyDown(Key.S))
					Position.Y += speed;
				if (Input.IsKeyDown(Key.A))
					Position.X -= speed;
				if (Input.IsKeyDown(Key.D))
					Position.X += speed;

				if (Input.IsKeyJustPressed(Key.Plus))
					scale = scale * 2f;
				if (Input.IsKeyJustPressed(Key.Minus))
					scale = scale / 2f;

				scale = Math.Clamp(scale, 0.125f, 64);
			}
		}

		public Rectangle GetBounds()
		{
			return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
		}

		public override Matrix4x4 GetView()
		{
			return Matrix4x4.CreateTranslation(-new Vector3(offset, 0)) * Matrix4x4.CreateScale(scale) * 
				Matrix4x4.CreateTranslation(new Vector3(offset, 0)) * Matrix4x4.CreateTranslation(-Position);
		}

		public override Matrix4x4 GetProjection()
		{
			return Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, Near, Far);
		}

		public override Vector3 GetLookDir()
		{
			return new Vector3(0, 0, 1);
		}

        public override void SetLookDir(Vector3 dir)
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetMouseProjection()
		{
			throw new NotImplementedException();
		}

        public override void UpdateViewport(int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
