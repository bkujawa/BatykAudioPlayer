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

        #endregion

        #region Event handlers

        public event EventHandler<SoundEngineEventArgs> StateChanged;
        public event EventHandler<SoundEngineErrorArgs> SoundError;

        #endregion

        #region Constructor

        public SoundEngine()
        {
            this.mediaPlayer = new MediaPlayer();
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
        public void VolumeUp()
        {
            if (this.mediaPlayer.Volume < 1)
            {
                this.mediaPlayer.Volume += 0.1;
            }
        }

        public void VolumeDown()
        {
            if (this.mediaPlayer.Volume > 0)
            {
                this.mediaPlayer.Volume -= 0.1;
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

        #region Private helpers

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
