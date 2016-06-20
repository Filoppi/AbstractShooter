﻿namespace AbstractShooter.States
{
    public class Level1 : Level
    {
        public override void Initialize()
        {
            Difficulty = 1;
            NOfPlayers = 1;
            EnemySpawTimer = 0.55f;
            LevelDimensionX = 2330;
            LevelDimensionY = 1430;
            TimeLeft = 61;
            EnemiesLeft = 80;
            isEndless = false;
            levelIndex = 1;
            base.Initialize();
        }
    }
    public class Level2 : Level
    {
        public override void Initialize()
        {
            Difficulty = 3;
            NOfPlayers = 1;
            EnemySpawTimer = 0.6166F;
            LevelDimensionX = 2330;
            LevelDimensionY = 1430;
            TimeLeft = 121;
            EnemiesLeft = 100;
            isEndless = false;
            levelIndex = 2;
            base.Initialize();
        }
    }
    public class Level3 : Level
    {
        public override void Initialize()
        {
            Difficulty = 5;
            NOfPlayers = 1;
            EnemySpawTimer = 0.6333F;
            LevelDimensionX = 2000;
            LevelDimensionY = 1300;
            TimeLeft = 181;
            EnemiesLeft = 130;
            isEndless = false;
            levelIndex = 3;
            base.Initialize();
        }
    }
    public class LevelEndless : Level
    {
        public override void Initialize()
        {
            Difficulty = 0; //3
            NOfPlayers = 1;
            EnemySpawTimer = 0.15F; //0.6166F
            LevelDimensionX = 2000; //2330
            LevelDimensionY = 1300; //1430
            TimeLeft = 50000;
            EnemiesLeft = 50000;
            isEndless = true;
            base.Initialize();
        }
    }
}