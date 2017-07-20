using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace UnrealMono
{
	public enum ActorUpdateGroup
	{
		Background,
		Default,
		PassiveObjects,
	    Enemies,
        NPCs,
		Players,
		Weapons,
		Particles,
		PostPhysics,
		Camera,
		MAX
	}

    public class AActor
    {
        [UnrealMono(category = "Actor", serializeAs = typeof(Dictionary<string, string>))]
        protected CSceneComponent rootComponent; //SceneComponents; no CSceneComponent list is directly stored
        public CSceneComponent RootComponent { get { return rootComponent; } }
		protected List<CComponent> components = new List<CComponent>(); //Other components that are not CSceneComponent
        [UnrealMono(customName = "Components", category = "Components")] //TO1 check
        public List<CComponent> Components { get { return components; } }

	    //TO1 //To add transform and ...
        [UnrealMono(customName = "Components Types and Attachement Index", category = "Actor")]
        public Dictionary<int, string> componentsTypesAndAttachementIndex;
        
        [UnrealMono(customName = "ijniub", category = "Actor")]
	    public Vector3 ijniub;
        
	    [UnrealMono]
	    public Vector2 Test { get; set; }

        [UnrealMono(customName = "rthrt", category = "Actor")]
	    public List<Vector3> rthrt;

	    [UnrealMono(customName = "papa", category = "Actor")]
	    public List<string> papa;

        private ActorUpdateGroup updateGroup = ActorUpdateGroup.Default;
		public ActorUpdateGroup UpdateGroup { get { return updateGroup; } }
		public bool isActorUpdateEnabled = true;
		public bool isComponentsUpdateEnabled = true;
		public bool isVisible = true;
		private bool pendingDestroy;
		public bool PendingDestroy { get { return pendingDestroy; } }

		private int uniqueId = -1;
		private bool isUniqueIdSet;

		public int UniqueId
		{
			set
			{
				if (!isUniqueIdSet)
				{
					isUniqueIdSet = true;
					uniqueId = value;
					UpdateDrawGroupDepth();
				}
			}
		}

		public float drawGroupDepth;

	    public AActor()
	    {
	        updateGroup = ActorUpdateGroup.Default;
	        //StateManager.currentState.RegisterActor(this); //TO1
	    }

        public AActor(ActorUpdateGroup updateGroup = ActorUpdateGroup.Default)
		{
			this.updateGroup = updateGroup;
			StateManager.currentState.RegisterActor(this);
		}

		public AActor(ref CSceneComponent rootComponent, ActorUpdateGroup updateGroup = ActorUpdateGroup.Default)
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
				List<CSceneComponent> sceneComponents = StateManager.currentState.GetAllSceneComponents();
				foreach (CSceneComponent sceneComponent in sceneComponents)
				{
					if (sceneComponent.Owner == this)
					{
						sceneComponent.Destroy(false);
					}
				}

				//Destroy all logical components
				foreach (CComponent component in components.ToList())
				{
					component.Destroy();
				}

				return true;
			}
			return false;
		}

		public void UpdateDrawGroupDepth()
		{
			drawGroupDepth = (uniqueId % State.maxUniqueDrawDepthActors) * State.componentsDrawDepthDelta;
		}

		public List<CSceneComponent> GetSceneComponents()
		{
			List<CSceneComponent> sceneComponents = new List<CSceneComponent>();
			rootComponent?.GetAllDescendantsAndSelf(ref sceneComponents);
			return sceneComponents;
		}

		public List<T> GetSceneComponentsByClass<T>() where T : CSceneComponent
		{
			List<T> sceneComponents = new List<T>();
			rootComponent?.GetAllDescendantsAndSelf<T>(ref sceneComponents);
			return sceneComponents;
		}

		public void GetSceneComponents(ref List<CSceneComponent> sceneComponents)
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

		public bool ContainsComponent(CComponent component)
		{
			return components.Contains(component);
		}

		public bool ContainsSceneComponent(CSceneComponent sceneComponent)
		{
			if (sceneComponent != null)
			{
				return GetSceneComponents().Contains(sceneComponent);
			}
			return false;
		}

		public void AddComponent(CComponent component)
		{
			if (!components.Contains(component))
			{
				components.Add(component);
			}
		}

		public bool RemoveComponent(CComponent component)
		{
			return components.Remove(component);
		}
	    
	    //To1 Add AddSceneComponent ?

		public bool RemoveSceneComponent(CSceneComponent sceneComponent)
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

		public bool SetRootComponent(CSceneComponent component)
		{
			if (rootComponent != null)
			{
				ExtendedException.ThrowMessage("rootComponent is already set");
				return false;
			}
			if (component == null)
			{
				ExtendedException.ThrowMessage("component is null");
				return false;
			}
			rootComponent = component;
			component.SetOwner(this);
			return true;
		}

		public bool AttachSceneComponent(CSceneComponent component)
		{
			return component.AttachToActor(this, true);
		}

		public List<AActor> GetOverlappingActors()
		{
			List<AActor> overlappingActors = new List<AActor>();
			foreach (CSceneComponent sceneComponent in GetSceneComponents())
			{
				overlappingActors.AddRange(sceneComponent.GetOverlappingActors());
			}
			return overlappingActors.Distinct().ToList();
		}

		public virtual void BeginActorOverlap(AActor otherActor)
		{
		}

		public virtual void EndActorBeginOverlap(AActor otherActor)
		{
		}

		public float GetMass()
		{
			return rootComponent.GetTotalMass();
		}

		public Vector2 GetMassCenter()
		{
			Vector2 massCenter = Vector2.Zero;
			List<CSceneComponent> components = new List<CSceneComponent>();
			rootComponent.GetAllDescendantsAndSelf(ref components);
			foreach (CSceneComponent component in components)
			{
				massCenter += component.WorldLocation;
			}
			return components.Count > 0 ? (massCenter / components.Count) : Vector2.Zero; //ToDo: Weight Average based on Mass
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

		protected virtual void UpdateActor(GameTime gameTime)
		{
		}

		public void UpdateComponents(GameTime gameTime, ComponentUpdateGroup updateGroup)
		{
			if (isComponentsUpdateEnabled)
			{
				foreach (CComponent component in components.ToList())
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
}