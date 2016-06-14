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
        public static List<Player> Players = new List<Player>();
        public static List<Enemy> Enemies = new List<Enemy>(); //for both EnemySeeker and EnemyWanderer
        private static Random rand = new Random();

        public static void Initialize(int NOfPlayers)
        {
            Players.Clear();
            Enemies.Clear();

            if (NOfPlayers == 1)
            {
                Player P1 = new Player(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
                Players.Add(P1);
            }
            else if (NOfPlayers == 2)
            {
                Player P1 = new Player(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
                Players.Add(P1);
                //Adds a second player
                Player P2 = new Player(new Vector2(GameManager.LevelDimensionX / 2.0f, GameManager.LevelDimensionY / 2.0f));
                Players.Add(P2);
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

                if (RandSide == 0) //Spawn from left of player
                {
                    newX = Players[0].ObjectBase.WorldLocation.X - (Game1.CurResolutionX / 2) - 45;
                    newY = Players[0].ObjectBase.WorldLocation.Y + (Game1.CurResolutionY * RandPosition);
                }
                else if (RandSide == 1) //Spawn from right of player
                {
                    newX = Players[0].ObjectBase.WorldLocation.X + (Game1.CurResolutionX / 2) + 45;
                    newY = Players[0].ObjectBase.WorldLocation.Y + (Game1.CurResolutionY * RandPosition);
                }
                else if (RandSide == 2) //Spawn from top of player
                {
                    newX = Players[0].ObjectBase.WorldLocation.X + (Game1.CurResolutionX * RandPosition);
                    newY = Players[0].ObjectBase.WorldLocation.Y - (Game1.CurResolutionY / 2) - 30;
                }
                else //if (RandSide == 3) //Spawn from bottom of player
                {
                    newX = Players[0].ObjectBase.WorldLocation.X + (Game1.CurResolutionX * RandPosition);
                    newY = Players[0].ObjectBase.WorldLocation.Y + (Game1.CurResolutionY / 2) + 30;
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
                EnemyWanderer newEnemy = new EnemyWanderer(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
                Enemies.Add(newEnemy);
            }
            else
            {
                EnemySeeker newEnemy = new EnemySeeker(squareLocation, type, speed);
                newEnemy.Lives += 3 * (currentDifficulty - 1);
                Enemies.Add(newEnemy);
            }

        }

        public static void Update(GameTime gameTime)
        {
            for (int x = Enemies.Count - 1; x >= 0; x--)
            {
                if (Enemies[x].Destroyed)
                    Enemies.RemoveAt(x);
                else
                    Enemies[x].Update(gameTime);
            }
            for (int x = Players.Count - 1; x >= 0; x--)
            {
                Players[x].Update(gameTime);
            }
        }

        public static void Draw()
        {
            foreach (Character obj in Enemies)
            {
                obj.Draw();
            }
            foreach (Player play in Players)
            {
                play.Draw();
            }
        }
    }
}
