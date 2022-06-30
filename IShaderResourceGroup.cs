using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
    public interface IShaderResourceGroup
    {
        IList<ShaderResourceManager> GetManagers();
        public bool AreManagersDisposed()
        {
            return (GetManagers()?.All(x =>
            {
                return x.IsDisposed();
            })).GetValueOrDefault(true);
        }
        public void DisposeManagers()
        {
            foreach (ShaderResourceManager srm in GetManagers())
            {
                srm.Dispose();
            }
        }
    }
}
