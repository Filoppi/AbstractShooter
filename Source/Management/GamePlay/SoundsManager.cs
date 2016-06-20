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
        private static bool mute = false;
        public static bool Mute
        {
            get { return mute; }
            set
            {
                mute = value;
                Volume = mute ? 0F : 1F;
            }
        }
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
            music = Game1.Get.Content.Load<Song>(@"Sounds\Music");
            shoot = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Shoot");
            shootMine = Game1.Get.Content.Load<SoundEffect>(@"Sounds\ShootMine");
            explosion = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Explosion");
            selection1 = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection1");
            selection2 = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection2");
            selection3 = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection3");
            powerUp = Game1.Get.Content.Load<SoundEffect>(@"Sounds\PowerUp");
            menuEffect = Game1.Get.Content.Load<SoundEffect>(@"Sounds\MenuEffect");
            hit = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Hit");
            kill = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Kill");
            spawn = Game1.Get.Content.Load<SoundEffect>(@"Sounds\Spawn");

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
            else
                selection3.Play(1.0f * Volume, 0, 0);
        }
        #endregion
    }
}
