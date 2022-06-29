using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase.Shaders
{
    public class ShaderResourceLayout
    {
        /*private readonly ResourceLayoutElement[][] elements;
        private ResourceLayout[] cachedLayouts;

        public ShaderResourceLayout(ResourceLayoutElement[][] elements)
        {
            this.elements = elements;
        }

        public ResourceLayout[] GetLayouts(ResourceFactory factory)
        {
            cachedLayouts = new ResourceLayout[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                ResourceLayoutBuilder builder = new ResourceLayoutBuilder(factory);

                for (int j = 0; j < elements.Length; j++)
                {
                    if (elements[i][j].ElementType == LayoutElementType.Texture)
                        builder.Texture(elements[i][j].Name, elements[i][j].Stage);
                }

                cachedLayouts[i] = builder.Build();
            }
        }*/
    }
}
