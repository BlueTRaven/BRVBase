using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
    public struct ModelResources : IShaderResourceGroup
    {
        public Func<PipelineProgram> Program;
        public string TextureName;
        public ShaderResourceManager ManagerWithViewProj;
        public ShaderResourceManager ManagerWithModel;
        public ShaderResourceManager ManagerWithTexture;
        public IList<ShaderResourceManager> AllManagers;

        public IList<ShaderResourceManager> GetManagers()
        {
            return AllManagers;
        }
    }
}
