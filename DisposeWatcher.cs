using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRVBase
{
    public class DisposeWatcher<T>
    {
        public delegate T CreateObjDel(ref T originalObj);

        private T obj;
        private CreateObjDel CreateObj;
        private Func<T, bool> GetDisposed;

        public DisposeWatcher(CreateObjDel createObj, Func<T, bool> getDisposed)
        {
            this.CreateObj = createObj;
            this.GetDisposed = getDisposed;

            T probablyNull = obj;

            obj = createObj(ref probablyNull);
        }

        public ref T Get()
        {
            if (GetDisposed(obj))
                obj = CreateObj(ref obj);

            return ref obj;
        }

        public bool IsDisposed()
        {
            return GetDisposed(obj);
        }
    }
}
