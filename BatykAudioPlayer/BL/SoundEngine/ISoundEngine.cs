using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngine
{
    interface ISoundEngine
    {
        void Play(string path);
        void Stop();
        void Pause();
        void VolumeUp();
        void VolumeDown();
        void VolumeMute();
    }
}
