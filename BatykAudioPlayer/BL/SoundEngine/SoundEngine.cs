using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public class SoundEngine : ISoundEngine
    {

        #region Private fields

        private readonly MediaPlayer mediaPlayer;
        private SoundState? currentState;
        private string currentPath;
        private double volume;

        #endregion

        #region Properties

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

        #endregion

        #region Event handlers

        public event EventHandler<SoundEngineEventArgs> StateChanged;
        public event EventHandler<SoundEngineErrorArgs> SoundError;

        #endregion

        #region Constructor

        public SoundEngine()
        {
            this.mediaPlayer = new MediaPlayer();
            this.volume = this.mediaPlayer.Volume;
        }

        #endregion

        #region Event handlers methods

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

        #endregion

    }

    public class SoundEngineEventArgs : EventArgs
    {
        public SoundState NewState { get; private set; }
        public SoundEngineEventArgs(SoundState NewState)
        {
            this.NewState = NewState;
        }
    }

    public class SoundEngineErrorArgs
    {
        public string ErrorDetails { get; private set; }
        public SoundEngineErrorArgs(string ErrorDetails)
        {
            this.ErrorDetails = ErrorDetails;
        }
    }
}
