using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace AbstractShooter
{
    public static class SoundsManager
    {
        private static Song music;
        private static SoundEffect shootMine;
        private static SoundEffect hit;
        private static SoundEffect kill;
        private static SoundEffect spawn;
        private static SoundEffect explosion;
        private static SoundEffect shoot;
        private static SoundEffect selection1;
        private static SoundEffect selection2;
        private static SoundEffect selection3;
        private static SoundEffect powerUp;
        private static SoundEffect menuEffect;
        private static float volume = 1F;
        public static float Volume {
            get { return volume; }
            set
            {
                volume = value;
                MediaPlayer.Volume = volume;
            }
        }
        private static Random rand = new Random();

        public static void Initialize()
        {
            music = StateManager.contentManager.Load<Song>(@"Sounds\Music");
            shoot = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Shoot");
            shootMine = StateManager.contentManager.Load<SoundEffect>(@"Sounds\ShootMine");
            explosion = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Explosion");
            selection1 = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Selection1");
            selection2 = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Selection2");
            selection3 = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Selection3");
            powerUp = StateManager.contentManager.Load<SoundEffect>(@"Sounds\PowerUp");
            menuEffect = StateManager.contentManager.Load<SoundEffect>(@"Sounds\MenuEffect");
            hit = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Hit");
            kill = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Kill");
            spawn = StateManager.contentManager.Load<SoundEffect>(@"Sounds\Spawn");

            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.68f;
        }

        #region Play Sound Methods
        public static void PlayMusic()
        {
            MediaPlayer.Play(music);
        }
        public static void PauseMusic()
        {
            MediaPlayer.Pause();
        }
        public static void ResumeMusic()
        {
            MediaPlayer.Resume();
        }
        public static void PlayShoot()
        {
            shoot.Play(0.35f * Volume, (float)rand.Next(-30, 30) / 100.0f, 0);
        }
        public static void PlayShootMine()
        {
            shootMine.Play(0.69f * Volume, 0, 0);
        }
        public static void PlayExplosion()
        {
            explosion.Play(0.69f * Volume, 0, 0);
        }
        public static void PlayHit()
        {
            hit.Play(0.92f * Volume, (float)rand.Next(-14, 14) / 100.0f, 0);
        }
        public static void PlayKill()
        {
            kill.Play(1.0f * Volume, (float)rand.Next(-14, 14) / 100.0f, 0);
        }
        public static void PlaySpawn()
        {
            spawn.Play(0.73f * Volume, 0, 0);
        }
        public static void PlayPowerUp()
        {
            powerUp.Play(1.0f * Volume, 0, 0);
        }
        public static void PlayMenuEffect()
        {
            menuEffect.Play(0.73f * Volume, 0, 0);
        }
        public static void PlaySelection()
        {
            //Decided randomly between 3 sounds
            int randTemp = rand.Next(0, 3);
            if (randTemp == 0)
                selection1.Play(1.0f * Volume, 0, 0);
            else if (randTemp == 1)
                selection2.Play(1.0f * Volume, 0, 0);
            else //if (randTemp == 2)
                selection3.Play(1.0f * Volume, 0, 0);
        }
        #endregion
    }
}
