using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnrealMono;

namespace AbstractShooter
{
	public class CSmogParticleEmitterComponent : CParticleEmitterComponent
	{
		public CSmogParticleEmitterComponent(AActor owner, Texture2D texture, List<Rectangle> frames, float emissionTimer, bool emitOnlyIfMoving, CSceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = 0.25F, Vector2 location = default(Vector2), bool isLocationWorld = false, float relativeScale = 1, Vector2 acceleration = default(Vector2), float maxSpeed = -1F) : base(owner, texture, frames, emissionTimer, emitOnlyIfMoving, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
		{
		}

		protected override void SpawnParticle()
		{
			ACharacterActor character = Owner as ACharacterActor;
			if (character != null)
			{
				ParticlesSpawner.AddSmog(WorldLocation, character.movingDirection);
			}
			else
			{
				ParticlesSpawner.AddSmog(WorldLocation, Vector2.Zero);
			}
		}
	}
}