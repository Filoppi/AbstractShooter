using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using UnrealMono;

namespace InputManagement
{
	public delegate void KeyEventHandler();

	public delegate void AxisEventHandler(float axisValue);

	public struct InputMode
	{
		public readonly bool Keyboard;
		public readonly bool Mouse;
		public readonly bool[] GamePads;

		public InputMode(bool Keyboard = true, bool Mouse = true, int GamePadsNumber = 1, bool GamePadsNumberIsIndex = false)
		{
			this.Keyboard = Keyboard;
			this.Mouse = Mouse;
			GamePads = new bool[GamePad.MaximumGamePadCount];
			if (GamePadsNumberIsIndex)
			{
				GamePads[MathHelper.Clamp(GamePadsNumber, 0, GamePad.MaximumGamePadCount)] = true;
			}
			else
			{
				for (int i = 0; i < Math.Min(GamePadsNumber, GamePad.MaximumGamePadCount); ++i)
				{
					GamePads[i] = true;
				}
			}
		}
	}

	public static class InputModes
	{
		public static readonly InputMode KeyboardMouseGamePad1 = new InputMode(true, true, 0, true);
		public static readonly InputMode KeyboardMouse = new InputMode(true, true, 0);
		public static readonly InputMode GamePad1 = new InputMode(false, false, 0, true);
		public static readonly InputMode GamePad2 = new InputMode(false, false, 1, true);
		public static readonly InputMode GamePad3 = new InputMode(false, false, 2, true);
		public static readonly InputMode GamePad4 = new InputMode(false, false, 3, true);
	}

	public enum MouseButtons
	{
		Left,
		Middle,
		Right
	};

	public enum GamePadAxis
	{
		LeftAnalogX,
		LeftAnalogY,
		RightAnalogX,
		RightAnalogY,
		LeftTrigger,
		RightTrigger
	};

	public enum KeyAction
	{
		Pressed, //Has been pressed in the last frame
		Released, //Has been released in the last frame
		Down, //Is currently down
		Up, //Is currently up
		Toggled //Has been toggled in the last frame
	};

	public struct KeyBinding<T>
	{
		public readonly KeyAction action;
		public readonly T key; //Keys, Buttons or MouseButtons

		public KeyBinding(T key, KeyAction action = KeyAction.Pressed)
		{
			this.key = key;
			this.action = action;
		}
	}

	public class ActionBindingWithEvent
	{
		public event KeyEventHandler actionBidingEvent = null;

		public readonly ActionBinding actionBinding;

		public ActionBindingWithEvent(ActionBinding actionBinding)
		{
			this.actionBinding = actionBinding;
		}

		public void BroadcastBidingChanged()
		{
			if (actionBidingEvent != null)
			{
				actionBidingEvent();
			}
		}
	}

	public class ActionBinding
	{
		public readonly KeyBinding<Keys>[] keyboardKeyBindings;
		public readonly KeyBinding<Buttons>[] gamePadButtonsBindings;
		public readonly KeyBinding<MouseButtons>[] mouseButtonsBindings;

		public ActionBinding(KeyBinding<Keys>[] keyboardKeyBindings = null, KeyBinding<Buttons>[] gamePadButtonsBindings = null, KeyBinding<MouseButtons>[] mouseButtonsBindings = null)
		{
			if (keyboardKeyBindings == null)
			{
				keyboardKeyBindings = new KeyBinding<Keys>[0];
			}
			if (gamePadButtonsBindings == null)
			{
				gamePadButtonsBindings = new KeyBinding<Buttons>[0];
			}
			if (mouseButtonsBindings == null)
			{
				mouseButtonsBindings = new KeyBinding<MouseButtons>[0];
			}
			this.keyboardKeyBindings = keyboardKeyBindings;
			this.gamePadButtonsBindings = gamePadButtonsBindings;
			this.mouseButtonsBindings = mouseButtonsBindings;
		}
	}

	public struct KeyAxisBinding<T>
	{
		public readonly float multiplier;
		public readonly T key; //Keys, Buttons or MouseButtons

		public KeyAxisBinding(T key, float multiplier = 1F)
		{
			this.key = key;
			this.multiplier = multiplier;
		}
	}

	public class AxisBindingWithEvent
	{
		public event AxisEventHandler axisBindingEvent = null;

		public readonly AxisBinding axisBinding;

		public AxisBindingWithEvent(AxisBinding axisBinding)
		{
			this.axisBinding = axisBinding;
		}

