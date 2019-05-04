using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.SoundEngine;
using System.Configuration;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public class FilePlaylistManager : IFilePlaylistManager
    {
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
                    playlists.Add(new Sound(Path.GetFileNameWithoutExtension(file), file));
                }
            }
            return playlists;
        }

        public List<Sound> FillSoundsFromDirectory(string dirPath)
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
                        soundList.Add(new Sound(Path.GetFileNameWithoutExtension(file), file));
                    }
                }
                return soundList;
            }
            return null;
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
            }
        }

        public void SetDefualtPlaylist(string dirPath)
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
            }
        }

        #endregion

        #region Private helpers

        #endregion
    }
}
