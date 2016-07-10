using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    static class TileMap
    {
        private static int TileWidth = 32;
        private static int TileHeight = 32;
        private static int MapWidth = 42;
        private static int MapHeight = 32;

        private static List<Rectangle> tiles = new List<Rectangle>();

        private static int[,] mapSquares = new int[MapWidth, MapHeight];

        private static Random rand = new Random();
        
        static public void Initialize(int RealMapWidth, int RealMapHeight)
        {
            MapWidth = RealMapWidth / TileWidth;
            MapHeight = RealMapHeight / TileHeight;
            mapSquares = new int[MapWidth, MapHeight];

            tiles.Clear();
            tiles.Add(new Rectangle(0, 0, TileWidth, TileHeight));
            tiles.Add(new Rectangle(0, 32, TileWidth, TileHeight));

            GenerateMap();
        }

        #region Information about Map Squares
        static public int GetSquareByPixelX(int pixelX)
        {
            return (int)(pixelX / Math.Round((float)TileWidth));
        }
        static public int GetSquareByPixelY(int pixelY)
        {
            return (int)(pixelY / Math.Round((float)TileHeight));
        }
        static public Vector2 GetSquareAtPixel(Vector2 pixelLocation)
        {
            return new Vector2(
                GetSquareByPixelX((int)pixelLocation.X),
                GetSquareByPixelY((int)pixelLocation.Y));
        }
        //static public Vector2 GetSquareCenter(int squareX, int squareY)
        //{
        //    return new Vector2(
        //        (squareX * TileWidth) + (TileWidth / 2),
        //        (squareY * TileHeight) + (TileHeight / 2));
        //}
        //static public Vector2 GetSquareCenter(Vector2 square)
        //{
        //    return GetSquareCenter(
        //        (int)square.X,
        //        (int)square.Y);
        //}
        static public RectangleF SquareWorldRectangle(int x, int y)
        {
            return new RectangleF(
                (float)Math.Round((float)x * (float)TileWidth),
                (float)Math.Round((float)y * (float)TileHeight),
                TileWidth,
                TileHeight);
        }
        //static public Rectangle SquareWorldRectangle(Vector2 square)
        //{
        //    return SquareWorldRectangle(
        //        (int)square.X,
        //        (int)square.Y);
        //}
        static public RectangleF SquareScreenRectangle(int x, int y)
        {
            return Camera.WorldToScreenSpace(SquareWorldRectangle(x, y));
        }
        //static public Rectangle SquareSreenRectangle(Vector2 square)
        //{
        //    return SquareScreenRectangle((int)square.X, (int)square.Y);
        //}
        #endregion

        #region Information about Map Tiles
        static public int GetTileAtSquare(int tileX, int tileY)
        {
            if ((tileX >= 0) && (tileX < MapWidth) &&
                (tileY >= 0) && (tileY < MapHeight))
            {
                return mapSquares[tileX, tileY];
            }
            else
            {
                return -1;
            }
        }
        //static public void SetTileAtSquare(int tileX, int tileY, int tile)
        //{
        //    if ((tileX >= 0) && (tileX < MapWidth) &&
        //        (tileY >= 0) && (tileY < MapHeight))
        //    {
        //        mapSquares[tileX, tileY] = tile;
        //    }
        //}
        static public int GetTileAtPixel(int pixelX, int pixelY)
        {
            return GetTileAtSquare(
                GetSquareByPixelX(pixelX),
                GetSquareByPixelY(pixelY));
        }
        //static public int GetTileAtPixel(Vector2 pixelLocation)
        //{
        //    return GetTileAtPixel(
        //        (int)pixelLocation.X,
        //        (int)pixelLocation.Y);
        //}
        static public bool IsWallTile(int tileX, int tileY)
        {
            int tileIndex = GetTileAtSquare(tileX, tileY);

            if (tileIndex == -1)
            {
                return false;
            }

            return tileIndex >= 1;
        }
        static public bool IsWallTile(Vector2 square)
        {
            return IsWallTile((int)square.X, (int)square.Y);
        }
        static public bool IsWallTileByPixel(Vector2 pixelLocation)
        {
            return IsWallTile(
                GetSquareByPixelX((int)pixelLocation.X),
                GetSquareByPixelY((int)pixelLocation.Y));
        }
        #endregion
        
        static public void GenerateMap()
        {
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    mapSquares[x, y] = 0;

                    if ((x == 0) || (y == 0) || (x == MapWidth - 1) || (y == MapHeight - 1))
                    {
                        mapSquares[x, y] = 1;
                    }
                }
            }
        }

        static public void Draw()
        {
            int startX = GetSquareByPixelX((int)Camera.TopLeft.X);
            int endX = GetSquareByPixelX((int)Camera.BottomRight.X);

            int startY = GetSquareByPixelY((int)Camera.TopLeft.Y);
            int endY = GetSquareByPixelY((int)Camera.BottomRight.Y);

            //int drawn = 0;
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < MapWidth) && (y < MapHeight))
                    {
                        Game1.spriteBatch.Draw(
                            StateManager.currentState.spriteSheet,
                            SquareScreenRectangle(x, y).GetRectangle(),
                            tiles[GetTileAtSquare(x, y)],
                            Color.White);

                        //drawn++;
                        //Game1.spriteBatch.Draw(
                        //    StateManager.currentState.spriteSheet,
                        //    new Vector2(SquareScreenRectangle(x, y).X * Game1.ResolutionScale, SquareScreenRectangle(x, y).Y * Camera.DrawScale),
                        //    tiles[GetTileAtSquare(x, y)],
                        //    Color.White, 0, Vector2.Zero, Camera.DrawScale, SpriteEffects.None, 0);
                    }
                }
            }
            //Console.WriteLine(drawn.ToString());
        }
    }
}
