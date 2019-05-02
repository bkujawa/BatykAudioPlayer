using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngine
{
    /// <summary>
    /// Interface implemented in <see cref="SoundEngine"/>.
    /// </summary>
    interface ISoundEngine
    {
        void Play(string path);
        void Stop();
        void Pause();
        void VolumeUp();
        void VolumeDown();
        void VolumeMute();
        event EventHandler<SoundEngineEventArgs> StateChanged;
        event EventHandler<SoundEngineErrorArgs> SoundError;
    }
}
