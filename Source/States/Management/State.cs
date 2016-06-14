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
    public abstract class State
    {
        public Texture2D spriteSheet;
        public Texture2D titleScreen;
        public virtual void Init(GraphicsDevice graphicsDevice, ContentManager content) { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content) { }
    }
}
