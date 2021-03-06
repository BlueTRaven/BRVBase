using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class TiledMap : IAsset// : IDisposable
	{
		public Rectangle bounds;
		
		public List<TiledObject> Objects { get; private set; } = new List<TiledObject>();
		public Dictionary<int, TiledObject> ObjectsById { get; private set; } = new Dictionary<int, TiledObject>();

		public Dictionary<string, TiledLayer> Layers { get; private set; } = new Dictionary<string, TiledLayer>();
		public TiledLayer[] IterableLayers { get; private set; }

		public Dictionary<string, string> Properties = new Dictionary<string, string>();

		public TiledTile[] Tileset;

		public int width, height;
		public int tileWidth, tileHeight;

		public RgbaFloat backgroundColor;

		private readonly string name;

		public TiledMap(string name)
		{
			this.name = name;
		}

		/*public void Dispose()
		{
			tiledObjects.Clear();
			tiledObjects = null;

			tiles.Clear();
			tiles = null;
			bounds = RectangleF.Empty;
			width = 0;
			height = 0;
			tileWidth = 0;
			tileHeight = 0;
		}*/

		public void AddLayer(string name, TiledLayer layer)
		{
			Layers.Add(name, layer);
			IterableLayers = Layers.Values.ToArray();
		}

		public bool HasLayer(string name)
		{
			return Layers.ContainsKey(name);
		}

		public TiledLayer GetLayer(string name)
		{
			if (HasLayer(name))
				return Layers[name];
			else return null;
		}

		public List<TiledLayer> GetLayers()
		{
			return Layers.Values.ToList();
		}

		public void SetTileset(Dictionary<int, TiledTile> tileset)
		{
			int max = tileset.Max(x => x.Value.tilesetId) + 1;

			this.Tileset = new TiledTile[max];

			foreach (TiledTile tile in tileset.Values)
			{
				Tileset[tile.tilesetId] = tile;
			}
		}

		public void SetTile(string layer, Point pos, TiledTile tileType, bool flipH = false, bool flipV = false)
		{
			if (tileType == null)
				Layers[layer].tiles[pos.X, pos.Y] = default;
			else
			{
				TiledLayer.TileType.Flip flip = TiledLayer.TileType.Flip.FLIP_NONE;
				if (flipH)
					flip |= TiledLayer.TileType.Flip.FLIP_H;
				if (flipV)
					flip |= TiledLayer.TileType.Flip.FLIP_V;

				Layers[layer].tiles[pos.X, pos.Y] = new TiledLayer.TileType() { id = (ushort)tileType.gid, flip = flip };
			}
		}

		public TiledTileInstance GetTile(string layer, Point pos)
		{
			return Layers[layer].GetTile(pos);
		}

		public string GetName()
		{
			return name;
		}
	}
}
