using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.SoundEngine;
using BatykAudioPlayer.BL.SoundEngineInterface;
using BatykAudioPlayer.BL.FileManager;
using BatykAudioPlayer.BL.FileManagerInterface;
using BatykAudioPlayer.APP.ViewModel;
using BatykAudioPlayer.BL.RelayCommand;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    // [TODO]: Split viewmodel into several viewmodels dealing with less functionality
    // i.e. playlistviemodel - soundlistviewmodel - reuse?, toolbarviewmodel?, mediaplayeroptionsviewmodel?, 
    class AudioPlayerViewModel : ViewModelBase
    {
        #region Private fields
        
        private IFileManager fileManager;
        private SoundState? currentSoundState;
        private AudioPlayerState currentAudioPlayerState;
        private DispatcherTimer timer;
        private SoundEngine soundEngine;
        private Sound selectedSound;
        private Sound currentSound;
        private Sound selectedPlaylist;
        private List<Sound> notPlayedSounds;
        private double progress;
        private string timeInfo;
        private string mute = "Mute";
        private string savedPlaylistName;
        private delegate void FillSoundsFromDirectory(string dir);
        Random random;

        #endregion

        #region Properties

        /// <summary>
        /// Represents sound selected by user in list box.
        /// </summary>
        public Sound SelectedSound
        {
            get => this.selectedSound;
            set
            {
                this.selectedSound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Represents current playing sound.
        /// </summary>
        public Sound CurrentSound
        {
            get => this.currentSound;
            set
            {
                this.currentSound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Represents playlist selected by user in list box.
        /// </summary>
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

        /// <summary>
        /// Property used in save playlist textbox. 
        /// </summary>
        public string SavedPlaylistName
        {
            get => this.savedPlaylistName;
            set
            {
                this.savedPlaylistName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Observable collection of Sound objects, represents data for current music list box.
        /// </summary>
        public ObservableCollection<Sound> Sounds
        {
            get;
            private set;
        }

        /// <summary>
        /// Observable collection of Sound objects, represents data for current playlists list box.
        /// </summary>
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
        public ICommand DeletePlaylist { get; private set; }

        #endregion

        #region Constructor

        public AudioPlayerViewModel()
        {
            InitializeManagers();
            InitializeSoundlistFromDefaultPlaylist();
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

        private void OnFileManagerError(object sender, FileManagerErrorArgs e)
        {
            MessageBox.Show(e.ErrorDetails, "FilePlaylistManager error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Refresh current list of sounds or playlists.
        /// </summary>
        /// <param name="e">Contains information about new list of sounds, and where they should be applied.</param>
        private void OnFileManagerStateChanged(object sender, FileManagerEventArgs e)
        {
            if (e.Refreshed == CollectionRefreshed.Sounds)
            {
                RefreshSounds(e.NewSounds);
            }
            else
            {
                RefreshPlaylists(e.NewSounds);
            }
            UpdateTime();
        }

        #endregion

        #region ICommand implementation

        /// <summary>
        /// Returns true if <see cref="currentSoundState"/> is not Unknown and SelectedSound is not null.
        /// </summary>
        private bool CanExecutePlay(object obj)
        {
            return SelectedSound != null && (this.currentSoundState != SoundState.Unknown || this.currentSoundState == null);
        }

        /// <summary>
        /// Play <see cref="SelectedSound"/>.
        /// </summary>
        private void ExecutePlay(object obj)
        {
            this.soundEngine.Play(SelectedSound.Path);
            CurrentSound = SelectedSound;
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                this.notPlayedSounds.Remove(CurrentSound);
            }
        }

        /// <summary>
        /// Returns true if <see cref="currentSoundState"/> is Playing.
        /// </summary>
        private bool CanExecutePause(object obj)
        {
            return this.currentSoundState == SoundState.Playing;
        }

        /// <summary>
        /// Pauses playing.
        /// </summary>
        private void ExecutePause(object obj)
        {
            this.soundEngine.Pause();
        }

        /// <summary>
        /// Returns true if <see cref="currentSoundState"/> is either Playing or Paused.
        /// </summary>
        private bool CanExecuteStop(object obj)
        {
            return this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused;
        }

        /// <summary>
        /// Stops playing.
        /// </summary>
        private void ExecuteStop(object obj)
        {
            this.soundEngine.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteOpen(object obj)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteOpen(object obj)
        {
            Sounds.Clear();
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var dirPath = dialog.SelectedPath;

            var fillSoundFromDirectory = new FillSoundsFromDirectory(this.fileManager.FillSoundsFromDirectory);
            fillSoundFromDirectory.BeginInvoke(dirPath, null, null);

            //this.fileManager.FillSoundsFromDirectory(dirPath);
            this.fileManager.SetDefaultDirectory(dirPath);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecutePlayPrevious(object obj)
        {
            if (SelectedSound == null || CurrentSound == null)
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
        private void ExecutePlayPrevious(object obj)
        {
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }
            if (this.currentSoundState == SoundState.Unknown)
            {
                // What to do?
                return;
            }
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                NextSoundRepeatShuffled(null, null);
                return;
            }
            else if (this.currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                //var indexOf = Sounds.IndexOf(CurrentSound);
                //if (indexOf == 0)
                //{
                //    SelectedSound = Sounds[Sounds.Count - 1];
                //    CurrentSound = SelectedSound;
                //    this.soundEngine.Play(SelectedSound.Path);
                //    return;
                //}
                PreviousSoundRepeatPlaylist(null, null);
                return;
            }
            PreviousSoundRepeatNormal(null, null);
            //var index = Sounds.IndexOf(CurrentSound);
            //SelectedSound = Sounds[--index];
            //CurrentSound = SelectedSound;
            //this.soundEngine.Play(SelectedSound.Path);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecutePlayNext(object obj)
        {
            if (SelectedSound == null || CurrentSound == null)
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
        private void ExecutePlayNext(object obj)
        {
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                NextSoundRepeatShuffled(null, null);
                return;
            }
            if (this.currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                NextSoundRepeatPlaylist(null, null);
                return;
            }
            NextSoundRepeatNormal(null, null);
            //var index = Sounds.IndexOf(CurrentSound);
            //SelectedSound = Sounds[++index];
            //CurrentSound = SelectedSound;
            //this.soundEngine.Play(SelectedSound.Path);
        }

        /// <summary>
        /// 
        /// </summary>
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
        private void ExecuteVolumeUp(object obj)
        {
            this.soundEngine.Volume += 0.1;
        }

        /// <summary>
        /// 
        /// </summary>
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
        private void ExecuteVolumeDown(object obj)
        {
            this.soundEngine.Volume -= 0.1;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteVolumeMute(object obj)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteRepeatShuffle(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteRepeatShuffle(object obj)
        {
            this.notPlayedSounds = Sounds.ToList();
            SetMediaEndedEvent(NextSoundRepeatShuffled, AudioPlayerState.Shuffled);
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.notPlayedSounds.Remove(CurrentSound);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteRepeatSound(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.RepeatSound)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteRepeatSound(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatSound, AudioPlayerState.RepeatSound);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteRepeatPlaylist(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.RepeatPlaylist)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteRepeatPlaylist(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatPlaylist, AudioPlayerState.RepeatPlaylist);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanExecuteRepeatNormal(object obj)
        {
            if (this.currentAudioPlayerState == AudioPlayerState.Normal)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteRepeatNormal(object obj)
        {
            SetMediaEndedEvent(NextSoundRepeatNormal, AudioPlayerState.Normal);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanSavePlaylist(object obj)
        {
            return true;
        }

        private void ExecuteSavePlaylist(object obj)
        {
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioPlayer");
            Directory.CreateDirectory(docPath);
            using (StreamWriter sr = new StreamWriter(Path.Combine(docPath, SavedPlaylistName + ".txt")))
            {
                foreach (var sound in Sounds)
                {
                    sr.WriteLine(sound.Name);
                    sr.WriteLine(sound.Path);
                    sr.WriteLine(sound.Time);
                }
            }
            this.fileManager.SetDefaultPlaylist(Path.Combine(docPath, SavedPlaylistName) + ".txt");
            Playlists.Add(new Sound()
            {
                Name = SavedPlaylistName,
                Path = Path.Combine(docPath, SavedPlaylistName)
            } 
            );
            RefreshPlaylists(Playlists.ToList());
            SavedPlaylistName = "";
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanOpenPlaylist(object obj)
        {
            return SelectedPlaylist != null;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteOpenPlaylist(object obj)
        {
            this.fileManager.FillSoundsFromPlaylist(SelectedPlaylist.Path);
            this.fileManager.SetDefaultPlaylist(SelectedPlaylist.Path);
            if (this.currentAudioPlayerState == AudioPlayerState.Shuffled)
            {
                this.notPlayedSounds = Sounds.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CanDeletePlaylist(object obj)
        {
            //return SelectedPlaylist != null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteDeletePlaylist(object obj)
        {

        }

        #endregion

        #region Pivate helper methods

        /// <summary>
        /// Creates RelayCommand instances for ICommand properties with appropriate Execute and CanExecute methods.
        /// </summary>
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
            DeletePlaylist = new RelayCommand(ExecuteDeletePlaylist, CanDeletePlaylist);
        }

        /// <summary>
        /// Refreshes UI.
        /// </summary>
        private void OnTick(object sender, EventArgs s)
        {
            UpdateProgress();
            UpdateTime();
        }

        /// <summary>
        /// Updates TimeInfo string with current position of time in sound next to total sound time.
        /// </summary>
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

        /// <summary>
        /// Updates progress bar according to current time position of sound.
        /// </summary>
        private void UpdateProgress()
        {
            Progress = this.soundEngine.GetFilePosition();
        }

        /// <summary>
        /// Refreshes <see cref="Sounds"/>.
        /// </summary>
        /// <param name="sounds">New list of sounds.</param>
        private void RefreshSounds(List<Sound> sounds)
        {
            if (sounds == null)
            {
                Sounds.Clear();
                return;
            }
            Sounds.Clear();
            sounds.ForEach(s => Sounds.Add(s));
        }

        /// <summary>
        /// Refreshes <see cref="Playlists"/>
        /// </summary>
        /// <param name="playlists">New list of playlists.</param>
        private void RefreshPlaylists(List<Sound> playlists)
        {
            if (playlists == null)
            {
                Playlists.Clear();
                return;
            }
            Playlists.Clear();
            playlists.ForEach(p => Playlists.Add(p));
        }

        /// <summary>
        /// Initialize <see cref="Sounds"/> with files found in last opened directory or from default 'User\Music' folder.
        /// </summary>
        private void InitializeSoundlistFromDefaultDirectory()
        {
            if (fileManager.CheckIfDefaultDirectoryIsSet())
            {
                fileManager.FillSoundsFromDefaultDirectory();
            }
            else
            {
                this.fileManager.SetDefaultDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                this.fileManager.FillSoundsFromDefaultDirectory();
            }
            this.fileManager.FillPlaylistFromDefaultDirectory();
        }

        /// <summary>
        /// Initialize <see cref="Sounds"/> with files found in last opened playlist.
        /// </summary>
        private void InitializeSoundlistFromDefaultPlaylist()
        {
            if (this.fileManager.CheckIfDefaultPlaylistIsSet())
            {
                this.fileManager.FillSoundsFromDefaultPlaylist();
                this.fileManager.FillPlaylistFromDefaultDirectory();
            }
            else
            {
                InitializeSoundlistFromDefaultDirectory();
            }
        }

        /// <summary>
        /// Initializes manager interfaces and creates new objects.
        /// </summary>
        private void InitializeManagers()
        {
            this.soundEngine = SoundEngine.SoundEngineInstance;
            this.soundEngine.StateChanged += OnSoundStateChanged;
            this.soundEngine.SoundError += OnSoundError;
            this.soundEngine.MediaEnded += NextSoundRepeatNormal;

            this.currentAudioPlayerState = AudioPlayerState.Normal;

            this.fileManager = new FileManagerImplementation();
            this.fileManager.Initialize();
            this.fileManager.StateChanged += OnFileManagerStateChanged;
            this.fileManager.FileManagerError += OnFileManagerError;

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

        private void PreviousSoundRepeatNormal(object sender, EventArgs e)
        {
            var index = Sounds.IndexOf(CurrentSound);
            if (index > 0)
            {
                SelectedSound = Sounds[--index];
                CurrentSound = SelectedSound;
                if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
                {
                    this.soundEngine.Stop();
                }
                this.soundEngine.Play(CurrentSound.Path);
            }
        }

        private void NextSoundRepeatNormal(object sender, EventArgs e)
        {
            var index = Sounds.IndexOf(CurrentSound);
            if (index < Sounds.Count - 1)
            {
                SelectedSound = Sounds[++index];
                CurrentSound = SelectedSound;
                if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
                {
                    this.soundEngine.Stop();
                }
                this.soundEngine.Play(CurrentSound.Path);
            }
        }

        private void NextSoundRepeatShuffled(object sender, EventArgs e)
        {
            if (this.notPlayedSounds.Count == 0)
            {
                this.notPlayedSounds = Sounds.ToList();
            }
            var randomSound = random.Next(0, this.notPlayedSounds.Count);
            CurrentSound = this.notPlayedSounds[randomSound];
            SelectedSound = CurrentSound;
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }
            this.soundEngine.Play(CurrentSound.Path);
            notPlayedSounds.Remove(CurrentSound);
        }

        private void NextSoundRepeatSound(object sender, EventArgs e)
        {
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }
            this.soundEngine.Play(CurrentSound.Path);
        }

        private void NextSoundRepeatPlaylist(object sender, EventArgs e)
        {
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }

            var index = Sounds.IndexOf(CurrentSound);
            if (index < Sounds.Count - 1)
            {
                CurrentSound = Sounds[++index];
                this.soundEngine.Play(CurrentSound.Path);
            }
            else
            {
                CurrentSound = Sounds[0];
                this.soundEngine.Play(CurrentSound.Path);
            }
            SelectedSound = CurrentSound;
        }

        private void PreviousSoundRepeatPlaylist(object sender, EventArgs e)
        {
            if (this.currentSoundState == SoundState.Playing || this.currentSoundState == SoundState.Paused)
            {
                this.soundEngine.Stop();
            }

            var index = Sounds.IndexOf(CurrentSound);
            if (index > 0)
            {
                CurrentSound = Sounds[--index];
                this.soundEngine.Play(CurrentSound.Path);
            }
            else
            {
                CurrentSound = Sounds[Sounds.Count - 1];
                this.soundEngine.Play(CurrentSound.Path);
            }
            SelectedSound = CurrentSound;
        }

        private void SetMediaEndedEvent(EventHandler newEventHandler, AudioPlayerState newState)
        {
            this.soundEngine.MediaEnded += newEventHandler;
            this.currentAudioPlayerState = newState;
        }

        #endregion
    }
}
