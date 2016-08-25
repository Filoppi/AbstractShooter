using AbstractShooter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AbstractShooter
{
	public class ACharacterActor : AAnimatedSpriteActor
	{
		protected float ObjectSpeed = 83f;
		public Vector2 objectDirection = Vector2.Zero; //to protected
		private int lives = 1; //It is either life of lives

		public ACharacterActor(Texture2D texture, List<Rectangle> frames,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
			: base(texture, frames, ActorUpdateGroup.Characters, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
		{
		}

		public virtual Color GetColor()
		{
			return Color.Black;
		}

		protected virtual void DetermineMoveDirection()
		{
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

		protected virtual void CollidedWithWorldBorders()
		{
		}

		protected void ClampToWorld()
		{
			float currentX = spriteComponent.TopLeftLocation.X;
			float currentY = spriteComponent.TopLeftLocation.Y;

			bool collided = false;
			//To improve...
			if (currentX != currentX.GetClamped(((Level)StateManager.currentState).grid.NodeSize, (((Level)StateManager.currentState).grid.gridWidth - 3) * ((Level)StateManager.currentState).grid.NodeSize))
			{
				currentX = currentX.GetClamped(((Level)StateManager.currentState).grid.NodeSize, (((Level)StateManager.currentState).grid.gridWidth - 3) * ((Level)StateManager.currentState).grid.NodeSize);

				collided = true;
			}
			if (currentY != currentY.GetClamped(((Level)StateManager.currentState).grid.NodeSize, (((Level)StateManager.currentState).grid.gridHeight - 3) * ((Level)StateManager.currentState).grid.NodeSize))
			{
				currentY = currentY.GetClamped(((Level)StateManager.currentState).grid.NodeSize, (((Level)StateManager.currentState).grid.gridHeight - 3) * ((Level)StateManager.currentState).grid.NodeSize);

				collided = true;
			}
			if (collided)
			{
				CollidedWithWorldBorders();
			}

			spriteComponent.TopLeftLocation = new Vector2(currentX, currentY);
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