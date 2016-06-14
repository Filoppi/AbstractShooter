using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    abstract class Entity
    {
        public float gravity = 7F;

        public abstract void Update(GameTime gameTime);
    }

    class SpriteEntity : Entity
    {
        public Sprite ObjectBase;

        public override void Update(GameTime gameTime)
        {
            ObjectBase.Update(gameTime);
            //GameManager.grid.Influence(ObjectBase.WorldCenter, gravity);
            GameManager.grid.Influence(ObjectBase.WorldCenter, gravity * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * ObjectBase.Velocity.Length());
        }

        public virtual void Draw()
        {
            ObjectBase.Draw();
        }
    }

    class ParticleEntity : Entity
    {
        public Particle ObjectBase;

        public ParticleEntity(
            Vector2 location,
            Texture2D texture,
            Rectangle initialFrame,
            Vector2 velocity,
            Vector2 acceleration,
            float maxSpeed,
            float duration,
            Color initialColor,
            Color finalColor)
        {
            gravity = 9F;
            ObjectBase = new Particle(location, texture, initialFrame, velocity, acceleration, maxSpeed, duration, initialColor, finalColor);
        }

        public override void Update(GameTime gameTime)
        {
            ObjectBase.Update(gameTime);
            //GameManager.grid.Influence(ObjectBase.WorldCenter, gravity);
            GameManager.grid.Influence(ObjectBase.WorldCenter, gravity * (float)gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale * ObjectBase.Velocity.Length());
        }

        public virtual void Draw()
        {
            ObjectBase.Draw();
        }
    }
}
