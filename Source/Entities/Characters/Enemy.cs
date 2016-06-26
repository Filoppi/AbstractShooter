using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AbstractShooter.States;

namespace AbstractShooter
{
    public class AEnemy : ACharacter
    {
        public AEnemy(Texture2D texture, List<Rectangle> frames,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(texture, frames, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
        {
        }

        protected virtual void CollidedWithWorldBorders() { }
        protected void ClampToWorld()
        {
            float currentX = WorldLocation.X;
            float currentY = WorldLocation.Y;

            //To Update numbers...
            if (currentX < 42 * WorldScale)
            {
                currentX = 42 * WorldScale;
                CollidedWithWorldBorders();
            }
            else if (currentX > ((Level)StateManager.currentState).LevelDimensionX - 32 - (42 * WorldScale))
            {
                currentX = ((Level)StateManager.currentState).LevelDimensionX - 32 - (42 * WorldScale);
                CollidedWithWorldBorders();
            }
            if (currentY < 42 * WorldScale)
            {
                currentY = 42 * WorldScale;
                CollidedWithWorldBorders();
            }
            else if (currentY > ((Level)StateManager.currentState).LevelDimensionY - 32 - (42 * WorldScale))
            {
                currentY = ((Level)StateManager.currentState).LevelDimensionY - 32 - (42 * WorldScale);
                CollidedWithWorldBorders();
            }

            WorldLocation = new Vector2(currentX, currentY);
        }
    }
}