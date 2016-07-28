﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbstractShooter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class Grid
    {
        public int NodeSize = 32; //Pixels? //TO private
        private const int NodeSubNumber = 0; //Small lines in each tile
        public int gridWidth = 42; //TO private
        public int gridHeight = 32; //TO private
        public float powExp = 0.5F; //private //Temp
        public bool speed = false; //private //temp
        private Vector2 worldLocation = Vector2.Zero; //Distance between centre of world and upper left edge of this grid (the first node world location)
        private Color bordersColor = Color.MediumPurple;
        private Color internalColor = Color.PaleTurquoise;
        private Color subInternalXColor = Color.PaleTurquoise;
        private Color subInternalYColor = Color.PaleTurquoise;
        private float bordersThickness = 4.5F;
        private float internalColorThickness = 2F;
        private float subInternalThickness = 0.75F;

        public float defaultGravity = 0; //private //0.044437F //0.000137F //0.0037F
        public float gravity = 300F; //private
        public bool hasMemory = false; //private //if true the Grid won't reset every frame to its default state, but will instead be have a reset coefficient which will slowly restore it (allows rubber band effect)
        //private Color[,] internalColorArray;
        private float minNodeOffset = 0.65F; //Nodes that have an offset from their original location lessthan this value, will be snapped to their original value
        private float maxInfluenceDistance = 370F; //Not in the same unit

        private Vector2[,] originalNodesLocation;
        private Vector2[,] nodesLocation;
        
        public void Initialize(int realMapWidth, int realMapHeight)
        {
            gridWidth = (int)Math.Round((double)realMapWidth / NodeSize);
            gridHeight = (int)Math.Round((double)realMapHeight / NodeSize);
            originalNodesLocation = new Vector2[gridWidth, gridHeight];
            //internalColorArray = new Color[gridWidth, gridHeight];
            Set();
            nodesLocation = new Vector2[gridWidth, gridHeight];
            Reset();
        }
        private Vector2 ClampToWorld(Vector2 location)
        {
            if (location.X < NodeSize)
            {
                location.X = NodeSize;
            }
            else if (location.X > (gridWidth - 2) * NodeSize)
            {
                location.X = (gridWidth - 2) * NodeSize;
            }
            if (location.Y < NodeSize)
            {
                location.Y = NodeSize;
            }
            else if (location.Y > (gridHeight - 2) * NodeSize)
            {
                location.Y = (gridHeight - 2) * NodeSize;
            }

            return location;
        }

        public void Update(GameTime gameTime)
        {
            //Temp code to update colors
            //Vector4 asd = internalColor.ToVector4();
            //float mina = 60 / 255F;
            //float max = 197 / 255F;
            //internalColor = new Color(MathExtention.PositiveCycle(asd.X + (0.005F * (float)rand.Next(3, 70) / 100f), rand.Next(40, 90) / 255f, rand.Next(140, 230) / 255f),
            //    MathExtention.PositiveCycle(asd.X + (0.005F * (float)MathExtention.Rand.Next(3, 70) / 100f), MathExtention.Rand.Next(40, 90) / 255f, MathExtention.Rand.Next(140, 230) / 255f),
            //    MathExtention.PositiveCycle(asd.X + (0.005F * (float)MathExtention.Rand.Next(3, 70) / 100f), MathExtention.Rand.Next(40, 90) / 255f, MathExtention.Rand.Next(140, 230) / 255f),
            //    asd.W);

            if (!hasMemory)
            {
                Reset();
            }

            foreach (AActor actor in StateManager.currentState.GetAllActors())
            {
                float actorMass = actor.GetMass();
                if (actorMass != 0F)
                {
                    if (speed)
                    {
                        actorMass *= actor.RootComponent.localVelocity.Length();
                    }
                    Influence(actor.GetMassCenter(), gravity * actorMass * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale);
                }
            }
            //OR:
            //foreach (CSpriteComponent spriteComponent in StateManager.currentState.GetAllSceneComponentsOfType<CSpriteComponent>())
            //{
            //    Influence(spriteComponent.WorldLocation, gravity * spriteComponent.localVelocity.Length() * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale);
            //}

            if (hasMemory)
            {
                for (int i = 0; i < gridWidth; i++)
                {
                    for (int k = 0; k < gridHeight; k++)
                    {
                        Vector2 originalNodeLocation = originalNodesLocation[i, k];
                        float distSquared = Vector2.DistanceSquared(nodesLocation[i, k], originalNodeLocation);
                        Vector2 dir = nodesLocation[i, k] - originalNodeLocation;
                        if (distSquared > minNodeOffset * minNodeOffset)
                        {
                            if (defaultGravity != 0)
                            {
                                dir.Normalize();
                                nodesLocation[i, k] += (-defaultGravity * dir) * distSquared;
                                if (Vector2.Dot(nodesLocation[i, k] - originalNodeLocation, dir) <= 0)
                                {
                                    nodesLocation[i, k] = originalNodeLocation;
                                }
                            }
                        }
                        else
                        {
                            nodesLocation[i, k] = originalNodeLocation;
                        }
                    }
                }
            }
        }
        public void Set()
        {
            for (int i = 0; i < gridWidth; ++i)
            {
                for (int k = 0; k < gridHeight; ++k)
                {
                    originalNodesLocation[i, k] = new Vector2(NodeSize * (float)i, NodeSize * (float)k) + worldLocation;
                    //internalColorArray[i, k] = MathExtention.GetRandomColor();
                }
            }
        }
        public void Reset()
        {
            for (int i = 0; i < gridWidth; ++i)
            {
                for (int k = 0; k < gridHeight; ++k)
                {
                    nodesLocation[i, k] = originalNodesLocation[i, k];
                }
            }
        }
        public void Influence(Vector2 forceLocation, float force)
        {
            int startX, endX, startY, endY;
            if (!hasMemory)
            {
                startX = 2;
                endX = gridWidth - 3;

                startY = 2;
                endY = gridHeight - 3;
            }
            else
            {
                startX = Math.Max(GetSquareByPixel((int)Camera.TopLeft.X), 2);
                endX = Math.Min(GetSquareByPixel((int)Camera.BottomRight.X), gridWidth - 3);

                startY = Math.Max(GetSquareByPixel((int)Camera.TopLeft.Y), 2);
                endY = Math.Min(GetSquareByPixel((int)Camera.BottomRight.Y), gridHeight - 3);
            }
            
            for (int i = startX; i <= endX; ++i)
            {
                for (int k = startY; k <= endY; ++k)
                {
                    double dist;
                    if (force < 0) //To refactor...
                    {
                        dist = Math.Pow(Vector2.DistanceSquared(new Vector2(NodeSize * (float)i, NodeSize * (float)k), forceLocation), powExp);
                    }
                    else
                    {
                        dist = Math.Pow(Vector2.DistanceSquared(nodesLocation[i, k], forceLocation), powExp);
                    }
                    if (dist < maxInfluenceDistance * maxInfluenceDistance)
                    {
                        Vector2 dir = nodesLocation[i, k] - forceLocation;
                        dir.Normalize();
                        nodesLocation[i, k] = ClampToWorld(nodesLocation[i, k] + ((force * dir) / Math.Max((float)dist, 1F)));
                        if (force < 0 && Vector2.Dot(nodesLocation[i, k] - forceLocation, dir) <= 0)
                        {
                            nodesLocation[i, k] = forceLocation;
                        }
                    }
                }
            }
        }

        public int GetSquareByPixel(int pixel)
        {
            return pixel / NodeSize;
        }
        
        public int GetSquareByPixelX(int pixelX)
        {
            return (int)(pixelX / Math.Round((float)NodeSize));
        }
        public int GetSquareByPixelY(int pixelY)
        {
            return (int)(pixelY / Math.Round((float)NodeSize));
        }
        public Vector2 GetSquareAtPixel(Vector2 pixelLocation)
        {
            return new Vector2(
                GetSquareByPixelX((int)pixelLocation.X),
                GetSquareByPixelY((int)pixelLocation.Y));
        }
        public RectangleF SquareWorldRectangle(int x, int y)
        {
            return new RectangleF(
                (float)Math.Round((float)x * (float)NodeSize),
                (float)Math.Round((float)y * (float)NodeSize),
                NodeSize,
                NodeSize);
        }
        public RectangleF SquareScreenRectangle(int x, int y)
        {
            return Camera.WorldToScreenSpace(SquareWorldRectangle(x, y));
        }
        public int GetTileAtSquare(int tileX, int tileY)
        {
            if (tileX < gridWidth - 1)
            {
                if (tileX == 0 || tileY == 0)
                {
                    return 1;
                }
                else if (tileX < gridWidth - 2 && tileY < gridHeight - 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            if (tileY < gridHeight - 1)
            {
                if (tileX == 0 || tileY == 0 || tileX == 1)
                {
                    return 1;
                }
                else if (tileY < gridHeight - 2 && tileX < gridWidth - 2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return -1;
        }
        public int GetTileAtPixel(int pixelX, int pixelY)
        {
            return GetTileAtSquare(
                GetSquareByPixelX(pixelX),
                GetSquareByPixelY(pixelY));
        }
        public bool IsWallTile(int tileX, int tileY)
        {
            return GetTileAtSquare(tileX, tileY) >= 1;
        }
        public bool IsWallTile(Vector2 square)
        {
            return IsWallTile((int)square.X, (int)square.Y);
        }
        public bool IsWallTileByPixel(Vector2 pixelLocation)
        {
            return IsWallTile(
                GetSquareByPixelX((int)pixelLocation.X),
                GetSquareByPixelY((int)pixelLocation.Y));
        }

        public void Draw()
        {
            int startX = Math.Max(GetSquareByPixel((int)Camera.TopLeft.X), 0);
            int endX = Math.Min(GetSquareByPixel((int)Camera.BottomRight.X), gridWidth - 1);

            int startY = Math.Max(GetSquareByPixel((int)Camera.TopLeft.Y), 0);
            int endY = Math.Min(GetSquareByPixel((int)Camera.BottomRight.Y), gridHeight - 1);

            //Temp to draw everything
            startX = 0;
            startY = 0;
            endX = gridWidth - 1;
            endY = gridHeight - 1;

            //To reduce DrawLine number by calling just one for straigth lines
            for (int i = startX; i <= endX; ++i)
            {
                for (int k = startY; k <= endY; ++k)
                {
                    if (i < gridWidth - 1)
                    {
                        if (i == 0 || k == 0 || k == 1)
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i + 1, k]), bordersColor, bordersThickness, DrawGroup.Background3);

                            if (k == 1 && i != 0 && (i < gridWidth - 2 && k < gridHeight - 2))
                            {
                                for (int n = 1; n <= NodeSubNumber; ++n)
                                {
                                    Vector2 delta = new Vector2(0, n * NodeSize / (NodeSubNumber + 1));
                                    Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k] + delta), Camera.WorldToScreenSpace(nodesLocation[i + 1, k] + delta), subInternalXColor, subInternalThickness, DrawGroup.Background1);
                                }
                            }
                        }
                        else if (i < gridWidth - 2 && k < gridHeight - 2)
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i + 1, k]), internalColor, internalColorThickness, DrawGroup.Background2);

                            for (int n = 1; n <= NodeSubNumber; ++n)
                            {
                                Vector2 delta = new Vector2(0, n * NodeSize / (NodeSubNumber + 1));
                                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k] + delta), Camera.WorldToScreenSpace(nodesLocation[i + 1, k] + delta), subInternalXColor, subInternalThickness, DrawGroup.Background1);
                            }
                        }
                        else
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i + 1, k]), bordersColor, bordersThickness, DrawGroup.Background3);
                        }
                    }
                    if (k < gridHeight - 1)
                    {
                        if (i == 0 || k == 0 || i == 1)
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i, k + 1]), bordersColor, bordersThickness, DrawGroup.Background3);

                            if (i == 1 && k != 0 && (k < gridHeight - 2 && i < gridWidth - 2))
                            {
                                for (int n = 1; n <= NodeSubNumber; ++n)
                                {
                                    Vector2 delta = new Vector2(n * NodeSize / (NodeSubNumber + 1), 0);
                                    Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k] + delta), Camera.WorldToScreenSpace(nodesLocation[i, k + 1] + delta), subInternalYColor, subInternalThickness, DrawGroup.Background1);
                                }
                            }
                        }
                        else if (k < gridHeight - 2 && i < gridWidth - 2)
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i, k + 1]), internalColor, internalColorThickness, DrawGroup.Background2);
                            
                            for (int n = 1; n <= NodeSubNumber; ++n)
                            {
                                Vector2 delta = new Vector2(n * NodeSize / (NodeSubNumber + 1), 0);
                                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k] + delta), Camera.WorldToScreenSpace(nodesLocation[i, k + 1] + delta), subInternalYColor, subInternalThickness, DrawGroup.Background1);
                            }
                        }
                        else
                        {
                            Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(nodesLocation[i, k]), Camera.WorldToScreenSpace(nodesLocation[i, k + 1]), bordersColor, bordersThickness, DrawGroup.Background3);
                        }
                    }
                }
            }
        }
    }
}
