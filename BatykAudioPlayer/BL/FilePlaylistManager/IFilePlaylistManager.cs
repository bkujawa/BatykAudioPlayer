using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatykAudioPlayer.BL.FilePlaylistManager
{
    public interface IFilePlaylistManager
    {
        void FillSoundsDirectory(string dirPath);
        void FillPlaylist();
        void SetDefaultDirectory(string dirPath);
        void SetDefualtPlaylist(string listPath);

    }
}
