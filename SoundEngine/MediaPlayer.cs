using System;
using System.Windows;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public class MediaPlayer : IMediaPlayer
    {
        private System.Windows.Media.MediaPlayer mediaPlayer = new System.Windows.Media.MediaPlayer();

        public bool IsMuted
        {
            get
            {
                return this.mediaPlayer.IsMuted;
            }
            set 
            {
                this.mediaPlayer.IsMuted = value;
            } 
        }
        public Duration NaturalDuration
        {
            get
            {
                return this.mediaPlayer.NaturalDuration;
            }
        }
        public TimeSpan Position
        {
            get
            {
                return this.mediaPlayer.Position;
            }
        }
        public double Volume
        {
            get
            {
                return this.mediaPlayer.Volume;
            }
            set
            {
                this.mediaPlayer.Volume = value;
            }
        }
        public EventHandler MediaEnded 
        {
            set
            {
                this.mediaPlayer.MediaEnded += value;
            }
            get
            {
                return null;
            }
        }

        public void Open(Uri path)
        {
            this.mediaPlayer.Open(path);
        }

        public void Pause()
        {
            this.mediaPlayer.Pause();
        }

        public void Play()
        {
            this.mediaPlayer.Play();
        }

        public void Stop()
        {
            this.mediaPlayer.Stop();
        }
    }
}
