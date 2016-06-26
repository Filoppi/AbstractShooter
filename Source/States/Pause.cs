using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InputManagement;

namespace AbstractShooter.States
{
    public class Pause : GUIState
    {
        private float pauseTime = 0.0f;
        protected const float enterActionWait = 0.54F;
        private ActionBinding enterAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.P, KeyAction.Pressed), new KeyBinding<Keys>(Keys.Enter, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Start, KeyAction.Pressed) });
        private ActionBinding exitAction = new ActionBinding(new KeyBinding<Keys>[] { new KeyBinding<Keys>(Keys.Back, KeyAction.Pressed) }, new KeyBinding<Buttons>[] { new KeyBinding<Buttons>(Buttons.Y, KeyAction.Pressed) });

        public override void Initialize()
        {
            base.Initialize();
            background = Game1.Get.Content.Load<Texture2D>(@"Textures\PauseScreen");
            SoundsManager.PauseMusic();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            pauseTime += (float)gameTime.ElapsedGameTime.TotalSeconds; // * StateManager.currentState.TimeScale;
            if (pauseTime > enterActionWait)
            {
                if (enterAction.CheckBindings())
                {
                    StateManager.Unpause();
                    SoundsManager.ResumeMusic();
                }
                else if (exitAction.CheckBindings())
                {
                    StateManager.CreateAndSetState<States.MainMenuState>();
                }
            }
        }
    }
}