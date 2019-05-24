using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.SoundEngine;
using System.Configuration;
using NAudio.Wave;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public class FilePlaylistManager : IFilePlaylistManager
    {
        #region Private fields

        private string defaultDirectory;
        private string defaultPlaylist;

        #endregion

        #region Event handlers

        public event EventHandler<FilePlaylistManagerEventArgs> StateChanged;
        public event EventHandler<FilePlaylistManagerErrorArgs> FilePlaylistError;

        #endregion

        #region Constructor

        public FilePlaylistManager()
        {
            defaultDirectory = ReturnDefaultDirectoryFromConfig();
            defaultPlaylist = ReturnDefaultPlaylistFromConfig();
        }
        #endregion

        #region Event handlers methods

        private void OnStateChanged(FilePlaylistManagerEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        private void OnError(string error)
        {
            FilePlaylistError?.Invoke(this, new FilePlaylistManagerErrorArgs(error));
        }

        #endregion

        #region IFilePlaylistManager implementation

        public List<Sound> FillPlaylist()
        {
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioPlayer");
            var playlists = new List<Sound>();
            var allFiles = Directory.GetFiles(docPath);
            foreach (var file in allFiles)
            {
                var pathExtension = Path.GetExtension(file);
                if (pathExtension?.ToUpper() == ".TXT")
                {
                    //playlists.Add(new Sound(Path.GetFileNameWithoutExtension(file), file));
                }
            }
            return playlists;
        }

        public void FillPlaylistFromDefaultDirectory()
        {
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioPlayer");
            var playLists = new List<Sound>();
            try
            {
                var allFiles = Directory.GetFiles(docPath);
                foreach (var file in allFiles)
                {
                    var pathExtension = Path.GetExtension(file);
                    if (pathExtension?.ToUpper() == ".TXT")
                    {
                        playLists.Add(new Sound(Path.GetFileNameWithoutExtension(file), file, null));
                    }
                }
            }
            catch(Exception ex)
            {
                OnStateChanged(new FilePlaylistManagerEventArgs(null, CollectionRefreshed.Playlists));
            }
            OnStateChanged(new FilePlaylistManagerEventArgs(playLists, CollectionRefreshed.Playlists));
        }

        public void FillSoundsFromDirectory(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                var allFiles = Directory.GetFiles(dirPath);
                var soundList = new List<Sound>();
                foreach (var file in allFiles)
                {
                    var pathExtension = Path.GetExtension(file);
                    if (pathExtension?.ToUpper() == ".MP3")
                    {
                        Mp3FileReader reader = new Mp3FileReader(file);
                        TimeSpan duration = reader.TotalTime;
                        soundList.Add(new Sound(Path.GetFileNameWithoutExtension(file), file, duration.ToString(@"hh\:mm\:ss")));
                    }
                }
                OnStateChanged(new FilePlaylistManagerEventArgs(soundList));
            }
            else
            {               
                OnStateChanged(null);
            }
        }

        public void FillSoundsFromDefaultDirectory()
        {
            if (!string.IsNullOrEmpty(defaultDirectory))
            {
                var allFiles = Directory.GetFiles(defaultDirectory);
                var soundList = new List<Sound>();
                foreach (var file in allFiles)
                {
                    var pathExtension = Path.GetExtension(file);
                    if (pathExtension?.ToUpper() == ".MP3")
                    {
                        Mp3FileReader reader = new Mp3FileReader(file);
                        TimeSpan duration = reader.TotalTime;
                        soundList.Add(new Sound(Path.GetFileNameWithoutExtension(file), file, duration.ToString(@"hh\:mm\:ss")));
                    }
                }
                OnStateChanged(new FilePlaylistManagerEventArgs(soundList));
            }
            else
            {
                OnStateChanged(null);
            }
        }

        public void SetDefaultDirectory(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["DirPath"] != null)
                {
                    config.AppSettings.Settings["DirPath"].Value = dirPath;
                }
                else
                {
                    config.AppSettings.Settings.Add(new KeyValueConfigurationElement("DirPath", dirPath));
                }
                ConfigurationManager.AppSettings["DirPath"] = dirPath;
                config.Save(ConfigurationSaveMode.Full);
                this.defaultDirectory = dirPath;
            }
        }

        public void SetDefaultPlaylist(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["PlaylistPath"] != null)
                {
                    config.AppSettings.Settings["PlaylistPath"].Value = dirPath;
                }
                else
                {
                    config.AppSettings.Settings.Add(new KeyValueConfigurationElement("PlaylistPath", dirPath));
                }
                ConfigurationManager.AppSettings["PlaylistPath"] = dirPath;
                config.Save(ConfigurationSaveMode.Full);
                this.defaultPlaylist = dirPath;
            }
        }

        public bool CheckIfDefaultDirectoryIsSet()
        {
            if (string.IsNullOrEmpty(defaultDirectory))
            {
                return false;
            }
            return true;
        }

        public bool CheckIfDefaultPlaylistIsSet()
        {
            if (string.IsNullOrEmpty(defaultPlaylist))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Private helpers

        private string ReturnDefaultDirectoryFromConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var directory = config.AppSettings.Settings["DirPath"]?.Value;
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }
            return directory;
        }

        private string ReturnDefaultPlaylistFromConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var playlist = config.AppSettings.Settings["PlaylistPath"]?.Value;
            if (string.IsNullOrEmpty(playlist))
            {
                return null;
            }
            return playlist;
        }

        #endregion
    }

    public class FilePlaylistManagerEventArgs : EventArgs
    {
        public List<Sound> NewSounds { get; private set; }
        public CollectionRefreshed Refreshed { get; private set; }
        public FilePlaylistManagerEventArgs(List<Sound> NewSounds, CollectionRefreshed Refreshed = CollectionRefreshed.Sounds)
        {
            this.NewSounds = NewSounds;
            this.Refreshed = Refreshed;
        }
    }

    public class FilePlaylistManagerErrorArgs
    {
        public string ErrorDetails { get; private set; }
        public FilePlaylistManagerErrorArgs(string ErrorDetails)
        {
            this.ErrorDetails = ErrorDetails;
        }
    }

    public enum CollectionRefreshed
    {
        Sounds,
        Playlists
    }
}
