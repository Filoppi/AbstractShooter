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
    public class GameOver : State
    {
        private float gameOverTimer = 0F;
        private const float okActionWait = 0.54F;
        private const float gameOverWait = 4.56F;
        private ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.A, KeyAction.Pressed) });

        public override void Initialize()
        {
            base.Initialize();
            SoundsManager.PauseMusic();
            SoundsManager.PlaySpawn();
            titleScreen = Game1.Get.Content.Load<Texture2D>(@"Textures\TitleScreen");
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            gameOverTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (gameOverTimer > gameOverWait || (gameOverTimer > okActionWait && enterAction.CheckBindings()))
            {
                StateManager.CreateAndSetState<States.MainMenuState>();
            }
        }
        public override void Draw()
        {
            base.Draw();
            float stringScale = 0.895F;
            string stringToDraw = "G A M E    O V E R !";
            Vector2 StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            Game1.spriteBatch.DrawString(
            Game1.defaultFont,
            stringToDraw,
            new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), ((Game1.curResolutionY / 2.0f) - ((StringSize.Y / 2F) * Game1.resolutionScale))),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
