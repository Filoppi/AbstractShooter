using AbstractShooter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AbstractShooter
{
    public enum ActorUpdateGroup
    {
        Background,
        Default,
        PassiveObjects,
        Characters,
        Players,
        Weapons,
        Particles,
        PostPhysics,
        Camera,
        MAX
    }

    public abstract class AActor
    {
        protected SceneComponent rootComponent; //SceneComponents; no list is driectly stored
        public SceneComponent RootComponent { get { return rootComponent; } }
        protected List<Component> components = new List<Component>(); //Other components that are not SceneComponent
        public List<Component> Components { get { return components; } }

        private ActorUpdateGroup updateGroup = ActorUpdateGroup.Default;
        public ActorUpdateGroup UpdateGroup { get { return updateGroup; } }
        public bool isActorUpdateEnabled = true;
        public bool isComponentsUpdateEnabled = true;
        public bool isVisible = true;
        private bool pendingDestroy;
        public bool PendingDestroy { get { return pendingDestroy; } }

        public AActor(ActorUpdateGroup updateGroup = ActorUpdateGroup.Default)
        {
            this.updateGroup = updateGroup;
            StateManager.currentState.RegisterActor(this);
        }
        public AActor(ref SceneComponent rootComponent, ActorUpdateGroup updateGroup = ActorUpdateGroup.Default)
        {
            this.updateGroup = updateGroup;
            this.rootComponent = rootComponent;
            this.rootComponent.SetOwner(this);
            StateManager.currentState.RegisterActor(this);
        }

        public bool Destroy()
        {
            if (!pendingDestroy)
            {
                pendingDestroy = true;
                StateManager.currentState.UnregisterActor(this);

                //Destroy root and all its child components, even if they have a different owner
                if (rootComponent != null) //Should never happen
                {
                    rootComponent.Destroy(true);
                }
                else
                {
                    throw new System.ArgumentException("rootComponent is null");
                }

                //Destroy all left components of the level that have this actor as owner
                List<SceneComponent> sceneComponents = StateManager.currentState.GetAllSceneComponents();
                foreach (SceneComponent sceneComponent in sceneComponents)
                {
                    if (sceneComponent.Owner == this)
                    {
                        sceneComponent.Destroy(false);
                    }
                }

                //Destroy all logical components
                foreach (Component component in components.ToList())
                {
                    component.Destroy();
                }

                return true;
            }
            return false;
        }

        public List<SceneComponent> GetSceneComponents()
        {
            List<SceneComponent> sceneComponents = new List<SceneComponent>();
            rootComponent?.GetAllDescendantsAndSelf(ref sceneComponents);
            return sceneComponents;
        }
        public List<T> GetSceneComponentsByClass<T>() where T : SceneComponent
        {
            List<T> sceneComponents = new List<T>();
            rootComponent?.GetAllDescendantsAndSelf<T>(ref sceneComponents);
            return sceneComponents;
        }
        public void GetSceneComponents(ref List<SceneComponent> sceneComponents)
        {
            if (rootComponent != null)
            {
                rootComponent.GetAllDescendantsAndSelf(ref sceneComponents);
            }
            else
            {
                throw new System.ArgumentException("rootComponent is null");
            }
        }
        public bool ContainsComponent(Component component)
        {
            return components.Contains(component);
        }
        public bool ContainsSceneComponent(SceneComponent sceneComponent)
        {
            if (sceneComponent != null)
            {
                return GetSceneComponents().Contains(sceneComponent);
            }
            return false;
        }
        public void AddComponent(Component component)
        {
            if (!components.Contains(component))
            {
                components.Add(component);
            }
        }

        public bool RemoveComponent(Component component)
        {
            return components.Remove(component);
        }
        public bool RemoveSceneComponent(SceneComponent sceneComponent)
        {
            if (sceneComponent != null)
            {
                if (rootComponent == sceneComponent)
                {
                    rootComponent = null;
                    return true;
                }
            }
            return false;
        }
        public bool RemoveRootComponent()
        {
            if (rootComponent != null)
            {
                rootComponent = null;
                return true;
            }
            return false;
        }
        public bool SetRootComponent(SceneComponent component)
        {
            if (rootComponent == null && component != null)
            {
                rootComponent = component;
                component.SetOwner(this);
                return true;
            }
            return false;
        }
        public bool AttachSceneComponent(SceneComponent component)
        {
            return component.AttachToActor(this, true);
        }

        public List<AActor> GetCollidingActors()
        {
            List<AActor> actors = new List<AActor>();
            foreach (SceneComponent sceneComponent in GetSceneComponents())
            {
                actors.AddRange(sceneComponent.overlappingActors);
            }
            return actors.Distinct().ToList();
        }
        public List<AActor> GetUpdatedCollidingActors()
        {
            //Recalculate collisions
            return GetCollidingActors();
        }

        public float GetMass()
        {
            return rootComponent.GetTotalMass();
        }

        public Vector2 GetMassCenter()
        {
            return rootComponent.GetMassCenter();
        }

        public Vector2 WorldLocation { get { return rootComponent.WorldLocation; } set { rootComponent.WorldLocation = value; } }
        public float WorldRotation { get { return rootComponent.WorldRotation; } set { rootComponent.WorldRotation = value; } }
        public float WorldScale { get { return rootComponent.WorldScale; } set { rootComponent.WorldScale = value; } }

        public void Update(GameTime gameTime)
        {
            UpdateComponents(gameTime, ComponentUpdateGroup.BeforeActor);
            if (isActorUpdateEnabled)
            {
                UpdateActor(gameTime);
            }
            UpdateComponents(gameTime, ComponentUpdateGroup.AfterActor);
        }
        protected virtual void UpdateActor(GameTime gameTime) { }
        public void UpdateComponents(GameTime gameTime, ComponentUpdateGroup updateGroup)
        {
            if (isComponentsUpdateEnabled)
            {
                foreach (Component component in components.ToList())
                {
                    component.Update(gameTime, updateGroup);
                }

                if (rootComponent != null)
                {
                    rootComponent.UpdateDescendantsAndSelf(gameTime, updateGroup);
                }
                else
                {
                    throw new System.ArgumentException("rootComponent is null");
                }
            }
        }

        public virtual void Draw()
        {
            if (isVisible)
            {
                rootComponent?.DrawDescendantsAndSelf();
            }
        }
    }

    public class AScene : AActor
    {
        public AScene()
        {
            rootComponent = new SceneComponent(this);
        }
    }

    public class ASprite : AActor
    {
        protected SpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public SpriteComponent SpriteComponent { get { return spriteComponent; } }
        public ASprite(Texture2D texture, List<Rectangle> frames,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new SpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
            rootComponent = spriteComponent;
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            //((Level)StateManager.currentState).grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            ((Level)StateManager.currentState).grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * rootComponent.localVelocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class AAnimatedSprite : AActor
    {
        protected AnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public AnimatedSpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AAnimatedSprite(Texture2D texture, List<Rectangle> frames,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new AnimatedSpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
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

    public class ATemporarySprite : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public ATemporarySprite(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Default,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }

    public class APowerUp : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public APowerUp(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Weapons,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }

    public class AMine : AActor
    {
        protected AnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public AnimatedSpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AMine(Texture2D texture, List<Rectangle> frames,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Weapons,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new AnimatedSpriteComponent(this, texture, frames, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
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

    public class AProjectile : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AProjectile(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ActorUpdateGroup actorUpdateGroup = ActorUpdateGroup.Weapons,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.BeforeActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
            : base(actorUpdateGroup)
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }
}
