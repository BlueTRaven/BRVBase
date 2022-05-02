using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace BRVBase.Shaders
{
	[Shader("vertex_generic_3d", "frag_generic")]
	public class ShaderGeneric3D : ShaderBase
	{
		private ShaderUniformManager vertexUniforms;
		private ShaderUniformManager fragmentUniforms;

		public ShaderGeneric3D(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 1)
		{
			vertexUniforms = new ShaderUniformManager(factory, device, "VertexLightSet", null, new Dictionary<string, ShaderUniformManager.UniformValidator>()
			{
				{ "LightViewProj", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4), ShaderStages.Vertex) },
			});
		}

		public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			Shader vertexShader = spirvOut[0];
			vertexShader.Name = "Vertex Generic 3d";
			Shader fragmentShader = spirvOut[1];
			fragmentShader.Name = "Frag Generic 3d";

			return new Shader[] { vertexShader, fragmentShader };
		}

		public override VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout();
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
