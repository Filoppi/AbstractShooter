using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        protected ActionBinding backAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.B, KeyAction.Pressed) });

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (backAction.CheckBindings())
            {
                if (menu.Exit())
                {
                    SoundsManager.PlaySelection();
                }
            }
            else if (enterAction.CheckBindings())
            {
                if (menu.Enter())
                {
                    SoundsManager.PlaySelection();
                }
            }
            else if (downAction.CheckBindings())
            {
                if (menu.Next())
                {
                    SoundsManager.PlaySelection();
                }
            }
            else if (upAction.CheckBindings())
            {
                if (menu.Previous())
                {
                    SoundsManager.PlaySelection();
                }
            }
            else if (rightAction.CheckBindings())
            {
                if (menu.SubNext())
                {
                    SoundsManager.PlaySelection();
                }
            }
            else if (leftAction.CheckBindings())
            {
                if (menu.SubPrevious())
                {
                    SoundsManager.PlaySelection();
                }
            }
        }

        public override void EndDraw()
        {
            menu.Draw();

            base.EndDraw();
        }

        public override void OnSetAsCurrentState()
        {
            base.OnSetAsCurrentState();
            menu.Reset();
        }
    }
}
