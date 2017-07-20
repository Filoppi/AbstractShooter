using Microsoft.Xna.Framework;

namespace UnrealMono
{
	public delegate void DestroyedEventHandler(CComponent sender);

	public abstract class CComponent
	{
		protected AActor owner;
		public AActor Owner { get { return owner; } }

		public ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor;
        
		public event DestroyedEventHandler destroyed;

		public bool isUpdateEnabled = true;
		protected bool pendingDestroy;
		public bool PendingDestroy { get { return pendingDestroy; } }

		public CComponent(AActor owner)
		{
			InternalSetOwner(owner);
		}

		public CComponent(AActor owner, ComponentUpdateGroup updateGroup)
		{
			InternalSetOwner(owner);
			this.updateGroup = updateGroup;
		}

		protected virtual void BroadcastDestroyed()
		{
			destroyed?.Invoke(this);
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

		///Only works if there are no external strong references to this object
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

		protected virtual void UpdateComponent(GameTime gameTime)
		{
		}
	}
}