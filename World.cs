using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public abstract class World
	{
		private AssetHandle<TiledMap> map;
		public Camera Camera;
		protected readonly GraphicsDevice device;

		public World(AssetHandle<TiledMap> map, GraphicsDevice device)
		{
			this.map = map;
			this.device = device;
		}

		public virtual void Update(DeltaTime delta)
		{
		}

		public virtual void PausedUpdate(DeltaTime delta)
		{

		}

		public AssetHandle<TiledMap> GetMap()
		{
			return map;
		}

		public virtual void Draw(DeltaTime delta)
		{

		}
	}
}
