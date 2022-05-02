using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase.Services
{
	public abstract class Service
	{
		public virtual void Update(DeltaTime delta) { }
	}
}
