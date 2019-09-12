using BatykAudioPlayer.BL.FileManager;
using BatykAudioPlayer.BL.FileManagerInterface;
using BatykAudioPlayer.BL.SoundEngine;
using BatykAudioPlayer.BL.SoundEngineInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BatykAudioPlayer.APP.AudioPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            IFileManager fileManager = new FileManager();
            ISoundEngine soundEngine = new SoundEngine();
            Application.Current.MainWindow = new AudioPlayerView(fileManager, soundEngine);
            Application.Current.MainWindow.Show();
        }
    }
}
