﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
	public class AssetHandle<TAsset> where TAsset : IAsset
	{
		public readonly string Name;
		private readonly AssetLoader<TAsset> loader;

		private event Action<AssetHandle<TAsset>> AssetUnloadedEvent;

		private bool loaded;
		private TAsset asset;

		public AssetHandle(AssetLoader<TAsset> loader, string name)
		{
			this.Name = name;
			this.loader = loader;
			asset = default;
		}

		public void MarkUnloaded()
		{
			asset = default;
			loaded = false;

			if (AssetUnloadedEvent != null)
				AssetUnloadedEvent.Invoke(this);
		}

		public ref TAsset Get()
		{
			if (!loaded)
			{
				asset = loader.GetRaw(Name);

				if (asset == null)
					Console.WriteLine("ERROR: while loading asset {0}.", Name);

				loaded = true;
			}

			return ref asset;
		}
	}

	public interface IAsset
	{
		string GetName();
	}

	public abstract class AssetLoader<TAsset> where TAsset : IAsset
	{
		private Dictionary<string, AssetHandle<TAsset>> handles = new Dictionary<string, AssetHandle<TAsset>>();
		private Dictionary<string, TAsset> assets = new Dictionary<string, TAsset>();

		protected readonly string[] baseDirs;

		protected readonly string extension;

		private readonly FileSystemWatcher[] watchers;

		public event Action<string> OnAssetUnloaded;
		public event Action<string> OnAssetChanged;

		public AssetLoader(string[] baseDirs, string extension)
		{
			this.extension = extension;
			this.baseDirs = baseDirs;
			watchers = new FileSystemWatcher[baseDirs.Length];
			for (int i = 0; i < baseDirs.Length; i++)
			{
				string baseDir = baseDirs[i];

				string path = "./../../../" + baseDir;

				if (Directory.Exists(path))
				{
					path = Path.GetFullPath(path);
					watchers[i] = new FileSystemWatcher(path);
					watchers[i].EnableRaisingEvents = true;
					watchers[i].NotifyFilter = NotifyFilters.Attributes |
						NotifyFilters.CreationTime |
						NotifyFilters.FileName |
						NotifyFilters.LastAccess |
						NotifyFilters.LastWrite |
						NotifyFilters.Size |
						NotifyFilters.Security;

					watchers[i].Filter = "*";

					watchers[i].Changed += OnChanged;

					Console.WriteLine("Set up asset hot-reloading in path {0} for asset loader {1}.", path, this.GetType().Name);
				}
				else Console.WriteLine("path {0} does not yet exist for asset loader {1}.", baseDirs[i], this.GetType().Name);
			}
		}

		public AssetHandle<TAsset> GetHandle(string name)
		{
			if (!handles.ContainsKey(name))
				handles.Add(name, new AssetHandle<TAsset>(this, name));

			return handles[name];
		}

		public TAsset GetRaw(string name)
		{
			if (!assets.ContainsKey(name))
			{
				TAsset asset = default;
				for (int i = 0; i < baseDirs.Length; i++)
				{
					asset = Load(baseDirs[i], name);

					if (asset != null && asset.GetName() != null)
						break;
				}

				assets.Add(name, asset);

				PostLoad(asset);
			}

			return assets[name];
		}

		public void UnloadAsset(string name)
		{
			if (handles.ContainsKey(name))
			{
				if (assets.ContainsKey(name))
				{
					TAsset unloadingAsset = assets[name];
					Unload(name, unloadingAsset);
					assets.Remove(name);
				}
				//it's perfectly valid to call Unload when the asset only has handles and no assets.
				handles[name].MarkUnloaded();

				if (OnAssetUnloaded != null)
					OnAssetUnloaded.Invoke(name);
			}
			else
			{
				//error?
			}
		}

		private void MarkAssetChanged(string name)
		{
			UnloadAsset(name);

			if (OnAssetChanged != null)
				OnAssetChanged.Invoke(name);
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			//shouldn't need to do anything, actually...
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			Runner.FrameSemaphore.Wait();

			string realName = e.Name.Replace(extension, "");
			if (handles.ContainsKey(realName))
			{
				//Have to wait for both to copy them
				//Or maybe only the destination? TODO: Look into this
				Util.WaitForFile(e.FullPath);

				for (int i = 0; i < baseDirs.Length; i++)
				{
					string baseDir = baseDirs[i];

					if (Util.WaitForFile(baseDir + e.Name))
					{

						File.Copy(e.FullPath, baseDir + e.Name, true);
						MarkAssetChanged(realName);
					}
				}
			}

			Runner.FrameSemaphore.Release();
		}

		protected virtual void Unload(string name, TAsset asset)
		{

		}

		protected abstract TAsset Load(string baseDir, string name);

		protected virtual void PostLoad(TAsset asset) { }
	}
}
