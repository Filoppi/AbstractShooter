using AbstractShooter.States;
using BloomPostprocess;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace AbstractShooter
{
#if DEBUG

	public struct DebugString
	{
		public string stringToPrint;
		public float timeLeft;
		public Color color;

		public DebugString(string stringToPrint, Color color, float timeLeft = 0)
		{
			this.stringToPrint = stringToPrint;
			this.timeLeft = timeLeft;
			this.color = color;
		}
	}

#endif

	public enum WindowState { Windowed, FullScreen, BorderlessFullScreen }

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		//Singleton:
		private static Game1 instance;

		public static Game1 Get { get { return instance; } }

		//Graphics:
		public static GraphicsDeviceManager graphicsDeviceManager; //private

		//private static RenderTarget2D renderTarget2D;
		public static readonly Point defaultResolution = new Point(1280, 720);

		public static float expectedAspectRatio = (float)defaultResolution.X / defaultResolution.Y;
		public static readonly Point minResolution = new Point(640, 360);
		public static Point currentResolution = defaultResolution;
		public static Point currentWindowResolution = defaultResolution;
		private static Point pendingResolution = defaultResolution;
		public static Point windowedResolution = defaultResolution;
		private static float resolutionScale = 1F;
		public static float ResolutionScale { get { return resolutionScale; } }
		public static bool shouldExit; //public set private get
		public static WindowState windowState; //public set private get
		public static bool isVSync; //public get private set
		private static bool justLaunched = true;

		//private static System.Timers.Timer timer;
		public static SpriteFont defaultFont; //public get private set

		public static float defaultFontScale = 0.5F; //public get private set
		public static SpriteFont smallerFont; //public get private set
		public static SpriteBatch spriteBatch; //public get private set
		private static bool pendingResolutionChange;
		public static bool PendingResolutionChange { get { return pendingResolutionChange; } }
		private static bool isInternallyChangingResolution;
		public static System.Windows.Forms.Form form; //private

		public static Bloom bloom; //public get private set
		public static RenderTarget2D renderTarget1, renderTarget2;

#if DEBUG
		private static List<DebugString> debugStrings = new List<DebugString>();
		private static double[] fps = new double[60];
		public static double fpsAverage;
		public static SpriteFont debugFont; //public get private set
		public static float debugFontScale = 0.5F; //public get private set
		private static int bloomSettingsIndex; //To remove
		private static ActionBinding exitAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Back, KeyAction.Pressed) });
		public static bool debugCollisions = false;
		public static bool debugOverlapCollisions = false;
		private static bool testWindow = false;
		private static bool testCamera = false;
		private static bool testTimeScale = true;
		private static bool testGraphics = false;
		private static bool testGrid = true;
