using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UnrealMono;

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
		public int maxInGameEnemies = 100;

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

        //Power up declarations
	    protected Rectangle powerupRectangle = new Rectangle(64, 128, 32, 32);
        private int maxActivePowerups = 3;

        public Grid grid;
		public GridNew gridNew;
		private readonly ActionBinding pauseAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Escape, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) }); //TO1 make all readonly

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
			spriteSheet = UnrealMonoGame.Get.Content.Load<Texture2D>(@"Texture\SpriteSheet");
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

			const int maxGridPoints = 1600;
			Vector2 gridSpacing = new Vector2((float)Math.Sqrt(UnrealMonoGame.currentResolution.X * UnrealMonoGame.currentResolution.Y / maxGridPoints));
			gridNew = new GridNew(new Rectangle(0, 0, levelDimensionX, levelDimensionY), gridSpacing);
            
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
			//SoundsManager.PlaySoundEffect("Spawn");
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
					AddEnemy(RandomLocation(), MathExtention.Rand.Next(1, 7), CurrentDifficulty);
					EnemySpawTimer = EnemySpawTimerOriginal;
					EnemiesSpawned++;
				}
				else if (EnemiesSpawned < EnemiesMax)
				{
					AddEnemy(RandomLocation(), MathExtention.Rand.Next(1, 7), CurrentDifficulty);
					EnemySpawTimer = EnemySpawTimerOriginal;
					EnemiesSpawned++;
				}
			}
			else
			{
				EnemySpawTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
			}

#if DEVELOPMENT
			//grid.Update(gameTime);
			gridNew.Update();
#endif
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
			{
				StateManager.Pause();
			}
		}

		public override void BeginDraw()
		{
			UnrealMonoGame.Get.GraphicsDevice.SetRenderTarget(UnrealMonoGame.renderTarget1);
			UnrealMonoGame.Get.GraphicsDevice.Clear(Color.TransparentBlack);

			UnrealMonoGame.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

#if DEVELOPMENT
			//grid.Draw();
			gridNew.Draw(UnrealMonoGame.spriteBatch);
#else
			TileMap.Draw();
#endif
		}

		public override void EndDraw()
		{
			ParticlesManager.Draw();

			int points = 32;
			VertexPositionColor[] primitiveList = new VertexPositionColor[points];

			for (int x = 0; x < points / 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					primitiveList[(x * 2) + y] = new VertexPositionColor(
						new Vector3(x * 100, y * 100, 0), Color.White);
				}
			}
			int width = 12;
			int height = 12;
			short[] triangleStripIndices = new short[(width - 1) * (height - 1) * 6];

			triangleStripIndices = new short[points];

			// Populate the array with references to indices in the vertex buffer.
			for (int i = 0; i < points; i++)
			{
				triangleStripIndices[i] = (short)i;
			}

			triangleStripIndices = new short[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

			for (int i = 0; i < primitiveList.Length; i++)
				primitiveList[i].Color = Color.Red;

			//UnrealMonoGame.Get.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
			//	PrimitiveType.TriangleStrip,
			//	primitiveList,
			//	0,  // vertex buffer offset to add to each element of the index buffer
			//	8,  // number of vertices to draw
			//	triangleStripIndices,
			//	0,  // first index element to read
			//	6   // number of primitives to draw
			//);

			UnrealMonoGame.spriteBatch.End();

			UnrealMonoGame.Bloom.Draw(UnrealMonoGame.renderTarget1, UnrealMonoGame.renderTarget2);

			UnrealMonoGame.Get.GraphicsDevice.SetRenderTarget(null);

			//Draw bloomed layer over top:
			UnrealMonoGame.spriteBatch.Begin(0, BlendState.AlphaBlend);
			UnrealMonoGame.spriteBatch.Draw(UnrealMonoGame.renderTarget2, new Rectangle(0, 0, UnrealMonoGame.currentResolution.X, UnrealMonoGame.currentResolution.Y), Color.White); // draw all glowing components

			DrawUI();
			
			base.EndDraw();
		}

		public void DrawUI()
		{
			float stringScale = 0.545F;
			string stringToDraw = "Score: " + GetScore().ToString();

			UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.defaultFont,
				stringToDraw,
				new Vector2(UnrealMonoGame.currentResolution.X * 0.86094F, UnrealMonoGame.currentResolution.Y * 0.00694F),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);

			stringToDraw = "Multiplier: " + Multiplier;
			UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.defaultFont,
				stringToDraw,
				new Vector2(UnrealMonoGame.currentResolution.X * 0.01328F, UnrealMonoGame.currentResolution.Y * 0.00694F),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);

			if (!isEndless)
			{
				Vector2 stringSize;
				stringToDraw = "Time : " + (int)TimeLeft;
				if (TimeLeft >= 100)
				{
					stringSize = UnrealMonoGame.defaultFont.MeasureString("Time : 999") * UnrealMonoGame.defaultFontScale * stringScale;
				}
				else
				{
					stringSize = UnrealMonoGame.defaultFont.MeasureString("Time : 99") * UnrealMonoGame.defaultFontScale * stringScale;
				}
				UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.defaultFont,
					stringToDraw,
					new Vector2(((UnrealMonoGame.currentResolution.X / 2.0f) - ((stringSize.X / 2F) * UnrealMonoGame.ResolutionScale)), UnrealMonoGame.currentResolution.Y * 0.00694F),
				Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}

			List<APlayerActor> player = StateManager.currentState.GetAllActorsOfType<APlayerActor>();
			if (NOfPlayers == 1 && player.Count == 1)
			{
				stringToDraw = "Lives: " + player[0].Lives;
				UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.defaultFont,
					stringToDraw,
					new Vector2(UnrealMonoGame.currentResolution.X * 0.9015625F, UnrealMonoGame.currentResolution.Y * 0.9583F),
					Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}
			else if (NOfPlayers == 2 && player.Count == 2)
			{
				stringToDraw = "Lives: " + StateManager.currentState.GetAllActorsOfType<APlayerActor>()[0].Lives + "/" + StateManager.currentState.GetAllActorsOfType<APlayerActor>()[1].Lives;
				UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.defaultFont,
					stringToDraw,
					new Vector2(UnrealMonoGame.currentResolution.X * 0.8625F, UnrealMonoGame.currentResolution.Y * 0.9583F),
					Color.WhiteSmoke, 0, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0);
			}

