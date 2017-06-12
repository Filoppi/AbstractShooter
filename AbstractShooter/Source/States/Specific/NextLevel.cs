using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnrealMono;

namespace AbstractShooter.States
{
	public class NextLevel : TransactionGUIState
	{
		public float TimeLeft; //Temp fix

		public override void Initialize()
		{
			base.Initialize();
			SoundsManager.PauseMusic();
			SoundsManager.PlaySoundEffect("PowerUp");
		}

		protected override void LoadNextState()
		{
			if (GameInstance.lastPlayedLevel == 1)
				StateManager.CreateAndSetState<States.Level2, States.PauseMenuState>();
			else if (GameInstance.lastPlayedLevel == 2)
				StateManager.CreateAndSetState<States.Level3, States.PauseMenuState>();
		}

		public override void EndDraw()
		{
			float stringScale = 0.79F;
			string stringToDraw;
			Vector2 stringSize;

			if (TimeLeft > 0)
			{
				stringToDraw = "Enemies Destroyed";
				stringSize = UnrealMonoGame.defaultFont.MeasureString(stringToDraw) * UnrealMonoGame.defaultFontScale * stringScale;
				UnrealMonoGame.spriteBatch.DrawString(
					UnrealMonoGame.defaultFont,
					stringToDraw,
					new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), (UnrealMonoGame.currentResolution.Y / 2.0f) - (135 * UnrealMonoGame.ResolutionScale)),
					Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}
			else
			{
				stringToDraw = "Survived Time";
				stringSize = UnrealMonoGame.defaultFont.MeasureString(stringToDraw) * UnrealMonoGame.defaultFontScale * stringScale;
				UnrealMonoGame.spriteBatch.DrawString(
					UnrealMonoGame.defaultFont,
					stringToDraw,
					new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), (UnrealMonoGame.currentResolution.Y / 2.0f) - (135 * UnrealMonoGame.ResolutionScale)),
					Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}

			stringToDraw = "L O A D I N G   N E X T   L E V E L";
			stringSize = UnrealMonoGame.defaultFont.MeasureString(stringToDraw) * UnrealMonoGame.defaultFontScale * stringScale;
			UnrealMonoGame.spriteBatch.DrawString(
				UnrealMonoGame.defaultFont,
				stringToDraw,
				new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), (UnrealMonoGame.currentResolution.Y / 2.0f) - (89 * UnrealMonoGame.ResolutionScale)),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);

			stringToDraw = "( H A R D E R )";
			stringSize = UnrealMonoGame.defaultFont.MeasureString(stringToDraw) * UnrealMonoGame.defaultFontScale * stringScale;
			UnrealMonoGame.spriteBatch.DrawString(
				UnrealMonoGame.defaultFont,
				stringToDraw,
				new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), (UnrealMonoGame.currentResolution.Y / 2.0f) - (43 * UnrealMonoGame.ResolutionScale)),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);

			base.EndDraw();
		}
	}
}