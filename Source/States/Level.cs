using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using InputManagement;

namespace AbstractShooter.States
{
    public enum LevelType
    {
        Endless, //Randomized
        //Other types...
        Campaign,
        CampaignBoss,
        CampaignBonus //Extra Bonus State
    }

    public class Level : State
    {
        protected int Difficulty = 1;
        protected int NOfPlayers = 1;
        protected float EnemySpawTimer = 0.55f;
        protected int levelDimensionX = 2330;
        protected int levelDimensionY = 1430;
        public int LevelDimensionX { get { return levelDimensionX; } }
        public int LevelDimensionY { get { return levelDimensionY; } }
        protected float TimeLeft = 60; //private
        protected int EnemiesLeft = 0;
        protected bool isEndless = false;
        protected int levelIndex = 0;
        public bool isLastLevel = false; 

        //Gamemanger
        public int CurrentDifficulty = 1; //1 = Super Easy, 2 = Easy, 3 = Medium, 4 = Hard, 5 = Extreme
        private int Multiplier = 1;
        //Time between enemy spawnings
        private float EnemySpawTimerOriginal = 1.6666F;
        //Time to win the level
        //Endless mode (Time and EnemiesLeft are ignored)
        private bool Endless = false;
        //Enemies left to kill
        private int EnemiesSpawned = 0;
        //Enemies max in game
        private int EnemiesMax = 100;
        private Random rand = new Random();
        public Grid grid;
        private ActionBinding pauseAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });

        public override void Initialize()
        {
            base.Initialize();
            spriteSheet = Game1.Get.Content.Load<Texture2D>(@"Textures\SpriteSheet");
            //((Level)StateManager.currentState).Initialize(Difficulty, NOfPlayers, EnemySpawTimer, LevelDimensionX, LevelDimensionY, TimeLeft, EnemiesLeft, isEndless);
            if (!isEndless)
            {
                GameInstance.lastPlayedLevel = levelIndex;
            }
            
            CurrentDifficulty = Difficulty;
            resetScore();
            EnemySpawTimerOriginal = EnemySpawTimer;
            EnemiesSpawned = 0;

            TileMap.Initialize(levelDimensionX, levelDimensionY);
            grid = new Grid();
            grid.Initialize(levelDimensionX, levelDimensionY);
            WeaponsAndFireManager.Initialize();
            ObjectManager.Initialize(NOfPlayers);
            Camera.Position = Vector2.Zero;

            //Avoids bugs with the player in case the music wasn't stopped
            SoundsManager.PauseMusic();
            SoundsManager.ResumeMusic();
            SoundsManager.PlayMusic();
            //SoundsManager.PlaySpawn();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!Endless && TimeLeft > 0)
            {
                TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }

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
                EnemySpawTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }

            grid.Update();
            //ObjectManager.Update(gameTime);
            WeaponsAndFireManager.Update(gameTime);
            //EffectsManager.Update(gameTime);

            checkPlayerDeath();

            if (TimeLeft <= 0 || EnemiesLeft <= 0)
            {
                if (isLastLevel || isEndless)
                {
                    StateManager.CreateAndSetState<States.GameOver>();
                }
                else
                {
                    StateManager.CreateAndSetState<States.NextLevel>();
                    (StateManager.nextState as States.NextLevel).TimeLeft = TimeLeft;
                }
            }
            else if (pauseAction.CheckBindings())
                StateManager.Pause();
        }
        public override void Draw()
        {
            Game1.spriteBatch.End();

            Game1.Get.GraphicsDevice.SetRenderTarget(Game1.renderTarget1);
            Game1.Get.GraphicsDevice.Clear(Color.TransparentBlack);

            Game1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            //TileMap.Draw();
            grid.Draw();

            base.Draw();

            Game1.spriteBatch.End();

            Game1.bloom.Draw(Game1.renderTarget1, Game1.renderTarget2);
            Game1.Get.GraphicsDevice.SetRenderTarget(null);

            //Draw bloomed layer over top: 
            Game1.spriteBatch.Begin(0, BlendState.AlphaBlend);
            Game1.spriteBatch.Draw(Game1.renderTarget2, new Rectangle(0, 0, Game1.curResolutionX, Game1.curResolutionY), Color.White); // draw all glowing components            

            DrawUI();
        }
        
        public int GetScore()
        {
            return GameInstance.Score;
        }
        public int GetMultiplier()
        {
            return Multiplier;
        }
        public int GetMaxNOfPlayers()
        {
            return 4; //GamePad.MaximumGamePadCount + 1;
        }
        public int GetNOfPlayers()
        {
            return NOfPlayers;
        }
        public void addScore(int toAdd)
        {
            GameInstance.Score += toAdd * Multiplier;
            if (!Endless)
                EnemiesLeft--;
        }
        public void growMultiplier()
        {
            Multiplier++;
        }
        public void resetScore()
        {
            GameInstance.Score = 0;
            Multiplier = 1;
        }
        public void checkPlayerDeath()
        {
            List<AEnemy> asd = new List<AEnemy>();
            foreach (APlayer play in StateManager.currentState.GetAllActorsOfClass<APlayer>())
            {
                if (play.Invincibility <= 0)
                {
                    foreach (AEnemy enemy in StateManager.currentState.GetAllActorsOfClass<AEnemy>())
                    {
                        if (enemy.RootComponent.IsInViewport && enemy.SpriteComponent.IsCircleColliding(play.RootComponent.WorldCenter, play.RootComponent.CollisionRadius))
                        {
                            EffectsManager.AddFlakesEffect(enemy.RootComponent.WorldCenter, enemy.RootComponent.localVelocity / 1, enemy.GetColor());
                            EffectsManager.AddExplosion(enemy.RootComponent.WorldCenter, enemy.RootComponent.localVelocity / 1, Color.White);

                            asd.Add(enemy);
                            enemy.Destroy();

                            addScore(0);
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

#if (DEBUG)
        private double fps;
#endif
        public void DrawUI()
        {
            float stringScale = 0.545F;
            string stringToDraw = "Score: " + GetScore().ToString();
            Vector2 stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;

            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1102 * Game1.resolutionScale, 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

#if (DEBUG)
            if (fps.ToString().Length > 3)
                stringToDraw = fps.ToString().Remove(3);
            else
                stringToDraw = fps.ToString();
            stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * 0.8F;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(1111 * Game1.resolutionScale, 58.5F * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * 0.8F, SpriteEffects.None, 0);
#endif
            stringToDraw = "Multiplier: " + Multiplier;
            stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.resolutionScale, 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            if (!Endless)
            {
                stringToDraw = "Time : " + (int)TimeLeft;
                if (TimeLeft >= 100)
                {
                    stringSize = Game1.defaultFont.MeasureString("Time : 999") * Game1.defaultFontScale * stringScale;
                }
                else
                {
                    stringSize = Game1.defaultFont.MeasureString("Time : 99") * Game1.defaultFontScale * stringScale;
                }
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(((Game1.curResolutionX / 2.0f) - ((stringSize.X / 2F) * Game1.resolutionScale)), 5 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "Mines: " + WeaponsAndFireManager.minesNumber;
            stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(17 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            List<APlayer> player = StateManager.currentState.GetAllActorsOfClass<APlayer>();
            if (NOfPlayers == 1 && player.Count == 1)
            {
                stringToDraw = "Lives: " + player[0].Lives;
                stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1154 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else if (NOfPlayers == 2 && player.Count == 2)
            {
                stringToDraw = "Lives: " + StateManager.currentState.GetAllActorsOfClass<APlayer>()[0].Lives + "/" + StateManager.currentState.GetAllActorsOfClass<APlayer>()[1].Lives;
                stringSize = Game1.defaultFont.MeasureString(stringToDraw) * Game1.defaultFontScale * stringScale;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(1104 * Game1.resolutionScale, 690 * Game1.resolutionScale),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.resolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
        }
    }
}