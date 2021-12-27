using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public readonly struct LoadableFile
	{
		public readonly string Name;
		public readonly string FullPath;

		public LoadableFile(string name, string fullPath)
		{
			Name = name;
			FullPath = fullPath;
		}
	}
}
