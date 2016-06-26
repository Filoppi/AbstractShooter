using Microsoft.Xna.Framework;
using System;

namespace AbstractShooter
{
    public static class MathExtention
    {
        private static Random rand = new Random();

        public static Vector2 Rotate(this Vector2 vector, float radians)
        {
            float cosA = (float)Math.Cos(radians);
            float sinA = (float)Math.Sin(radians);
            return new Vector2((cosA * vector.X) - (sinA * vector.Y), (sinA * vector.X) + (cosA * vector.Y));
        }

        public static void Pow(this Vector2 vector, double exponent)
        {
            vector.X = (float)Math.Pow(vector.X, exponent);
            vector.Y = (float)Math.Pow(vector.X, exponent);
        }
        public static float PositiveCycle(this float val, float max, float min)
        {
            while (val.CompareTo(max) > 0)
            {
                val -= max;
            }
            return val;
        }
        public static float Cycle(this float val, float max, float min)
        {
            while (val.CompareTo(max) > 0)
            {
                val -= max;
            }
            while (val.CompareTo(min) < 0)
            {
                val += min;
            }
            return val;
        }
        public static T GetClamped<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            else if (val.CompareTo(max) > 0)
                return max;
            return val;
        }
        public static void Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                val = min;
            else if (val.CompareTo(max) > 0)
                val = max;
        }
        public static Vector2 GetRandomVector(float length)
        {
            Vector2 direction = new Vector2(rand.Next(1, 101) - 50, rand.Next(1, 101) - 50);
            direction.Normalize();
            return direction * length;
        }
        public static Color GetRandomColor()
        {
           return new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255), 255);
        }
    }
}