using BatykAudioPlayer.BL.FileManager;
using BatykAudioPlayer.BL.FileManagerInterface;
using BatykAudioPlayer.BL.SoundEngine;
using BatykAudioPlayer.BL.SoundEngineInterface;
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
            // TODO: Setup IoC container in some bootstrapper class and call it here. / OR / make it into a bootsraper class and move AudioPlayerView/Model to other project.
            //       Imperative/Declarative registration?
            base.OnStartup(e);
            IFileManager fileManager = new FileManager();
            IMediaPlayer mediaPlayer = new BatykAudioPlayer.BL.SoundEngine.MediaPlayer();
            ISoundEngine soundEngine = new SoundEngine(mediaPlayer);
            Application.Current.MainWindow = new AudioPlayerView(fileManager, soundEngine);
            Application.Current.MainWindow.Show();
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            // TODO: Log?
        }
    }
}
