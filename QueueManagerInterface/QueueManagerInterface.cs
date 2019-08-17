using BatykAudioPlayer.BL.SoundEngineInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.QueueManagerInterface
{
    public interface IQueueManager
    {
        /// <summary>
        /// Adds new sound to queue.
        /// </summary>
        /// <param name="sound"></param>
        void Enqueue(Sound sound);

        /// <summary>
        /// Returns next sound from queue.
        /// </summary>
        /// <returns></returns>
        Sound Dequeue();

        /// <summary>
        /// Inserts specific sound at specific index in queue.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sound"></param>
        void Change(int index, Sound sound);

        /// <summary>
        /// Returns true if queue is empty.
        /// </summary>
        bool IsQueueEmpty { get; }
    }
}
