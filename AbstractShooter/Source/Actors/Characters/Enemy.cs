using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
	public enum DamageType
	{
		Shared, //All components share the same life
		SharedWeighted, //All components share the same life, but damage is weighted depending on which component you hit
		Components, //Components are destroyed one by one, without affecting the others, each has its own life
		ComponentsFirstAndLast, //Each component has its own life; only the first one and the last one in the chain can be damaged
		ComponentsFirst, //Each component has its own life; only the first one in the chain can be damaged
		ComponentsLast //Each component has its own life; only the last one in the chain can be damaged
		//Custom: e.g. some components are shielded and cannot be hit, some other ...
	}

	public enum DestructionType
	{
		AllTogether,
		OneByOne,
		ChainExternal,
		ChainBothSides
	}

	public class AEnemyActor : ACharacterActor
	{
		public AEnemyActor(Texture2D texture, List<Rectangle> frames,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1, Color tintColor = new Color())
			: base(texture, frames, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed, tintColor)
		{
			spriteComponent.collisionGroup = (int)ComponentCollisionGroup.Character;
			spriteComponent.overlappingGroups = ComponentCollisionGroup.Character | ComponentCollisionGroup.Static | ComponentCollisionGroup.Weapon;
		}

		public DamageType damageType;
		public DestructionType destructionType;
	}
}