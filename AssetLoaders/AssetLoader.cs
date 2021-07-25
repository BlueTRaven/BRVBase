using System;
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

		protected readonly string baseDir;
		protected readonly string extension;

		private readonly FileSystemWatcher watcher;

		public event Action<string> OnAssetUnloaded;
		public event Action<string> OnAssetChanged;

		public AssetLoader(string baseDir, string extension)
		{
			if (!Directory.Exists(baseDir))
			{
				return;
			}
			this.baseDir = baseDir;
			this.extension = extension;

			string path = "./../../../" + baseDir.Replace("./", "");
			path = Path.GetFullPath(path);
			watcher = new FileSystemWatcher(path);
			watcher.EnableRaisingEvents = true;
			watcher.NotifyFilter = NotifyFilters.Attributes |
				NotifyFilters.CreationTime |
				NotifyFilters.FileName |
				NotifyFilters.LastAccess |
				NotifyFilters.LastWrite |
				NotifyFilters.Size |
				NotifyFilters.Security;

			watcher.Filter = "";

			watcher.Changed += OnChanged;
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
				assets.Add(name, Load(name));

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
				Util.WaitForFile(baseDir + e.Name);

				File.Copy(e.FullPath, baseDir + e.Name, true);
				MarkAssetChanged(realName);
			}

			Runner.FrameSemaphore.Release();
		}

		protected virtual void Unload(string name, TAsset asset)
		{

		}

		protected abstract TAsset Load(string name);
	}
}
