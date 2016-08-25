using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AbstractShooter
{
	public class CParticleEmitterComponent : CSceneComponent
	{
		private Texture2D texture;
		private List<Rectangle> frames;
		private bool emitOnlyIfMoving = false;
		private float elapsedTime = 0f;
		private float emissionTimer = 0.03f;

		public float EmissionTimer
		{
			get { return emissionTimer; }
		}

		private Vector2 previousWorldLocation;

		public CParticleEmitterComponent(AActor owner,
			Texture2D texture, List<Rectangle> frames,
			float emissionTimer, bool emitOnlyIfMoving,
			CSceneComponent parentSceneComponent = null,
			ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
			Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1)
			: base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
		{
			this.emissionTimer = emissionTimer;
			this.emitOnlyIfMoving = emitOnlyIfMoving;
			this.texture = texture;
			this.frames = frames;
			previousWorldLocation = WorldLocation;
		}

		private void SpawnParticle()
		{
			ACharacterActor character = Owner as ACharacterActor;
			if (character != null)
			{
				ParticlesManager.AddSmog(WorldLocation, character.objectDirection);
			}
			else
			{
				ParticlesManager.AddSmog(WorldLocation, Vector2.Zero);
			}
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			base.UpdateComponent(gameTime);

			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
			elapsedTime += elapsed;

			if (elapsedTime >= emissionTimer)
			{
				if (!emitOnlyIfMoving || WorldLocation != previousWorldLocation)
				{
					SpawnParticle();
				}

				do
				{
					elapsedTime -= emissionTimer;
				}
				while (elapsedTime >= emissionTimer);
			}

			previousWorldLocation = WorldLocation;
		}
	}
}