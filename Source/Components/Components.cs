using System;
using System.Collections.Generic;

namespace AbstractShooter
{
    public delegate void DestroyedEventHandler(CComponent sender);

    //public enum InputModes
    //{
    //    KeyboardMouse,
    //    Keyboard,
    //    Mouse,
    //    GamePad,
    //    KeyboardMouseGamePad1, //Allows the use of everything together
    //    KeyboardMouseGamePads, //Allows the use of everything together for Multiplayer
    //    None
    //};

    public enum ComponentUpdateGroup
    {
        BeforeActor,
        Custom1,
        Custom2,
        Custom3,
        AfterActor
    }

    public enum ComponentCollisionsState
    {
        Disabled,
        OverlapOnly,
        BlockingOnly,
        Enabled
    }

    public static class DrawGroup
    {
        public const float FarBackground = 0.05F;
        public const float Background = 0.1F;
        public const float BackgroundParticles = 0.15F;
        public const float PassiveObjects = 0.2F;
        public const float Default = 0.25F;
        public const float Powerups = 0.3F;
        public const float Mines = 0.35F;
        public const float Characters = 0.4F; //To do enemies before???
        public const float Shots = 0.45F;
        public const float Players = 0.5F;
        public const float ForegroundParticles = 0.55F;
        public const float Foreground = 0.6F;
        public const float DebugGraphics = 0.65F;
        public const float UI = 0.95F;
    }

    [Flags]
    public enum ComponentCollisionGroup
    {
        None = 0,
        Static = 1,
        Dynamic = 2,
        Physic = 4,
        Character = 8,
        Weapon = 16,
        Particle = 32,
        Custom1 = 64,
        Custom2 = 128,
        Custom3 = 256
    }

    public struct Animation
    {
        public List<int> framesIndex;
        public float frameTime;
        public Animation(ref List<int> newFramesIndex, float newframeTime = 0.1f)
        {
            frameTime = newframeTime;
            framesIndex = newFramesIndex;
        }
    }
}