#endif

		public Game1()
		{
			instance = this;
			graphicsDeviceManager = new GraphicsDeviceManager(this);
			form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new System.Drawing.Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)currentResolution.X / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)currentResolution.Y / 2)));
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related Game1 Calling base.Initialize will enumerate through any components
		/// and Initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			SaveManager.Load();
			GraphicsDevice.DeviceReset += OnDisplayChanged;
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplayChanged;

			SetVSync(isVSync);
			Window.AllowUserResizing = true;
			form.MaximizeBox = false;
			form.ResizeEnd += OnWindowResized;
			form.SizeChanged += OnWindowSizeChanged;
			form.LostFocus += OnFocusLost;
			form.GotFocus += OnFocusGained;
			form.MinimumSize = new System.Drawing.Size(minResolution.X, minResolution.Y);
			graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth16;

			SetScreenState(windowState);
			IsMouseVisible = true;

			bloom = new Bloom();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your Game1.
		/// </summary>
		protected override void LoadContent()
		{
			defaultFont = Content.Load<SpriteFont>(@"Fonts\Font1");
			smallerFont = Content.Load<SpriteFont>(@"Fonts\Font2");
#if DEBUG
			debugFont = Content.Load<SpriteFont>(@"Fonts\ReadableFont");
#endif

			spriteBatch = new SpriteBatch(GraphicsDevice);
			SoundsManager.Initialize();
			//Set the game to its initial state: menu
			StateManager.CreateAndSetState<States.MainMenuState>();

			bloom.LoadContent();
			bloom.ChangeResolution(currentResolution.X, currentResolution.Y);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific Game1.
		/// </summary>
		protected override void UnloadContent()
		{
			bloom.UnloadContent();
			renderTarget1.Dispose(); renderTarget2.Dispose();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (pendingResolutionChange)
			{
				FinishSetScreenState(false);
			}

			if (IsActive)
			{
#if DEBUG
				fpsAverage = 0;
				for (int i = 0; i < fps.Length - 1; i++)
				{
					fps[i] = fps[i + 1];
					fpsAverage += fps[i];
				}
				fps[fps.Length - 1] = 1 / gameTime.ElapsedGameTime.TotalSeconds;
				fpsAverage += fps[fps.Length - 1];
				fpsAverage /= fps.Length;

				for (int i = 0; i < debugStrings.Count; i++)
				{
					DebugString debugString = debugStrings[i];
					debugString.timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
					debugStrings[i] = debugString;
					if (debugString.timeLeft < 0)
					{
						debugStrings.RemoveAt(i);
						i--;
					}
				}
#endif

				InputManager.Update();
				pendingResolutionChange = false;

#if DEBUG
				if (exitAction.CheckBindings())
				{
					shouldExit = true;
				}
#endif
				if (shouldExit)
				{
					SaveManager.Save();
					Exit();
					return;
				}

#if DEBUG
				if (testWindow)
				{
					if (InputManager.currentKeyboardState.IsKeyDown(Keys.F) && InputManager.previousKeyboardState.IsKeyUp(Keys.F))
					{
						SetScreenState(WindowState.FullScreen);
						SaveManager.Save();
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.B) && InputManager.previousKeyboardState.IsKeyUp(Keys.B))
					{
						SetScreenState(WindowState.BorderlessFullScreen);
						SaveManager.Save();
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.M) && InputManager.previousKeyboardState.IsKeyUp(Keys.M))
					{
						SetScreenState(WindowState.Windowed);
						SaveManager.Save();
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.V) && InputManager.previousKeyboardState.IsKeyUp(Keys.V))
					{
						SoundsManager.Mute = !SoundsManager.Mute;
					}
				}
				if (testTimeScale)
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
				if (testGraphics)
				{
					if (InputManager.currentKeyboardState.IsKeyDown(Keys.H) && InputManager.previousKeyboardState.IsKeyUp(Keys.H))
					{
						bloomSettingsIndex = (bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
						bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
					}
				}
				if (testCamera)
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
				if (testGrid)
				{
					if (InputManager.currentKeyboardState.IsKeyDown(Keys.G) && InputManager.previousKeyboardState.IsKeyUp(Keys.G))
					{
						((Level)StateManager.currentState).grid.hasMemory = !((Level)StateManager.currentState).grid.hasMemory;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.Z) && InputManager.previousKeyboardState.IsKeyUp(Keys.Z))
					{
						((Level)StateManager.currentState).grid.speed = !((Level)StateManager.currentState).grid.speed;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.H))
					{
						((Level)StateManager.currentState).grid.defaultGravity = Math.Max(((Level)StateManager.currentState).grid.defaultGravity - (0.005F * (float)gameTime.ElapsedGameTime.TotalSeconds), 0);
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.J))
					{
						((Level)StateManager.currentState).grid.defaultGravity += 0.005F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.K))
					{
						((Level)StateManager.currentState).grid.gravity -= 37767 * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.L))
					{
						((Level)StateManager.currentState).grid.gravity += 37767 * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.I))
					{
						((Level)StateManager.currentState).grid.powExp -= 0.3F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
					else if (InputManager.currentKeyboardState.IsKeyDown(Keys.O))
					{
						((Level)StateManager.currentState).grid.powExp += 0.3F * (float)gameTime.ElapsedGameTime.TotalSeconds;
					}
				}
#endif

				StateManager.Update(gameTime);
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			if (IsActive)
			{
				StateManager.Draw();
			}
			base.Draw(gameTime);
		}

