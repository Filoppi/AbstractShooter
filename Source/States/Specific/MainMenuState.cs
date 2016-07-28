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
                float stringScale = 0.94F;
                Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont, stringToDraw, new Vector2((Game1.currentResolution.X / 2F) - ((stringSize.X / 2F) * Game1.ResolutionScale), Game1.currentResolution.Y * 0.00694F), Color.Fuchsia, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
        }
    }

    public class Level1MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "New Game"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level1, States.PauseMenuState>();
        }
    }
    public class Level2MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Level 2"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level2, States.PauseMenuState>();
        }
    }
    public class Level3MenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Level 3"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.Level3, States.PauseMenuState>();
        }
    }
    public class LevelEndlessMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Endless"; } }
        public override void Enter()
        {
            StateManager.CreateAndSetState<States.LevelEndless, States.PauseMenuState>();
        }
    }
    public class ShowControlsMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Controls"; } }
        public ShowControlsMenuChoice()
        {
            child = new Menu();
            child.backgorundScreen = Game1.Get.Content.Load<Texture2D>(@"Textures\ControlScreen");
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
                if (Game1.windowState == WindowState.FullScreen)
                {
                    return "Windowed";
                }
                return "Exclusive Fullscreen";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetScreenState(Game1.windowState == WindowState.FullScreen ? WindowState.Windowed : WindowState.FullScreen);
        }
    }
    public class BorderlessMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.windowState == WindowState.BorderlessFullScreen)
                {
                    return "Windowed";
                }
                return "Borderless Fullscreen";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetScreenState(Game1.windowState == WindowState.BorderlessFullScreen ? WindowState.Windowed : WindowState.BorderlessFullScreen);
        }
    }
    public class VSyncMenuChoice : MenuEntryChoice
    {
        public override string Name
        {
            get
            {
                if (Game1.isVSync)
                {
                    return "Disable V-Sync";
                }
                return "Enable V-Sync";
            }
        }
        public override void Enter()
        {
            Game1.Get.SetVSync(!Game1.isVSync);
        }
    }
    public class ResetSaveMenuChoice : MenuEntryChoice
    {
        public override string Name { get { return "Reset Save"; } }
        public override void Enter()
        {
            SaveManager.ResetSave();
        }
    }
    //public class InterateResolutionsMenuChoice : MenuEntryChoice
    //{
    //    private int count;
    //    private int currentIndex;
    //    public InterateResolutionsMenuChoice()
    //    {
    //        count = Game1.supportedFullScreenResolutions.Count;
    //    }
    //    public override string Name
    //    {
    //        get
    //        {
    //            if (count > 0)
    //            {
    //                if (currentIndex < count - 1)
    //                {
    //                    return Game1.supportedFullScreenResolutions[currentIndex + 1].ToString();
    //                }
    //                return Game1.supportedFullScreenResolutions[0].ToString();
    //            }
    //            return "???";
    //        }
    //    }
    //    public override void Enter()
    //    {
    //        if (count > 0)
    //        {
    //            if (currentIndex < count - 1)
    //            {
    //                currentIndex++;
    //            }
    //            else
    //            {
    //                currentIndex = 0;
    //            }
    //        }
    //    }
    //}

    public class MainMenuState : MenuState
    {
        private static bool EffectPlayed;

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
            List<MenuEntryChoice> Settings = new List<MenuEntryChoice> { new FullScreenMenuChoice(), new BorderlessMenuChoice(), new VSyncMenuChoice(), new VolumeMenuChoice(), new ResetSaveMenuChoice() };
            //List<MenuEntryChoice> Resolution = new List<MenuEntryChoice> { new InterateResolutionsMenuChoice() };
            List<MenuEntryChoice> Quit = new List<MenuEntryChoice> { new ExitMenuChoice() };

            menu = new MainMenu(new List<MenuEntry> {
                new MenuEntry(SelectLevel),
                new MenuEntry(EndlessMode),
                new MenuEntry(ShowControls),
                new MenuEntry(Settings),
                //new MenuEntry(Resolution),
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