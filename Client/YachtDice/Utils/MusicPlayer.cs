using System;
using System.IO;
using System.Windows.Media;
using YachtDice.Client.Logic.Audio;

namespace YachtDice.Utils
{
    /// <summary>
    /// Plays the background music in a loop and follows the volume and the mute that the player chose. The
    /// music keeps playing while the windows change; it only changes when another track is asked.
    /// </summary>
    public sealed class MusicPlayer
    {
        private const string MusicFolder = "Resources\\Music";
        private const string TrackExtension = ".wav";
        private const string MusicFailedLogMessage = "The background music could not be played.";

        private readonly MediaPlayer _player = new MediaPlayer();
        private readonly AudioSettings _settings;
        private readonly AppLogger _logger = new AppLogger(typeof(MusicPlayer));
        private MusicTrack? _currentTrack;

        /// <summary>
        /// Initializes the player with the choices of the player about the sound.
        /// </summary>
        /// <param name="settings">Choices of the player; the volume follows their changes.</param>
        public MusicPlayer(AudioSettings settings)
        {
            _settings = settings;
            _settings.Changed += ApplyVolume;
            _player.MediaEnded += HandleMediaEnded;
            _player.MediaFailed += HandleMediaFailed;
            ApplyVolume();
        }

        /// <summary>
        /// Starts a track. If that track is already playing nothing changes, so the windows can ask for
        /// their track every time they open.
        /// </summary>
        /// <param name="track">Track to play.</param>
        public void Play(MusicTrack track)
        {
            if (_currentTrack != track)
            {
                _currentTrack = track;
                _player.Open(CreateTrackUri(track));
                _player.Play();
            }
        }

        private static Uri CreateTrackUri(MusicTrack track)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MusicFolder, track + TrackExtension);

            return new Uri(path);
        }

        private void ApplyVolume()
        {
            _player.Volume = _settings.GetMusicPlayerVolume();
        }

        private void HandleMediaEnded(object sender, EventArgs e)
        {
            _player.Position = TimeSpan.Zero;
            _player.Play();
        }

        private void HandleMediaFailed(object sender, ExceptionEventArgs e)
        {
            _logger.Error(e.ErrorException, MusicFailedLogMessage);
        }
    }
}
