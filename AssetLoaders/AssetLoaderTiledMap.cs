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

		protected override TiledMap Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				return loader.LoadMap(new TiledSharp.TmxMap(file.FullPath), file.Name);
			}

			return null;
		}
	}
}
