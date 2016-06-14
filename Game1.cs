using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using BloomPostprocess;

namespace AbstractShooter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        public static RenderTarget2D renderTarget2D;
        public static Int32 MinResolutionX = 1280;
        public static Int32 MinResolutionY = 720;
        public static Int32 MaxResolutionX = MinResolutionX;
        public static Int32 MaxResolutionY = MinResolutionY;
        public static Int32 CurResolutionX = MinResolutionX;
        public static Int32 CurResolutionY = MinResolutionY;
        public static float ResolutionScale = 1F;
        public static bool ShouldExit = false;
        public static bool IsMaxResolution = false;
        public static bool IsBorderless = false;
        public static bool IsFullScreenAllowed = false;
        public static bool IsFullScreen = false;
        private bool justLaunched = true;
        private static System.Timers.Timer timer;
        public static SpriteFont defaultFont;
        public static float defaultFontScale = 0.5F;
        public static bool mute = false;
        public static SpriteFont smallerFont;
        public static KeyboardState keyboardState = new KeyboardState();

        PresentationParameters pp;
        public static Bloom bloom; // <--need to make this class still
        public static RenderTarget2D renderTarget1, renderTarget2;
        int bloomSettingsIndex = 0;

        public static Game1 game;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            game = this;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            GameManager.Load();

            float FullScreenMonitorAspectRatio = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.AspectRatio;
            float currentMonitorAspectRatio = (float)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            float expectedAspectRatio = (float)MinResolutionX / MinResolutionY;
            IsFullScreenAllowed = FullScreenMonitorAspectRatio.Equals(expectedAspectRatio);

            if (currentMonitorAspectRatio > expectedAspectRatio)
            {
                MaxResolutionX = (Int32)Math.Round((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * expectedAspectRatio);
                MaxResolutionY = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            }
            else if (currentMonitorAspectRatio > expectedAspectRatio)
            {
                MaxResolutionX = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                MaxResolutionY = (Int32)Math.Round((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / expectedAspectRatio);
            }
            else
            {
                MaxResolutionX = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                MaxResolutionY = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            }

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = false;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth16;

            pp = GraphicsDevice.PresentationParameters;
            SetScreenState(IsMaxResolution, IsFullScreen, IsBorderless);
            IsMouseVisible = true;

            Camera.ChangeResolution(CurResolutionX, CurResolutionY);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            defaultFont = Content.Load<SpriteFont>(@"Fonts\Font1");
            smallerFont = Content.Load<SpriteFont>(@"Fonts\Font2");
            StateManager.Init(graphics.GraphicsDevice, Content);
            //Set the game to its initial state: menu
            StateManager.SetState(new States.Menu());

            bloom = new Bloom(GraphicsDevice, StateManager.spriteBatch);

            pp = GraphicsDevice.PresentationParameters;
            bloom.LoadContent(Content, pp);
            bloom.ChangeResolution(CurResolutionX, CurResolutionY);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || (Keyboard.GetState().IsKeyDown(Keys.Escape)) || ShouldExit)
            {
                GameManager.Save();
                Exit();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.F) && keyboardState.IsKeyUp(Keys.F))
            {
                SetScreenState(IsMaxResolution, !IsFullScreen, IsBorderless);
                GameManager.Save();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.B) && keyboardState.IsKeyUp(Keys.B))
            {
                if (!IsFullScreen)
                {
                    SetScreenState(IsMaxResolution, IsFullScreen, !IsBorderless);
                    GameManager.Save();
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.M) && keyboardState.IsKeyUp(Keys.M))
            {
                if (!IsFullScreen)
                {
                    SetScreenState(!IsMaxResolution, IsFullScreen, IsBorderless);
                    GameManager.Save();
                }
            }
