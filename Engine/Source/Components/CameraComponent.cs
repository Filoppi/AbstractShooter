using Microsoft.Xna.Framework;

namespace UnrealMono
{
    public class CCameraComponent : CSceneComponent
    {
        public CCameraComponent(AActor owner, CSceneComponent parentSceneComponent = null, ComponentUpdateGroup updateGroup = ComponentUpdateGroup.AfterActor, float layerDepth = DrawGroup.Default,
            Vector2 location = new Vector2(), bool isLocationWorld = false, float relativeScale = 1F, Vector2 acceleration = new Vector2(), float maxSpeed = -1)
            : base(owner, parentSceneComponent, updateGroup, layerDepth, location, isLocationWorld, relativeScale, acceleration, maxSpeed)
		{
        }

        protected override void UpdateComponent(GameTime gameTime)
        {
            base.UpdateComponent(gameTime);

            //Sets the center of the camera at the location of this component
            Camera.Center = WorldLocation;
        }
    }
}