using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public struct BoundingBox
	{
		public Vector3 PositionA;
		public Vector3 PositionB;

		public BoundingBox(Vector3 positionA, Vector3 positionB)
		{
			this.PositionA = positionA;
			this.PositionB = positionB;
		}

		public Vector3 GetCenter()
		{
			return ((this.PositionA - this.PositionB) / 2f) + this.PositionB;
		}
	}
}
