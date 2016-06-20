using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    static class EffectsManager
    {
        private static int maxParticles = 5000; //Around 2760 is a good balance
        private static Random rand = new Random();
        private static Rectangle ParticleFrame = new Rectangle(143, 0, 12, 2);

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

        //Usually used when shot hits wall (random color)
        public static void AddExplosion(Vector2 location, Vector2 momentum, Color initialColor)
        {
            int particleCount = rand.Next(43, 98);
            for (int x = 0; x < particleCount; x++)
            {
                ParticleComponent particle = new ParticleComponent(
                    StateManager.currentState.Scene,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    190, 0,
                    StateManager.currentState.Scene.RootComponent,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Particles,
                    location - (momentum / 60),
                    true,
                    1,
                    MathExtention.GetRandomVector(rand.Next(90, 260)),
                    Vector2.Zero,
                    298,
                    new Color(initialColor.R + rand.Next(-108, 108), initialColor.G + rand.Next(-108, 108), initialColor.B + rand.Next(-108, 108)),
                    Color.Black);
                particle.LocalRotation = rand.Next(0, 22);
            }
        }
        //Used when shot hits enemy (get shot color)
        public static void AddSparksEffect(Vector2 location, Vector2 impactVelocity)
        {
            int particleCount = rand.Next(15, 44);
            for (int x = 0; x < particleCount; x++)
            {
                ParticleComponent particle = new ParticleComponent(
                    StateManager.currentState.Scene,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    190, 0,
                    StateManager.currentState.Scene.RootComponent,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Particles,
                    location - (impactVelocity / 60),
                    true,
                    1,
                    MathExtention.GetRandomVector(rand.Next(37, 290)),
                    Vector2.Zero,
                    298,
                    GetRandomParticleColor(),
                    Color.Black);
                particle.LocalRotation = rand.Next(0, 22);
            }
        }
        //Used when shot hits enemy (get enemy color)
        public static void AddFlakesEffect(Vector2 location, Vector2 impactVelocity, Color startColor)
        {
            int particleCount = rand.Next(22, 80);
            for (int x = 0; x < particleCount; x++)
            {
                ParticleComponent particle = new ParticleComponent(
                    StateManager.currentState.Scene,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    68, 0,
                    StateManager.currentState.Scene.RootComponent,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Particles,
                    location - (impactVelocity / 60),
                    true,
                    1,
                    MathExtention.GetRandomVector(rand.Next(0, 49)),
                    Vector2.Zero,
                    31,
                    startColor,
                    Color.Black);
                particle.LocalRotation = rand.Next(0, 22);
            }
        }
        //Used when mixe Exploded (red)
        public static void AddMineExplosion(Vector2 location)
        {
            int particleCount = rand.Next(230, 500);
            for (int x = 0; x < particleCount; x++)
            {
                ParticleComponent particle = new ParticleComponent(
                    StateManager.currentState.Scene,
                    StateManager.currentState.spriteSheet,
                    new List<Rectangle> { ParticleFrame },
                    100, 0,
                    StateManager.currentState.Scene.RootComponent,
                    ComponentUpdateGroup.AfterActor, DrawGroup.Particles,
                    location,
                    true,
                    1,
                    MathExtention.GetRandomVector(rand.Next(140, 440)),
                    Vector2.Zero,
                    450,
                    Color.Red,
                    Color.Black);
                particle.LocalRotation = rand.Next(0, 22);
            }
        }

        //static public void Update(GameTime gameTime)
        //{
        //    while (Effects.Count >= EffectsMaxNumber)
        //    {
        //        Effects.RemoveAt(0);
        //    }
        //    for (int x = Effects.Count - 1; x >= 0; x--)
        //    {
        //        Effects[x].Update(gameTime);
        //    }
        //}
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    static class EffectsManager
    {
        public static List<Particle> Effects = new List<Particle>();
        private static int EffectsMaxNumber = 5000; //Around 2760 is a good balance
        private static Random rand = new Random();
        private static Rectangle ParticleFrame = new Rectangle(143, 0, 12, 2);

        public static void Initialize()
        {
            Effects.Clear();
        }

        #region AddExplosions
        //Usually used when shot hits wall (random color)
        public static void AddExplosion(Vector2 location, Vector2 momentum, Color initialColor)
        {
            int particleCount = rand.Next(43, 98);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (momentum / 60),
                    StateManager.State.spriteSheet,
                    ParticleFrame,
                    randomDirection((float)rand.Next(90, 260)),
                    Vector2.Zero,
                    298,
                    190,
                    new Color(initialColor.R + rand.Next(-108, 108), initialColor.G + rand.Next(-108, 108), initialColor.B + rand.Next(-108, 108)),
                    Color.Black);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
        }
        //Used when shot hits enemy (get shot color)
        public static void AddSparksEffect(Vector2 location, Vector2 impactVelocity)
        {
            int particleCount = rand.Next(15, 44);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (impactVelocity / 60),
                    StateManager.State.spriteSheet,
                    ParticleFrame,
                    randomDirection((float)rand.Next(37, 290)),
                    Vector2.Zero,
                    298,
                    190,
                    RandomColor(),
                    Color.Black);
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
                    StateManager.State.spriteSheet,
                    ParticleFrame,
                    randomDirection((float)rand.Next(0, 49)),
                    Vector2.Zero,
                    31,
                    68,
                    startColor,
                    Color.Black);
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
                    StateManager.State.spriteSheet,
                    ParticleFrame,
                    randomDirection((float)rand.Next(140, 440)),
                    Vector2.Zero,
                    450,
                    100,
                    Color.Red,
                    Color.Black);
                particle.Rotation = (float)rand.Next(0, 22);
                Effects.Add(particle);
            }
        }
        #endregion

        static public void Update(GameTime gameTime)
        {
            while (Effects.Count >= EffectsMaxNumber)
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
            foreach (Sprite sprite in Effects)
            {
                sprite.Draw();
            }
        }
    }
}
*/
