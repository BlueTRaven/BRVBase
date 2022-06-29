using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class FrameCounter
	{   //taken from https://stackoverflow.com/questions/20676185/xna-monogame-getting-the-frames-per-second
		public FrameCounter()
		{
		}

		public long TotalFrames { get; private set; }
		public double TotalSeconds { get; private set; }
		public double AverageFramesPerSecond { get; private set; }
		public double CurrentFramesPerSecond { get; private set; }

		public const int MAXIMUM_SAMPLES = 100;

		private Queue<double> _sampleBuffer = new Queue<double>();

		public bool Update(double deltaTime)
		{
			CurrentFramesPerSecond = 1.0f / deltaTime;

			_sampleBuffer.Enqueue(CurrentFramesPerSecond);

			if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
			{
				_sampleBuffer.Dequeue();
				AverageFramesPerSecond = _sampleBuffer.Average(i => i);
			}
			else
			{
				AverageFramesPerSecond = CurrentFramesPerSecond;
			}

			TotalFrames++;
			TotalSeconds += deltaTime;
			return true;
		}
	}
}
