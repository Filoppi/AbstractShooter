using AbstractShooter.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
	public class AEnemySeekerActor : AEnemyActor
	{
		private Vector2 offSet;
		private Vector2 offSetTemp;
		private Vector2 offSetCurrent = Vector2.Zero;
		private int type;
		private float baseAngle;
		private float rotationSpeed;
		private bool goingUp = true;
		private int followingPlayerN;
		private bool surroundingPlayer = true;
		private float sinusoidalRotation;

		public AEnemySeekerActor(Vector2 worldLocation, int newType, int newSpeed)
			: base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 250, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Enemies1, worldLocation, true)
		{
			type = newType;
			Lives = 5;
			if (type == 6) //Pink Circle
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(0, 250, 32, 32) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(80, 125) / 100.0f;
				Lives = 7;
			}
			else if (type == 5) //Orange Pinwheel
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(34, 288, 32, 32) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(74, 126) / 100.0f;
			}
			else if (type == 4) //Violet Star
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(0, 288, 32, 32) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(74, 126) / 100.0f;
				Lives = 3;
			}
			else if (type == 3) //Yellow Triangle
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(8, 231, 18, 18) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(76, 128) / 100.0f;
				Lives = 1;
			}
			else if (type == 2) //Green Square
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(0, 194, 32, 32),
						new Rectangle(32, 194, 32, 32),
						new Rectangle(64, 194, 32, 32),
						new Rectangle(96, 194, 32, 32),
						new Rectangle(128, 194, 32, 32),
						new Rectangle(160, 194, 32, 32),
						new Rectangle(192, 194, 32, 32),
						new Rectangle(192, 194, 32, 32),
						new Rectangle(160, 194, 32, 32),
						new Rectangle(128, 194, 32, 32),
						new Rectangle(96, 194, 32, 32),
						new Rectangle(64, 194, 32, 32),
						new Rectangle(32, 194, 32, 32),
						new Rectangle(0, 194, 32, 32) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(65, 120) / 100.0f;
				spriteComponent.GenerateDefaultAnimation(0.1F);
			}
			else //if (type == 1) //Sky blue diamond
			{
				spriteComponent.SetFrames(new List<Rectangle> { new Rectangle(0, 161, 32, 32),
						new Rectangle(32, 161, 32, 32),
						new Rectangle(64, 161, 32, 32),
						new Rectangle(96, 161, 32, 32),
						new Rectangle(128, 161, 32, 32),
						new Rectangle(160, 161, 32, 32),
						new Rectangle(192, 161, 32, 32),
						new Rectangle(224, 161, 32, 32) });
				spriteComponent.RelativeScale = MathExtention.Rand.Next(68, 120) / 100.0f;
				spriteComponent.GenerateDefaultAnimation(0.1F);
			}

			AddComponent(new CScalingBehaveComponent(this));

			//Decide if this enemy is going to try to surround the player
			if (MathExtention.Rand.Next(0, 4) == 3)
				surroundingPlayer = false;
			else
				surroundingPlayer = true;

			//Surrounding max angle
			offSet = new Vector2(0, (float)MathExtention.Rand.Next(2, 7));
			offSetCurrent = offSet;

			movingSpeed = newSpeed * ((float)MathExtention.Rand.Next(49, 305) / 100.0f);

			//Rotate of random amount
			baseAngle = (float)Math.Atan2(1, 0);
			rotationSpeed = (float)MathExtention.Rand.Next(-400, 400) / 1.666f;

			followingPlayerN = MathExtention.Rand.Next(0, ((Level)StateManager.currentState).GetNOfPlayers());

			WorldScale = 1.135f;

			goingUp = true;
			sinusoidalRotation = 0.04f;
		}

		public override Color GetColor()
		{
			if (type == 6) //Pink Circle
			{
				return new Color(244, 0, 126);
			}
			else if (type == 5) //Orange Pinwheel
			{
				return new Color(255, 101, 0);
			}
			else if (type == 4) //Violet Star
			{
				return new Color(143, 0, 255);
			}
			else if (type == 3) //Yellow Triangle
			{
				return new Color(255, 255, 43);
			}
			else if (type == 2) //Green Square
			{
				return new Color(47, 255, 50);
			}
			//else if (type == 1) //Sky blue diamond
			return new Color(60, 226, 255);
		}

		protected override void CollidedWithWorldBorders()
		{
			DetermineMoveDirection();
		}

		protected override void DetermineMoveDirection()
		{
			movingDirection = StateManager.currentState.GetAllActorsOfType<APlayerActor>()[followingPlayerN].RootComponent.WorldLocation - rootComponent.WorldLocation;
			movingDirection.Normalize();
		}

		protected override void UpdateActor(GameTime gameTime)
		{
			DetermineMoveDirection();

			//Make enemies travel with a Sinusoidal wave
			if (surroundingPlayer)
			{
				offSetCurrent = new Vector2(0, offSetCurrent.Y + (sinusoidalRotation * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * 60F * (goingUp ? 1F : -1F)));

				if (offSetCurrent.Y > offSet.Y)
				{
					goingUp = false;
				}
				else if (offSetCurrent.Y < -offSet.Y)
				{
					goingUp = true;
				}

				offSetTemp = offSetCurrent;
				offSetTemp.Normalize();
				Vector2 OffSetTempTemp = offSetTemp;
				OffSetTempTemp.X = Math.Abs(OffSetTempTemp.X);
				OffSetTempTemp.Y = Math.Abs(OffSetTempTemp.Y);

				//Calculates angle
				float Angle = (float)Math.Atan2(OffSetTempTemp.Y - movingDirection.Y, OffSetTempTemp.X - movingDirection.X);

				//Fix angle
				Angle *= 2.0f;
				Angle -= 3.14f / 2.0f;

				//Rotate vector correctly
				Vector2 v = offSetTemp;
				float sin = (float)Math.Sin(Angle);
				float cos = (float)Math.Cos(Angle);
				float tx = v.X;
				float ty = v.Y;
				v.X = (cos * tx) - (sin * ty);
				v.Y = (sin * tx) + (cos * ty);
				offSetTemp.X = v.X;
				offSetTemp.Y = v.Y;

				//Add Sinusoidal to normal movement
				offSetTemp.Normalize();
				movingDirection += offSetTemp;
				movingDirection.Normalize();
			}

			rootComponent.localVelocity = movingDirection * movingSpeed;

			//Rotates the angle
			baseAngle -= MathHelper.ToRadians(rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale);
			spriteComponent.RotateTo(new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)));

			base.UpdateActor(gameTime);

			ClampToWorld();
		}
	}
}