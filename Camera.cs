using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public abstract class Camera
	{
		public Vector3 Position;
		public float Yaw;
		public float Pitch;
		public float Roll;
		public float Width;
		public float Height;
		public readonly float Near;
		public readonly float Far;

		protected Point viewport;

		public bool DebugControlled;

		public Camera(float width, float height, float near, float far, Point viewport)
		{
			Width = width;
			Height = height;
			Near = near;
			Far = far;

			this.viewport = viewport;
		}

		public virtual void UpdateViewport(int width, int height)
        {
			this.viewport.X = width;
			this.viewport.Y = height;
        }

		public virtual void Update(DeltaTime delta, bool canControlSelf)
		{

		}

		public abstract Matrix4x4 GetView();

		public abstract Matrix4x4 GetProjection();

		public abstract Vector3 GetLookDir();

		public abstract void SetLookDir(Vector3 dir);

		public abstract Vector3 GetMouseProjection();
	}
}
