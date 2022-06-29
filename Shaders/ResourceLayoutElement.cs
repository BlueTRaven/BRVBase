using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase.Shaders
{
    public enum LayoutElementType
    {
        Texture,
        Sampler,
        Uniform
    }

    public struct ResourceLayoutElement
    {
        public int Set;
        public int Binding;

        public string Name;
        public ShaderStages Stage;

        public LayoutElementType ElementType;

        public List<ShaderResourceManager.UniformValidator> Uniforms;
    }
}
