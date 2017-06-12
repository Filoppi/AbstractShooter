using AbstractShooter.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
    public class CMineLauncherComponent : CWeaponComponent
    {
        private Rectangle mineRectangle = new Rectangle(72, 95, 25, 23);
        private Rectangle mineRectangle2 = new Rectangle(106, 95, 25, 23);

        public CMineLauncherComponent(AActor owner, CSceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F,
            Vector2 acceleration = new Vector2(), float maxSpeed = -1)
            : base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale,
                acceleration, maxSpeed)
        {
            maxAmmo = 10;
        }

        public override void Initialize()
        {
            base.Initialize();
            timeBetweenShots = 0.315f;
			timeBetweenSounds = 0;
			ammo = 3;
        }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            CheckPowerupPickups(gameTime);

            lastShotTimer += (float) gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;

            foreach (AMineActor mine in spawnedProjectiles)
            {
                if (CheckMineCollisions(mine.SpriteComponent))
                    CheckMineSplashDamage(mine.RootComponent.WorldLocation);
            }
        }

        public override bool CanFire
        {
            get { return ammo > 0 && lastShotTimer >= timeBetweenShots; }
        }

        public override void Fire(Vector2 location, Vector2 direction, Vector2 baseVelocity)
        {
            if (CanFire)
            {
                FireShot(location, direction, baseVelocity);
				ammo--;
				if (CanPlaySound)
				{
					SoundsManager.PlaySoundEffect("ShootMine");
					lastShotSoundTimer = 0.0f;
				}
				lastShotTimer = 0.0f;
			}
        }

        protected override void FireShot(Vector2 location, Vector2 direction, Vector2 velocity)
        {
            //Can't have more than 6 Mines at the time in the game.
            List<AMineActor> mines = StateManager.currentState.GetAllActorsOfType<AMineActor>();
            while (mines.Count >= 6)
            {
                mines[0].Destroy();
                mines.RemoveAt(0);
            }

            AMineActor mine = new AMineActor(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> {mineRectangle, mineRectangle2},
                ActorUpdateGroup.Weapons,
                ComponentUpdateGroup.AfterActor, DrawGroup.Mines,
                location,
                true,
                1,
                Vector2.Zero,
                920);
            //Set lifeSpan at 2000

            spawnedProjectiles.Add(mine);

            mine.RootComponent.localVelocity = velocity * 0.5F;
            mine.SpriteComponent.GenerateDefaultAnimation(0.4635F, true);
            mine.SpriteComponent.loop = true;

            mine.RootComponent.CollisionRadiusMultiplier = 1.6F; //To rebalance //To check if right
        }

        public void TriggerMines()
        {
            //Triggers all the mines
            List<AMineActor> mines = StateManager.currentState.GetAllActorsOfType<AMineActor>();
            foreach (AMineActor mine in mines)
            {
                SoundsManager.PlaySoundEffect("Explosion", mine.RootComponent.WorldLocation);
                ParticlesSpawner.AddMineExplosion(mine.RootComponent.WorldLocation);
                CheckMineSplashDamage(mine.RootComponent.WorldLocation);
                mine.Destroy();
            }
        }

        private bool CheckMineCollisions(CSpriteComponent mine)
        {
            if (((Level) StateManager.currentState).grid.IsWallTile(
                ((Level) StateManager.currentState).grid.GetSquareAtPixel(mine.WorldLocation)))
            {
                mine.Destroy();
                ParticlesSpawner.AddMineExplosion(mine.WorldLocation);
				SoundsManager.PlaySoundEffect("Explosion", mine.WorldLocation);
				return true;
            }
            else
            {
                foreach (AEnemyActor enemy in StateManager.currentState.GetAllActorsOfType<AEnemyActor>())
                {
                    foreach (CSpriteComponent spriteComponent in enemy.GetSceneComponentsByClass<CSpriteComponent>())
                    {
                        if (mine.CollisionCircle.IsCollidingWith(enemy.RootComponent.CollisionCircle))
                        {
                            mine.Destroy();
                            ParticlesSpawner.AddMineExplosion(spriteComponent.WorldLocation);
							SoundsManager.PlaySoundEffect("Explosion", spriteComponent.WorldLocation);
							return true;
                        }
                    }
                }
            }

            return false;
        }

        private void CheckMineSplashDamage(Vector2 location)
        {
            int mineSplashRadius = 165;
            bool played1 = false;
            bool played2 = false;

            foreach (AEnemyActor enemy in StateManager.currentState.GetAllActorsOfType<AEnemyActor>())
            {
                foreach (CSpriteComponent spriteComponent in enemy.GetSceneComponentsByClass<CSpriteComponent>())
                {
                    if (spriteComponent.CollisionCircle.IsCollidingWith(location, mineSplashRadius))
                    {
                        if (!played1)
                        {
                            played1 = true;
							SoundsManager.PlaySoundEffect("Hit", enemy.WorldLocation);
						}

                        if (((Level) StateManager.currentState).CurrentDifficulty <= 1)
                        {
                            enemy.Hit(6);
                        }
                        else if (((Level) StateManager.currentState).CurrentDifficulty == 2)
                        {
                            enemy.Hit(12);
                        }
                        else if (((Level) StateManager.currentState).CurrentDifficulty >= 3)
                        {
                            enemy.Hit(18);
                        }

                        //if dead
                        if (enemy.Lives <= 0)
                        {
                            if (!played2)
                            {
                                played2 = true;
								SoundsManager.PlaySoundEffect("Kill", enemy.WorldLocation);
							}
                            enemy.Destroy();
                            ((Level) StateManager.currentState).AddScore(10);
                            ((Level) StateManager.currentState).GrowMultiplier();
                            ParticlesSpawner.AddExplosion(spriteComponent.WorldLocation,
                                spriteComponent.localVelocity / 30, enemy.GetColor());

                            //Spawn PowerUp
                            if (MathExtention.Rand.Next(0, 12) == 3)
                                ((Level) StateManager.currentState).TryToSpawnPowerup(enemy.WorldLocation);
                            break;
                        }
                    }
                }
            }
        }


        private void CheckPowerupPickups(GameTime gameTime)
        {
            List<APlayerActor> players = StateManager.currentState.GetAllActorsOfType<APlayerActor>();
            foreach (APlayerActor play in players)
            {
                List<AMinePowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<AMinePowerUpActor>();
                foreach (AMinePowerUpActor powerUp in powerUps)
                {
                    if (play.SpriteComponent.CollisionCircle.IsCollidingWith(powerUp.RootComponent.CollisionCircle))
                    {
                        //if (CanBeUpgradedWith(gunPowerUp.gunSettings.level)) //1 Not always //2 Always //3 Always
                        {
                            ammo += 3;
                            if (ammo > maxAmmo)
                                ammo = maxAmmo;
                        }

						SoundsManager.PlaySoundEffect("PowerUp");
						powerUp.Destroy();
                        break;
                    }
                }
            }
        }
    }
}