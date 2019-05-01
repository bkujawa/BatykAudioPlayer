using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngine
{
    /// <summary>
    /// Enum representation of <see cref="SoundEngine"/> (<see cref="System.Windows.Media.MediaPlayer"/>) state.
    /// </summary>
    enum SoundState
    {
        Unknown,
        Playing,
        Paused,
        Stopped
    }
}
