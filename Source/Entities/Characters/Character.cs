using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class ACharacter : AAnimatedSprite
    {
        protected float ObjectSpeed = 83f;
        protected Vector2 ObjectAngle = Vector2.Zero;
        private int lives = 1; //It is either life of lives

        public ACharacter(Texture2D texture, List<Rectangle> frames,
            ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
            : base(texture, frames, ActorUpdateGroup.Characters,updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
        {
        }

        public virtual Color GetColor() { return Color.Black; }
        protected virtual Vector2 determineMoveDirection()
        {
            return ObjectAngle;
        }
        public virtual int Lives
        {
            get { return lives; }
            set { lives = value; }
        }
        public virtual void Hit(int damage = 1)
        {
            lives = MathHelper.Max(lives - damage, 0);
        }

        //protected override void UpdateActor(GameTime gameTime)
        //{
        //    base.Update(gameTime);
        //}
        //public override void Draw()
        //{
        //    base.Draw();
        //}
    }
}
