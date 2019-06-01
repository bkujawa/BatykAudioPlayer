using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngineInterface
{
    /// <summary>
    /// Interface implemented in <see cref="SoundEngine"/>.
    /// </summary>
    public interface ISoundEngine
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        void Play(string path);

        /// <summary>
        /// 
        /// </summary>
        void Stop();

        /// <summary>
        /// 
        /// </summary>
        void Pause();

        /// <summary>
        /// 
        /// </summary>
        void VolumeMute();

        /// <summary>
        /// 
        /// </summary>
        /// <returns><see cref="Tuple"/> of <see cref="TimeSpan"/> containing T1 actual time and T2 total time of sound.</returns>
        Tuple<TimeSpan, TimeSpan> GetTimePosition();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Double value between 0-100 representing % of actual time in sound.</returns>
        double GetFilePosition();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventHandler"></param>
        event EventHandler MediaEnded;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<SoundEngineEventArgs> StateChanged;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<SoundEngineErrorArgs> SoundError;

        /// <summary>
        /// 
        /// </summary>
        double Volume { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        void Initialize();
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
