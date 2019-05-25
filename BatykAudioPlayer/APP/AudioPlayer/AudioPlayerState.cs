using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    /// <summary>
    /// Represents state of audio player.
    /// </summary>
    public enum AudioPlayerState
    {
        Normal,
        Shuffled,
        RepeatSound,
        RepeatPlaylist
    }
}
