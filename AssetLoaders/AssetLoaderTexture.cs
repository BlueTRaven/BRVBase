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

		public AssetLoaderTexture(GraphicsDevice device, ResourceFactory factory) : base(new string[] { "./Assets/Textures/" }, ".png")
		{
			this.device = device;
			this.factory = factory;
		}

		protected override TextureAndSampler Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				ImageSharpTexture image = new ImageSharpTexture(file.FullPath);
				var texture = image.CreateDeviceTexture(device, factory);
				texture.Name = file.Name;

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
