using FontStashSharp;
using StbTrueTypeSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class Font : IAsset
	{
		private readonly string name;

		private FontSystem fs;

		public Font(FontTextureManager textureManager, int blur = 0, int stroke = 0, bool premult = false)
		{
			FontSystemSettings settings = new FontSystemSettings()
			{
				TextureHeight = 1024,
				TextureWidth = 1024,
				PremultiplyAlpha = premult
			};

			if (stroke > 0)
			{
				settings.Effect = FontSystemEffect.Stroked;
				settings.EffectAmount = stroke;
			}
			else if (blur > 0)
			{
				settings.Effect = FontSystemEffect.Blurry;
				settings.EffectAmount = blur;
			}

			fs = new FontSystem(settings);
		}

		public void AddFont(byte[] fontBytes)
		{
			fs.AddFont(fontBytes);
		}

		public DynamicSpriteFont GetFont(int size)
		{
			return fs.GetFont(size);
		}

		public string GetName()
		{
			return name;
		}
	}

	public class AssetLoaderFont : AssetLoader<Font>
	{
		private readonly GraphicsDevice device;
		private readonly ResourceFactory factory;
		private readonly FontTextureManager textureManager;

		private JsonSerializerOptions options;

		public AssetLoaderFont(GraphicsDevice device, ResourceFactory factory) : base(new string[] { "../Asssets/Fonts/", "./Assets/Fonts/" }, ".json")
		{
			this.device = device;
			this.factory = factory;

			textureManager = new FontTextureManager(device, factory);

			options = new JsonSerializerOptions()
			{
				IncludeFields = true
			};
		}

		private struct FontData
		{
			public string[] fontFiles;
			public int blur;
			public int stroke;
			public bool premult;
		}

		protected override Font Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				var fontData = JsonSerializer.Deserialize<FontData>(File.ReadAllText(file.FullPath), options);

				Font f = new Font(textureManager, fontData.blur, fontData.stroke, fontData.premult);

				foreach (string fontName in fontData.fontFiles)
				{
					f.AddFont(File.ReadAllBytes(file.FullPath));
				}

				return f;
			}

			return default;
		}
	}
}
