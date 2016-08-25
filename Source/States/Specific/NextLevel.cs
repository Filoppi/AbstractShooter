using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter.States
{
	public class NextLevel : TransactionGUIState
	{
		public float TimeLeft; //Temp fix

		public override void Initialize()
		{
			base.Initialize();
			SoundsManager.PauseMusic();
			SoundsManager.PlayPowerUp();
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
				stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
				Game1.spriteBatch.DrawString(
					Game1.defaultFont,
					stringToDraw,
					new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), (Game1.currentResolution.Y / 2.0f) - (135 * Game1.ResolutionScale)),
					Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}
			else
			{
				stringToDraw = "Survived Time";
				stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
				Game1.spriteBatch.DrawString(
					Game1.defaultFont,
					stringToDraw,
					new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), (Game1.currentResolution.Y / 2.0f) - (135 * Game1.ResolutionScale)),
					Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}

			stringToDraw = "L O A D I N G   N E X T   L E V E L";
			stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
			Game1.spriteBatch.DrawString(
				Game1.defaultFont,
				stringToDraw,
				new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), (Game1.currentResolution.Y / 2.0f) - (89 * Game1.ResolutionScale)),
				Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

			stringToDraw = "( H A R D E R )";
			stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
			Game1.spriteBatch.DrawString(
				Game1.defaultFont,
				stringToDraw,
				new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), (Game1.currentResolution.Y / 2.0f) - (43 * Game1.ResolutionScale)),
				Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

			base.EndDraw();
		}
	}
}