using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AbstractShooter
{
    class Player : Character
    {
        #region Initialization
        private Rectangle baseInitialFrame = new Rectangle(0, 96, 32, 32);
        //It's a glow that the player spaceship has
        private Sprite ObjectTransparent;
        private float ObjectSpeedOriginal;
        private Vector2 LatestSpawnPosition;
        public float Invincibility = 0F;
        private bool PlayShootSound = true;
        private bool isMineUp = false;
        private bool fireMine = false;

        public Player(Vector2 worldLocation)
        {
            ObjectBase = new Sprite(
                worldLocation,
                1.0f,
                StateManager.State.spriteSheet,
                baseInitialFrame,
                Vector2.Zero);
            baseInitialFrame.X += 32;

            ObjectTransparent = new Sprite(
                worldLocation,
                1.0f,
                StateManager.State.spriteSheet,
                new Color(66, 66, 66, 0),
                baseInitialFrame,
                Vector2.Zero);

            ObjectSpeed = 390;
            ObjectSpeedOriginal = 390;

            Invincibility = 0F;
            LatestSpawnPosition = worldLocation;
            Lives = 3;
            isMineUp = false;
            fireMine = false;
        }
        #endregion
        
        #region Input Handling
        private Vector2 handleKeyboardMovement(KeyboardState keyState)
        {
            Vector2 keyMovement = Vector2.Zero;
            if (keyState.IsKeyDown(Keys.W))
                keyMovement.Y--;

            if (keyState.IsKeyDown(Keys.A))
                keyMovement.X--;

            if (keyState.IsKeyDown(Keys.S))
                keyMovement.Y++;

            if (keyState.IsKeyDown(Keys.D))
                keyMovement.X++;

            return keyMovement;
        }
        private Vector2 handleGamePadMovement(GamePadState gamepadState)
        {
            return new Vector2(gamepadState.ThumbSticks.Left.X, -gamepadState.ThumbSticks.Left.Y);
        }
        private Vector2 handleKeyboardShots(KeyboardState keyState)
        {
            Vector2 keyShots = Vector2.Zero;

            if (keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Left))
                keyShots = new Vector2(-1, 1);
            else if (keyState.IsKeyDown(Keys.Down) && keyState.IsKeyDown(Keys.Right))
                keyShots = new Vector2(1, 1);
            else if (keyState.IsKeyDown(Keys.Left) && keyState.IsKeyDown(Keys.Up))
                keyShots = new Vector2(-1, -1);
            else if (keyState.IsKeyDown(Keys.Right) && keyState.IsKeyDown(Keys.Up))
                keyShots = new Vector2(1, -1);
            else if (keyState.IsKeyDown(Keys.Down))
                keyShots = new Vector2(0, 1);
            else if (keyState.IsKeyDown(Keys.Left))
                keyShots = new Vector2(-1, 0);
            else if (keyState.IsKeyDown(Keys.Right))
                keyShots = new Vector2(1, 0);
            else if (keyState.IsKeyDown(Keys.Up))
                keyShots = new Vector2(0, -1);

            return keyShots;
        }
        private Vector2 handleGamePadShots(GamePadState gamepadState)
        {
            return new Vector2(gamepadState.ThumbSticks.Right.X, -gamepadState.ThumbSticks.Right.Y);
        }
        private Vector2 handleMouseShots(MouseState mouseState)
        {
            return new Vector2(mouseState.Position.X - (Game1.CurResolutionX / 2), mouseState.Position.Y - (Game1.CurResolutionY / 2));
        }
        private bool handleMineShots(GamePadState gamepadState, KeyboardState keyState, MouseState mouseState)
        {
            fireMine = false;
            if (gamepadState.Triggers.Right > 0.1f || mouseState.RightButton  == ButtonState.Pressed)
            {
                fireMine = true;
            }
            else if (keyState.IsKeyUp(Keys.Space))
            {
                isMineUp = true;
            }
            else if (isMineUp && keyState.IsKeyDown(Keys.Space))
            {
                fireMine = true;
                isMineUp = false;
            }
            else if (keyState.IsKeyDown(Keys.Space))
            {
                isMineUp = false;
            }
            return fireMine;
        }
        private bool handleMineTrigger(GamePadState gamepadState, KeyboardState keyState)
        {
            return (gamepadState.Triggers.Left > 0.1f || keyState.IsKeyDown(Keys.LeftControl));
        }
        private void handleTurbo(GamePadState gamepadState, KeyboardState keyState)
        {
            if ((gamepadState.Buttons.RightShoulder == ButtonState.Pressed) ||
                (keyState.IsKeyDown(Keys.LeftShift)))
            {
                WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal * 1.24f;
                ObjectSpeed = ObjectSpeedOriginal * 1.77f;
            }
            else
            {
                WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal;
                ObjectSpeed = ObjectSpeedOriginal;
            }
        }
        private void handleInput(GameTime gameTime)
        {
            handleTurbo(GamePad.GetState(PlayerIndex.One), Keyboard.GetState());

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;

            Vector2 moveAngle = Vector2.Zero;
            Vector2 fireAngle = Vector2.Zero;

            moveAngle += handleKeyboardMovement(Keyboard.GetState());
            moveAngle += handleGamePadMovement(GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular));

            fireAngle += handleKeyboardShots(Keyboard.GetState());
            fireAngle += handleGamePadShots(GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular));
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                fireAngle = handleMouseShots(Mouse.GetState());

            if (moveAngle != Vector2.Zero)
            {
                moveAngle.Normalize();
                ObjectAngle = moveAngle;
                moveAngle = checkTileObstacles(elapsed, moveAngle);
                //moveAngle.Normalize();
                if (WeaponsAndFireManager.CanFireSmog)
                {
                    WeaponsAndFireManager.FireSmog(ObjectBase.WorldLocation, ObjectAngle);
                }
            }
            
            ObjectBase.RotateTo(ObjectAngle);
            ObjectBase.Velocity = moveAngle * ObjectSpeed;
            
            if (fireAngle != Vector2.Zero)
            {
                fireAngle.Normalize();

                if (WeaponsAndFireManager.CanFireWeapon)
                {
                    WeaponsAndFireManager.FireWeapon(ObjectBase.WorldLocation, fireAngle, ObjectBase.Velocity);

                    //Playing once out of two times
                    if (PlayShootSound)
                    {
                        SoundsManager.PlayShoot();
                        PlayShootSound = false;
                    }
                    else
                    {
                        PlayShootSound = true;
                    }
                }
            }

            if (handleMineShots(GamePad.GetState(PlayerIndex.One), Keyboard.GetState(), Mouse.GetState()))
            {
                if (WeaponsAndFireManager.CanFireMine)
                {
                    WeaponsAndFireManager.FireMine(ObjectBase.WorldLocation, ObjectSpeed != ObjectSpeedOriginal ? ObjectBase.Velocity * 0.85F : Vector2.Zero);
                    SoundsManager.PlayShootMine();
                }
            }
            else if (handleMineTrigger(GamePad.GetState(PlayerIndex.One), Keyboard.GetState()))
            {
                WeaponsAndFireManager.TriggerMines();
            }
        }
        #endregion

        #region Movements
        public void ResetLocation()
        {
            foreach (Enemy enemy in ObjectManager.Enemies)
            {
                if (!enemy.Destroyed)
                {
                    if (ObjectBase.IsCircleColliding(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.CollisionRadius * 5.75f))
                    {
                        EffectsManager.AddFlakesEffect(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 1, Color.White);
                        EffectsManager.AddExplosion(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 1, enemy.GetColor());

                        enemy.Destroyed = true;
                        GameManager.addScore(0);
                    }
                }
            }
            ObjectBase.WorldLocation = LatestSpawnPosition;
            Invincibility = 187;
        }
        private void ClampToWorld()
        {
            float currentX = ObjectBase.WorldLocation.X;
            float currentY = ObjectBase.WorldLocation.Y;

            currentX = MathHelper.Clamp(
                currentX,
                0,
                Camera.WorldRectangle.Right - ObjectBase.FrameWidth);

            currentY = MathHelper.Clamp(
                currentY,
                0,
                Camera.WorldRectangle.Bottom - ObjectBase.FrameHeight);

            ObjectBase.WorldLocation = new Vector2(currentX, currentY);
        }
        private Vector2 checkTileObstacles(float elapsedTime, Vector2 moveAngle)
        {
            Vector2 newHorizontalLocation = ObjectBase.WorldLocation +
                (new Vector2(moveAngle.X, 0) * (ObjectSpeed * elapsedTime));

            Vector2 newVerticalLocation = ObjectBase.WorldLocation +
                (new Vector2(0, moveAngle.Y) * (ObjectSpeed * elapsedTime));

            Rectangle newHorizontalRect = new Rectangle(
                (int)newHorizontalLocation.X,
                (int)ObjectBase.WorldLocation.Y,
                ObjectBase.FrameWidth,
                ObjectBase.FrameHeight);

            Rectangle newVerticalRect = new Rectangle(
                (int)ObjectBase.WorldLocation.X,
                (int)newVerticalLocation.Y,
                ObjectBase.FrameWidth,
                ObjectBase.FrameHeight);

            int horizLeftPixel = 0;
            int horizRightPixel = 0;

            int vertTopPixel = 0;
            int vertBottomPixel = 0;

            if (moveAngle.X < 0)
            {
                horizLeftPixel = (int)newHorizontalRect.Left;
                horizRightPixel = (int)ObjectBase.WorldRectangle.Left;
            }

            if (moveAngle.X > 0)
            {
                horizLeftPixel = (int)ObjectBase.WorldRectangle.Right;
                horizRightPixel = (int)newHorizontalRect.Right;
            }

            if (moveAngle.Y < 0)
            {
                vertTopPixel = (int)newVerticalRect.Top;
                vertBottomPixel = (int)ObjectBase.WorldRectangle.Top;
            }

            if (moveAngle.Y > 0)
            {
                vertTopPixel = (int)ObjectBase.WorldRectangle.Bottom;
                vertBottomPixel = (int)newVerticalRect.Bottom;
            }

            if (moveAngle.X != 0)
            {
                for (int x = horizLeftPixel; x < horizRightPixel; x++)
                {
                    for (int y = 0; y < ObjectBase.FrameHeight; y++)
                    {
                        if (TileMap.IsWallTileByPixel(
                            new Vector2(x, newHorizontalLocation.Y + y)))
                        {
                            moveAngle.X = 0;
                            break;
                        }
                    }
                    if (moveAngle.X == 0)
                    {
                        break;
                    }
                }
            }

            if (moveAngle.Y != 0)
            {
                for (int y = vertTopPixel; y < vertBottomPixel; y++)
                {
                    for (int x = 0; x < ObjectBase.FrameWidth; x++)
                    {
                        if (TileMap.IsWallTileByPixel(
                            new Vector2(newVerticalLocation.X + x, y)))
                        {
                            moveAngle.Y = 0;
                            break;
                        }
                    }
                    if (moveAngle.Y == 0)
                    {
                        break;
                    }
                }
            }

            return moveAngle;
        }
        #endregion

        #region Update and Draw
        
        public override void Update(GameTime gameTime)
        {
            if (Invincibility > 0)
                Invincibility -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * GameManager.TimeScale;
            handleInput(gameTime);
            base.Update(gameTime);
            ObjectTransparent.WorldLocation = ObjectBase.WorldLocation;
            ObjectTransparent.Rotation = ObjectBase.Rotation;
            //Sets the camera so that the player is in its centre
            //Camera.PositionD = new Vector2(this.ObjectBase.WorldCenter.X - (Game1.MinResolutionX / 2.0f), this.ObjectBase.WorldCenter.Y - (Game1.MinResolutionY / 2.0f));
            //Camera.Position = new Vector2(this.ObjectBase.WorldCenter.X - (Game1.CurResolutionX / 2.0f), this.ObjectBase.WorldCenter.Y - (Game1.CurResolutionY / 2.0f));
            Camera.Position = new Vector2(ObjectBase.WorldCenter.X - (Camera.ViewPortWidth / 2.0f), ObjectBase.WorldCenter.Y - (Camera.ViewPortHeight / 2.0f));
        }
        public override void Draw()
        {
            ObjectTransparent.Draw();

            //Makes the blinking effect when invincible
            if ((Invincibility <= 0) ||
                (Invincibility > 15 && Invincibility < 30) ||
                (Invincibility > 45 && Invincibility < 60) ||
                (Invincibility > 75 && Invincibility < 90) ||
                (Invincibility > 105 && Invincibility < 120) ||
                (Invincibility > 135 && Invincibility < 150) ||
                (Invincibility > 165 && Invincibility < 173) ||
                Invincibility > 181)
            {
                base.Draw();
            }
        }
        #endregion
    }
}
