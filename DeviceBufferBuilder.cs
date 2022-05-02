using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public struct DeviceBufferBuilder
	{
		private readonly ResourceFactory factory;

		private uint bytes;

		public DeviceBufferBuilder(ResourceFactory factory)
		{
			this.factory = factory;
			bytes = 0;
		}

		public DeviceBufferBuilder Float()
		{
			//pad to 16 bytes (vec4)
			bytes += 16;
			return this;
		}

		public DeviceBufferBuilder Int()
		{
			bytes += 16;
			return this;
		}

		public DeviceBufferBuilder Vector2()
		{
			//pad to 16 bytes (vec4)
			bytes += 16;
			return this;
		}

		public DeviceBufferBuilder Vector3()
		{
			//pad to 16 bytes (vec4)
			bytes += 16;
			return this;
		}

		public DeviceBufferBuilder Vector4()
		{
			//pad to 16 bytes (vec4)
			bytes += 16;
			return this;
		}

		public DeviceBufferBuilder Mat4x4()
		{
			bytes += 64;
			return this;
		}

		public DeviceBufferBuilder FloatArr(int elements)
		{
			bytes += (uint)(16 * elements);

			return this;
		}

		public DeviceBufferBuilder IntArr(int elements) 
		{
			bytes += (uint)(16 * elements);

			return this;
		}

		public DeviceBufferBuilder Vector2Arr(int elements)
		{
			bytes += (uint)(16 * elements);

			return this;
		}

		public DeviceBufferBuilder Vector3Arr(int elements)
		{
			bytes += (uint)(16 * elements);

			return this;
		}

		public DeviceBufferBuilder Vector4Arr(int elements)
		{
			bytes += (uint)(16 * elements);

			return this;
		}

		public DeviceBufferBuilder Mat4x4Arr(int elements)
		{
			bytes += (uint)(64 * elements);

			return this;
		}

		public DeviceBuffer Build()
		{
			if (bytes > 0)
				return factory.CreateBuffer(new BufferDescription(bytes, BufferUsage.UniformBuffer));
            else
            {
				Console.WriteLine("Cannot build a buffer with 0 bytes. Did you forget to add values?");
				return null;
            }
		}
    }
}
