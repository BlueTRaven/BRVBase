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
    [Shader("vertex_cubedepth", "frag_cubedepth")]
    public class ShaderCubeDepth : ShaderBase
    {
		public ShaderUniformManager UniformManager;

        public ShaderCubeDepth(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 0)
        {
            UniformManager = new ShaderUniformManager(factory, device, "CubeSet", null, new Dictionary<string, ShaderUniformManager.UniformValidator>()
            {
                { "CubeViewProj", new ShaderUniformManager.UniformValidator(typeof(Matrix4x4[]), ShaderStages.Vertex) },
				{ "Face", new ShaderUniformManager.UniformValidator(typeof(int), ShaderStages.Vertex) }
            });

			UniformManager.InitArr<Matrix4x4>("CubeViewProj", 5, ShaderStages.Vertex);
			UniformManager.Set("Face", 0, ShaderStages.Vertex);
        }

        public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			Shader vertexShader = spirvOut[0];
			vertexShader.Name = "Vertex Cube Depth";
			Shader fragmentShader = spirvOut[1];
			fragmentShader.Name = "Frag Cube Depth";

			return new Shader[] { vertexShader, fragmentShader };
		}

		//We do have to use a bigger vertex definition because otherwise we get improper stride
		public override VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout();
		}

		protected override ResourceLayout CreateUserDefinedResourceLayout()
		{
			return UniformManager.GetLayout();
		}

		protected override ResourceSet CreateUserDefinedResourceSet()
		{
			return UniformManager.GetSet();
		}
	}
}
