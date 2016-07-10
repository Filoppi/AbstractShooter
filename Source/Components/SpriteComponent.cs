using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public class CSpriteComponent : CSceneComponent
    {
        private Texture2D texture;

        protected List<Rectangle> frames = new List<Rectangle>();
        protected List<float> framesCollisionRadius = new List<float>();

        protected Color tintColor = Color.White;
        
        protected int currentFrame = 0;

        public virtual int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = (int)MathHelper.Clamp(value, 0, frames.Count - 1); }
        }

        public override float CollisionRadius
        {
            get
            {
                return WorldScale * collisionRadiusMultiplier * (framesCollisionRadius.Count > 0 ? framesCollisionRadius[currentFrame] : 0);
            }
        }

        //public int BoundingXPadding = 0;
        //public int BoundingYPadding = 0;

        private const float collisionsTransparencyAlphaTreshold = 0.5F; //Temp
        
        public CSpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            CSceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color? tintColor = null)
            : base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
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

            Color[] texturePixelsColor = new Color[texture.Width * texture.Height];
            texture.GetData(texturePixelsColor);
            for (int k = 0; k < frames.Count; k++)
            {
                float frameRadiusSquared = 0;
                Vector2 center = new Vector2((frames[k].Width / 2F ), (frames[k].Height / 2F ));
                //Color[,] texturePixelsColor2D = new Color[frames[k].Width, frames[k].Height];
                for (int x = 0; x < frames[k].Width; x++)
                {
                    for (int y = 0; y < frames[k].Height; y++)
                    {
                        int textureX = x + frames[k].Left;
                        int textureY = y + frames[k].Top;
                        //texturePixelsColor2D[x, y] = texturePixelsColor[textureX + textureY * texture.Width];
                        if (texturePixelsColor[textureX + textureY * texture.Width].A >= collisionsTransparencyAlphaTreshold * 255)
                        {
                            float distanceSquared = Vector2.DistanceSquared(new Vector2(x + ((float)x / (frames[k].Width - 1)), y + ((float)y / (frames[k].Height - 1))), center);
                            if (distanceSquared >= frameRadiusSquared)
                            {
                                frameRadiusSquared = distanceSquared;
                            }
                        }
                    }
                }
                framesCollisionRadius.Add((float)Math.Sqrt(frameRadiusSquared));
            }
        }

        public int FrameWidth { get { return frames[0].Width; } }
        public int FrameHeight { get { return frames[0].Height; } }

        public Color TintColor { get { return tintColor; } }

        public virtual Rectangle CurrentFrameRectangle { get { return frames[currentFrame]; } }

        ///Imprecise. Should not be used
        public Rectangle WorldRectangle
        {
            get
            {
                return new Rectangle(
                    (int)(WorldLocation.X - (WorldScale * (float)FrameWidth / 2F)), //Is the Float division needed?
                    (int)(WorldLocation.Y - (WorldScale * (float)FrameHeight / 2F)),
                    (int)(WorldScale * FrameWidth),
                    (int)(WorldScale * FrameHeight));
            }
        }
        ///Current Frame Bounding Box
        public Rectangle BoundingBox
        {
            get
            {
                return WorldRectangle.FitRotation(worldRotation);
            }
        }
        public Rectangle ApproximatedBoundingBox
        {
            get
            {
                float curXSize = (FrameWidth / 2F) + ((CollisionRadius - (FrameWidth / 2F)) * (Math.Abs(WorldRotation.ToDegrees()).DistanceFrom45()));
                float curYSize = (FrameHeight / 2F) + ((CollisionRadius - (FrameHeight / 2F)) * (Math.Abs(WorldRotation.ToDegrees()).DistanceFrom45()));
                return new Rectangle(
                    (WorldLocation.X - curXSize).SymmetricCeiling(),
                    (WorldLocation.Y - curYSize).SymmetricCeiling(),
                    (int)Math.Ceiling(curXSize * 2F),
                    (int)Math.Ceiling(curYSize * 2F));
            }
        }
        ///Current Frame Max Bounding Box
        public Rectangle MaxBoundingBox
        {
            get
            {
                return new Rectangle(
                    (WorldLocation.X - CollisionRadius).SymmetricCeiling(),
                    (WorldLocation.Y - CollisionRadius).SymmetricCeiling(),
                    (int)Math.Ceiling(CollisionRadius * 2F),
                    (int)Math.Ceiling(CollisionRadius * 2F));
            }
        }
        //public Rectangle BoundingBoxRect
        //{
        //    get
        //    {
        //        return new Rectangle(
        //            (int)WorldLocation.X + BoundingXPadding,
        //            (int)WorldLocation.Y + BoundingYPadding,
        //            FrameWidth - (BoundingXPadding * 2),
        //            FrameHeight - (BoundingYPadding * 2));
        //    }
        //}

        ///Only use if CSceneComponent is not rotated?
        public Vector2 TopLeftLocation
        {
            get { return new Vector2(WorldLocation.X - (FrameWidth/2F), (WorldLocation.Y - (FrameHeight/2F))); }
            set { WorldLocation = new Vector2(value.X + (FrameWidth/2F), value.Y + (FrameHeight/2F)); }
        }

        //public Rectangle WorldRectangleRotated //To Do

        //public Rectangle ScreenRectangle
        //{
        //    get { return Camera.Transform(WorldRectangle); }
        //}

        private Vector2 RelativeCenter
        {
            get { return new Vector2(FrameWidth / 2, FrameHeight / 2); }
        }
        
        public override bool IsInViewport { get { return(Camera.IsRectangleVisible(BoundingBox)); } }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            double elapsed = gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            Animate(elapsed);
        }

        public virtual void Animate(double elapsed) { }
        
        protected override void Draw()
        {
            if (IsInViewport)
            {
                Game1.spriteBatch.Draw(
                    texture,
                    ScreenCenter,
                    CurrentFrameRectangle,
                    tintColor,
                    WorldRotation,
                    RelativeCenter,
                    WorldScale * Camera.DrawScale,
                    SpriteEffects.None,
                    LayerDepth);
            }
#if DEBUG
            if (Game1.debugCollisions) //Show Wireframe Collision Boxes (Also select wireframe color???)
            {
                Game1.spriteBatch.DrawCircle(Camera.WorldToScreenSpace(WorldLocation), CollisionRadius*2, 16, Color.White, 1, DrawGroup.DebugGraphics);
                
                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(new Vector2(BoundingBox.Left, BoundingBox.Top)), Camera.WorldToScreenSpace(new Vector2(BoundingBox.Right, BoundingBox.Top)), Color.White, 1, DrawGroup.DebugGraphics);
                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(new Vector2(BoundingBox.Left, BoundingBox.Bottom)), Camera.WorldToScreenSpace(new Vector2(BoundingBox.Right, BoundingBox.Bottom)), Color.White, 1, DrawGroup.DebugGraphics);
                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(new Vector2(BoundingBox.Left, BoundingBox.Top)), Camera.WorldToScreenSpace(new Vector2(BoundingBox.Left, BoundingBox.Bottom)), Color.White, 1, DrawGroup.DebugGraphics);
                Game1.spriteBatch.DrawLine(Camera.WorldToScreenSpace(new Vector2(BoundingBox.Right, BoundingBox.Top)), Camera.WorldToScreenSpace(new Vector2(BoundingBox.Right, BoundingBox.Bottom)), Color.White, 1, DrawGroup.DebugGraphics);
            }
#endif
        }
    }

    public class CAnimatedSpriteComponent : CSpriteComponent
    {
        public CAnimatedSpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            CSceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color()) :
            base(owner, texture, frames, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
        { }

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

    public class CTemporarySpriteComponent : CSpriteComponent
    {
        protected float initialDuration;
        protected float remainingDuration;
        protected Color initialColor;
        protected Color finalColor;
        protected float startFlashingAtRemainingTime;
        
        public CTemporarySpriteComponent(AActor owner,
            Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            CSceneComponent parentSceneComponent = null,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color? initialColor = null, Color finalColor = default(Color)) :
            base(owner, texture, frames, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor.Value)
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
                    tintColor = Color.Lerp(
                        initialColor,
                        finalColor,
                        DurationProgress);
                }
                remainingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds * 100F * StateManager.currentState.TimeScale;

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
}