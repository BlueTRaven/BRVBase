using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.ImageSharp;
using SixLabors.ImageSharp;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Numerics;

namespace BRVBase
{
	public static class Util
	{
		public static bool CheckLOS(World world, Vector2 startPosition, Vector2 endPosition)
		{
			Veldrid.Point start = new Veldrid.Point((int)startPosition.X / 16, (int)startPosition.Y / 16);// world.GetMap().GetLayer("collision").GetTile();
			Veldrid.Point end = new Veldrid.Point((int)endPosition.X / 16, (int)endPosition.Y / 16);//GetTile(world, endPosition);

			var path = BresenhamLine(start, end);

			foreach (Veldrid.Point pos in path)
			{
				if (!world.GetMap().Get().GetLayer("collision").IsValidTile(pos))
					continue;

				if (world.GetMap().Get().GetTile("collision", pos).valid)
					return false;
			}

			return true;
		}

		private static void Swap<T>(ref T a, ref T b)
		{
			T c = a;
			a = b;
			b = c;
		}

		private static List<Veldrid.Point> BresenhamLine(Veldrid.Point start, Veldrid.Point end)
		{
			// Optimization: it would be preferable to calculate in
			// advance the size of "result" and to use a fixed-size array
			// instead of a list.
			List<Veldrid.Point> result = new List<Veldrid.Point>();

			bool steep = Math.Abs(end.Y - start.Y) > Math.Abs(end.X - start.X);
			if (steep)
			{
				Swap(ref start.X, ref start.Y);
				Swap(ref end.X, ref end.Y);
			}
			if (start.X > end.X)
			{
				Swap(ref start.X, ref end.X);
				Swap(ref start.Y, ref end.Y);
			}

			int deltax = end.X - start.X;
			int deltay = Math.Abs(end.Y - start.Y);
			int error = 0;
			int ystep;
			int y = start.Y;
			if (start.Y < end.Y) ystep = 1; else ystep = -1;
			for (int x = start.X; x <= end.X; x++)
			{
				if (steep) result.Add(new Veldrid.Point(y, x));
				else result.Add(new Veldrid.Point(x, y));
				error += deltay;
				if (2 * error >= deltax)
				{
					y += ystep;
					error -= deltax;
				}
			}

			return result;
		}

		public static Type GetType(string name)
		{
			foreach (Assembly assemb in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assemb.GetTypes())
				{
					if (type.Name == name)
						return type;
				}
			}

			return null;
		}

		public static void WaitForFile(string path)
		{
			SpinWait spinWait = new SpinWait();

			while (true)
			{
				try
				{
					//stupid dumb way to check if a file is open. 
					using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
					{
						stream.Close();
					}

					break;
				}
				catch
				{
					spinWait.SpinOnce();
				}
			}
		}

		public static void DisposeShaders(Shader[] shaders)
		{
			foreach (Shader shader in shaders)
			{
				shader.Dispose();
			}
		}

		private static CommandList writeCommandList;
		public static void WriteTextureToPng(GraphicsDevice device, ResourceFactory factory, Texture texture, string output)
		{
			if (texture.Format == PixelFormat.R8_G8_B8_A8_UNorm)
			{

			}
			else if (texture.Format == PixelFormat.R8_UNorm)
			{

			}
			else
			{
				Console.WriteLine("Cannot write texture " + texture.Name + ". WriteTextureToPng only supports R8_G8_B8_A8_UNorm and R8_UNorm.");
				return;
			}

			Texture staging = factory.CreateTexture(TextureDescription.Texture2D(texture.Width, texture.Height, 
				texture.MipLevels, texture.ArrayLayers, texture.Format, TextureUsage.Staging));
			Fence fence = factory.CreateFence(false);

			if (writeCommandList == null)
				writeCommandList = factory.CreateCommandList();

			writeCommandList.Begin();

			writeCommandList.CopyTexture(texture, staging);

			writeCommandList.End();

			device.SubmitCommands(writeCommandList, fence);

			WaitForFence(fence);

			MappedResource mapped = device.Map(staging, MapMode.Read);

			byte[] bytes = new byte[mapped.SizeInBytes];

			Marshal.Copy(mapped.Data, bytes, 0, (int)mapped.SizeInBytes);

			var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder();
			Image image = null;
			if (texture.Format == PixelFormat.R8_G8_B8_A8_UNorm)
			{
				image = Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(bytes, (int)texture.Width, (int)texture.Height);
			}
			else if (texture.Format == PixelFormat.R8_UNorm)
			{
				image = Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.A8>(bytes, (int)texture.Width, (int)texture.Height);
			}
			
			image.Save(output, encoder);

			device.Unmap(staging);
			staging.Dispose();
			image.Dispose();
		}

		public static void WaitForFence(Fence fence)
		{
			SpinWait spinWait = new SpinWait();
			while (true)
			{
				if (fence.Signaled)
					break;

				spinWait.SpinOnce();
			}
		}

		public static RgbaFloat ColorFromInts(int r, int g, int b)
		{
			const float max = 255;
			return new RgbaFloat((float)r / max, (float)g / max, (float)b / max, 1);
		}

		public static T MultiLerp<T>(float t, Func<T, T, float, T> lerpFunc, params T[] values)
		{
			int c = values.Length - 1;  // number of transitions
			t = Math.Clamp(t, 0, 1) * c;   // expand t from 0-1 to 0-c
			int index = (int)Math.Clamp((float)Math.Floor(t), 0, c - 1); // get current index and clamp
			t -= index; // subract the index to get back a value of 0-1

			return lerpFunc.Invoke(values[index], values[index + 1], t);
		}

		public static float Lerp(float v1, float v2, float t)
		{
			return v1 + ((v2 - v1) * t);
		}

		public static RgbaFloat LerpColor(RgbaFloat first, RgbaFloat second, float t)
		{
			float r = Lerp(first.R, second.R, t);
			float g = Lerp(first.G, second.G, t);
			float b = Lerp(first.B, second.B, t);
			float a = Lerp(first.A, second.A, t);

			return new RgbaFloat(r, g, b, a);
		}
	}
}
