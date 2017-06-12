using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnrealMono
{
	//public class RectangleF2
	//{
	//    public float Height;
	//    public float Width;
	//    public float X;
	//    public float Y;

	//    public RectangleF2(float x, float y, float width, float height)
	//    {
	//        X = x;
	//        Y = y;
	//        Width = width;
	//        Height = height;
	//    }
	//    public RectangleF2(int x, int y, int width, int height)
	//    {
	//        X = (float)x;
	//        Y = (float)y;
	//        Width = (float)width;
	//        Height = (float)height;
	//    }
	//    public Rectangle GetRectangle()
	//    {
	//        return new Rectangle((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Width), (int)Math.Round(Height));
	//    }
	//}

	public struct Transform
	{
		public Vector2 location;
		public float rotation;
		public float scale;

		public Transform(Vector2 location)
		{
			this.location = location;
			rotation = 0F;
			scale = 1F;
		}

		public Transform(Vector2 location, float rotation, float scale = 1F)
		{
			this.location = location;
			this.rotation = rotation;
			this.scale = scale;
		}
	}

	public class Circle
	{
		private Vector2 location;
		private float radius;

		public Circle(Vector2 location, float radius)
		{
			this.location = location;
			this.radius = radius;
		}

		public bool IsCollidingWith(Vector2 otherLocation, float otherRadius)
		{
			if (Vector2.DistanceSquared(location, otherLocation) < (radius + otherRadius) * (radius + otherRadius))
			{
				return true;
			}
			return false;
		}

		public bool IsCollidingWith(Circle otherCircle)
		{
			if (Vector2.DistanceSquared(location, otherCircle.location) < (radius + otherCircle.radius) * (radius + otherCircle.radius))
			{
				return true;
			}
			return false;
		}
	}

	public static class MathExtention
	{
		private static Random rand = new Random();
		public static Random Rand { get { return rand; } }

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
			if (val.CompareTo(max) > 0)
				return max;
			return val;
		}

		//public static void Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		//{
		//    if (val.CompareTo(min) < 0)
		//        val = min;
		//    else if (val.CompareTo(max) > 0)
		//        val = max;
		//}
		//public static bool TryClamp<T>(this T val, T min, T max) where T : IComparable<T>
		//{
		//    if (val.CompareTo(min) < 0)
		//    {
		//        val = min;
		//        return true;
		//    }
		//    if (val.CompareTo(max) > 0)
		//    {
		//        val = max;
		//        return true;
		//    }
		//    return false;
		//}
		public static Rectangle FitRotation(this Rectangle rectangle, float angle)
		{
			Vector2 center = new Vector2(rectangle.Left, rectangle.Bottom) + ((new Vector2(rectangle.Right, rectangle.Top) - new Vector2(rectangle.Left, rectangle.Bottom)) / 2F);
			Vector2 topLeft = (new Vector2(rectangle.Left, rectangle.Top) - center).Rotate(angle);
			Vector2 topRight = (new Vector2(rectangle.Right, rectangle.Top) - center).Rotate(angle);
			float topY = Math.Max(Math.Abs(topLeft.Y), Math.Abs(topRight.Y));
			float rightX = Math.Max(Math.Abs(topLeft.X), Math.Abs(topRight.X));
			return new Rectangle(SymmetricCeiling(center.X - rightX), SymmetricCeiling(center.Y - topY), (int)Math.Ceiling(topY * 2), (int)Math.Ceiling(rightX * 2));
		}

		public static bool IntersectsPoint(this Rectangle rectangle, Point point)
		{
			return rectangle.Intersects(new Rectangle(point.X, point.Y, 1, 1));
		}

		public static bool IntersectsPoint(this Rectangle rectangle, Vector2 point)
		{
			return rectangle.Intersects(new Rectangle((int)point.X, (int)point.Y, 1, 1));
		}

		public static int Round(this float val)
		{
			return (int)Math.Round(val);
		}

		public static int SymmetricCeiling(this float val)
		{
			return (int)(val >= 0f ? Math.Ceiling(val) : Math.Floor(val));
	    }

	    public static int SymmetricFloor(this float val)
	    {
	        return (int)(val >= 0f ? Math.Floor(val) : Math.Ceiling(val));
	    }

        public static float DistanceFrom45(this float val)
		{
			while (val > 1)
			{
				val -= 1;
			}
			return Math.Abs((Math.Abs(val - 0.5F) * 2F) - 1F);
		}

		public static float ToDegrees(this float val)
		{
			return val / ((float)Math.PI / 180F);
		}

		public static float ToRadians(this float val)
		{
			return val * ((float)Math.PI / 180F);
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

	public static class TypeExtention
	{
		//public static Vector2 ReplaceString(this string val, string toReplace, string with)
		//{
		//    float cosA = (float)Math.Cos(radians);
		//    float sinA = (float)Math.Sin(radians);
		//    return new Vector2((cosA * vector.X) - (sinA * vector.Y), (sinA * vector.X) + (cosA * vector.Y));
		//}
	}
	
	public static class ContentManagerExtention
	{
		public static Dictionary<string, T> LoadContent<T>(this ContentManager contentManager, bool recursive = false, string contentLocalDirectory = "")
		{
			//Load directory info, abort if none
			DirectoryInfo directoryInfo = new DirectoryInfo(contentManager.RootDirectory + (contentLocalDirectory != "" ? ("\\" + contentLocalDirectory) : ""));
			if (!directoryInfo.Exists)
			{
				return new Dictionary<string, T>();
				//throw new DirectoryNotFoundException();
			}

			//Init the resulting list
			Dictionary<string, T> result = new Dictionary<string, T>();

			//Load all files that matches the file filter
			FileInfo[] filesInfo = directoryInfo.GetFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			foreach (FileInfo fileInfo in filesInfo)
			{
				//Used as Dictionary key
				string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
				string filePath = fileInfo.DirectoryName.Replace(directoryInfo.Parent.FullName, "").TrimStart('\\').Replace("\\", "/") + "/" + fileName;
				
				//if (result.ContainsKey(fileName)) //Not necessary
				result[fileName] = contentManager.Load<T>(filePath);
			}
			//Return the result
			return result;
		}
	}
}