#if DEBUG
			UnrealMonoGame.DrawFPS();
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

	    public void TryToSpawnPowerup(Vector2 Position)
	    {
            //TO1 Refactor
	        int temp = MathExtention.Rand.Next(0, 10);
	        if (temp == 9)
	        {
	            AGunPowerUpActor newPowerup = new AGunPowerUpActor(
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
	                ComponentUpdateGroup.AfterActor, DrawGroup.Enemies1,
	                Position,
	                true,
	                1F,
	                default(Vector2),
	                -1F,
	                Color.White,
	                Color.White);

	            newPowerup.SpriteComponent.CurrentFrame = 2; //Ultra

	            newPowerup.gunSettings.projectilesColor = new Color(0, 255, 0);
                newPowerup.gunSettings.level = 3;
	            newPowerup.gunSettings.timeLength = 3.9f;
                newPowerup.gunSettings.rotationSpeed = 0.16f / (1f / 60f);
	            newPowerup.gunSettings.numberOfProjectiles = 8;
                newPowerup.gunSettings.angleBetweenProjectiles = 360 / newPowerup.gunSettings.numberOfProjectiles;
	            newPowerup.gunSettings.timeBetweenShots = 0.06f;
	            newPowerup.gunSettings.projectilesSpeed = 900f;
            }
            else if (temp == 8 || temp == 7)
	        {
	            AGunPowerUpActor newPowerup = new AGunPowerUpActor(
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
	                ComponentUpdateGroup.AfterActor, DrawGroup.Enemies1,
	                Position,
	                true,
	                1F,
	                default(Vector2),
	                -1F,
	                Color.White,
	                Color.White);
	            newPowerup.SpriteComponent.CurrentFrame = 1; //Quintuple
                
	            newPowerup.gunSettings.projectilesColor = new Color(255, 150, 0);
                newPowerup.gunSettings.level = 2;
	            newPowerup.gunSettings.timeLength = 15;
	            newPowerup.gunSettings.rotationSpeed = 0;
	            newPowerup.gunSettings.numberOfProjectiles = 5;
	            newPowerup.gunSettings.angleBetweenProjectiles = 15;
	            newPowerup.gunSettings.timeBetweenShots = 0.0825f;
                newPowerup.gunSettings.projectilesSpeed = 900f * 0.74F;
            }
            else if (temp == 6 || temp == 5)
	        {
	            AMinePowerUpActor newPowerup = new AMinePowerUpActor(
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
	                ComponentUpdateGroup.AfterActor, DrawGroup.Enemies1,
	                Position,
	                true,
	                1F,
	                default(Vector2),
	                -1F,
	                Color.White,
	                Color.White);
	            newPowerup.SpriteComponent.CurrentFrame = 3; //Mine
            }
	        else
	        {
	            AGunPowerUpActor newPowerup = new AGunPowerUpActor(
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
	                ComponentUpdateGroup.AfterActor, DrawGroup.Enemies1,
	                Position,
	                true,
	                1F,
	                default(Vector2),
	                -1F,
	                Color.White,
	                Color.White);
	            newPowerup.SpriteComponent.CurrentFrame = 0; //Triple
                
	            newPowerup.gunSettings.projectilesColor = new Color(0, 167, 255);
                newPowerup.gunSettings.level = 1;
	            newPowerup.gunSettings.timeLength = 15;
	            newPowerup.gunSettings.rotationSpeed = 0;
	            newPowerup.gunSettings.numberOfProjectiles = 3;
	            newPowerup.gunSettings.angleBetweenProjectiles = 15;
	            newPowerup.gunSettings.timeBetweenShots = 0.051f; //Was 0.035F
                newPowerup.gunSettings.projectilesSpeed = 900f * 0.82F;
            }

            List<APowerUpActor> powerUps = StateManager.currentState.GetAllActorsOfType<APowerUpActor>();
	        while (powerUps.Count >= maxActivePowerups)
	        {
	            powerUps[0].Destroy();
	            powerUps.RemoveAt(0);
	        }

			SoundsManager.PlaySoundEffect("PowerUp");
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
				List<APlayerActor> players = StateManager.currentState.GetAllActorsOfType<APlayerActor>();

				if (players.Count > 0) //TO1 improve
				{
				    int RandSide = MathExtention.Rand.Next(0, 4);
				    float RandPosition = MathExtention.Rand.Next(-50, 51) / 100.0f; //Rand between -0.5 and 0.5

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

#if DEVELOPMENT
			ASkeletalEnemyActor newEnemy = new ASkeletalEnemyActor(squareLocation, speed);
#else
			if (MathExtention.Rand.Next(0, 5) == 4)
			{
				AEnemyWandererActor newEnemy = new AEnemyWandererActor(squareLocation, type, speed);
				newEnemy.Lives += 3 * (currentDifficulty - 1);
			}
			else
			{
				AEnemySeekerActor newEnemy = new AEnemySeekerActor(squareLocation, type, speed);
				newEnemy.Lives += 3 * (currentDifficulty - 1);
			}
#endif
		}
	}
}