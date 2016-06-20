using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class Sprite
    {
        #region Declarations
        public Texture2D Texture;

        private Vector2 worldLocation = Vector2.Zero;
        private Vector2 velocity = Vector2.Zero;

        private List<Rectangle> frames = new List<Rectangle>();

        private int currentFrame;
        private float frameTime = 0.1f;
        private double timeForCurrentFrame = 0.0f;

        private Color tintColor = Color.White;

        private float rotation = 0.0f;
        public float scale = 1.0f;

        public bool Expired = false;
        public bool Animate = true;
        public bool AnimateWhenStopped = true;
        public bool Reanimate = true;

        public bool Collidable = true;
        public float CollisionRadius = 0;
        public int BoundingXPadding = 0;
        public int BoundingYPadding = 0;
        #endregion

        #region Constructors
        public Sprite(Vector2 worldLocation, float newScale, Texture2D texture, Rectangle initialFrame, Vector2 velocity)
        {
            this.worldLocation = worldLocation;
            scale = newScale;
            Texture = texture;
            this.velocity = velocity;

            frames.Add(initialFrame);

            CollisionRadius = newScale * initialFrame.Width / 2.0f;
        }
        public Sprite(Vector2 worldLocation, float newScale, Texture2D texture, Color newColor, Rectangle initialFrame, Vector2 velocity)
        {
            this.worldLocation = worldLocation;
            scale = newScale;
            Texture = texture;
            tintColor = newColor;
            this.velocity = velocity;

            frames.Add(initialFrame);

            CollisionRadius = newScale * initialFrame.Width / 2.0f;
        }
        #endregion

        #region Drawing and Animation Properties
        public int FrameWidth
        {
            get { return frames[0].Width; }
        }

        public int FrameHeight
        {
            get { return frames[0].Height; }
        }

        public Color TintColor
        {
            get { return tintColor; }
            set { tintColor = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value % MathHelper.TwoPi; }
        }

        public int Frame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0, frames.Count - 1);
            }
        }

        public float FrameTime
        {
            get { return frameTime; }
            set { frameTime = MathHelper.Max(0, value); }
        }

        public Rectangle Source
        {
            get { return frames[currentFrame]; }
        }
        #endregion

        #region Positional Properties
        public Vector2 WorldLocation
        {
            get { return worldLocation; }
            set { worldLocation = value; }
        }

        public Vector2 ScreenLocation
        {
            get
            {
                return Camera.Transform(worldLocation);
            }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Rectangle WorldRectangle
        {
            get
            {
                return new Rectangle(
                    (int)worldLocation.X,
                    (int)worldLocation.Y,
                    FrameWidth,
                    FrameHeight);
            }
        }

        //public Rectangle ScreenRectangle
        //{
        //    get
        //    {
        //        return Camera.Transform(WorldRectangle);
        //    }
        //}

        public Vector2 RelativeCenter
        {
            get { return new Vector2(FrameWidth / 2, FrameHeight / 2); }
        }

        public Vector2 WorldCenter
        {
            get { return worldLocation + RelativeCenter; }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return Camera.Transform(worldLocation + RelativeCenter);
            }
        }
        public bool isVisible
        {
            get
            {
                return (Camera.ObjectIsVisible(WorldRectangle));
            }
        }
        #endregion

        #region Collision Related Properties
        public Rectangle BoundingBoxRect
        {
            get
            {
                return new Rectangle(
                    (int)worldLocation.X + BoundingXPadding,
                    (int)worldLocation.Y + BoundingYPadding,
                    FrameWidth - (BoundingXPadding * 2),
                    FrameHeight - (BoundingYPadding * 2));
            }
        }
        #endregion

        #region Collision Detection Methods
        public bool IsBoxColliding(Rectangle OtherBox)
        {
            if ((Collidable) && (!Expired))
            {
                return BoundingBoxRect.Intersects(OtherBox);
            }
            else
            {
                return false;
            }
        }

        public bool IsCircleColliding(Vector2 otherCenter, float otherRadius)
        {
            if ((Collidable) && (!Expired))
            {
                if (Vector2.Distance(WorldCenter, otherCenter) < (CollisionRadius + otherRadius))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Animation-Related Methods
        public void AddFrame(Rectangle frameRectangle)
        {
            frames.Add(frameRectangle);
        }

        public void RotateTo(Vector2 direction)
        {
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        #endregion

        #region Update and Draw Methods
        public virtual void Update(GameTime gameTime)
        {
            if (FrameTime != 0 && !Expired)
            {
                double elapsed = gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
                timeForCurrentFrame += elapsed;

                if (Animate)
                {
                    while (timeForCurrentFrame >= FrameTime)
                    {
                        if ((AnimateWhenStopped) || (velocity != Vector2.Zero))
                        {
                            if (Reanimate || (!Reanimate && currentFrame != frames.Count - 1))
                            {
                                currentFrame = (currentFrame + 1) % (frames.Count);
                                timeForCurrentFrame -= FrameTime;
                            }
                            else if (!Reanimate)
                            {
                                currentFrame = frames.Count - 1;
                                timeForCurrentFrame = 0;
                            }
                        }
                    }
                }

                worldLocation += (velocity * (float)elapsed);
            }
        }

        public virtual void Draw()
        {
            if (!Expired)
            {
                if (Camera.ObjectIsVisible(WorldRectangle))
                {
                    Game1.spriteBatch.Draw(
                        Texture,
                        ScreenCenter,
                        Source,
                        tintColor,
                        rotation,
                        RelativeCenter,
                        scale * Game1.resolutionScale,
                        SpriteEffects.None,
                        0.0f);
                }
            }
        }
        #endregion
    }

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