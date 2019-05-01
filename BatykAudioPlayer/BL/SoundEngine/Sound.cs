using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Name of sound.</param>
        /// <param name="Path">URI path to sound file.</param>
        public Sound(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }
    }
}
