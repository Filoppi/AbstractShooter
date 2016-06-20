using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InputManagement;

namespace AbstractShooter.States
{
    public abstract class MenuState : State
    {
        //Delay between two button pressing
        private float MoveTimer = 0.1545F;
        private float MoveTime = 0.1545F;
        protected Menu menu;
        private ActionBinding downAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Down, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadDown, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickDown, KeyAction.Pressed) });
        private ActionBinding upAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Up, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadUp, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickUp, KeyAction.Pressed) });
        private ActionBinding rightAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Right, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadRight, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickRight, KeyAction.Pressed) });
        private ActionBinding leftAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Left, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.DPadLeft, KeyAction.Pressed), new KeyBinding<Buttons>(Buttons.LeftThumbstickLeft, KeyAction.Pressed) });
        private ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Enter, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.A, KeyAction.Pressed) });
        private ActionBinding backAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.B, KeyAction.Pressed) });
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (backAction.CheckBindings())
            {
                if (menu.Exit())
                {
                    //MoveTimer = MoveTime;
                    SoundsManager.PlaySelection();
                }
            }
            else if (enterAction.CheckBindings())
            {
                if (menu.Enter())
                {
                    //MoveTimer = MoveTime * 1.8F;
                    SoundsManager.PlaySelection();
                }
            }
            else if (MoveTimer <= 0)
            {
                if (downAction.CheckBindings())
                {
                    if (menu.Next())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if (upAction.CheckBindings())
                {
                    if (menu.Previous())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if (rightAction.CheckBindings())
                {
                    if (menu.SubNext())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
                else if (leftAction.CheckBindings())
                {
                    if (menu.SubPrevious())
                    {
                        MoveTimer = MoveTime;
                        SoundsManager.PlaySelection();
                    }
                }
            }

            if (MoveTimer > 0)
                MoveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public override void Draw()
        {
            base.Draw();
            Game1.spriteBatch.Draw(
                titleScreen,
                new Rectangle(0, 0, Game1.curResolutionX, Game1.curResolutionY),
                Color.White);

            menu.Draw();
        }
    }
}
