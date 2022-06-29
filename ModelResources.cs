using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
    public struct ModelResources
    {
        public PipelineProgram Program;
        public string TextureName;
        public ShaderResourceManager ManagerWithViewProj;
        public ShaderResourceManager ManagerWithModel;
        public ShaderResourceManager ManagerWithTexture;
        public IList<ShaderResourceManager> AllManagers;

        public static bool IsDisposed(ModelResources resources)
        {
            return resources.Program == null || resources.Program.IsDisposed() || resources.AllManagers.Any(x => x.IsDisposed());
        }
    }
}
