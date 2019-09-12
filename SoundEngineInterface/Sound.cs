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
        /// 
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name { get;  set; }

        /// <summary>
        /// Path of file.
        /// </summary>
        public string Path { get;  set; }

        /// <summary>
        /// Duration of media file.
        /// </summary>
        public string Time { get;  set; }

        /// <summary>
        /// 
        /// </summary>
        public Sound()
        {
        }
    }
}
