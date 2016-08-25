using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AbstractShooter
{
	public class Particle
	{
		public Texture2D Texture;

		private Vector2 worldLocation = Vector2.Zero;
		private Vector2 velocity = Vector2.Zero;

		protected List<Rectangle> frames = new List<Rectangle>();
		protected int currentFrame = 0;

		public virtual int CurrentFrame
		{
			get { return currentFrame; }
			set { currentFrame = (int)MathHelper.Clamp(value, 0, frames.Count - 1); }
		}

		public virtual Rectangle CurrentFrameRectangle { get { return frames[currentFrame]; } }

		private Color tintColor = Color.White;

		private float rotation = 0.0f;
		public float scale = 1.0f;

		public bool Expired = false;

		public int BoundingXPadding = 0;
		public int BoundingYPadding = 0;

		public float drawDepth;

		private Vector2 acceleration;
		private float maxSpeed;
		private float initialDuration;
		protected float remainingDuration;
		private Color initialColor;
		private Color finalColor;

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
				return remainingDuration > 0;
			}
		}

		public Particle(Vector2 worldLocation, float newScale, Texture2D texture, List<Rectangle> frames, Vector2 velocity, float duration, bool foreground = false)
		{
			this.worldLocation = worldLocation;
			scale = newScale;
			Texture = texture;
			this.velocity = velocity;

			this.frames = frames;

			initialDuration = duration;
			remainingDuration = duration;
			this.acceleration = Vector2.Zero;
			this.maxSpeed = 0;

			if (foreground)
			{
				drawDepth = DrawGroup.ForegroundParticles;
			}
			else
			{
				drawDepth = DrawGroup.BackgroundParticles;
			}
		}

		public Particle(Vector2 location, Texture2D texture, List<Rectangle> frames, Vector2 velocity, Vector2 acceleration, float maxSpeed, float duration, Color initialColor, Color finalColor, bool foreground = false)
		{
			this.worldLocation = location;
			scale = 1F;
			Texture = texture;
			this.velocity = velocity;

			this.frames = frames;

			initialDuration = duration;
			remainingDuration = duration;
			this.acceleration = acceleration;
			this.maxSpeed = maxSpeed;
			this.initialColor = initialColor;
			this.finalColor = finalColor;
			this.finalColor.A = 0;

			if (foreground)
			{
				drawDepth = DrawGroup.ForegroundParticles;
			}
			else
			{
				drawDepth = DrawGroup.BackgroundParticles;
			}
		}

		public int FrameWidth
		{
			get { return frames[0].Width; }
		}

		public int FrameHeight
		{
			get { return frames[0].Height; }
		}

		public float Rotation
		{
			get { return rotation; }
			set { rotation = value % MathHelper.TwoPi; }
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
					(int)(worldLocation.X - FrameWidth / 2F), //Is the Float division needed?
					(int)(worldLocation.Y - FrameHeight / 2F),
					(int)FrameWidth,
					(int)FrameHeight);
			}
		}

		public Rectangle BoundingBox
		{
			get
			{
				return WorldRectangle.FitRotation(rotation);
			}
		}

		public Vector2 RelativeCenter
		{
			get { return new Vector2(FrameWidth / 2, FrameHeight / 2); }
		}

		public Vector2 ScreenCenter
		{
			get { return Camera.WorldToScreenSpace(worldLocation); }
		}

		public virtual void Animate(double elapsed)
		{
		}

		public virtual void Update(GameTime gameTime)
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
					tintColor = Color.Lerp(
						initialColor,
						finalColor,
						DurationProgress);
				}
				remainingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;

				double elapsed = gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
				worldLocation += velocity * (float)elapsed;

				Animate(elapsed);
			}
			else
			{
				Expired = true;
			}
		}

		public virtual void Draw()
		{
			if (IsActive && !Expired)
			{
				if (Camera.IsRectangleVisible(WorldRectangle))
				{
					Game1.spriteBatch.Draw(
						Texture,
						ScreenCenter,
						CurrentFrameRectangle,
						tintColor,
						rotation,
						RelativeCenter,
						scale * Camera.DrawScale,
						SpriteEffects.None,
						drawDepth); //Used as particles...
				}
			}
		}
	}

	//TO1 Assets:
	//public class SmogAnimatedParticle : AnimatedParticle
	//{
	//	public SmogAnimatedParticle(Vector2 worldLocation, float newScale, Texture2D texture, List<Rectangle> frames, Vector2 velocity, float duration, bool foreground = false)
	//		: base(worldLocation, newScale, texture, frames, velocity, duration, foreground)
	//	{
	//		AnimatedParticle smog = new AnimatedParticle(
	//					location,
	//					StateManager.currentState.spriteSheet,
	//					new List<Rectangle> { smogRectangle,
	//				new Rectangle(smogRectangle.X + smogRectangle.Width, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height),
	//				new Rectangle(smogRectangle.X + smogRectangle.Width * 2, smogRectangle.Y, smogRectangle.Width, smogRectangle.Height) },
	//					Vector2.Zero,
	//					Vector2.Zero,
	//					0,
	//					108,
	//					Color.White,
	//					Color.Black,
	//					false);
	//		smog.Rotation = (float)Math.Atan2(direction.Y, direction.X);
	//		smog.loop = false;
	//		smog.GenerateDefaultAnimation(((float)108 / 1000) * 3);
	//	}

	//	public SmogAnimatedParticle(Vector2 location, Texture2D texture, List<Rectangle> frames, Vector2 velocity, Vector2 acceleration, float maxSpeed, float duration, Color initialColor, Color finalColor, bool foreground = false)
	//			: base(location, texture, frames, velocity, acceleration, maxSpeed, duration, initialColor, finalColor, foreground)
	//	{
	//	}
	//}

	public class AnimatedParticle : Particle
	{
		public AnimatedParticle(Vector2 worldLocation, float newScale, Texture2D texture, List<Rectangle> frames, Vector2 velocity, float duration, bool foreground = false)
			: base(worldLocation, newScale, texture, frames, velocity, duration, foreground)
		{
		}

		public AnimatedParticle(Vector2 location, Texture2D texture, List<Rectangle> frames, Vector2 velocity, Vector2 acceleration, float maxSpeed, float duration, Color initialColor, Color finalColor, bool foreground = false)
			: base(location, texture, frames, velocity, acceleration, maxSpeed, duration, initialColor, finalColor, foreground)
		{
		}

		private List<Animation> animations = new List<Animation>();

		private int currentAnimation = 0;
		private int currentAnimationFrameIndex = 0;
		private double timeForCurrentFrame = 0.0f;

		public bool Animated = true;

		//public bool AnimateWhenSteady = true;
		public bool loop = true; //Re-Animate: Means that it should keep animating after the first cycle

		//public void SetFrameTime(int animationIndex, float value)
		//{
		//   animations[animationIndex].frameTime = MathHelper.Max(0, value);
		//}

		public void GenerateDefaultAnimation(float newAnimationTime = 0.1F, bool setAsCurrent = false)
		{
			List<int> newAnimation = new List<int>();
			for (int i = 0; i < frames.Count; ++i)
			{
				newAnimation.Add(i);
			}
			animations.Add(new Animation(ref newAnimation, newAnimationTime));
			if (setAsCurrent)
			{
				currentAnimation = animations.Count - 1;
			}
		}

		public void AddAnimation(ref List<int> newFramesIndex, float newAnimationTime)
		{
			animations.Add(new Animation(ref newFramesIndex, newAnimationTime));
		}

		public void SetAnimation(int index)
		{
			currentAnimation = index;
			currentFrame = 0;
			timeForCurrentFrame = 0.0f;
			currentAnimationFrameIndex = 0;
		}

		public override void Animate(double elapsed)
		{
			if (Animated && animations.Count > currentAnimation && animations[currentAnimation].frameTime > 0)
			{
				timeForCurrentFrame += elapsed;

				while (timeForCurrentFrame >= animations[currentAnimation].frameTime)
				{
					//if ((AnimateWhenSteady) || (velocity != Vector2.Zero))
					{
						if (!loop && currentFrame == animations[currentAnimation].framesIndex.Count - 1)
						{
							currentFrame = animations[currentAnimation].framesIndex.Last();
							timeForCurrentFrame = 0;
						}
						else
						{
							currentAnimationFrameIndex = (currentAnimationFrameIndex + 1) % animations[currentAnimation].framesIndex.Count;
							currentFrame = animations[currentAnimation].framesIndex[currentAnimationFrameIndex];
							timeForCurrentFrame -= animations[currentAnimation].frameTime;
						}
					}
				}
			}
		}
	}

	/* Old Version
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

        #endregion Declarations

        #region Constructors

        public Sprite(Vector2 worldLocation, float newScale, Texture2D texture, Rectangle initialFrame, Vector2 velocity)
        {
            WorldLocation = worldLocation;
            scale = newScale;
            Texture = texture;
            this.velocity = velocity;

            frames.Add(initialFrame);

            CollisionRadius = newScale * initialFrame.Width / 2.0f;
        }
        public Sprite(Vector2 worldLocation, float newScale, Texture2D texture, Color newColor, Rectangle initialFrame, Vector2 velocity)
        {
            WorldLocation = worldLocation;
            scale = newScale;
            Texture = texture;
            tintColor = newColor;
            this.velocity = velocity;

            frames.Add(initialFrame);

            CollisionRadius = newScale * initialFrame.Width / 2.0f;
        }

        #endregion Constructors

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

        #endregion Drawing and Animation Properties

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
                return (Camera.IsRectangleVisible(WorldRectangle));
            }
        }

        #endregion Positional Properties

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

        #endregion Collision Related Properties

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

        #endregion Collision Detection Methods

        #region Animation-Related Methods

        public void AddFrame(Rectangle frameRectangle)
        {
            frames.Add(frameRectangle);
        }

        public void RotateTo(Vector2 direction)
        {
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }

        #endregion Animation-Related Methods

        #region Update and Draw Methods

        public virtual void Update(GameTime gameTime)
        {
            if (FrameTime != 0 && !Expired)
            {
                double elapsed = gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
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
                if (Camera.IsRectangleVisible(WorldRectangle))
                {
                    Game1.spriteBatch.Draw(
                        Texture,
                        ScreenCenter,
                        Source,
                        tintColor,
                        rotation,
                        RelativeCenter,
                        scale * Camera.DrawScale,
                        SpriteEffects.None,
                        DrawGroup.Particles); //Used as particles...
                }
            }
        }

        #endregion Update and Draw Methods
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
                remainingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;
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
    */
}