using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngineInterface
{
    public interface ISoundEngine
    {
        /// <summary>
        /// Plays sound pointed by path.
        /// </summary>
        /// <param name="path">String with windows-type path to sound file.</param>
        void Play(string path);

        /// <summary>
        /// Pause.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop.
        /// </summary>
        void Stop();

        void VolumeMute();

        Tuple<TimeSpan, TimeSpan> TimePosition();

        double FilePosition();

        event EventHandler<SoundEngineEventArgs> StateChanged;
        event EventHandler<SoundEngineErrorArgs> SoundError;
        event EventHandler MediaEnded;
        double Volume { get; set; }

    }
}
