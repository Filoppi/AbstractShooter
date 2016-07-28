using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using InputManagement;

namespace AbstractShooter.States
{
    public abstract class MenuState : GUIState
    {
        protected Menu menu;
        protected ActionBinding downAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Down, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadDown, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickDown, KeyAction.Pressed) });
        protected ActionBinding upAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Up, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadUp, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickUp, KeyAction.Pressed) });
        protected ActionBinding rightAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Right, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadRight, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickRight, KeyAction.Pressed) });
        protected ActionBinding leftAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Left, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadLeft, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickLeft, KeyAction.Pressed) });
        protected ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Enter, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.A, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });
        protected ActionBinding cursorEnterAction = new ActionBinding(new KeyBinding<Keys>[] { }, new KeyBinding<Buttons>[] { }, new KeyBinding<MouseButtons>[] { new KeyBinding<MouseButtons>(MouseButtons.Left, KeyAction.Pressed) });
        protected ActionBinding backAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.B, KeyAction.Pressed) });

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (menu.Update(ref downAction, ref upAction, ref rightAction, ref leftAction, ref enterAction, ref cursorEnterAction, ref backAction, InputManager.currentMouseState.Position))
            {
                SoundsManager.PlaySelection();
            }
        }

        public override void EndDraw()
        {
            menu.Draw();
#if DEBUG
            /*int cc = 39;
            Point currentResolution = Game1.currentResolution;
            currentResolution.X = (int)Math.Round((double)(currentResolution.Y * Game1.expectedAspectRatio));
            //BottomRight
            Game1.spriteBatch.DrawLine(currentResolution.X - cc, currentResolution.Y, currentResolution.X, currentResolution.Y, Color.Yellow, 1F);
            Game1.spriteBatch.DrawLine(currentResolution.X, currentResolution.Y - cc, currentResolution.X, currentResolution.Y, Color.Yellow, 1F);

            //BottomLeft
            Game1.spriteBatch.DrawLine(0, currentResolution.Y, cc, currentResolution.Y, Color.Yellow, 1F);
            Game1.spriteBatch.DrawLine(1, currentResolution.Y - cc, 1, currentResolution.Y, Color.Yellow, 1F);

            //TopLeft
            Game1.spriteBatch.DrawLine(0, 1, cc, 1, Color.Yellow, 1F);
            Game1.spriteBatch.DrawLine(1, 0, 1, cc, Color.Yellow, 1F);

            //TopRight
            Game1.spriteBatch.DrawLine(currentResolution.X - cc, 1, currentResolution.X, 1, Color.Yellow, 1F);
            Game1.spriteBatch.DrawLine(currentResolution.X, 0, currentResolution.X, cc, Color.Yellow, 1F);*/
#endif
            base.EndDraw();
        }

        public override void OnSetAsCurrentState()
        {
            base.OnSetAsCurrentState();
            menu.Reset();

            InputManager.CaptureMouse = false;
            InputManager.HideMouseWhenNotUsed = false;
        }
    }
}
