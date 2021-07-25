using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace BRVBase.Shaders
{
	public class ShaderError : ShaderBase
	{
		private Shader vertexShader;
		private Shader fragmentShader;

		public ShaderError(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 0)
		{
		}

		public override Shader[] LoadShaders()
		{
			AssetHandle<ShaderWrapper> vertex = Main.AssetManager.ShaderLoader.GetHandle("vertex_error");
			AssetHandle<ShaderWrapper> fragment = Main.AssetManager.ShaderLoader.GetHandle("frag_error");

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			vertexShader = spirvOut[0];
			fragmentShader = spirvOut[1];

			return new Shader[] { vertexShader, fragmentShader };
		}

		public override void SetTexture(int index, TextureAndSampler texture)
		{
			//Do nothing, can't bind textures
		}

		protected override ResourceLayout CreateUserDefinedResourceLayout()
		{
			return null;
		}

		protected override ResourceSet CreateUserDefinedResourceSet()
		{
			return null;
		}
	}
}
