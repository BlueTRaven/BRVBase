using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public static class CollisionUtils
	{
		public static (bool hit, float min, float max) RaycastVsBoundingBox(Vector3 origin, Vector3 direction, BoundingBox boundingBox, Matrix4x4? offset = null)
		{
            Vector3 min = boundingBox.PositionA;
            Vector3 max = boundingBox.PositionB;

            if (offset.HasValue)
			{
                min = Vector3.Transform(min, offset.Value);
                max = Vector3.Transform(max, offset.Value);
            }

			float tmin = (min.X - origin.X) / direction.X;
			float tmax = (max.X - origin.X) / direction.X;

            Vector3 invdir = direction;
            invdir.X = 1f / direction.X;
            invdir.Y = 1f / direction.Y;
            invdir.Z = 1f / direction.Z;

            if (invdir.X >= 0)
            {
                tmin = (min.X - origin.X) * invdir.X;
                tmax = (max.X - origin.X) * invdir.X;
            }
            else
            {
                tmin = (max.X - origin.X) * invdir.X;
                tmax = (min.X - origin.X) * invdir.X;
            }

            float tymin = (min.Y - origin.Y) / direction.Y;
            float tymax = (max.Y - origin.Y) / direction.Y;

            if (invdir.Y >= 0)
            {
                tymin = (min.Y - origin.Y) * invdir.Y;
                tymax = (max.Y - origin.Y) * invdir.Y;
            }
            else
            {
                tymin = (max.Y - origin.Y) * invdir.Y;
                tymax = (min.Y - origin.Y) * invdir.Y;
            }
            /*if (tymin > tymax)
            {
                var temp = tymin;
                tymin = tymax;
                tymax = temp;
            }*/

            if ((tmin > tymax) || (tymin > tmax))
                return (false, tmin, tmax);

            if (tymin > tmin)
                tmin = tymin;

            if (tymax < tmax)
                tmax = tymax;

            float tzmin = (min.Z - origin.Z) / direction.Z;
            float tzmax = (max.Z - origin.Z) / direction.Z;

            if (invdir.Z >= 0)
            {
                tzmin = (min.Z - origin.Z) * invdir.Z;
                tzmax = (max.Z - origin.Z) * invdir.Z;
            }
            else
            {
                tzmin = (max.Z - origin.Z) * invdir.Z;
                tzmax = (min.Z - origin.Z) * invdir.Z;
            }

            /*if (tzmin > tzmax)
            {
                var temp = tzmin;
                tzmin = tzmax;
                tzmax = temp;
            }*/

            if ((tmin > tzmax) || (tzmin > tmax))
                return (false, tmin, tmax);

            if (tzmin > tmin)
                tmin = tzmin;

            if (tzmax < tmax)
                tmax = tzmax;

            return (true, tmin, tmax);
        }
	}
}
