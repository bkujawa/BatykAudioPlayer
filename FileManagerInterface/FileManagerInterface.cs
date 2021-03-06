﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.FileManagerInterface
{
    public interface IFileManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirPath"></param>
        void FillSoundsFromDirectory(string dirPath);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        void FillSoundsFromDefaultDirectory();

        /// <summary>
        /// 
        /// </summary>
        List<Sound> FillPlaylist();

        /// <summary>
        /// Fills <see cref="BatykAudioPlayer.APP.AudioPlayer.AudioPlayerViewModel.Playlists"/> with files found in default directory.
        /// <para></para> 
        /// For now this directory is "C:\'User'\Documents\AudioPlayer"
        /// </summary>
        void FillPlaylistFromDefaultDirectory();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirPath"></param>
        void SetDefaultDirectory(string dirPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listPath"></param>
        void SetDefaultPlaylist(string listPath);

        /// <summary>
        /// 
        /// </summary>
        bool CheckIfDefaultDirectoryIsSet();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool CheckIfDefaultPlaylistIsSet();

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<FileManagerEventArgs> StateChanged;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<FileManagerErrorArgs> FilePlaylistError;

        /// <summary>
        /// 
        /// </summary>
        void Initialize();
    }

    public class FileManagerEventArgs : EventArgs
    {
        public List<Sound> NewSounds { get; private set; }
        public CollectionRefreshed Refreshed { get; private set; }
        public FileManagerEventArgs(List<Sound> NewSounds, CollectionRefreshed Refreshed = CollectionRefreshed.Sounds)
        {
            this.NewSounds = NewSounds;
            this.Refreshed = Refreshed;
        }
    }

    public class FileManagerErrorArgs
    {
        public string ErrorDetails { get; private set; }
        public FileManagerErrorArgs(string ErrorDetails)
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
