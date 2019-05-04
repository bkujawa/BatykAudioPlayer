using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatykAudioPlayer.APP.AudioPlayer;
using BatykAudioPlayer.BL.SoundEngine;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public interface IFilePlaylistManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirPath"></param>
        List<Sound> FillSoundsFromDirectory(string dirPath);

        /// <summary>
        /// 
        /// </summary>
        List<Sound> FillPlaylist();

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

    }
}
