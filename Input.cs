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
	public static class Input
	{
		private static bool DEBUG = false;

		private static Sdl2Window window;
		public static InputSnapshot InputSnapshot;
		private static KeyboardState currentKeyboardState;
		private static KeyboardState previousKeyboardState;

		private static Vector2 mouseDelta;
		private static Vector2 mousePosition;
		private static Vector2 mousePositionAbsolute;
		private static Vector2 previousMousePosition;
		public static Vector2 MouseDelta => mouseDelta;
		public static Vector2 MousePosition => mousePosition;
		public static Vector2 MousePositionAbsolute => mousePositionAbsolute;

		private struct KeyboardState
		{
			public bool[] keys;
			public bool[] mouseButtons;
		}

		static Input()
		{
			currentKeyboardState = new KeyboardState();
			currentKeyboardState.keys = new bool[(int)Key.LastKey];
			currentKeyboardState.mouseButtons = new bool[(int)MouseButton.LastButton];

			previousKeyboardState = new KeyboardState();
			previousKeyboardState.keys = new bool[(int)Key.LastKey];
			previousKeyboardState.mouseButtons = new bool[(int)MouseButton.LastButton];
		}

		public static void Update(Sdl2Window window, InputSnapshot inputSnapshot, DeltaTime delta)
		{
			if (Input.window == null)
			{
				Input.window = window;
			}

			InputSnapshot = inputSnapshot;

			for (int i = 0; i < inputSnapshot.KeyEvents.Count; i++)
			{
				KeyEvent e = inputSnapshot.KeyEvents[i];
				if (e.Down)
					KeyDown(e);
				else KeyUp(e);
			}

			for (int i = 0; i < inputSnapshot.MouseEvents.Count; i++)
			{
				MouseEvent e = inputSnapshot.MouseEvents[i];
				if (e.Down)
					MouseDown(e);
				else MouseUp(e);
			}

			unsafe
			{
				int wx = 0;
				int wy = 0;

				Sdl2Native.SDL_GetWindowPosition(window.SdlWindowHandle, &wx, &wy);

				mousePositionAbsolute = inputSnapshot.MousePosition + new Vector2(wx, wy);
			}
			mousePosition = inputSnapshot.MousePosition;
			mouseDelta = window.MouseDelta;
		}

		public static void PreFrameUpdate()
		{
		}

		public static void PostFrameUpdate()
		{
			mouseDelta = Vector2.Zero;
			//mouseDelta = mousePosition - previousMousePosition;

			previousMousePosition = mousePosition;
			CopyKeyboardState(ref previousKeyboardState, ref currentKeyboardState);
		}

		private static void CopyKeyboardState(ref KeyboardState to, ref KeyboardState from)
		{
			for (int i = 0; i < (int)Key.LastKey; i++)
			{
				to.keys[i] = from.keys[i];
			}

			for (int i = 0; i < (int)MouseButton.LastButton; i++)
			{
				to.mouseButtons[i] = from.mouseButtons[i];
			}
		}

		private static void MouseDown(MouseEvent e)
		{
			currentKeyboardState.mouseButtons[(int)e.MouseButton] = true;

			if (DEBUG)
				Console.WriteLine("MOUSE DOWN");
		}

		private static void MouseUp(MouseEvent e)
		{
			currentKeyboardState.mouseButtons[(int)e.MouseButton] = false;
			if (DEBUG)
				Console.WriteLine("MOUSE UP");
		}

		private static void MouseMoved(MouseMoveEventArgs e)
		{
			mousePosition = e.MousePosition;
		}

		private static void KeyDown(KeyEvent e)
		{
			currentKeyboardState.keys[(int)e.Key] = true;

			if (DEBUG)
				Console.WriteLine("KEY {0} DOWN", e.Key.ToString());
		}

		private static void KeyUp(KeyEvent e)
		{
			currentKeyboardState.keys[(int)e.Key] = false;

			if (DEBUG)
				Console.WriteLine("KEY {0} UP", e.Key.ToString());
		}

		public static bool IsKeyDown(Key key)
		{
			return currentKeyboardState.keys[(int)key];
		}

		public static bool IsKeyUp(Key key)
		{
			return !currentKeyboardState.keys[(int)key];
		}

		public static bool IsMouseDown(MouseButton button)
		{
			return currentKeyboardState.mouseButtons[(int)button];
		}

		public static bool IsMouseUp(MouseButton button)
		{
			return !currentKeyboardState.mouseButtons[(int)button];
		}

		public static bool IsKeyJustPressed(Key key)
		{
			return currentKeyboardState.keys[(int)key] && !previousKeyboardState.keys[(int)key];
		}

		public static bool IsKeyJustReleased(Key key)
		{
			return !currentKeyboardState.keys[(int)key] && previousKeyboardState.keys[(int)key];
		}

		public static bool IsMouseJustPressed(MouseButton button)
		{
			return currentKeyboardState.mouseButtons[(int)button] && !previousKeyboardState.mouseButtons[(int)button];
		}

		public static bool IsMouseJustReleased(MouseButton button)
		{
			return !currentKeyboardState.mouseButtons[(int)button] && previousKeyboardState.mouseButtons[(int)button];
		}
	}
}
