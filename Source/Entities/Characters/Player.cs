using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using InputManagement;
using AbstractShooter.States;

namespace AbstractShooter
{
    public class APlayer : ACharacter
    {
        #region Initialization
        //It's a glow that the player spaceship has
        private float ObjectSpeedOriginal;
        private Vector2 LatestSpawnPosition;
        public float Invincibility = 0F;
        private bool PlayShootSound = true;
        private bool isMineUp = false;
        protected InputMode inputMode = InputModes.KeyboardMouseGamePad1;
        private SpriteComponent transparentBackground; //Addictional sprite that gives a transparent background

        AxisBinding fireAngleX = new AxisBinding(new GamePadAxis[] { GamePadAxis.RightAnalogX }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Up, false), new KeyAxisBinding<Keys>(Keys.Down, true) });
        AxisBinding fireAngleY = new AxisBinding(new GamePadAxis[] { GamePadAxis.RightAnalogY }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Right, true), new KeyAxisBinding<Keys>(Keys.Left, false) });

        public APlayer(Vector2 worldLocation)
            : base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 96, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Players, worldLocation)
        {
            transparentBackground = new SpriteComponent(
                this,
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { new Rectangle(32, 96, 32, 32) },
                rootComponent,
                ComponentUpdateGroup.AfterActor, DrawGroup.Players,
                Vector2.Zero,
                false,
                1F,
                Vector2.Zero,
                0,
                new Color(66, 66, 66, 0));

            ObjectSpeed = 390;
            ObjectSpeedOriginal = 390;

            Invincibility = 0F;
            LatestSpawnPosition = worldLocation;
            Lives = 3;
            isMineUp = false;
        }
        #endregion
        
        #region Input Handling
        private Vector2 handleKeyboardMovement()
        {
            Vector2 keyMovement = Vector2.Zero;
            if (InputManager.currentKeyboardState.IsKeyDown(Keys.W))
                keyMovement.Y--;

            if (InputManager.currentKeyboardState.IsKeyDown(Keys.A))
                keyMovement.X--;

            if (InputManager.currentKeyboardState.IsKeyDown(Keys.S))
                keyMovement.Y++;

            if (InputManager.currentKeyboardState.IsKeyDown(Keys.D))
                keyMovement.X++;

            return keyMovement;
        }
        private Vector2 handleGamePadMovement()
        {
            return new Vector2(InputManager.currentGamePadState[0].ThumbSticks.Left.X, -InputManager.currentGamePadState[0].ThumbSticks.Left.Y);
        }
        private Vector2 handleMouseShots()
        {
            return new Vector2(InputManager.currentMouseState.Position.X - (Game1.curResolutionX / 2), InputManager.currentMouseState.Position.Y - (Game1.curResolutionY / 2));
        }
        private bool handleMineShots()
        {
            bool fireMine = false;
            if (InputManager.currentGamePadState[0].Triggers.Right > 0.1f || InputManager.currentMouseState.RightButton  == ButtonState.Pressed)
            {
                fireMine = true;
            }
            else if (InputManager.currentKeyboardState.IsKeyUp(Keys.Space))
            {
                isMineUp = true;
            }
            else if (isMineUp && InputManager.currentKeyboardState.IsKeyDown(Keys.Space))
            {
                fireMine = true;
                isMineUp = false;
            }
            else if (InputManager.currentKeyboardState.IsKeyDown(Keys.Space))
            {
                isMineUp = false;
            }
            return fireMine;
        }
        private bool handleMineTrigger()
        {
            return (InputManager.currentGamePadState[0].Triggers.Left > 0.1f || InputManager.currentKeyboardState.IsKeyDown(Keys.LeftControl));
        }
        private void handleTurbo()
        {
            if ((InputManager.currentGamePadState[0].Buttons.RightShoulder == ButtonState.Pressed) ||
                (InputManager.currentKeyboardState.IsKeyDown(Keys.LeftShift)))
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
            handleTurbo();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;

            Vector2 moveAngle = Vector2.Zero;
            Vector2 fireAngle = Vector2.Zero;

            if (inputMode.Keyboard && (!inputMode.GamePads[0] || !InputManager.IsUsingGamePad))
            {
                moveAngle = handleKeyboardMovement();
            }
            else
            {
                moveAngle = handleGamePadMovement();
            }
            if (inputMode.Mouse && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                fireAngle = handleMouseShots();
            }
            else
            {
                fireAngle = new Vector2(fireAngleX.CheckBindings(inputMode), -fireAngleY.CheckBindings(inputMode));
            }

            if (moveAngle != Vector2.Zero)
            {
                moveAngle.Normalize();
                ObjectAngle = moveAngle;
                moveAngle = checkTileObstacles(elapsed, moveAngle);
                //moveAngle.Normalize();
                if (WeaponsAndFireManager.CanFireSmog)
                {
                    WeaponsAndFireManager.FireSmog(rootComponent.WorldLocation, ObjectAngle);
                }
            }
            
            spriteComponent.RotateTo(ObjectAngle);
            rootComponent.localVelocity = moveAngle * ObjectSpeed;
            
            if (fireAngle != Vector2.Zero)
            {
                fireAngle.Normalize();

                if (WeaponsAndFireManager.CanFireWeapon)
                {
                    WeaponsAndFireManager.FireWeapon(rootComponent.WorldLocation, fireAngle, rootComponent.localVelocity);

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

            if (handleMineShots())
            {
                if (WeaponsAndFireManager.CanFireMine)
                {
                    WeaponsAndFireManager.FireMine(rootComponent.WorldLocation, ObjectSpeed != ObjectSpeedOriginal ? rootComponent.localVelocity * 0.85F : Vector2.Zero);
                    SoundsManager.PlayShootMine();
                }
            }
            else if (handleMineTrigger())
            {
                WeaponsAndFireManager.TriggerMines();
            }
        }
        #endregion

        #region Movements
        public void ResetLocation()
        {
            foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
            {
                if (spriteComponent.IsCircleColliding(enemy.RootComponent.WorldCenter, enemy.RootComponent.CollisionRadius * 5.75f))
                {
                    EffectsManager.AddFlakesEffect(enemy.RootComponent.WorldCenter, enemy.RootComponent.localVelocity / 1, Color.White);
                    EffectsManager.AddExplosion(enemy.RootComponent.WorldCenter, enemy.RootComponent.localVelocity / 1, enemy.GetColor());

                    enemy.Destroy();
                    ((Level)StateManager.currentState).addScore(0);
                }
            }
            rootComponent.WorldLocation = LatestSpawnPosition;
            Invincibility = 187;
        }
        private void ClampToWorld()
        {
            float currentX = rootComponent.WorldLocation.X;
            float currentY = rootComponent.WorldLocation.Y;

            currentX = MathHelper.Clamp(
                currentX,
                0,
                Camera.WorldRectangle.Right - spriteComponent.FrameWidth);

            currentY = MathHelper.Clamp(
                currentY,
                0,
                Camera.WorldRectangle.Bottom - spriteComponent.FrameHeight);

            rootComponent.WorldLocation = new Vector2(currentX, currentY);
        }
        private Vector2 checkTileObstacles(float elapsedTime, Vector2 moveAngle)
        {
            Vector2 newHorizontalLocation = rootComponent.WorldLocation +
                (new Vector2(moveAngle.X, 0) * (ObjectSpeed * elapsedTime));

            Vector2 newVerticalLocation = rootComponent.WorldLocation +
                (new Vector2(0, moveAngle.Y) * (ObjectSpeed * elapsedTime));

            Rectangle newHorizontalRect = new Rectangle(
                (int)newHorizontalLocation.X,
                (int)rootComponent.WorldLocation.Y,
                spriteComponent.FrameWidth,
                spriteComponent.FrameHeight);

            Rectangle newVerticalRect = new Rectangle(
                (int)rootComponent.WorldLocation.X,
                (int)newVerticalLocation.Y,
                spriteComponent.FrameWidth,
                spriteComponent.FrameHeight);

            int horizLeftPixel = 0;
            int horizRightPixel = 0;

            int vertTopPixel = 0;
            int vertBottomPixel = 0;

            if (moveAngle.X < 0)
            {
                horizLeftPixel = (int)newHorizontalRect.Left;
                horizRightPixel = (int)spriteComponent.WorldRectangle.Left;
            }

            if (moveAngle.X > 0)
            {
                horizLeftPixel = (int)spriteComponent.WorldRectangle.Right;
                horizRightPixel = (int)newHorizontalRect.Right;
            }

            if (moveAngle.Y < 0)
            {
                vertTopPixel = (int)newVerticalRect.Top;
                vertBottomPixel = (int)spriteComponent.WorldRectangle.Top;
            }

            if (moveAngle.Y > 0)
            {
                vertTopPixel = (int)spriteComponent.WorldRectangle.Bottom;
                vertBottomPixel = (int)newVerticalRect.Bottom;
            }

            if (moveAngle.X != 0)
            {
                for (int x = horizLeftPixel; x < horizRightPixel; x++)
                {
                    for (int y = 0; y < spriteComponent.FrameHeight; y++)
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
                    for (int x = 0; x < spriteComponent.FrameWidth; x++)
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

        protected override void UpdateActor(GameTime gameTime)
        {
            if (Invincibility > 0)
                Invincibility -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;
            handleInput(gameTime);

            base.UpdateActor(gameTime);
            //Sets the camera so that the player is in its centre
            //Camera.PositionD = new Vector2(this.rootComponent.WorldCenter.X - (Game1.minResolutionX / 2.0f), this.rootComponent.WorldCenter.Y - (Game1.minResolutionY / 2.0f));
            //Camera.Position = new Vector2(this.rootComponent.WorldCenter.X - (Game1.curResolutionX / 2.0f), this.rootComponent.WorldCenter.Y - (Game1.curResolutionY / 2.0f));
            Camera.Position = new Vector2(rootComponent.WorldCenter.X - (Camera.ViewPortWidth / 2.0f), rootComponent.WorldCenter.Y - (Camera.ViewPortHeight / 2.0f));
        }
        public override void Draw()
        {
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
