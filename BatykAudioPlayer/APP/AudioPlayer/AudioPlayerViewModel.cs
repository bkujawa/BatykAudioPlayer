using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using BatykAudioPlayer.BL;
using BatykAudioPlayer.BL.SoundEngine;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    class AudioPlayerViewModel : ViewModelBase
    {
        private ISoundEngine soundEngine;
        private SoundState currentSoundState;
        private readonly DispatcherTimer timer;

        #region ICommand commands

        public ICommand Play { get; private set; }
        public ICommand Pause { get; private set; }

        #endregion

        #region Public Properties

        public Sound SelectedSound
        {
            get => SelectedSound;
            set
            {
                SelectedSound = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public AudioPlayerViewModel()
        {
            this.soundEngine = new SoundEngine();
            this.soundEngine.StateChanged += null;
            this.soundEngine.SoundError += null;

            Play = new RelayCommand(null, null);
            Pause = new RelayCommand(null, null);

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(0.5);
            this.timer.Tick += OnTick;
            this.timer.Start();
        }

        private void OnTick(object sender, EventArgs s)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            
        }

        private bool CanExecutePause(object obj)
        {
            return this.currentSoundState == SoundState.Playing;
        }

        private void ExecutePause(object obj)
        {
            this.soundEngine.Pause();
        }

        private bool CanExecutePlay(object obj)
        {
            return (this.currentSoundState != SoundState.Playing && this.currentSoundState != SoundState.Unknown);
        }

        private void ExecutePlay(object obj)
        {
            this.soundEngine.Play(SelectedSound.Path);
        }
    }
}
