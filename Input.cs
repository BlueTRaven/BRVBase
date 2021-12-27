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
	public class Input
	{
		private readonly Sdl2Window window;
		private KeyboardState currentKeyboardState;
		private KeyboardState previousKeyboardState;

		private Vector2 mouseDelta;
		private Vector2 mousePosition;
		public Vector2 MousePosition => mousePosition;

		public struct KeyboardState
		{
			public bool[] keys;
			public bool[] mouseButtons;
		}

		public Input(Sdl2Window window)
		{
			this.window = window;

			window.KeyDown += KeyDown;
			window.KeyUp += KeyUp;

			window.MouseDown += MousePress;
			window.MouseUp += MousePress;
			window.MouseMove += MouseMoved;

			currentKeyboardState = new KeyboardState();
			currentKeyboardState.keys = new bool[(int)Key.LastKey];
			currentKeyboardState.mouseButtons = new bool[(int)MouseButton.LastButton];

			previousKeyboardState = new KeyboardState();
			previousKeyboardState.keys = new bool[(int)Key.LastKey];
			previousKeyboardState.mouseButtons = new bool[(int)MouseButton.LastButton];
		}

		public void Update()
		{
			CopyKeyboardState(ref previousKeyboardState, ref currentKeyboardState);
		}

		public KeyboardState GetState()
		{
			return currentKeyboardState;
		}

		private void CopyKeyboardState(ref KeyboardState to, ref KeyboardState from)
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

		private void MousePress(MouseEvent e)
		{
			currentKeyboardState.mouseButtons[(int)e.MouseButton] = e.Down;
		}

		private void MouseMoved(MouseMoveEventArgs e)
		{
			mousePosition = e.MousePosition;
		}

		private void KeyDown(KeyEvent e)
		{
			currentKeyboardState.keys[(int)e.Key] = true;
		}

		private void KeyUp(KeyEvent e)
		{
			currentKeyboardState.keys[(int)e.Key] = false;
		}

		public bool IsKeyDown(Key key)
		{
			return currentKeyboardState.keys[(int)key];
		}

		public bool IsKeyUp(Key key)
		{
			return !currentKeyboardState.keys[(int)key];
		}

		public bool IsMouseDown(MouseButton button)
		{
			return currentKeyboardState.mouseButtons[(int)button];
		}

		public bool IsMouseUp(MouseButton button)
		{
			return !currentKeyboardState.mouseButtons[(int)button];
		}

		public bool IsKeyJustPressed(Key key)
		{
			return currentKeyboardState.keys[(int)key] && !previousKeyboardState.keys[(int)key];
		}

		public bool IsKeyJustReleased(Key key)
		{
			return !currentKeyboardState.keys[(int)key] && previousKeyboardState.keys[(int)key];
		}

		public bool IsMouseJustPressed(MouseButton button)
		{
			return currentKeyboardState.mouseButtons[(int)button] && !previousKeyboardState.mouseButtons[(int)button];
		}

		public bool IsMouseJustReleased(MouseButton button)
		{
			return !currentKeyboardState.mouseButtons[(int)button] && previousKeyboardState.mouseButtons[(int)button];
		}
	}
}
