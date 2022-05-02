using BRVBase.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace BRVBase.Shaders
{
	[Shader("vertex_generic_2d", "frag_generic")]
	public class ShaderGeneric2D : ShaderBase
	{
		public ShaderGeneric2D(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 1)
		{
		}

		public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			Shader vertexShader = spirvOut[0];
			Shader fragmentShader = spirvOut[1];

			return new Shader[] { vertexShader, fragmentShader };
		}

		protected override ResourceLayout CreateUserDefinedResourceLayout()
		{
			return null;
		}

		protected override ResourceSet CreateUserDefinedResourceSet()
		{
			return null;
		}

		private static string[] shaderNames = { "vertex_generic", "frag_generic" };
		public override string[] GetShaderNamesToWatchForChanges()
		{
			return shaderNames;
		}
	}
}
