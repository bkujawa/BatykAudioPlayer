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
        private readonly MediaPlayer mediaPlayer;
        private SoundState? currentState;
        private string currentPath;

        public event EventHandler<SoundEngineEventArgs> StateChanged;
        public event EventHandler<SoundEngineErrorArgs> SoundError;

        public SoundEngine()
        {
            this.mediaPlayer = new MediaPlayer();
        }

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
            catch(Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void VolumeDown()
        {
            throw new NotImplementedException();
        }

        public void VolumeMute()
        {
            throw new NotImplementedException();
        }

        public void VolumeUp()
        {
            throw new NotImplementedException();
        }

        private void OnStateChanged(SoundState newState)
        {
            this.currentState = newState;
            StateChanged?.Invoke(this, new SoundEngineEventArgs(newState));
        }

        private void OnError(string error)
        {
            SoundError?.Invoke(this, new SoundEngineErrorArgs(error));
        }
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
