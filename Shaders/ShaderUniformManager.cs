using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace BRVBase
{
	public class ShaderUniformManager : IDisposable
	{
		public readonly struct UniformValidator
        {
			public readonly string Name;
			public readonly Type Type;
			public readonly ShaderStages Stage;

			public UniformValidator(Type type, ShaderStages stage)
            {
				this.Name = "";
				this.Type = type;
				this.Stage = stage;
            }
        }

		private readonly struct Uniform
		{
			public readonly string Name;
			public readonly object Value;
			public readonly Type Type;
			public readonly int ArrayLength;
			public readonly uint SizeBytes;
			public readonly uint Offset;
			public readonly ShaderStages Stage;

			public Uniform(string name, object value, Type type, int arrayLength, uint sizeBytes, uint offset, ShaderStages stage)
			{
				this.Name = name;
                this.Value = value;
				this.ArrayLength = arrayLength;
                this.Type = type;
				this.SizeBytes = sizeBytes;
				this.Offset = offset;
				this.Stage = stage;
			}
		}

		private DeviceBuffer bufferVertex;
		private DeviceBuffer bufferFragment;
		private Dictionary<string, Uniform> uniformsVertex = new Dictionary<string, Uniform>();
		private Dictionary<string, Uniform> uniformsFragment = new Dictionary<string, Uniform>();
		private HashSet<string> dirtyUniforms = new HashSet<string>();
		private uint offsetVertex;
		private uint offsetFragment;

		private ResourceLayout layout;
		private ResourceSet set;
		private readonly ResourceFactory factory;
		private readonly GraphicsDevice device;
		private string vertexName;
		private string fragName;

		private bool hasVertexStage;
		private bool hasFragmentStage;
        
		private bool disposedValue;

        //A dictionary of available uniforms.
        //Eventually we'll probably want to parse the glsl file to look for these, but for right now this has to be input manually.
        //If a validator does not exist, it will not validate.
        private readonly Dictionary<string, UniformValidator> validator;

		/// <summary>
		/// Creates a new ShaderUniformManager, which manages device buffers for vertex and fragment shaders.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="device"></param>
		/// <param name="vertexName">The name of the vertex shader's uniform. May be null in the case where the shader does not have one.</param>
		/// <param name="fragName">The name of the fragment shader's uniform. May be null in the case where the shader does not have one.</param>
		/// <param name="validator">A dictionary of names:validators which are responsible for validating whether a uniform exists or not.</br>
		/// Leave out to perform no validation.</br>
		/// //TODO: This should only be usable on debug mode.</param>
		public ShaderUniformManager(ResourceFactory factory, GraphicsDevice device, string vertexName, string fragName, Dictionary<string, UniformValidator> validator = null)
		{
			this.factory = factory;
			this.device = device;
			this.vertexName = vertexName;
			this.fragName = fragName;

			if (this.vertexName != null)
				hasVertexStage = true;
			if (this.fragName != null)
				hasFragmentStage = true;

			this.validator = validator;
		}

		//Use this function to initialize arrays to an empty value, as we cannot initialize them to null with Set.
		public void InitArr<T>(string name, uint length, ShaderStages stage, CommandList commandList = null) where T : struct
        {
			if (!uniformsVertex.ContainsKey(name))
			{
				if (stage == ShaderStages.Vertex)
				{
					if (Validate(name, typeof(T[]), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, Array.Empty<T>(), typeof(T[]), (int)length, (uint)(size * length), offsetVertex, stage);
						uniformsVertex.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetVertex += (uint)(size * length);
					}
					else ErrorInvalid(name, typeof(T[]));
				}
			}
			else
			{
				//If it does exist, don't do anything
				Console.WriteLine("Tried to run InitArr on value that is already initialized. Use this function once only!");
			}

			if (!uniformsFragment.ContainsKey(name))
			{
				if (stage == ShaderStages.Fragment)
				{
					if (Validate(name, typeof(T[]), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, Array.Empty<T>(), typeof(T[]), (int)length, (uint)(size * length), offsetFragment, stage);
						uniformsFragment.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetFragment += (uint)(size * length);
					}
					else ErrorInvalid(name, typeof(T[]));
				}
			}
			else
            {
				Console.WriteLine("Tried to run InitArr on value that is already initialized. Use this function once only!");
            }
		}

		//Sets the uniform and immediately updates/uploads the buffer.
		public void Set<T>(string name, T value, ShaderStages stage, CommandList commandList = null) where T : struct
        {
			if (!uniformsVertex.ContainsKey(name))
			{
				if (stage == ShaderStages.Vertex)
				{
					if (Validate(name, typeof(T), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, value, typeof(T), 0, size, offsetVertex, stage);
						uniformsVertex.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetVertex += size;
					}
					else ErrorInvalid(name, typeof(T));
				}
			}
			else
			{
				uint size = GetShaderSafeSize<T>();

				uniformsVertex[name] = new Uniform(name, value, typeof(T), 0, size, uniformsVertex[name].Offset, stage);

				//Update the one part of the buffer
				Upload(uniformsVertex[name], commandList);
			}

			if (!uniformsFragment.ContainsKey(name))
            {
				if (stage == ShaderStages.Fragment)
				{
					if (Validate(name, typeof(T), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, value, typeof(T), 0, size, offsetFragment, stage);
						uniformsFragment.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetFragment += size;
					}
					else ErrorInvalid(name, typeof(T));
				}
			}
			else
			{
				uint size = GetShaderSafeSize<T>();

				uniformsFragment[name] = new Uniform(name, value, typeof(T), 0, size, uniformsFragment[name].Offset, stage);

				Upload(uniformsFragment[name], commandList);
			}
		}

		//Sets the uniform and immediately updates/uploads the buffer.
		public void Set<T>(string name, T[] value, ShaderStages stage, CommandList commandList = null) where T : struct
		{
			if (!uniformsVertex.ContainsKey(name))
			{
				if (stage == ShaderStages.Vertex)
				{
					if (Validate(name, typeof(T[]), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, value, typeof(T[]), 0, (uint)(size * value.Length), offsetVertex, stage);
						uniformsVertex.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetVertex += (uint)(size * value.Length);
					}
					else ErrorInvalid(name, typeof(T[]));
				}
			}
			else
			{
				uint size = GetShaderSafeSize<T>();

				uint previousLen = (uint)((T[])uniformsVertex[name].Value).Length;
				if (previousLen == 0)
					previousLen = (uint)uniformsVertex[name].ArrayLength;
				previousLen *= size;

				if (previousLen != size * value.Length)
                {
					ErrorSize(name, (int)previousLen, (int)size * value.Length);
					return;
                }

				uniformsVertex[name] = new Uniform(name, value, typeof(T[]), 0, (uint)(size * value.Length), uniformsVertex[name].Offset, stage);

				Upload(uniformsVertex[name], commandList);
			}

			if (!uniformsFragment.ContainsKey(name))
			{
				if (stage == ShaderStages.Fragment)
				{
					if (Validate(name, typeof(T[]), stage))
					{
						uint size = GetShaderSafeSize<T>();

						Uniform uniform = new Uniform(name, value, typeof(T[]), 0, (uint)(size * value.Length), offsetFragment, stage);
						uniformsFragment.Add(name, uniform);
						//A new uniform has been added, therefore the buffer and set needs to be recreated.
						CreateAndUploadBuffer(commandList);
						offsetFragment += (uint)(size * value.Length);
					}
					else ErrorInvalid(name, typeof(T[]));
				}
			}
			else
			{
				uint size = GetShaderSafeSize<T>();

				uint previousLen = (uint)((T[])uniformsVertex[name].Value).Length;
				if (previousLen == 0)
					previousLen = (uint)uniformsVertex[name].ArrayLength;
				previousLen *= size;

				if (previousLen != size * value.Length)
				{
					ErrorSize(name, (int)previousLen, (int)size * value.Length);
					return;
				}

				uniformsFragment[name] = new Uniform(name, value, typeof(T[]), 0, (uint)(size * value.Length), uniformsFragment[name].Offset, stage);

				Upload(uniformsFragment[name], commandList);
			}
		}

		private bool Validate(string name, Type type, ShaderStages stage)
		{
			//If we have a validator, then check it.
			if (validator != null)
			{
				if (validator.ContainsKey(name) && validator[name].Type == type && validator[name].Stage == stage)
					return true;
				return false;
			}

			//If we don't, we just assume it exists.
			return true;
		}

		private void ErrorInvalid(string name, Type givenType)
		{
			if (validator.ContainsKey(name))
				Console.WriteLine("Attempted to set uniform {0} with invalid parameters. Given type: {1}, expected type: {2}.", name, givenType, validator[name].Type);
			else Console.WriteLine("Attempted to set non-existent uniform {0}.", name);
		}

		private void ErrorSize(string name, int oldSize, int newSize)
        {
			Console.WriteLine("Attempted to resize uniform {0} from size {1} to {2}. Resizing is an invalid operation.", name, oldSize, newSize);
		}

		//Create and upload the layouts, buffers, and sets.
		//Run this if the layout is dirty.
		//Everything needs to be reuploaded if this is run!
		private void CreateAndUploadBuffer(CommandList commandList = null)
		{
			//dispose of old buffer and set since we're going to be replacing it.
			if (bufferVertex != null)
				bufferVertex.Dispose();
			if (bufferFragment != null)
				bufferFragment.Dispose();

			if (layout != null)
				layout.Dispose();
			if (set != null)
				set.Dispose();

			bufferVertex = BuildBuffer(uniformsVertex.Values.ToList());
			bufferFragment = BuildBuffer(uniformsFragment.Values.ToList());

			//TODO: slow iteration
			foreach (Uniform uniform in uniformsVertex.Values)
			{
				Upload(uniform, commandList);
			}

			//TODO: slow iteration
			foreach (Uniform uniform in uniformsFragment.Values)
			{
				Upload(uniform, commandList);
			}

			var layoutBuilder = new ResourceLayoutBuilder(factory);
			if (uniformsVertex.Count > 0)
				layoutBuilder.Uniform(vertexName, ShaderStages.Vertex);
			if (uniformsFragment.Count > 0)
				layoutBuilder.Uniform(fragName, ShaderStages.Fragment);
			layout = layoutBuilder.Build();

			//TODO optimize this, maybe find a way to use a span? I don't like the allocation here.
			List<DeviceBuffer> bindables = new List<DeviceBuffer>();
			if (bufferVertex != null)
				bindables.Add(bufferVertex);
			if (bufferFragment != null)
				bindables.Add(bufferFragment);
			set = factory.CreateResourceSet(new ResourceSetDescription(layout, bindables.ToArray()));
		}

		private DeviceBuffer BuildBuffer(List<Uniform> uniforms)
		{
			//it's valid to not have any uniforms to build. Return null in this case.
			if (uniforms.Count == 0)
				return null;

			var builder = new DeviceBufferBuilder(factory);

			//TODO: is this necessary? Seems slow
			List<Uniform> sorted = uniforms.OrderBy(x => x.Offset).ToList();

			foreach (Uniform uniform in sorted)
			{
				if (uniform.Value is Vector4 || uniform.Value is RgbaFloat)
					builder = builder.Vector4();
				else if (uniform.Value is Vector4[] vec4Arr)
					builder = builder.Vector4Arr(vec4Arr.Length > 0 ? vec4Arr.Length : uniform.ArrayLength);
				else if (uniform.Value is Vector3 vec3)
					builder = builder.Vector3();
				else if (uniform.Value is Vector3[] vec3Arr)
					builder = builder.Vector3Arr(vec3Arr.Length > 0 ? vec3Arr.Length : uniform.ArrayLength);
				else if (uniform.Value is Vector2 vec2)
					builder = builder.Vector2();
				else if (uniform.Value is Vector2[] vec2Arr)
					builder = builder.Vector2Arr(vec2Arr.Length > 0 ? vec2Arr.Length : uniform.ArrayLength);
				else if (uniform.Value is float f)
					builder = builder.Float();
				else if (uniform.Value is float[] fArr)
					builder = builder.FloatArr(fArr.Length > 0 ? fArr.Length : uniform.ArrayLength);
				else if (uniform.Value is int i)
					builder = builder.Int();
				else if (uniform.Value is int[] iArr)
					builder = builder.FloatArr(iArr.Length > 0 ? iArr.Length : uniform.ArrayLength);
				//else if (uniform.Value is Matrix3x2 mat3x2)			//TODO implement
				//builder = builder.Mat3x2();
				//else if (uniform.Value is Matrix3x2[] mat3x2Arr)
				//builder = builder.Mat3x2Arr(mat3x2Arr.Length);
				else if (uniform.Value is Matrix4x4 mat4x4)
					builder = builder.Mat4x4();
				else if (uniform.Value is Matrix4x4[] mat4x4Arr)
					builder = builder.Mat4x4Arr(mat4x4Arr.Length > 0 ? mat4x4Arr.Length : uniform.ArrayLength);
			}

			return builder.Build();
		}

		private void Upload(in Uniform uniform, CommandList commandList = null)
		{
			//Gross else if statement, but being perfectly generic is a pain in the ass and will only make this code even harder to read,
			//so this is what we're getting.
			if (uniform.Value is Vector4 vec4)
				UploadIfList(commandList, uniform, vec4);
			else if (uniform.Value is RgbaFloat col)
				UploadIfList(commandList, uniform, col.ToVector4());
			else if (uniform.Value is Vector4[] vec4Arr)
				UploadIfList(commandList, uniform, vec4Arr);
			else if (uniform.Value is Vector3 vec3)
				UploadIfList(commandList, uniform, vec3);
			else if (uniform.Value is Vector3[] vec3Arr)
				UploadIfList(commandList, uniform, vec3Arr);
			else if (uniform.Value is Vector2 vec2)
				UploadIfList(commandList, uniform, vec2);
			else if (uniform.Value is Vector2[] vec2Arr)
				UploadIfList(commandList, uniform, vec2Arr);
			else if (uniform.Value is float f)
				UploadIfList(commandList, uniform, f);
			else if (uniform.Value is float[] fArr)
				UploadIfList(commandList, uniform, fArr);
			else if (uniform.Value is int i)
				UploadIfList(commandList, uniform, i);
			else if (uniform.Value is int[] iArr)
				UploadIfList(commandList, uniform, iArr);
			else if (uniform.Value is Matrix3x2 mat3x2)
				UploadIfList(commandList, uniform, mat3x2);
			else if (uniform.Value is Matrix3x2[] mat3x2Arr)
				UploadIfList(commandList, uniform, mat3x2Arr);
			else if (uniform.Value is Matrix4x4 mat4x4)
				UploadIfList(commandList, uniform, mat4x4);
			else if (uniform.Value is Matrix4x4[] mat4x4Arr)
				UploadIfList(commandList, uniform, mat4x4Arr);
            else
            {
				Console.WriteLine("Tried to upload unsupported type {0}", uniform.Type);
            }
		}

		private void UploadIfList<T>(CommandList commandList, Uniform uniform, T obj) where T : struct
		{
			DeviceBuffer buffer = null;
			if (uniform.Stage == ShaderStages.Vertex)
				buffer = bufferVertex;
			if (uniform.Stage == ShaderStages.Fragment)
				buffer = bufferFragment;

			if (commandList == null)
				device.UpdateBuffer(buffer, uniform.Offset, ref obj, uniform.SizeBytes);
			else commandList.UpdateBuffer(buffer, uniform.Offset, ref obj, uniform.SizeBytes);
		}

		private void UploadIfList<T>(CommandList commandList, Uniform uniform, T[] obj) where T : struct
		{
			DeviceBuffer buffer = null;
			if (uniform.Stage == ShaderStages.Vertex)
				buffer = bufferVertex;
			if (uniform.Stage == ShaderStages.Fragment)
				buffer = bufferFragment;

			if (commandList == null)
				device.UpdateBuffer(buffer, uniform.Offset, obj);
			else commandList.UpdateBuffer(buffer, uniform.Offset, obj);
		}

		public ResourceLayout GetLayout()
		{
			return layout;
		}

		public ResourceSet GetSet()
		{
			/*if (setDirty || set == null)
			{
				//UploadDirty(factory, device, commandList);

				set = factory.CreateResourceSet(new ResourceSetDescription(GetLayout(factory), buffer));
				setDirty = false;
			}*/
			return set;
		}

		public void Bind(CommandList commandList, uint slot)
        {
			commandList.SetGraphicsResourceSet(slot, set);
        }

		//Returns a size rounded up to the nearest multiple of 16.
		//This is due to the fact that shaders automatically pad up to the nearest 16 bits. Otherwise uploading 2 8 bit bytes would be seen as 1 16 bit variable in the shader.
		private uint GetShaderSafeSize<T>() where T : struct
        {
			//Sizes must be multiples of 16, so take the size
			uint size = (uint)Marshal.SizeOf<T>();
			float roundA = MathF.Round((float)size / 16f, MidpointRounding.ToPositiveInfinity);
			//and round it to the nearest multiple of 16.
			size = (uint)(roundA * 16);

			return size;
		}

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
					bufferVertex?.Dispose();
					bufferFragment?.Dispose();
					layout?.Dispose();
					set?.Dispose();
                }

				uniformsVertex = null;
				uniformsFragment = null;
				dirtyUniforms = null;
                disposedValue = true;
            }
        }

        ~ShaderUniformManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
