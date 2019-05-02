using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public ICommand Open { get; private set; }

        #endregion

        #region Properties

        public Sound SelectedSound
        {
            get => SelectedSound;
            set
            {
                SelectedSound = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Sound> Sounds { get; private set; }

        #endregion

        public AudioPlayerViewModel()
        {
            this.soundEngine = new SoundEngine();
            this.soundEngine.StateChanged += OnStateChanged;
            this.soundEngine.SoundError += OnSoundError;

            Play = new RelayCommand(ExecutePlay, CanExecutePlay);
            Pause = new RelayCommand(ExecutePause, CanExecutePause);
            Open = new RelayCommand(ExecuteOpen, CanExecuteOpen);

            Sounds = new ObservableCollection<Sound>();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(0.5);
            this.timer.Tick += OnTick;
            this.timer.Start();

            FillSoundsDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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

        private bool CanExecuteOpen(object obj)
        {
            return true;
        }

        private void ExecuteOpen(object obj)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var dirPath = dialog.SelectedPath;
            FillSoundsDirectory(dirPath);
        }

        private void FillSoundsDirectory(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var allFiles = Directory.GetFiles(path);
                var soundList = new List<Sound>();
                foreach(var file in allFiles)
                {
                    var pathExtension = Path.GetExtension(file);
                    if (pathExtension?.ToUpper() == ".MP3")
                    {
                        soundList.Add(new Sound(Path.GetFileNameWithoutExtension(file), file));
                    }
                }
                Sounds.Clear();
                soundList.ForEach(s => Sounds.Add(s));
            }
        }

        private void OnSoundError(object sender, SoundEngineErrorArgs e)
        {
            MessageBox.Show(e.ErrorDetails, "Sound error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnStateChanged(object sender, SoundEngineEventArgs e)
        {
            this.currentSoundState = e.NewState;
            UpdateTime();
        }
    }
}
