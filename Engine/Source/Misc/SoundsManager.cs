using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace UnrealMono
{
	public struct CustomizedSoundEffect
	{
		private SoundEffect[] soundEffects;
		private float volumeMultiplier;
		private float maxPitchDelta;

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
#if DEBUG
				MediaPlayer.Volume = 0;
#else
				MediaPlayer.Volume = volume * currentSong.volumeMultiplier;
#endif
			}
		}
		
		private static CustomizedSong currentSong;
		private static Dictionary<string, CustomizedSoundEffect> soundEffects;

		public static Dictionary<string, CustomizedSoundEffect> SoundEffects
		{
			get
			{
				return soundEffects;
			}
		}

		public static void Initialize()
		{
			Dictionary<string, Song> musicsContent = UnrealMonoGame.Get.Content.LoadContent<Song>(true, "Music");
			Dictionary<string, SoundEffect> soundEffectsContent = UnrealMonoGame.Get.Content.LoadContent<SoundEffect>(true, "SoundEffect");

			foreach (KeyValuePair<string, Song> thisMusic in musicsContent)
			{
				CustomizedSong music = new CustomizedSong(thisMusic.Value, 0.68F); //TO1 volume
				music.SetAsCurrentSong();
				break;
			}

			soundEffects = new Dictionary<string, CustomizedSoundEffect>();
			foreach (KeyValuePair<string, SoundEffect> thisSoundEffect in soundEffectsContent)
			{
				soundEffects[thisSoundEffect.Key] = new CustomizedSoundEffect(new[] { thisSoundEffect.Value }, 0.4F);
			}

			//TO1
			//shoot = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Shoot") }, 0.35F, 0.3F);
			//shootMine = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\ShootMine") }, 0.69F, 0F);
			//explosion = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Explosion") }, 0.69F, 0F);
			//hit = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Hit") }, 0.92F, 0.14F);
			//kill = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Kill") }, 1F, 0.14F);
			//spawn = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Spawn") }, 0.73F, 0F);
			//powerUp = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\PowerUp") }, 1F, 0F);

			//menuEffect = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\MenuEffect") }, 0.73F, 0F);
			//selection = new CustomizedSoundEffect(new[] { UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Selection1"), UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Selection2"), UnrealMonoGame.Get.Content.Load<SoundEffect>(@"SoundEffect\Selection3") }, 1F, 0F);

			MediaPlayer.Stop();
			MediaPlayer.IsRepeating = true;
		}

		public static void SetAsCurrentSong(this CustomizedSong customizedSong)
		{
			currentSong = customizedSong;
		}
		//TO MOVEEEE
#region Play Sound Methods

		public static void PlayMusic()
		{
#if DEBUG
			MediaPlayer.Volume = 0;
#else
			MediaPlayer.Volume = volume * currentSong.volumeMultiplier;
#endif
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

		public static void PlaySoundEffect(string name)
		{
			soundEffects[name].Play();
		}

		public static void PlaySoundEffect(string name, Vector2 worldLocation)
		{
			soundEffects[name].Play(worldLocation);
		}

#endregion Play Sound Methods
	}
}