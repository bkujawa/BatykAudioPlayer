using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BatykAudioPlayer.BL.SoundEngine;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public interface IFilePlaylistManager
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
        /// 
        /// </summary>
        void FillPlaylistsFromDefaultDirectory();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirPath"></param>
        void SetDefaultDirectory(string dirPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listPath"></param>
        void SetDefualtPlaylist(string listPath);

        /// <summary>
        /// 
        /// </summary>
        bool CheckIfDefaultDirectoryIsSet();

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<FilePlaylistManagerEventArgs> StateChanged;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<FilePlaylistManagerErrorArgs> FilePlaylistError;
    }
}
