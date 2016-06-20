using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public static class Camera
    {
        private static Vector2 position = Vector2.Zero;
        //private static Vector2 positionD = Vector2.Zero;
        private static Vector2 viewPortSize = Vector2.Zero;
        private static Rectangle worldRectangle = new Rectangle(-3840, -3840, 3840, 3840);

        public static Vector2 Position
        {
            get { return position; }
            set
            {
                position = new Vector2(
                    MathHelper.Clamp(value.X,
                        worldRectangle.X,
                        worldRectangle.Width - ViewPortWidth),
                    MathHelper.Clamp(value.Y,
                        worldRectangle.Y,
                        worldRectangle.Height - ViewPortHeight));
            }
        }
        public static Vector2 DefaultPosition
        {
            get { return position + (new Vector2(ViewPortWidth, ViewPortHeight) * 0.5F) - (new Vector2(ViewPortWidth, ViewPortHeight) * 0.5F / Game1.resolutionScale); }
        }
        //public static Vector2 Center
        //{
        //    get { return position + (new Vector2(ViewPortWidth, ViewPortHeight) * 0.5F); }
        //}
        //public static Vector2 PositionD
        //{
        //    get { return positionD; }
        //    set
        //    {
        //        positionD = new Vector2(
        //            MathHelper.Clamp(value.X,
        //                worldRectangle.X,
        //                worldRectangle.Width - ViewPortWidth),
        //            MathHelper.Clamp(value.Y,
        //                worldRectangle.Y,
        //                worldRectangle.Height - ViewPortHeight));
        //    }
        //}

        public static Rectangle WorldRectangle
        {
            get { return worldRectangle; }
            set { worldRectangle = value; }
        }

        public static Int32 ViewPortWidth
        {
            get { return (Int32)viewPortSize.X; }
            set { viewPortSize.X = value; }
        }

        public static Int32 ViewPortHeight
        {
            get { return (Int32)viewPortSize.Y; }
            set { viewPortSize.Y = value; }
        }

        public static void ChangeResolution(Int32 X, Int32 Y)
        {
            //ViewPortWidth = X;
            //ViewPortHeight = Y;
            ViewPortWidth = Game1.minResolutionX;
            ViewPortHeight = Game1.minResolutionY;
        }

        public static Rectangle ViewPort
        {
            get
            {
                return new Rectangle(
                    (Int32)Position.X, (Int32)Position.Y,
                    ViewPortWidth, ViewPortHeight);
            }
        }

        //public static void Move(Vector2 offset)
        //{
        //    Position += offset;
        //}

        public static bool ObjectIsVisible(Rectangle bounds)
        {
            return (ViewPort.Intersects(bounds));
        }

        public static Vector2 Transform(Vector2 point)
        {
            return (point - position) * Game1.resolutionScale;
        }

        public static Rectangle Transform(Rectangle rectangle)
        {
            return new Rectangle(
                (Int32)Math.Round(((float)rectangle.Left - position.X) * Game1.resolutionScale),
                (Int32)Math.Round(((float)rectangle.Top - position.Y) * Game1.resolutionScale),
                (Int32)Math.Round(((float)rectangle.Width * Game1.resolutionScale)),
                (Int32)Math.Round(((float)rectangle.Height * Game1.resolutionScale)));
        }

        public static RectangleF Transform(RectangleF rectangle)
        {
            return new RectangleF(
                (rectangle.X - position.X) * Game1.resolutionScale,
                (rectangle.Y - position.Y) * Game1.resolutionScale,
                rectangle.Width * Game1.resolutionScale,
                rectangle.Height * Game1.resolutionScale);
        }
    }
}
