using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public static class Extensions
	{
		public static Rectangle Offset(this Rectangle rect, Vector2 offset)
		{
			return new Rectangle((int)(rect.X + offset.X), (int)(rect.Y + offset.Y), rect.Width, rect.Height);
		}

		public static bool Intersects(this Rectangle a, Rectangle b)
		{
			return b.Left < a.Right &&
				   a.Left < b.Right &&
				   b.Top < a.Bottom &&
				   a.Top < b.Bottom;
		}

		public static Point ToPointNumerics(this Vector2 vector)
		{
			return new Point((int)vector.X, (int)vector.Y);
		}

		#region Random
		public static int NextSign(this Random rand)
		{
			return rand.NextCoinFlip() ? -1 : 1;
		}

		public static Vector2 NextAngle(this Random rand)
		{
			return Vector2.Transform(new Vector2(-1, 0), Matrix3x2.CreateRotation((float)Constants.DEG_TO_RAD * rand.NextFloat(0, 360)));
		}

		public static Vector2 NextPointInside(this Random rand, Rectangle rectangle)
		{
			float x = rand.NextFloat(rectangle.X, rectangle.X + rectangle.Width);
			float y = rand.NextFloat(rectangle.Y, rectangle.Y + rectangle.Height);

			return new Vector2(x, y);
		}

		public static double NextDouble(this Random rand, double minimum, double maximum)
		{
			return rand.NextDouble() * (maximum - minimum) + minimum;
		}

		public static float NextFloat(this Random rand)
		{
			return (float)rand.NextDouble();
		}

		public static float NextFloat(this Random rand, float minimum, float maximum)
		{
			return (float)rand.NextDouble() * (maximum - minimum) + minimum;
		}

		public static bool NextCoinFlip(this Random rand)
		{   //non inclusive, so either 0 or 1
			return rand.Next(2) == 0;
		}

		public static Vector2 NextInside(this Random rand, Rectangle rectangle)
		{
			return new Vector2(rand.NextFloat(rectangle.X, rectangle.X + rectangle.Width), rand.NextFloat(rectangle.Y, rectangle.Y + rectangle.Height));
		}

		public static Vector2 NextInside(this Random rand, in Rectangle rectangle)
		{
			return new Vector2(rand.NextFloat(rectangle.X, rectangle.X + rectangle.Width), rand.NextFloat(rectangle.Y, rectangle.Y + rectangle.Height));
		}

		public static Vector2 NextOnEdge(this Random rand, Rectangle rectangle)
		{
			int side = rand.Next(4);

			switch (side)
			{
				case 0: //top side
					return new Vector2(rand.Next(rectangle.X, rectangle.X + rectangle.Width), rectangle.Y);
				case 1: //right side
					return new Vector2(rectangle.X + rectangle.Width, rand.Next(rectangle.Y, rectangle.Y + rectangle.Height));
				case 2: //bottom side
					return new Vector2(rand.Next(rectangle.X, rectangle.X + rectangle.Width), rectangle.Y + rectangle.Height);
				case 3: //left side
					return new Vector2(rectangle.X, rand.Next(rectangle.Y, rectangle.Y + rectangle.Height));
			}

			return Vector2.Zero;
		}
		#endregion
	}
}
