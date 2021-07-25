using System;
using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.SPIRV;
using Veldrid.ImageSharp;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace BRVBase
{
	public class Program
	{
		static void Main(string[] args)
		{
			Game game = new Main();
			Runner runner = new Runner(game, new StartupSettings()
			{
				WindowWidth = Constants.WINDOW_WIDTH,
				WindowHeight = Constants.WINDOW_HEIGHT,
				Backend = GraphicsBackend.Direct3D11,
				FixedTimestep = 1f / 60f,
				UseFixedTimestep = true,

				Title = "TEMPLATE"
			});

			runner.Run(game);
		}
	}
}
