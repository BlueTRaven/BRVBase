using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace BRVBase
{
	public struct StartupSettings
	{
		public int WindowWidth;
		public int WindowHeight;
		public GraphicsBackend Backend;

		public string Title;

		public bool UseFixedTimestep;
		public float FixedTimestep;
	}

	public class Runner
	{
		//While the frame is active, this semaphore is locked.
		public static SemaphoreSlim FrameSemaphore;

		private GraphicsDevice device;
		private ResourceFactory factory;

		private Sdl2Window window;

		public static FrameCounter FrameCounter;

		private StartupSettings settings;

		private bool exit;

		public Runner(Game game, StartupSettings settings)
		{
			FrameCounter = new FrameCounter();

			FrameSemaphore = new SemaphoreSlim(1);

			WindowCreateInfo ci = new WindowCreateInfo()
			{
				X = 64,
				Y = 64,
				WindowWidth = (int)Constants.WINDOW_WIDTH,
				WindowHeight = (int)Constants.WINDOW_HEIGHT,
				WindowTitle = "Test",
			};

			window = VeldridStartup.CreateWindow(ci);

			GraphicsDeviceOptions options = new GraphicsDeviceOptions()
			{
				PreferStandardClipSpaceYDirection = true,
				PreferDepthRangeZeroToOne = true,
			};

			device = VeldridStartup.CreateGraphicsDevice(window, options, settings.Backend);
			factory = device.ResourceFactory;

			game.LoadContent(device, factory);
			game.Initialize(window);

			this.settings = settings;
		}

		public void Run(Game game)
		{
			Stopwatch watch = Stopwatch.StartNew();
			double totalTime = watch.Elapsed.TotalSeconds;
			DeltaTime delta = new DeltaTime();

			double accumulator = 0;
			double accumulatedTimestep = 0;

			const double ACCUMULATOR_DISCARD = 1;

			while (window.Exists && !BRVBase.Game.Exit && !device.SwapchainFramebuffer.IsDisposed)
			{
				window.PumpEvents();

				if (settings.UseFixedTimestep)
				{
					accumulatedTimestep = 0;
					accumulator += delta.Delta;

					//if greater than ACCUMULATOR_DISCARD seconds, we probably paused or something, so just skip it
					if (accumulator > ACCUMULATOR_DISCARD)
						accumulator = 0;

					while (accumulator > settings.FixedTimestep)
					{
						accumulator -= settings.FixedTimestep;
						accumulatedTimestep += settings.FixedTimestep;

						DeltaTime dt = new DeltaTime(delta.Now + accumulatedTimestep, settings.FixedTimestep);
						Update(game, dt);

						if (window.Exists)
							Draw(game, delta);
						else exit = true;

						FrameCounter.Update(dt.Delta);
					}
				}
				else
				{
					Update(game, delta);

					if (window.Exists)
						Draw(game, delta);
					else exit = true;

					FrameCounter.Update(delta.Delta);
				}

				double newTotalTime = watch.Elapsed.TotalSeconds;
				delta = new DeltaTime(newTotalTime, newTotalTime - totalTime);
				totalTime = newTotalTime;

				if (exit)
					break;
			}

			watch.Stop();
			Console.WriteLine("Ran for " + watch.Elapsed.TotalSeconds.ToString() + " seconds.");
			window.Close();
			device.Dispose();
		}

		private void Update(Game main, DeltaTime delta)
		{
			FrameSemaphore.Wait();

			main.Update(delta);

			FrameSemaphore.Release();
		}

		private void Draw(Game main, DeltaTime delta)
		{
			FrameSemaphore.Wait();

			main.Draw(delta);

			FrameSemaphore.Release();

			device.SwapBuffers();
		}
	}
}