		public void BroadcastBidingChanged(float value)
		{
			if (axisBindingEvent != null)
			{
				axisBindingEvent(value);
			}
		}
	}

	public class AxisBinding
	{
		//GamePadAxis can be between -1 and 1 or 0 and 1
		public readonly KeyAxisBinding<GamePadAxis>[] gamePadAxes;

		//Buttons are 0 if unpressed, 1 if pressed
		public readonly KeyAxisBinding<Keys>[] keyboardKeyAxisBindings;

		public readonly KeyAxisBinding<Buttons>[] gamePadButtonsAxisBindings;
		public readonly KeyAxisBinding<MouseButtons>[] mouseButtonsAxisBindings;

		public AxisBinding(KeyAxisBinding<GamePadAxis>[] gamePadAxes = null, KeyAxisBinding<Keys>[] keyboardKeyAxisBindings = null, KeyAxisBinding<Buttons>[] gamePadButtonsAxisBindings = null, KeyAxisBinding<MouseButtons>[] mouseButtonsAxisBindings = null)
		{
			if (gamePadAxes == null)
			{
				gamePadAxes = new KeyAxisBinding<GamePadAxis>[0];
			}
			if (keyboardKeyAxisBindings == null)
			{
				keyboardKeyAxisBindings = new KeyAxisBinding<Keys>[0];
			}
			if (gamePadButtonsAxisBindings == null)
			{
				gamePadButtonsAxisBindings = new KeyAxisBinding<Buttons>[0];
			}
			if (mouseButtonsAxisBindings == null)
			{
				mouseButtonsAxisBindings = new KeyAxisBinding<MouseButtons>[0];
			}
			this.gamePadAxes = gamePadAxes;
			this.keyboardKeyAxisBindings = keyboardKeyAxisBindings;
			this.gamePadButtonsAxisBindings = gamePadButtonsAxisBindings;
			this.mouseButtonsAxisBindings = mouseButtonsAxisBindings;
		}
	}

	public static class InputManager
	{
		public static KeyboardState currentKeyboardState;
		public static KeyboardState previousKeyboardState;
		public static GamePadState[] currentGamePadState = new GamePadState[GamePad.MaximumGamePadCount];
		public static GamePadState[] previousGamePadState = new GamePadState[GamePad.MaximumGamePadCount];
		public static MouseState currentMouseState;
		public static MouseState previousMouseState;
		private static int numberOfGamePads;
		public static int NumberOfGamePads { get { return numberOfGamePads; } }
		private static bool isUsingGamePad;
		public static bool IsUsingGamePad { get { return isUsingGamePad; } }
		private static bool hideMouseWhenNotUsed;
		public static bool HideMouseWhenNotUsed { set { hideMouseWhenNotUsed = value; } }
		private static bool captureMouse;
		public static bool CaptureMouse { set { captureMouse = value; } }
		private static List<ActionBindingWithEvent> actionBindingsWithEvent = new List<ActionBindingWithEvent>();
		private static List<AxisBindingWithEvent> axisBindingsWithEvent = new List<AxisBindingWithEvent>();

		public static void AddActionBinding(ActionBindingWithEvent actionBindingWithEvent)
		{
			actionBindingsWithEvent.Add(actionBindingWithEvent);
		}

		public static void RemoveActionBinding(ActionBindingWithEvent actionBindingWithEvent)
		{
			actionBindingsWithEvent.Remove(actionBindingWithEvent);
		}

		public static void ClearActionBindings()
		{
			actionBindingsWithEvent.Clear();
		}

		public static void AddAxisBinding(AxisBindingWithEvent axisBindingWithEvent)
		{
			axisBindingsWithEvent.Add(axisBindingWithEvent);
		}

		public static void RemoveAxisBinding(AxisBindingWithEvent axisBindingWithEvent)
		{
			axisBindingsWithEvent.Remove(axisBindingWithEvent);
		}

		public static void ClearAxisBindings()
		{
			axisBindingsWithEvent.Clear();
		}

		public static void ClearInputBindings()
		{
			actionBindingsWithEvent.Clear();
			axisBindingsWithEvent.Clear();
		}

