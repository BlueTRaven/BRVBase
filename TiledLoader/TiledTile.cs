using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using Veldrid;

namespace BRVBase
{
	public class TiledTile
	{
		public int tilesetId;
		public int gid;
		public string type;

		//public Animation animation;

		public string textureName;
		private TextureAndSampler texture;
		public Rectangle sourceRectangle;
		public Rectangle collisionAABB;

		public float drawPriority;

		public Dictionary<string, string> properties;

		public TiledTile()
		{

		}

		public void Initialize(TmxTilesetTile tile, float drawPriority, string textureName, Rectangle sourceRectangle)
		{
			this.tilesetId = tile.Id;

			this.drawPriority = drawPriority;
			this.textureName = textureName;
			this.sourceRectangle = sourceRectangle;

			TmxObject obj = null;
			if (tile.ObjectGroups != null && tile.ObjectGroups.Count > 0 && tile.ObjectGroups[0].Objects != null && tile.ObjectGroups[0].Objects.Count > 0)
				obj = tile.ObjectGroups[0].Objects[0];

			if (obj != null)
				collisionAABB = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);

			this.type = tile.Type;

			properties = tile.Properties;
	
			if (GetPropertyBool("draw", true))
				this.drawPriority = 0;
			else this.drawPriority = -1;
		}

		/*public void Initialize(float drawPriority, Texture2D texture, Rectangle sourceRectangle, Dictionary<string, string> properties)
		{
			this.drawPriority = drawPriority;
			this.texture = texture;
			this.sourceRectangle = sourceRectangle;
			
			this.properties = new Dictionary<string, string>();

			if (properties != null)
			{	//NOTE: we need to do this this way otherwise the properties dictionary will remain the TiledSharp version (for some reason???)
				this.properties = new Dictionary<string, string>();

				foreach (string key in properties.Keys)
				{
					this.properties.Add(key, properties[key]);
				}
			}
		}*/

		public bool HasTexture()
		{
			return texture.texture != null;
		}

		public void SetTexture(TextureAndSampler texture)
		{
			if (this.texture.texture == null)
				this.texture = texture;
		}

		public TextureAndSampler GetTexture()
		{
			if (texture.texture != null)
				return texture;
			else return default;
		}

		public bool GetPropertyBool(string name, bool def = false)
		{
			if (properties.ContainsKey(name))
				return bool.Parse(properties[name]);
			else return def;
		}

		public float GetPropertyFloat(string name, float def = 0)
		{
			if (properties.ContainsKey(name))
				return float.Parse(properties[name]);
			else return def;
		}

		public int GetPropertyInt(string name, int def = 0)
		{
			if (properties.ContainsKey(name))
				return int.Parse(properties[name]);
			else return def;
		}

		public string GetPropertyString(string name, string def = "")
		{
			if (properties.ContainsKey(name))
				return properties[name];
			else return def;
		}

		public RgbaFloat GetPropertyColor(string name, RgbaFloat def)
		{
			if (properties.ContainsKey(name))
			{
				string str = properties[name].Substring(1);
				string aStr = str.Substring(0, 2);
				string rStr = str.Substring(2, 2);
				string gStr = str.Substring(4, 2);
				string bStr = str.Substring(6, 2);

				int.TryParse(rStr, System.Globalization.NumberStyles.HexNumber, null, out int r);
				int.TryParse(gStr, System.Globalization.NumberStyles.HexNumber, null, out int g);
				int.TryParse(bStr, System.Globalization.NumberStyles.HexNumber, null, out int b);
				int.TryParse(aStr, System.Globalization.NumberStyles.HexNumber, null, out int a);

				return new RgbaFloat(r, g, b, a);
			}
			else return def;
		}
	}
}
