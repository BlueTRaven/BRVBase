using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BRVBase
{
	public class DialogueAsset : IAsset
	{
		private readonly string name;
		private DialogueAssetData data;

		public readonly int NumSpeakers;
		public readonly int NumDialogues;

		public DialogueAsset(string name, DialogueAssetData data)
		{
			this.name = name;
			this.data = data;

			this.NumSpeakers = data.speakers.Length;
			this.NumDialogues = data.dialogues.Length;
		}

		public ref DialogueAssetData.Speaker GetSpeaker(int index)
		{
			return ref data.speakers[index];
		}

		public ref DialogueAssetData.Dialogue GetDialogue(int index)
		{
			return ref data.dialogues[index];
		}

		public string GetName()
		{
			return name;
		}
	}

	public struct DialogueAssetData
	{
		public struct Speaker
		{
			public string name;
			public string textureAssetName;
		}

		public struct Dialogue
		{
			public int speaker;
			public int charspeed;
			public string dialogue;
		}

		public Speaker[] speakers;
		public Dialogue[] dialogues;

		public void Cleanup()
		{
			for (int i = 0; i < dialogues.Length; i++)
			{
				if (dialogues[i].dialogue.Contains("<speaker>"))
					dialogues[i].dialogue = dialogues[i].dialogue.Replace("<speaker>", speakers[dialogues[i].speaker].name);
			}
		}
	}

	public class AssetLoaderDialogue : AssetLoader<DialogueAsset>
	{
		private JsonSerializerOptions options;

		public AssetLoaderDialogue() : base(Constants.ASSETS_BASE_DIR + "Dialogues/", ".json")
		{
			options = new JsonSerializerOptions()
			{
				IncludeFields = true
			};
		}

		protected override DialogueAsset Load(string name)
		{
			if (File.Exists(baseDir + name + extension))
			{
				Util.WaitForFile(baseDir + name + extension);

				var data = JsonSerializer.Deserialize<DialogueAssetData>(File.ReadAllText(baseDir + name + extension), options);
				data.Cleanup();

				return new DialogueAsset(name, data);
			}

			return null;
		}
	}
}
