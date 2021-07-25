using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.ImageSharp;

namespace BRVBase
{
	public class AssetLoaderTexture : AssetLoader<TextureAndSampler>
	{
		private readonly GraphicsDevice device;
		private readonly ResourceFactory factory;

		public AssetLoaderTexture(GraphicsDevice device, ResourceFactory factory) : base(Constants.ASSETS_BASE_DIR + "Textures/", ".png")
		{
			this.device = device;
			this.factory = factory;
		}

		protected override TextureAndSampler Load(string name)
		{
			if (File.Exists(baseDir + name + extension))
			{
				Util.WaitForFile(baseDir + name + extension);

				ImageSharpTexture image = new ImageSharpTexture(baseDir + name + extension);
				var texture = image.CreateDeviceTexture(device, factory);
				texture.Name = name;

				return new TextureAndSampler(texture, device.PointSampler);
			}

			return default;
		}

		protected override void Unload(string name, TextureAndSampler unloadingAsset)
		{
			base.Unload(name, unloadingAsset);

			unloadingAsset.texture.Dispose();
		}
	}
}
