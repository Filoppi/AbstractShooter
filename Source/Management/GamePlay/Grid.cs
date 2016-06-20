using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class Grid
    {
        private Int32 NodeSize = 32; //Pixels?
        private const Int32 NodeSubNumber = 0; //Small lines in each tile
        private Int32 GridWidth = 42;
        private Int32 GridHeight = 32;
        //private Vector2 offset; //Distance between centre of world and upper left edge of this grid (the first node world location)

        private float defaultGravity = 0.044437F; //0.000137F //0.0037F
        private bool isElastic = false;
        private Color bordersColor = Color.MediumPurple;
        private Color internalColor = Color.PaleTurquoise;
        private Color[,] internalColorArray;
        private Color subInternalXColor = Color.PaleGreen;
        private Color subInternalYColor = Color.PaleGreen;
        private float bordersThickness = 7F;
        private float internalColorThickness = 3F;
        private float subInternalThickness = 1F;

        private Vector2[,] originalNodesLocation;
        private Vector2[,] nodesLocation;

        public void Initialize(Int32 RealMapWidth, Int32 RealMapHeight)
        {
            GridWidth = (Int32)Math.Round((Double)RealMapWidth / NodeSize);
            GridHeight = (Int32)Math.Round((Double)RealMapHeight / NodeSize);
            originalNodesLocation = new Vector2[GridWidth, GridHeight];
            internalColorArray = new Color[GridWidth, GridHeight];
            Set();
            nodesLocation = new Vector2[GridWidth, GridHeight];
            Reset();
        }

        private Vector2 ClampToWorld(Vector2 location)
        {
            if (location.X < NodeSize)
            {
                location.X = NodeSize;
            }
            else if (location.X > (GridWidth-2) * NodeSize)
            {
                location.X = (GridWidth - 2) * NodeSize;
            }
            if (location.Y < NodeSize)
            {
                location.Y = NodeSize;
            }
            else if (location.Y > (GridHeight - 2) * NodeSize)
            {
                location.Y = (GridHeight - 2) * NodeSize;
            }

            return location;
        }

        public void Update()
        {
            if (defaultGravity != 0)
            {
                for (Int32 i = 0; i < GridWidth; i++)
                {
                    for (Int32 k = 0; k < GridHeight; k++)
                    {
                        Vector2 location = new Vector2(NodeSize * (float)i, NodeSize * (float)k);
                        float dist = Vector2.DistanceSquared(nodesLocation[i, k], location);
                        Vector2 dir = nodesLocation[i, k] - location;
                        float min = 0.65F;
                        if (dist > min * min)
                        {
                            dir.Normalize();
                            nodesLocation[i, k] += (-defaultGravity * dir) * dist;
                            if (Vector2.Dot(nodesLocation[i, k] - location, dir) <= 0)
                            {
                                nodesLocation[i, k] = location;
                            }
                        }
                        else
                        {
                            nodesLocation[i, k] = location;
                        }
                    }
                }
            }
            else if (isElastic)
            {
                Reset();
            }
        }
        public void Set()
        {
            for (Int32 i = 0; i < GridWidth; ++i)
            {
                for (Int32 k = 0; k < GridHeight; ++k)
                {
                    originalNodesLocation[i, k] = new Vector2(NodeSize * (float)i, NodeSize * (float)k);
                    internalColorArray[i, k] = MathExtention.GetRandomColor();
                }
            }
        }
        public void Reset()
        {
            for (Int32 i = 0; i < GridWidth; ++i)
            {
                for (Int32 k = 0; k < GridHeight; ++k)
                {
                    nodesLocation[i, k] = originalNodesLocation[i, k];
                }
            }
        }
        public void Influence(Vector2 location, float gravity)
        {
            Int32 startX, endX, startY, endY;
            if (isElastic)
            {
                startX = 2;
                endX = GridWidth - 3;

                startY = 2;
                endY = GridHeight - 3;
            }
            else
            {
                startX = Math.Max(GetSquareByPixel((int)Camera.Position.X), 2);
                endX = Math.Min(GetSquareByPixel((int)Camera.Position.X + Camera.ViewPortWidth), GridWidth - 3);

                startY = Math.Max(GetSquareByPixel((int)Camera.Position.Y), 2);
                endY = Math.Min(GetSquareByPixel((int)Camera.Position.Y + Camera.ViewPortHeight), GridHeight - 3);
            }
            
            for (Int32 i = startX; i <= endX; ++i)
            {
                for (Int32 k = startY; k <= endY; ++k)
                {
                    double dist;
                    if (gravity < 0) //To refactor...
                    {
                        dist = Math.Pow(Vector2.DistanceSquared(new Vector2(NodeSize * (float)i, NodeSize * (float)k), location), 0.5);
                    }
                    else
                    {
                        dist = Math.Pow(Vector2.DistanceSquared(nodesLocation[i, k], location), 0.5);
                    }
                    double min = 370F;
                    if (dist < min * min)
                    {
                        Vector2 dir = nodesLocation[i, k] - location;
                        dir.Normalize();
                        nodesLocation[i, k] = ClampToWorld(nodesLocation[i, k] + ((gravity * dir) / Math.Max((float)dist, 24)));
                        if (gravity < 0 && Vector2.Dot(nodesLocation[i, k] - location, dir) <= 0)
                        {
                            nodesLocation[i, k] = location;
                        }
                    }
                }
            }
        }

        public Int32 GetSquareByPixel(Int32 pixel)
        {
            return pixel / NodeSize;
        }

        public void Draw()
        {
            Int32 startX = Math.Max(GetSquareByPixel((Int32)Camera.Position.X), 0);
            Int32 endX = Math.Min(GetSquareByPixel((Int32)Camera.Position.X + Camera.ViewPortWidth), GridWidth - 1);

            Int32 startY = Math.Max(GetSquareByPixel((Int32)Camera.Position.Y), 0);
            Int32 endY = Math.Min(GetSquareByPixel((Int32)Camera.Position.Y + Camera.ViewPortHeight), GridHeight - 1);

            //To reduce DrawLine number by calling just one for straigth lines
            for (Int32 i = startX; i <= endX; ++i)
            {
                for (Int32 k = startY; k <= endY; ++k)
                {
                    if (i < GridWidth - 1)
                    {
                        if (i == 0 || k == 0 || k == 1)
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i + 1, k]), bordersColor, bordersThickness, (float)DrawGroup.Background);
                        }
                        else if (i < GridWidth - 2 && k < GridHeight - 2)
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i + 1, k]), internalColorArray[i, k], internalColorThickness, (float)DrawGroup.Background);

                            for (Int32 n = 1; n <= NodeSubNumber; ++n)
                            {
                                Vector2 delta = new Vector2(0, n * NodeSize / (NodeSubNumber + 1));
                                Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k] + delta), Camera.Transform(nodesLocation[i + 1, k] + delta), subInternalXColor, subInternalThickness, (float)DrawGroup.Background);
                            }
                        }
                        else
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i + 1, k]), bordersColor, bordersThickness, (float)DrawGroup.Background);
                        }
                    }
                    if (k < GridHeight - 1)
                    {
                        if (i == 0 || k == 0 || i == 1)
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i, k + 1]), bordersColor, bordersThickness, (float)DrawGroup.Background);
                        }
                        else if (k < GridHeight - 2 && i < GridWidth - 2)
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i, k + 1]), internalColorArray[i, k], internalColorThickness, (float)DrawGroup.Background);
                            
                            for (Int32 n = 1; n <= NodeSubNumber; ++n)
                            {
                                Vector2 delta = new Vector2(n * NodeSize / (NodeSubNumber + 1), 0);
                                Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k] + delta), Camera.Transform(nodesLocation[i, k + 1] + delta), subInternalYColor, subInternalThickness, (float)DrawGroup.Background);
                            }
                        }
                        else
                        {
                            Game1.spriteBatch.DrawLine(Camera.Transform(nodesLocation[i, k]), Camera.Transform(nodesLocation[i, k + 1]), bordersColor, bordersThickness, (float)DrawGroup.Background);
                        }
                    }
                }
            }
        }
    }
}
