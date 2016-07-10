using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using BloomPostprocess;
using InputManagement;

namespace AbstractShooter
{
#if DEBUG
    public struct DebugString
    {
        public string stringToPrint;
        public float timeLeft;
        public Color color;

        public DebugString(string stringToPrint, float timeLeft = 0)
        {
            this.stringToPrint = stringToPrint;
            this.timeLeft = timeLeft;
            color = Color.Yellow;
        }
        public DebugString(string stringToPrint, Color color, float timeLeft = 0)
        {
            this.stringToPrint = stringToPrint;
            this.timeLeft = timeLeft;
            this.color = color;
        }
    }
#endif

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        //Singleton:
        private static Game1 instance;
        public static Game1 Get { get { return instance; } }

        //Graphics:
        private static GraphicsDeviceManager graphicsDeviceManager;
        //private static RenderTarget2D renderTarget2D;
        public const int defaultResolutionX = 1280;
        public const int defaultResolutionY = 720;
        public static int minResolutionX = defaultResolutionX; // / 2
        public static int minResolutionY = defaultResolutionY; // / 2
        private static int maxResolutionX = defaultResolutionX;
        private static int maxResolutionY = defaultResolutionY;
        public static int curResolutionX = defaultResolutionX;
        public static int curResolutionY = defaultResolutionY;
        private static float resolutionScale = 1F;
        public static float ResolutionScale { get { return resolutionScale; } }
        public static bool shouldExit; //public set private get
        public static bool isMaxResolution; //public get private set
        public static bool isBorderless; //public get private set
        private static bool isFullScreenAllowed;
        public static bool isFullScreen; //public get private set
        public static bool isVSync; //public get private set
        private static bool justLaunched = true;
        private static System.Timers.Timer timer;
        public static SpriteFont defaultFont; //public get private set
        public static float defaultFontScale = 0.5F; //public get private set
        public static SpriteFont smallerFont; //public get private set
        public static SpriteBatch spriteBatch = null; //public get private set

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
        private static ActionBinding resetCameraZoomAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.J, KeyAction.Down) }, new KeyBinding<Buttons>[] { });
        private static ActionBinding zoomCameraAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.L, KeyAction.Down) }, new KeyBinding<Buttons>[] { });
        private static ActionBinding unzoomCameraAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.K, KeyAction.Down) }, new KeyBinding<Buttons>[] { });
        public static bool debugCollisions = false;
        public static bool debugOverlapCollisions = false;
