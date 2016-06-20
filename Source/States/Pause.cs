using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using InputManagement;

namespace AbstractShooter.States
{
    public class Pause : State
    {
        private float pauseTime = 0.0f;
        private ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Enter, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });
        private ActionBinding exitAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Y, KeyAction.Pressed) });

        public override void Initialize()
        {
            base.Initialize();
            titleScreen = Game1.Get.Content.Load<Texture2D>(@"Textures\PauseScreen");
            SoundsManager.PauseMusic();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            pauseTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // * GameManager.TimeScale;
            if (pauseTime > 0.54)
            {
                if (enterAction.CheckBindings())
                {
                    StateManager.Unpause();
                    SoundsManager.ResumeMusic();
                }
                else if (exitAction.CheckBindings())
                {
                    StateManager.CreateAndSetState<States.MainMenuState>();
                }
            }
        }
        public override void Draw()
        {
            base.Draw();
            Game1.spriteBatch.Draw(titleScreen, new Rectangle(0, 0, Game1.curResolutionX, Game1.curResolutionY), Color.White);
        }
    }
}