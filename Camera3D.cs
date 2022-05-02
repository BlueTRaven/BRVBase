using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace BRVBase
{
	public class Camera3D : Camera
	{
		private readonly float fov;

		private bool mousePressed;
		private Vector2 mousePressedPos;

		public Camera3D(int width, int height, float fov, Point viewport, float near = 0.001f, float far = 1000f) : base(width, height, near, far, viewport)
		{
			this.fov = fov;
		}

		public override void Update(DeltaTime delta, bool canControlSelf)
		{
			float speed = 1;

			float sprintFactor = 2.5f;
			if (Input.IsKeyDown(Key.ControlLeft))
			{
				sprintFactor = 0.5f;
			}
			else
			{
				if (Input.IsKeyDown(Key.ShiftLeft))
					sprintFactor += 7.5f;
				if (Input.IsKeyDown(Key.Space))
					sprintFactor += 25f;
			}

			Vector3 motionDir = Vector3.Zero;

			if (Input.IsKeyDown(Key.A))
			{
				motionDir += -Vector3.UnitX;
			}
			if (Input.IsKeyDown(Key.D))
			{
				motionDir += Vector3.UnitX;
			}
			if (Input.IsKeyDown(Key.W))
			{
				motionDir += -Vector3.UnitZ;
			}
			if (Input.IsKeyDown(Key.S))
			{
				motionDir += Vector3.UnitZ;
			}
			if (Input.IsKeyDown(Key.Q))
			{
				motionDir += -Vector3.UnitY;
			}
			if (Input.IsKeyDown(Key.E))
			{
				motionDir += Vector3.UnitY;
			}

			if (motionDir != Vector3.Zero)
			{
				Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
				motionDir = Vector3.Transform(motionDir, lookRotation);
				Position += motionDir * speed * sprintFactor * (float)delta.Delta;
			}

			if (!ImGui.GetIO().WantCaptureMouse && !Gizmos.Dragging &&
				(Input.IsMouseDown(MouseButton.Left) || Input.IsMouseDown(MouseButton.Right)))
			{
				if (!mousePressed)
				{
					DebugControlled = true;

					mousePressed = true;
					mousePressedPos = Input.MousePosition;

					Sdl2Native.SDL_SetRelativeMouseMode(true);
					Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, true);
				}

				//Console.WriteLine(Input.MouseDelta);
				Vector2 mouseDelta = Input.MouseDelta;
				Yaw -= mouseDelta.X * 0.002f;
				Pitch -= mouseDelta.Y * 0.002f;
			}
			else if (mousePressed && !Gizmos.Dragging)
			{
				Sdl2Native.SDL_SetRelativeMouseMode(false);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);

				Sdl2Native.SDL_WarpMouseInWindow(Runner.Window.SdlWindowHandle, (int)mousePressedPos.X, (int)mousePressedPos.Y);
				mousePressed = false;

				DebugControlled = false;
			}
		}

		public Rectangle GetBounds()
		{
			return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
		}

		public override Vector3 GetLookDir()
		{
			Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
			return Vector3.Transform(-Vector3.UnitZ, lookRotation);
		}

        public override void SetLookDir(Vector3 dir)
        {
			Pitch = MathF.Asin(-dir.Y);
			Yaw = MathF.Atan2(-dir.X, dir.Z);
		}

        public override Matrix4x4 GetView()
		{
			Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
			Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
			return Matrix4x4.CreateLookAt(Position, Position + lookDir, Vector3.UnitY);
		}

		public override Matrix4x4 GetProjection()
		{
			return Util.CreatePerspective(fov, Width / Height, Near, Far);
		}

		public override Vector3 GetMouseProjection()
		{
			Vector2 mousePos = Input.MousePosition;

			Matrix4x4.Invert(GetProjection(), out Matrix4x4 invertedProj);
			Matrix4x4.Invert(GetView(), out Matrix4x4 invertedView);

			float x = (mousePos.X) / viewport.X  *  2 - 1;
			float y = (mousePos.Y) / viewport.Y * -2 + 1;

			Vector3 projectedFar = Vector3.Transform(Vector3.Transform(new Vector3(x, y, Far), invertedProj), invertedView);
			
			return projectedFar;
		}

		private static bool WithinEpsilon(float a, float b)
		{
			float num = a - b;
			return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}

        public override void UpdateViewport(int width, int height)
        {
        }
    }
}
