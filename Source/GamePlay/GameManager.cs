using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AbstractShooter
{
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
        private static bool isPauseUp = false;
        private static bool toPause = false;
        public static int lastPlayedLevel = 0;
        public static float TimeScale = 1F;
        public static Grid grid;

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
            EffectsManager.Initialize();
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
            foreach (Player play in ObjectManager.Players)
            {
                if (play.Invincibility <= 0)
                {
                    foreach (Enemy enemy in ObjectManager.Enemies)
                    {
                        if (enemy.ObjectBase.isVisible && enemy.ObjectBase.IsCircleColliding(play.ObjectBase.WorldCenter, play.ObjectBase.CollisionRadius))
                        {
                            EffectsManager.AddFlakesEffect(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 1, enemy.GetColor());
                            EffectsManager.AddExplosion(enemy.ObjectBase.WorldCenter, enemy.ObjectBase.Velocity / 1, Color.White);

                            enemy.Destroyed = true;

                            GameManager.addScore(0);
                            SoundsManager.PlayKill();
                            play.Hit();
                            play.ResetLocation();
                            //SoundsManager.PlaySpawn();
                            Multiplier = 1;
                            //Clean every weapon, shot and powerup that is in the current scene.
                            WeaponsAndFireManager.Shots.Clear();
                            WeaponsAndFireManager.Mines.Clear();
                            WeaponsAndFireManager.Smogs.Clear();
                            WeaponsAndFireManager.PowerUps.Clear();
                            WeaponsAndFireManager.CurrentWeaponType = WeaponsAndFireManager.WeaponType.Normal;
                            WeaponsAndFireManager.WeaponSpeed = WeaponsAndFireManager.WeaponSpeedOriginal;
                            WeaponsAndFireManager.shotMinTimer = WeaponsAndFireManager.shotMinTimerOriginal;
                            break;
                        }
                    }
                }
                if (play.Lives <= 0)
                {
                    StateManager.SetState(new States.GameOver());
                    //BinaryWriter
                    if (Endless)
                    {
                        Save();
                    }
                }
            }
        }
        public static bool checkIfPause(GamePadState gamepadState, KeyboardState keyState)
        {
            toPause = false;
            if (keyState.IsKeyUp(Keys.P) && gamepadState.Buttons.Start == ButtonState.Released)
            {
                isPauseUp = true;
            }
            else if (isPauseUp && (keyState.IsKeyDown(Keys.P) || gamepadState.Buttons.Start == ButtonState.Pressed))
            {
                toPause = true;
                isPauseUp = false;
            }
            else if (keyState.IsKeyDown(Keys.P) || gamepadState.Buttons.Start == ButtonState.Pressed)
            {
                isPauseUp = false;
            }
            return toPause;
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
            if (EnemySpawTimer <= 0 && ObjectManager.Enemies.Count() < 50)
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
            ObjectManager.Update(gameTime);
            WeaponsAndFireManager.Update(gameTime);
            EffectsManager.Update(gameTime);

            checkPlayerDeath();

            if (checkIfPause(GamePad.GetState(PlayerIndex.One), Keyboard.GetState()))
                StateManager.Pause(new States.Pause());

            if (!Endless && (TimeLeft <= 0 || EnemiesLeft <= 0))
                StateManager.SetState(new States.NextLevel());
        }
        public static void Draw()
        {
            Game1.game.GraphicsDevice.SetRenderTarget(Game1.renderTarget1);
            Game1.game.GraphicsDevice.Clear(Color.TransparentBlack);

            StateManager.spriteBatch.End();
            StateManager.spriteBatch.Begin(0, BlendState.AlphaBlend);

            //TileMap.Draw();
            grid.Draw();
            EffectsManager.Draw();
            ObjectManager.Draw();
            WeaponsAndFireManager.Draw();

            StateManager.spriteBatch.End();

            Game1.bloom.Draw(Game1.renderTarget1, Game1.renderTarget2);
            Game1.game.GraphicsDevice.SetRenderTarget(null);

            //Draw bloomed layer over top: 
            StateManager.spriteBatch.Begin(0, BlendState.AlphaBlend);
            StateManager.spriteBatch.Draw(Game1.renderTarget2, new Rectangle(0, 0, Game1.CurResolutionX, Game1.CurResolutionY), Color.White); // draw all glowing components            

            DrawUI();
        }
        public static void DrawUI()
        {
            float stringScale = 0.545F;
            string stringToDraw = "Score: " + GameManager.GetScore().ToString();
            Vector2 StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            StateManager.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1102 * Game1.ResolutionScale, 5 * Game1.ResolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

#if (DEBUG)
            stringToDraw = fps.ToString().Remove(3);
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * 0.8F;
            StateManager.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1111 * Game1.ResolutionScale, 58.5F * Game1.ResolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * 0.8F, SpriteEffects.None, 0);
#endif
            stringToDraw = "Multiplier: " + Multiplier;
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            StateManager.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.ResolutionScale, 5 * Game1.ResolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            if (!Endless)
            {
                stringToDraw = "Time : " + (int)TimeLeft;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                StateManager.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale)), 5 * Game1.ResolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "Mines: " + WeaponsAndFireManager.minesNumber;
            StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            StateManager.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.ResolutionScale, 690 * Game1.ResolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);


            if (NOfPlayers == 1)
            {
                stringToDraw = "Lives: " + ObjectManager.Players[0].Lives;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                StateManager.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1154 * Game1.ResolutionScale, 690 * Game1.ResolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else //if (NOfPlayers == 2)
            {
                stringToDraw = "Lives: " + ObjectManager.Players[0].Lives + "/" + ObjectManager.Players[1].Lives;
                StringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                StateManager.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1104 * Game1.ResolutionScale, 690 * Game1.ResolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
        }

        public static void ClearSave()
        {
            HiScore = 0;
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                BinaryWriter w = new BinaryWriter(stream);
                w.Write(HiScore);
                w.Write(false);
                w.Write(false);
                w.Write(false);
                stream.Close();
                w.Close();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public static void Save()
        {
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                BinaryWriter w = new BinaryWriter(stream);
                w.Write(HiScore);
                w.Write(Game1.IsMaxResolution);
                w.Write(Game1.IsFullScreen);
                w.Write(Game1.IsBorderless);
                stream.Close();
                w.Close();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public static void Load()
        {
            try
            {
                Stream stream = new FileStream("Save.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader r = new BinaryReader(stream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);
                HiScore = r.ReadInt32();
                Game1.IsMaxResolution = r.ReadBoolean();
                Game1.IsFullScreen = r.ReadBoolean();
                Game1.IsBorderless = r.ReadBoolean();
                r.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex.ToString());
#endif
            }
        }
    }
}
