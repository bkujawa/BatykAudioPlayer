using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.FileManagerInterface;
using BatykAudioPlayer.BL.SoundEngineInterface;
using NAudio.Wave;
using System.Configuration;


namespace BatykAudioPlayer.BL.FileManager
{
    public class FileManagerImplementation : IFileManager
    {
        #region Private fields

        private string defaultDirectory;
        private string defaultPlaylist;

        #endregion

        #region Event handlers

        public event EventHandler<FileManagerEventArgs> StateChanged;
        public event EventHandler<FileManagerErrorArgs> FilePlaylistError;

        #endregion

        #region Event handlers methods

        private void OnStateChanged(FileManagerEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        private void OnError(string error)
        {
            FilePlaylistError?.Invoke(this, new FileManagerErrorArgs(error));
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
            catch (Exception ex)
            {
                OnStateChanged(new FileManagerEventArgs(null, CollectionRefreshed.Playlists));
            }
            OnStateChanged(new FileManagerEventArgs(playLists, CollectionRefreshed.Playlists));
        }

        public void FillSoundsFromDirectory(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                var soundList = new List<Sound>();
                FillSoundsFromDirectoryRecursive(dirPath, ref soundList);
                if (soundList.Any())
                {
                    OnStateChanged(new FileManagerEventArgs(soundList));
                }
            }
            else
            {
                OnStateChanged(new FileManagerEventArgs(new List<Sound>()));
                OnError("FillSoundsFromDirectory error.");
            }
        }

        private void FillSoundsFromDirectoryRecursive(string dirPath, ref List<Sound> soundList)
        {
            // TODO: Make searching for files faster, nonblocking, multithreading.
            //allFiles.ToList().ForEach(file =>
            //{
            //    var pathExtension = Path.GetExtension(file);
            //    if (pathExtension?.ToUpper() == ".MP3")
            //    {
            //        Mp3FileReader reader = new Mp3FileReader(file);
            //        TimeSpan duration = reader.TotalTime;
            //        soundList.Add(new Sound(Path.GetFileNameWithoutExtension(file), file, duration.ToString(@"hh\:mm\:ss")));
            //    }
            //});
            var allFiles = Directory.GetFiles(dirPath);
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
            var allDirectories = Directory.GetDirectories(dirPath);
            foreach (var directory in allDirectories)
            {
                FillSoundsFromDirectoryRecursive(directory, ref soundList);
            }
        }

        public void FillSoundsFromDefaultDirectory()
        {
            if (!string.IsNullOrEmpty(defaultDirectory))
            {
                FillSoundsFromDirectory(defaultDirectory);
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

        public void Initialize()
        {
            defaultDirectory = ReturnDefaultDirectoryFromConfig();
            defaultPlaylist = ReturnDefaultPlaylistFromConfig();
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


}