		public static void Update()
		{
			previousKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();

			previousMouseState = currentMouseState;
			currentMouseState = Mouse.GetState();
			//currentMouseState = new MouseState((((float)currentMouseState.X / UnrealMonoGame.form.Size.Width) * (float)UnrealMonoGame.graphicsDeviceManager.PreferredBackBufferWidth).Round(), (((float)currentMouseState.Y / UnrealMonoGame.form.Size.Height) * (float)UnrealMonoGame.graphicsDeviceManager.PreferredBackBufferHeight).Round(),
			//    currentMouseState.ScrollWheelValue, currentMouseState.LeftButton, currentMouseState.MiddleButton, currentMouseState.RightButton,
			//    currentMouseState.XButton1, currentMouseState.XButton2);

			if (captureMouse)
			{
				Cursor cursor = new Cursor(Cursor.Current.Handle);
				//Cursor.Position = new Point(Cursor.Position.X - 50, Cursor.Position.Y - 50);
				Cursor.Clip = new System.Drawing.Rectangle(0, 0, 0, 0);
				//Cursor.Clip = System.Drawing.Rectangle.Empty;
				//Mouse.SetPosition(currentMouseState.X.GetClamped(0, UnrealMonoGame.Get.GraphicsDevice.Viewport.Width), currentMouseState.Y.GetClamped(0, UnrealMonoGame.Get.GraphicsDevice.Viewport.Height));
			}

			int foundGamePads = 0;
			//previousGamePadState = currentGamePadState;
			for (int i = 0; i < currentGamePadState.Length; ++i)
			{
				previousGamePadState[i] = currentGamePadState[i];
				currentGamePadState[i] = GamePad.GetState(i, GamePadDeadZone.Circular);
				if (currentGamePadState[i].IsConnected)
				{
					foundGamePads++;
				}
			}
			if (numberOfGamePads != foundGamePads)
			{
				//Broadcast Connected GamePads Number Changed Event
				numberOfGamePads = foundGamePads;
			}

			if (isUsingGamePad)
			{
				if (!HasGamePadStateChanged(0) && (HasKeyboardStateChanged() || (HasMouseStateChanged() && !UnrealMonoGame.PendingResolutionChange)))
				{
					//Broadcast Using GamePad value Event
					isUsingGamePad = false;

					if (hideMouseWhenNotUsed) //&& ! gamepad and mouse are two different players
					{
						UnrealMonoGame.Get.IsMouseVisible = true;
					}
				}
			}
			else if (HasGamePadStateChanged(0) && !HasKeyboardStateChanged() && !HasMouseStateChanged())
			{
				//Broadcast Using GamePad value Event
				isUsingGamePad = true;
				if (hideMouseWhenNotUsed) //&& ! gamepad and mouse are two different players
				{
					UnrealMonoGame.Get.IsMouseVisible = false;
				}
			}

			foreach (ActionBindingWithEvent actionBindingWithEvent in actionBindingsWithEvent)
			{
				if (actionBindingWithEvent.actionBinding.CheckBindings())
				{
					actionBindingWithEvent.BroadcastBidingChanged();
				}
			}
			foreach (AxisBindingWithEvent axisBindingWithEvent in axisBindingsWithEvent)
			{
				axisBindingWithEvent.BroadcastBidingChanged(axisBindingWithEvent.axisBinding.CheckBindings());
			}
		}

		public static bool WasKeyboardKeyToggled(Keys key)
		{
			return currentKeyboardState.IsKeyDown(key) != previousKeyboardState.IsKeyDown(key);
		}

		public static bool WasKeyboardKeyPressed(Keys key)
		{
			return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
		}

		public static bool WasKeyboardKeyReleased(Keys key)
		{
			return currentKeyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key);
		}

		public static bool WasGamePadButtonToggled(Buttons button, int playerIndex)
		{
			return currentGamePadState[playerIndex].IsButtonDown(button) != previousGamePadState[playerIndex].IsButtonDown(button);
		}

		public static bool WasGamePadButtonPressed(Buttons button, int playerIndex)
		{
			return currentGamePadState[playerIndex].IsButtonDown(button) && previousGamePadState[playerIndex].IsButtonUp(button);
		}

		public static bool WasGamePadButtonReleased(Buttons button, int playerIndex)
		{
			return currentGamePadState[playerIndex].IsButtonUp(button) && previousGamePadState[playerIndex].IsButtonDown(button);
		}

