using ImGuiNET;
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
	[Shader("vertex_generic_3d", "frag_litgeneric")]
	public class ShaderLitGeneric : ShaderBase
	{
		private float ambientStrength = 0.5f;
		private RgbaFloat ambientColor = RgbaFloat.White;
		private Vector3 lightPos;

		public ShaderUniformManager Uniforms;

		public ShaderLitGeneric(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 1)
		{
			Uniforms = new ShaderUniformManager(factory, device, null, "UserDefined", new Dictionary<string, ShaderUniformManager.UniformValidator>()
            {
				{ "AmbientStrength", new ShaderUniformManager.UniformValidator(typeof(float), ShaderStages.Fragment) },
				{ "AmbientColor", new ShaderUniformManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Fragment) },
				{ "LightPos", new ShaderUniformManager.UniformValidator(typeof(Vector3), ShaderStages.Fragment) }
            });

			Uniforms.Set("AmbientStrength", ambientStrength, ShaderStages.Fragment);
			Uniforms.Set("AmbientColor", ambientColor, ShaderStages.Fragment);
			Uniforms.Set("LightPos", lightPos, ShaderStages.Fragment);
		}

		public override Shader[] LoadShaders()
		{
			(AssetHandle<ShaderWrapper> vertex,
			AssetHandle<ShaderWrapper> fragment) = Util.GetAttribute<ShaderAttribute>(this).GetShaders();

			ShaderDescription vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertex.Get().Content), "main");
			ShaderDescription fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragment.Get().Content), "main");

			var spirvOut = factory.CreateFromSpirv(vertexDesc, fragDesc);
			Shader vertexShader = spirvOut[0];
			vertexShader.Name = "Vertex Lit Generic";
			Shader fragmentShader = spirvOut[1];
			fragmentShader.Name = "Frag Lit Generic";

			return new Shader[] { vertexShader, fragmentShader };
		}

		public void UpdateInspector(DeltaTime delta, Camera camera)
		{
			if (ImGui.Begin(GetType().Name))
			{
				bool anyChanged = false;
				Vector3 color = new Vector3(ambientColor.R, ambientColor.G, ambientColor.B);

				if (ImGui.SliderFloat("Ambient Strength", ref ambientStrength, 0, 1))
					anyChanged = true;
				if (ImGui.ColorEdit3("Ambient Color", ref color))
				{
					ambientColor = new RgbaFloat(color.X, color.Y, color.Z, 1); 
					anyChanged = true;
				}
				if (ImGui.Button("Set Position"))
                {
					lightPos = camera.Position;
					anyChanged = true;
                }

                if (anyChanged)
                {
					Uniforms.Set("AmbientStrength", ambientStrength, ShaderStages.Fragment);
					Uniforms.Set("AmbientColor", ambientColor, ShaderStages.Fragment);
					Uniforms.Set("LightPos", lightPos, ShaderStages.Fragment);
					MarkUserDefinedUniformsDirty();
                }
			}
		}

		public override VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout();
		}

		protected override ResourceLayout CreateUserDefinedResourceLayout()
		{
			return Uniforms.GetLayout();
		}

		protected override ResourceSet CreateUserDefinedResourceSet()
		{
			return Uniforms.GetSet();
		}
	}
}
