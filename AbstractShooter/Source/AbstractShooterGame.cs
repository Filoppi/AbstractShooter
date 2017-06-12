using AbstractShooter.States;
using BloomPostprocess;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UnrealMono;

namespace AbstractShooter
{
	public struct AbstractShooterSave
	{
		public int HiScore;
	}
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class AbstractShooterGame : UnrealMonoGame
	{
		public override Type GetInitialState()
		{
			return typeof(MainMenuState);
		}

		protected override void Initialize()
		{
			base.Initialize();

			AbstractShooterSave abstractShooterSave = new AbstractShooterSave();
			abstractShooterSave = (AbstractShooterSave)SaveManager.Load(abstractShooterSave);
			abstractShooterSave.HiScore = GameInstance.HiScore;
		}

		protected override void OnExit()
		{
			AbstractShooterSave abstractShooterSave;
			abstractShooterSave.HiScore = GameInstance.HiScore;
			SaveManager.Save(abstractShooterSave);

			base.OnExit();
		}

#if DEBUG
		protected override void ExecuteDebugOptions(GameTime gameTime)
		{
            //This methods switches between debug types, it might not make sense becayse they are mutually exclusive
            List<Keys> pressedKeys = InputManager.currentKeyboardState.GetPressedKeys().ToList();
            foreach (Keys key in new List<Keys> { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 })
            {
                if (pressedKeys.IndexOf(key) >= 0 && InputManager.previousKeyboardState.IsKeyUp(key))
                {
                    int exponent = (int)(key - Keys.D0);
                    debugType = (DebugType)(int)Math.Pow(2, exponent - 1);
                    AddDebugString("Debug Type: " + debugType.ToString(), 3.37F);
                    break;
                }
            }
            /*if (InputManager.currentKeyboardState.IsKeyDown(Keys.Tab) && InputManager.previousKeyboardState.IsKeyUp(Keys.Tab))
			{
				if (debugType.HasFlag(Enum.GetValues(typeof(DebugType)).Cast<DebugType>().Max()))
				{
					debugType = new DebugType();
				}
				else
				{
					int debugTypeInt = (int)debugType;
					if (debugTypeInt != 0)
					{
						int exponent = (int)Math.Round(Math.Log(debugTypeInt) / Math.Log(2));
						debugTypeInt = (int)Math.Pow(2, exponent + 1);
					}
					else
					{
						debugTypeInt = 1;
					}
					debugType = (DebugType)debugTypeInt;
				}
				AddDebugString("Debug Type: " + debugType.ToString(), 3.37F);
			}*/
			if (debugType.HasFlag(DebugType.Window))
			{
				if (InputManager.currentKeyboardState.IsKeyDown(Keys.F) && InputManager.previousKeyboardState.IsKeyUp(Keys.F))
				{
					SetScreenState(WindowState.FullScreen);
					SaveManager.SaveEngineSettings();
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.B) && InputManager.previousKeyboardState.IsKeyUp(Keys.B))
				{
					SetScreenState(WindowState.BorderlessFullScreen);
					SaveManager.SaveEngineSettings();
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.M) && InputManager.previousKeyboardState.IsKeyUp(Keys.M))
				{
					SetScreenState(WindowState.Windowed);
					SaveManager.SaveEngineSettings();
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.V) && InputManager.previousKeyboardState.IsKeyUp(Keys.V))
				{
					SoundsManager.Mute = !SoundsManager.Mute;
				}
			}
            if (debugType.HasFlag(DebugType.TimeScale))
			{
				if (InputManager.currentKeyboardState.IsKeyDown(Keys.T) && InputManager.previousKeyboardState.IsKeyUp(Keys.T))
				{
					if (StateManager.currentState.TimeScale != 2.5F)
						StateManager.currentState.TimeScale = 2.5F;
					else
						StateManager.currentState.TimeScale = 1F;
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.Y) && InputManager.previousKeyboardState.IsKeyUp(Keys.Y))
				{
					if (StateManager.currentState.TimeScale != 0.25F)
						StateManager.currentState.TimeScale = 0.25F;
					else
						StateManager.currentState.TimeScale = 1F;
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.U) && InputManager.previousKeyboardState.IsKeyUp(Keys.U))
				{
					if (StateManager.currentState.TimeScale != 0F)
						StateManager.currentState.TimeScale = 0F;
					else
						StateManager.currentState.TimeScale = 1F;
				}
			}
			if (debugType.HasFlag(DebugType.Graphics))
			{
				if (InputManager.currentKeyboardState.IsKeyDown(Keys.H) && InputManager.previousKeyboardState.IsKeyUp(Keys.H))
				{
					bloomSettingsIndex = (bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
					Bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex]; //TO1 check
				}
			}
			if (debugType.HasFlag(DebugType.Camera))
			{
				if (InputManager.currentKeyboardState.IsKeyDown(Keys.J) && InputManager.previousKeyboardState.IsKeyUp(Keys.J))
				{
					Camera.ZoomScale = 1;
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.K))
				{
					Camera.ZoomScale -= 0.77F * Camera.ZoomScale * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.L))
				{
					Camera.ZoomScale += 0.77F * Camera.ZoomScale * (float)gameTime.ElapsedGameTime.TotalSeconds;
				}
			}
			if (debugType.HasFlag(DebugType.Custom1) && (StateManager.currentState as Level) != null)
			{
				if (InputManager.currentKeyboardState.IsKeyDown(Keys.G) && InputManager.previousKeyboardState.IsKeyUp(Keys.G))
				{
					((Level)StateManager.currentState).grid.hasMemory = !((Level)StateManager.currentState).grid.hasMemory;
					AddDebugString("Grid Memory: " + ((Level)StateManager.currentState).grid.hasMemory.ToString(), 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.Z) && InputManager.previousKeyboardState.IsKeyUp(Keys.Z))
				{
					((Level)StateManager.currentState).grid.speed = !((Level)StateManager.currentState).grid.speed;
					AddDebugString("Grid Speed Influence: " + ((Level)StateManager.currentState).grid.speed.ToString(), 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.H))
				{
					((Level)StateManager.currentState).grid.defaultGravity = Math.Max(((Level)StateManager.currentState).grid.defaultGravity - (0.005F * (float)gameTime.ElapsedGameTime.TotalSeconds), 0);
					AddDebugString("Grid Default Gravity: " + ((Level)StateManager.currentState).grid.defaultGravity, 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.J))
				{
					((Level)StateManager.currentState).grid.defaultGravity += 0.005F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					AddDebugString("Grid Default Gravity: " + ((Level)StateManager.currentState).grid.defaultGravity, 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.K))
				{
					((Level)StateManager.currentState).grid.gravity -= 237600.7F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					AddDebugString("Grid Gravity: " + ((Level)StateManager.currentState).grid.gravity, 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.L))
				{
					((Level)StateManager.currentState).grid.gravity += 237600.7F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					AddDebugString("Grid Gravity: " + ((Level)StateManager.currentState).grid.gravity, 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.I))
				{
					((Level)StateManager.currentState).grid.powExp -= 0.3F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					AddDebugString("Grid Power Exponent: " + ((Level)StateManager.currentState).grid.powExp, 3);
				}
				else if (InputManager.currentKeyboardState.IsKeyDown(Keys.O))
				{
					((Level)StateManager.currentState).grid.powExp += 0.3F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					AddDebugString("Grid Power Exponent: " + ((Level)StateManager.currentState).grid.powExp, 3);
				}
			}
		}
#endif
	}
}