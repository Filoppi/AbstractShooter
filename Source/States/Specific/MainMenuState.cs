using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter.States
{
    public class MainMenu : Menu
    {
        public MainMenu(List<MenuEntry> menuChoices, Color newSelectedColor, Color newUnselectedColor, Texture2D newBackgorundScreen, float newStartingYAlpha, float newYAlphaBetweenLines, float newStringScale)
        : base(menuChoices, newSelectedColor, newUnselectedColor, newBackgorundScreen, newStartingYAlpha, newYAlphaBetweenLines, newStringScale) { }

        public override void DrawMenu()
        {
            if (GameInstance.HiScore != 0)
            {
                string stringToDraw = "Hiscore: " + GameInstance.HiScore.ToString();
                float fontScale = 0.94F;
                Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * fontScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont, stringToDraw, new Vector2((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale), 5 * Game1.resolutionScale), Color.Fuchsia, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * fontScale, SpriteEffects.None, 0);
            }
        }
    }

    public class Level1MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "New Game"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level1, States.Pause>();
        }
    }
    public class Level2MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Level 2"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level2, States.Pause>();
        }
    }
    public class Level3MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Level 3"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level3, States.Pause>();
        }
    }
    public class LevelEndlessMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Endless"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.LevelEndless, States.Pause>();
        }
    }
    public class ShowControlsMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Controls"; } }
        public ShowControlsMenuChoice()
        {
            Child = new Menu();
            Child.BackgorundScreen = Game1.Get.Content.Load<Texture2D>(@"Textures\ControlScreen");
        }
    }
    public class ExitMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Quit"; } }
        public override void Enter()
        {
            Game1.shouldExit = true;
        }
    }
    public class VolumeMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (SoundsManager.Mute)
                {
                    return "Unmute";
                }
                return "Mute";
            }
        }
        public override void Enter()
        {
            SoundsManager.Mute = !SoundsManager.Mute;
        }
    }
    public class FullScreenMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.isFullScreen)
                {
                    return "Windowed";
                }
                return "Exclusive Fullscreen";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetScreenState(Game1.isMaxResolution, !Game1.isFullScreen, Game1.isBorderless);
        }
    }
    public class BorderlessMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.isBorderless)
                {
                    return "Disable Borderless";
                }
                return "Borderless";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetScreenState(Game1.isMaxResolution, Game1.isFullScreen, !Game1.isBorderless);
        }
    }
    public class MaximizeMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.isMaxResolution)
                {
                    return "Minimize Window";
                }
                return "Maximize Window";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetScreenState(!Game1.isMaxResolution, Game1.isFullScreen, Game1.isBorderless);
        }
    }
    public class ClearSaveMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Clear Save"; } }
        public override void Enter()
        {
            SaveManager.ClearSave();
        }
    }

    public class MainMenuState : MenuState
    {
        private static bool EffectPlayed = false;

        public override void Initialize()
        {
            base.Initialize();
            //Play intro Music
            if (!EffectPlayed)
            {
                SoundsManager.PlayMenuEffect();
                EffectPlayed = true;
            }

            List<MenuEntryChoice> SelectLevel = new List<MenuEntryChoice>() { new Level1MenuChoice(), new Level2MenuChoice(), new Level3MenuChoice() };
            List<MenuEntryChoice> EndlessMode = new List<MenuEntryChoice> { new LevelEndlessMenuChoice() };
            List<MenuEntryChoice> ShowControls = new List<MenuEntryChoice> { new ShowControlsMenuChoice() };
            List<MenuEntryChoice> Settings = new List<MenuEntryChoice> { new FullScreenMenuChoice(), new MaximizeMenuChoice(), new BorderlessMenuChoice(), new VolumeMenuChoice(), new ClearSaveMenuChoice() };
            List<MenuEntryChoice> Quit = new List<MenuEntryChoice> { new ExitMenuChoice() };

            menu = new MainMenu(new List<MenuEntry> {
                new MenuEntry(SelectLevel),
                new MenuEntry(EndlessMode),
                new MenuEntry(ShowControls),
                new MenuEntry(Settings),
                new MenuEntry(Quit) },
                Color.Fuchsia,
                Color.Black,
                null,
                0.5278F,
                0.05556F, //0.08125F;
                2.5F); //0.82F;

            background = Game1.Get.Content.Load<Texture2D>(@"Textures\TitleScreen");

            if (GameInstance.Score > GameInstance.HiScore)
                GameInstance.HiScore = GameInstance.Score;
        }
    }
}