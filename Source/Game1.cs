using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using BloomPostprocess;
using InputManagement;

namespace AbstractShooter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private static GraphicsDeviceManager graphicsDeviceManager;
        //private static RenderTarget2D renderTarget2D;
        public const int minResolutionX = 1280;
        public const int minResolutionY = 720;
        private static int maxResolutionX = minResolutionX;
        private static int maxResolutionY = minResolutionY;
        public static int curResolutionX = minResolutionX;
        public static int curResolutionY = minResolutionY;
        public static float resolutionScale = 1F; //public get private set
        public static bool shouldExit = false; //public set private get
        public static bool isMaxResolution = false; //public get private set
        public static bool isBorderless = false; //public get private set
        private static bool isFullScreenAllowed = false;
        public static bool isFullScreen = false; //public get private set
        private static bool justLaunched = true;
        private static System.Timers.Timer timer;
        public static SpriteFont defaultFont; //public get private set
        public static float defaultFontScale = 0.5F; //public get private set
        public static SpriteFont smallerFont; //public get private set
        public static SpriteBatch spriteBatch = null; //public get private set

        public static Bloom bloom; //public get private set
        public static RenderTarget2D renderTarget1, renderTarget2;
#if DEBUG
        private static int bloomSettingsIndex = 0; //To remove
#endif

        private static ActionBinding exitAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Back, KeyAction.Pressed) });

        private static Game1 instance;
        public static Game1 Get { get { return instance; } }

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
            float expectedAspectRatio = (float)minResolutionX / minResolutionY;
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

            graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = false;
            graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth16;

            SetScreenState(isMaxResolution, isFullScreen, isBorderless);
            IsMouseVisible = true;

            Camera.ChangeResolution(curResolutionX, curResolutionY);

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
            InputManager.Update();

            if (exitAction.CheckBindings() || shouldExit)
            {
                SaveManager.Save();
                Exit();
            }
#if DEBUG
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.F) && InputManager.previousKeyboardState.IsKeyUp(Keys.F))
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
            else
            {
                curResolutionX = minResolutionX;
                curResolutionY = minResolutionY;
            }
            graphicsDeviceManager.IsFullScreen = isFullScreen;
            Window.IsBorderless = isBorderless;
            graphicsDeviceManager.PreferredBackBufferWidth = curResolutionX;
            graphicsDeviceManager.PreferredBackBufferHeight = curResolutionY;
            resolutionScale = (float)curResolutionX / minResolutionX;
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
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            StateManager.Draw();
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}