#endif

        public Game1()
        {
            instance = this;
            graphicsDeviceManager = new GraphicsDeviceManager(this);
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

            float FullScreenMonitorAspectRatio = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.AspectRatio;
            float currentMonitorAspectRatio = (float)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            float expectedAspectRatio = (float)defaultResolutionX / defaultResolutionY;
            isFullScreenAllowed = FullScreenMonitorAspectRatio.Equals(expectedAspectRatio);

            if (currentMonitorAspectRatio > expectedAspectRatio)
            {
                maxResolutionX = (int)Math.Round((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * expectedAspectRatio);
                maxResolutionY = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            }
            else if (currentMonitorAspectRatio > expectedAspectRatio)
            {
                maxResolutionX = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                maxResolutionY = (int)Math.Round((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / expectedAspectRatio);
            }
            else
            {
                maxResolutionX = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                maxResolutionY = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            }

            SetVSync(isVSync);
            Window.AllowUserResizing = false;
            graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth16;

            SetScreenState(isMaxResolution, isFullScreen, isBorderless);
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
            bloom.ChangeResolution(curResolutionX, curResolutionY);
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
#if (DEBUG)
            fpsAverage = 0;
            for (int i = 0; i < fps.Length - 1; i++)
            {
                fps[i] = fps[i+1];
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
            if (InputManager.currentKeyboardState.IsKeyDown(Keys.F) && InputManager.previousKeyboardState.IsKeyUp(Keys.F))
            {
                SetScreenState(isMaxResolution, !isFullScreen, isBorderless);
                SaveManager.Save();
            }
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.B) && InputManager.previousKeyboardState.IsKeyUp(Keys.B))
            {
                if (!isFullScreen)
                {
                    SetScreenState(isMaxResolution, isFullScreen, !isBorderless);
                    SaveManager.Save();
                }
            }
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.M) && InputManager.previousKeyboardState.IsKeyUp(Keys.M))
            {
                if (!isFullScreen)
                {
                    SetScreenState(!isMaxResolution, isFullScreen, isBorderless);
                    SaveManager.Save();
                }
            }
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.T) && InputManager.previousKeyboardState.IsKeyUp(Keys.T))
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
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.H) && InputManager.previousKeyboardState.IsKeyUp(Keys.H))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
            }
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.V) && InputManager.previousKeyboardState.IsKeyUp(Keys.V))
            {
                SoundsManager.Mute = !SoundsManager.Mute;
            }
            else if (resetCameraZoomAction.CheckBindings())
            {
                Camera.ZoomScale = 1;
            }
            if (zoomCameraAction.CheckBindings())
            {
                Camera.ZoomScale += 0.77F * Camera.ZoomScale * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (unzoomCameraAction.CheckBindings())
            {
                Camera.ZoomScale -= 0.77F * Camera.ZoomScale * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
#endif

            StateManager.Update(gameTime);

            base.Update(gameTime);
        }
        
        public void SetScreenState(bool maxResolution, bool fullscreen, bool borderless = false)
        {
            isMaxResolution = maxResolution;
            isBorderless = borderless;
            if (isFullScreenAllowed)
            {
                isFullScreen = fullscreen;
            }
            if (isFullScreen)
            {
                curResolutionX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                curResolutionY = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else if (isMaxResolution)
            {
                curResolutionX = maxResolutionX;
                curResolutionY = maxResolutionY;
            }
            else if (false)
            {
                curResolutionX = defaultResolutionX;
                curResolutionY = defaultResolutionY;
            }
            else
            {
                curResolutionX = minResolutionX;
                curResolutionY = minResolutionY;
            }
            graphicsDeviceManager.IsFullScreen = isFullScreen;
            Window.IsBorderless = isBorderless;
            graphicsDeviceManager.PreferredBackBufferWidth = curResolutionX;
            graphicsDeviceManager.PreferredBackBufferHeight = curResolutionY;
            resolutionScale = (float)curResolutionX / defaultResolutionX;
            graphicsDeviceManager.ApplyChanges();
            if (timer != null)
            {
                timer.Close();
            }
            if (!isFullScreen)
            {
                timer = new System.Timers.Timer();
                if (justLaunched)
                {
                    timer.Interval = 400;
                    justLaunched = false;
                }
                else
                {
                    timer.Interval = 1;
                }
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                timer.AutoReset = false;
                timer.Start();
                if (isBorderless)
                {
                    Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)curResolutionX / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)curResolutionY / 2)));
                }
                else
                {
                    Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)curResolutionX / 2)) - 8, (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)curResolutionY / 2)) - 4);
                }
            }

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget1 = new RenderTarget2D(GraphicsDevice, curResolutionX, curResolutionY, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, curResolutionX, curResolutionY, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
        }

        public void SetVSync(bool vSync)
        {
            isVSync = vSync;
            graphicsDeviceManager.SynchronizeWithVerticalRetrace = isVSync;
            IsFixedTimeStep = isVSync;
        }
        
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (isBorderless)
            {
                Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)curResolutionX / 2)), (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)curResolutionY / 2)));
            }
            else
            {
                Window.Position = new Point((int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)curResolutionX / 2)) - 8, (int)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)curResolutionY / 2)) - 4);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            StateManager.Draw();
            base.Draw(gameTime);
        }

#if (DEBUG)
        public static void AddDebugString(string stringToPrint, Color color, float timeLeft = 0)
        {
            debugStrings.Add(new DebugString(stringToPrint, color, timeLeft));
        }
        public static void AddDebugString(string stringToPrint, float timeLeft = 0)
        {
            debugStrings.Add(new DebugString(stringToPrint, timeLeft));
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
                new Vector2(0.9328125F * curResolutionX, 0.05347F * curResolutionY),
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