		public static bool WasMouseButtonToggled(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					{
						return (currentMouseState.LeftButton == ButtonState.Pressed) != (previousMouseState.LeftButton == ButtonState.Pressed);
					}
				case MouseButtons.Middle:
					{
						return (currentMouseState.MiddleButton == ButtonState.Pressed) != (previousMouseState.MiddleButton == ButtonState.Pressed);
					}
				case MouseButtons.Right:
					{
						return (currentMouseState.RightButton == ButtonState.Pressed) != (previousMouseState.RightButton == ButtonState.Pressed);
					}
			}
			return false;
		}

		public static bool WasMouseButtonPressed(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					{
						return (currentMouseState.LeftButton == ButtonState.Pressed) && (previousMouseState.LeftButton == ButtonState.Released);
					}
				case MouseButtons.Middle:
					{
						return (currentMouseState.MiddleButton == ButtonState.Pressed) && (previousMouseState.MiddleButton == ButtonState.Released);
					}
				case MouseButtons.Right:
					{
						return (currentMouseState.RightButton == ButtonState.Pressed) && (previousMouseState.RightButton == ButtonState.Released);
					}
			}
			return false;
		}

		public static bool WasMouseButtonReleased(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					{
						return (currentMouseState.LeftButton == ButtonState.Released) && (previousMouseState.LeftButton == ButtonState.Pressed);
					}
				case MouseButtons.Middle:
					{
						return (currentMouseState.MiddleButton == ButtonState.Released) && (previousMouseState.MiddleButton == ButtonState.Pressed);
					}
				case MouseButtons.Right:
					{
						return (currentMouseState.RightButton == ButtonState.Released) && (previousMouseState.RightButton == ButtonState.Pressed);
					}
			}
			return false;
		}

		public static bool IsMouseButtonDown(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					{
						return currentMouseState.LeftButton == ButtonState.Pressed;
					}
				case MouseButtons.Middle:
					{
						return currentMouseState.MiddleButton == ButtonState.Pressed;
					}
				case MouseButtons.Right:
					{
						return currentMouseState.RightButton == ButtonState.Pressed;
					}
			}
			return false;
		}

		public static bool IsMouseButtonUp(MouseButtons button)
		{
			switch (button)
			{
				case MouseButtons.Left:
					{
						return currentMouseState.LeftButton == ButtonState.Released;
					}
				case MouseButtons.Middle:
					{
						return currentMouseState.MiddleButton == ButtonState.Released;
					}
				case MouseButtons.Right:
					{
						return currentMouseState.RightButton == ButtonState.Released;
					}
			}
			return false;
		}

		public static float GetGamePadAxisValue(GamePadAxis gamePadAxes, int playerIndex)
		{
			switch (gamePadAxes)
			{
				case GamePadAxis.LeftAnalogX:
					{
						return currentGamePadState[playerIndex].ThumbSticks.Left.X;
					}
				case GamePadAxis.LeftAnalogY:
					{
						return currentGamePadState[playerIndex].ThumbSticks.Left.Y;
					}
				case GamePadAxis.RightAnalogX:
					{
						return currentGamePadState[playerIndex].ThumbSticks.Right.X;
					}
				case GamePadAxis.RightAnalogY:
					{
						return currentGamePadState[playerIndex].ThumbSticks.Right.Y;
					}
				case GamePadAxis.LeftTrigger:
					{
						return currentGamePadState[playerIndex].Triggers.Left;
					}
				case GamePadAxis.RightTrigger:
					{
						return currentGamePadState[playerIndex].Triggers.Right;
					}
			}
			return 0;
		}

		private static bool HasKeyboardStateChanged()
		{
			return currentKeyboardState != previousKeyboardState;
		}

		private static bool HasMouseStateChanged()
		{
			return currentMouseState != previousMouseState;
		}

		private static bool HasGamePadStateChanged(int playerIndex)
		{
			return currentGamePadState[playerIndex].Buttons != previousGamePadState[playerIndex].Buttons
				|| currentGamePadState[playerIndex].ThumbSticks != previousGamePadState[playerIndex].ThumbSticks
				|| currentGamePadState[playerIndex].Triggers != previousGamePadState[playerIndex].Triggers;
		}

		public static bool CheckBindings(this ActionBinding actionBinding, InputMode? inputMode = null)
		{
			InputMode newInputMode;
			if (inputMode.HasValue)
			{
				newInputMode = inputMode.Value;
			}
			else
			{
				newInputMode = InputModes.KeyboardMouseGamePad1;
			}
			if (newInputMode.Keyboard)
			{
				foreach (KeyBinding<Keys> keyBinding in actionBinding.keyboardKeyBindings)
				{
					switch (keyBinding.action)
					{
						case KeyAction.Pressed:
							{
								if (WasKeyboardKeyPressed(keyBinding.key))
									return true;
								break;
							}
						case KeyAction.Released:
							{
								if (WasKeyboardKeyPressed(keyBinding.key))
									return true;
								break;
							}
						case KeyAction.Toggled:
							{
								if (WasKeyboardKeyToggled(keyBinding.key))
									return true;
								break;
							}
						case KeyAction.Down:
							{
								if (currentKeyboardState.IsKeyDown(keyBinding.key))
									return true;
								break;
							}
						case KeyAction.Up:
							{
								if (currentKeyboardState.IsKeyUp(keyBinding.key))
									return true;
								break;
							}
					}
				}
			}
			if (newInputMode.Mouse)
			{
				foreach (KeyBinding<MouseButtons> mouseButtonsBinding in actionBinding.mouseButtonsBindings)
				{
					switch (mouseButtonsBinding.action)
					{
						case KeyAction.Pressed:
							{
								if (WasMouseButtonPressed(mouseButtonsBinding.key))
									return true;
								break;
							}
						case KeyAction.Released:
							{
								if (WasMouseButtonReleased(mouseButtonsBinding.key))
									return true;
								break;
							}
						case KeyAction.Toggled:
							{
								if (WasMouseButtonToggled(mouseButtonsBinding.key))
									return true;
								break;
							}
						case KeyAction.Down:
							{
								if (IsMouseButtonDown(mouseButtonsBinding.key))
									return true;
								break;
							}
						case KeyAction.Up:
							{
								if (IsMouseButtonUp(mouseButtonsBinding.key))
									return true;
								break;
							}
					}
				}
			}
			for (int i = 0; i < newInputMode.GamePads.Length; ++i)
			{
				if (newInputMode.GamePads[i])
				{
					foreach (KeyBinding<Buttons> gamePadButtonsBinding in actionBinding.gamePadButtonsBindings)
					{
						switch (gamePadButtonsBinding.action)
						{
							case KeyAction.Pressed:
								{
									if (WasGamePadButtonPressed(gamePadButtonsBinding.key, i))
										return true;
									break;
								}
							case KeyAction.Released:
								{
									if (WasGamePadButtonReleased(gamePadButtonsBinding.key, i))
										return true;
									break;
								}
							case KeyAction.Toggled:
								{
									if (WasGamePadButtonToggled(gamePadButtonsBinding.key, i))
										return true;
									break;
								}
							case KeyAction.Down:
								{
									if (currentGamePadState[i].IsButtonDown(gamePadButtonsBinding.key))
										return true;
									break;
								}
							case KeyAction.Up:
								{
									if (currentGamePadState[i].IsButtonUp(gamePadButtonsBinding.key))
										return true;
									break;
								}
						}
					}
				}
			}
			return false;
		}

		public static float CheckBindings(this AxisBinding axisBinding, InputMode? inputMode = null)
		{
			InputMode newInputMode;
			if (inputMode.HasValue)
			{
				newInputMode = inputMode.Value;
			}
			else
			{
				newInputMode = InputModes.KeyboardMouseGamePad1;
			}
			float value = 0F;
			if (newInputMode.Keyboard)
			{
				foreach (KeyAxisBinding<Keys> keyboardKeyAxisBinding in axisBinding.keyboardKeyAxisBindings)
				{
					if (currentKeyboardState.IsKeyDown(keyboardKeyAxisBinding.key))
						value += keyboardKeyAxisBinding.multiplier;
				}
			}
			if (newInputMode.Mouse)
			{
				foreach (KeyAxisBinding<MouseButtons> mouseButtonsAxisBinding in axisBinding.mouseButtonsAxisBindings)
				{
					if (IsMouseButtonDown(mouseButtonsAxisBinding.key))
						value += mouseButtonsAxisBinding.multiplier;
				}
			}
			for (int i = 0; i < newInputMode.GamePads.Length; ++i)
			{
				if (newInputMode.GamePads[i])
				{
					foreach (KeyAxisBinding<GamePadAxis> gamePadAxis in axisBinding.gamePadAxes)
					{
						value += GetGamePadAxisValue(gamePadAxis.key, i) * gamePadAxis.multiplier;
					}
					foreach (KeyAxisBinding<Buttons> gamePadButtonsAxisBindings in axisBinding.gamePadButtonsAxisBindings)
					{
						if (currentGamePadState[i].IsButtonDown(gamePadButtonsAxisBindings.key))
							value += gamePadButtonsAxisBindings.multiplier;
					}
				}
			}
			return MathHelper.Clamp(value, -1F, 1F);
		}
	}
}