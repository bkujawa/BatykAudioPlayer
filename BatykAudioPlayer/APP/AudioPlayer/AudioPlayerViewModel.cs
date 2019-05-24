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
        private AudioPlayerState currentAudioPlayerState;
        private DispatcherTimer timer;
        private Sound selectedSound;
        private Sound currentSound;
        private Sound selectedPlaylist;
        private double progress;
        private string timeInfo;
        private string mute = "Mute";
        Random random;

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

        public string Mute
        {
            get => this.mute;
            set
            {
                this.mute = value;
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
        public ICommand VolumeUp { get; private set; }
        public ICommand VolumeDown { get; private set; }
        public ICommand VolumeMute { get; private set; }
        public ICommand RepeatShuffle { get; private set; }
        public ICommand RepeatSound { get; private set; }
        public ICommand RepeatPlaylist { get; private set; }
        public ICommand RepeatNormal { get; private set; }
        public ICommand SavePlaylist { get; private set; }
        public ICommand OpenPlaylist { get; private set; }

        #endregion

        #region Constructor

        public AudioPlayerViewModel()
        {
            InitializeDependencies();
            InitializePlaylist();
            RegisterCommands();
        }

        #endregion

        #region Event handlers methods

        private void OnSoundError(object sender, SoundEngineErrorArgs e)
        {
            MessageBox.Show(e.ErrorDetails, "Sound error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnSoundStateChanged(object sender, SoundEngineEventArgs e)
        {
            this.currentSoundState = e.NewState;
            UpdateTime();
        }

        private void OnFilePlaylistManagerError(object sender, FilePlaylistManagerErrorArgs e)
        {
            MessageBox.Show(e.ErrorDetails, "FilePlaylistManager error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnFilePlaylistStateChanged (object sender, FilePlaylistManagerEventArgs e)
        {
            RefreshSounds(e.NewSounds, e.Refreshed);
            UpdateTime();
        }

        #endregion

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
            this.currentSound = SelectedSound;
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
            filePlaylistManager.FillSoundsFromDirectory(dirPath);
            filePlaylistManager.SetDefaultDirectory(dirPath);
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
            if (currentAudioPlayerState == AudioPlayerState.Shuffled || currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                return true;
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
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                var randomSound = random.Next(0, Sounds.Count);
                SelectedSound = Sounds[randomSound];
                this.currentSound = SelectedSound;
                this.soundEngine.Play(SelectedSound.Path);
                return;
            }
            var index = Sounds.IndexOf(SelectedSound);
            SelectedSound = Sounds[--index];
            this.soundEngine.Play(SelectedSound.Path);
            this.currentSound = SelectedSound;
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
            if (currentAudioPlayerState == AudioPlayerState.Shuffled || currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                return true;
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
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                var randomSound = random.Next(0, Sounds.Count);
                SelectedSound = Sounds[randomSound];
                this.currentSound = SelectedSound;
                this.soundEngine.Play(SelectedSound.Path);
                return;
            }
            var index = Sounds.IndexOf(SelectedSound);
            SelectedSound = Sounds[++index];
            this.soundEngine.Play(SelectedSound.Path);
            this.currentSound = SelectedSound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteVolumeUp(object obj)
        {
            if (this.soundEngine.Volume < 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteVolumeUp(object obj)
        {
            this.soundEngine.Volume += 0.1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteVolumeDown(object obj)
        {
            if (this.soundEngine.Volume > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteVolumeDown(object obj)
        {
            soundEngine.Volume -= 0.1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExecuteVolumeMute(object obj)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteVolumeMute(object obj)
        {
            this.soundEngine.VolumeMute();
            if (Mute.Equals("Mute"))
            {
                Mute = "Unmute";
            }
            else if (Mute.Equals("Unmute"))
            {
                Mute = "Mute";
            }
        }

        private bool CanExecuteRepeatShuffle(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                return false;
            }
            return true;
        }

        private void ExecuteRepeatShuffle(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatShuffled);
            this.currentAudioPlayerState = AudioPlayerState.Shuffled;
        }

        private bool CanExecuteRepeatSound(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.RepeatSound)
            {
                return false;
            }
            return true;
        }

        private void ExecuteRepeatSound(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatSound);
            this.currentAudioPlayerState = AudioPlayerState.RepeatSound;
        }

        private bool CanExecuteRepeatPlaylist(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                return false;
            }
            return true;
        }

        private void ExecuteRepeatPlaylist(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatPlaylist);
            this.currentAudioPlayerState = AudioPlayerState.RepeatPlaylist;
        }

        private bool CanExecuteRepeatNormal(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.Normal)
            {
                return false;
            }
            return true;
        }

        private void ExecuteRepeatNormal(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatNormal);
            this.currentAudioPlayerState = AudioPlayerState.Normal;
        }

        private bool CanSavePlaylist(object obj)
        {
            return true;
        }

        private void ExecuteSavePlaylist(object obj)
        {
            string playlistName = "temp.txt";
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioPlayer");
            Directory.CreateDirectory(docPath);
            using (StreamWriter sr = new StreamWriter(Path.Combine(docPath, playlistName)))
            {
                foreach (var sound in Sounds)
                {
                    sr.WriteLine(sound.Name);
                    sr.WriteLine(sound.Path);
                    sr.WriteLine(sound.Time);
                }
            }
        }

        private bool CanOpenPlaylist(object obj)
        {
            return SelectedPlaylist != null;
        }

        private void ExecuteOpenPlaylist(object obj)
        {
            if (!string.IsNullOrEmpty(SelectedPlaylist.Path))
            {
                var soundList = new List<Sound>();
                var allFiles = File.ReadAllLines(SelectedPlaylist.Path);
                for (int i = 0; i < allFiles.Length - 1; i = i + 2)
                {
                    soundList.Add(new Sound(allFiles[i], allFiles[i + 1], allFiles[i + 2]));
                }
                RefreshSounds(soundList);
            }
        }

        #endregion

        #region Pivate helper methods

        private void RegisterCommands()
        {
            Play = new RelayCommand(ExecutePlay, CanExecutePlay);
            Pause = new RelayCommand(ExecutePause, CanExecutePause);
            Stop = new RelayCommand(ExecuteStop, CanExecuteStop);
            Open = new RelayCommand(ExecuteOpen, CanExecuteOpen);
            PlayPrevious = new RelayCommand(ExecutePlayPrevious, CanExecutePlayPrevious);
            PlayNext = new RelayCommand(ExecutePlayNext, CanExecutePlayNext);
            VolumeUp = new RelayCommand(ExecuteVolumeUp, CanExecuteVolumeUp);
            VolumeDown = new RelayCommand(ExecuteVolumeDown, CanExecuteVolumeDown);
            VolumeMute = new RelayCommand(ExecuteVolumeMute, CanExecuteVolumeMute);
            RepeatShuffle = new RelayCommand(ExecuteRepeatShuffle, CanExecuteRepeatShuffle);
            RepeatSound = new RelayCommand(ExecuteRepeatSound, CanExecuteRepeatSound);
            RepeatPlaylist = new RelayCommand(ExecuteRepeatPlaylist, CanExecuteRepeatPlaylist);
            RepeatNormal = new RelayCommand(ExecuteRepeatNormal, CanExecuteRepeatNormal);
            SavePlaylist = new RelayCommand(ExecuteSavePlaylist, CanSavePlaylist);
            OpenPlaylist = new RelayCommand(ExecuteOpenPlaylist, CanOpenPlaylist);
        }

        private void OnTick(object sender, EventArgs s)
        {
            UpdateProgress();
            UpdateTime();
        }

        private void UpdateTime()
        {
            var time = soundEngine.GetTimePosition();
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

        private void RefreshSounds(List<Sound> sounds, CollectionRefreshed refreshed = CollectionRefreshed.Sounds)
        {
            if (sounds == null)
            {
                return;
            }
            if (refreshed == CollectionRefreshed.Sounds)
            {
                Sounds.Clear();
                sounds.ForEach(s => Sounds.Add(s));
            }
            else
            {
                Playlists.Clear();
                sounds.ForEach(p => Playlists.Add(p));
            }
        }

        private void InitializePlaylist()
        {
            if (filePlaylistManager.CheckIfDefaultDirectoryIsSet())
            {
                filePlaylistManager.FillSoundsFromDefaultDirectory();
            }
            else
            {
                filePlaylistManager.SetDefaultDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                filePlaylistManager.FillSoundsFromDefaultDirectory();               
            }
            filePlaylistManager.FillPlaylistsFromDefaultDirectory();
        }

        private void InitializeDependencies()
        {
            this.soundEngine = new SoundEngine();
            this.soundEngine.StateChanged += OnSoundStateChanged;
            this.soundEngine.SoundError += OnSoundError;
            this.soundEngine.MediaEnded += NextSoundRepeatNormal;
            this.currentAudioPlayerState = AudioPlayerState.Normal;

            this.filePlaylistManager = new FilePlaylistManager();
            this.filePlaylistManager.StateChanged += OnFilePlaylistStateChanged;
            this.filePlaylistManager.FilePlaylistError += OnFilePlaylistManagerError;

            Sounds = new ObservableCollection<Sound>();
            Playlists = new ObservableCollection<Sound>();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(0.1);
            this.timer.Tick += OnTick;
            this.timer.Start();

            random = new Random();
        }

        #endregion

        #region MediaPlayer.MediaEnded event handlers

        private void NextSoundRepeatNormal(object sender, EventArgs e)
        {
            var index = Sounds.IndexOf(this.currentSound);
            if (index < Sounds.Count - 1)
            {
                this.currentSound = Sounds[++index];
                SelectedSound = this.currentSound;
                this.soundEngine.Stop();
                this.soundEngine.Play(this.currentSound.Path);
            }
        }

        private void NextSoundRepeatShuffled(object sender, EventArgs e)
        {
            var randomSound = random.Next(0, Sounds.Count);
            this.currentSound = Sounds[randomSound];
            SelectedSound = this.currentSound;
            this.soundEngine.Stop();
            this.soundEngine.Play(currentSound.Path);
        }

        private void NextSoundRepeatSound(object sender, EventArgs e)
        {
            this.soundEngine.Stop();
            this.soundEngine.Play(this.currentSound.Path);
        }

        private void NextSoundRepeatPlaylist(object sender, EventArgs e)
        {
            var index = Sounds.IndexOf(this.currentSound);
            if (index < Sounds.Count - 1)
            {
                this.currentSound = Sounds[++index];
                this.soundEngine.Stop();
                this.soundEngine.Play(this.currentSound.Path);
            }
            else
            {
                this.currentSound = Sounds[0];
                this.soundEngine.Stop();
                this.soundEngine.Play(this.currentSound.Path);
            }
            SelectedSound = this.currentSound;
        }

        private void SetMediaEndedEvent(EventHandler eventHandler)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.Normal)
            {
                this.soundEngine.MediaEnded -= NextSoundRepeatNormal;
            }
            else if (this.currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                this.soundEngine.MediaEnded -= NextSoundRepeatPlaylist;
            }
            else if (this.currentAudioPlayerState == AudioPlayerState.RepeatSound)
            {
                this.soundEngine.MediaEnded -= NextSoundRepeatSound;
            }
            else if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                this.soundEngine.MediaEnded -= NextSoundRepeatShuffled;
            }
            this.soundEngine.MediaEnded += eventHandler;          
        }

        #endregion
    }
}
