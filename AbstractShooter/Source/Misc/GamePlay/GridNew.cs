using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using AbstractShooter.States; //Temp
using UnrealMono;

namespace AbstractShooter
{
	public class PointMass
	{
		public Vector3 RestPosition;
		public Vector3 Position;
		public Vector3 Velocity;
		public float InverseMass;

		private Vector3 acceleration;
		private float damping = 0.98f;

		public PointMass(Vector3 position, float invMass)
		{
			RestPosition = position;
			Position = position;
			InverseMass = invMass;
		}

		public void ApplyForce(Vector3 force)
		{
			acceleration += force * InverseMass;
		}

		public void IncreaseDamping(float factor)
		{
			damping *= factor;
		}

		public void Update()
		{
			Vector3 dist = Position - RestPosition;
			Velocity += acceleration;
			Position += Velocity;
			Vector3 dist2 = Position - RestPosition;
			dist.Normalize();
			dist2.Normalize();
			if (Velocity != Vector3.Zero && Vector3.Dot(dist, dist2) < 0)
			//if (Velocity != Vector3.Zero && (-dist - dist2).Length() < 0.01)
			{
				Position = RestPosition;
				Velocity = Vector3.Zero;
				acceleration = Vector3.Zero;
			}
			acceleration = Vector3.Zero;
			if (Velocity.LengthSquared() < 0.001f * 0.001f)
				Velocity = Vector3.Zero;

			Velocity *= damping;
			damping = 0.98f;
		}
	}
	public struct Spring
	{
		public PointMass End1;
		public PointMass End2;
		public float TargetLength;
		public float Stiffness;
		public float Damping;

		public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
		{
			End1 = end1;
			End2 = end2;
			Stiffness = stiffness;
			Damping = damping;
			TargetLength = Vector3.Distance(end1.Position, end2.Position) * 0.95F;
			TargetLength = 0F; //Temp
		}

		public void Update()
		{
			var x = End1.Position - End2.Position;
			x = End1.Position - End1.RestPosition; //Temp

			float length = x.Length();
			// these springs can only pull, not push
			if (length <= TargetLength)
				return;

			x = (x / length) * (length - TargetLength);
			var dv = End2.Velocity - End1.Velocity;
			dv = Vector3.Zero; //Temp
			var force = Stiffness * x - dv * Damping;

			End1.ApplyForce(-force);
			//End2.ApplyForce(force); //Temp
		}
	}

	public class GridNew
	{
		//const int maxGridPoints = 1600;
		//Vector2 gridSpacing = new Vector2((float)Math.Sqrt(Viewport.Width * Viewport.Height / maxGridPoints));
		//GridNew = new GridNew(Viewport.Bounds, gridSpacing);

		Spring[] springs;
		PointMass[,] points;

		public GridNew(Rectangle size, Vector2 spacing)
		{
			var springList = new List<Spring>();

			int numColumns = (int)(size.Width / spacing.X) + 1;
			int numRows = (int)(size.Height / spacing.Y) + 1;
			points = new PointMass[numColumns, numRows];

			// these fixed points will be used to anchor the grid to fixed positions on the screen
			PointMass[,] fixedPoints = new PointMass[numColumns, numRows];

			// create the point masses
			int column = 0, row = 0;
			for (float y = size.Top; y <= size.Bottom; y += spacing.Y)
			{
				for (float x = size.Left; x <= size.Right; x += spacing.X)
				{
					points[column, row] = new PointMass(new Vector3(x, y, 0), 1);
					fixedPoints[column, row] = new PointMass(new Vector3(x, y, 0), 0);
					column++;
				}
				row++;
				column = 0;
			}

			// link the point masses with springs
			for (int y = 0; y < numRows; y++)
				for (int x = 0; x < numColumns; x++)
				{
					//Temp disabled
					//if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)    // anchor the border of the grid 
					//	springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.1f, 0.1f));
					//else if (x % 3 == 0 && y % 3 == 0)                                  // loosely anchor 1/9th of the point masses 
					//	springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.5002f, 0.502f));

					const float stiffness = 0.28f;
					const float damping = 0.06f;
					//Temp disabled
					//if (x > 0)
					//	springList.Add(new Spring(points[x - 1, y], points[x, y], stiffness, damping));
					//if (y > 0)
					//	springList.Add(new Spring(points[x, y - 1], points[x, y], stiffness, damping));
					if (x > 0 && y > 0) //Temp
						springList.Add(new Spring(points[x, y], points[x, y], stiffness, damping));
				}

			springs = springList.ToArray();
		}

		public void Update()
		{
			foreach (AActor actor in StateManager.currentState.GetAllActors()) //Disable influence from world scene
			{
				float actorMass = actor.GetMass();
				if (actorMass != 0F)
				{
					//if (((Level)StateManager.currentState).grid.speed)
					//{
					//	actorMass *= 0.18F + (actor.RootComponent.localVelocity.Length() / 400F);
					//	actorMass = -actorMass;
					//}
					if (actorMass > 0)
					{
						ApplyExplosiveForce(actorMass * ((Level)StateManager.currentState).grid.powExp * 4, new Vector3(actor.GetMassCenter(), 0F), ((Level)StateManager.currentState).grid.gravity / 4500F); //To adjust Z
					}
					else
					{
						actorMass = -actorMass;
						ApplyImplosiveForce(actorMass * ((Level)StateManager.currentState).grid.powExp * 4, new Vector3(actor.GetMassCenter(), 0F), ((Level)StateManager.currentState).grid.gravity / 4500F); //To adjust Z
					}
				}
			}

			foreach (var spring in springs)
				spring.Update();

			foreach (var mass in points)
				mass.Update();
		}

