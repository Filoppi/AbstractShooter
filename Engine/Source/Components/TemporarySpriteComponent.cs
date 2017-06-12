using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace UnrealMono
{
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