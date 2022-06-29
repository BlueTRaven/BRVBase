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
		public ShaderResourceManager UniformManager;

        public ShaderCubeDepth(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 0)
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
			vertexShader.Name = "Vertex Cube Depth";
			Shader fragmentShader = spirvOut[1];
			fragmentShader.Name = "Frag Cube Depth";

			return new Shader[] { vertexShader, fragmentShader };
		}
		
        protected override ShaderResourceManager[] CreateResourceManagers(ResourceLayout[] layouts)
        {
			ShaderResourceManager[] managers = new ShaderResourceManager[1];
			managers[0] = new ShaderResourceManager(layouts[0], factory, device, "Default", null);
			managers[0].Assign<Matrix4x4>("ViewProj", ShaderStages.Vertex);
			managers[0].Assign<Matrix4x4>("Model", ShaderStages.Vertex);

			managers[0].AssignArr<Matrix4x4>("CubeViewProj", 5, ShaderStages.Vertex);
			managers[0].Assign<int>("Face", ShaderStages.Vertex);

			return managers;
        }

        protected override ResourceLayout[] CreateResourceLayouts()
        {
			ResourceLayout[] layouts = new ResourceLayout[1];

			ResourceLayoutBuilder builder = new ResourceLayoutBuilder(factory);
			builder.Uniform("Default", ShaderStages.Vertex);
			layouts[0] = builder.Build();

			return layouts;
        }

        //We do have to use a bigger vertex definition because otherwise we get improper stride
        public override VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout();
		}
	}
}
