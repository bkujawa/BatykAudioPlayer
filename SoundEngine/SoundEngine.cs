using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public static class SoundEngine
    {
        #region Private fields

        private static MediaPlayer mediaPlayer;
        private static SoundState? currentState;
        private static string currentPath;
        private static double volume;

        #endregion

        #region Public properties

        /// <summary>
        /// Represents current volume of sound engine. 
        /// Respects max and min boundaries.
        /// </summary>
        public static double Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (volume > 1)
                {
                    volume = 1;
                }
                else if (volume < 0)
                {
                    volume = 0;
                }
                mediaPlayer.Volume = volume;
            }
        }

        /// <summary>
        /// Event handler used for MediaEnded event. 
        /// Releases previous event before adds new event. No reason to explicitly remove events.
        /// </summary>
        public static event EventHandler MediaEnded
        {
            add
            {
                if (mediaEnded != null)
                {
                    mediaPlayer.MediaEnded -= mediaEnded;
                    mediaEnded -= mediaEnded;
                }
                mediaEnded += value;
                mediaPlayer.MediaEnded += value;
            }
            remove
            {
                mediaPlayer.MediaEnded -= value;
                mediaEnded -= value;
            }
        }

        #endregion

        #region Event handlers

        public static event EventHandler<SoundEngineEventArgs> StateChanged;
        public static event EventHandler<SoundEngineErrorArgs> SoundError;
        private static event EventHandler mediaEnded;

        #endregion

        #region Event handlers

        private static void OnStateChanged(SoundState newState)
        {
            currentState = newState;
            StateChanged?.Invoke(null, new SoundEngineEventArgs(newState));
        }

        private static void OnError(string error)
        {
            SoundError?.Invoke(null, new SoundEngineErrorArgs(error));
        }

        #endregion

        #region ISoundEngine implementation

        /// <summary>
        /// Plays sound pointed by path.
        /// </summary>
        /// <param name="path">String with windows-type path to sound file.</param>
        public static void Play(string path)
        {
            try
            {
                if (currentState != SoundState.Unknown || currentState == null)
                {
                    if (currentPath == path)
                    {
                        mediaPlayer.Play();
                    }
                    else
                    {
                        mediaPlayer.Open(new Uri(path));
                        currentPath = path;
                        mediaPlayer.Play();
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
        public static void Pause()
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
        public static void Stop()
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
        public static void VolumeMute()
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
        public static Tuple<TimeSpan, TimeSpan> GetTimePosition()
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
        public static double GetFilePosition()
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

        /// <summary>
        /// Creates instance of MediaPlayer and sets volume.
        /// </summary>
        public static void Initialize()
        {
            mediaPlayer = new MediaPlayer();
            volume = mediaPlayer.Volume;
        }
        
        static SoundEngine()
        {
            mediaPlayer = new MediaPlayer();
            volume = mediaPlayer.Volume;
        }

        #endregion
    }
}
