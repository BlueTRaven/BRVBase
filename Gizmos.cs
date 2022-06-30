using BRVBase.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace BRVBase
{
	public static class Gizmos
	{
		public struct GizmosResources : IShaderResourceGroup
        {
			public ShaderResourceManager ManagerWithViewProj;
			public ShaderResourceManager ManagerWithModel;
			public ShaderResourceManager ManagerWithTexture;
			public string TextureName;
			public ShaderResourceManager ManagerWithTint;
			public IList<ShaderResourceManager> AllManagers;

            public bool AreManagersDisposed()
            {
				return AllManagers.All(x => x.IsDisposed());
            }

            public IList<ShaderResourceManager> GetManagers()
            {
				return AllManagers;
            }
        }

		private struct Render
		{
			public Model Model;
			public Matrix4x4 Matrix;
			public RgbaFloat Color;

			public Render(Model model, Matrix4x4 matrix, RgbaFloat color)
			{
				Model = model;
				Matrix = matrix;
				Color = color;
			}
		}

		private static Dictionary<GizmosResources, ModelResources> cachedModelResources = new Dictionary<GizmosResources, ModelResources>();

		private static Model cylinder;
		private static Model cube;
		private static TextureAndSampler texture;
		private static bool init;

		private static CommandList commandList;
		private static Camera camera;
		private static Func<PipelineProgram> program;
		private static GizmosResources resources;
		private static bool started;

		private static List<Render> renders = new List<Render>();

		public static bool Dragging;
		private static Vector3 draggingStartProjection;
		private static Vector3 startPos;
		private static Vector3 startRot;
		private static Vector3 draggingAxis;
		private static float startMin;

		private static bool draggingPositionAxisX;
		private static bool draggingPositionAxisY;
		private static bool draggingPositionAxisZ;
		private static bool draggingPositionPlaneX;
		private static bool draggingPositionPlaneY;
		private static bool draggingPositionPlaneZ;

		private static bool draggingRotationAxisX;
		private static bool draggingRotationAxisY;
		private static bool draggingRotationAxisZ;

		private const float WORLD_SPACE_UNITS_TO_360_DEGREES = 16f;
		public static void Init(ResourceFactory factory)
		{
			InitModels();

			commandList = factory.CreateCommandList();
		}

		private static void InitModels()
		{
			init = true;
			cube = ServiceManager.Instance.GetService<AssetManager>().ModelObjLoader.GetHandle("PrimitiveCube").Get();
			cylinder = ServiceManager.Instance.GetService<AssetManager>().ModelObjLoader.GetHandle("PrimitiveCylinder").Get();

			texture = ServiceManager.Instance.GetService<AssetManager>().TextureLoader.GetHandle("whitepixel").Get();
		}

		public static void Start(Camera camera, Func<PipelineProgram> program, GizmosResources resources)
		{
			Gizmos.camera = camera;
			Gizmos.program = program;
            Gizmos.resources = resources;

			if (!cachedModelResources.ContainsKey(resources))
			{
				cachedModelResources.Add(resources, new ModelResources()
				{
					Program = program,
					AllManagers = resources.AllManagers,
					ManagerWithModel = resources.ManagerWithModel,
					ManagerWithTexture = resources.ManagerWithTexture,
					ManagerWithViewProj = resources.ManagerWithViewProj,
					TextureName = resources.TextureName
				});
			}
			else if (cachedModelResources[resources].Program().IsDisposed())
			{
				cachedModelResources.Remove(resources);
				cachedModelResources.Add(resources, new ModelResources()
				{
					Program = program,
					AllManagers = resources.AllManagers,
					ManagerWithModel = resources.ManagerWithModel,
					ManagerWithTexture = resources.ManagerWithTexture,
					ManagerWithViewProj = resources.ManagerWithViewProj,
					TextureName = resources.TextureName
				});
			}

            renders.Clear();

			started = true;
		}

		public static Vector3 RotationDraggable(Vector3 origin, Vector3 rotation)
		{
			const float width = 4f / 480f;
			const float planeSize = 0.8f;

			Matrix4x4 matAxisX = Matrix4x4.CreateScale(width, planeSize, planeSize) *
				Matrix4x4.CreateTranslation(origin);
			if (draggingRotationAxisX)
				matAxisX = Matrix4x4.CreateRotationX(rotation.X * (float)Constants.DEG_TO_RAD) * matAxisX;

			Matrix4x4 matAxisY = Matrix4x4.CreateScale(planeSize, width, planeSize) *
				Matrix4x4.CreateTranslation(origin);
			if (draggingRotationAxisY)
				matAxisY = Matrix4x4.CreateRotationY(rotation.Y * (float)Constants.DEG_TO_RAD) * matAxisY;

			Matrix4x4 matAxisZ = Matrix4x4.CreateScale(planeSize, planeSize, width) *
				Matrix4x4.CreateTranslation(origin);
			if (draggingRotationAxisZ)
				matAxisZ = Matrix4x4.CreateRotationZ(rotation.Z * (float)Constants.DEG_TO_RAD) * matAxisZ;

			Vector3 mouseWorldPos = camera.GetMouseProjection();
			Vector3 direction = mouseWorldPos - camera.Position;
			Vector3.Normalize(direction);

			(bool hitX, float minX, float maxX) = CollisionUtils.RaycastVsBoundingBox(camera.Position, direction, cube.BoundingBox, matAxisX);
			(bool hitY, float minY, float maxY) = CollisionUtils.RaycastVsBoundingBox(camera.Position, direction, cube.BoundingBox, matAxisY);
			(bool hitZ, float minZ, float maxZ) = CollisionUtils.RaycastVsBoundingBox(camera.Position, direction, cube.BoundingBox, matAxisZ);

			if (hitX && minX > minY && minX > minZ)
			{
				hitY = false;
				hitZ = false;
			}
			if (hitY && minY > minX && minY > minZ)
			{
				hitX = false;
				hitZ = false;
			}
			if (hitZ && minZ > minX && minZ > minY)
			{
				hitX = false;
				hitY = false;
			}

			Console.WriteLine("MIN - X: {0}, Y: {1}, Z: {2}", minX, minY, minZ);
			Console.WriteLine("MAX - X: {0}, Y: {1}, Z: {2}", maxX, maxY, maxZ);

			RotationDraggableAxis(hitX, origin, ref rotation, ref draggingRotationAxisX, new Vector3(1, 0, 0), new Vector3(0, 0, 1), direction, matAxisX);
			renders.Add(new Render(cube, matAxisX, hitX ? RgbaFloat.Orange : RgbaFloat.Red));

			Vector3 axis = new Vector3(0, 0, 1);
			float dotZ = Vector3.Dot(camera.GetLookDir(), axis);
			float dotX = Vector3.Dot(camera.GetLookDir(), new Vector3(1, 0, 0));
			if (dotX > dotZ)
				axis = new Vector3(1, 0, 0);

			RotationDraggableAxis(hitY, origin, ref rotation, ref draggingRotationAxisY, new Vector3(0, 1, 0), axis, direction, matAxisY);
			renders.Add(new Render(cube, matAxisY, hitY ? RgbaFloat.Cyan : RgbaFloat.Blue));
			RotationDraggableAxis(hitZ, origin, ref rotation, ref draggingRotationAxisZ, new Vector3(0, 0, 1), new Vector3(1, 0, 0), direction, matAxisZ);
			renders.Add(new Render(cube, matAxisZ, hitZ ? RgbaFloat.Grey : RgbaFloat.Green));

			return rotation;
		}

		private static bool RotationDraggableAxis(bool hit, Vector3 origin, ref Vector3 rotation, ref bool checker, Vector3 normal, Vector3 axis, Vector3 mouseDirection, Matrix4x4 matrix)
		{
			if (hit && Input.IsMouseDown(MouseButton.Left) && !Dragging)
			{
				Dragging = true;
				checker = true;

				draggingAxis = normal;

				IntersectPlane(-normal, origin, camera.Position, mouseDirection, out float t);
				draggingStartProjection = camera.Position + mouseDirection * t;

				float dot = Vector3.Dot(draggingStartProjection - origin, axis);

				draggingStartProjection = axis * dot + origin;

				startPos = rotation;

				//Sdl2Native.SDL_SetRelativeMouseMode(true);
				//Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, true);
			}

			if (Input.IsMouseUp(MouseButton.Left) && Dragging && checker)
			{
				Dragging = false;
				checker = false;

				//Sdl2Native.SDL_SetRelativeMouseMode(false);
				//Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);
			}

			if (Dragging && checker)
			{
				IntersectPlane(-normal, origin, camera.Position, mouseDirection, out float t);
				Vector3 projected = camera.Position + mouseDirection * t;

				float dot = Vector3.Dot(projected - origin, axis);

				projected = axis * dot + origin;

				renders.Add(new Render(cube, Matrix4x4.CreateTranslation(projected), RgbaFloat.Black));

				float dist = (projected * axis).Length() - (origin * axis).Length();
				float percent = dist / WORLD_SPACE_UNITS_TO_360_DEGREES * 360f;

				rotation = startPos + normal * percent;
				return true;
			}

			return hit;
		}

		public static Vector3 PositionDraggable(Vector3 origin)
		{
			if (!started)
				throw new Exception("Cannot draw gizmos while not started! Call Gizmos.Start first.");

			const float width = 4f / 480f;

			const float axisMargin = 0.25f;

			const float planeSize = 0.4f;
			const float planeMargin = 0.5f;

			Matrix4x4 matAxisX = Matrix4x4.CreateScale(1.5f, width, width) *
				Matrix4x4.CreateTranslation(1.5f + axisMargin, 0, 0) * Matrix4x4.CreateTranslation(origin);
			Matrix4x4 matPlaneX = Matrix4x4.CreateScale(width, planeSize, planeSize) *
				Matrix4x4.CreateTranslation(0, planeSize + planeMargin, planeSize + planeMargin) * Matrix4x4.CreateTranslation(origin);

			Matrix4x4 matAxisY = Matrix4x4.CreateScale(width, 1.5f, width) *
				Matrix4x4.CreateTranslation(0, 1.5f + axisMargin, 0) * Matrix4x4.CreateTranslation(origin);
			Matrix4x4 matPlaneY = Matrix4x4.CreateScale(planeSize, width, planeSize) *
				Matrix4x4.CreateTranslation(planeSize + planeMargin, 0, planeSize + planeMargin) * Matrix4x4.CreateTranslation(origin);

			Matrix4x4 matAxisZ = Matrix4x4.CreateScale(width, width, 1.5f) * 
				Matrix4x4.CreateTranslation(0, 0, 1.5f + axisMargin) * Matrix4x4.CreateTranslation(origin);
			Matrix4x4 matPlaneZ = Matrix4x4.CreateScale(planeSize, planeSize, width) *
				Matrix4x4.CreateTranslation(planeSize + planeMargin, planeSize + planeMargin, 0) * Matrix4x4.CreateTranslation(origin);

			Vector3 mouseWorldPos = camera.GetMouseProjection();
			Vector3 direction = mouseWorldPos - camera.Position;
			Vector3.Normalize(direction);

			bool hit = PositionDraggableAxis(ref origin, ref draggingPositionAxisX, new Vector3(1, 0, 0), direction, matAxisX);
			renders.Add(new Render(cube, matAxisX, hit ? RgbaFloat.Orange : RgbaFloat.Red));
			hit = PositionDraggablePlane(ref origin, ref draggingPositionPlaneX, new Vector3(1, 0, 0), direction, mouseWorldPos, matPlaneX);
			renders.Add(new Render(cube, matPlaneX, hit ? RgbaFloat.Orange : RgbaFloat.Red));

			hit = PositionDraggableAxis(ref origin, ref draggingPositionAxisY, new Vector3(0, 1, 0), direction, matAxisY);
			renders.Add(new Render(cube, matAxisY, hit ?  RgbaFloat.Cyan : RgbaFloat.Blue));
			hit = PositionDraggablePlane(ref origin, ref draggingPositionPlaneY, new Vector3(0, 1, 0), direction, mouseWorldPos, matPlaneY);
			renders.Add(new Render(cube, matPlaneY, hit ? RgbaFloat.Cyan : RgbaFloat.Blue));

			hit = PositionDraggableAxis(ref origin, ref draggingPositionAxisZ, new Vector3(0, 0, 1), direction, matAxisZ);
			renders.Add(new Render(cube, matAxisZ, hit ? RgbaFloat.Grey : RgbaFloat.Green));
			hit = PositionDraggablePlane(ref origin, ref draggingPositionPlaneZ, new Vector3(0, 0, 1), direction, mouseWorldPos, matPlaneZ);
			renders.Add(new Render(cube, matPlaneZ, hit ? RgbaFloat.Grey : RgbaFloat.Green));

			return origin;
		}

		private static bool PositionDraggableAxis(ref Vector3 origin, ref bool checker, Vector3 draggableAxis, Vector3 mouseDirection, Matrix4x4 matrix)
		{
			(bool hit, float min, float max) = CollisionUtils.RaycastVsBoundingBox(camera.Position, mouseDirection, cube.BoundingBox, matrix);

			if (hit && Input.IsMouseDown(MouseButton.Left) && !Dragging)
			{
				Dragging = true;

				draggingAxis = draggableAxis;
				checker = true;

				Vector3 mouseRel = (mouseDirection * min) + camera.Position - origin;
				startMin = min;

				float dot = Vector3.Dot(mouseRel, draggingAxis);

				draggingStartProjection = draggingAxis * dot + origin;
				startPos = origin;

				Sdl2Native.SDL_SetRelativeMouseMode(true);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, true);
			}

			if (Input.IsMouseUp(MouseButton.Left) && Dragging && checker)
			{
				Dragging = false;
				checker = false;

				Sdl2Native.SDL_SetRelativeMouseMode(false);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);
			}

			if (Dragging && checker)
			{
				if (hit)
					startMin = min;

				Vector3 mouseRel = (mouseDirection * startMin) + camera.Position - origin;

				float dot = Vector3.Dot(mouseRel, draggableAxis);

				Vector3 projected = draggableAxis * dot + origin;

				renders.Add(new Render(cube, Matrix4x4.CreateScale(0.1f) * Matrix4x4.CreateTranslation(projected), RgbaFloat.Orange));

				origin = startPos + (projected - draggingStartProjection);
				return true;
			}

			return hit;
		}

		private static bool PositionDraggablePlane(ref Vector3 origin, ref bool checker, Vector3 normal, Vector3 mouseDirection, Vector3 mousePosition, Matrix4x4 matrix)
		{
			(bool hit, float min, float max) = CollisionUtils.RaycastVsBoundingBox(camera.Position, mouseDirection, cube.BoundingBox, matrix);

			if (hit && Input.IsMouseDown(MouseButton.Left) && !Dragging)
			{
				Dragging = true;
				checker = true;

				draggingAxis = normal;

				IntersectPlane(-normal, origin, camera.Position, mouseDirection, out float t);
				Vector3 projected = camera.Position + mouseDirection * t;

				draggingStartProjection = projected;
				startPos = origin;

				Sdl2Native.SDL_SetRelativeMouseMode(true);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, true);
			}

			if (Input.IsMouseUp(MouseButton.Left) && Dragging && checker)
			{
				Dragging = false;
				checker = false;

				Sdl2Native.SDL_SetRelativeMouseMode(false);
				Sdl2Native.SDL_SetWindowGrab(Runner.Window.SdlWindowHandle, false);
			}

			if (Dragging && checker)
			{
				IntersectPlane(-normal, origin, camera.Position, mouseDirection, out float t);
				Vector3 projected = camera.Position + mouseDirection * t;

				origin = startPos + (projected - draggingStartProjection);
				return true;
			}

			return hit;
		}

		private static bool IntersectPlane(Vector3 n, Vector3 p0, Vector3 l0, Vector3 l, out float t) 
		{ 
			// assuming vectors are all normalized
			float denom = Vector3.Dot(n, l); 
			if (denom > 1e-6) { 
				Vector3 p0l0 = p0 - l0;
				t = Vector3.Dot(p0l0, n) / denom; 
				return (t >= 0); 
			}

			t = -1;
			return false; 
		}

		public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
		{
			float distance;
			Vector3 translationVector;

			distance = Vector3.Dot(planeNormal, (point - planePoint));

			distance *= -1;

			translationVector = Vector3.Normalize(planeNormal) * distance;

			return point + translationVector;
		}

		public static void Draw(GraphicsDevice device)
		{
			commandList.Begin();

			RgbaFloat oldColor = RgbaFloat.White;

			foreach (Render render in renders)
			{
				if (render.Color != oldColor)
				{
					resources.ManagerWithTint.Set("Tint", render.Color, ShaderStages.Vertex, commandList);
					//program.GetShader().SetTint(commandList, render.Color);
					oldColor = render.Color;
				}
				Model.Draw(render.Model, commandList, camera, render.Matrix, texture, cachedModelResources[resources]);
				//Model.Draw(render.Model, commandList, camera, program, texture, render.Matrix);
			}

			resources.ManagerWithTint.Set("Tint", RgbaFloat.White, ShaderStages.Vertex, commandList);
			//program.GetShader().SetTint(commandList, RgbaFloat.White);

			commandList.End();
			device.SubmitCommands(commandList);
		}
	}
}
