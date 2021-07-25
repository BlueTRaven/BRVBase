using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public static class CollisionHelper
	{
		private static Vector2[] compass = new Vector2[4]
		   {
			new Vector2(0, -1),	//up
			new Vector2(0, 1),  //down
			new Vector2(1, 0),	//left
			new Vector2(-1, 0),	//right
		   };

		public static bool CheckCollision(Rectangle bounds, in Vector2 position, in float radius, out Vector2 change)
		{
			change = Vector2.Zero;

			Vector2 halfExtents = bounds.Size / 2f;
			Vector2 center = bounds.Position + halfExtents;

			Vector2 diff = position - center;
			Vector2 clamped = Vector2.Clamp(diff, -halfExtents, halfExtents);

			Vector2 closest = center + clamped;

			diff = closest - position;

			bool intersects = diff.Length() < radius;

			if (intersects)
			{
				float max = 0f;
				int direction = -1;

				for (int i = 0; i < 4; i++)
				{
					float dot = Vector2.Dot(Vector2.Normalize(diff), compass[i]);
					if (dot > max)
					{
						max = dot;
						direction = i;
					}
				}

				if (direction == 0 || direction == 1)
				{
					float penetration = radius - Math.Abs(diff.Y);
					if (direction == 0)
						change.Y += penetration;
					else change.Y -= penetration;
				}
				else if (direction == 2 || direction == 3)
				{
					float penetration = radius - Math.Abs(diff.X);
					if (direction == 2)
						change.X -= penetration;
					else change.X += penetration;
				}
			}

			return intersects;
		}
	}
}
