using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase.Shaders
{
    public class ShaderResourceLayoutBuilder
    {
        /*private int currentSet = 0;
        private int currentBinding = 0;

        private List<List<ResourceLayoutElement>> elements = new List<List<ResourceLayoutElement>>();

        public void AddSet()
        {
            currentSet++;
            currentBinding = 0;

            elements.Add(new List<ResourceLayoutElement>());
        }

        public void AddTexture(string name, ShaderStages stage)
        {
            ResourceLayoutElement element = new ResourceLayoutElement()
            {
                Set = currentSet,
                Binding = currentBinding,
                ElementType = LayoutElementType.Texture,
                Name = name, 
                Stage = stage
            };

            elements[currentSet].Add(element);
        }

        public void AddSampler(string name, ShaderStages stage)
        {
            ResourceLayoutElement element = new ResourceLayoutElement()
            {
                Set = currentSet,
                Binding = currentBinding,
                ElementType = LayoutElementType.Sampler,
                Name = name,
                Stage = stage
            };

            elements[currentSet].Add(element);
        }

        public void AddUniform(string name, ShaderStages stage, List<ShaderResourceManager.UniformValidator> uniforms)
        {
            ResourceLayoutElement element = new ResourceLayoutElement()
            {
                Set = currentSet,
                Binding = currentBinding,
                ElementType = LayoutElementType.Uniform,
                Name = name, Stage = stage,
                Uniforms = uniforms
            };

            elements[currentSet].Add(element);
        }

        public ShaderResourceLayout Build()
        {
            ResourceLayoutElement[][] layoutAsArray = new ResourceLayoutElement[elements.Count][];

            for (int i = 0; i < elements.Count; i++)
            {
                layoutAsArray[i] = new ResourceLayoutElement[elements[i].Count];
                
                for (int j = 0; j < elements[i].Count; j++)
                {
                    layoutAsArray[i][j] = elements[i][j];
                }
            }

            return new ShaderResourceLayout(layoutAsArray);
        }*/
    }
}
