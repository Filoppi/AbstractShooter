using AbstractShooter.States;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
	public class APlayerActor : ACharacterActor
	{
		//It's a glow that the player spaceship has
		private float originalMovingSpeed;

		private Vector2 lastSpawnPosition;
		public float invincibilityTimer = 0F;
		protected InputMode inputMode = InputModes.KeyboardMouseGamePad1;
        private CCameraComponent camera;
		private CSpriteComponent transparentBackground; //Addictional sprite that gives a transparent background
		private CSmogParticleEmitterComponent smokeEmitter;
        private CMineLauncherComponent mineLauncher;
        private CGunComponent gun;

        //To find a way to do something like player controller and pawn (this is a pawn)
        private ActionBinding turboAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.LeftShift, KeyAction.Down) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.RightShoulder, KeyAction.Down) });
		private ActionBinding triggerMineAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.LeftControl, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.LeftTrigger, KeyAction.Pressed) });
		private ActionBinding deployMineAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.RightTrigger, KeyAction.Pressed) }, new KeyBinding<MouseButtons>[] { new KeyBinding<MouseButtons>(MouseButtons.Right, KeyAction.Pressed) });
		private AxisBinding fireDirectionX = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.RightAnalogX, 1F) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Right, 1F), new KeyAxisBinding<Keys>(Keys.Left, -1F) });
		private AxisBinding fireDirectionY = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.RightAnalogY, -1f) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Up, -1F), new KeyAxisBinding<Keys>(Keys.Down, 1F) });
		private AxisBinding movementDirectionX = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.LeftAnalogX, 1F) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.D, 1F), new KeyAxisBinding<Keys>(Keys.A, -1F) });
		private AxisBinding movementDirectionY = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.LeftAnalogY, -1F) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.S, 1F), new KeyAxisBinding<Keys>(Keys.W, -1F) });
        
		public APlayerActor(Vector2 worldLocation)
			: base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 96, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Players, worldLocation)
        {
            camera = new CCameraComponent(
                this,
                rootComponent,
                ComponentUpdateGroup.AfterActor, DrawGroup.Players,
                Vector2.Zero,
                false,
                1F,
                Vector2.Zero,
                0);

            transparentBackground = new CSpriteComponent(
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

			smokeEmitter = new CSmogParticleEmitterComponent(
				this,
				StateManager.currentState.spriteSheet,
				new List<Rectangle> { new Rectangle(32, 96, 32, 32) },
				0.03F, true,
				rootComponent,
				ComponentUpdateGroup.AfterActor, DrawGroup.Players,
				Vector2.Zero,
				false,
				1F,
				Vector2.Zero,
				0);

            mineLauncher = new CMineLauncherComponent(
                this,
                rootComponent,
                ComponentUpdateGroup.AfterActor, DrawGroup.Players,
                Vector2.Zero,
                false,
                1F,
                Vector2.Zero,
                0);

            gun = new CGunComponent(
                this,
                rootComponent,
                ComponentUpdateGroup.AfterActor, DrawGroup.Players,
                Vector2.Zero,
                false,
                1F,
                Vector2.Zero,
                0);

            spriteComponent.collisionGroup = (int)ComponentCollisionGroup.Character;
			spriteComponent.overlappingGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Static;

			movingSpeed = 390;
			originalMovingSpeed = 390;

			invincibilityTimer = 0F;
			lastSpawnPosition = worldLocation;
			Lives = 3;
		}

		private void HandleInput(GameTime gameTime)
		{
			//Turbo //TO1 Temp disabled
			//if (turboAction.CheckBindings(inputMode))
			//{
			//	gun.weaponSpeed = gun.originalWeaponSpeed * 1.24f;
   //             mineLauncher.weaponSpeed = mineLauncher.originalWeaponSpeed * 1.24f;
   //             movingSpeed = originalMovingSpeed * 1.77f;
			//}
			//else
   //         {
   //             gun.weaponSpeed = gun.originalWeaponSpeed;
   //             mineLauncher.weaponSpeed = mineLauncher.originalWeaponSpeed;
			//	movingSpeed = originalMovingSpeed;
			//}

			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;

			Vector2 moveDirection = Vector2.Zero;
			Vector2 fireDirection = Vector2.Zero;

			moveDirection = new Vector2(movementDirectionX.CheckBindings(inputMode), movementDirectionY.CheckBindings(inputMode)); //To clamp to circle ? (controller is clamped but keyboard not...)
			//if (inputMode.Keyboard && (!inputMode.GamePads[0] || !InputManager.IsUsingGamePad))
			//{
			//    moveDirection = handleKeyboardMovement();
			//}
			//else
			//{
			//    moveDirection = handleGamePadMovement();
			//}
			if (inputMode.Mouse && Mouse.GetState().LeftButton == ButtonState.Pressed)
			{
				fireDirection = new Vector2(InputManager.currentMouseState.Position.X - (UnrealMonoGame.currentWindowResolution.X / 2), InputManager.currentMouseState.Position.Y - (UnrealMonoGame.currentWindowResolution.Y / 2));
			}
			else
			{
				fireDirection = new Vector2(fireDirectionX.CheckBindings(inputMode), fireDirectionY.CheckBindings(inputMode));
			}
            
            //StateManager.currentState.TimeScale = moveDirection.Length(); //Temp

            if (moveDirection != Vector2.Zero)
			{
				moveDirection.Normalize();
				movingDirection = moveDirection;
				moveDirection = CheckTileObstacles(elapsed, moveDirection);
				//moveDirection.Normalize();
				//if (WeaponsAndFireManager.CanFireSmog)
				//{
				//	WeaponsAndFireManager.FireSmog(rootComponent.WorldLocation, movingDirection);
				//}
			}

			spriteComponent.RotateTo(movingDirection);
			rootComponent.localVelocity = moveDirection * movingSpeed;

            //To add these actions to a list and activate them on the first frame where TimeScale > 0
            if (StateManager.currentState.TimeScale > 0)
            {
                if (fireDirection != Vector2.Zero)
                {
                    fireDirection.Normalize();
                    gun.Fire(rootComponent.WorldLocation, fireDirection, rootComponent.localVelocity);
                }

                if (deployMineAction.CheckBindings(inputMode))
                {
                    mineLauncher.Fire(rootComponent.WorldLocation, Vector2.Zero, movingSpeed != originalMovingSpeed ? rootComponent.localVelocity * 0.85F : Vector2.Zero);
                }
                else if (triggerMineAction.CheckBindings(inputMode))
                {
                    mineLauncher.TriggerMines();
                }
            }
        }

		public void ResetLocation()
		{
			foreach (AEnemyActor enemy in StateManager.currentState.GetAllActorsOfType<AEnemyActor>())
			{
				foreach (CSpriteComponent spriteComponent in enemy.GetSceneComponentsByClass<CSpriteComponent>())
				{
					if (new Circle(spriteComponent.WorldLocation, spriteComponent.CollisionRadius * 5.75f).IsCollidingWith(enemy.RootComponent.CollisionCircle))
					{
						ParticlesSpawner.AddFlakesEffect(spriteComponent.WorldLocation,
							spriteComponent.localVelocity / 1, Color.White);
						ParticlesSpawner.AddExplosion(spriteComponent.WorldLocation, spriteComponent.localVelocity / 1,
							enemy.GetColor());

						((Level)StateManager.currentState).AddScore(0);
						enemy.Destroy();
						break;
					}
				}
			}
			rootComponent.WorldLocation = lastSpawnPosition;
			invincibilityTimer = 187;
		}

        //To rename
		private Vector2 CheckTileObstacles(float elapsedTime, Vector2 moveDirection)
		{
			Vector2 newHorizontalLocation = spriteComponent.TopLeftLocation + (new Vector2(moveDirection.X, 0) * (movingSpeed * elapsedTime));
			Vector2 newVerticalLocation = spriteComponent.TopLeftLocation + (new Vector2(0, moveDirection.Y) * (movingSpeed * elapsedTime));

			Rectangle newHorizontalRect = new Rectangle(
				(int)newHorizontalLocation.X,
				(int)spriteComponent.TopLeftLocation.Y,
				spriteComponent.FrameWidth,
				spriteComponent.FrameHeight);
			Rectangle newVerticalRect = new Rectangle(
				(int)spriteComponent.TopLeftLocation.X,
				(int)newVerticalLocation.Y,
				spriteComponent.FrameWidth,
				spriteComponent.FrameHeight);

			int horizLeftPixel = 0;
			int horizRightPixel = 0;

			int vertTopPixel = 0;
			int vertBottomPixel = 0;

			if (moveDirection.X < 0)
			{
				horizLeftPixel = (int)newHorizontalRect.Left;
				horizRightPixel = (int)spriteComponent.WorldRectangle.Left;
			}
			else if (moveDirection.X > 0)
			{
				horizLeftPixel = (int)spriteComponent.WorldRectangle.Right;
				horizRightPixel = (int)newHorizontalRect.Right;
			}

			if (moveDirection.Y < 0)
			{
				vertTopPixel = (int)newVerticalRect.Top;
				vertBottomPixel = (int)spriteComponent.WorldRectangle.Top;
			}
			else if (moveDirection.Y > 0)
			{
				vertTopPixel = (int)spriteComponent.WorldRectangle.Bottom;
				vertBottomPixel = (int)newVerticalRect.Bottom;
			}

			if (moveDirection.X != 0 && spriteComponent.FrameHeight > 0)
			{
				for (int x = horizLeftPixel; x < horizRightPixel; x++)
				{
					if (((Level)StateManager.currentState).grid.IsWallTileByPixel(new Vector2(x, newHorizontalLocation.Y))
						|| ((Level)StateManager.currentState).grid.IsWallTileByPixel(new Vector2(x, newHorizontalLocation.Y + spriteComponent.FrameHeight - 1)))
					{
						moveDirection.X = 0;
						break;
					}
				}
			}
			if (moveDirection.Y != 0 && spriteComponent.FrameWidth > 0)
			{
				for (int y = vertTopPixel; y < vertBottomPixel; y++)
				{
					if (((Level)StateManager.currentState).grid.IsWallTileByPixel(new Vector2(newVerticalLocation.X, y))
						|| ((Level)StateManager.currentState).grid.IsWallTileByPixel(new Vector2(newVerticalLocation.X + spriteComponent.FrameWidth - 1, y)))
					{
						moveDirection.Y = 0;
						break;
					}
				}
			}

			return moveDirection;
		}

		protected override void UpdateActor(GameTime gameTime)
        {
            if (invincibilityTimer > 0)
				invincibilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;
			HandleInput(gameTime);

			base.UpdateActor(gameTime);
        }

		public override void Draw()
		{
			//Proceduces the blinking effect when invincible
			if ((invincibilityTimer <= 0) ||
				(invincibilityTimer > 15 && invincibilityTimer < 30) ||
				(invincibilityTimer > 45 && invincibilityTimer < 60) ||
				(invincibilityTimer > 75 && invincibilityTimer < 90) ||
				(invincibilityTimer > 105 && invincibilityTimer < 120) ||
				(invincibilityTimer > 135 && invincibilityTimer < 150) ||
				(invincibilityTimer > 165 && invincibilityTimer < 173) ||
				invincibilityTimer > 181)
			{
				base.Draw();
			}
		}

		public override void BeginActorOverlap(AActor otherActor)
		{
			if (otherActor is AEnemyActor)
			{
				if (invincibilityTimer <= 0)
				{
					foreach (CSpriteComponent spriteComponent in otherActor.GetSceneComponentsByClass<CSpriteComponent>())
					{
						if (spriteComponent.IsInViewport &&
							spriteComponent.CollisionCircle.IsCollidingWith(RootComponent.CollisionCircle))
						{
							ParticlesSpawner.AddFlakesEffect(spriteComponent.WorldLocation,
								spriteComponent.localVelocity / 1, ((AEnemyActor)otherActor).GetColor());
							ParticlesSpawner.AddExplosion(spriteComponent.WorldLocation,
								spriteComponent.localVelocity / 1, Color.White);

							otherActor.Destroy();

							//AddScore(0); //TO readd
							SoundsManager.PlaySoundEffect("Kill", otherActor.WorldLocation);
							Hit();
							ResetLocation();
							//SoundsManager.PlaySoundEffect("Spawn");
							//Multiplier = 1; //TO readd
							//Clean every gun, shot and powerup that is in the current scene.
							List<AMineActor> mines = StateManager.currentState.GetAllActorsOfType<AMineActor>();
							foreach (AMineActor mine in mines)
							{
								mine.Destroy();
							}
							List<APowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<APowerUpActor>();
							foreach (APowerUpActor powerUp in powerUps)
							{
								powerUp.Destroy();
							}
							List<AProjectileActor> projectiles =
								StateManager.currentState.GetAllActorsOfType<AProjectileActor>();
							foreach (AProjectileActor projectile in projectiles)
							{
								projectile.Destroy();
							}

                            //To clear smog

                            //To check
                            gun.Initialize();
						    mineLauncher.Initialize();
                            break;
						}
					}
				}
				if (Lives <= 0)
				{
					StateManager.CreateAndSetState<States.GameOver>();
					//BinaryWriter
					//if (Endless) //TO readd
					{
						AbstractShooterSave abstractShooterSave;
						abstractShooterSave.HiScore = GameInstance.HiScore;
						SaveManager.Save(abstractShooterSave);
					}
				}
			}
		}
	}
}