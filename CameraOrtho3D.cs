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
	public class CameraOrtho3D : Camera
	{
		private readonly float left;
		private readonly float right;
		private readonly float bottom;
		private readonly float top;

		private bool mousePressed;
		private Vector2 mousePressedPos;

		public CameraOrtho3D(float width, float height, Point viewport) : base(width, height, 0, 1000f, viewport)
		{
			this.right = width;
			this.bottom = height;
		}

		public CameraOrtho3D(float left, float right, float bottom, float top, Point viewport) : base(Math.Abs(right - left), Math.Abs(top - bottom), 0, 1000, viewport)
		{
			this.left = left;
			this.right = right;
			this.bottom = bottom;
			this.top = top;
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

			if (!ImGui.GetIO().WantCaptureMouse &&
				(Input.IsMouseDown(MouseButton.Left) || Input.IsMouseDown(MouseButton.Right)))
			{
				if (!mousePressed)
				{
					mousePressed = true;
					mousePressedPos = Input.MousePosition;

					Sdl2Native.SDL_SetRelativeMouseMode(true);
					Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);
				}

				//Console.WriteLine(Input.MouseDelta);
				Vector2 mouseDelta = Input.MouseDelta;
				Yaw -= mouseDelta.X * 0.002f;
				Pitch -= mouseDelta.Y * 0.002f;
			}
			else if (mousePressed)
			{
				Sdl2Native.SDL_SetRelativeMouseMode(false);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);

				Sdl2Native.SDL_WarpMouseInWindow(Runner.Window.SdlWindowHandle, (int)mousePressedPos.X, (int)mousePressedPos.Y);
				mousePressed = false;
			}
		}

		public Rectangle GetBounds()
		{
			return new Rectangle((int)Position.X - (int)left, (int)Position.Y - (int)top, (int)right, (int)bottom);
		}

		public override Matrix4x4 GetView()
		{
			Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
			Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
			return Matrix4x4.CreateLookAt(Position, Position + lookDir, Vector3.UnitY);
		}

		public override Matrix4x4 GetProjection()
		{
			return Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, Near, Far);
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

		public override Vector3 GetMouseProjection()
		{
			Vector3 near = new Vector3(Input.MousePosition.X, Input.MousePosition.Y, 0);
			Vector3 far = new Vector3(Input.MousePosition.X, Input.MousePosition.Y, 1);

			Matrix4x4.Invert(GetProjection(), out Matrix4x4 invertedProj);
			Matrix4x4.Invert(GetView(), out Matrix4x4 invertedView);

			Vector3 projectedNear = Vector3.Transform(Vector3.Transform(near, invertedProj), invertedView);
			Vector3 projectedFar = Vector3.Transform(Vector3.Transform(far, invertedProj), invertedView);

			return projectedFar;
		}
    }
}
