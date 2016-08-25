using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter.States
{
	public class GameOver : TransactionGUIState
	{
		public override void Initialize()
		{
			base.Initialize();
			SoundsManager.PauseMusic();
			SoundsManager.PlaySpawn();
		}

		protected override void LoadNextState()
		{
			StateManager.CreateAndSetState<States.MainMenuState>();
		}

		public override void EndDraw()
		{
			float stringScale = 0.895F;
			string stringToDraw = "G A M E    O V E R !";
			Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

			Game1.spriteBatch.DrawString(
			Game1.defaultFont,
			stringToDraw,
			new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), ((Game1.currentResolution.Y / 2.0f) - ((stringSize.Y / 2F) * Game1.ResolutionScale))),
				Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

			base.EndDraw();
		}
	}
}