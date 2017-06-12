using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace UnrealMono
{
	public static class ParticlesManager
	{
		private static int maxParticles = 5000; //Around 2760 is a good balance
		public static List<Particle> Effects = new List<Particle>();

		public static void Initialize()
		{
			Effects.Clear();
		}

		public static Color GetRandomParticleColor()
		{
			Color color;

			int randTemp = MathExtention.Rand.Next(0, 7);

			if (randTemp == 0)
				color = new Color(0, 161, 48);
			else if (randTemp == 1)
				color = new Color(0, 141, 151);
			else if (randTemp == 2)
				color = new Color(0, 103, 141);
			else if (randTemp == 3)
				color = new Color(0, 175, 157);
			else if (randTemp == 4)
				color = new Color(169, 0, 6);
			else if (randTemp == 5)
				color = new Color(168, 67, 0);
			else //if (randTemp == 6)
				color = new Color(171, 171, 0);

			return color;
		}

		static public void Update(GameTime gameTime)
		{
			while (Effects.Count >= maxParticles)
			{
				Effects.RemoveAt(0);
			}
			for (int x = Effects.Count - 1; x >= 0; x--)
			{
				Effects[x].Update(gameTime);
				if (Effects[x].Expired)
				{
					Effects.RemoveAt(x);
				}
			}
		}

		static public void Draw()
		{
			foreach (Particle sprite in Effects)
			{
				sprite.Draw();
			}
		}
	}
}