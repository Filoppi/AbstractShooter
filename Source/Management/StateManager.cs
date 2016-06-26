using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public static class StateManager
    {
        public static State currentState;
        private static State pauseState;
        public static State nextState;
        private static State prePauseState;

        public static void CreateAndSetState<S, P>() where S : State, new() where P : State, new()
        {
            nextState = new S();
            //nextState = (T)Activator.CreateInstance(typeof(T));
            prePauseState = null;
            pauseState = new P();
        }
        public static void CreateAndSetState<T>() where T : State, new()
        {
            nextState = new T();
            //nextState = (T)System.Activator.CreateInstance(typeof(T));
            prePauseState = null;
            pauseState = null;
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
                //currentState = (T)Activator.CreateInstance(typeof(T));
            }
        }
        public static void Pause()
        {
            if (nextState == null && pauseState != currentState && pauseState != null)
            {
                prePauseState = currentState;
                currentState = pauseState;
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
                }
            }
        }
        public static void Unpause()
        {
            if (nextState == null)
            {
                //Restores the previous state
                currentState = prePauseState;
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
            }

            currentState?.Update(gameTime);
        }

        public static void Draw()
        {
            currentState?.Draw();
        }
    }
}
