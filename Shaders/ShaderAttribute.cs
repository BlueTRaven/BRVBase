using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase.Shaders
{
	public class ShaderAttribute : Attribute
	{
		private readonly string vertexShader;
		private readonly string fragmentShader;
		private readonly string[] shaderNames;

		public ShaderAttribute(string vertexShader, string fragmentShader)
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;

			this.shaderNames = new string[2] { vertexShader, fragmentShader };
		}

		public (string vertex, string fragment) GetNames()
		{
			return (vertexShader, fragmentShader);
		}

		public string[] GetNamesArr()
		{
			return shaderNames;
		}

		public (AssetHandle<ShaderWrapper> vertex, AssetHandle<ShaderWrapper> fragment) GetShaders()
		{
			var vertex = Services.ServiceManager.Instance.GetService<AssetManager>().ShaderLoader.GetHandle(vertexShader);
			var fragment = Services.ServiceManager.Instance.GetService<AssetManager>().ShaderLoader.GetHandle(fragmentShader);
			return (vertex, fragment);
		}
	}
}
