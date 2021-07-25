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

		public DeviceBuffer Build()
		{
			return factory.CreateBuffer(new BufferDescription(bytes, BufferUsage.UniformBuffer));
		}
	}
}
