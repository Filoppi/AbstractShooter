using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public class SpriteComponent : SceneComponent
    {
        private Texture2D texture;

        protected List<Rectangle> frames = new List<Rectangle>();

        private Color tintColor = Color.White;

        private float localRotation = 0.0f;
        
        protected Int32 currentFrame = 0;

        public virtual Int32 CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = (int)MathHelper.Clamp(value, 0, frames.Count - 1); }
        }

        public bool Collidable = true;
        private float collisionRadiusMultiplier = 1F;
        public float CollisionRadius
        {
            get
            {
                return collisionRadiusMultiplier * WorldScale * FrameWidth / 2F;
            }
        }
        public float CollisionRadiusMultiplier
        {
            get
            {
                return collisionRadiusMultiplier;
            }
            set
            {
                collisionRadiusMultiplier = Math.Max(0, value);
            }
        }
        public int BoundingXPadding = 0;
        public int BoundingYPadding = 0;
        
        public SpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            SceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color? tintColor = null) :
            base(owner, parentSceneComponent, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
        {
            Color foundTintColor;
            if (tintColor.HasValue && tintColor.Value != default(Color))
            {
                foundTintColor = tintColor.Value;
            }
            else
            {
                foundTintColor = Color.White;
            }
            this.texture = texture;
            this.tintColor = foundTintColor;
            this.frames = frames;
        }

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

        public float LocalRotation
        {
            get { return localRotation; }
            set { localRotation = value % MathHelper.TwoPi; }
        }

        public virtual Rectangle CurrentFrameRectangle
        {
            get { return frames[currentFrame]; }
        }

        public Vector2 ScreenLocation
        {
            get
            {
                return Camera.Transform(WorldLocation);
            }
        }

        public Rectangle WorldRectangle
        {
            get
            {
                return new Rectangle(
                    (int)WorldLocation.X,
                    (int)WorldLocation.Y,
                    FrameWidth,
                    FrameHeight);
            }
        }

        //public Rectangle ScreenRectangle
        //{
        //    get { return Camera.Transform(WorldRectangle); }
        //}

        public Vector2 RelativeCenter
        {
            get { return new Vector2(FrameWidth / 2, FrameHeight / 2); }
        }

        public Vector2 WorldCenter
        {
            get { return WorldLocation + RelativeCenter; }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return Camera.Transform(WorldLocation + RelativeCenter);
            }
        }
        public bool IsInViewport
        {
            get
            {
                return(Camera.ObjectIsVisible(WorldRectangle));
            }
        }

        public Rectangle BoundingBoxRect
        {
            get
            {
                return new Rectangle(
                    (int)WorldLocation.X + BoundingXPadding,
                    (int)WorldLocation.Y + BoundingYPadding,
                    FrameWidth - (BoundingXPadding * 2),
                    FrameHeight - (BoundingYPadding * 2));
            }
        }

        public bool IsBoxColliding(Rectangle OtherBox)
        {
            if (Collidable)
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
            if (Collidable)
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

        public void RotateTo(Vector2 direction)
        {
            localRotation = (float)Math.Atan2(direction.Y, direction.X);
        }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            double elapsed = gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
            Animate(elapsed);
        }

        public virtual void Animate(double elapsed) { }
        
        protected override void Draw()
        {
            if (Camera.ObjectIsVisible(WorldRectangle))
            {
                Game1.spriteBatch.Draw(
                    texture,
                    ScreenCenter,
                    CurrentFrameRectangle,
                    tintColor,
                    localRotation,
                    RelativeCenter,
                    WorldScale * Game1.resolutionScale,
                    SpriteEffects.None,
                    0);
            }
        }
    }

    public class AnimatedSpriteComponent : SpriteComponent
    {
        public struct Animation
        {
            public List<Int32> framesIndex;
            public float frameTime;
            public Animation(ref List<Int32> newFramesIndex, float newframeTime = 0.1f)
            {
                frameTime = newframeTime;
                framesIndex = newFramesIndex;
            }
        }
        
        public AnimatedSpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            SceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color()) :
            base(owner, texture, frames, parentSceneComponent, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
        { }

        private List<Animation> animations = new List<Animation>();

        private Int32 currentAnimation = 0;
        private Int32 currentAnimationFrameIndex = 0;
        private double timeForCurrentFrame = 0.0f;

        public bool Animated = true;
        //public bool AnimateWhenSteady = true;
        public bool loop = true; //Re-Animate: Means that it should keep animating after the first cycle

        //public void SetFrameTime(Int32 animationIndex, float value)
        //{
        //   animations[animationIndex].frameTime = MathHelper.Max(0, value);
        //}

        public void GenerateDefaultAnimation(float newAnimationTime = 0.1F, bool setAsCurrent = false)
        {
            List<Int32> newAnimation = new List<Int32>();
            for (Int32 i = 0; i < frames.Count; ++i)
            {
                newAnimation.Add(i);
            }
            animations.Add(new Animation(ref newAnimation, newAnimationTime));
            if (setAsCurrent)
            {
                currentAnimation = animations.Count - 1;
            }
        }
        public void AddAnimation(ref List<Int32> newFramesIndex, float newAnimationTime)
        {
            animations.Add(new Animation(ref newFramesIndex, newAnimationTime));
        }
        public void SetAnimation(Int32 index)
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

    public class TemporarySpriteComponent : SpriteComponent
    {
        protected float initialDuration;
        protected float remainingDuration;
        protected Color initialColor;
        protected Color finalColor;
        protected float startFlashingAtRemainingTime = 0;
        
        public TemporarySpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            SceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color? initialColor = null, Color finalColor = default(Color)) :
            base(owner, texture, frames, parentSceneComponent, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor.Value)
        {
            Color foundInitialColor;
            if (initialColor.HasValue && initialColor.Value != default(Color))
            {
                foundInitialColor = initialColor.Value;
            }
            else
            {
                foundInitialColor = Color.White;
            }
            //Color foundFinalColor;
            //if (finalColor.HasValue && finalColor.Value != default(Color))
            //{
            //    foundFinalColor = finalColor.Value;
            //}
            //else
            //{
            //    foundFinalColor = Color.TransparentBlack;
            //}
            initialDuration = remainingDuration;
            this.remainingDuration = remainingDuration;
            this.startFlashingAtRemainingTime = startFlashingAtRemainingTime;
            this.initialColor = foundInitialColor;
            this.finalColor = finalColor;
            this.finalColor.A = 0;
        }

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

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            if (IsActive)
            {
                if (initialColor != finalColor)
                {
                    TintColor = Color.Lerp(
                        initialColor,
                        finalColor,
                        DurationProgress);
                }
                remainingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * GameManager.TimeScale;

                if (remainingDuration < startFlashingAtRemainingTime)
                {
                    isVisible = System.Decimal.Remainder(System.Decimal.Remainder((Decimal)remainingDuration, 30M), 2M) < 1M;
                }
            }
            else
            {
                Destroy(false);
            }
        }
    }

    public class ParticleComponent : TemporarySpriteComponent
    {
        public ParticleComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            SceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 velocity = new Vector2(), Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color()) :
            base(owner, texture, frames, remainingDuration, startFlashingAtRemainingTime, parentSceneComponent, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor)
        {
            this.velocity = velocity;
        }
    }
}