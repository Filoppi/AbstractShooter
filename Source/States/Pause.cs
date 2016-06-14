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

namespace AbstractShooter.States
{
    public class Pause : State
    {
        private float pauseTime;

        public override void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            titleScreen = content.Load<Texture2D>(@"Textures\PauseScreen");
            pauseTime = 0.0f;
            SoundsManager.PauseMusic();
        }
        public override void Update(GameTime gameTime)
        {
            pauseTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // * GameManager.TimeScale;
            if (pauseTime > 0.54)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    StateManager.Unpause();
                    SoundsManager.ResumeMusic();
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    StateManager.Unpause();
                    StateManager.SetState(new States.Menu());
                }
            }
        }
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            StateManager.spriteBatch.Draw(titleScreen, new Rectangle(0, 0, Game1.CurResolutionX, Game1.CurResolutionY), Color.White);
        }
    }
}