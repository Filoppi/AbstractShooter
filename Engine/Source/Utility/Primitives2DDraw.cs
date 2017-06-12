using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace UnrealMono
{
	public static class Primitives2DDraw
	{
		private static Texture2D pixel;
		private static readonly Dictionary<String, List<Vector2>> circleCache = new Dictionary<string, List<Vector2>>();

		public static void CreatePixel(SpriteBatch spriteBatch)
		{
			if (pixel == null)
			{
				pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
				pixel.SetData(new[] { Color.White });
			}
		}

		///Creates a list of vectors that represents a circle
		private static List<Vector2> CreateCircle(double radius, int sides)
		{
			// Look for a cached version of this circle
			String circleKey = radius + "x" + sides;
			if (circleCache.ContainsKey(circleKey))
			{
				return circleCache[circleKey];
			}

			List<Vector2> vectors = new List<Vector2>();

			const double max = 2.0 * Math.PI;
			double step = max / sides;

			for (double theta = 0.0; theta < max; theta += step)
			{
				vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
			}

			// then add the first vector again so it's a complete loop
			vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

			// Cache this circle so that it can be quickly drawn next time
			circleCache.Add(circleKey, vectors);

			return vectors;
		}

		///Draws a circle
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness, float layerDepth)
		{
			DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, thickness, layerDepth);
		}

		///Draws a circle
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness, float layerDepth)
		{
			DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness, layerDepth);
		}

		/// <summary>
		/// Draws a list of connecting points
		/// </summary>
		/// <param name="position">Where to position the points</param>
		/// <param name="points">The points to connect with lines</param>
		private static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, List<Vector2> points, Color color, float thickness, float layerDepth)
		{
			if (points.Count < 2)
				return;

			for (int i = 1; i < points.Count; i++)
			{
				DrawLine(spriteBatch, points[i - 1] + position, points[i] + position, color, thickness, layerDepth);
			}
		}

		///Draws a line from screen position (x1,y1) to (x2,y2) with thickness thickness
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness, float layerDepth)
		{
			DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness, layerDepth);
		}

		///Draws a line from screen point1 to point2 with thickness thickness
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness, float layerDepth)
		{
			//calculate the distance between the two vectors
			float distance = Vector2.Distance(point1, point2);

			//calculate the angle between the two vectors
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			DrawLine(spriteBatch, point1, distance, angle, color, thickness, layerDepth);
		}

		///Draws a line from point with length length and thickness thickness
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness, float layerDepth)
		{
			Vector2 thicnessCenterDelta = Vector2.Zero;
			double upAngle = angle - (Math.PI / 2);
			thicnessCenterDelta.X = (float)Math.Cos(upAngle);
			thicnessCenterDelta.Y = (float)Math.Sin(upAngle);
			thicnessCenterDelta *= thickness / 2;
			//stretch the pixel between the two vectors
			spriteBatch.Draw(pixel,
							 point + thicnessCenterDelta,
							 null,
							 color,
							 angle,
							 Vector2.Zero,
							 new Vector2(length, thickness),
							 SpriteEffects.None,
							 layerDepth);
		}
	}
}