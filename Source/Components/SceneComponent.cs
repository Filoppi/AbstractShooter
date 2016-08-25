using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractShooter
{
	public enum ComponentUpdateGroup
	{
		BeforeActor,
		Custom1,
		Custom2,
		Custom3,
		AfterActor
	}

	public enum ComponentCollisionsState
	{
		Disabled,
		OverlapOnly,
		BlockingOnly,
		Enabled
	}

	public static class DrawGroup
	{
		public const float FarBackground = 0.05F;
		public const float Background1 = 0.075F;
		public const float Background2 = 0.1F;
		public const float Background3 = 0.125F;
		public const float BackgroundParticles = 0.15F;
		public const float PassiveObjects = 0.2F;
		public const float Default = 0.25F;
		public const float Powerups = 0.3F;
		public const float Mines = 0.35F;
		public const float Characters = 0.4F; //To do enemies before???
		public const float Shots = 0.45F;
		public const float Players = 0.5F;
		public const float ForegroundParticles = 0.55F;
		public const float Foreground = 0.6F;
		public const float DebugGraphics = 0.65F;
		public const float UI = 0.95F;
	}

	[Flags]
	public enum ComponentCollisionGroup
	{
		None = 0,
		Static = 1,
		Dynamic = 2,
		Physic = 4,
		Character = 8,
		Weapon = 16,
		Particle = 32,
		Custom1 = 64,
		Custom2 = 128,
		Custom3 = 256
	}

	public class CSceneComponent : CComponent
	{
		protected CSceneComponent parent; //Can be a component that has another owner as well
		protected List<CSceneComponent> children = new List<CSceneComponent>();

		//public List<CSceneComponent> Children { get { return children; } }
		public void GetAllDescendantsAndSelf(ref List<CSceneComponent> sceneComponents)
		{
			sceneComponents.Add(this);
			foreach (CSceneComponent child in children)
			{
				child.GetAllDescendantsAndSelf(ref sceneComponents);
			}
		}

		public void GetAllDescendantsAndSelf<T>(ref List<T> sceneComponents) where T : CSceneComponent
		{
			if (this is T)
			{
				sceneComponents.Add((T)this);
			}
			foreach (CSceneComponent child in children)
			{
				child.GetAllDescendantsAndSelf<T>(ref sceneComponents);
			}
		}

		public int collisionGroup = 1; //To private
		public ComponentCollisionGroup overlappingGroups; //To private
		protected ComponentCollisionGroup blockingGroups;
		private ComponentCollisionsState collisionsState = ComponentCollisionsState.Enabled;

		public ComponentCollisionsState CollisionsState
		{
			get { return collisionsState; }
			set
			{
				if (collisionsState != ComponentCollisionsState.BlockingOnly && collisionsState != ComponentCollisionsState.Disabled && (value == ComponentCollisionsState.BlockingOnly || value == ComponentCollisionsState.Disabled))
				{
					foreach (CSceneComponent overlappingComponent in overlappingComponents)
					{
						if (!overlappingComponent.overlappingComponents.Remove(this))
						{
							throw new System.ArgumentException("overlappingComponent did not contain this component");
						}
						EndComponentOverlap(overlappingComponent);
						overlappingComponent.EndComponentOverlap(this);
					}
					overlappingComponents.Clear();
				}
				else if (collisionsState != ComponentCollisionsState.OverlapOnly && collisionsState != ComponentCollisionsState.Enabled && (value == ComponentCollisionsState.OverlapOnly || value == ComponentCollisionsState.Enabled))
				{
					UpdateDescendantsAndSelfOverlapCollisions(); //Only self?
				}
				collisionsState = value;
			}
		}

		public virtual float CollisionRadius { get { return 0F; } }
		protected float collisionRadiusMultiplier = 1F;

		public float CollisionRadiusMultiplier
		{
			get
			{
				return collisionRadiusMultiplier;
			}
			set
			{
				collisionRadiusMultiplier = Math.Max(0F, value);
			}
		}

		public Circle CollisionCircle { get { return new Circle(worldLocation, CollisionRadius); } }

		public List<AActor> GetOverlappingActors()
		{
			List<AActor> overlappingActors = new List<AActor>();
			foreach (CSceneComponent sceneComponent in overlappingComponents)
			{
				overlappingActors.Add(sceneComponent.GetRootComponentOwner());
			}
			return overlappingActors.Distinct().ToList();
		}

		public List<CSceneComponent> overlappingComponents = new List<CSceneComponent>();
		private float layerDepth;
		public float LayerDepth { get { return layerDepth + drawGroupDepth + owner.drawGroupDepth; } }
		public float drawGroupDepth;

		///Only works if there are no external strong references to this object
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
				CollisionsState = ComponentCollisionsState.Disabled;

				pendingDestroy = true;
				BroadcastDestroyed();

				StateManager.currentState.RemoveSceneComponent(this);

				if (owner != null && owner.RootComponent == this) //If it is root component
				{
					owner.Destroy();
					//owner.RemoveRootComponent();

					foreach (CSceneComponent sceneComponent in children.ToList())
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
						foreach (CSceneComponent sceneComponent in children.ToList())
						{
							sceneComponent.InternalDestroy(allDescendants);
						}
					}
					else
					{
						foreach (CSceneComponent sceneComponent in children.ToList())
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
		protected float worldRotation; //Clockwise //Final rotation you see on the screen
		protected float relativeRotation; //Clockwise //Relative to partents until the root. for root components it is equal to worldRotation

		//protected float drawRotation = 0F; //Clockwise //Does not affect children, only rendering
		protected float worldScale = 1F;

		protected float relativeScale = 1F;

		//protected float drawScale = 1F; //Does not affect children, only rendering
		protected float mass = 1F;

		public float ScaledMass
		{
			get { return mass * worldScale; }
		}

		public Vector2 localVelocity = Vector2.Zero;
		protected Vector2 acceleration = Vector2.Zero;
		protected float maxLocalSpeed = -1F;
		public bool isVisible = true;

		public CSceneComponent(AActor owner, CSceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1) : base(owner, updateGroup)
		{
			if (this.owner.RootComponent == null)
			{
				this.owner.SetRootComponent(this);
			}

			this.layerDepth = layerDepth;

			StateManager.currentState.AddSceneComponent(this);

			if (parentSceneComponent != null)
			{
				AttachToSceneComponent(parentSceneComponent);
			}
			else if (!IsRootComponent()) //if this is not root and doesn't have parents...
			{
				ExtendedException.ThrowMessage("This component is not a rootComponent and does not have a parent");
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

		private void AddChild(CSceneComponent parent)
		{
			if (!children.Contains(parent))
			{
				children.Add(parent);
			}
		}

		private void RemoveChild(CSceneComponent parent)
		{
			children.Remove(parent);
		}

		public int GetChildrenCount()
		{
			return children.Count;
		}

		public int GetChildrenCountOfType<T>() where T : CSceneComponent
		{
			return children.OfType<T>().Count();
		}

		public int GetAllAncestorsCount()
		{
			int count = 0;
			CSceneComponent component = this;
			while (component.parent != null)
			{
				count++;
				component = component.parent;
			}
			return count;
		}

		private void UpdateDrawGroupDepth()
		{
			drawGroupDepth = (GetAllAncestorsCount() % State.maxUniqueDrawDepthComponents) * State.componentsDrawDepthMultiplier;
		}

		public int GetAllDescendantsCount()
		{
			return GetAllDescendantsCountInternal();
		}

		private int GetAllDescendantsCountInternal(int count = 0)
		{
			foreach (CSceneComponent child in children)
			{
				count = child.GetAllDescendantsCountInternal(count);
			}
			return count += GetChildrenCount();
		}

		public int GetAllDescendantsCountOfType<T>() where T : CSceneComponent
		{
			return GetAllDescendantsCountInternalOfType<T>();
		}

		private int GetAllDescendantsCountInternalOfType<T>(int count = 0) where T : CSceneComponent
		{
			foreach (CSceneComponent child in children)
			{
				count = child.GetAllDescendantsCountInternalOfType<T>(count);
			}
			return count += GetChildrenCountOfType<T>();
		}

		public bool AttachSceneComponent(CSceneComponent child, bool keepWorldTransform = true)
		{
			return child.AttachToSceneComponent(this, keepWorldTransform);
		}

		public bool AttachToSceneComponent(CSceneComponent parent, bool keepWorldTransform = true)
		{
			List<CSceneComponent> sceneComponents = new List<CSceneComponent>();
			GetAllDescendantsAndSelf(ref sceneComponents);
			if ((owner == null || owner.RootComponent != this) //If this actor doesn't have an owner or if it is not the rootComponent of its owner
				&& this.parent != parent
				&& (parent == null || !sceneComponents.Contains(parent))) //and if the sceneComponent we are trying to attach this to is not already a child of this
			{
				if (parent != null)
				{
					Transform previousWorldTransform = WorldTransform;
					if (this.parent != null)
					{
						this.parent.RemoveChild(this);
						this.parent = null;
					}
					this.parent = parent;
					this.parent.AddChild(this);
					if (keepWorldTransform)
					{
						WorldTransform = previousWorldTransform;
					}
					UpdateDrawGroupDepth();
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
					Transform previousWorldTransform = WorldTransform;
					parent.RemoveChild(this);
					parent = null;
					actor.SetRootComponent(this);
					if (keepWorldTransform)
					{
						WorldTransform = previousWorldTransform;
					}
					UpdateDrawGroupDepth();
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
			CSceneComponent component = this;
			while (component.parent != null)
			{
				component = component.parent;
			}
			return component.owner;
		}

		public CSceneComponent GetRootComponent()
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

		public bool IsRootComponent()
		{
			if (owner == null)
			{
				throw new System.ArgumentException("Owner is null");
			}
			else
			{
				if (parent != null && owner.RootComponent == this)
				{
					throw new System.ArgumentException("Parent is different than null");
				}
				else if (parent == null && owner.RootComponent != this)
				{
					throw new System.ArgumentException("Parent is null but Owner rootComponent is different than this");
				}
			}
			return owner.RootComponent == this;
		}

		public float GetTotalMass()
		{
			float totalMass = 0;
			List<CSceneComponent> components = new List<CSceneComponent>();
			GetAllDescendantsAndSelf(ref components);
			foreach (CSceneComponent component in components)
			{
				totalMass += component.mass * component.worldScale;
			}
			return totalMass;
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
			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfLocation();
			}
		}

		private void UpdateRelativeLocation()
		{
			if (parent != null)
			{
				relativeLocation = ((worldLocation - parent.worldLocation) / parent.worldScale).Rotate(-parent.worldRotation);
			}
			else
			{
				relativeLocation = worldLocation;
			}
			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfLocation();
			}
		}

		private void UpdateAllDescendantsAndSelfLocation()
		{
			worldLocation = parent.worldLocation + (relativeLocation.Rotate(parent.worldRotation) * parent.worldScale);
			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfLocation();
			}
		}

		public void AddLocalOffset(Vector2 offset)
		{
			RelativeLocation += offset.Rotate(relativeRotation);
		}

		public Transform RelativeTransform
		{
			set
			{
				RelativeLocation = value.location;
				RelativeRotation = value.rotation;
				RelativeScale = value.scale;
			}
			get
			{
				return new Transform(relativeLocation, relativeRotation, relativeScale);
			}
		}

		public Transform WorldTransform
		{
			set
			{
				WorldLocation = value.location;
				WorldRotation = value.rotation;
				WorldScale = value.scale;
			}
			get
			{
				return new Transform(worldLocation, worldRotation, worldScale);
			}
		}

		public Vector2 RelativeLocation
		{
			set
			{
				relativeLocation = value;
				UpdateWorldLocation();
				UpdateDescendantsAndSelfOverlapCollisions();
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
				UpdateDescendantsAndSelfOverlapCollisions();
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
				worldRotation = parent.worldRotation + relativeRotation;
			}
			else
			{
				worldRotation = relativeRotation;
			}
			worldRotation %= MathHelper.TwoPi;

			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfRotation();
			}
		}

		private void UpdateRelativeRotation()
		{
			if (parent != null)
			{
				relativeRotation = worldRotation - parent.worldRotation;
			}
			else
			{
				relativeRotation = worldRotation;
			}
			relativeRotation %= MathHelper.TwoPi;

			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfRotation();
			}
		}

		private void UpdateAllDescendantsAndSelfRotation()
		{
			worldRotation = (parent.worldRotation + relativeRotation) % MathHelper.TwoPi;
			worldLocation = parent.worldLocation + (relativeLocation.Rotate(parent.worldRotation) * parent.worldScale);
			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfRotation();
			}
		}

		public float RelativeRotation
		{
			set
			{
				relativeRotation = value % MathHelper.TwoPi;
				UpdateWorldRotation();
				UpdateDescendantsAndSelfOverlapCollisions();
			}
			get
			{
				return relativeRotation;
			}
		}

		public float WorldRotation
		{
			set
			{
				worldRotation = value % MathHelper.TwoPi;
				UpdateRelativeRotation();
				UpdateDescendantsAndSelfOverlapCollisions();
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

			foreach (CSceneComponent child in children)
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

			foreach (CSceneComponent child in children)
			{
				child.UpdateAllDescendantsAndSelfScale();
			}
		}

		private void UpdateAllDescendantsAndSelfScale()
		{
			worldScale = parent.worldScale * relativeScale;
			foreach (CSceneComponent child in children)
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
				UpdateDescendantsAndSelfOverlapCollisions();
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
				UpdateDescendantsAndSelfOverlapCollisions();
			}
			get
			{
				return relativeScale;
			}
		}

		private void UpdateDescendantsAndSelfOverlapCollisions()
		{
			if ((collisionsState == ComponentCollisionsState.OverlapOnly || collisionsState == ComponentCollisionsState.Enabled)
				&& collisionGroup > 0)
			{
				List<CSceneComponent> foundSceneComponents = StateManager.currentState.GetAllSceneComponents();
				foreach (CSceneComponent foundSceneComponent in foundSceneComponents)
				{
					if (foundSceneComponent.owner != owner
						&& (foundSceneComponent.collisionsState == ComponentCollisionsState.OverlapOnly || foundSceneComponent.collisionsState == ComponentCollisionsState.Enabled)
						&& overlappingGroups.HasFlag((ComponentCollisionGroup)foundSceneComponent.collisionGroup)
						&& foundSceneComponent.overlappingGroups.HasFlag((ComponentCollisionGroup)collisionGroup)) //To1 check if root components is the same or owner?
					{
						if (CollisionCircle.IsCollidingWith(foundSceneComponent.CollisionCircle))
						{
							if (!overlappingComponents.Contains(foundSceneComponent))
							{
#if DEBUG
								if (Game1.debugOverlapCollisions)
								{
									string first = Owner.ToString();
									if (first.StartsWith("AbstractShooter.A"))
									{
										first = first.Remove(0, "AbstractShooter.A".Length);
									}
									string second = foundSceneComponent.owner.ToString();
									if (second.StartsWith("AbstractShooter.A"))
									{
										second = second.Remove(0, "AbstractShooter.A".Length);
									}
									Game1.AddDebugString(first + " >-< " + second, false, 1F);
								}
#endif
								overlappingComponents.Add(foundSceneComponent);
								if (foundSceneComponent.overlappingComponents.Contains(this))
								{
									throw new System.ArgumentException("foundSceneComponent already contained this component");
								}
								foundSceneComponent.overlappingComponents.Add(this);
								BeginComponentOverlap(foundSceneComponent);
								owner.BeginActorOverlap(foundSceneComponent.owner);
								foundSceneComponent.BeginComponentOverlap(this);
								foundSceneComponent.owner.BeginActorOverlap(owner);
							}
						}
						else if (overlappingComponents.Contains(foundSceneComponent))
						{
							overlappingComponents.Remove(foundSceneComponent);
							if (!foundSceneComponent.overlappingComponents.Remove(this))
							{
								throw new System.ArgumentException("foundSceneComponent did not contain this component");
							}
							EndComponentOverlap(foundSceneComponent);
							foundSceneComponent.EndComponentOverlap(this);
						}
					}
				}
			}

			foreach (CSceneComponent child in children)
			{
				child.UpdateDescendantsAndSelfOverlapCollisions();
			}
		}

		public virtual void BeginComponentOverlap(CSceneComponent otherComponent)
		{
		}

		public virtual void EndComponentOverlap(CSceneComponent otherComponent)
		{
		}

		public void AddWorldOffsetAndRotation(Vector2 offset, float angle)
		{
			RelativeRotation += angle;
			WorldLocation += offset;
		}

		public void AddRelativeOffsetAndRotation(Vector2 offset, float angle)
		{
			RelativeRotation += angle;
			RelativeLocation += offset;
		}

		public void AddLocalOffsetAndRotation(Vector2 offset, float angle)
		{
			RelativeRotation += angle;
			AddLocalOffset(offset);
		}

		public void SetRelativeLocationAndRotation(Vector2 location, float angle)
		{
			RelativeRotation = angle;
			RelativeLocation = location;
		}

		public void SetWorldLocatioAndRotation(Vector2 location, float angle)
		{
			WorldRotation = angle;
			WorldLocation = location;
		}

		public void SetRelativeLocationRotationAndScale(Vector2 location, float angle, float scale)
		{
			RelativeRotation = angle;
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
			foreach (CSceneComponent child in children.ToList())
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
			foreach (CSceneComponent child in children)
			{
				child.DrawDescendantsAndSelf();
			}
			if (isVisible)
			{
				Draw();
			}
		}

		protected virtual void Draw()
		{
		}

		public virtual Vector2 ScreenCenter { get { return Camera.WorldToScreenSpace(WorldLocation); } }
		public Vector2 ScreenLocation { get { return Camera.WorldToScreenSpace(WorldLocation); } }
		public virtual bool IsInViewport { get { return false; } }

		public void RotateTo(Vector2 direction, bool worldRotation = true)
		{
			if (worldRotation)
			{
				WorldRotation = (float)Math.Atan2(direction.Y, direction.X);
			}
			else
			{
				RelativeRotation = (float)Math.Atan2(direction.Y, direction.X);
			}
		}
	}
}