using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    static class ParticlesManager
    {
        private static int maxParticles = 5000; //Around 2760 is a good balance
        private static Random rand = new Random();
        private static Rectangle ParticleFrame = new Rectangle(143, 0, 12, 2);
        static private Rectangle smogRectangle = new Rectangle(32, 65, 32, 30);
        public static List<Particle> Effects = new List<Particle>();
        
        public static void Initialize()
        {
            Effects.Clear();
        }

        public static Color GetRandomParticleColor()
        {
            Color color;

            int randTemp = rand.Next(0, 7);

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
            Effects.Add(smog);
        }
        //Used when shot hits enemy (get shot color)
        public static void AddExplosion(Vector2 location, Vector2 momentum, Color initialColor)
        {
            int particleCount = rand.Next(43, 98);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (momentum / 60),
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    MathExtention.GetRandomVector(rand.Next(90, 260)),
                    Vector2.Zero,
                    298,
                    190,
                    new Color(initialColor.R + rand.Next(-108, 108), initialColor.G + rand.Next(-108, 108), initialColor.B + rand.Next(-108, 108)),
                    Color.Black,
                    true);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
        }
        //Used when shot hits enemy (get enemy color)
        public static void AddFlakesEffect(Vector2 location, Vector2 impactVelocity, Color startColor)
        {
            int particleCount = rand.Next(22, 80);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (impactVelocity / 60),
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    MathExtention.GetRandomVector(rand.Next(0, 49)),
                    Vector2.Zero,
                    31,
                    68,
                    startColor,
                    Color.Black,
                    true);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
        }
        //Usually used when shot hits wall (random color)
        public static void AddSparksEffect(Vector2 location, Vector2 impactVelocity)
        {
            int particleCount = rand.Next(15, 44);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (impactVelocity / 60),
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    MathExtention.GetRandomVector(rand.Next(37, 290)),
                    Vector2.Zero,
                    298,
                    190,
                    GetRandomParticleColor(),
                    Color.Black,
                    true);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
        }
        //Used when mixe Exploded (red)
        public static void AddMineExplosion(Vector2 location)
        {
            int particleCount = rand.Next(230, 500);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    MathExtention.GetRandomVector(rand.Next(140, 440)),
                    Vector2.Zero,
                    450,
                    100,
                    Color.Red,
                    Color.Black,
                    true);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
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