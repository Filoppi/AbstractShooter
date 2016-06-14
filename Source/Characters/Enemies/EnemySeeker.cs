using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class EnemySeeker : Enemy
    {
        private static Random rand = new Random();
        private Vector2 OffSet;
        private Vector2 OffSetTemp;
        private Vector2 OffSetCurrent = Vector2.Zero;
        private int type;
        private float baseAngle;
        private float rotationSpeed;
        private bool goingUp = true;
        private int FollowingPlayerN;
        private bool surroundingPlayer = true;
        private float SinusoidalRotation;

        public EnemySeeker(Vector2 worldLocation, int newType, int newSpeed)
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

            //Decide if this enemy is going to try to surround the player
            if (rand.Next(0, 4) == 3)
                surroundingPlayer = false;
            else
                surroundingPlayer = true;

            //Surrounding max angle
            OffSet = new Vector2(0, (float)rand.Next(2, 7));
            OffSetCurrent = OffSet;

            ObjectSpeed = newSpeed * ((float)rand.Next(49, 305) / 100.0f);

            //Rotate of random amount
            baseAngle = (float)Math.Atan2(1, 0);
            rotationSpeed = (float)rand.Next(-400, 400) / 1.666f;

            FollowingPlayerN = rand.Next(0, GameManager.GetNOfPlayers());

            ObjectBase.Animate = true;
            ObjectBase.Reanimate = true;

            ObjectBase.scale = 1.135f;

            behaves.Add(new ScalingBehave(this));
            
            goingUp = true;
            SinusoidalRotation = 0.04f;
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
        private new Vector2 determineMoveDirection()
        {
            return ObjectManager.Players[FollowingPlayerN].ObjectBase.WorldCenter - ObjectBase.WorldCenter;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Destroyed)
            {
                ObjectAngle = determineMoveDirection();
                ObjectAngle.Normalize();

                foreach (CharacterBehave behave in behaves)
                {
                    behave.Update(gameTime);
                }

                //Make enemies travel with a Sinusoidal wave
                if (surroundingPlayer)
                {
                    if (goingUp)
                        OffSetCurrent = new Vector2(0, OffSetCurrent.Y + (SinusoidalRotation * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale) * 60F);
                    else
                        OffSetCurrent = new Vector2(0, OffSetCurrent.Y - (SinusoidalRotation * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale) * 60F);

                    if (OffSetCurrent.Y > OffSet.Y)
                    {
                        goingUp = false;
                    }
                    else if (OffSetCurrent.Y < -OffSet.Y)
                    {
                        goingUp = true;
                    }

                    OffSetTemp = OffSetCurrent;
                    OffSetTemp.Normalize();
                    Vector2 OffSetTempTemp = OffSetTemp;
                    OffSetTempTemp.X = Math.Abs(OffSetTempTemp.X);
                    OffSetTempTemp.Y = Math.Abs(OffSetTempTemp.Y);

                    //Calculates angle
                    float Angle = (float)Math.Atan2(OffSetTempTemp.Y - ObjectAngle.Y, OffSetTempTemp.X - ObjectAngle.X);
                    
                    //Fix angle 
                    Angle *= 2.0f;
                    Angle -= 3.14f / 2.0f;

                    //Rotate vector correctly 
                    Vector2 v = OffSetTemp;
                    float sin = (float)Math.Sin(Angle);
                    float cos = (float)Math.Cos(Angle);
                    float tx = v.X;
                    float ty = v.Y;
                    v.X = (cos * tx) - (sin * ty);
                    v.Y = (sin * tx) + (cos * ty);
                    OffSetTemp.X = v.X;
                    OffSetTemp.Y = v.Y;

                    //Add Sinusoidal to normal movement 
                    OffSetTemp.Normalize();
                    ObjectAngle += OffSetTemp;
                    ObjectAngle.Normalize();
                }

                ObjectBase.Velocity = ObjectAngle * ObjectSpeed;

                //Rotates the angle
                baseAngle -= MathHelper.ToRadians(rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale);
                ObjectBase.RotateTo(new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)));

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