using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UnrealMono;

namespace AbstractShooter.States
{
	public class GUIState : State
	{
		protected Texture2D background;

		public override void BeginDraw()
		{
			base.BeginDraw();

			if (background != null)
			{
				UnrealMonoGame.spriteBatch.Draw(background, new Rectangle(0, 0, UnrealMonoGame.currentResolution.X, UnrealMonoGame.currentResolution.Y), Color.White);
			}
		}
	}

	public class TransactionGUIState : GUIState
	{
		protected float timer = 0F;
		protected const float enterActionWait = 0.54F;
		protected const float transactionWait = 4.56F;
		protected ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.A, KeyAction.Pressed) });

		protected virtual void LoadNextState()
		{
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (timer > transactionWait || (timer > enterActionWait && enterAction.CheckBindings()))
			{
				LoadNextState();
			}
		}
	}
}