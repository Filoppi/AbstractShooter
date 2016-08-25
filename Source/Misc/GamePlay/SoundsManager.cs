using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace AbstractShooter
{
	public struct CustomizedSoundEffect
	{
		public SoundEffect[] soundEffects;
		public float volumeMultiplier;
		public float maxPitchDelta;

		public CustomizedSoundEffect(SoundEffect[] soundEffects, float volumeMultiplier = 1F, float maxPitchDelta = 0F)
		{
			this.soundEffects = soundEffects;
			this.volumeMultiplier = volumeMultiplier;
			this.maxPitchDelta = maxPitchDelta;
		}

		public void Play()
		{
			soundEffects[MathExtention.Rand.Next(0, soundEffects.Length)].Play(SoundsManager.Volume * volumeMultiplier, maxPitchDelta * (float)((MathExtention.Rand.NextDouble() * 2.0) - 1.0), 0);
		}

		public void Play(Vector2 worldLocation)
		{
			soundEffects[MathExtention.Rand.Next(0, soundEffects.Length)].Play(SoundsManager.Volume * volumeMultiplier, maxPitchDelta * (float)((MathExtention.Rand.NextDouble() * 2.0) - 1.0), (Camera.GetNormalizedViewPortAlpha(worldLocation).X * 0.5F).GetClamped(-1F, 1F));
		}
	}

	public struct CustomizedSong
	{
		public Song song;
		public float volumeMultiplier;

		public CustomizedSong(Song song, float volumeMultiplier = 1F)
		{
			this.song = song;
			this.volumeMultiplier = volumeMultiplier;
		}
	}

	public static class SoundsManager
	{
		private static bool mute;

		public static bool Mute
		{
			get { return mute; }
			set
			{
				if (!mute && value)
				{
					volumeBeforeMute = volume;
				}
				mute = value;
				Volume = mute ? 0F : volumeBeforeMute;
			}
		}

		private static float volume = 1F;
		private static float volumeBeforeMute = 1F;

		public static float Volume
		{
			get { return volume; }
			set
			{
				volume = value;
				MediaPlayer.Volume = volume * currentSong.volumeMultiplier;
			}
		}

		private static CustomizedSong currentSong;

		//Custom
		private static CustomizedSong music;

		private static CustomizedSoundEffect shoot;
		private static CustomizedSoundEffect shootMine;
		private static CustomizedSoundEffect explosion;
		private static CustomizedSoundEffect hit;
		private static CustomizedSoundEffect kill;
		private static CustomizedSoundEffect spawn;
		private static CustomizedSoundEffect powerUp;
		private static CustomizedSoundEffect menuEffect;
		private static CustomizedSoundEffect selection;

		public static void Initialize()
		{
			music = new CustomizedSong(Game1.Get.Content.Load<Song>(@"Sounds\Music"), 0.68F);
			music.SetAsCurrentSong();

			shoot = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Shoot") }, 0.35F, 0.3F);
			shootMine = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\ShootMine") }, 0.69F, 0F);
			explosion = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Explosion") }, 0.69F, 0F);
			hit = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Hit") }, 0.92F, 0.14F);
			kill = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Kill") }, 1F, 0.14F);
			spawn = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Spawn") }, 0.73F, 0F);
			powerUp = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\PowerUp") }, 1F, 0F);

			menuEffect = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\MenuEffect") }, 0.73F, 0F);
			selection = new CustomizedSoundEffect(new[] { Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection1"), Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection2"), Game1.Get.Content.Load<SoundEffect>(@"Sounds\Selection3") }, 1F, 0F);

			MediaPlayer.Stop();
			MediaPlayer.IsRepeating = true;
		}

		public static void SetAsCurrentSong(this CustomizedSong customizedSong)
		{
			currentSong = customizedSong;
		}

		#region Play Sound Methods

		public static void PlayMusic()
		{
			MediaPlayer.Volume = volume * currentSong.volumeMultiplier;
			MediaPlayer.Play(currentSong.song);
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
			shoot.Play();
		}

		public static void PlayShootMine()
		{
			shootMine.Play();
		}

		public static void PlayExplosion(Vector2 worldLocation)
		{
			explosion.Play(worldLocation);
		}

		public static void PlayHit(Vector2 worldLocation)
		{
			hit.Play(worldLocation);
		}

		public static void PlayKill(Vector2 worldLocation)
		{
			kill.Play(worldLocation);
		}

		public static void PlaySpawn()
		{
			spawn.Play();
		}

		public static void PlayPowerUp()
		{
			powerUp.Play();
		}

		public static void PlayMenuEffect()
		{
			menuEffect.Play();
		}

		public static void PlaySelection()
		{
			selection.Play();
		}

		#endregion Play Sound Methods
	}
}