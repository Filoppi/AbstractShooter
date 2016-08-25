using AbstractShooter.States;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace AbstractShooter
{
	public class APlayerActor : ACharacterActor
	{
		//It's a glow that the player spaceship has
		private float ObjectSpeedOriginal;

		private Vector2 LatestSpawnPosition;
		public float Invincibility = 0F;
		private bool PlayShootSound = true;
		protected InputMode inputMode = InputModes.KeyboardMouseGamePad1;
		private CSpriteComponent transparentBackground; //Addictional sprite that gives a transparent background
		private CParticleEmitterComponent smokeEmitter;

		private ActionBinding turboAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.LeftShift, KeyAction.Down) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.RightShoulder, KeyAction.Down) });
		private ActionBinding triggerMineAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.LeftControl, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.LeftTrigger, KeyAction.Pressed) });
		private ActionBinding deployMineAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Space, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.RightTrigger, KeyAction.Pressed) }, new KeyBinding<MouseButtons>[] { new KeyBinding<MouseButtons>(MouseButtons.Right, KeyAction.Pressed) });
		private AxisBinding fireDirectionX = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.RightAnalogX, 1f) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Right, 1f), new KeyAxisBinding<Keys>(Keys.Left, -1F) });
		private AxisBinding fireDirectionY = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.RightAnalogY, -1f) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.Up, -1F), new KeyAxisBinding<Keys>(Keys.Down, 1f) });
		private AxisBinding movementDirectionX = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.LeftAnalogX, 1f) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.D, 1f), new KeyAxisBinding<Keys>(Keys.A, -1F) });
		private AxisBinding movementDirectionY = new AxisBinding(new KeyAxisBinding<GamePadAxis>[] { new KeyAxisBinding<GamePadAxis>(GamePadAxis.LeftAnalogY, -1f) }, new KeyAxisBinding<Keys>[] { new KeyAxisBinding<Keys>(Keys.S, 1f), new KeyAxisBinding<Keys>(Keys.W, -1F) });

		public APlayerActor(Vector2 worldLocation)
			: base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 96, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Players, worldLocation)
		{
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

			smokeEmitter = new CParticleEmitterComponent(
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

			spriteComponent.collisionGroup = (int)ComponentCollisionGroup.Character;
			spriteComponent.overlappingGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Static;

			ObjectSpeed = 390;
			ObjectSpeedOriginal = 390;

			Invincibility = 0F;
			LatestSpawnPosition = worldLocation;
			Lives = 3;
		}

		private void HandleInput(GameTime gameTime)
		{
			//Turbo
			if (turboAction.CheckBindings(inputMode))
			{
				WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal * 1.24f;
				ObjectSpeed = ObjectSpeedOriginal * 1.77f;
			}
			else
			{
				WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal;
				ObjectSpeed = ObjectSpeedOriginal;
			}

			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;

			Vector2 moveDirection = Vector2.Zero;
			Vector2 fireDirection = Vector2.Zero;

			moveDirection = new Vector2(movementDirectionX.CheckBindings(inputMode), movementDirectionY.CheckBindings(inputMode));
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
				fireDirection = new Vector2(InputManager.currentMouseState.Position.X - (Game1.currentWindowResolution.X / 2), InputManager.currentMouseState.Position.Y - (Game1.currentWindowResolution.Y / 2));
			}
			else
			{
				fireDirection = new Vector2(fireDirectionX.CheckBindings(inputMode), fireDirectionY.CheckBindings(inputMode));
			}

			if (moveDirection != Vector2.Zero)
			{
				moveDirection.Normalize();
				objectDirection = moveDirection;
				moveDirection = CheckTileObstacles(elapsed, moveDirection);
				//moveDirection.Normalize();
				//if (WeaponsAndFireManager.CanFireSmog)
				//{
				//	WeaponsAndFireManager.FireSmog(rootComponent.WorldLocation, objectDirection);
				//}
			}

			spriteComponent.RotateTo(objectDirection);
			rootComponent.localVelocity = moveDirection * ObjectSpeed;

			if (fireDirection != Vector2.Zero)
			{
				fireDirection.Normalize();

				if (WeaponsAndFireManager.CanFireWeapon)
				{
					WeaponsAndFireManager.FireWeapon(rootComponent.WorldLocation, fireDirection, rootComponent.localVelocity);

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

			if (deployMineAction.CheckBindings(inputMode))
			{
				if (WeaponsAndFireManager.CanFireMine)
				{
					WeaponsAndFireManager.FireMine(rootComponent.WorldLocation, ObjectSpeed != ObjectSpeedOriginal ? rootComponent.localVelocity * 0.85F : Vector2.Zero);
					SoundsManager.PlayShootMine();
				}
			}
			else if (triggerMineAction.CheckBindings(inputMode))
			{
				WeaponsAndFireManager.TriggerMines();
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
						ParticlesManager.AddFlakesEffect(spriteComponent.WorldLocation,
							spriteComponent.localVelocity / 1, Color.White);
						ParticlesManager.AddExplosion(spriteComponent.WorldLocation, spriteComponent.localVelocity / 1,
							enemy.GetColor());

						((Level)StateManager.currentState).AddScore(0);
						enemy.Destroy();
						break;
					}
				}
			}
			rootComponent.WorldLocation = LatestSpawnPosition;
			Invincibility = 187;
		}

		private Vector2 CheckTileObstacles(float elapsedTime, Vector2 moveDirection)
		{
			Vector2 newHorizontalLocation = spriteComponent.TopLeftLocation + (new Vector2(moveDirection.X, 0) * (ObjectSpeed * elapsedTime));
			Vector2 newVerticalLocation = spriteComponent.TopLeftLocation + (new Vector2(0, moveDirection.Y) * (ObjectSpeed * elapsedTime));

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
			if (Invincibility > 0)
				Invincibility -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;
			HandleInput(gameTime);

			base.UpdateActor(gameTime);
			//Sets the camera so that the player is in its centre
			Camera.Center = rootComponent.WorldLocation;
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

		public override void BeginActorOverlap(AActor otherActor)
		{
			if (otherActor is AEnemyActor)
			{
				if (Invincibility <= 0)
				{
					foreach (CSpriteComponent spriteComponent in otherActor.GetSceneComponentsByClass<CSpriteComponent>())
					{
						if (spriteComponent.IsInViewport &&
							spriteComponent.CollisionCircle.IsCollidingWith(RootComponent.CollisionCircle))
						{
							ParticlesManager.AddFlakesEffect(spriteComponent.WorldLocation,
								spriteComponent.localVelocity / 1, ((AEnemyActor)otherActor).GetColor());
							ParticlesManager.AddExplosion(spriteComponent.WorldLocation,
								spriteComponent.localVelocity / 1, Color.White);

							otherActor.Destroy();

							//AddScore(0); //TO readd
							SoundsManager.PlayKill(otherActor.WorldLocation);
							Hit();
							ResetLocation();
							//SoundsManager.PlaySpawn();
							//Multiplier = 1; //TO readd
							//Clean every weapon, shot and powerup that is in the current scene.
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
							WeaponsAndFireManager.CurrentWeaponType = WeaponsAndFireManager.WeaponType.Normal;
							WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal;
							WeaponsAndFireManager.shotMinTimer = WeaponsAndFireManager.shotMinTimerOriginal;
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
						SaveManager.Save();
					}
				}
			}
		}
	}
}