#if DEBUG

		public static void AddDebugString(string stringToPrint, Color color, bool log = false, float timeLeft = 0)
		{
			if (log)
			{
				Debug.WriteLine(stringToPrint);
			}
			debugStrings.Add(new DebugString(stringToPrint, color, timeLeft));
		}

		public static void AddDebugString(string stringToPrint, bool log = false, float timeLeft = 0)
		{
			AddDebugString(stringToPrint, Color.Yellow, log, timeLeft);
		}

		public static void DrawFPS()
		{
			float stringScale = 3F;
			string stringToDraw;
			if (fpsAverage.ToString().Length > 3)
				stringToDraw = fpsAverage.ToString().Remove(3);
			else
				stringToDraw = fpsAverage.ToString();
			spriteBatch.DrawString(debugFont, stringToDraw,
				new Vector2(0.9328125F * currentResolution.X, 0.05347F * currentResolution.Y),
				Color.WhiteSmoke, 0, Vector2.Zero,
				ResolutionScale * debugFontScale * stringScale, SpriteEffects.None, 0);
		}

		public static void DrawDebugStrings()
		{
			for (int i = debugStrings.Count - 1; i >= 0; i--)
			{
				string stringToDraw = debugStrings[i].stringToPrint;
				int orderedI = debugStrings.Count - 1 - i;
				float stringScale = 1.7F;
				Vector2 stringSize = debugFont.MeasureString(stringToDraw) * debugFontScale * stringScale;
				spriteBatch.DrawString(debugFont, stringToDraw,
					new Vector2(0F, stringSize.Y * orderedI * ResolutionScale),
					debugStrings[i].color, 0, Vector2.Zero,
					ResolutionScale * debugFontScale * stringScale, SpriteEffects.None, 0);
			}
		}

