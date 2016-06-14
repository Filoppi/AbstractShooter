using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class EnemyWanderer : Enemy
    {
        private static Random rand = new Random();
        private int type;
        private float baseAngle;
        private float rotationSpeed;
        private int FollowingPlayerN;

        public EnemyWanderer(Vector2 worldLocation, int newType, int newSpeed)
        {
            type = newType;
            Lives = 5;
            if (type == 6) //Pink Circle
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(80, 125) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(0, 250, 32, 32),
                    Vector2.Zero);
                Lives = 7;
            }
            else if (type == 5) //Orange Pinwheel
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(74, 126) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(34, 288, 32, 32),
                    Vector2.Zero);
            }
            else if (type == 4) //Violet Star
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(74, 126) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(0, 288, 32, 32),
                    Vector2.Zero);
                Lives = 3;
            }
            else if (type == 3) //Yellow Triangle
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(76, 128) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(8, 231, 18, 18),
                    Vector2.Zero);
                Lives = 1;
            }
            else if (type == 2) //Green Square
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(65, 120) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(0, 194, 32, 32),
                    Vector2.Zero);
                ObjectBase.AddFrame(new Rectangle(32, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(64, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(96, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(128, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(160, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(192, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(192, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(160, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(128, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(96, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(64, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(32, 194, 32, 32));
                ObjectBase.AddFrame(new Rectangle(0, 194, 32, 32));
            }
            else //if (type == 1) //Sky blue diamond
            {
                ObjectBase = new Sprite(
                    worldLocation,
                    (float)rand.Next(68, 120) / 100.0f,
                    StateManager.State.spriteSheet,
                    new Rectangle(0, 161, 32, 32),
                    Vector2.Zero);
                ObjectBase.AddFrame(new Rectangle(32, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(64, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(96, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(128, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(160, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(192, 161, 32, 32));
                ObjectBase.AddFrame(new Rectangle(224, 161, 32, 32));
            }

            ObjectSpeed = newSpeed * ((float)rand.Next(49, 305) / 100.0f);

            //Rotate of random amount
            baseAngle = (float)Math.Atan2(1, 0);
            rotationSpeed = (float)rand.Next(-400, 400) / 1.666f;

            FollowingPlayerN = rand.Next(0, GameManager.GetNOfPlayers());

            ObjectBase.Animate = true;
            ObjectBase.Reanimate = true;

            ObjectBase.scale = 1.135f;

            behaves.Add(new ScalingBehave(this));

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

        public override void Update(GameTime gameTime)
        {
            if (!Destroyed)
            {
                foreach (CharacterBehave behave in behaves)
                {
                    behave.Update(gameTime);
                }

                ObjectBase.Velocity = ObjectAngle * ObjectSpeed;

                //Rotates the angle
                baseAngle -= MathHelper.ToRadians(rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale);
                ObjectBase.RotateTo(new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)));

                ObjectBase.Update(gameTime);

                base.Update(gameTime);

                ClampToWorld();
            }
        }
        public override void Draw()
        {
            base.Draw();
        }
    }
}