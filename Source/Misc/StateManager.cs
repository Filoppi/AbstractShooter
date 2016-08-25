using Microsoft.Xna.Framework;

namespace AbstractShooter
{
	public static class StateManager
	{
		public static State currentState;
		private static State pauseState;
		private static State nextState;
		private static State prePauseState;

		public static State ReloadCurrentState()
		{
			if (currentState != null)
			{
				if (prePauseState != null) //State was paused
				{
					nextState = System.Activator.CreateInstance(prePauseState.GetType()) as State;
				}
				else
				{
					nextState = System.Activator.CreateInstance(currentState.GetType()) as State;
				}
				prePauseState = null;
				if (pauseState != null)
				{
					pauseState = System.Activator.CreateInstance(pauseState.GetType()) as State;
				}
			}
			return nextState;
		}

		public static S CreateAndSetState<S, P>() where S : State, new() where P : State, new()
		{
			nextState = new S();
			prePauseState = null;
			pauseState = new P();
			return (S)nextState;
		}

		public static S CreateAndSetState<S>() where S : State, new()
		{
			nextState = new S();
			prePauseState = null;
			pauseState = null;
			return (S)nextState;
		}

		public static void SetState(State newState, State newPauseState = null)
		{
			nextState = newState;
			prePauseState = null;
			pauseState = newPauseState;
		}

		public static void CreateAndSetPause<T>() where T : State, new()
		{
			if (nextState == null && pauseState != currentState)
			{
				//Store The previous state so to restore it later
				pauseState = new T();
				prePauseState = currentState;
				currentState = pauseState;
				currentState.OnSetAsCurrentState();
			}
		}

		public static void Pause()
		{
			if (nextState == null && pauseState != currentState && pauseState != null)
			{
				prePauseState = currentState;
				currentState = pauseState;
				currentState.OnSetAsCurrentState();
			}
		}

		public static void Pause(State newPauseState)
		{
			if (pauseState != currentState)
			{
				pauseState = newPauseState;
				if (nextState == null)
				{
					//Store The previous state so to restore it later
					prePauseState = currentState;
					currentState = newPauseState;
					currentState.OnSetAsCurrentState();
				}
			}
		}

		public static void Unpause()
		{
			if (nextState == null && prePauseState != null)
			{
				//Restores the previous state
				currentState = prePauseState;
				currentState.OnSetAsCurrentState();
				prePauseState = null;
			}
		}

		public static void Update(GameTime gameTime)
		{
			if (nextState != null)
			{
				currentState = nextState;
				nextState = null;
				currentState.Initialize();
				pauseState?.Initialize();
				currentState.OnSetAsCurrentState();
			}

			currentState?.Update(gameTime);
		}

		public static void Draw()
		{
			currentState?.Draw();
		}
	}
}