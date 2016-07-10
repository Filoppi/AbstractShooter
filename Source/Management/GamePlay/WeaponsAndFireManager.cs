using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using AbstractShooter.States;

namespace AbstractShooter
{
    static class WeaponsAndFireManager
    {
        #region Declarations
        static private Rectangle shotRectangle = new Rectangle(34, 1, 32, 32);
        static private Rectangle mineRectangle = new Rectangle(72, 95, 25, 23);
        static private Rectangle mineRectangle2 = new Rectangle(106, 95, 25, 23);
        static private Rectangle powerupRectangle = new Rectangle(64, 128, 32, 32);
        //static public Rectangle starRectangle = new Rectangle(143, 0, 2, 2);
        static public float WeaponSpeed = 900; //1650 //1845
        static public float WeaponSpeedOriginal = 900;
        static private float shotTimer = 0f;
        static public float shotMinTimer = 0.046f;
        static public float shotMinTimerOriginal = 0.046f;
        static private float smogTimer = 0f;
        static private float smogMinTimer = 0.03f;
        static private float mineTimer = 0f;
        static private float mineMinTimer = 0.315f;

        //Weapon Upgrade declarations
        public enum WeaponType { Normal, Triple, Quintuple, Ultra, Mine };
        static public WeaponType CurrentWeaponType = WeaponType.Normal;
        static private float WeaponTimeRemaining = 15.0f;
        static private float weaponTimeDefault = 15.0f;
        static private float tripleWeaponSplitAngle = 15;
        static private float quintupleWeaponSplitAngle = 15;
        static private int ultraWeaponProjectilesNumber = 8;
        static private float ultraWeaponSplitAngle = 360F / (float)ultraWeaponProjectilesNumber;
        static private float ultraWeaponCurrentAngle = 0;
        static public int minesNumber = 3;

        //Power up declarations
        static private int maxActivePowerups = 3;
        static private Random rand = new Random();
        #endregion

        public static void Initialize()
        {
            CurrentWeaponType = WeaponType.Normal;
            WeaponSpeed = WeaponSpeedOriginal;
            shotMinTimer = shotMinTimerOriginal;
            minesNumber = 3;
            WeaponTimeRemaining = 15.0f;
        }

        #region Properties
        static public float WeaponFireDelay
        {
            get
            {
                return shotMinTimer;
            }
        }
        static public bool CanFireWeapon
        {
            get
            {
                return shotTimer >= WeaponFireDelay;
            }
        }
        static public bool CanFireSmog
        {
            get
            {
                return smogTimer >= smogMinTimer;
            }
        }
        static public bool CanFireMine
        {
            get
            {
                return (minesNumber > 0 && mineTimer >= mineMinTimer);
            }
        }
        #endregion

        #region Effects Management Methods
        private static void AddShot(Vector2 location, Vector2 direction, Vector2 velocity)
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
            
