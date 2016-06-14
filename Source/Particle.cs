using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class Particle : Sprite
    {
        private Vector2 acceleration;
        private float maxSpeed;
        private float initialDuration;
        protected float remainingDuration;
        private Color initialColor;
        private Color finalColor;

        private bool ShouldRender = true;
        public float StartFlashingAt = 0; //Temp

        public float ElapsedDuration
        {
            get
            {
                return initialDuration - remainingDuration;
            }
        }
        public float DurationProgress
        {
            get
            {
                return ElapsedDuration / initialDuration;
            }
        }
        public bool IsActive
        {
            get
            {
                return (remainingDuration > 0);
            }
        }

        public Particle(Vector2 worldLocation, float newScale, Texture2D texture, Rectangle initialFrame, Vector2 velocity, float duration)
            : base(worldLocation, newScale, texture, initialFrame, velocity)
        {
            initialDuration = duration;
            remainingDuration = duration;
            this.FrameTime = 0;
            this.acceleration = Vector2.Zero;
            this.maxSpeed = 0;
            this.Reanimate = false;
        }
        public Particle(
            Vector2 location,
            Texture2D texture,
            Rectangle initialFrame,
            Vector2 velocity,
            Vector2 acceleration,
            float maxSpeed,
            float duration,
            Color initialColor,
            Color finalColor)
            : base(location, 1.0f, texture, initialFrame, velocity)
        {
            initialDuration = duration;
            remainingDuration = duration;
            this.FrameTime = 0.4635f;
            this.acceleration = acceleration;
            this.maxSpeed = maxSpeed;
            this.initialColor = initialColor;
            this.finalColor = finalColor;
            this.finalColor.A = 0;
            this.Reanimate = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                Velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds * 60F;
                if (Velocity.Length() > maxSpeed)
                {
                    Vector2 vel = Velocity;
                    vel.Normalize();
                    Velocity = vel * maxSpeed;
                }
                if (initialColor != finalColor)
                {
                    TintColor = Color.Lerp(
                        initialColor,
                        finalColor,
                        DurationProgress);
                }
                remainingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * GameManager.TimeScale;
            }
            else
            {
                Expired = true;
            }

            base.Update(gameTime);

            if (remainingDuration < StartFlashingAt)
            {
                ShouldRender = System.Decimal.Remainder(System.Decimal.Remainder((Decimal)remainingDuration, 30M), 2M) < 1M;
            }
        }

        public override void Draw()
        {
            if (ShouldRender && IsActive)
            {
                base.Draw();
            }
        }
    }
}