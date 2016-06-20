using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    static class WeaponsAndFireManager
    {
        #region Declarations
        static private Rectangle shotRectangle = new Rectangle(34, 1, 32, 32);
        static private Rectangle mineRectangle = new Rectangle(72, 95, 25, 23);
        static private Rectangle mineRectangle2 = new Rectangle(106, 95, 25, 23);
        static private Rectangle smogRectangle = new Rectangle(32, 65, 32, 30);
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
        static private Int32 ultraWeaponProjectilesNumber = 8;
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
            AProjectile shot = new AProjectile(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { shotRectangle },
                90F, //To invert with 4000??
                0,
                ComponentUpdateGroup.AfterActor, DrawGroup.Shots,
                location,
                true,
                1,
                Vector2.Zero,
                4000F,
                GetShotColor(),
                finalColor);

            shot.RootComponent.velocity = velocity;
            shot.SpriteComponent.relativeScale = 0.9f;
            shot.SpriteComponent.CollisionRadiusMultiplier = 0.5F;
            shot.SpriteComponent.RotateTo(direction);
        }
        private static void AddMine(Vector2 location, Vector2 velocity)
        {
            //Can't have more than 6 Mines at the time in the game.
            List<AMine> mines = StateManager.currentState.GetAllActorsOfClass<AMine>();
            while (mines.Count >= 6)
            {
                mines[0].Destroy();
                mines.RemoveAt(0);
            }

            AMine mine = new AMine(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { mineRectangle, mineRectangle2 },
                ComponentUpdateGroup.AfterActor, DrawGroup.Mines,
                location,
                true,
                1,
                Vector2.Zero,
                920);
            //Set lifeSpan at 2000

            mine.RootComponent.velocity = velocity * 0.5F;
            mine.SpriteComponent.GenerateDefaultAnimation(0.4635F, true);
            mine.SpriteComponent.loop = true;

            mine.SpriteComponent.CollisionRadiusMultiplier = 1.6F; //To check if right
        }
        private static void AddSmog(Vector2 location, Vector2 direction)
        {
            ATemporarySprite smog = new ATemporarySprite(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { smogRectangle,
                    new Rectangle(smogRectangle.X + smogRectangle.Width, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height),
                    new Rectangle(smogRectangle.X + smogRectangle.Width * 2, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height) },
                108,
                0,
                ComponentUpdateGroup.AfterActor, DrawGroup.Particles,
                location,
                true,
                1,
                Vector2.Zero,
                0,
                Color.White,
                Color.Black);
            
            smog.SpriteComponent.RotateTo(direction);
        }
        #endregion

        #region Weapons Management Methods
        static private Color GetShotColor()
        {
            if (CurrentWeaponType == WeaponType.Triple)
                return new Color(0, 167, 255);
            else if (CurrentWeaponType == WeaponType.Quintuple)
                return new Color(255, 150, 0);
            else if (CurrentWeaponType == WeaponType.Ultra)
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
                        for (Int32 i = -1; i <= 1; ++i)
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
                        for (Int32 i = -2; i <= 2; ++i)
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
                        for (Int32 i = 0; i < ultraWeaponProjectilesNumber; ++i)
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
            List<AMine> mines = StateManager.currentState.GetAllActorsOfClass<AMine>();
            foreach (AMine mine in mines)
            {
                SoundsManager.PlayExplosion();
                EffectsManager.AddMineExplosion(mine.SpriteComponent.WorldCenter);
                checkMineSplashDamage(mine.SpriteComponent.WorldCenter);
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
            APowerUp newPowerup = new APowerUp(
                StateManager.currentState.spriteSheet,
                new List<Rectangle> { powerupRectangle,
                    new Rectangle(powerupRectangle.X + powerupRectangle.Width, powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height),
                    new Rectangle(powerupRectangle.X + (powerupRectangle.Width * 2), powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height),
                    new Rectangle(powerupRectangle.X + (powerupRectangle.Width * 3), powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height) },
                560F,
                165F,
                ComponentUpdateGroup.AfterActor, DrawGroup.Characters,
                Position,
                true);

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
                newPowerup.SpriteComponent.CurrentFrame = 0;
            else if (type == WeaponType.Quintuple)
                newPowerup.SpriteComponent.CurrentFrame = 1;
            else if (type == WeaponType.Ultra)
                newPowerup.SpriteComponent.CurrentFrame = 2;
            else if (type == WeaponType.Mine)
                newPowerup.SpriteComponent.CurrentFrame = 3;

            List<APowerUp> powerUps = StateManager.currentState.GetAllActorsOfClass<APowerUp>();
            while (powerUps.Count >= maxActivePowerups)
            {
                powerUps[0].Destroy();
                powerUps.RemoveAt(0);
            }

            SoundsManager.PlayPowerUp();
        }
        #endregion

        #region Collision Detection
        private static void checkShotEnemyImpacts(AProjectile shot)
        {
            foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
            {
                if (shot.SpriteComponent.IsCircleColliding(enemy.SpriteComponent.WorldCenter, enemy.SpriteComponent.CollisionRadius))
                {
                    shot.Destroy();
                    enemy.Hit();

                    EffectsManager.AddFlakesEffect(enemy.SpriteComponent.WorldCenter, enemy.RootComponent.velocity / 30, GetShotColor());

                    //if dead
                    if (enemy.Lives <= 0)
                    {
                        SoundsManager.PlayKill();
                        GameManager.addScore(10);
                        GameManager.growMultiplier();
                        EffectsManager.AddExplosion(enemy.SpriteComponent.WorldCenter, enemy.RootComponent.velocity / 30, enemy.GetColor());

                        //Spawn PowerUp
                        if (rand.Next(0, 7) == 3)
                            tryToSpawnPowerup(enemy.RootComponent.WorldLocation);
                        enemy.Destroy();
                    }
                    else
                    {
                        SoundsManager.PlayHit();
                    }
                }
            }
        }
        private static bool checkMineCollisions(SpriteComponent mine)
        {
            if (TileMap.IsWallTile(TileMap.GetSquareAtPixel(mine.WorldCenter)))
            {
                mine.Destroy();
                EffectsManager.AddMineExplosion(mine.WorldCenter);
                SoundsManager.PlayExplosion();
                return true;
            }
            else
            {
                foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
                {
                    if ((enemy is AEnemySeeker || enemy is AEnemyWanderer))
                    {
                        if (mine.IsCircleColliding(enemy.SpriteComponent.WorldCenter, enemy.SpriteComponent.CollisionRadius))
                        {
                            mine.Destroy();
                            EffectsManager.AddMineExplosion(enemy.SpriteComponent.WorldCenter);
                            SoundsManager.PlayExplosion();
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

            foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
            {
                if (enemy.SpriteComponent.IsInViewport)
                {
                    if (enemy.SpriteComponent.IsCircleColliding(location, mineSplashRadius))
                    {
                        if (!played1)
                        {
                            played1 = true;
                            SoundsManager.PlayHit();
                        }

                        if (GameManager.CurrentDifficulty <= 1)
                        {
                            enemy.Hit(6);
                        }
                        else if (GameManager.CurrentDifficulty == 2)
                        {
                            enemy.Hit(12);
                        }
                        else if (GameManager.CurrentDifficulty >= 3)
                        {
                            enemy.Hit(18);
                        }

                        //if dead
                        if (enemy.Lives <= 0)
                        {
                            if (!played2)
                            {
                                played2 = true;
                                SoundsManager.PlayKill();
                            }
                            enemy.Destroy();
                            GameManager.addScore(10);
                            GameManager.growMultiplier();
                            EffectsManager.AddExplosion(enemy.SpriteComponent.WorldCenter, enemy.RootComponent.velocity / 30, enemy.GetColor());

                            //Spawn PowerUp
                            if (rand.Next(0, 12) == 3)
                                tryToSpawnPowerup(enemy.WorldLocation);
                        }
                    }
                }
            }
        }
        private static void checkShotWallImpacts(AProjectile shot)
        {
            if (TileMap.IsWallTile(TileMap.GetSquareAtPixel(shot.SpriteComponent.WorldCenter)))
            {
                shot.Destroy();
                EffectsManager.AddSparksEffect(shot.SpriteComponent.WorldCenter, shot.SpriteComponent.velocity);
            }
        }
        private static void checkPowerupPickups(GameTime gameTime)
        {
            List<APlayer> players = StateManager.currentState.GetAllActorsOfClass<APlayer>();
            foreach (APlayer play in players)
            {
                List<APowerUp> powerUps = StateManager.currentState.GetAllActorsOfClass<APowerUp>();
                foreach (APowerUp powerUp in powerUps)
                {
                    //PowerUps[x].Update(gameTime);
                    if (play.SpriteComponent.IsCircleColliding(powerUp.SpriteComponent.WorldCenter, powerUp.SpriteComponent.CollisionRadius))
                    {
                        switch (powerUp.SpriteComponent.CurrentFrame)
                        {
                            case 0: //3 Shots
                                if (CurrentWeaponType == WeaponType.Normal || CurrentWeaponType == WeaponType.Triple)
                                {
                                    CurrentWeaponType = WeaponType.Triple;
                                    WeaponTimeRemaining = weaponTimeDefault;
                                    WeaponSpeed = WeaponSpeedOriginal;
                                    shotMinTimer = 0.035f;
                                }
                                break;
                            case 1: //5 Shots
                                //if (CurrentWeaponType == WeaponType.Normal || CurrentWeaponType == WeaponType.Triple || CurrentWeaponType == WeaponType.Quintuple || CurrentWeaponType == WeaponType.Ultra)
                                {
                                    CurrentWeaponType = WeaponType.Quintuple;
                                    WeaponTimeRemaining = weaponTimeDefault;
                                    WeaponSpeed = WeaponSpeedOriginal;
                                    shotMinTimer = 0.1f;
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
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
            shotTimer += elapsed;
            smogTimer += elapsed;
            mineTimer += elapsed;
            checkWeaponUpgradeExpire(elapsed);
            List<AProjectile> projectiles = StateManager.currentState.GetAllActorsOfClass<AProjectile>();
            foreach (AProjectile projectile in projectiles)
            {
                checkShotWallImpacts(projectile);
                checkShotEnemyImpacts(projectile);
            }
            List<AMine> mines = StateManager.currentState.GetAllActorsOfClass<AMine>();
            foreach (AMine mine in mines)
            {
                if (checkMineCollisions(mine.SpriteComponent))
                    checkMineSplashDamage(mine.SpriteComponent.WorldLocation);
            }

            checkPowerupPickups(gameTime);
        }
    }
}
