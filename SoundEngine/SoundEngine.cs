using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.SoundEngine
{
    // [TODO]: revert changes back to interface implementation
    // think about singleton
    // different viewmodels will need same instance of soundengine
    public class SoundEngine : ISoundEngine
    {
        #region Private fields        
        private readonly MediaPlayer mediaPlayer;
        private static SoundEngine soundEngine;
        private SoundState? currentState;
        private string currentPath;
        private double volume;
        #endregion

        #region Public properties
        // [TODO]: Do we need this property thread-safe?
        public static SoundEngine SoundEngineInstance
        {
            get
            {
                if (soundEngine == null)
                {
                    soundEngine = new SoundEngine();
                }
                return soundEngine;
            }
        }

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
        /// Event handler used for MediaEnded event. 
        /// Releases previous event before adds new event. No reason to explicitly remove events.
        /// </summary>
        public event EventHandler MediaEnded
        {
            add
            {
                if (this.mediaEnded != null)
                {
                    this.mediaPlayer.MediaEnded -= mediaEnded;
                    this.mediaEnded -= mediaEnded;
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
        /// <summary>
        /// Plays sound pointed by path.
        /// </summary>
        /// <param name="path">String with windows-type path to sound file.</param>
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

        /// <summary>
        /// Pause.
        /// </summary>
        public void Pause()
        {
            try
            {
                mediaPlayer.Pause();
                OnStateChanged(SoundState.Paused);
            }
            catch (Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        /// <summary>
        /// Stop. 
        /// </summary>
        public void Stop()
        {
            try
            {
                mediaPlayer.Stop();
                OnStateChanged(SoundState.Stopped);
            }
            catch (Exception ex)
            {
                OnStateChanged(SoundState.Unknown);
                OnError(ex.Message);
            }
        }

        /// <summary>
        /// Either mutes or unmutes MediaPlayer, depending on previous value.
        /// </summary>
        public void VolumeMute()
        {
            if (mediaPlayer.IsMuted)
            {
                mediaPlayer.IsMuted = false;
            }
            else
            {
                mediaPlayer.IsMuted = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Tuple<TimeSpan, TimeSpan> TimePosition()
        {
            if (currentState == null || currentState == SoundState.Unknown || currentState == SoundState.Stopped)
            {
                return null;
            }
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                return new Tuple<TimeSpan, TimeSpan>(mediaPlayer.Position, mediaPlayer.NaturalDuration.TimeSpan);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double FilePosition()
        {
            if (currentState == null || currentState == SoundState.Unknown || currentState == SoundState.Stopped)
            {
                return 0;
            }
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                return Math.Min(100, 100 * mediaPlayer.Position.TotalSeconds / mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            }
            return 0;
        }
        #endregion  
        
        public SoundEngine()
        {
            this.mediaPlayer = new MediaPlayer();
            this.volume = this.mediaPlayer.Volume;
        }
    }
}
