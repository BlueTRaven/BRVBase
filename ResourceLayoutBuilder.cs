using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public struct ResourceLayoutBuilder
	{
		private readonly ResourceFactory factory;
		private List<ResourceLayoutElementDescription> layout;

		public ResourceLayoutBuilder(ResourceFactory factory)
		{
			this.factory = factory;
			layout = new List<ResourceLayoutElementDescription>();
		}

		public ResourceLayoutBuilder Texture(string name, ShaderStages stages = ShaderStages.Fragment)
		{
			layout.Add(new ResourceLayoutElementDescription(name, ResourceKind.TextureReadOnly, stages));

			return this;
		}

		public ResourceLayoutBuilder Sampler(string name, ShaderStages stages = ShaderStages.Fragment)
		{
			layout.Add(new ResourceLayoutElementDescription(name, ResourceKind.Sampler, stages));

			return this;
		}

		public ResourceLayoutBuilder Uniform(string name, ShaderStages stages = ShaderStages.Fragment)
		{
			layout.Add(new ResourceLayoutElementDescription(name, ResourceKind.UniformBuffer, stages));

			return this;
		}

		public ResourceLayout Build()
		{
			return factory.CreateResourceLayout(new ResourceLayoutDescription(layout.ToArray()));
		}
	}
}
