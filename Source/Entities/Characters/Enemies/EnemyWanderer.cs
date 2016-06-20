using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class AEnemyWanderer : AEnemy
    {
        private static Random rand = new Random();
        private int type;
        private float baseAngle;
        private float rotationSpeed;
        private int FollowingPlayerN;

        public AEnemyWanderer(Vector2 worldLocation, int newType, int newSpeed)
            : base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 250, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Characters, worldLocation, true)
        {
            type = newType;
            Lives = 5;
            if (type == 6) //Pink Circle
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(0, 250, 32, 32) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(80, 125) / 100.0f);
                Lives = 7;
            }
            else if (type == 5) //Orange Pinwheel
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(34, 288, 32, 32) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(74, 126) / 100.0f);
            }
            else if (type == 4) //Violet Star
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(0, 288, 32, 32) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(74, 126) / 100.0f);
                Lives = 3;
            }
            else if (type == 3) //Yellow Triangle
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(8, 231, 18, 18) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(76, 128) / 100.0f);
                Lives = 1;
            }
            else if (type == 2) //Green Square
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(0, 194, 32, 32),
                        new Rectangle(32, 194, 32, 32),
                        new Rectangle(64, 194, 32, 32),
                        new Rectangle(96, 194, 32, 32),
                        new Rectangle(128, 194, 32, 32),
                        new Rectangle(160, 194, 32, 32),
                        new Rectangle(192, 194, 32, 32),
                        new Rectangle(192, 194, 32, 32),
                        new Rectangle(160, 194, 32, 32),
                        new Rectangle(128, 194, 32, 32),
                        new Rectangle(96, 194, 32, 32),
                        new Rectangle(64, 194, 32, 32),
                        new Rectangle(32, 194, 32, 32),
                        new Rectangle(0, 194, 32, 32) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(65, 120) / 100.0f);
                spriteComponent.GenerateDefaultAnimation(0.1F);
            }
            else //if (type == 1) //Sky blue diamond
            {
                spriteComponent = new AnimatedSpriteComponent(this,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { new Rectangle(0, 161, 32, 32),
                        new Rectangle(32, 161, 32, 32),
                        new Rectangle(64, 161, 32, 32),
                        new Rectangle(96, 161, 32, 32),
                        new Rectangle(128, 161, 32, 32),
                        new Rectangle(160, 161, 32, 32),
                        new Rectangle(192, 161, 32, 32),
                        new Rectangle(224, 161, 32, 32) },
                    null,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                    worldLocation,
                    true,
                    rand.Next(68, 120) / 100.0f);
                spriteComponent.GenerateDefaultAnimation(0.1F);
            }
            rootComponent = spriteComponent;
            AddComponent(new ScalingBehaveComponent(this));

            ObjectSpeed = newSpeed * ((float)rand.Next(49, 305) / 100.0f);

            //Rotate of random amount
            baseAngle = (float)Math.Atan2(1, 0);
            rotationSpeed = (float)rand.Next(-400, 400) / 1.666f;

            FollowingPlayerN = rand.Next(0, GameManager.GetNOfPlayers());
            
            WorldScale = 1.135f;
            
            determineMoveDirection();
        }

        public override Color GetColor()
        {
            if (type == 6) //Pink Circle
            {
                return new Color(244, 0, 126);
            }
            else if (type == 5) //Orange Pinwheel
            {
                return new Color(255, 101, 0);
            }
            else if (type == 4) //Violet Star
            {
                return new Color(143, 0, 255);
            }
            else if (type == 3) //Yellow Triangle
            {
                return new Color(255, 255, 43);
            }
            else if (type == 2) //Green Square
            {
                return new Color(47, 255, 50);
            }
            //else if (type == 1) //Sky blue diamond
            return new Color(60, 226, 255);
        }
        protected override void CollidedWithWorldBorders()
        {
            determineMoveDirection();
        }
        private new void determineMoveDirection()
        {
            ObjectAngle = new Vector2(rand.Next(-10, 11), rand.Next(-10, 11));
            ObjectAngle.Normalize();
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            rootComponent.velocity = ObjectAngle * ObjectSpeed;

            //Rotates the angle
            baseAngle -= MathHelper.ToRadians(rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale);
            spriteComponent.RotateTo(new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)));
            
            base.UpdateActor(gameTime);

            ClampToWorld();
        }
    }
}