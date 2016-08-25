using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace AbstractShooter.States
{
	public class PauseMenu : Menu
	{
		public PauseMenu(List<MenuEntry> menuChoices, Color newSelectedColor, Color newUnselectedColor, Texture2D newBackgorundScreen, float newStartingYAlpha, float newYAlphaBetweenLines, float newStringScale)
		: base(menuChoices, newSelectedColor, newUnselectedColor, newBackgorundScreen, newStartingYAlpha, newYAlphaBetweenLines, newStringScale)
		{ }

		public override void DrawMenu()
		{
			string stringToDraw = "Pause";
			float stringScale = 1F;
			Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
			Game1.spriteBatch.DrawString(Game1.defaultFont, stringToDraw,
				new Vector2((Game1.currentResolution.X / 2F) - ((stringSize.X / 2F) * Game1.ResolutionScale),
				Game1.currentResolution.Y * 0.00694F), Color.White, 0, Vector2.Zero,
				Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
		}
	}

	public class ResumeMenuChoice : MenuEntryChoice
	{
		public override string Name { get { return "Resume"; } }

		public override void Enter()
		{
			StateManager.Unpause();
		}
	}

	public class RestartMenuChoice : MenuEntryChoice
	{
		public override string Name { get { return "Restart"; } }

		public override void Enter()
		{
			StateManager.ReloadCurrentState();
		}
	}

	public class MainMenuMenuChoice : MenuEntryChoice
	{
		public override string Name { get { return "Main Menu"; } }

		public override void Enter()
		{
			StateManager.CreateAndSetState<States.MainMenuState>();
		}
	}

	public class PauseMenuState : MenuState
	{
		public override void Initialize()
		{
			base.Initialize();

			backAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed), new KeyBinding<Keys>(Keys.P, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.B, KeyAction.Pressed) });

			List<MenuEntryChoice> Resume = new List<MenuEntryChoice> { new ResumeMenuChoice() };
			List<MenuEntryChoice> Restart = new List<MenuEntryChoice> { new RestartMenuChoice() };
			List<MenuEntryChoice> ShowControls = new List<MenuEntryChoice> { new ShowControlsMenuChoice() };
			List<MenuEntryChoice> Settings = new List<MenuEntryChoice> { new FullScreenMenuChoice(), new BorderlessMenuChoice(), new VSyncMenuChoice(), new VolumeMenuChoice() };
			List<MenuEntryChoice> ReturnToMainMenu = new List<MenuEntryChoice> { new MainMenuMenuChoice() };
			List<MenuEntryChoice> Quit = new List<MenuEntryChoice> { new ExitMenuChoice() };

			menu = new PauseMenu(new List<MenuEntry> {
				new MenuEntry(Resume),
				new MenuEntry(Restart),
				new MenuEntry(ShowControls),
				new MenuEntry(Settings),
				new MenuEntry(ReturnToMainMenu),
				new MenuEntry(Quit) },
				Color.Fuchsia,
				Color.White,
				null,
				0.375F,
				0.05556F, //0.08125F;
				2.5F); //0.82F;
		}

		public override void OnSetAsCurrentState()
		{
			base.OnSetAsCurrentState();
			SoundsManager.PauseMusic();
		}
	}
}