            shot.RootComponent.localVelocity = velocity;
            shot.RootComponent.RelativeScale = 0.9f;
            //shot.RootComponent.CollisionRadiusMultiplier = 0.5F; //To rebalance
            shot.RootComponent.RotateTo(direction);
        }
        private static void AddMine(Vector2 location, Vector2 velocity)
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
                new List<Rectangle> { mineRectangle, mineRectangle2 },
                ActorUpdateGroup.Weapons,
                ComponentUpdateGroup.AfterActor, DrawGroup.Mines,
                location,
                true,
                1,
                Vector2.Zero,
                920);
            //Set lifeSpan at 2000

            mine.RootComponent.localVelocity = velocity * 0.5F;
            mine.CSpriteComponent.GenerateDefaultAnimation(0.4635F, true);
            mine.CSpriteComponent.loop = true;

            mine.RootComponent.CollisionRadiusMultiplier = 1.6F;  //To rebalance //To check if right
        }
        private static void AddSmog(Vector2 location, Vector2 direction)
        {
            ParticlesManager.AddSmog(location, direction);
        }
        #endregion

        #region Weapons Management Methods
        static private Color GetShotColor()
        {
            if (CurrentWeaponType == WeaponType.Triple)
                return new Color(0, 167, 255);
            if (CurrentWeaponType == WeaponType.Quintuple)
                return new Color(255, 150, 0);
            if (CurrentWeaponType == WeaponType.Ultra)
                return new Color(0, 255, 0);

            return new Color(255, 255, 0);
        }
        public static void FireWeapon(Vector2 location, Vector2 direction, Vector2 baseVelocity)
        {
            switch (CurrentWeaponType)
            {
                case WeaponType.Normal:
                    AddShot(location, direction, (direction * WeaponSpeed) + baseVelocity);
                    break;
                case WeaponType.Triple:
                    {
                        float baseAngle = (float)Math.Atan2(direction.Y, direction.X);
                        float offset = MathHelper.ToRadians(tripleWeaponSplitAngle);
                        for (int i = -1; i <= 1; ++i)
                        {
                            Vector2 newDirection = new Vector2((float)Math.Cos(baseAngle + (offset * i)), (float)Math.Sin(baseAngle + (offset * i)));
                            AddShot(location, newDirection, (newDirection * WeaponSpeed) + baseVelocity);
                        }
                        break;
                    }
                case WeaponType.Quintuple:
                    {
                        float baseAngle = (float)Math.Atan2(direction.Y, direction.X);
                        float offset = MathHelper.ToRadians(quintupleWeaponSplitAngle);
                        for (int i = -2; i <= 2; ++i)
                        {
                            Vector2 newDirection = new Vector2((float)Math.Cos(baseAngle + (offset * i)), (float)Math.Sin(baseAngle + (offset * i)));
                            AddShot(location, newDirection, (newDirection * WeaponSpeed) + baseVelocity);
                        }
                        break;
                    }
                case WeaponType.Ultra:
                    {
                        ultraWeaponCurrentAngle += 0.16F;
                        float offset = MathHelper.ToRadians(ultraWeaponSplitAngle);
                        for (int i = 0; i < ultraWeaponProjectilesNumber; ++i)
                        {
                            Vector2 newDirection = new Vector2((float)Math.Cos(ultraWeaponCurrentAngle + (offset * i)), (float)Math.Sin(ultraWeaponCurrentAngle + (offset * i)));
                            AddShot(location, newDirection, (newDirection * WeaponSpeed) + baseVelocity);
                        }
                        break;
                    }
            }

            shotTimer = 0.0f;
        }
        public static void FireSmog(Vector2 location, Vector2 velocity)
        {
            AddSmog(location, velocity);
            smogTimer = 0.0f;
        }
        public static void FireMine(Vector2 location, Vector2 velocity)
        {
            AddMine(location, velocity);
            mineTimer = 0.0f;
            minesNumber--;
        }
        public static void TriggerMines()
        {
            //Triggers all the mines
            List<AMineActor> mines = StateManager.currentState.GetAllActorsOfType<AMineActor>();
            foreach (AMineActor mine in mines)
            {
                SoundsManager.PlayExplosion(mine.RootComponent.WorldCenter);
                ParticlesManager.AddMineExplosion(mine.RootComponent.WorldCenter);
                checkMineSplashDamage(mine.RootComponent.WorldCenter);
                mine.Destroy();
            }
        }

        private static void checkWeaponUpgradeExpire(float elapsed)
        {
            if (CurrentWeaponType != WeaponType.Normal)
            {
                WeaponTimeRemaining -= elapsed;
                if (WeaponTimeRemaining <= 0)
                {
                    CurrentWeaponType = WeaponType.Normal;
                    shotMinTimer = shotMinTimerOriginal;
                    WeaponSpeed = WeaponSpeedOriginal;
                }
            }
        }

        private static void tryToSpawnPowerup(Vector2 Position)
        {
            APowerUpActor newPowerup = new APowerUpActor(
                StateManager.currentState.spriteSheet,
                new List<Rectangle>
                {
                    powerupRectangle,
                    new Rectangle(powerupRectangle.X + powerupRectangle.Width, powerupRectangle.Y,
                        powerupRectangle.Width, powerupRectangle.Height),
                    new Rectangle(powerupRectangle.X + (powerupRectangle.Width*2), powerupRectangle.Y,
                        powerupRectangle.Width, powerupRectangle.Height),
                    new Rectangle(powerupRectangle.X + (powerupRectangle.Width*3), powerupRectangle.Y,
                        powerupRectangle.Width, powerupRectangle.Height)
                },
                560F,
                165F,
                ActorUpdateGroup.Weapons,
                ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                Position,
                true,
                1F,
                default(Vector2),
                -1F,
                Color.White,
                Color.White);

            //TORefactor
            WeaponType type;
            int temp = rand.Next(0, 10);
            if (temp == 9)
                type = WeaponType.Ultra;
            else if (temp == 8 || temp == 7)
                type = WeaponType.Quintuple;
            else if (temp == 6 || temp == 5)
                type = WeaponType.Mine;
            else
                type = WeaponType.Triple;
            if (type == WeaponType.Triple)
                newPowerup.CSpriteComponent.CurrentFrame = 0;
            else if (type == WeaponType.Quintuple)
                newPowerup.CSpriteComponent.CurrentFrame = 1;
            else if (type == WeaponType.Ultra)
                newPowerup.CSpriteComponent.CurrentFrame = 2;
            else if (type == WeaponType.Mine)
                newPowerup.CSpriteComponent.CurrentFrame = 3;

            List<APowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<APowerUpActor>();
            while (powerUps.Count >= maxActivePowerups)
            {
                powerUps[0].Destroy();
                powerUps.RemoveAt(0);
            }

            SoundsManager.PlayPowerUp();
        }
        #endregion

        #region Collision Detection
        private static void checkShotEnemyImpacts(AProjectileActor shot)
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

                                ParticlesManager.AddFlakesEffect(spriteComponent.WorldCenter,
                                    spriteComponent.localVelocity / 30,
                                    GetShotColor());

                                //if dead
                                if (enemy.Lives <= 0)
                                {
                                    SoundsManager.PlayKill(enemy.WorldLocation);
                                    ((Level)StateManager.currentState).addScore(10);
                                    ((Level)StateManager.currentState).growMultiplier();
                                    ParticlesManager.AddExplosion(spriteComponent.WorldCenter,
                                        spriteComponent.localVelocity / 30, enemy.GetColor());

                                    //Spawn PowerUp
                                    if (rand.Next(0, 7) == 3)
                                        tryToSpawnPowerup(spriteComponent.WorldLocation);
                                    enemy.Destroy();
                                }
                                else
                                {
                                    SoundsManager.PlayHit(enemy.WorldLocation);

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
                                ParticlesManager.AddFlakesEffect(spriteComponent.WorldCenter,
                                    spriteComponent.localVelocity / 30,
                                    Color.DarkSlateGray);

                                SoundsManager.PlayHit(spriteComponent.WorldCenter); //Play avoided hit
                            }
                        }
                    }
                }
            }
        }
        private static bool checkMineCollisions(CSpriteComponent mine)
        {
            if (((Level)StateManager.currentState).grid.IsWallTile(((Level)StateManager.currentState).grid.GetSquareAtPixel(mine.WorldCenter)))
            {
                mine.Destroy();
                ParticlesManager.AddMineExplosion(mine.WorldCenter);
                SoundsManager.PlayExplosion(mine.WorldCenter);
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
                            ParticlesManager.AddMineExplosion(spriteComponent.WorldCenter);
                            SoundsManager.PlayExplosion(spriteComponent.WorldCenter);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private static void checkMineSplashDamage(Vector2 location)
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
                            SoundsManager.PlayHit(enemy.WorldLocation);
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
                                SoundsManager.PlayKill(enemy.WorldLocation);
                            }
                            enemy.Destroy();
                            ((Level) StateManager.currentState).addScore(10);
                            ((Level) StateManager.currentState).growMultiplier();
                            ParticlesManager.AddExplosion(spriteComponent.WorldCenter,
                                spriteComponent.localVelocity/30, enemy.GetColor());

                            //Spawn PowerUp
                            if (rand.Next(0, 12) == 3)
                                tryToSpawnPowerup(enemy.WorldLocation);
                            break;
                        }
                    }
                }
            }
        }
        private static void checkShotWallImpacts(AProjectileActor shot)
        {
            if (((Level)StateManager.currentState).grid.IsWallTile(((Level)StateManager.currentState).grid.GetSquareAtPixel(shot.RootComponent.WorldCenter)))
            {
                shot.Destroy();
                ParticlesManager.AddSparksEffect(shot.RootComponent.WorldCenter, shot.RootComponent.localVelocity);
            }
        }
        private static void checkPowerupPickups(GameTime gameTime)
        {
            List<APlayerActor> players = StateManager.currentState.GetAllActorsOfType<APlayerActor>();
            foreach (APlayerActor play in players)
            {
                List<APowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<APowerUpActor>();
                foreach (APowerUpActor powerUp in powerUps)
                {
                    //PowerUps[x].Update(gameTime);
                    if (play.CSpriteComponent.CollisionCircle.IsCollidingWith(powerUp.RootComponent.CollisionCircle))
                    {
                        switch (powerUp.CSpriteComponent.CurrentFrame)
                        {
                            case 0: //3 Shots
                                if (CurrentWeaponType == WeaponType.Normal || CurrentWeaponType == WeaponType.Triple)
                                {
                                    CurrentWeaponType = WeaponType.Triple;
                                    WeaponTimeRemaining = weaponTimeDefault;
                                    WeaponSpeed = WeaponSpeedOriginal * 0.82F;
                                    shotMinTimer = 0.051f; //Was 0.035F
                                }
                                break;
                            case 1: //5 Shots
                                //if (CurrentWeaponType == WeaponType.Normal || CurrentWeaponType == WeaponType.Triple || CurrentWeaponType == WeaponType.Quintuple || CurrentWeaponType == WeaponType.Ultra)
                                {
                                    CurrentWeaponType = WeaponType.Quintuple;
                                    WeaponTimeRemaining = weaponTimeDefault;
                                    WeaponSpeed = WeaponSpeedOriginal * 0.74F;
                                    shotMinTimer = 0.0825f;
                                }
                                break;
                            case 2: //Ultra (EverySide)
                                CurrentWeaponType = WeaponType.Ultra;
                                WeaponTimeRemaining = weaponTimeDefault / 3.9f;
                                WeaponSpeed = WeaponSpeedOriginal;
                                shotMinTimer = 0.06f;
                                break;
                            case 3: //Mines
                                minesNumber += 3;
                                if (minesNumber > 10)
                                    minesNumber = 10;
                                break;
                        }
                        SoundsManager.PlayPowerUp();
                        powerUp.Destroy();
                        break;
                    }
                }
            }
        }
        #endregion
        
        static public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            shotTimer += elapsed;
            smogTimer += elapsed;
            mineTimer += elapsed;
            checkWeaponUpgradeExpire(elapsed);
            List<AProjectileActor> projectiles = StateManager.currentState.GetAllActorsOfType<AProjectileActor>();
            foreach (AProjectileActor projectile in projectiles)
            {
                checkShotWallImpacts(projectile);
                checkShotEnemyImpacts(projectile);
            }
            List<AMineActor> mines = StateManager.currentState.GetAllActorsOfType<AMineActor>();
            foreach (AMineActor mine in mines)
            {
                if (checkMineCollisions(mine.CSpriteComponent))
                    checkMineSplashDamage(mine.RootComponent.WorldLocation);
            }

            checkPowerupPickups(gameTime);
        }
    }
}