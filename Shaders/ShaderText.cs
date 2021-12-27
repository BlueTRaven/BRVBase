using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace BRVBase
{
	public class ShaderText : ShaderBase
	{
		private Shader vertexShader;
		private Shader fragmentShader;

		public ShaderText(AssetManager assetManager, GraphicsDevice device, ResourceFactory factory) : base(assetManager, device, factory, 1)
		{
		}

		public override Shader[] LoadShaders()
		{
			AssetHandle<ShaderWrapper> vertex = assetManager.ShaderLoader.GetHandle("vertex_generic");
			AssetHandle<ShaderWrapper> fragment = assetManager.ShaderLoader.GetHandle("frag_text");

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			vertexShader = spirvOut[0];
			fragmentShader = spirvOut[1];

			return new Shader[] { vertexShader, fragmentShader };
		}

		protected override ResourceSet CreateUserDefinedResourceSet()
		{
			return null;
		}

		protected override ResourceLayout CreateUserDefinedResourceLayout()
		{
			return null;
		}

		private static string[] shaderNames = { "vertex_generic", "frag_text" };
		public override string[] GetShaderNamesToWatchForChanges()
		{
			return shaderNames;
		}
	}
}
