using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    static class WeaponsAndFireManager
    {
        #region Declarations
        static public List<ParticleEntity> Shots = new List<ParticleEntity>();
        static public List<Particle> Mines = new List<Particle>();
        static public List<Particle> Smogs = new List<Particle>();
        //static public List<Particle> Stars = new List<Particle>();
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
        static public List<Particle> PowerUps = new List<Particle>();
        static private int maxActivePowerups = 3;
        static private Random rand = new Random();
        #endregion

        public static void Initialize()
        {
            Shots.Clear();
            Mines.Clear();
            Smogs.Clear();
            CurrentWeaponType = WeaponType.Normal;
            WeaponSpeed = WeaponSpeedOriginal;
            shotMinTimer = shotMinTimerOriginal;
            minesNumber = 3;
            WeaponTimeRemaining = 15.0f;
            PowerUps.Clear();
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
                return (shotTimer >= WeaponFireDelay);
            }
        }
        static public bool CanFireSmog
        {
            get
            {
                return (smogTimer >= smogMinTimer);
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
            Color temp = GetShotColor();
            direction.Normalize();
            location += direction * 21F; //Makes it start a bit distant from the player
            ParticleEntity shot = new ParticleEntity(
                location,
                StateManager.State.spriteSheet,
                shotRectangle,
                velocity,
                Vector2.Zero,
                4000f,
                90,
                GetShotColor(),
                new Color(temp.R / 3, temp.G / 3, temp.B / 3));

            shot.ObjectBase.scale = 0.9f;
            shot.ObjectBase.CollisionRadius = shot.ObjectBase.scale * shotRectangle.Height / 4.0f;
            shot.ObjectBase.RotateTo(direction);
            Shots.Add(shot);
        }
        private static void AddMine(Vector2 location, Vector2 velocity)
        {
            //Can't have more than 6 Mines at the time in the game.
            if (Mines.Count == 6)
            {
                Mines.RemoveAt(0);
            }

            Particle mine = new Particle(
                location,
                StateManager.State.spriteSheet,
                mineRectangle,
                velocity * 0.5F,
                Vector2.Zero,
                2000,
                920,
                Color.White,
                Color.White);

            mine.AddFrame(mineRectangle2);
            mine.Reanimate = true;

            mine.CollisionRadius *= 1.6f;
            Mines.Add(mine);
        }
        private static void AddSmog(Vector2 location, Vector2 direction)
        {
            Particle smog = new Particle(
                location,
                StateManager.State.spriteSheet,
                smogRectangle,
                direction,
                Vector2.Zero,
                0,
                108,
                Color.White,
                Color.Black);

            smog.AddFrame(new Rectangle(smogRectangle.X + smogRectangle.Width, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height));
            smog.AddFrame(new Rectangle(smogRectangle.X + smogRectangle.Width * 2, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height));

            smog.RotateTo(direction);
            Smogs.Add(smog);
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
            if (Mines.Count() > 0)
            {
                SoundsManager.PlayExplosion();

                foreach (Particle sprite in Mines)
                {
                    EffectsManager.AddMineExplosion(sprite.WorldCenter);
                    checkMineSplashDamage(sprite.WorldCenter);
                    sprite.Expired = true;
                }
                Mines.Clear();
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
            Particle newPowerup = new Particle(Position, 1.0f, StateManager.State.spriteSheet, powerupRectangle, Vector2.Zero, 560);

            newPowerup.StartFlashingAt = 165F;

            newPowerup.AddFrame(new Rectangle(powerupRectangle.X + (powerupRectangle.Width * 1), powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height));
            newPowerup.AddFrame(new Rectangle(powerupRectangle.X + (powerupRectangle.Width * 2), powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height));
            newPowerup.AddFrame(new Rectangle(powerupRectangle.X + (powerupRectangle.Width * 3), powerupRectangle.Y, powerupRectangle.Width, powerupRectangle.Height));

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
                newPowerup.Frame = 0;
            else if (type == WeaponType.Quintuple)
                newPowerup.Frame = 1;
            else if (type == WeaponType.Ultra)
                newPowerup.Frame = 2;
            else if (type == WeaponType.Mine)
                newPowerup.Frame = 3;

            if (PowerUps.Count >= maxActivePowerups)
            {
                if (newPowerup.Frame > PowerUps[0].Frame)
                {
                    PowerUps.Add(newPowerup);
                    PowerUps.RemoveAt(0);
                }
            }
            else
                PowerUps.Add(newPowerup);

            SoundsManager.PlayPowerUp();
        }
        #endregion

        #region Collision Detection
        private static void checkShotEnemyImpacts(Sprite shot)
        {
            if (shot.Expired)
            {
                return;
            }
            foreach (Enemy enemy in ObjectManager.Enemies)
            {
                if (!enemy.Destroyed)
                {
                    if (shot.IsCircleColliding(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.CollisionRadius))
                    {
                        shot.Expired = true;
                        enemy.Hit();

                        EffectsManager.AddFlakesEffect(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 30, GetShotColor());

                        //if dead
                        if (enemy.Lives <= 0)
                        {
                            SoundsManager.PlayKill();
                            enemy.Destroyed = true;
                            GameManager.addScore(10);
                            GameManager.growMultiplier();
                            EffectsManager.AddExplosion(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 30, enemy.GetColor());

                            //Spawn PowerUp
                            if (rand.Next(0, 7) == 3)
                                tryToSpawnPowerup(enemy.ObjectBase.WorldLocation);
                        }
                        else
                        {
                            SoundsManager.PlayHit();
                        }
                    }
                }
            }
        }
        private static bool checkMineCollisions(Sprite mine)
        {
            if (TileMap.IsWallTile(TileMap.GetSquareAtPixel(mine.WorldCenter)))
            {
                mine.Expired = true;
                EffectsManager.AddMineExplosion(mine.WorldCenter);
                SoundsManager.PlayExplosion();
                return true;
            }
            else
            {
                foreach (Enemy enemy in ObjectManager.Enemies)
                {
                    if ((enemy is EnemySeeker || enemy is EnemyWanderer) && !enemy.Destroyed)
                    {
                        if (mine.IsCircleColliding(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.CollisionRadius))
                        {
                            mine.Expired = true;
                            EffectsManager.AddMineExplosion(enemy.ObjectBase.WorldCenter);
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

            foreach (Enemy enemy in ObjectManager.Enemies)
            {
                if (enemy.ObjectBase.isVisible && !enemy.Destroyed)
                {
                    if (enemy.ObjectBase.IsCircleColliding(location, mineSplashRadius))
                    {
                        if (!played1)
                        {
                            played1 = true;
                            SoundsManager.PlayHit();
                        }

                        if (GameManager.CurrentDifficulty <= 1)
                        {
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                        }
                        else if (GameManager.CurrentDifficulty == 2)
                        {
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                        }
                        else if (GameManager.CurrentDifficulty >= 3)
                        {
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                            enemy.Hit();
                        }

                        //if dead
                        if (enemy.Lives <= 0)
                        {
                            if (!played2)
                            {
                                played2 = true;
                                SoundsManager.PlayKill();
                            }
                            enemy.Destroyed = true;
                            GameManager.addScore(10);
                            GameManager.growMultiplier();
                            EffectsManager.AddExplosion(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 30, enemy.GetColor());

                            //Spawn PowerUp
                            if (rand.Next(0, 12) == 3)
                                tryToSpawnPowerup(enemy.ObjectBase.WorldLocation);
                        }
                    }
                }
            }
        }
        private static void checkShotWallImpacts(Sprite shot)
        {
            if (shot.Expired)
            {
                return;
            }
            if (TileMap.IsWallTile(TileMap.GetSquareAtPixel(shot.WorldCenter)))
            {
                shot.Expired = true;
                EffectsManager.AddSparksEffect(shot.WorldCenter, shot.Velocity);
            }
        }
        private static void checkPowerupPickups(GameTime gameTime)
        {
            foreach (Player play in ObjectManager.Players)
            {
                for (int x = PowerUps.Count - 1; x >= 0; x--)
                {
                    PowerUps[x].Update(gameTime);
                    if (PowerUps[x].Expired)
                        PowerUps.RemoveAt(x);
                    else if (play.ObjectBase.IsCircleColliding(PowerUps[x].WorldCenter, PowerUps[x].CollisionRadius))
                    {
                        switch (PowerUps[x].Frame)
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
                        PowerUps.RemoveAt(x);
                        SoundsManager.PlayPowerUp();
                        break;
                    }
                }
            }
        }
        #endregion

        #region Update and Draw
        static public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
            shotTimer += elapsed;
            smogTimer += elapsed;
            mineTimer += elapsed;
            checkWeaponUpgradeExpire(elapsed);
            for (int x = Shots.Count - 1; x >= 0; x--)
            {
                Shots[x].Update(gameTime);
                checkShotWallImpacts(Shots[x].ObjectBase);
                checkShotEnemyImpacts(Shots[x].ObjectBase);
                if (Shots[x].ObjectBase.Expired)
                    Shots.RemoveAt(x);
            }
            for (int x = Smogs.Count - 1; x >= 0; x--)
            {
                Smogs[x].Update(gameTime);
                if (Smogs[x].Expired)
                    Smogs.RemoveAt(x);
            }
            for (int x = Mines.Count - 1; x >= 0; x--)
            {
                Mines[x].Update(gameTime);
                if (checkMineCollisions(Mines[x]))
                    checkMineSplashDamage(Mines[x].WorldLocation);
                if (Mines[x].Expired)
                    Mines.RemoveAt(x);
            }

            checkPowerupPickups(gameTime);
        }

        static public void Draw()
        {
            foreach (Particle sprite in Smogs)
            {
                sprite.Draw();
            }

            foreach (Particle sprite in PowerUps)
            {
                sprite.Draw();
            }

            foreach (Particle sprite in Mines)
            {
                sprite.Draw();
            }

            foreach (ParticleEntity sprite in Shots)
            {
                sprite.Draw();
            }
        }
        #endregion
    }
}
