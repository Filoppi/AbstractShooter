using Microsoft.Xna.Framework;
using UnrealMono;

namespace AbstractShooter
{
	public class CScalingBehaveComponent : CComponent
	{
		protected double scaleTimeElapsed = 0;
		protected double scaleTime = 0;
		protected double scaleScale = 0.15;
		protected double originalScale = 1;
		protected bool scaleUp = true;

		public CScalingBehaveComponent(AActor owner) : base(owner)
		{
			scaleTime = 0.8824F;
			originalScale = owner.WorldScale;
			scaleUp = true;
		}

		protected override void UpdateComponent(GameTime gameTime)
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
			owner.WorldScale = (float)(originalScale + (originalScale * (((scaleTimeElapsed / scaleTime) * 2) - 1) * scaleScale));
			if (scaleUp)
				scaleTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
			else
				scaleTimeElapsed -= gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
		}
	}
}