using System;
using System.Collections.Generic;
using BatykAudioPlayer.BL.SoundEngineInterface;

namespace BatykAudioPlayer.BL.FileManagerInterface
{
    public interface IFileManager
    {
        /// <summary>
        /// Fills Sounds collection with files found in directory pointed by dirPath.
        /// </summary>
        /// <param name="dirPath"></param>
        void FillSoundsFromDirectory(string dirPath);

        /// <summary>
        /// Fills Sounds collection with files found in default directory.
        /// </summary>
        /// <returns></returns>
        void FillSoundsFromDefaultDirectory();

        /// <summary>
        /// Fills Sounds collection with files found in playlist file pointed by playlistPath.
        /// </summary>
        /// <param name="playlistPath"></param>
        void FillSoundsFromPlaylist(string playlistPath);

        /// <summary>
        /// Fills Sounds collection with files found in default playlist file.
        /// </summary>
        void FillSoundsFromDefaultPlaylist();

        /// <summary>
        /// Fills Playlists collection with files found in default directory.
        /// <para></para> 
        /// For now this directory is "C:\'User'\Documents\AudioPlayer"
        /// </summary>
        void FillPlaylistFromDefaultDirectory();

        /// <summary>
        /// Saves default directory in ConfigurationManager.
        /// </summary>
        /// <param name="dirPath"></param>
        void SetDefaultDirectory(string dirPath);

        /// <summary>
        /// Saves default playlist file in ConfigurationManager.
        /// </summary>
        /// <param name="listPath"></param>
        void SetDefaultPlaylist(string listPath);

        /// <summary>
        /// Self-explanatory.
        /// </summary>
        bool CheckIfDefaultDirectoryIsSet();

        /// <summary>
        /// Self-explanatory.
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
        event EventHandler<FileManagerErrorArgs> FileManagerError;
    }


    // TODO: 
    // I don't think FileManagerEventArgs should be handled this way between Sounds and Playlists.
    // There should be seperate eventArgs class for sounds change and playlists change.
    // They can both use same errorArgs class. Actually, why not most of event shouldn't use it?

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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
