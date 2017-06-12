using AbstractShooter.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
    public class CWeaponComponent : CSceneComponent
    {
        #region Declarations
        
        protected List<AActor> spawnedProjectiles;
        
        ///Time elapsed from last shot
        protected float lastShotTimer = 0f;

		protected float lastShotSoundTimer = 0f;

		///Projectiles speed
		protected float weaponSpeed = 100;

        ///Min time between two shots
        protected float timeBetweenShots = 0.046f;

        protected float timeBetweenSounds = 0.046f * 2;

        ///Number of ammunitions left
        protected int ammo = -1;
        protected int maxAmmo = 3;

        protected float originalWeaponSpeed = 900;
        protected float originalDelayBetweenShots = 0.046f;
        
        #endregion Declarations

        public virtual bool CanFire
        {
            get
            {
                return lastShotTimer >= timeBetweenShots;
            }
        }
        public virtual bool CanPlaySound
        {
            get
            {
                return lastShotSoundTimer >= timeBetweenSounds;
            }
        }

        public CWeaponComponent(AActor owner, CSceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1)
            : base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
        {
            spawnedProjectiles = new List<AActor>();

            //To1 move
            Initialize();
        }
        
        public virtual void Initialize()
        {
            weaponSpeed = originalWeaponSpeed;
            timeBetweenShots = originalDelayBetweenShots;
        }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            //Remove pending destroy projectiles from list
            List<AActor> spawnedProjectilesCopy = new List<AActor>(spawnedProjectiles);
            foreach (AActor projectile in spawnedProjectilesCopy)
            {
                if (projectile.PendingDestroy)
                {
                    spawnedProjectiles.Remove(projectile);
                }
            }
        }

        ///Wants to fire
        public virtual void Fire(Vector2 location, Vector2 direction, Vector2 baseVelocity)
        {
        }

        ///Actual fire, if firing conditions are true
        protected virtual void FireShot(Vector2 location, Vector2 direction, Vector2 velocity)
        {
        }
    }
}