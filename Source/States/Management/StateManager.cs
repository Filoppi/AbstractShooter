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

namespace AbstractShooter
{
    public static class StateManager
    {
        public static State State = null;
        private static State NextState = null;
        private static State prePauseState = null;

        private static GraphicsDevice graphicsDevice = null;
        public static SpriteBatch spriteBatch = null;
        public static ContentManager contentManager = null;

        public static void Init(GraphicsDevice g, ContentManager c)
        {
            graphicsDevice = g;
            contentManager = c;

            spriteBatch = new SpriteBatch(graphicsDevice);

            SoundsManager.Initialize();
            prePauseState = null;
        }

        public static void SetState(State NewState)
        {
            NextState = NewState;
            prePauseState = null;
        }

        public static void Pause(State NewState)
        {
            //Store The previous state so to restore it later
            prePauseState = State;
            State = NewState;
            State.Init(graphicsDevice, contentManager);
        }
        public static void Unpause()
        {
            //Restores the previous state
            State = prePauseState;
            prePauseState = null;
        }

        public static void Update(GameTime gameTime)
        {
            if (NextState != null)
            {
                State = NextState;
                NextState = null;

                if (State != null)
                {
                    State.Init(graphicsDevice, contentManager);
                }
            }

            if (State != null)
            {
                State.Update(gameTime);
            }
        }

        public static void Draw(GameTime gameTime)
        {
            if (State != null)
            {
                try
                {
                    spriteBatch.Begin();
                    State.Draw(gameTime, graphicsDevice, contentManager);
                    spriteBatch.End();
                }
                catch (Exception e)
                {
#if (DEBUG)
                Console.WriteLine(e.ToString());
#endif
                    spriteBatch.End();
                }
            }
        }
    }
}
