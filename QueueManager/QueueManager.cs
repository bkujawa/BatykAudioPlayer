using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.QueueManagerInterface;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.QueueManager
{
    public class QueueManager : IQueueManager
    {
        private List<Sound> queuedSounds;

        public void Enqueue(Sound sound)
        {
            queuedSounds.Add(sound);
        }

        public Sound Dequeue()
        {
            var sound = queuedSounds[0];
            queuedSounds.RemoveAt(0);
            return sound;
        }

        public void Change(int index, Sound sound)
        {
            queuedSounds.Insert(index, sound);
        }

        public bool IsQueueEmpty => this.queuedSounds.Any() ? false : true;
    }
}
