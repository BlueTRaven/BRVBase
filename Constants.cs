using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class Constants
	{
		public const int WINDOW_WIDTH = 960;
		public const int WINDOW_HEIGHT = 540;
		public const int INTERNAL_WIDTH = 480;
		public const int INTERNAL_HEIGHT = 270;
		public const string ASSETS_BASE_DIR = "./Assets/";

		public const float WINDOW_SCALE_WIDTH = (float)INTERNAL_WIDTH / (float)WINDOW_WIDTH;
		public const float WINDOW_SCALE_HEIGHT = (float)INTERNAL_HEIGHT / (float)WINDOW_HEIGHT;

		public const double RAD_TO_DEG = 180.0 / Math.PI;
		public const double DEG_TO_RAD = Math.PI / 180.0;
	}
}
