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
using BatykAudioPlayer.BL.FilePlaylistManager;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    class AudioPlayerViewModel : ViewModelBase
    {
        #region Private fields

        private ISoundEngine soundEngine;
        private IFilePlaylistManager filePlaylistManager;
        private SoundState? currentSoundState;
        private DispatcherTimer timer;
        private Sound selectedSound;
        private Sound selectedPlaylist;
        private double progress;
        private string timeInfo;

        #endregion

        #region Properties

        public Sound SelectedSound
        {
            get => this.selectedSound;
            set
            {
                this.selectedSound = value;
                OnPropertyChanged();
            }
        }

        public Sound SelectedPlaylist
        {
            get => this.selectedPlaylist;
            set
            {
                this.selectedPlaylist = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get => this.progress;
            set
            {
                this.progress = value;
                OnPropertyChanged();
            }
        }

        public string TimeInfo
        {
            get => this.timeInfo;
            set
            {
                this.timeInfo = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Sound> Sounds
        {
            get;
            private set;
        }

        public ObservableCollection<Sound> Playlists
        {
            get;
            private set;
        }

        #endregion

        #region ICommand

        public ICommand Play { get; private set; }
        public ICommand Pause { get; private set; }
        public ICommand Open { get; private set; }
        public ICommand Stop { get; private set; }
        public ICommand PlayNext { get; private set; }
        public ICommand PlayPrevious { get; private set; }

        #endregion

        public AudioPlayerViewModel()
        {
            this.soundEngine = new SoundEngine();
            this.soundEngine.StateChanged += OnStateChanged;
            this.soundEngine.SoundError += OnSoundError;
            this.filePlaylistManager = new FilePlaylistManager();

            RegisterCommands();

            Sounds = new ObservableCollection<Sound>();
            Playlists = new ObservableCollection<Sound>();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(0.1);
            this.timer.Tick += OnTick;
            this.timer.Start();

            this.filePlaylistManager.FillSoundsFromDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void RegisterCommands()
        {
            Play = new RelayCommand(ExecutePlay, CanExecutePlay);
            Pause = new RelayCommand(ExecutePause, CanExecutePause);
            Stop = new RelayCommand(ExecuteStop, CanExecuteStop);
            Open = new RelayCommand(ExecuteOpen, CanExecuteOpen);
            PlayPrevious = new RelayCommand(ExecutePlayPrevious, CanExecutePlayPrevious);
            PlayNext = new RelayCommand(ExecutePlayNext, CanExecutePlayNext);
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

        #region ICommand implementation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecutePlay(object obj)
        {
            return this.SelectedSound != null && (this.currentSoundState != SoundState.Unknown || this.currentSoundState == null);
        }

        /// <summary>
        /// Play <see cref="SelectedSound"/>.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePlay(object obj)
        {
            this.soundEngine.Play(SelectedSound.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if <see cref="currentSoundState"/> is set on <see cref="SoundState.Playing"/></returns>
        private bool CanExecutePause(object obj)
        {
            return this.currentSoundState == SoundState.Playing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePause(object obj)
        {
            this.soundEngine.Pause();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteStop(object obj)
        {
            return this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteStop(object obj)
        {
            this.soundEngine.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteOpen(object obj)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteOpen(object obj)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var dirPath = dialog.SelectedPath;
            var sounds = this.filePlaylistManager.FillSoundsFromDirectory(dirPath);
            RefreshPlaylist(sounds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecutePlayPrevious(object obj)
        {
            if (SelectedSound == null)
            {
                return false;
            }
            var index = Sounds.IndexOf(SelectedSound);
            return index > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePlayPrevious(object obj)
        {
            this.soundEngine.Stop();
            var index = Sounds.IndexOf(SelectedSound);
            SelectedSound = Sounds[--index];
            this.soundEngine.Play(SelectedSound.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecutePlayNext(object obj)
        {
            if (SelectedSound == null)
            {
                return false;
            }
            var index = Sounds.IndexOf(SelectedSound);
            return index < Sounds.Count - 1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        private void ExecutePlayNext(object obj)
        {
            this.soundEngine.Stop();
            var index = Sounds.IndexOf(SelectedSound);
            SelectedSound = Sounds[++index];
            this.soundEngine.Play(SelectedSound.Path);
        }
        #endregion

        #region Pivate helper methods

        private void OnTick(object sender, EventArgs s)
        {
            UpdateProgress();
            UpdateTime();
        }

        private void UpdateTime()
        {
            var time = this.soundEngine.GetTimePosition();
            if (time == null)
            {
                TimeInfo = "--/--";
            }
            else
            {
                TimeInfo = $"{time.Item1.ToString(@"hh\:mm\:ss")} / {time.Item2.ToString(@"hh\:mm\:ss")}";
            }
        }

        private void UpdateProgress()
        {
            Progress = this.soundEngine.GetFilePosition();
        }

        private void RefreshPlaylist(List<Sound> sounds)
        {
            Sounds.Clear();
            sounds.ForEach(s => Sounds.Add(s));
        }

        #endregion
    }
}
