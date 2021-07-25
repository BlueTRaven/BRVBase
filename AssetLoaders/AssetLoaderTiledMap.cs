using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class AssetLoaderTiledMap : AssetLoader<TiledMap>
	{
		private TiledLoader loader;

		public AssetLoaderTiledMap() : base(Constants.ASSETS_BASE_DIR + "Maps/", ".tmx")
		{
			loader = new TiledLoader();
		}

		protected override TiledMap Load(string name)
		{
			if (File.Exists(baseDir + name + extension))
			{
				Util.WaitForFile(baseDir + name + extension);

				return loader.LoadMap(new TiledSharp.TmxMap(baseDir + name + extension));
			}

			return null;
		}
	}
}
