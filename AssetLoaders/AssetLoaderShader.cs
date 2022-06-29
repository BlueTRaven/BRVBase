using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public readonly struct ShaderWrapper : IAsset
	{
		public readonly string Name;
		public readonly Veldrid.ShaderStages Stage;
		public readonly string Entry;
		public readonly string Content;

		public readonly bool Valid;

		public ShaderWrapper(string name, Veldrid.ShaderStages stage, string entry, string content)
		{
			this.Name = name;
			this.Stage = stage;
			Entry = entry;
			Content = content;

			Valid = true;
		}

		public string GetName()
		{
			return Name;
		}
	}

	public class AssetLoaderShader : AssetLoader<ShaderWrapper>
	{
		public AssetLoaderShader() : base(new string[] { "./Assets/Shaders/", "./BRVBase/Assets/Shaders/" }, ".glsl")
		{
		}

		protected override ShaderWrapper Load(LoadableFile file)
		{
			if (File.Exists(file.FullPath))
			{
				Util.WaitForFile(file.FullPath);

				string text = File.ReadAllText(file.FullPath);

				ShaderStages stage = ShaderStages.None;

				//TODO might have to support different line ending types. offset might be wrong on other systems!
				int offset = 0;
				if (text.StartsWith("//vertex"))
				{
					stage = ShaderStages.Vertex;
					offset = 10;
				}
				else if (text.StartsWith("//fragment"))
				{
					stage = ShaderStages.Fragment;
					offset = 12;
				}
				else
				{
					throw new Exception("Couldn't determine shader type. Each shader file must start with //vertex, //fragment, //compute, etc.");
				}

				string contentMinusStage = text.Substring(offset, text.Length - offset);
				string entry = "main";

				if (contentMinusStage.StartsWith("//entry"))
				{
					int offset1 = 7;
					for (int i = offset1; i < contentMinusStage.Length; i++)
					{ 
						if (contentMinusStage[i] == '\n')
						{
							if (i == offset1)
								break;
							else
							{
								entry = contentMinusStage.Substring(offset1, i - offset1 - 1).Trim();
								break;
							}
						}
					}
				}

				return new ShaderWrapper(file.Name, stage, entry, contentMinusStage);
			}

			return default;
		}
	}
}
