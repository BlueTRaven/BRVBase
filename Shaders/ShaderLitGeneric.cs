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
		private int currentResourceSet;

		private float ambientStrength = 0.5f;
		private RgbaFloat ambientColor = RgbaFloat.White;
		private Vector3 lightPos;

		public ShaderResourceManager Uniforms;

		public ShaderLitGeneric(GraphicsDevice device, ResourceFactory factory) : base(device, factory, 1)
		{
			/*Uniforms = new ShaderUniformManager(factory, device, null, "UserDefined", new Dictionary<string, ShaderResourceManager.UniformValidator>()
            {
				{ "AmbientStrength", new ShaderResourceManager.UniformValidator(typeof(float), ShaderStages.Fragment) },
				{ "AmbientColor", new ShaderResourceManager.UniformValidator(typeof(RgbaFloat), ShaderStages.Fragment) },
				{ "LightPos", new ShaderResourceManager.UniformValidator(typeof(Vector3), ShaderStages.Fragment) }
            });

			Uniforms.Set("AmbientStrength", ambientStrength, ShaderStages.Fragment);
			Uniforms.Set("AmbientColor", ambientColor, ShaderStages.Fragment);
			Uniforms.Set("LightPos", lightPos, ShaderStages.Fragment);*/
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
				bool all = false;

				if (ImGui.BeginCombo("Resource Set", currentResourceSet == -1 ? "All" : currentResourceSet.ToString()))
                {
					for (int i = -1; i < trackedManagers.Count; i++)
                    {
						if (i == -1)
                        {
							if (ImGui.Selectable("All"))
							{
								all = true;
								currentResourceSet = -1;
							}
							continue;
                        }

						if (trackedManagers[i].TryGetTarget(out ShaderResourceManager[] target) )
						{
							if (ImGui.Selectable("Set " + i.ToString()))
								currentResourceSet = i;
						}
						else
						{
							trackedManagers.RemoveAt(i);
							i++;
						}
                    }

					ImGui.EndCombo();
                }

				int rsI = currentResourceSet == -1 ? 0 : currentResourceSet;
				if (trackedManagers.Count > 0 && trackedManagers[rsI].TryGetTarget(out ShaderResourceManager[] managers) && !managers.All(x => x.IsDisposed()))
				{
					float ambientStrength = managers[1].Get<float>("AmbientStrength", ShaderStages.Fragment);
					RgbaFloat ambientColor = managers[1].Get<RgbaFloat>("AmbientColor", ShaderStages.Fragment);
					Vector3 lightPos = managers[1].Get<Vector3>("LightPos", ShaderStages.Fragment);

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
						if (!all)
						{
							managers[1].Set("AmbientStrength", ambientStrength, ShaderStages.Fragment);
							managers[1].Set("AmbientColor", ambientColor, ShaderStages.Fragment);
							managers[1].Set("LightPos", lightPos, ShaderStages.Fragment);
						}
						else
						{
							for (int i = 1; i < trackedManagers.Count; i++)
                            {
								if (trackedManagers[currentResourceSet].TryGetTarget(out ShaderResourceManager[] allManagers) && !managers.All(x => x.IsDisposed()))
                                {
									allManagers[1].Set("AmbientStrength", ambientStrength, ShaderStages.Fragment);
									allManagers[1].Set("AmbientColor", ambientColor, ShaderStages.Fragment);
									allManagers[1].Set("LightPos", lightPos, ShaderStages.Fragment);
								}
							}
						}
					}
				}
				else currentResourceSet = 0;
			}
		}

        public override VertexLayoutDescription GetVertexLayout()
		{
			return DefaultVertexDefinitions.VertexPositionNormalTextureColor.GetLayout();
		}

		protected override ShaderResourceManager[] CreateResourceManagers(ResourceLayout[] layouts)
		{
			ShaderResourceManager[] managers = new ShaderResourceManager[2];
			managers[0] = new ShaderResourceManager(layouts[0], factory, device, "Default", null);
			managers[0].Assign<Matrix4x4>("ViewProj", ShaderStages.Vertex);
			managers[0].Assign<Matrix4x4>("Model", ShaderStages.Vertex);
			managers[0].Assign<RgbaFloat>("Tint", ShaderStages.Vertex, defaultValue: RgbaFloat.White);
			managers[0].AssignTextureAndSampler("Texture1", ShaderStages.Fragment);

			managers[1] = new ShaderResourceManager(layouts[1], factory, device, null, "UserDefined");
			managers[1].Assign<float>("AmbientStrength", ShaderStages.Fragment, defaultValue: 0.1f);
			managers[1].Assign<RgbaFloat>("AmbientColor", ShaderStages.Fragment, defaultValue: RgbaFloat.White);
			managers[1].Assign<Vector3>("LightPos", ShaderStages.Fragment);

			return managers;
		}

		protected override ResourceLayout[] CreateResourceLayouts()
		{
			return new ResourceLayout[2]
			{
				new ResourceLayoutBuilder(factory).Uniform("Default", ShaderStages.Vertex)
					.Texture("Texture1", ShaderStages.Fragment)
					.Sampler("Texture1Sampler", ShaderStages.Fragment).Build(),
				new ResourceLayoutBuilder(factory).Uniform("UserDefined", ShaderStages.Fragment).Build()
			};
		}
	}
}
