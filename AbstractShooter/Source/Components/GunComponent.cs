using AbstractShooter.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
    //TO1
    public interface Upgradable
    {
        bool UpgradeApplicable(); //Is this object compatible with the upgrade?
        bool CanBeUpgraded(); //Can the object currently be upgraded?
        bool CanBeUpgradedWith(object newUpgrade); //Is new upgrade better than current one? Are we already at maximum level?
        void Upgrade(object newUpgrade);
        void Downgrade();
        void Reset();
        void CheckUpgradePickup();
        bool IsUpgradeExpired();
        bool ShouldResetOnExpiration(); //Should reset or downgrade on expiration?
        int CurrentUpgradeLevel { get; set; }
    }

    public struct GunSettings
    {
        public float timeLength;
        public float numberOfProjectiles;
        public float timeBetweenShots;
        public float angleBetweenProjectiles;
        public float projectilesSpeed;
        public float rotationSpeed;
        public Color projectilesColor;
        public int level;

        public GunSettings(Color projectilesColor, float timeBetweenShots = 0.3f, float projectilesSpeed = 100f, float rotationSpeed = 0, float timeLength = -1, float numberOfProjectiles = 1, float angleBetweenProjectiles = 15, int level = 1)
        {
            this.projectilesColor = projectilesColor;
            this.projectilesSpeed = projectilesSpeed;
            this.timeBetweenShots = timeBetweenShots;
            this.timeLength = timeLength;
            this.numberOfProjectiles = numberOfProjectiles;
            this.angleBetweenProjectiles = angleBetweenProjectiles;
            this.rotationSpeed = rotationSpeed;
            this.level = level;
        }
    }

    public class CGunComponent : CWeaponComponent
    {
        ///GunSettings
        protected float numberOfProjectiles;
        protected float angleBetweenProjectiles;
        protected Color projectilesColor;

        protected Rectangle shotRectangle = new Rectangle(34, 1, 32, 32);

        private float remainingWeaponTime = -1;
        private float currentRotation = 0;
        private float currentRotationSpeed = 0;

        protected int currentUpgradeLevel = 0;
        
        public CGunComponent(AActor owner, CSceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1)
            : base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
        {
        }

        public override void Initialize()
        {
            numberOfProjectiles = 1;
            angleBetweenProjectiles = 0;
            projectilesColor = new Color(255, 255, 0);
            originalWeaponSpeed = 900;
            weaponSpeed = originalWeaponSpeed; //1650 //1845
            originalDelayBetweenShots = 0.046f;
            timeBetweenShots = originalDelayBetweenShots;
            timeBetweenSounds = originalDelayBetweenShots * 2;
            currentUpgradeLevel = 0;
            currentRotation = 0;
            currentRotationSpeed = 0;

            base.Initialize();

            remainingWeaponTime = -1;
        }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            CheckPowerupPickups(gameTime);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
			lastShotSoundTimer += elapsed;
            lastShotTimer += elapsed;
            CheckWeaponUpgradeExpire(elapsed);

            foreach (AProjectileActor projectile in spawnedProjectiles)
            {
                CheckShotWallImpacts(projectile);
                CheckShotEnemyImpacts(projectile);
            }

            if (currentRotationSpeed != 0)
            {
                currentRotation += currentRotationSpeed * elapsed;
            }
            else
            {
                currentRotation = 0;
            }
        }

        public override void Fire(Vector2 location, Vector2 direction, Vector2 baseVelocity)
        {
            if (CanFire)
            {
                if (numberOfProjectiles > 1)
                {
                    float baseAngle = (float)Math.Atan2(direction.Y, direction.X);
                    float offset = MathHelper.ToRadians(angleBetweenProjectiles);
                    
                    for (int i = -(int)(((float)numberOfProjectiles / 2f).SymmetricFloor()); i <= (int)(((float)numberOfProjectiles / 2f).SymmetricFloor()); ++i)
                    {
                        Vector2 newDirection = new Vector2((float)Math.Cos(currentRotation + baseAngle + (offset * i)),
                            (float)Math.Sin(currentRotation + baseAngle + (offset * i)));
                        FireShot(location, newDirection, (newDirection * weaponSpeed) + baseVelocity);
                    }
                }
                else
                {
                    FireShot(location, direction, (direction * weaponSpeed) + baseVelocity);
				}

				lastShotTimer = 0.0f;

				//Playing once out of two times
				if (CanPlaySound)
                {
                    SoundsManager.PlaySoundEffect("Shoot");
					lastShotSoundTimer = 0;
				}
			}
        }

        protected override void FireShot(Vector2 location, Vector2 direction, Vector2 velocity)
        {
            Color finalColor = GetShotColor();
            finalColor = new Color(finalColor.R / 3, finalColor.G / 3, finalColor.B / 3);
            direction.Normalize();
            location += direction * 21F; //Makes it start a bit distant from the player

            AProjectileActor shot = new AProjectileActor(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { shotRectangle },
                90F, //To invert with 4000??
                0,
                ActorUpdateGroup.Weapons,
                ComponentUpdateGroup.AfterActor, DrawGroup.Shots,
                location,
                true,
                1,
                Vector2.Zero,
                4000F,
                GetShotColor(),
                finalColor);

            spawnedProjectiles.Add(shot);

            shot.RootComponent.localVelocity = velocity;
            shot.RootComponent.RelativeScale = 0.9f;
            //shot.RootComponent.CollisionRadiusMultiplier = 0.5F; //To rebalance
            shot.RootComponent.RotateTo(direction);
        }

        private Color GetShotColor()
        {
            return projectilesColor;
        }

        private void CheckWeaponUpgradeExpire(float elapsed)
        {
            if (currentUpgradeLevel != 0)
            {
                remainingWeaponTime -= elapsed;
                if (remainingWeaponTime <= 0)
                {
                    Initialize();
                }
            }
        }

        private void CheckShotEnemyImpacts(AProjectileActor shot)
        {
            foreach (AActor actor in shot.GetOverlappingActors())
            {
                if (actor is AEnemyActor)
                {
                    AEnemyActor enemy = actor as AEnemyActor;
                    foreach (CSpriteComponent spriteComponent in enemy.GetSceneComponentsByClass<CSpriteComponent>())
                    {
                        //if (shot.CSpriteComponent.CollisionCircle.IsCollidingWith(spriteComponent.CollisionCircle))
                        {
                            if (enemy.damageType != DamageType.ComponentsLast || spriteComponent.GetAllDescendantsCountOfType<CSpriteComponent>() == 0) //To Finish
                            {
                                shot.Destroy();

                                enemy.Hit();
                                //if (enemy.damageType == DamageType.ComponentsLast && spriteComponent.IsRootComponent())
                                //{
                                //    enemy.Hit();
                                //}

                                ParticlesSpawner.AddFlakesEffect(spriteComponent.WorldLocation,
                                    spriteComponent.localVelocity / 30,
                                    GetShotColor());

                                //if dead
                                if (enemy.Lives <= 0)
                                {
									SoundsManager.PlaySoundEffect("Kill", enemy.WorldLocation);
									((Level)StateManager.currentState).AddScore(10);
                                    ((Level)StateManager.currentState).GrowMultiplier();
                                    ParticlesSpawner.AddExplosion(spriteComponent.WorldLocation,
                                        spriteComponent.localVelocity / 30, enemy.GetColor());

                                    //Spawn PowerUp
                                    if (MathExtention.Rand.Next(0, 7) == 3)
                                        ((Level)StateManager.currentState).TryToSpawnPowerup(spriteComponent.WorldLocation);
                                    enemy.Destroy();
                                }
                                else
                                {
									SoundsManager.PlaySoundEffect("Hit", enemy.WorldLocation);

                                    if (enemy.damageType == DamageType.ComponentsLast)
                                    {
                                        spriteComponent.Destroy();
                                    }
                                }
                            }
                            else
                            {
                                shot.Destroy();

                                //Color shotInitialColor = GetShotColor()*0.02F;
                                //shotInitialColor.A = 1;
                                ParticlesSpawner.AddFlakesEffect(spriteComponent.WorldLocation,
                                    spriteComponent.localVelocity / 30,
                                    Color.DarkSlateGray);

								SoundsManager.PlaySoundEffect("Hit", spriteComponent.WorldLocation); //Play avoided hit
                            }
                        }
                    }
                }
            }
        }

        private void CheckShotWallImpacts(AProjectileActor shot)
        {
            if (((Level)StateManager.currentState).grid.IsWallTile(((Level)StateManager.currentState).grid.GetSquareAtPixel(shot.RootComponent.WorldLocation)))
            {
                shot.Destroy();
                ParticlesSpawner.AddSparksEffect(shot.RootComponent.WorldLocation, shot.RootComponent.localVelocity);
            }
        }

        private void CheckPowerupPickups(GameTime gameTime)
        {
            List<APlayerActor> players = StateManager.currentState.GetAllActorsOfType<APlayerActor>();
            foreach (APlayerActor play in players)
            {
                List<AGunPowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<AGunPowerUpActor>();
                foreach (AGunPowerUpActor powerUp in powerUps)
                {
                    if (play.SpriteComponent.CollisionCircle.IsCollidingWith(powerUp.RootComponent.CollisionCircle))
                    {
                        //if (CanBeUpgradedWith(gunPowerUp.gunSettings.level)) //1 Not always //2 Always //3 Always
                        {
                            currentUpgradeLevel = powerUp.gunSettings.level;
                            remainingWeaponTime = powerUp.gunSettings.timeLength;

                            currentRotation = 0;
                            currentRotationSpeed = powerUp.gunSettings.rotationSpeed;

                            timeBetweenShots = powerUp.gunSettings.timeBetweenShots;
                            numberOfProjectiles = powerUp.gunSettings.numberOfProjectiles;
                            angleBetweenProjectiles = powerUp.gunSettings.angleBetweenProjectiles;
                            weaponSpeed = powerUp.gunSettings.projectilesSpeed;

                            projectilesColor = powerUp.gunSettings.projectilesColor;
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