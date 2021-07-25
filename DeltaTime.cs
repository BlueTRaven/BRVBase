using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public ref struct DeltaTime
	{
		public readonly double Now;
		public readonly double Delta;

		public DeltaTime(double now, double delta)
		{
			this.Now = now;
			this.Delta = delta; 
		}
	}
}
