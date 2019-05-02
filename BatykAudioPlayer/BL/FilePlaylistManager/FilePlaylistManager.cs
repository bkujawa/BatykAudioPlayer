using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.APP;
using BatykAudioPlayer.APP.AudioPlayer;
using BatykAudioPlayer.BL.SoundEngine;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public class FilePlaylistManager : IFilePlaylistManager
    {
        #region Private fields

        private AudioPlayerViewModel APViewModel;

        #endregion

        #region IFilePlaylistManager implementation
        public void FillPlaylist()
        {
            throw new NotImplementedException();
        }

        public void FillSoundsDirectory(string dirPath)
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
                APViewModel.Sounds.Clear();
                soundList.ForEach(c => APViewModel.Sounds.Add(c));
            }
        }

        public void SetDefaultDirectory(string dirPath)
        {
            throw new NotImplementedException();
        }

        public void SetDefualtPlaylist(string listPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Constructor

        public FilePlaylistManager(object sender)
        {
            SetViewModel(sender);
        }

        #endregion

        #region Private helpers

        private void SetViewModel(object sender)
        {
            switch (sender)
            {
                case AudioPlayerViewModel apvm:
                    APViewModel = (AudioPlayerViewModel)apvm;
                    break;
            }
        }

        #endregion
    }
}
