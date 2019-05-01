using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public class Sound
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public Sound(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }
    }
}
