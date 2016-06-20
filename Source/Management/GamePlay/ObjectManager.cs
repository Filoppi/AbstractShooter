using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    static class ObjectManager
    {
        //public static List<APlayer> Players = new List<APlayer>();
        //public static List<AEnemy> Enemies = new List<AEnemy>(); //for both AEnemySeeker and AEnemyWanderer
        private static Random rand = new Random();

        public static void Initialize(int NOfPlayers)
        {
            if (NOfPlayers == 1)
            {
                APlayer P1 = new APlayer(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
            }
            else if (NOfPlayers == 2)
            {
                APlayer P1 = new APlayer(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
                //Adds a second player
                APlayer P2 = new APlayer(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
            }
        }

        //Generates a random location for the enemy to spawn where you can't see, within levels boundaries
        public static Vector2 RandomLocation()
        {
            float newX = 0;
            float newY = 0;
            //Avoids infinite attempts in case the map is too small.
            int counter = 0;

            while (counter < 40 && (newX < 32 || newX > GameManager.LevelDimensionX - 32 || newY < 32 || newY > GameManager.LevelDimensionY - 32))
            {
                int RandSide = rand.Next(0, 4);
                float RandPosition = (float)rand.Next(-50, 51) / 100.0f;

                List<APlayer> players = StateManager.currentState.GetAllActorsOfClass<APlayer>();

                if (RandSide == 0) //Spawn from left of player
                {
                    newX = players[0].WorldLocation.X - (Game1.curResolutionX / 2) - 45;
                    newY = players[0].WorldLocation.Y + (Game1.curResolutionY * RandPosition);
                }
                else if (RandSide == 1) //Spawn from right of player
                {
                    newX = players[0].WorldLocation.X + (Game1.curResolutionX / 2) + 45;
                    newY = players[0].WorldLocation.Y + (Game1.curResolutionY * RandPosition);
                }
                else if (RandSide == 2) //Spawn from top of player
                {
                    newX = players[0].WorldLocation.X + (Game1.curResolutionX * RandPosition);
                    newY = players[0].WorldLocation.Y - (Game1.curResolutionY / 2) - 30;
                }
                else //if (RandSide == 3) //Spawn from bottom of player
                {
                    newX = players[0].WorldLocation.X + (Game1.curResolutionX * RandPosition);
                    newY = players[0].WorldLocation.Y + (Game1.curResolutionY / 2) + 30;
                }
                counter++;
            }

            return new Vector2(newX, newY);
        }

        public static void AddEnemy(Vector2 squareLocation, int type, int currentDifficulty)
        {
            int startX = (int)squareLocation.X;
            int startY = (int)squareLocation.Y;
            Rectangle squareRect = TileMap.SquareWorldRectangle(startX, startY).GetRectangle();

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

            if (rand.Next(0, 5) == 4)
            {
                AEnemyWanderer newEnemy = new AEnemyWanderer(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
            }
            else
            {
                AEnemySeeker newEnemy = new AEnemySeeker(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
            }

        }
    }
}
