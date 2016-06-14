using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class Enemy : Character
    {
        public bool Destroyed = false;

        protected virtual void CollidedWithWorldBorders() { }
        protected void ClampToWorld()
        {
            float currentX = ObjectBase.WorldLocation.X;
            float currentY = ObjectBase.WorldLocation.Y;

            //To Update numbers...
            if (currentX < 42 * ObjectBase.scale)
            {
                currentX = 42 * ObjectBase.scale;
                CollidedWithWorldBorders();
            }
            else if (currentX > GameManager.LevelDimensionX - 32 - (42 * ObjectBase.scale))
            {
                currentX = GameManager.LevelDimensionX - 32 - (42 * ObjectBase.scale);
                CollidedWithWorldBorders();
            }
            if (currentY < 42 * ObjectBase.scale)
            {
                currentY = 42 * ObjectBase.scale;
                CollidedWithWorldBorders();
            }
            else if (currentY > GameManager.LevelDimensionY - 32 - (42 * ObjectBase.scale))
            {
                currentY = GameManager.LevelDimensionY - 32 - (42 * ObjectBase.scale);
                CollidedWithWorldBorders();
            }

            ObjectBase.WorldLocation = new Vector2(currentX, currentY);
        }
    }
}