using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnrealMono;

namespace AbstractShooter.States
{
	public class GameOver : TransactionGUIState
	{
		public override void Initialize()
		{
			base.Initialize();
			SoundsManager.PauseMusic();
			SoundsManager.PlaySoundEffect("Spawn");
		}

		protected override void LoadNextState()
		{
			StateManager.CreateAndSetState<States.MainMenuState>();
		}

		public override void EndDraw()
		{
			float stringScale = 0.895F;
			string stringToDraw = "G A M E    O V E R !";
			Vector2 stringSize = UnrealMonoGame.defaultFont.MeasureString(stringToDraw) * UnrealMonoGame.defaultFontScale * stringScale;

			UnrealMonoGame.spriteBatch.DrawString(
			UnrealMonoGame.defaultFont,
			stringToDraw,
			new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), ((UnrealMonoGame.currentResolution.Y / 2.0f) - ((stringSize.Y / 2F) * UnrealMonoGame.ResolutionScale))),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);

			base.EndDraw();
		}
	}
}