#endif

		private void OnDisplayChanged(object sender, EventArgs e)
		{
#if DEBUG
			AddDebugString("OnDisplayChanged", Color.White, false, 3);
#endif
			SetScreenState(windowState, true);
		}

		private void OnWindowResized(object sender, EventArgs e)
		{
#if DEBUG
			AddDebugString("OnWindowResized", Color.Gray, false, 3);
#endif
			if (!isInternallyChangingResolution && Window.ClientBounds.Size != currentResolution)
			{
				SetScreenState(windowState, false);
			}
		}

		private void OnWindowSizeChanged(object sender, EventArgs e)
		{
			if (form.WindowState == FormWindowState.Maximized)
			{
				form.WindowState = FormWindowState.Normal;
			}
		}

		private void OnFocusLost(object sender, EventArgs e)
		{
			if (windowState != WindowState.Windowed)
			{
				//form.TopMost = false;
				form.WindowState = FormWindowState.Minimized;
			}
		}

		private void OnFocusGained(object sender, EventArgs e)
		{
			//form.TopMost = windowState != WindowState.Windowed;
			//form.WindowState = FormWindowState.Normal;
		}

		public void SetScreenState(WindowState windowState, bool immediate = true)
		{
			if (windowState != WindowState.Windowed && Game1.windowState == WindowState.Windowed)
			{
				windowedResolution = currentResolution;
			}
			if (Game1.windowState != WindowState.Windowed && windowState == WindowState.Windowed)
			{
				pendingResolution = windowedResolution;
			}
			else
			{
				switch (windowState)
				{
					case WindowState.FullScreen:
						{
							int greatestDisplayModePixels = 0;
							DisplayMode greatestDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
							foreach (DisplayMode displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
							{
								int displayModePixels = displayMode.Height * displayMode.Width;
								if (displayModePixels > greatestDisplayModePixels)
								{
									greatestDisplayMode = displayMode;
									greatestDisplayModePixels = displayModePixels;
								}
								//if (displayMode.Width == 1920 && displayMode.Height == 1440)
								//{
								//    greatestDisplayMode = displayMode;
								//    break;
								//}
							}
							//pendingResolution.X = greatestDisplayMode.Width;
							//pendingResolution.Y = greatestDisplayMode.Height;

							float currentMonitorAspectRatio = (float)greatestDisplayMode.Width / greatestDisplayMode.Height;

							if (currentMonitorAspectRatio > expectedAspectRatio)
							{
								pendingResolution.X = (int)Math.Round((double)greatestDisplayMode.Height * expectedAspectRatio);
								pendingResolution.Y = greatestDisplayMode.Height;
							}
							else if (currentMonitorAspectRatio < expectedAspectRatio)
							{
								pendingResolution.X = greatestDisplayMode.Width;
								pendingResolution.Y = (int)Math.Round((double)greatestDisplayMode.Width / expectedAspectRatio);
							}
							else
							{
								pendingResolution.X = greatestDisplayMode.Width;
								pendingResolution.Y = greatestDisplayMode.Height;
							}
							break;
						}
					case WindowState.BorderlessFullScreen:
						{
							float currentMonitorAspectRatio = (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

							if (currentMonitorAspectRatio > expectedAspectRatio)
							{
								pendingResolution.X = (int)Math.Round((double)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * expectedAspectRatio);
								pendingResolution.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
							}
							else if (currentMonitorAspectRatio < expectedAspectRatio)
							{
								pendingResolution.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
								pendingResolution.Y = (int)Math.Round((double)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / expectedAspectRatio);
							}
							else
							{
								pendingResolution.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
								pendingResolution.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
							}
							break;

							//float currentMonitorAspectRatio = (float)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

							//if (currentMonitorAspectRatio > expectedAspectRatio)
							//{
							//    pendingResolution.X = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * expectedAspectRatio).Round();
							//    pendingResolution.Y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
							//}
							//else if (currentMonitorAspectRatio < expectedAspectRatio)
							//{
							//    pendingResolution.X = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
							//    pendingResolution.Y = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / expectedAspectRatio).Round();
							//}
							//else
							//{
							//    pendingResolution.X = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
							//    pendingResolution.Y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
							//}
						}
					case WindowState.Windowed:
						{
							if (!justLaunched)
							{
								float currentWindowAspectRatio = (float)Window.ClientBounds.Width / Window.ClientBounds.Height;
								if (currentWindowAspectRatio > expectedAspectRatio)
								{
									if (Window.ClientBounds.Width != currentResolution.X)
									{
										int clampedWidth = Math.Max(Window.ClientBounds.Width, minResolution.X);
										pendingResolution.X = clampedWidth;
										pendingResolution.Y = (clampedWidth / expectedAspectRatio).Round();
									}
									else
									{
										int clampedHeight = Math.Max(Window.ClientBounds.Height, minResolution.Y);
										pendingResolution.X = (clampedHeight * expectedAspectRatio).Round();
										pendingResolution.Y = clampedHeight;
									}
								}
								else if (currentWindowAspectRatio < expectedAspectRatio)
								{
									if (Window.ClientBounds.Height != currentResolution.Y)
									{
										int clampedHeight = Math.Max(Window.ClientBounds.Height, minResolution.Y);
										pendingResolution.X = (clampedHeight * expectedAspectRatio).Round();
										pendingResolution.Y = clampedHeight;
									}
									else
									{
										int clampedWidth = Math.Max(Window.ClientBounds.Width, minResolution.X);
										pendingResolution.X = clampedWidth;
										pendingResolution.Y = (clampedWidth / expectedAspectRatio).Round();
									}
								}
								else
								{
									pendingResolution.X = Math.Max(Window.ClientBounds.Width, minResolution.X);
									pendingResolution.Y = Math.Max(Window.ClientBounds.Height, minResolution.Y);
								}
							}
							break;
						}
				}
			}

			Game1.windowState = windowState;

			if (immediate)
			{
				FinishSetScreenState(true);
			}
			else
			{
				pendingResolutionChange = true;
			}
		}

		public void FinishSetScreenState(bool centerWindow)
		{
			isInternallyChangingResolution = true;

			currentResolution = pendingResolution;
			currentWindowResolution = currentResolution;
			//currentResolution.X /= 2;
			//currentResolution.Y /= 2;

			if ((windowState == WindowState.FullScreen) != graphicsDeviceManager.IsFullScreen)
			{
				graphicsDeviceManager.IsFullScreen = windowState == WindowState.FullScreen;
			}
			if ((windowState == WindowState.BorderlessFullScreen) != Window.IsBorderless)
			{
				Window.IsBorderless = windowState == WindowState.BorderlessFullScreen;
				form.FormBorderStyle = Window.IsBorderless ? FormBorderStyle.None : FormBorderStyle.Sizable;
				//form.MaximumSize = new System.Drawing.Size(0, 0);
			}
			//if (!Window.IsBorderless)
			//{
			//    float currentMonitorAspectRatio = (float)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

			//    System.Drawing.Size temp = new System.Drawing.Size();
			//    if (currentMonitorAspectRatio > expectedAspectRatio)
			//    {
			//        temp.Width = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * expectedAspectRatio).Round();
			//        temp.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
			//    }
			//    else if (currentMonitorAspectRatio < expectedAspectRatio)
			//    {
			//        temp.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
			//        temp.Height = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / expectedAspectRatio).Round();
			//    }
			//    else
			//    {
			//        temp.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
			//        temp.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
			//    }
			//    form.MaximumSize = temp;
			//}
			if ((windowState == WindowState.BorderlessFullScreen) != form.TopMost)
			{
				form.TopMost = windowState == WindowState.BorderlessFullScreen;
			}

			graphicsDeviceManager.PreferredBackBufferWidth = currentResolution.X;
			graphicsDeviceManager.PreferredBackBufferHeight = currentResolution.Y;
			resolutionScale = (float)currentResolution.X / defaultResolution.X;

			graphicsDeviceManager.ApplyChanges();

#if DEBUG
			AddDebugString(Window.ClientBounds.ToString(), windowState != WindowState.Windowed ? Color.Green : Color.Yellow, false, 3);
			//AddDebugString(Window.ClientBounds.ToString(), windowState != WindowState.Windowed ? Color.Blue : Color.Red, false, 3);
			//AddDebugString(currentResolution.ToString(), windowState != WindowState.Windowed ? Color.Green : Color.Yellow, false, 3);
#endif
			//if (windowState == WindowState.FullScreen)
			//{
			//    currentWindowResolution.X = 1920;
			//    currentWindowResolution.Y = 1440;
			//    form.Size = new System.Drawing.Size(1920, 1440);
			//}

			PresentationParameters pp = GraphicsDevice.PresentationParameters;
			renderTarget1 = new RenderTarget2D(GraphicsDevice, currentResolution.X, currentResolution.Y, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			renderTarget2 = new RenderTarget2D(GraphicsDevice, currentResolution.X, currentResolution.Y, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

			//if (timer != null)
			//{
			//    timer.Close();
			//}
			if (windowState != WindowState.FullScreen)
			{
				//if (justLaunched || centerWindow)
				//{
				//    timer = new System.Timers.Timer();
				//    if (justLaunched)
				//    {
				//        timer.Interval = 400;
				//    }
				//    else
				//    {
				//        timer.Interval = 1;
				//    }
				//    timer.Elapsed += OnTimedEvent;
				//    timer.AutoReset = false;
				//    timer.Start();
				//}
				if (windowState == WindowState.BorderlessFullScreen)
				{
					form.Location = System.Drawing.Point.Empty;
				}
				else if (centerWindow)
				{
					form.Location = new System.Drawing.Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)currentResolution.X / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)currentResolution.Y / 2)));
					//Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)currentResolution.X / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)currentResolution.Y / 2)));
				}
			}
			justLaunched = false;
			isInternallyChangingResolution = false;
		}

		public void SetVSync(bool vSync)
		{
			isVSync = vSync;
			graphicsDeviceManager.SynchronizeWithVerticalRetrace = isVSync;
			IsFixedTimeStep = isVSync;
		}

		//private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
		//{
		//    if (windowState == WindowState.BorderlessFullScreen)
		//    {
		//        form.Location = System.Drawing.Point.Empty;
		//    }
		//    else
		//    {
		//        form.Location = new System.Drawing.Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)currentResolution.X / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)currentResolution.Y / 2)));
		//        //Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)currentResolution.X / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)currentResolution.Y / 2)));
		//    }
		//}
	}
}