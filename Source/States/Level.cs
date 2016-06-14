using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        public override void Init(GraphicsDevice graphicsDevice, ContentManager content)
        {
            spriteSheet = content.Load<Texture2D>(@"Textures\SpriteSheet");

            GameManager.Initialize(Difficulty, NOfPlayers, EnemySpawTimer, LevelDimensionX, LevelDimensionY, TimeLeft, EnemiesLeft, isEndless);
            GameManager.lastPlayedLevel = 1;
        }
        public override void Update(GameTime gameTime)
        {
            GameManager.Update(gameTime);
        }
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            GameManager.Draw();
        }
    }
}