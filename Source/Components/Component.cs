using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AbstractShooter
{
    public delegate void DestroyedEventHandler(Component sender);

    //public enum InputModes
    //{
    //    KeyboardMouse,
    //    Keyboard,
    //    Mouse,
    //    GamePad,
    //    KeyboardMouseGamePad1, //Allows the use of everything together
    //    KeyboardMouseGamePads, //Allows the use of everything together for Multiplayer
    //    None
    //};

    public static class DrawGroup
    {
        public const float FarBackground = 0.05F;
        public const float Background = 0.1F;
        public const float Particles = 0.15F;
        public const float PassiveObjects = 0.2F;
        public const float Default = 0.25F;
        public const float Powerups = 0.3F;
        public const float Mines = 0.35F;
        public const float Characters = 0.4F;
        public const float Shots = 0.45F;
        public const float Players = 0.5F;
        public const float Foreground = 0.55F;
        public const float UI = 0.95F;
    }

    public enum ComponentUpdateGroup
    {
        BeforeActor,
        Custom1,
        Custom2,
        Custom3,
        AfterActor
    }

    [Flags]
    public enum ComponentCollisionGroup
    {
        Static,
        Dynamic,
        Physic,
        Character,
        Weapon,
        Particle,
        MAX
    }

    public abstract class Component
    {
        protected AActor owner;
        public AActor Owner { get { return owner; } }

        public ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor;
        public float layerDepth = DrawGroup.Default;
        public event DestroyedEventHandler Destroyed;
        public bool isUpdateEnabled = true;
        protected bool pendingDestroy;
        public bool PendingDestroy { get { return pendingDestroy; } }

        public Component(AActor owner)
        {
            InternalSetOwner(owner);
        }
        public Component(AActor owner, ComponentUpdateGroup updateGroup, float layerDepth)
        {
            InternalSetOwner(owner);
            this.updateGroup = updateGroup;
            this.layerDepth = layerDepth;
        }

        protected virtual void BroadcastDestroyed()
        {
            Destroyed?.Invoke(this);
        }

        protected bool InternalSetOwner(AActor owner)
        {
            if (owner != null)
            {
                this.owner = owner;
            }
            else
            {
                this.owner = StateManager.currentState.Scene;
            }
            return true;
        }
        /// <summary>
        /// Only works if there are no external strong references to this object
        /// </summary>
        public bool Destroy()
        {
            return InternalDestroy();
        }
        protected virtual bool InternalDestroy()
        {
            if (!pendingDestroy)
            {
                pendingDestroy = true;
                if (owner != null)
                {
                    BroadcastDestroyed();
                    owner.RemoveComponent(this);
                    owner = null;
                    return true;
                }
                return true; //If there is no owner, it was already going to be destroyed
            }
            return false;
        }
        public void Update(GameTime gameTime, ComponentUpdateGroup updateGroup)
        {
            if (isUpdateEnabled && this.updateGroup == updateGroup)
            {
                UpdateComponent(gameTime);
            }
        }
        protected virtual void UpdateComponent(GameTime gameTime) { }
    }

    public class SceneComponent : Component
    {
        protected SceneComponent parent; //Can be a component that has another owner as well
        protected List<SceneComponent> children = new List<SceneComponent>();
        //public List<SceneComponent> Children { get { return children; } }
        public void GetAllDescendantsAndSelf(ref List<SceneComponent> sceneComponents)
        {
            sceneComponents.Add(this);
            foreach (SceneComponent child in children)
            {
                child.GetAllDescendantsAndSelf(ref sceneComponents);
            }
        }
        public void GetAllDescendantsAndSelf<T>(ref List<T> sceneComponents) where T : SceneComponent
        {
            if (this is T)
            {
                sceneComponents.Add((T)this);
            }
            foreach (SceneComponent child in children)
            {
                child.GetAllDescendantsAndSelf<T>(ref sceneComponents);
            }
        }

        //To read only
        public ComponentCollisionGroup CollisionGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Dynamic;
        public List<AActor> overlappingActors = new List<AActor>();
        public List<SceneComponent> overlappingComponents = new List<SceneComponent>();

        /// <summary>
        /// Only works if there are no external strong references to this object
        /// </summary>
        public bool Destroy(bool allDescendants = false)
        {
            return InternalDestroy(allDescendants);
        }
        protected sealed override bool InternalDestroy()
        {
            return InternalDestroy(false);
        }
        private bool InternalDestroy(bool allDescendants = false)
        {
            if (!pendingDestroy)
            {
                pendingDestroy = true;
                BroadcastDestroyed();

                StateManager.currentState.RemoveSceneComponent(this);

                if (owner != null && owner.RootComponent == this) //If it is root component
                {
                    owner.Destroy();
                    //owner.RemoveRootComponent();

                    foreach (SceneComponent sceneComponent in children.ToList())
                    {
                        sceneComponent.InternalDestroy(true);
                    }
                    return true;
                }
                if (parent != null)
                {
                    parent.RemoveChild(this);

                    if (allDescendants)
                    {
                        foreach (SceneComponent sceneComponent in children.ToList())
                        {
                            sceneComponent.InternalDestroy(allDescendants);
                        }
                    }
                    else
                    {
                        foreach (SceneComponent sceneComponent in children)
                        {
                            sceneComponent.AttachToSceneComponent(parent, true);
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        protected Vector2 worldLocation = Vector2.Zero; //Relative to world
        protected Vector2 relativeLocation = Vector2.Zero; //Relative to partents until the root. for root components it is equal to worldLocation
        protected float worldRotation = 0F; //Clockwise //Final rotation you see on the screen
        protected float localRotation = 0F; //Clockwise //Relative to partents until the root. for root components it is equal to worldRotation
        //protected float drawRotation = 0F; //Clockwise //Does not affect children, only rendering
        public float worldScale = 1F;
        public float relativeScale = 1F;
        //protected float drawScale = 1F; //Does not affect children, only rendering
        protected float mass = 0F;
        public Vector2 localVelocity = Vector2.Zero;
        protected Vector2 acceleration = Vector2.Zero;
        protected float maxLocalSpeed = -1F;
        protected float collisionRadiusMultiplier = 1F;
        public bool Collidable = true; //To rename
        public bool isVisible = true;

        public SceneComponent(AActor owner, SceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1) : base(owner, updateGroup, layerDepth)
        {
            StateManager.currentState.AddSceneComponent(this);

            if (parentSceneComponent != null)
            {
                AttachToSceneComponent(parentSceneComponent);
            }
            this.acceleration = acceleration;
            this.maxLocalSpeed = maxSpeed;

            this.relativeScale = relativeScale;
            if (isLocationWorld)
            {
                WorldLocation = location;
            }
            else
            {
                RelativeLocation = location;
            }
        }

        private void AddChild(SceneComponent parent)
        {
            if (!children.Contains(parent))
            {
                children.Add(parent);
            }
        }
        private void RemoveChild(SceneComponent parent)
        {
            children.Remove(parent);
        }
        public int GetAllDescendantsNum(int count = 0)
        {
            foreach (SceneComponent child in children)
            {
                count = child.GetAllDescendantsNum(count);
            }
            return ++count;
        }
        public bool AttachSceneComponent(SceneComponent child, bool keepWorldTransform = true)
        {
            return child.AttachToSceneComponent(this, keepWorldTransform);
        }
        public bool AttachToSceneComponent(SceneComponent parent, bool keepWorldTransform = true) //To add scale and rot
        {
            List<SceneComponent> sceneComponents = new List<SceneComponent>();
            GetAllDescendantsAndSelf(ref sceneComponents);
            if ((owner == null || owner.RootComponent != this) //If this actor doesn't have an owner or if it is not the rootComponent of its owner
                && this.parent != parent
                && (parent == null || !sceneComponents.Contains(parent))) //and if the sceneComponent we are trying to attach this to is not already a child of this
            {
                Vector2 previousWorldLocation = Vector2.Zero;
                if (parent != null)
                {
                    if (keepWorldTransform)
                    {
                        previousWorldLocation = WorldLocation;
                    }
                    if (this.parent != null)
                    {
                        this.parent.RemoveChild(this);
                        this.parent = null;
                    }
                    this.parent = parent;
                    this.parent.AddChild(this);
                    if (keepWorldTransform)
                    {
                        WorldLocation = previousWorldLocation;
                    }
                    return true;
                }
                return AttachToActor(null, keepWorldTransform);
            }
            return false;
        }
        public bool AttachToActor(AActor actor, bool keepWorldTransform = true)
        {
            if (actor == null)
            {
                actor = StateManager.currentState.Scene;
            }
            if ((owner == null || owner.RootComponent != this) //If this actor doesn't have an owner or if it is not the rootComponent of its owner
                && actor != null && GetRootComponentOwner() != actor) //and if the actor we are trying to attach this to is not empty or not already our container
            {
                if (actor.RootComponent == null)
                {
                    Vector2 previousWorldLocation = Vector2.Zero;
                    if (keepWorldTransform)
                    {
                        previousWorldLocation = WorldLocation;
                    }
                    parent.RemoveChild(this);
                    parent = null;
                    actor.SetRootComponent(this);
                    if (keepWorldTransform)
                    {
                        WorldLocation = previousWorldLocation;
                    }
                    return true;
                }
                else
                {
                    return AttachToSceneComponent(actor.RootComponent, keepWorldTransform);
                }
            }
            return false;
        }
        public AActor GetRootComponentOwner()
        {
            if (parent == null)
            {
                return owner;
            }
            else
            {
                return parent.GetRootComponentOwner();
            }
        }
        public SceneComponent GetRootComponent()
        {
            if (parent == null)
            {
                return this;
            }
            else
            {
                return parent.GetRootComponent();
            }
        }
        public bool SetOwner(AActor owner)
        {
            return InternalSetOwner(owner);
        }

        public float GetTotalMass(float mass = 0)
        {
            //To multiply by scale???
            foreach (SceneComponent child in children)
            {
                mass = child.GetTotalMass(mass);
            }
            mass += this.mass;
            return mass;
        }
        public Vector2 GetMassCenter()
        {
            return GetMassCenters() / (float)GetAllDescendantsNum();
        }
        private Vector2 GetMassCenters(Vector2 massCenter = default(Vector2))
        {
            foreach (SceneComponent child in children)
            {
                massCenter = child.GetMassCenters(massCenter);
            }
            massCenter += WorldLocation;
            return massCenter;
        }

        private void UpdateWorldLocation()
        {
            if (parent != null)
            {
                worldLocation = parent.worldLocation + (relativeLocation.Rotate(parent.worldRotation) * parent.worldScale);
            }
            else
            {
                worldLocation = relativeLocation;
            }
            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfLocation();
            }
        }
        private void UpdateRelativeLocation()
        {
            if (this is ParticleComponent)
            {
                float asd = 5;
            }
            if (parent != null)
            {
                relativeLocation = ((worldLocation - parent.worldLocation) / parent.worldScale).Rotate(-parent.worldRotation);
            }
            else
            {
                relativeLocation = worldLocation;
            }
            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfLocation();
            }
        }
        private void UpdateAllDescendantsAndSelfLocation()
        {
            worldLocation = parent.worldLocation + (relativeLocation.Rotate(parent.worldRotation) * parent.worldScale);
            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfLocation();
            }
        }
        public void AddLocalOffset(Vector2 offset)
        {
            RelativeLocation += offset.Rotate(localRotation);
        }
        public Vector2 RelativeLocation
        {
            set
            {
                relativeLocation = value;
                UpdateWorldLocation();
            }
            get
            {
                return relativeLocation;
            }
        }
        public Vector2 WorldLocation
        {
            set
            {
                worldLocation = value;
                UpdateRelativeLocation();
            }
            get
            {
                return worldLocation;
            }
        }

        private void UpdateWorldRotation()
        {
            if (parent != null)
            {
                worldRotation = parent.worldRotation + localRotation;
            }
            else
            {
                worldRotation = localRotation;
            }
            worldRotation %= MathHelper.TwoPi;

            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfRotation();
            }
        }
        private void UpdateLocalRotation()
        {
            if (parent != null)
            {
                localRotation = worldRotation - parent.worldRotation;
            }
            else
            {
                localRotation = worldRotation;
            }
            localRotation %= MathHelper.TwoPi;

            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfRotation();
            }
        }
        private void UpdateAllDescendantsAndSelfRotation()
        {
            worldRotation = (parent.worldRotation + localRotation) % MathHelper.TwoPi;
            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfRotation();
            }
        }
        public float LocalRotation
        {
            set
            {
                localRotation = value % MathHelper.TwoPi;
                UpdateWorldRotation();
            }
            get
            {
                return localRotation;
            }
        }
        public float WorldRotation
        {
            set
            {
                worldRotation = value % MathHelper.TwoPi;
                UpdateLocalRotation();
            }
            get
            {
                return worldRotation;
            }
        }
        
        private void UpdateWorldScale()
        {
            if (parent != null)
            {
                worldScale = parent.worldScale * relativeScale;
            }
            else
            {
                worldScale = relativeScale;
            }

            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfScale();
            }
        }
        private void UpdateRelativeScale()
        {
            if (parent != null)
            {
                relativeScale = worldScale / parent.worldScale;
            }
            else
            {
                relativeScale = worldScale;
            }

            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfScale();
            }
        }
        private void UpdateAllDescendantsAndSelfScale()
        {
            worldScale = parent.worldScale * relativeScale;
            foreach (SceneComponent child in children)
            {
                child.UpdateAllDescendantsAndSelfRotation();
            }
        }
        public float WorldScale
        {
            set
            {
                worldScale = value;
                UpdateRelativeScale();
            }
            get
            {
                return worldScale;
            }
        }
        public float RelativeScale
        {
            set
            {
                relativeScale = value;
                UpdateWorldScale();
            }
            get
            {
                return relativeScale;
            }
        }

        public void AddWorldOffsetAndRotation(Vector2 offset, float angle)
        {
            LocalRotation += angle;
            WorldLocation += offset;
        }
        public void AddRelativeOffsetAndRotation(Vector2 offset, float angle)
        {
            LocalRotation += angle;
            RelativeLocation += offset;
        }
        public void AddLocalOffsetAndRotation(Vector2 offset, float angle)
        {
            LocalRotation += angle;
            AddLocalOffset(offset);
        }
        public void SetRelativeLocationAndRotation(Vector2 location, float angle)
        {
            LocalRotation = angle;
            RelativeLocation = location;
        }
        public void SetWorldLocatioAndRotation(Vector2 location, float angle)
        {
            WorldRotation = angle;
            WorldLocation = location;
        }
        public void SetRelativeLocationRotationAndScale(Vector2 location, float angle, float scale)
        {
            LocalRotation = angle;
            RelativeLocation = location;
            RelativeScale = scale;
        }
        public void SetWorldLocatioRotationAndScale(Vector2 location, float angle, float scale)
        {
            WorldRotation = angle;
            WorldLocation = location;
            WorldScale = scale;
        }

        public void UpdateDescendantsAndSelf(GameTime gameTime, ComponentUpdateGroup updateGroup)
        {
            foreach (SceneComponent child in children.ToList())
            {
                child.UpdateDescendantsAndSelf(gameTime, updateGroup);
            }
            if (isUpdateEnabled && this.updateGroup == updateGroup)
            {
                UpdateComponent(gameTime);
            }
        }
        protected override void UpdateComponent(GameTime gameTime) //Should call base before doing anything else before overriding
        {
            UpdateLocation(gameTime);
        }
        private void UpdateLocation(GameTime gameTime, bool isWorld = true)
        {
            localVelocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * 60F;
            if (maxLocalSpeed >= 0 && localVelocity.LengthSquared() > maxLocalSpeed * maxLocalSpeed)
            {
                localVelocity.Normalize();
                localVelocity *= maxLocalSpeed;
            }
            if (isWorld)
            {
                WorldLocation += localVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }
            else
            {
                RelativeLocation += localVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            }
        }

        public void DrawDescendantsAndSelf()
        {
            foreach (SceneComponent child in children)
            {
                child.DrawDescendantsAndSelf();
            }
            if (isVisible)
            {
                Draw();
            }
        }
        protected virtual void Draw() { }

        public virtual Vector2 ScreenCenter { get { return Camera.Transform(WorldLocation); } }
        public Vector2 ScreenLocation { get { return Camera.Transform(WorldLocation); } }
        public virtual Vector2 WorldCenter { get { return WorldLocation; } }
        public virtual bool IsInViewport { get { return false; } }
        
        public void RotateTo(Vector2 direction, bool worldRotation = true)
        {
            if (worldRotation)
            {
                WorldRotation = (float)Math.Atan2(direction.Y, direction.X);
            }
            else
            {
                LocalRotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }
        public virtual float CollisionRadius
        {
            get
            {
                return collisionRadiusMultiplier * WorldScale;
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

        public virtual bool IsBoxColliding(Rectangle otherBox)
        {
            if (Collidable)
            {
                return false; //To implement
            }
            return false;
        }

        public bool IsCircleColliding(Vector2 otherCenter, float otherRadius)
        {
            if (Collidable)
            {
                if (Vector2.Distance(WorldCenter, otherCenter) < CollisionRadius + otherRadius)
                    return true;
                return false;
            }
            return false;
        }
    }
}