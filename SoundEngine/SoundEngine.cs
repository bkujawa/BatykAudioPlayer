using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public class SoundEngineImplementation : ISoundEngine
    {
        #region Private fields

        private MediaPlayer mediaPlayer;
        private SoundState? currentState;
        private string currentPath;
        private double volume;

        #endregion

        #region Properties

        /// <summary>
        /// Represents current volume of sound engine. 
        /// Respects max and min boundaries.
        /// </summary>
        public double Volume
        {
            get => this.volume;
            set
            {
                this.volume = value;
                if (this.volume > 1)
                {
                    this.volume = 1;
                }
                else if (this.volume < 0)
                {
                    this.volume = 0;
                }
                this.mediaPlayer.Volume = volume;
            }
        }

        /// <summary>
        /// Event handler used for MediaEnded event. Releases previous event before adds new event. No reason to explicitly remove events.
        /// </summary>
        public event EventHandler MediaEnded
        {
            add
            {
                if (this.mediaEnded != null)
                {
                    this.mediaPlayer.MediaEnded -= this.mediaEnded;
                    this.mediaEnded -= this.mediaEnded;
                }
                this.mediaEnded += value;
                this.mediaPlayer.MediaEnded += value;
            }
            remove
            {
                this.mediaPlayer.MediaEnded -= value;
                this.mediaEnded -= value;
            }
        }

        #endregion

        #region Event handlers

        public event EventHandler<SoundEngineEventArgs> StateChanged;
        public event EventHandler<SoundEngineErrorArgs> SoundError;
        private event EventHandler mediaEnded;

        #endregion

        #region Event handlers

        private void OnStateChanged(SoundState newState)
        {
            this.currentState = newState;
            StateChanged?.Invoke(this, new SoundEngineEventArgs(newState));
        }

        private void OnError(string error)
        {
            SoundError?.Invoke(this, new SoundEngineErrorArgs(error));
        }

        #endregion

        #region ISoundEngine implementation

        public void Play(string path)
        {
            try
            {
                if (this.currentState != SoundState.Unknown || this.currentState == null)
                {
                    if (this.currentPath == path)
                    {
                        this.mediaPlayer.Play();
                    }
                    else
                    {
                        this.mediaPlayer.Open(new Uri(path));
                        this.currentPath = path;
                        this.mediaPlayer.Play();
                    }
                    OnStateChanged(SoundState.Playing);
                }
            }
            catch (Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        public void Pause()
        {
            try
            {
                this.mediaPlayer.Pause();
                OnStateChanged(SoundState.Paused);
            }
            catch (Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                this.mediaPlayer.Stop();
                OnStateChanged(SoundState.Stopped);
            }
            catch (Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        public void VolumeMute()
        {
            if (this.mediaPlayer.IsMuted)
            {
                this.mediaPlayer.IsMuted = false;
            }
            else
            {
                this.mediaPlayer.IsMuted = true;
            }
        }

        public Tuple<TimeSpan, TimeSpan> GetTimePosition()
        {
            if (this.currentState == null || this.currentState == SoundState.Unknown || this.currentState == SoundState.Stopped)
            {
                return null;
            }
            if (this.mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                return new Tuple<TimeSpan, TimeSpan>(this.mediaPlayer.Position, this.mediaPlayer.NaturalDuration.TimeSpan);
            }
            return null;
        }

        public double GetFilePosition()
        {
            if (this.currentState == null || this.currentState == SoundState.Unknown || this.currentState == SoundState.Stopped)
            {
                return 0;
            }
            if (this.mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                return Math.Min(100, 100 * this.mediaPlayer.Position.TotalSeconds / this.mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            }
            return 0;
        }

        public void Initialize()
        {
            this.mediaPlayer = new MediaPlayer();
            this.volume = this.mediaPlayer.Volume;
        }

        #endregion
    }
}
