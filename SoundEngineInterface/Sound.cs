using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.SoundEngineInterface
{
    /// <summary>
    /// Class representing Sound properties
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Path of file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Duration of media file.
        /// </summary>
        public string Time { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Name of sound.</param>
        /// <param name="Path">URI path to sound file.</param>
        /// <param name="Time">Duration of media file.</param>
        public Sound(string Name, string Path, string Time = null)
        {
            this.Name = Name;
            this.Path = Path;
            this.Time = Time;
        }
    }
}
