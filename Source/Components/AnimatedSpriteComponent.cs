using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AbstractShooter
{
	public struct Animation
	{
		public List<int> framesIndex;
		public float frameTime;

		public Animation(ref List<int> newFramesIndex, float newframeTime = 0.1f)
		{
			frameTime = newframeTime;
			framesIndex = newFramesIndex;
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
}