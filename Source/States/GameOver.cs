﻿using System;
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
    public class GameOver : TransactionGUIState
    {
        public override void Initialize()
        {
            base.Initialize();
            SoundsManager.PauseMusic();
            SoundsManager.PlaySpawn();
            background = Game1.Get.Content.Load<Texture2D>(@"Textures\TitleScreen");
        }

        protected override void LoadNextState()
        {
            StateManager.CreateAndSetState<States.MainMenuState>();
        }

        public override void Draw()
        {
            base.Draw();
            float stringScale = 0.895F;
            string stringToDraw = "G A M E    O V E R !";
            Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            Game1.spriteBatch.DrawString(
            Game1.defaultFont,
            stringToDraw,
            new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), ((Game1.curResolutionY / 2.0f) - ((stringSize.Y / 2F) * Game1.resolutionScale))),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
        }
    }
}
