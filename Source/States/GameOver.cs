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
    public class GameOver : State
    {
        private float gameOverTimer;
        private float gameOverWait;

        public override void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            SoundsManager.PauseMusic();
            SoundsManager.PlaySpawn();
            gameOverTimer = 0.0f;
            gameOverWait = 4.56f;
            titleScreen = content.Load<Texture2D>(@"Textures\TitleScreen");
            //font1 = content.Load<SpriteFont>(@"Fonts\Font1");
        }
        public override void Update(GameTime gameTime)
        {
            gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds; // * GameManager.TimeScale;
            if (gameOverTimer > gameOverWait)
            {
                StateManager.SetState(new States.Menu());
                gameOverTimer = 0.0f;
            }
            if (gameOverTimer > 0.54 && ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) ||
                (Keyboard.GetState().IsKeyDown(Keys.Space))))
            {
                StateManager.SetState(new States.Menu());
                gameOverTimer = 0.0f;
            }
        }
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            float stringScale = 0.895F;
            string stringToDraw = "G A M E    O V E R !";
            Vector2 StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            StateManager.spriteBatch.DrawString(
            Game1.defaultFont,
            stringToDraw,
            new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), ((Game1.CurResolutionY / 2.0f) - ((StringSize.Y / 2F) * Game1.ResolutionScale))),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
