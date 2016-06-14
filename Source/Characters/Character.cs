using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    class Character : SpriteEntity
    {
        protected float ObjectSpeed = 83f;
        protected Vector2 ObjectAngle = Vector2.Zero;
        private int lives = 1; //It is either life of lives
        protected List<CharacterBehave> behaves;

        public Character()
        {
            behaves = new List<CharacterBehave>();
        }
        public Character(Vector2 worldLocation) {}

        public virtual Color GetColor() { return new Color(0, 0, 0); }
        protected virtual Vector2 determineMoveDirection()
        {
            return ObjectAngle;
        }
        public virtual int Lives
        {
            get { return lives; }
            set { lives = value; }
        }
        public virtual void Hit()
        {
            if (lives > 0)
                lives--;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw()
        {
            base.Draw();
        }
    }

    abstract class CharacterBehave
    {
        public CharacterBehave(Character newCharacter)
        {
            character = newCharacter;
        }
        protected Character character;
        public abstract void Update(GameTime gameTime);
    }

    class ScalingBehave : CharacterBehave
    {
        public ScalingBehave(Character newCharacter) : base(newCharacter)
        {
            scaleTime = 0.8824F;
            orginalScale = character.ObjectBase.scale;
            scaleUp = true;
        }
        protected double scaleTimeElapsed = 0;
        protected double scaleTime = 0;
        protected double scaleScale = 0.15;
        protected double orginalScale = 1;
        protected bool scaleUp = true;
        public override void Update(GameTime gameTime)
        {
            if (scaleTimeElapsed > scaleTime)
            {
                scaleUp = !scaleUp;
                scaleTimeElapsed = scaleTime;
            }
            else if (scaleTimeElapsed < 0)
            {
                scaleUp = !scaleUp;
                scaleTimeElapsed = 0;
            }
            character.ObjectBase.scale = (float)(orginalScale + (orginalScale * (((scaleTimeElapsed / scaleTime) * 2) - 1) * scaleScale));
            character.ObjectBase.CollisionRadius = character.ObjectBase.scale * character.ObjectBase.FrameWidth / 2F;
            if (scaleUp)
                scaleTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
            else
                scaleTimeElapsed -= gameTime.ElapsedGameTime.TotalSeconds * GameManager.TimeScale;
        }
    }
}