		public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
		{
			foreach (var mass in points)
				if (Vector3.DistanceSquared(position, mass.Position) < radius * radius)
					mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
		}

		public void ApplyImplosiveForce(float force, Vector3 position, float radius)
		{
			foreach (var mass in points)
			{
				float dist2 = Vector3.DistanceSquared(position, mass.Position);
				if (dist2 < radius * radius * radius)
				{
					mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
					mass.IncreaseDamping(0.6f);
				}
			}
		}

		public void ApplyExplosiveForce(float force, Vector3 position, float radius)
		{
			foreach (var mass in points)
			{
				float dist2 = Vector3.DistanceSquared(position, mass.Position);
				if (dist2 < radius * radius * radius)
				{
					mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
					mass.IncreaseDamping(0.6f);
				}
			}
		}
		public Vector2 ToVec2(Vector3 v) //To check
		{
			// do a perspective projection
			float factor = 1F + (v.Z / 2000F);
			if (((Level)StateManager.currentState).grid.speed)
			{
				return (new Vector2(v.X, v.Y));
			}
			else
			{
				//return (new Vector2(v.X, v.Y)) * factor;
				return (new Vector2(v.X, v.Y) - UnrealMonoGame.currentResolution.ToVector2() / 2f) * factor + UnrealMonoGame.currentResolution.ToVector2() / 2;
			}
		}
		public void Draw(SpriteBatch spriteBatch)
		{
			int width = points.GetLength(0);
			int height = points.GetLength(1);
			Color color = new Color(30, 30, 139, 85);   // dark blue

			for (int y = 1; y < height; y++)
			{
				for (int x = 1; x < width; x++)
				{
					color = Color.Lerp(new Color(30, 30, 139, 85), Color.Orange, Vector3.Distance(points[x, y].Position , points[x, y].RestPosition) * 0.01F);

					//1
					Vector2 left = new Vector2();
					Vector2 up = new Vector2();
					Vector3 pos = new Vector3(Camera.WorldToScreenSpace(new Vector2(points[x, y].Position.X, points[x, y].Position.Y)), points[x, y].Position.Z);
					Vector2 p = ToVec2(points[x, y].Position);
					p = ToVec2(pos);
					if (x > 1)
					{
						pos = new Vector3(Camera.WorldToScreenSpace(new Vector2(points[x - 1, y].Position.X, points[x - 1, y].Position.Y)), points[x - 1, y].Position.Z);
						//left = ToVec2(points[x - 1, y].Position);
						left = ToVec2(pos);
						float thicknessLocal = y % 3 == 1 ? 3f : 1f;
						spriteBatch.DrawLine(left, p, color, thicknessLocal, DrawGroup.Background3);
					}
					if (y > 1)
					{
						pos = new Vector3(Camera.WorldToScreenSpace(new Vector2(points[x, y - 1].Position.X, points[x, y - 1].Position.Y)), points[x, y - 1].Position.Z);
						//up = ToVec2(points[x, y - 1].Position);
						up = ToVec2(pos);
						float thicknessLocal = x % 3 == 1 ? 3f : 1f;
						spriteBatch.DrawLine(up, p, color, thicknessLocal, DrawGroup.Background3);
					}


					////2
					if (x > 1 && y > 1)
					{
						pos = new Vector3(Camera.WorldToScreenSpace(new Vector2(points[x - 1, y - 1].Position.X, points[x - 1, y - 1].Position.Y)), points[x - 1, y - 1].Position.Z);
						Vector2 upLeft = ToVec2(points[x - 1, y - 1].Position);
						upLeft = ToVec2(pos);
						spriteBatch.DrawLine(0.5f * (upLeft + up), 0.5f * (left + p), color, 1f, DrawGroup.Background3);   // vertical line
						spriteBatch.DrawLine(0.5f * (upLeft + left), 0.5f * (up + p), color, 1f, DrawGroup.Background3);   // horizontal line
					}


					////3
					//left = ToVec2(points[x - 1, y].Position);
					//float thickness = y % 3 == 1 ? 3f : 1f;

					//// use Catmull-Rom interpolation to help smooth bends in the grid
					//int clampedX = Math.Min(x + 1, width - 1);
					//Vector2 mid = Vector2.CatmullRom(ToVec2(points[x - 2, y].Position), left, p, ToVec2(points[clampedX, y].Position), 0.5f);

					//// If the grid is very straight here, draw a single straight line. Otherwise, draw lines to our
					//// new interpolated midpoint
					//if (Vector2.DistanceSquared(mid, (left + p) / 2) > 1)
					//{
					//	spriteBatch.DrawLine(left, mid, color, thickness, DrawGroup.Background3);
					//	spriteBatch.DrawLine(mid, p, color, thickness, DrawGroup.Background3);
					//}
					//else
					//	spriteBatch.DrawLine(left, p, color, thickness, DrawGroup.Background3);
				}
			}
		}
	}
}