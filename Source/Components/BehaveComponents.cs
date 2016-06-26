using Microsoft.Xna.Framework;

namespace AbstractShooter
{
    public class ScalingBehaveComponent : Component
    {
        protected double scaleTimeElapsed = 0;
        protected double scaleTime = 0;
        protected double scaleScale = 0.15;
        protected double orginalScale = 1;
        protected bool scaleUp = true;

        public ScalingBehaveComponent(AActor owner) : base(owner)
        {
            scaleTime = 0.8824F;
            orginalScale = owner.WorldScale;
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
            owner.WorldScale = (float)(orginalScale + (orginalScale * (((scaleTimeElapsed / scaleTime) * 2) - 1) * scaleScale));
            if (scaleUp)
                scaleTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
            else
                scaleTimeElapsed -= gameTime.ElapsedGameTime.TotalSeconds * StateManager.currentState.TimeScale;
        }
    }
}
