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
    public class NextLevel : State
    {
        private float nextLevelTimer = 0.0f;
        private float nextLevelWait = 6.0f;
        private const float okActionWait = 0.54F;
        private ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.A, KeyAction.Pressed) });

        public override void Initialize()
        {
            base.Initialize();
            SoundsManager.PauseMusic();
            SoundsManager.PlayPowerUp();
            titleScreen = Game1.Get.Content.Load<Texture2D>(@"Textures\TitleScreen");
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (GameManager.lastPlayedLevel == 3 || GameManager.lastPlayedLevel == 0)
            {
                StateManager.CreateAndSetState<States.GameOver>();
                nextLevelTimer = 0.0f;
            }

            nextLevelTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (nextLevelTimer > nextLevelWait || (nextLevelTimer > okActionWait && enterAction.CheckBindings()))
            {
                if (GameManager.lastPlayedLevel == 1)
                    StateManager.CreateAndSetState<States.Level2, States.Pause>();
                else if (GameManager.lastPlayedLevel == 2)
                    StateManager.CreateAndSetState<States.Level3, States.Pause>();
            }
        }
        public override void Draw()
        {
            base.Draw();
            float stringScale = 0.79F;
            string stringToDraw;
            Vector2 StringSize;

            if (GameManager.TimeLeft > 0)
            {
                stringToDraw = "Enemies Destroyed";
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (135 * Game1.resolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else
            {
                stringToDraw = "Survived Time";
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (135 * Game1.resolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "L O A D I N G   N E X T   L E V E L";
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (89 * Game1.resolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            stringToDraw = "( H A R D E R )";
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (43 * Game1.resolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
