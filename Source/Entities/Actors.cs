using AbstractShooter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AbstractShooter
{
    public class ASceneActor : AActor
    {
        public ASceneActor()
        {
            rootComponent = new CSceneComponent(this);
        }
    }

    public class ASpriteActor : AActor
    {
        protected CSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public CSpriteComponent CSpriteComponent { get { return spriteComponent; } }
        public ASpriteActor(Texture2D texture, List<Rectangle> frames,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new CSpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
            rootComponent = spriteComponent;
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            //((Level)StateManager.currentState).grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            ((Level)StateManager.currentState).grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * rootComponent.localVelocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class AAnimatedSpriteActor : AActor
    {
        protected CAnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public CAnimatedSpriteComponent CSpriteComponent { get { return spriteComponent; } }
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

        protected override void UpdateActor(GameTime gameTime)
        {
            //((Level)StateManager.currentState).grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            ((Level)StateManager.currentState).grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * rootComponent.localVelocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class ATemporarySpriteActor : AActor
    {
        protected CTemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public CTemporarySpriteComponent CSpriteComponent { get { return spriteComponent; } }
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