using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
	static class ParticlesSpawner
	{
		private static Rectangle particleFrame = new Rectangle(143, 0, 12, 2); //To move somewhere else
		private static Rectangle smogRectangle = new Rectangle(32, 65, 32, 30); //To move somewhere else

		public static void AddSmog(Vector2 location, Vector2 direction)
		{
			AnimatedParticle smog = new AnimatedParticle(
				location,
				StateManager.currentState.spriteSheet,
				new List<Rectangle> { smogRectangle,
					new Rectangle(smogRectangle.X + smogRectangle.Width, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height),
					new Rectangle(smogRectangle.X + smogRectangle.Width * 2, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height) },
				Vector2.Zero,
				Vector2.Zero,
				0,
				108,
				Color.White,
				Color.Black,
				false);
			smog.Rotation = (float)Math.Atan2(direction.Y, direction.X);
			smog.loop = false;
			smog.GenerateDefaultAnimation(((float)108 / 1000) * 3);
			ParticlesManager.Effects.Add(smog);
		}

		//Used when shot hits enemy (get shot color)
		public static void AddExplosion(Vector2 location, Vector2 momentum, Color initialColor)
		{
			int particleCount = MathExtention.Rand.Next(43, 98);
			for (int x = 0; x < particleCount; x++)
			{
				Particle particle = new Particle(
					location - (momentum / 60),
					StateManager.currentState.spriteSheet,
					new List<Rectangle> { particleFrame },
					MathExtention.GetRandomVector(MathExtention.Rand.Next(90, 260)),
					Vector2.Zero,
					298,
					190,
					new Color(initialColor.R + MathExtention.Rand.Next(-108, 108), initialColor.G + MathExtention.Rand.Next(-108, 108), initialColor.B + MathExtention.Rand.Next(-108, 108)),
					Color.Black,
					true);
				particle.Rotation = (float)MathExtention.Rand.Next(0, 22);
				ParticlesManager.Effects.Add(particle);
			}
		}

		//Used when shot hits enemy (get enemy color)
		public static void AddFlakesEffect(Vector2 location, Vector2 impactVelocity, Color startColor)
		{
			int particleCount = MathExtention.Rand.Next(22, 80);
			for (int x = 0; x < particleCount; x++)
			{
				Particle particle = new Particle(
					location - (impactVelocity / 60),
					StateManager.currentState.spriteSheet,
					new List<Rectangle> { particleFrame },
					MathExtention.GetRandomVector(MathExtention.Rand.Next(0, 49)),
					Vector2.Zero,
					31,
					68,
					startColor,
					Color.Black,
					true);
				particle.Rotation = (float)MathExtention.Rand.Next(0, 22);
				ParticlesManager.Effects.Add(particle);
			}
		}

		//Usually used when shot hits wall (random color)
		public static void AddSparksEffect(Vector2 location, Vector2 impactVelocity)
		{
			int particleCount = MathExtention.Rand.Next(15, 44);
			for (int x = 0; x < particleCount; x++)
			{
				Particle particle = new Particle(
					location - (impactVelocity / 60),
					StateManager.currentState.spriteSheet,
					new List<Rectangle> { particleFrame },
					MathExtention.GetRandomVector(MathExtention.Rand.Next(37, 290)),
					Vector2.Zero,
					298,
					190,
					ParticlesManager.GetRandomParticleColor(),
					Color.Black,
					true);
				particle.Rotation = (float)MathExtention.Rand.Next(0, 22);
				ParticlesManager.Effects.Add(particle);
			}
		}

		//Used when mixe Exploded (red)
		public static void AddMineExplosion(Vector2 location)
		{
			int particleCount = MathExtention.Rand.Next(230, 500);
			for (int x = 0; x < particleCount; x++)
			{
				Particle particle = new Particle(
					location,
					StateManager.currentState.spriteSheet,
					new List<Rectangle> { particleFrame },
					MathExtention.GetRandomVector(MathExtention.Rand.Next(140, 440)),
					Vector2.Zero,
					450,
					100,
					Color.Red,
					Color.Black,
					true);
				particle.Rotation = (float)MathExtention.Rand.Next(0, 22);
				ParticlesManager.Effects.Add(particle);
			}
		}
	}
}