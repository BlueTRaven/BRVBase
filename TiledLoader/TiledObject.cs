using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;
using Veldrid;

namespace BRVBase
{
	/*public class TiledText
	{
		public string FontFamily { get; }
		public int PixelSize { get; }
		public bool Wrap { get; }
		public RgbaFloat Color { get; }
		public bool Bold { get; }
		public bool Italic { get; }
		public bool Underline { get; }
		public bool Strikeout { get; }
		public bool Kerning { get; }
		public Enums.Alignment Alignment { get; }
		public string Value { get; }

		public TiledText(TmxText text)
		{
			FontFamily = text.FontFamily;
			PixelSize = text.PixelSize;
			Wrap = text.Wrap;
			Color = new Color(text.Color.R, text.Color.G, text.Color.B);
			Bold = text.Bold;
			Italic = text.Italic;
			Underline = text.Underline;
			Strikeout = text.Strikeout;
			Kerning = text.Kerning;

			switch (text.Alignment.Horizontal)
			{
				case TmxHorizontalAlignment.Left:
					Alignment |= Enums.Alignment.Left;
					break;
				case TmxHorizontalAlignment.Center:
					Alignment = Enums.Alignment.Center;
					break;
				case TmxHorizontalAlignment.Right:
					Alignment |= Enums.Alignment.Right;
					break;
				case TmxHorizontalAlignment.Justify:
				default:
					break;
			}

			switch (text.Alignment.Vertical)
			{
				case TmxVerticalAlignment.Top:
					Alignment |= Enums.Alignment.Top;
					break;
				case TmxVerticalAlignment.Center:
					Alignment = Enums.Alignment.Center;
					break;
				case TmxVerticalAlignment.Bottom:
					Alignment = Enums.Alignment.Bottom;
					break;
				default:
					break;
			}

			Value = text.Value;
		}
	}*/

	public class TiledObject
	{
		public string layer;
		public Dictionary<string, string> properties;

		public int id;
		public string name;
		public string objType;

		public Rectangle position;

		//public TiledText text;

		public TiledObject()
		{
		}

		public void LoadObj(TmxObject obj)
		{
			this.id = obj.Id;
			this.name = obj.Name;
			//if (obj.Text != null)
				//text = new TiledText(obj.Text);

			position = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);

			GetProperties(obj);
		}

		private void GetProperties(TmxObject obj)
		{
			//NOTE: we need to do this this way otherwise the properties dictionary will remain the TiledSharp version (for some reason???)
			//A little inefficient but hey this will only be done content compile time
			properties = new Dictionary<string, string>();
			foreach (string key in obj.Properties.Keys)
				this.properties.Add(key, obj.Properties[key]);

			objType = obj.Type;
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

				return new RgbaFloat((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f);
			}
			else return def;
		}

		public override string ToString()
		{
			return "Tiled Object: " + name + " type: " + objType + " layer: " + layer;
		}
	}
}
