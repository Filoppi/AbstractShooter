using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter.States
{
    public class Level : State
    {
        protected int Difficulty = 1;
        protected int NOfPlayers = 1;
        protected float EnemySpawTimer = 0.55f;
        protected int LevelDimensionX = 2330;
        protected int LevelDimensionY = 1430;
        protected float TimeLeft = 60;
        protected int EnemiesLeft = 0;
        protected bool isEndless = false;
        protected int levelIndex = 0;

        public override void Initialize()
        {
            base.Initialize();
            spriteSheet = Game1.Get.Content.Load<Texture2D>(@"Textures\SpriteSheet");
            GameManager.Initialize(Difficulty, NOfPlayers, EnemySpawTimer, LevelDimensionX, LevelDimensionY, TimeLeft, EnemiesLeft, isEndless);
            if (!isEndless)
            {
                GameManager.lastPlayedLevel = levelIndex;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            GameManager.Update(gameTime);
        }
        public override void Draw()
        {
            GameManager.Draw();
            base.Draw();
        }
    }
}