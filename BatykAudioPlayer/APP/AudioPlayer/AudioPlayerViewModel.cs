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
            this.timer.Interval = TimeSpan.FromSeconds(0.250);
            this.timer.Tick += OnTick;
            this.timer.Start();

            FillSoundsDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void RegisterCommands()
        {
            Play = new RelayCommand(ExecutePlay, CanExecutePlay);
            Pause = new RelayCommand(ExecutePause, CanExecutePause);
            Open = new RelayCommand(ExecuteOpen, CanExecuteOpen);
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

        #region ICommand implementation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
            FillSoundsDirectory(dirPath);
        }

        #endregion

        #region Pivate helper methods

        private void OnTick(object sender, EventArgs s)
        {
            Progress = this.soundEngine.GetFilePosition();
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

        #endregion
    }
}
