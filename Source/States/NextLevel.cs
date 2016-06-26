using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
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
    public class NextLevel : TransactionGUIState
    {
        public float TimeLeft; //Temp fix
        public int lastPlayedLevel; //Temp fix

        public override void Initialize()
        {
            base.Initialize();
            SoundsManager.PauseMusic();
            SoundsManager.PlayPowerUp();
            background = Game1.Get.Content.Load<Texture2D>(@"Textures\TitleScreen");
        }

        protected override void LoadNextState()
        {
            if (GameInstance.lastPlayedLevel == 1)
                StateManager.CreateAndSetState<States.Level2, States.Pause>();
            else if (GameInstance.lastPlayedLevel == 2)
                StateManager.CreateAndSetState<States.Level3, States.Pause>();
        }

        public override void Draw()
        {
            base.Draw();
            float stringScale = 0.79F;
            string stringToDraw;
            Vector2 stringSize;

            if (TimeLeft > 0)
            {
                stringToDraw = "Enemies Destroyed";
                stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (135 * Game1.resolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else
            {
                stringToDraw = "Survived Time";
                stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(
                    Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (135 * Game1.resolutionScale)),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "L O A D I N G   N E X T   L E V E L";
            stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (89 * Game1.resolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            stringToDraw = "( H A R D E R )";
            stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(
                Game1.defaultFont,
                stringToDraw,
                new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), (Game1.curResolutionY / 2.0f) - (43 * Game1.resolutionScale)),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
