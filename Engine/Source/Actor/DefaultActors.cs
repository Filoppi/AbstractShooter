using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace UnrealMono
{
	/// <summary>
	/// Base default Actor which only has a SceneComponent.
	/// </summary>
	public class ASceneActor : AActor
	{
		public ASceneActor() //TO1 call base?
		{
			rootComponent = new CSceneComponent(this);
		}
	}

	/// <summary>
	/// An Actor that has a SpriteComponent as RootComponent
	/// </summary>
	public class ASpriteActor : AActor
	{
		protected CSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
		public CSpriteComponent SpriteComponent { get { return spriteComponent; } }

		public ASpriteActor(Texture2D texture, List<Rectangle> frames,
			ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
			: base(actorUpdateGroup)
		{
			spriteComponent = new CSpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
			rootComponent = spriteComponent;
		}
	}

	/// <summary>
	/// An Actor that has a AnimatedSprite as RootComponent
	/// </summary>
	public class AAnimatedSpriteActor : AActor
	{
		protected CAnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
		public CAnimatedSpriteComponent SpriteComponent { get { return spriteComponent; } }

		public AAnimatedSpriteActor(Texture2D texture, List<Rectangle> frames,
			ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
			: base(actorUpdateGroup)
		{
			spriteComponent = new CAnimatedSpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
			rootComponent = spriteComponent;
			spriteComponent.GenerateDefaultAnimation();
		}
	}

	/// <summary>
	/// An Actor that has a TemporarySprite as RootComponent
	/// </summary>
	public class ATemporarySpriteActor : AActor
	{
		protected CTemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
		public CTemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }

		public ATemporarySpriteActor(Texture2D texture, List<Rectangle> frames,
			float remainingDuration, float startFlashingAtRemainingTime,
			ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
			: base(actorUpdateGroup)
		{
			spriteComponent = new CTemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
			rootComponent = spriteComponent;
		}
	}
}