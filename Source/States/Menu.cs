using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AbstractShooter.States
{
    public class Level1MenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "New Game"; } }
        public override void Enter()
        {
            StateManager.SetState(new States.Level1());
        }
    }
    public class Level2MenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Level 2"; } }
        public override void Enter()
        {
            StateManager.SetState(new States.Level2());
        }
    }
    public class Level3MenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Level 3"; } }
        public override void Enter()
        {
            StateManager.SetState(new States.Level3());
        }
    }
    public class LevelEndlessMenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Endless"; } }
        public override void Enter()
        {
            StateManager.SetState(new States.LevelEndless());
        }
    }
    public class ShowControlsMenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Controls"; } }
        public ShowControlsMenuChoice()
        {
            Child = new MenuChoices();
            Child.BackgorundScreen = Game1.game.Content.Load<Texture2D>(@"Textures\ControlScreen");
        }
    }
    public class ExitMenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Quit"; } }
        public override void Enter()
        {
            Game1.ShouldExit = true;
        }
    }
    public class VolumeMenuChoice : MenuChoiceChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.mute)
                    return "Unmute";
                return "Mute";
            }
        }
        public override void Enter()
        {
            Game1.mute = !Game1.mute;
            SoundsManager.Volume = Game1.mute ? 0F : 1F;
        }
    }
    public class FullScreenMenuChoice : MenuChoiceChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.IsFullScreen)
                    return "Windowed";
                return "Exclusive Fullscreen";
            }
        }
        public override void Enter()
        {
            Game1.game.SetScreenState(Game1.IsMaxResolution, !Game1.IsFullScreen, Game1.IsBorderless);
        }
    }
    public class BorderlessMenuChoice : MenuChoiceChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.IsBorderless)
                    return "Disable Borderless";
                return "Borderless";
            }
        }
        public override void Enter()
        {
            Game1.game.SetScreenState(Game1.IsMaxResolution, Game1.IsFullScreen, !Game1.IsBorderless);
        }
    }
    public class MaximizeMenuChoice : MenuChoiceChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.IsMaxResolution)
                    return "Minimize Window";
                return "Maximize Window";
            }
        }
        public override void Enter()
        {
            Game1.game.SetScreenState(!Game1.IsMaxResolution, Game1.IsFullScreen, Game1.IsBorderless);
        }
    }
    public class ClearSaveMenuChoice : MenuChoiceChoice
    {
        public override string Name { get { return "Clear Save"; } }
        public override void Enter()
        {
            GameManager.ClearSave();
        }
    }

    public class Menu : State
    {
        //Delay between two button pressing
        private float MoveTimer = 0.1545F;
        private float MoveTime = 0.1545F;
        private static bool EffectPlayed = false;
        private MenuChoices menu;

        public override void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            //Play intro Music
            if (!EffectPlayed)
            {
                SoundsManager.PlayMenuEffect();
                EffectPlayed = true;
            }

            List<MenuChoiceChoice> SelectLevel = new List<MenuChoiceChoice>() { new Level1MenuChoice(), new Level2MenuChoice(), new Level3MenuChoice() };
            List<MenuChoiceChoice> EndlessMode = new List<MenuChoiceChoice> { new LevelEndlessMenuChoice() };
            List<MenuChoiceChoice> ShowControls = new List<MenuChoiceChoice> { new ShowControlsMenuChoice() };
            List<MenuChoiceChoice> Settings = new List<MenuChoiceChoice> { new FullScreenMenuChoice(), new MaximizeMenuChoice(), new BorderlessMenuChoice(), new VolumeMenuChoice(), new ClearSaveMenuChoice() };
            List<MenuChoiceChoice> Quit = new List<MenuChoiceChoice> { new ExitMenuChoice() };

            menu = new MenuChoices(new List<MenuChoice> {
                new MenuChoice(SelectLevel),
                new MenuChoice(EndlessMode),
                new MenuChoice(ShowControls),
                new MenuChoice(Settings),
                new MenuChoice(Quit) },
                Color.Fuchsia,
                Color.Black,
                null,
                0.5278F,
                0.05556F, //0.08125F;
                2.5F); //0.82F;

            titleScreen = content.Load<Texture2D>(@"Textures\TitleScreen");

            if (GameManager.Score > GameManager.HiScore)
                GameManager.HiScore = GameManager.Score;
        }
        private KeyboardState KS;
        private KeyboardState PKS;
        private GamePadState GS;
        private GamePadState PGS;
        public override void Update(GameTime gameTime)
        {
            KS = Keyboard.GetState();
            GS = GamePad.GetState(PlayerIndex.One);
            if ((GS.Buttons.B == ButtonState.Pressed && PGS.Buttons.B == ButtonState.Released) ||
                (KS.IsKeyDown(Keys.Back) && PKS.IsKeyUp(Keys.Back)))
            {
                if (menu.Exit())
                {
                    //MoveTimer = MoveTime;
                    SoundsManager.PlaySelection();
                }
            }
            else if ((GS.Buttons.A == ButtonState.Pressed && PGS.Buttons.A == ButtonState.Released) ||
                (KS.IsKeyDown(Keys.Space) && PKS.IsKeyUp(Keys.Space)) ||
                (KS.IsKeyDown(Keys.Enter) && PKS.IsKeyUp(Keys.Enter)))
            {
                if (menu.Enter())
                {
                    //MoveTimer = MoveTime * 1.8F;
                    SoundsManager.PlaySelection();
                }
            }
            else if (MoveTimer <= 0)
            {
                if ((GS.DPad.Down == ButtonState.Pressed) ||
                    (GS.ThumbSticks.Left.Y < -0.23f) ||
                    (KS.IsKeyDown(Keys.Down)))
                {
                    if (menu.Next())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if ((GS.DPad.Up == ButtonState.Pressed) ||
                    (GS.ThumbSticks.Left.Y > 0.23f) ||
                    (KS.IsKeyDown(Keys.Up)))
                {
                    if (menu.Previous())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if ((GS.DPad.Right == ButtonState.Pressed) ||
                    (GS.ThumbSticks.Left.X > 0.23f) ||
                    (KS.IsKeyDown(Keys.Right)))
                {
                    if (menu.SubNext())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if ((GS.DPad.Left == ButtonState.Pressed) ||
                    (GS.ThumbSticks.Left.X < -0.23f) ||
                    (KS.IsKeyDown(Keys.Left)))
                {
                    if (menu.SubPrevious())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
            }

            PKS = KS;
            PGS = GS;

            if (MoveTimer > 0)
                MoveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            StateManager.spriteBatch.Draw(
                titleScreen,
                new Rectangle(0, 0, Game1.CurResolutionX, Game1.CurResolutionY),
                Color.White);

            if (GameManager.HiScore != 0)
            {
                string stringToDraw = "Hiscore: " + GameManager.HiScore.ToString();
                float fontScale = 0.94F;
                Vector2 StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * fontScale;
                StateManager.spriteBatch.DrawString(Game1.defaultFont, stringToDraw, new Vector2((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale), 5 * Game1.ResolutionScale), Color.Fuchsia, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * fontScale, SpriteEffects.None, 0);
            }

            menu.Draw(gameTime, graphicsDevice, content);
        }
    }
}
