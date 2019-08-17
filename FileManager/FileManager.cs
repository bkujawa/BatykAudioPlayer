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
        private List<Sound> soundlist = new List<Sound>();
        private List<Sound> playlist = new List<Sound>();
        private delegate void FilLSoundsFromDirectory(string dir);

        #endregion

        #region Event handlers

        public event EventHandler<FileManagerEventArgs> StateChanged;
        public event EventHandler<FileManagerErrorArgs> FileManagerError;

        #endregion

        #region Event handlers methods

        private void OnStateChanged(FileManagerEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        private void OnError(string error)
        {
            FileManagerError?.Invoke(this, new FileManagerErrorArgs(error));
        }

        #endregion

        #region IFilePlaylistManager implementation

        public void FillPlaylistFromDefaultDirectory()
        {
            string docPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioPlayer");
            playlist.Clear();
            try
            {
                var allFiles = Directory.GetFiles(docPath);
                foreach (var file in allFiles)
                {
                    var pathExtension = Path.GetExtension(file);
                    if (pathExtension?.ToUpper() == ".TXT")
                    {
                        playlist.Add(new Sound()
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Path = file
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                OnStateChanged(new FileManagerEventArgs(null, CollectionRefreshed.Playlists));
                OnError("FillplaylistFromDefaultDirectory error.");
            }
            OnStateChanged(new FileManagerEventArgs(playlist, CollectionRefreshed.Playlists));
        }

        // [TODO]: Implement asynchronously
        public void FillSoundsFromDirectory(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                this.soundlist.Clear();
                //var fillSoundFromDirectoryRecurisve = new FilLSoundsFromDirectory(FillSoundsFromDirectoryRecursive);
                // i dont want to call asynchronous method here,
                // i want to fillsoundsfromdirectory method to be called asynchronously from UI layer
                // should i make a delegate in audioplayerviewmodel to call this method recursive?
                //fillSoundFromDirectoryRecurisve.BeginInvoke(dirPath, null, null);
                FillSoundsFromDirectoryRecursive(dirPath);
                if (this.soundlist.Any())
                {
                    OnStateChanged(new FileManagerEventArgs(this.soundlist));
                }
            }
            else
            {
                OnStateChanged(new FileManagerEventArgs(new List<Sound>()));
                OnError("FillSoundsFromDirectory error.");
            }
        }

        private void FillSoundsFromDirectoryRecursive(string dirPath)
        {
            // TODO: Make searching for files faster, nonblocking, multithreading.
            var allFiles = Directory.GetFiles(dirPath);
            allFiles.ToList().ForEach(file =>
            {
                var pathExtension = Path.GetExtension(file);
                if (pathExtension?.ToUpper() == ".MP3")
                {
                    Mp3FileReader reader = new Mp3FileReader(file);
                    TimeSpan duration = reader.TotalTime;
                    this.soundlist.Add(new Sound()
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                        Time = duration.ToString(@"hh\:mm\:ss")
                    });
                }
            });
            var allDirectories = Directory.GetDirectories(dirPath);
            allDirectories.ToList().ForEach(dir =>
            {
                FillSoundsFromDirectoryRecursive(dir);
            });

            /*
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
                FillSoundsFromDirectoryRecursive(directory);
            } */
        }

        public void FillSoundsFromDefaultDirectory()
        {
            if (!string.IsNullOrEmpty(defaultDirectory))
            {
                FillSoundsFromDirectory(defaultDirectory);
            }
        }

        public void FillSoundsFromPlaylist(string playlistPath)
        {
            if (!string.IsNullOrEmpty(playlistPath))
            {
                this.soundlist.Clear();
                var allFiles = File.ReadAllLines(playlistPath);
                for (int i = 0; i < allFiles.Length - 1; i = i + 3)
                {
                    soundlist.Add(new Sound()
                    {
                        Name = allFiles[i],
                        Path = allFiles[i + 1],
                        Time = allFiles[i + 2]
                    });
                }
                if (this.soundlist.Any())
                {
                    OnStateChanged(new FileManagerEventArgs(soundlist));
                }
            }
        }

        public void FillSoundsFromDefaultPlaylist()
        {
            if (!string.IsNullOrEmpty(defaultPlaylist))
            {
                FillSoundsFromPlaylist(defaultPlaylist);
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
                config.Save(ConfigurationSaveMode.Full);
                this.defaultDirectory = dirPath;
            }
        }

        public void SetDefaultPlaylist(string dirPath)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config?.AppSettings.Settings["PlaylistPath"] != null)
                {
                    config.AppSettings.Settings["PlaylistPath"].Value = dirPath;
                }
                else
                {
                    config.AppSettings.Settings.Add(new KeyValueConfigurationElement("PlaylistPath", dirPath));
                }
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
            soundlist = new List<Sound>();
            playlist = new List<Sound>();
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Searches through configuration file to find default directory. Returns null if there is no default directory, otherwise returns path to default directory.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Searches through configuration file to find default playlist. Returns null if there is no default playlist, otherwise returns path to default playlist.
        /// </summary>
        /// <returns></returns>
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
