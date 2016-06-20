using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractShooter
{
    public delegate void DestroyedEventHandler(Component sender);

    public enum ComponentUpdateGroup
    {
        BeforeActor,
        Custom1,
        Custom2,
        Custom3,
        AfterActor
    }

    public abstract class Component
    {
        protected AActor owner;
        public AActor Owner { get { return owner; } }

        public ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor;
        public DrawGroup drawGroup = DrawGroup.Default;

        public event DestroyedEventHandler Destroyed;

        public bool isUpdateEnabled = true;

        protected bool pendingDestroyed = false;
        public bool PendingDestroyed { get { return pendingDestroyed; } }

        public Component(AActor owner)
        {
            InternalSetOwner(owner);
        }
        public Component(AActor owner, ComponentUpdateGroup updateGroup, DrawGroup drawGroup)
        {
            InternalSetOwner(owner);
            this.updateGroup = updateGroup;
            this.drawGroup = drawGroup;
        }

        protected virtual void BroadcastDestroyed()
        {
            if (Destroyed != null)
            {
                Destroyed(this);
            }
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
            pendingDestroyed = true;
            if (owner != null)
            {
                owner.RemoveComponent(this);
                BroadcastDestroyed();
                return true;
            }
            return true; //If there is no owner, was already going to be destroyed
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
        private bool InternalDestroy(bool allDescendants = false) //To improve
        {
            pendingDestroyed = true;
            if (owner != null && owner.RootComponent == this) //If it is root component
            {
                owner.RemoveRootComponent();
            }
            else if (parent != null)
            {
                parent.RemoveChild(this);
            }
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
            BroadcastDestroyed();
            return true;
        }

        public Vector2 relativeLocation = Vector2.Zero; //Relative to partent, then relative to owner; if owner is null, then it is WorldLocation
        public float relativeRotation = 0; //Clockwise
        public float relativeScale = 1.0f;
        protected float mass = 0F;
        public Vector2 velocity = Vector2.Zero;
        protected Vector2 acceleration = Vector2.Zero;
        protected float maxSpeed = -1;
        public bool isVisible = true;

        public SceneComponent(AActor owner, SceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, DrawGroup drawGroup = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1) : base(owner, updateGroup, drawGroup)
        {
            if (parentSceneComponent != null)
            {
                AttachToSceneComponent(parentSceneComponent);
            }
            this.acceleration = acceleration;
            this.maxSpeed = maxSpeed;

            this.relativeScale = relativeScale;
            if (isLocationWorld)
            {
                WorldLocation = location;
            }
            else
            {
                relativeLocation = location;
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
        public Int32 GetAllDescendantsNum(Int32 count = 0)
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
            massCenter += this.WorldLocation;
            return massCenter;
        }

        public Vector2 WorldLocation
        {
            set
            {
                relativeLocation = value;
            }
            get
            {
                return relativeLocation;
            }
        }
        //private Vector2 GetWorldLocation(Vector2 location = default(Vector2))
        //{
        //    if (parent == null)
        //    {
        //        location += Vector2.Transform(relativeLocation, Matrix.CreateRotationX(relativeRotation));
        //    }
        //    else
        //    {
        //        location += Vector2.Transform(relativeLocation, Matrix.CreateRotationX(relativeRotation));
        //        location = parent.GetWorldLocation(location);
        //    }
        //    return location;
        //}

        public float WorldRotation //Unimplemented
        {
            set
            {
                relativeRotation = value;
            }
            get
            {
                return relativeRotation;
            }
        }
        public float WorldScale //Unimplemented
        {
            set
            {
                relativeScale = value;
            }
            get
            {
                return relativeScale;
            }
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
            UpdatePhysics(gameTime);
        }
        private void UpdatePhysics(GameTime gameTime)
        {
            velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * 60F;
            if (maxSpeed >= 0 && velocity.LengthSquared() > maxSpeed * maxSpeed)
            {
                velocity.Normalize();
                velocity *= maxSpeed;
            }
            WorldLocation += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
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
    }
}