using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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
        Camera
    }

    public class AActor
    {
        protected SceneComponent rootComponent; //SceneComponents; no list is driectly stored
        public SceneComponent RootComponent { get { return rootComponent; } }
        protected List<Component> components = new List<Component>(); //Other components that are not SceneComponent
        public List<Component> Components { get { return components; } }

        private ActorUpdateGroup updateGroup = ActorUpdateGroup.Default;
        public bool isUpdateEnabled = true;
        public bool isComponentsUpdateEnabled = true;
        public bool isVisible = true;
        private bool pendingDestroyed = false;

        public AActor()
        {
            rootComponent = new SceneComponent(this);
            StateManager.currentState.AddActor(this);
        }
        public AActor(SceneComponent rootComponent)
        {
            this.rootComponent = rootComponent;
            this.rootComponent.SetOwner(this);
            StateManager.currentState.AddActor(this);
        }

        public bool Destroy()
        {
            if (!pendingDestroyed)
            {
                pendingDestroyed = true;
                if (StateManager.currentState.CanRemoveActor(this))
                {
                    //Destroy all components in this actor and all components that have this actor as owner
                    if (rootComponent != null) //Should never happen
                    {
                        rootComponent.Destroy(true);
                    }
                    List<SceneComponent> sceneComponents = StateManager.currentState.GetAllSceneComponents();
                    foreach (SceneComponent sceneComponent in sceneComponents)
                    {
                        if (sceneComponent.Owner == this)
                        {
                            sceneComponent.Destroy(true);
                        }
                    }
                    foreach (Component component in components)
                    {
                        component.Destroy();
                    }
                    components = null;
                    StateManager.currentState.RemoveActor(this);
                    return true;
                }
                return false;
            }
            return true;
        }

        public List<SceneComponent> GetSceneComponents()
        {
            List<SceneComponent> sceneComponents = new List<SceneComponent>();
            if (rootComponent != null)
            {
                rootComponent.GetAllDescendantsAndSelf(ref sceneComponents);
            }
            return sceneComponents;
        }
        public List<T> GetSceneComponents<T>() where T : SceneComponent
        {
            List<T> sceneComponents = new List<T>();
            if (rootComponent != null)
            {
                rootComponent.GetAllDescendantsAndSelf<T>(ref sceneComponents);
            }
            return sceneComponents;
        }
        public void GetSceneComponents(ref List<SceneComponent> sceneComponents)
        {
            if (rootComponent != null)
            {
                rootComponent.GetAllDescendantsAndSelf(ref sceneComponents);
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
            if (!pendingDestroyed)
            {
                return components.Remove(component);
            }
            return false;
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
        public bool RemoveRootComponent() //Destroy the actor
        {
            if (rootComponent != null)
            {
                rootComponent = null;
                return true;
            }
            return false;
        }
        public bool AttachSceneComponent(SceneComponent component)
        {
            return component.AttachToActor(this, true);
        }

        public float GetMass()
        {
            return rootComponent.GetTotalMass();
        }

        public Vector2 GetMassCenter()
        {
            return rootComponent.GetMassCenter();
        }

        public Vector2 WorldLocation { get { return rootComponent.relativeLocation; } set { rootComponent.relativeLocation = value; } }
        public float WorldRotation { get { return rootComponent.relativeRotation; } set { rootComponent.relativeRotation = value; } }
        public float WorldScale { get { return rootComponent.relativeScale; } set { rootComponent.relativeScale = value; } }

        public void Update(GameTime gameTime, ActorUpdateGroup updateGroup)
        {
            if (this.updateGroup == updateGroup)
            {
                UpdateComponents(gameTime, ComponentUpdateGroup.BeforeActor);
                if (isUpdateEnabled)
                {
                    UpdateActor(gameTime);
                }
                UpdateComponents(gameTime, ComponentUpdateGroup.AfterActor);
            }
        }
        protected virtual void UpdateActor(GameTime gameTime) { }
        protected void UpdateComponents(GameTime gameTime, ComponentUpdateGroup updateGroup)
        {
            if (isComponentsUpdateEnabled)
            {
                foreach (Component component in components)
                {
                    component.Update(gameTime, updateGroup);
                }

                if (rootComponent != null)
                {
                    rootComponent.UpdateDescendantsAndSelf(gameTime, updateGroup);
                }
            }
        }

        public virtual void Draw()
        {
            if (isVisible && rootComponent != null)
            {
                rootComponent.DrawDescendantsAndSelf();
            }
        }
    }

    public class ASprite : AActor
    {
        protected SpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public SpriteComponent SpriteComponent { get { return spriteComponent; } }
        public ASprite(Texture2D texture, List<Rectangle> frames,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
        {
            spriteComponent = new SpriteComponent(this, texture, frames, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
            rootComponent = spriteComponent;
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            //GameManager.grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            GameManager.grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * rootComponent.velocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class AAnimatedSprite : AActor
    {
        protected AnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public AnimatedSpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AAnimatedSprite(Texture2D texture, List<Rectangle> frames,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
        {
            spriteComponent = new AnimatedSpriteComponent(this, texture, frames, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
            rootComponent = spriteComponent;
            spriteComponent.GenerateDefaultAnimation();
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            //GameManager.grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            GameManager.grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * rootComponent.velocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class ATemporarySprite : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public ATemporarySprite(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }

    public class APowerUp : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public APowerUp(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }

    public class AMine : AActor
    {
        protected AnimatedSpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public AnimatedSpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AMine(Texture2D texture, List<Rectangle> frames,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
        {
            spriteComponent = new AnimatedSpriteComponent(this, texture, frames, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor);
            rootComponent = spriteComponent;
            spriteComponent.GenerateDefaultAnimation();
        }

        protected override void UpdateActor(GameTime gameTime)
        {
            //GameManager.grid.Influence(rootComponent.WorldCenter, gravity);
            //Get Gravity Centre by summing up all SceneComponents Weights
            GameManager.grid.Influence(rootComponent.WorldLocation, 300F * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * rootComponent.velocity.Length()); //Should be WorldCenter, not location
        }
    }

    public class AProjectile : AActor
    {
        protected TemporarySpriteComponent spriteComponent; //SceneComponents. No list is stored directly.
        public TemporarySpriteComponent SpriteComponent { get { return spriteComponent; } }
        public AProjectile(Texture2D texture, List<Rectangle> frames,
            float remainingDuration, float startFlashingAtRemainingTime,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color initialColor = new Color(), Color finalColor = new Color())
        {
            spriteComponent = new TemporarySpriteComponent(this, texture, frames, remainingDuration, startFlashingAtRemainingTime, null, updateGroup, drawGroup, location, isLocationWorld, relativeScale, acceleration, maxSpeed, initialColor, finalColor);
            rootComponent = spriteComponent;
        }
    }
}
