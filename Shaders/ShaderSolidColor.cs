using BRVBase.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;

namespace BRVBase.Shaders
{
	[Shaders.Shader("vertex_generic_2d", "frag_solidcolor")]
	public class ShaderSolidColor : ShaderBase
	{
		private Shader vertexShader;
		private Shader fragmentShader;

		public ShaderSolidColor(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 1)
		{
		}

		public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<Shaders.ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			vertexShader = spirvOut[0];
			fragmentShader = spirvOut[1];

			return new Shader[] { vertexShader, fragmentShader };
		}

		protected override ShaderResourceManager[] CreateResourceManagers(ResourceLayout[] layouts)
		{
			ShaderResourceManager[] managers = new ShaderResourceManager[1];

			managers[0] = new ShaderResourceManager(layouts[0], factory, device, "Default", null);
			managers[0].Assign<Matrix4x4>("ViewProj", ShaderStages.Vertex);
			managers[0].AssignTextureAndSampler("Texture1", ShaderStages.Fragment);

			return managers;
		}

		protected override ResourceLayout[] CreateResourceLayouts()
		{
			return new ResourceLayout[1]
			{
				new ResourceLayoutBuilder(factory).Uniform("Default", ShaderStages.Vertex)
					.Texture("Texture1", ShaderStages.Fragment)
					.Sampler("Texture1Sampler", ShaderStages.Fragment).Build()
			};
		}

		private static string[] shaderNames = { "vertex_generic_2d", "frag_solidcolor" };
		public override string[] GetShaderNamesToWatchForChanges()
		{
			return shaderNames;
		}
	}
}