#if DEBUG
            else if (Keyboard.GetState().IsKeyDown(Keys.T) && keyboardState.IsKeyUp(Keys.T))
            {
                if (GameManager.TimeScale != 2.5F)
                    GameManager.TimeScale = 2.5F;
                else
                    GameManager.TimeScale = 1F;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Y) && keyboardState.IsKeyUp(Keys.Y))
            {
                if (GameManager.TimeScale != 0.25F)
                    GameManager.TimeScale = 0.25F;
                else
                    GameManager.TimeScale = 1F;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.U) && keyboardState.IsKeyUp(Keys.U))
            {
                if (GameManager.TimeScale != 0F)
                    GameManager.TimeScale = 0F;
                else
                    GameManager.TimeScale = 1F;
            }
            // Switch to the next bloom settings preset?
            else if (Keyboard.GetState().IsKeyDown(Keys.H) && keyboardState.IsKeyUp(Keys.H))
            {
                bloomSettingsIndex = (bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
                bloom.Settings = BloomSettings.PresetSettings[bloomSettingsIndex];
            }
#endif
            else if (Keyboard.GetState().IsKeyDown(Keys.V) && keyboardState.IsKeyUp(Keys.V))
            {
                mute = !mute;
                SoundsManager.Volume = mute ? 0F : 1F;
            }

            keyboardState = Keyboard.GetState();

            StateManager.Update(gameTime);

            base.Update(gameTime);
        }
        
        public void SetScreenState(bool maxResolution, bool fullscreen, bool borderless = false)
        {
            IsMaxResolution = maxResolution;
            IsBorderless = borderless;
            if (IsFullScreenAllowed)
            {
                IsFullScreen = fullscreen;
            }
            if (IsFullScreen)
            {
                CurResolutionX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                CurResolutionY = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else if (IsMaxResolution)
            {
                CurResolutionX = MaxResolutionX;
                CurResolutionY = MaxResolutionY;
            }
            else
            {
                CurResolutionX = MinResolutionX;
                CurResolutionY = MinResolutionY;
            }
            graphics.IsFullScreen = IsFullScreen;
            Window.IsBorderless = IsBorderless;
            graphics.PreferredBackBufferWidth = CurResolutionX;
            graphics.PreferredBackBufferHeight = CurResolutionY;
            ResolutionScale = (float)CurResolutionX / MinResolutionX;
            graphics.ApplyChanges();
            if (timer != null)
            {
                timer.Close();
            }
            if (!IsFullScreen)
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
                if (IsBorderless)
                {
                    Window.Position = new Point((Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)CurResolutionX / 2)), (Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)CurResolutionY / 2)));
                }
                else
                {
                    Window.Position = new Point((Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)CurResolutionX / 2)) - 8, (Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)CurResolutionY / 2)) - 4);
                }
            }

            renderTarget1 = new RenderTarget2D(GraphicsDevice, CurResolutionX, CurResolutionY, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, CurResolutionX, CurResolutionY, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
        }
        
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (IsBorderless)
            {
                Window.Position = new Point((Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)CurResolutionX / 2)), (Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)CurResolutionY / 2)));
            }
            else
            {
                Window.Position = new Point((Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2) - ((double)CurResolutionX / 2)) - 8, (Int32)Math.Round(((double)System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2) - ((double)CurResolutionY / 2)) - 4);
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
            
            StateManager.Draw(gameTime);
            
            base.Draw(gameTime);
        }
    }

    public static class Primitives2D
    {
        private static Texture2D pixel;
        private static void CreatePixel(SpriteBatch spriteBatch)
        {
            pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });
        }
        
        ///Draws a line from point1 to point2 with an offset
        public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }
        ///Draws a line from point1 to point2 with an offset
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
        {
            //calculate the distance between the two vectors
            float distance = Vector2.Distance(point1, point2);

            //calculate the angle between the two vectors
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }
        ///Draws a line from point1 to point2 with an offset
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness)
        {
            if (pixel == null)
            {
                CreatePixel(spriteBatch);
            }

            //stretch the pixel between the two vectors
            spriteBatch.Draw(pixel,
                             point,
                             null,
                             color,
                             angle,
                             Vector2.Zero,
                             new Vector2(length, thickness),
                             SpriteEffects.None,
                             0);
        }
    }

    public static class MathExtention
    {
        public static void Pow(this Vector2 vector, double exponent)
        {
            vector.X = (float)Math.Pow(vector.X, exponent);
            vector.Y = (float)Math.Pow(vector.X, exponent);
        }
    }
}
