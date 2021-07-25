using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class World
	{
		private AssetHandle<TiledMap> map;

		public World(AssetHandle<TiledMap> map, GraphicsDevice device)
		{
			this.map = map;
		}

		public void Update(DeltaTime dt)
		{
		}

		public AssetHandle<TiledMap> GetMap()
		{
			return map;
		}

		public void Draw(DeltaTime dt)
		{

		}
	}
}
