using System;
using System.Windows;

namespace BatykAudioPlayer.BL.SoundEngine
{
    public interface IMediaPlayer
    {
        void Play();

        void Open(Uri path);

        void Pause();

        void Stop();

        bool IsMuted { get; set; }

        Duration NaturalDuration { get; }

        TimeSpan Position { get; }

        double Volume { get; set; }

        EventHandler MediaEnded { get; set; }
    }
}
