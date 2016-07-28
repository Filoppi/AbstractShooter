using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public class GameMode
    {
        //Singleton:
        private static GameMode instance;
        public static GameMode Get { get { return instance; } }

        public GameMode()
        {
            instance = this;
        }

        public virtual void Initialize(int Difficulty, int NewNOfPlayers, float newEnemySpawTimerOriginal, int levelDimensionX, int levelDimensionY, float newTimeLeft, int newEnemiesLeft, bool newEndless) { }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw() { }
    }
}
