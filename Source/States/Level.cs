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
        protected int EnemiesLeft;
        protected bool isEndless = false;
        protected int levelIndex = 0;
        public bool isLastLevel = false;
        public int maxInGameEnemies = 0;

        //Gamemanger
        public int CurrentDifficulty = 1; //1 = Super Easy, 2 = Easy, 3 = Medium, 4 = Hard, 5 = Extreme
        private int Multiplier = 1;
        //Time between enemy spawnings
        private float EnemySpawTimerOriginal = 1.6666F;
        //Time to win the level
        //Endless mode (Time and EnemiesLeft are ignored)
        //Enemies left to kill
        private int EnemiesSpawned = 0;
        //Enemies max in game
        private int EnemiesMax = 100;
        private Random rand = new Random();
        public Grid grid;
        private ActionBinding pauseAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });

        public override void OnSetAsCurrentState()
        {
            base.OnSetAsCurrentState();
            SoundsManager.ResumeMusic();
            InputManager.CaptureMouse = true;
            InputManager.HideMouseWhenNotUsed = true;
        }

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
            ResetScore();
            EnemySpawTimerOriginal = EnemySpawTimer;
            EnemiesSpawned = 0;

            ParticlesManager.Initialize();
            TileMap.Initialize(levelDimensionX, levelDimensionY);
            grid = new Grid();
            grid.Initialize(levelDimensionX, levelDimensionY);
            WeaponsAndFireManager.Initialize();
            
            if (NOfPlayers == 1)
            {
                APlayerActor P1 = new APlayerActor(new Vector2(((Level)StateManager.currentState).LevelDimensionX / 2.0f, ((Level)StateManager.currentState).LevelDimensionY / 2.0f));
            }
            else if (NOfPlayers == 2)
            {
                APlayerActor P1 = new APlayerActor(new Vector2(((Level)StateManager.currentState).LevelDimensionX / 2.0f, ((Level)StateManager.currentState).LevelDimensionY / 2.0f));
                //Adds a second player
                APlayerActor P2 = new APlayerActor(new Vector2(((Level)StateManager.currentState).LevelDimensionX / 2.0f, ((Level)StateManager.currentState).LevelDimensionY / 2.0f));
            }

            Camera.TopLeft = Vector2.Zero;

            //Avoids bugs with the player in case the music wasn't stopped
            SoundsManager.PauseMusic();
            SoundsManager.ResumeMusic();
            SoundsManager.PlayMusic();
            //SoundsManager.PlaySpawn();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!isEndless && TimeLeft > 0)
            {
                TimeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }

            //Spawn Enemies
            if (EnemySpawTimer <= 0 && StateManager.currentState.GetAllActorsOfType<AEnemyActor>().Count() < maxInGameEnemies)
            {
                if (isEndless)
                {
                    AddEnemy(RandomLocation(), rand.Next(1, 7), CurrentDifficulty);
                    EnemySpawTimer = EnemySpawTimerOriginal;
                    EnemiesSpawned++;
                }
                else if (EnemiesSpawned < EnemiesMax)
                {
                    AddEnemy(RandomLocation(), rand.Next(1, 7), CurrentDifficulty);
                    EnemySpawTimer = EnemySpawTimerOriginal;
                    EnemiesSpawned++;
                }
            }
            else
            {
                EnemySpawTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }

            grid.Update(gameTime);
            WeaponsAndFireManager.Update(gameTime);
            ParticlesManager.Update(gameTime);

            //checkPlayerDeath();

            if (TimeLeft <= 0 || EnemiesLeft <= 0)
            {
                if (isLastLevel || isEndless)
                {
                    StateManager.CreateAndSetState<States.GameOver>();
                }
                else
                {
                    StateManager.CreateAndSetState<States.NextLevel>().TimeLeft = TimeLeft;
                }
            }
            else if (pauseAction.CheckBindings())
                StateManager.Pause();
        }

        public override void BeginDraw()
        {
            Game1.Get.GraphicsDevice.SetRenderTarget(Game1.renderTarget1);
            Game1.Get.GraphicsDevice.Clear(Color.TransparentBlack);

            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            //TileMap.Draw();
            grid.Draw();
        }
        public override void EndDraw()
        {
            ParticlesManager.Draw();

            Game1.spriteBatch.End();

            Game1.bloom.Draw(Game1.renderTarget1, Game1.renderTarget2);

            Game1.Get.GraphicsDevice.SetRenderTarget(null);

            //Draw bloomed layer over top: 
            Game1.spriteBatch.Begin(0, BlendState.AlphaBlend);
            Game1.spriteBatch.Draw(Game1.renderTarget2, new Rectangle(0, 0, Game1.currentResolution.X, Game1.currentResolution.Y), Color.White); // draw all glowing components            

            DrawUI();

            base.EndDraw();
        }
        public void DrawUI()
        {
            float stringScale = 0.545F;
            string stringToDraw = "Score: " + GetScore().ToString();

            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(Game1.currentResolution.X * 0.86094F, Game1.currentResolution.Y * 0.00694F),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            stringToDraw = "Multiplier: " + Multiplier;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(Game1.currentResolution.X * 0.01328F, Game1.currentResolution.Y * 0.00694F),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            if (!isEndless)
            {
                Vector2 stringSize;
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
                    new Vector2(((Game1.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * Game1.ResolutionScale)), Game1.currentResolution.Y * 0.00694F),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

            stringToDraw = "Mines: " + WeaponsAndFireManager.minesNumber;
            Game1.spriteBatch.DrawString(Game1.defaultFont,
                stringToDraw,
                new Vector2(Game1.currentResolution.X * 0.01328F, Game1.currentResolution.Y * 0.9583F),
                Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);

            List<APlayerActor> player = StateManager.currentState.GetAllActorsOfType<APlayerActor>();
            if (NOfPlayers == 1 && player.Count == 1)
            {
                stringToDraw = "Lives: " + player[0].Lives;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(Game1.currentResolution.X * 0.9015625F, Game1.currentResolution.Y * 0.9583F),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }
            else if (NOfPlayers == 2 && player.Count == 2)
            {
                stringToDraw = "Lives: " + StateManager.currentState.GetAllActorsOfType<APlayerActor>()[0].Lives + "/" + StateManager.currentState.GetAllActorsOfType<APlayerActor>()[1].Lives;
                Game1.spriteBatch.DrawString(Game1.defaultFont,
                    stringToDraw,
                    new Vector2(Game1.currentResolution.X * 0.8625F, Game1.currentResolution.Y * 0.9583F),
                    Color.WhiteSmoke, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0);
            }

#if DEBUG
            Game1.DrawFPS();
#endif
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
        public void AddScore(int toAdd)
        {
            GameInstance.Score += toAdd * Multiplier;
            if (!isEndless)
                EnemiesLeft--;
        }
        public void GrowMultiplier()
        {
            Multiplier++;
        }
        public void ResetScore()
        {
            GameInstance.Score = 0;
            Multiplier = 1;
        }
        ///Generates a random location for the enemy to spawn where you can't see, within levels boundaries
        public Vector2 RandomLocation()
        {
            float newX = 0;
            float newY = 0;
            //Avoids infinite attempts in case the map is too small.
            int counter = 0;

            while (counter < 40 &&
                (newX < ((Level)StateManager.currentState).grid.NodeSize
                || newX > (((Level)StateManager.currentState).grid.gridWidth - 3) * ((Level)StateManager.currentState).grid.NodeSize
                || newY < ((Level)StateManager.currentState).grid.NodeSize
                || newY > (((Level)StateManager.currentState).grid.gridHeight - 3) * ((Level)StateManager.currentState).grid.NodeSize))
            {
                int RandSide = rand.Next(0, 4);
                float RandPosition = rand.Next(-50, 51) / 100.0f; //Rand between -0.5 and 0.5
                
                List<APlayerActor> players = StateManager.currentState.GetAllActorsOfType<APlayerActor>();

                if (players.Count > 0) //To improve
                {
                    if (RandSide == 0) //Spawn from left of player
                    {
                        newX = players[0].WorldLocation.X - (Camera.ViewPortWidth / 2) - 45;
                        newY = players[0].WorldLocation.Y + (Camera.ViewPortHeight * RandPosition);
                    }
                    else if (RandSide == 1) //Spawn from right of player
                    {
                        newX = players[0].WorldLocation.X + (Camera.ViewPortWidth / 2) + 45;
                        newY = players[0].WorldLocation.Y + (Camera.ViewPortHeight * RandPosition);
                    }
                    else if (RandSide == 2) //Spawn from top of player
                    {
                        newX = players[0].WorldLocation.X + (Camera.ViewPortWidth * RandPosition);
                        newY = players[0].WorldLocation.Y - (Camera.ViewPortHeight / 2) - 30;
                    }
                    else //if (RandSide == 3) //Spawn from bottom of player
                    {
                        newX = players[0].WorldLocation.X + (Camera.ViewPortWidth * RandPosition);
                        newY = players[0].WorldLocation.Y + (Camera.ViewPortHeight / 2) + 30;
                    }
                }
                counter++;
            }

            return new Vector2(newX, newY);
        }
        public void AddEnemy(Vector2 squareLocation, int type, int currentDifficulty)
        {
            int startX = (int)squareLocation.X;
            int startY = (int)squareLocation.Y;
            Rectangle squareRect = grid.SquareWorldRectangle(startX, startY).ToRectangle();

            int speed = 63;
            if (currentDifficulty == 1)
                speed = 63;
            else if (currentDifficulty == 2)
                speed = 70;
            else if (currentDifficulty == 3)
                speed = 79;
            else if (currentDifficulty == 4)
                speed = 83;
            else if (currentDifficulty == 5)
                speed = 98;

            //ASkeletalEnemy newEnemy = new ASkeletalEnemy(squareLocation, speed);
            if (rand.Next(0, 5) == 4)
            {
                AEnemyWandererActor newEnemy = new AEnemyWandererActor(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
            }
            else
            {
                AEnemySeekerActor newEnemy = new AEnemySeekerActor(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
            }
        }
    }
}