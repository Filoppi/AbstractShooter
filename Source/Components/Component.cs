using Microsoft.Xna.Framework;

namespace AbstractShooter
{
	public delegate void DestroyedEventHandler(CComponent sender);

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

	public abstract class CComponent
	{
		protected AActor owner;
		public AActor Owner { get { return owner; } }

		public ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor;

		public event DestroyedEventHandler Destroyed;

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