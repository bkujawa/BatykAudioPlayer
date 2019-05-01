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
        private SoundState CurrentState;

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play(string path)
        {
            throw new NotImplementedException();
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
    }
}
