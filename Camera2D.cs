using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class Camera2D
	{
		public Vector2 Position;
		private Vector2 shakeOffset;

		private float timer;

		public void SetShake(float time)
		{
			this.timer = time;
		}

		public void Update(DeltaTime delta, Random rand)
		{
			if (timer > 0)
			{
				timer -= (float)delta.Delta;
				shakeOffset = new Vector2(rand.Next(-4, 4), rand.Next(-4, 4));
			}
			else shakeOffset = Vector2.Zero;
		}
		
		public Matrix4x4 GetMatrix()
		{
			return Matrix4x4.CreateTranslation(-new Vector3(Position + shakeOffset, 0));
		}

		public Matrix4x4 GetScaledMatrix()
		{
			return Matrix4x4.CreateScale((float)Constants.WINDOW_WIDTH / (float)Constants.INTERNAL_WIDTH,
				(float)Constants.WINDOW_HEIGHT / (float)Constants.INTERNAL_HEIGHT, 1) * GetMatrix();
		}

		public Matrix4x4 GetProjection()
		{
			return Matrix4x4.CreateOrthographicOffCenter(0, Constants.INTERNAL_WIDTH, Constants.INTERNAL_HEIGHT, 0, 0, 100);
		}
	}
}
