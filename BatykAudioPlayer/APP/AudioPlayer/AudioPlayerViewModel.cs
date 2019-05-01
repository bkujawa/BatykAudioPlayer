using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.SoundEngine;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    class AudioPlayerViewModel : ViewModelBase
    {
        public SoundEngine soundEngine;
        public SoundState currentSoundState;
        public string currentSoundPath;
    }
}
