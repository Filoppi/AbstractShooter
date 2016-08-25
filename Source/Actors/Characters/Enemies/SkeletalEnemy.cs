using AbstractShooter.States;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AbstractShooter
{
	public class ASkeletalEnemyActor : AEnemyActor
	{
		private static Random rand = new Random();
		private int FollowingPlayerN;
		private float baseAngle;
		private float rotationSpeed;
		private double sinTime;

		private int branches = 3;
		private int jointsNumber = 3;
		private CSceneComponent[,] joints; //0 Head, middle body, Last tail

		public ASkeletalEnemyActor(Vector2 worldLocation, int newSpeed)
			: base(StateManager.currentState.spriteSheet, new List<Rectangle> { new Rectangle(0, 250, 32, 32) }, ComponentUpdateGroup.AfterActor, DrawGroup.Characters, worldLocation, true)
		{
			//if jointsNumber >= 1;

			damageType = DamageType.ComponentsLast;

			spriteComponent = new CAnimatedSpriteComponent(this,
					StateManager.currentState.spriteSheet,
					new List<Rectangle> { new Rectangle(0, 288, 32, 32) },
					null,
					ComponentUpdateGroup.BeforeActor, DrawGroup.Characters,
					worldLocation,
					true,
					rand.Next(33, 80) / 100.0f);
			rootComponent = spriteComponent;

			spriteComponent.collisionGroup = (int)ComponentCollisionGroup.Character;
			spriteComponent.overlappingGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Static | ComponentCollisionGroup.Weapon;

			joints = new CSceneComponent[branches, jointsNumber];
			for (int k = 0; k < branches; k++)
			{
				for (int i = 0; i < jointsNumber; i++)
				{
					Vector2 relativeLocation = new Vector2(17 * (float)Math.Acos(Math.PI * 2.0 * (double)k / (branches - 1)), 17 * (float)Math.Asin(Math.PI * 2.0 * (double)k / (branches - 1)));
					if (i == 0)
					{
						joints[k, i] = new CSceneComponent(this,
							rootComponent,
							ComponentUpdateGroup.BeforeActor);
						joints[k, i].RelativeRotation += (float)Math.PI * 2F * (float)k / (branches);
						//joints[k, i].RelativeLocation = ((i == 0) ? new Vector2(0, 0) : new Vector2(17, 0));
					}
					else
					{
						joints[k, i] = new CSpriteComponent(this,
							StateManager.currentState.spriteSheet,
							new List<Rectangle> { new Rectangle(8, 231, 18, 18) },
							joints[k, i - 1],
							ComponentUpdateGroup.BeforeActor, DrawGroup.Characters,
							new Vector2(0, 0),
							//((i == 0) ? new Vector2(34, 0) : new Vector2(17, 0)),
							false);
						//joints[k, i].RelativeRotation = joints[k, i].RelativeRotation;
						joints[k, i].RelativeLocation = ((i == 0) ? new Vector2(0, 0) : new Vector2(17, 0));
					}
					joints[k, i].collisionGroup = (int)ComponentCollisionGroup.Character;
					joints[k, i].overlappingGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Static | ComponentCollisionGroup.Weapon;
				}
			}

			Lives = 5;

			ObjectSpeed = newSpeed * ((float)rand.Next(30, 145) / 100.0f);

			//Rotate of random amount
			baseAngle = (float)Math.Atan2(1, 0);
			rotationSpeed = (float)rand.Next(-200, 200) / 1.666f;

			FollowingPlayerN = rand.Next(0, ((Level)StateManager.currentState).GetNOfPlayers());

			WorldScale = 1F;

			DetermineMoveDirection();
		}

		protected override void CollidedWithWorldBorders()
		{
			DetermineMoveDirection();
		}

		protected override void DetermineMoveDirection()
		{
			objectDirection = new Vector2(rand.Next(-10, 11), rand.Next(-10, 11));
			objectDirection.Normalize();
		}

		protected override void UpdateActor(GameTime gameTime)
		{
			rootComponent.localVelocity = objectDirection * 35;

			sinTime += gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * 1; //0.45
			for (int k = 0; k < branches; k++)
			{
				for (int i = 1; i < jointsNumber; i++)
				{
					if (!joints[k, i].PendingDestroy)
					{
						joints[k, i].RelativeRotation = (float)Math.Sin(sinTime + ((Math.PI / jointsNumber) * 1 * (double)i)) / 4;
					}
				}
			}
			//Behaviour 1
			//for (int i = 0; i < jointsNumber; i++)
			//{
			//    joints[i].RelativeRotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale * 0.1F;
			//}

			//Rotates the angle
			baseAngle -= MathHelper.ToRadians(rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale);
			spriteComponent.RotateTo(new Vector2((float)Math.Cos(baseAngle), (float)Math.Sin(baseAngle)));

			base.UpdateActor(gameTime);

			ClampToWorld();
		}
	}
}