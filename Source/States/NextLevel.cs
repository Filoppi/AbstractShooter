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
    public class NextLevel : State
    {
        private float nextLevelTimer;
        private float nextLevelWait;

        public override void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            SoundsManager.PauseMusic();
            SoundsManager.PlayPowerUp();
            nextLevelTimer = 0.0f;
            nextLevelWait = 6.0f;
            titleScreen = content.Load<Texture2D>(@"Textures\TitleScreen");
        }
        public override void Update(GameTime gameTime)
        {
            if (GameManager.lastPlayedLevel == 3 || GameManager.lastPlayedLevel == 0)
            {
                StateManager.SetState(new States.GameOver());
                nextLevelTimer = 0.0f;
            }

            nextLevelTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (nextLevelTimer > nextLevelWait)
            {
                if (GameManager.lastPlayedLevel == 1)
                    StateManager.SetState(new States.Level2());
                else if (GameManager.lastPlayedLevel == 2)
                    StateManager.SetState(new States.Level3());
                nextLevelTimer = 0.0f;
            }
            if (nextLevelTimer > 0.54 && ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) ||
                (Keyboard.GetState().IsKeyDown(Keys.Space))))
            {
                if (GameManager.lastPlayedLevel == 1)
                    StateManager.SetState(new States.Level2());
                else if (GameManager.lastPlayedLevel == 2)
                    StateManager.SetState(new States.Level2());
                nextLevelTimer = 0.0f;
            }
        }
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            float stringScale = 0.79F;
            string stringToDraw;
            Vector2 StringSize;

            if (GameManager.TimeLeft > 0)
            {
                stringToDraw = "Enemies Destroyed";
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                StateManager.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), (Game1.CurResolutionY / 2.0f) - (135 * Game1.ResolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else
            {
                stringToDraw = "Survived Time";
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                StateManager.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), (Game1.CurResolutionY / 2.0f) - (135 * Game1.ResolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "L O A D I N G   N E X T   L E V E L";
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            StateManager.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), (Game1.CurResolutionY / 2.0f) - (89 * Game1.ResolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            stringToDraw = "( H A R D E R )";
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            StateManager.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), (Game1.CurResolutionY / 2.0f) - (43 * Game1.ResolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
