using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AbstractShooter
{
    public static class Camera
    {
        public const int defaultViewPortX = 1280;
        public const int defaultViewPortY = 720;
        private static Vector2 centerLocation = Vector2.Zero;
        private static Vector2 topLeftLocation = Vector2.Zero;
        private static Vector2 viewPort = new Vector2(defaultViewPortX, defaultViewPortY);
        private static float zoomScale = 1F;
        public static float ZoomScale
        {
            get { return zoomScale; }
            set { zoomScale = value.GetClamped(0.01F, 10F); }
        }
        public static float DrawScale { get { return Game1.ResolutionScale * zoomScale; } }

        public static Vector2 TopLeft
        {
            get { return topLeftLocation; }
            set
            {
                topLeftLocation = value;
                centerLocation = value + (ViewPort * 0.5F);
            }
        }
        public static Vector2 Center
        {
            get { return centerLocation; }
            set
            {
                centerLocation = value;
                topLeftLocation = value - (ViewPort * 0.5F);
            }
        }
        public static Vector2 BottomRight
        {
            get { return topLeftLocation + ViewPort; }
        }

        public static Vector2 ViewPort
        {
            get { return viewPort / zoomScale; }
        }
        public static int ViewPortWidth
        {
            get { return (int)(viewPort.X / zoomScale); }
            set { viewPort.X = value; }
        }
        public static int ViewPortHeight
        {
            get { return (int)(viewPort.Y / zoomScale); }
            set { viewPort.Y = value; }
        }
        
        public static Rectangle ViewPortRectangle
        {
            get
            {
                return new Rectangle((int)topLeftLocation.X, (int)topLeftLocation.Y,
                    ViewPortWidth,
                    ViewPortHeight);
            }
        }
        //public static Rectangle HalvedViewPort //Just for test.. Could be used as a kind of torch in exploration levels if the shape was circular???
        //{
        //    get
        //    {
        //        return new Rectangle(
        //            (int)Position.X + (ViewPortWidth / 4), (int)Position.Y + (ViewPortHeight / 4),
        //            ViewPortWidth / 2, ViewPortHeight / 2);
        //    }
        //}

        //public static void Move(Vector2 offset)
        //{
        //    Position += offset;
        //}

        public static bool IsRectangleVisible(Rectangle worldRectangle)
        {
            return ViewPortRectangle.Intersects(worldRectangle);
        }

        public static Vector2 GetNormalizedViewPortAlpha(Vector2 worldLocation)
        {
            return (worldLocation - centerLocation) / (ViewPort / 2);
        }

        public static Vector2 WorldToScreenSpace(Vector2 worldLocation)
        {
            return (worldLocation - topLeftLocation) * DrawScale;
        }
        public static Rectangle WorldToScreenSpace(Rectangle worldRectangle)
        {
            return new Rectangle(
                (int)Math.Round((worldRectangle.Left - topLeftLocation.X) * DrawScale),
                (int)Math.Round((worldRectangle.Top - topLeftLocation.Y) * DrawScale),
                (int)Math.Round((worldRectangle.Width * DrawScale)),
                (int)Math.Round((worldRectangle.Height * DrawScale)));
        }
        public static RectangleF WorldToScreenSpace(RectangleF worldRectangle)
        {
            return new RectangleF(
                (worldRectangle.X - topLeftLocation.X) * DrawScale,
                (worldRectangle.Y - topLeftLocation.Y) * DrawScale,
                worldRectangle.Width * DrawScale,
                worldRectangle.Height * DrawScale);
        }
    }
}
