using BRVBase.Services;
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
	[Shader("vertex_error", "frag_error")]
	public class ShaderError : ShaderBase
	{
		private Shader vertexShader;
		private Shader fragmentShader;

		public ShaderError(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 0)
		{
		}

		public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			vertexShader = spirvOut[0];
			fragmentShader = spirvOut[1];

			return new Shader[] { vertexShader, fragmentShader };
		}

        protected override ResourceLayout[] CreateResourceLayouts()
        {
			return new ResourceLayout[1] { new ResourceLayoutBuilder(factory).Uniform("Default").Build() };
        }

        protected override ShaderResourceManager[] CreateResourceManagers(ResourceLayout[] layouts)
		{
			ShaderResourceManager[] managers = new ShaderResourceManager[1];

			managers[0] = new ShaderResourceManager(layouts[0], factory, device, "Default", null);
			managers[0].Assign<Matrix4x4>("ViewProj", ShaderStages.Vertex);

			return managers;
        }
    }
}
