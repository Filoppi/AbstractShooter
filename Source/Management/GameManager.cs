using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InputManagement;

namespace AbstractShooter
{
    //public enum InputModes
    //{
    //    KeyboardMouse,
    //    Keyboard,
    //    Mouse,
    //    GamePad,
    //    KeyboardMouseGamePad1, //Allows the use of everything together
    //    KeyboardMouseGamePads, //Allows the use of everything together for Multiplayer
    //    None
    //};
    public enum DrawGroup
    {
        FarBackground,
        Background,
        Particles,
        PassiveObjects,
        Default,
        Powerups,
        Mines,
        Characters,
        Shots,
        Players,
        Foreground,
        UI

    }

    static class GameManager
    {
        public static int CurrentDifficulty = 1; //1 = Super Easy, 2 = Easy, 3 = Medium, 4 = Hard, 5 = Extreme
        private static int NOfPlayers = 1;
        public static int Score = 0;
        public static int HiScore = 0;
        private static int Multiplier = 1;
        //Time between enemy spawnings
        private static float EnemySpawTimerOriginal = 1.6666F;
        private static float EnemySpawTimer = 1.6666F;
        //Time to win the level
        public static float TimeLeft = 60.0f;
        //Endless mode (Time and EnemiesLeft are ignored)
        private static bool Endless = false;
        //Enemies left to kill
        private static int EnemiesLeft = 100;
        private static int EnemiesSpawned = 0;
        //Enemies max in game
        private static int EnemiesMax = 100;
        public static int LevelDimensionX = 1000;
        public static int LevelDimensionY = 1000;
        private static Random rand = new Random();
        public static int lastPlayedLevel = 0;
        public static float TimeScale = 1F;
        public static Grid grid;
        private static ActionBinding pauseAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });

        public static void Initialize(int Difficulty, int NewNOfPlayers, float newEnemySpawTimerOriginal, int levelDimensionX, int levelDimensionY, float newTimeLeft, int newEnemiesLeft, bool newEndless)
        {
            CurrentDifficulty = Difficulty;
            NOfPlayers = NewNOfPlayers;
            resetScore();
            EnemySpawTimerOriginal = newEnemySpawTimerOriginal;
            EnemySpawTimer = EnemySpawTimerOriginal;
            EnemiesLeft = newEnemiesLeft;
            EnemiesSpawned = 0;
            EnemiesMax = newEnemiesLeft;
            TimeLeft = newTimeLeft;
            Endless = newEndless;
            lastPlayedLevel = 0;
            LevelDimensionX = levelDimensionX;
            LevelDimensionY = levelDimensionY;

            TileMap.Initialize(LevelDimensionX, LevelDimensionY);
            grid = new Grid();
            grid.Initialize(LevelDimensionX, LevelDimensionY);
            WeaponsAndFireManager.Initialize();
            ObjectManager.Initialize(NewNOfPlayers);
            Camera.Position = Vector2.Zero;

            //Avoids bugs with the player in case the music wasn't stopped
            SoundsManager.PauseMusic();
            SoundsManager.ResumeMusic();
            SoundsManager.PlayMusic();
            //SoundsManager.PlaySpawn();
        }

        #region Public GetSet Methos
        public static int GetScore()
        {
            return Score;
        }
        public static int GetMultiplier()
        {
            return Multiplier;
        }
        public static int GetMaxNOfPlayers()
        {
            return 4; //GamePad.MaximumGamePadCount + 1;
        }
        public static int GetNOfPlayers()
        {
            return NOfPlayers;
        }
        public static void addScore(int toAdd)
        {
            Score += toAdd * Multiplier;
            if (!Endless)
                EnemiesLeft--;
        }
        public static void growMultiplier()
        {
            Multiplier++;
        }
        public static void resetScore()
        {
            Score = 0;
            Multiplier = 1;
        }
        public static void checkPlayerDeath()
        {
            foreach (APlayer play in StateManager.currentState.GetAllActorsOfClass<APlayer>())
            {
                if (play.Invincibility <= 0)
                {
                    foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
                    {
                        if (enemy.SpriteComponent.IsInViewport && enemy.SpriteComponent.IsCircleColliding(play.SpriteComponent.WorldCenter, play.SpriteComponent.CollisionRadius))
                        {
                            EffectsManager.AddFlakesEffect(enemy.SpriteComponent.WorldCenter, enemy.RootComponent.velocity / 1, enemy.GetColor());
                            EffectsManager.AddExplosion(enemy.SpriteComponent.WorldCenter, enemy.RootComponent.velocity / 1, Color.White);

                            enemy.Destroy();

                            GameManager.addScore(0);
                            SoundsManager.PlayKill();
                            play.Hit();
                            play.ResetLocation();
                            //SoundsManager.PlaySpawn();
                            Multiplier = 1;
                            //Clean every weapon, shot and powerup that is in the current scene.
                            List<AMine> mines = StateManager.currentState.GetAllActorsOfClass<AMine>();
                            foreach (AMine mine in mines)
                            {
                                mine.Destroy();
                            }
                            List<APowerUp> powerUps = StateManager.currentState.GetAllActorsOfClass<APowerUp>();
                            foreach (APowerUp powerUp in powerUps)
                            {
                                powerUp.Destroy();
                            }
                            List<AProjectile> projectiles = StateManager.currentState.GetAllActorsOfClass<AProjectile>();
                            foreach (AProjectile projectile in projectiles)
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
                if (play.Lives <= 0)
                {
                    StateManager.CreateAndSetState<States.GameOver>();
                    //BinaryWriter
                    if (Endless)
                    {
                        SaveManager.Save();
                    }
                }
            }
        }
        #endregion

#if (DEBUG)
        private static double fps;
#endif
        public static void Update(GameTime gameTime)
        {
            if (TimeLeft > 0 && !Endless)
                TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;

#if (DEBUG)
            fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
#endif
            //Spawn Enemies
            if (EnemySpawTimer <= 0 && StateManager.currentState.GetAllActorsOfClass<AEnemy>().Count() < 50)
            {
                if (Endless)
                {
                    ObjectManager.AddEnemy(ObjectManager.RandomLocation(), rand.Next(1, 7), CurrentDifficulty);
                    EnemySpawTimer = EnemySpawTimerOriginal;
                    EnemiesSpawned++;
                }
                else if (EnemiesSpawned < EnemiesMax)
                {
                    ObjectManager.AddEnemy(ObjectManager.RandomLocation(), rand.Next(1, 7), CurrentDifficulty);
                    EnemySpawTimer = EnemySpawTimerOriginal;
                    EnemiesSpawned++;
                }
            }
            else
            {
                EnemySpawTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
            }

            grid.Update();
            //ObjectManager.Update(gameTime);
            WeaponsAndFireManager.Update(gameTime);
            //EffectsManager.Update(gameTime);

            checkPlayerDeath();

            if (!Endless && (TimeLeft <= 0 || EnemiesLeft <= 0))
                StateManager.CreateAndSetState<States.NextLevel>();
            else if (pauseAction.CheckBindings())
                StateManager.Pause();
        }
        public static void Draw()
        {
            Game1.spriteBatch.End();

            Game1.Get.GraphicsDevice.SetRenderTarget(Game1.renderTarget1);
            Game1.Get.GraphicsDevice.Clear(Color.TransparentBlack);

            Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            //TileMap.Draw();
            grid.Draw();

            StateManager.currentState.DrawActors();

            Game1.spriteBatch.End();

            Game1.bloom.Draw(Game1.renderTarget1, Game1.renderTarget2);
            Game1.Get.GraphicsDevice.SetRenderTarget(null);

            //Draw bloomed layer over top: 
            Game1.spriteBatch.Begin(0, BlendState.AlphaBlend);
            Game1.spriteBatch.Draw(Game1.renderTarget2, new Rectangle(0, 0, Game1.curResolutionX, Game1.curResolutionY), Color.White); // draw all glowing components            

            DrawUI();
        }
        public static void DrawUI()
        {
            float stringScale = 0.545F;
            string stringToDraw = "Score: " + GameManager.GetScore().ToString();
            Vector2 StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1102 * Game1.resolutionScale, 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

#if (DEBUG)
            if (fps.ToString().Length > 3)
                stringToDraw = fps.ToString().Remove(3);
            else
                stringToDraw = fps.ToString();
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * 0.8F;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1111 * Game1.resolutionScale, 58.5F * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * 0.8F, SpriteEffects.None, 0);
#endif
            stringToDraw = "Multiplier: " + Multiplier;
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.resolutionScale, 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            if (!Endless)
            {
                stringToDraw = "Time : " + (int)TimeLeft;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.resolutionScale)), 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "Mines: " + WeaponsAndFireManager.minesNumber;
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            List<APlayer> player = StateManager.currentState.GetAllActorsOfClass<APlayer>();
            if (NOfPlayers == 1 && player.Count == 1)
            {
                stringToDraw = "Lives: " + player[0].Lives;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1154 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else if (NOfPlayers == 2 && player.Count == 2)
            {
                stringToDraw = "Lives: " + StateManager.currentState.GetAllActorsOfClass<APlayer>()[0].Lives + "/" + StateManager.currentState.GetAllActorsOfClass<APlayer>()[1].Lives;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1104 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
        }
    }
}
