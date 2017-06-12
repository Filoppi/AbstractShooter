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

namespace UnrealMono
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

    //We can have multiple debug types going on
    [Flags]
    public enum DebugType
    {
        None = 0,
        Window = 1,
        Camera = 2,
        TimeScale = 4,
        Graphics = 8,
        Collisions = 16,
        OverlapCollisions = 32,
        Custom1 = 64, //Grid
        Custom2 = 128,
        Custom3 = 256
    }
#endif

    public enum WindowState { Windowed, FullScreen, BorderlessFullScreen }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class UnrealMonoGame : Game
    {
        //Singleton:
        protected static UnrealMonoGame instance;

        public static UnrealMonoGame Get => instance;

        //Graphics:
        public static GraphicsDeviceManager graphicsDeviceManager; //protected

        public static SpriteBatch spriteBatch; //public get protected set

        public static GameTime frameGameTime;//To
                                             //protected static RenderTarget2D renderTarget2D;
        public static readonly Point defaultResolution = new Point(1280, 720);

        public static float expectedAspectRatio = (float)defaultResolution.X / defaultResolution.Y;
        public static readonly Point minResolution = new Point(640, 360);
        public static Point currentResolution = defaultResolution;
        public static Point currentWindowResolution = defaultResolution;
        protected static Point pendingResolution = defaultResolution;
        public static Point windowedResolution = defaultResolution;
        protected static float resolutionScale = 1F;
        public static float ResolutionScale => resolutionScale;
        public static bool shouldExit; //public set protected get
        public static WindowState windowState; //public set protected get
        public static bool isVSync; //public get protected set
        protected static bool justLaunched = true;

        //private static System.Timers.Timer timer;
        public static SpriteFont defaultFont; //public get private set

        public static float defaultFontScale = 0.5F; //public get private set
        public static SpriteFont smallerFont; //public get private set
        private static bool pendingResolutionChange;
        public static bool PendingResolutionChange => pendingResolutionChange;
        private static bool isInternallyChangingResolution;
        public static System.Windows.Forms.Form form; //private

        private static Bloom bloom;
        public static Bloom Bloom { get { return bloom; } private set { bloom = value; } }
        public static RenderTarget2D renderTarget1, renderTarget2;

#if DEBUG
        protected static List<DebugString> debugStrings = new List<DebugString>();
        protected static double[] fps = new double[60];
        public static double fpsAverage;
        public static SpriteFont debugFont; //public get protected set
        public static float debugFontScale = 0.5F; //public get protected set
        public static DebugType debugType = DebugType.None;
        protected static int bloomSettingsIndex; //To remove
        protected static ActionBinding exitAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Back, KeyAction.Pressed) });
#endif

        public UnrealMonoGame()
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
        /// related AbstractShooterGame Calling base.Initialize will enumerate through any components
        /// and Initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            SaveManager.LoadAndSetEngineSettings();
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
        /// all of your UnrealMonoGame.
        /// </summary>
        protected override void LoadContent()
        {
            defaultFont = Content.Load<SpriteFont>(@"Font\Font1");
            smallerFont = Content.Load<SpriteFont>(@"Font\Font2");
#if DEBUG
            debugFont = Content.Load<SpriteFont>(@"Font\ReadableFont");
#endif

            spriteBatch = new SpriteBatch(GraphicsDevice);
            Primitives2DDraw.CreatePixel(spriteBatch);
            SoundsManager.Initialize();
            //Set the game to its initial state: menu
            StateManager.CreateAndSetState(GetInitialState());

            bloom.LoadContent();
            bloom.ChangeResolution(currentResolution.X, currentResolution.Y);
        }

        public virtual Type GetInitialState() //To finish
        {
            return typeof(State);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific UnrealMonoGame.
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
            frameGameTime = gameTime;
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
                    SaveManager.SaveEngineSettings();
                    OnExit();
                    Exit();
                    return;
                }

#if DEBUG
                ExecuteDebugOptions(gameTime);
#endif

                StateManager.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected virtual void OnExit() { }

#if DEBUG
        protected virtual void ExecuteDebugOptions(GameTime gameTime) { }
#endif

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

        private void OnDisplayChanged(object sender, EventArgs e)
        {
#if DEBUG
            AddDebugString("OnDisplayChanged", Color.White, false, 3);
#endif
            if (!isInternallyChangingResolution)
            {
                SetScreenState(windowState, true);
            }
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
            if (windowState != WindowState.Windowed && UnrealMonoGame.windowState == WindowState.Windowed)
            {
                windowedResolution = currentResolution;
            }
            if (UnrealMonoGame.windowState != WindowState.Windowed && windowState == WindowState.Windowed)
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

            UnrealMonoGame.windowState = windowState;

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
#if !DEBUG
				form.TopMost = windowState == WindowState.BorderlessFullScreen;
#endif
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

#if DEBUG
        public static void AddDebugString(string stringToPrint, Color color, bool log = false, float timeLeft = 0)
        {
            if (log)
            {
                Debug.WriteLine(stringToPrint);
            }
            debugStrings.Add(new DebugString(stringToPrint, color, timeLeft));
        }
        public static void AddDebugString(string stringToPrint, float timeLeft = 0)
        {
            AddDebugString(stringToPrint, Color.Yellow, false, timeLeft);
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
    }
}