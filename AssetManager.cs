using BRVBase.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class AssetManager : Service
	{
		public readonly AssetLoaderTexture TextureLoader;
		public readonly AssetLoaderFont FontLoader;
		public readonly AssetLoaderTiledMap MapLoader;
		public readonly AssetLoaderShader ShaderLoader;
		public readonly AssetLoaderNodeGraph NodeGraphLoader;
		public readonly AssetLoaderDialogue DialogueLoader;
		public readonly AssetLoaderModelObj ModelObjLoader;

		public AssetManager(GraphicsDevice device, ResourceFactory factory)
		{
			ShaderLoader = new AssetLoaderShader();
			FontLoader = new AssetLoaderFont(device, factory);
			TextureLoader = new AssetLoaderTexture(device, factory);
			MapLoader = new AssetLoaderTiledMap();
			ModelObjLoader = new AssetLoaderModelObj(device, factory);
		}
	}
}
