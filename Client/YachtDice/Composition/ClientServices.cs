using System;
using YachtDice.Client.Logic.Audio;
using YachtDice.Client.Logic.Logging;
using YachtDice.Client.Logic.Settings;
using YachtDice.Navigation;
using YachtDice.Utils;

namespace YachtDice.Composition
{
    /// <summary>
    /// Components that the windows share while the application is open: the choices about the sound, the music
    /// and what happens when the connection is lost. The container creates it once and it goes from one window
    /// to the next one through the constructors and the session of the player.
    /// </summary>
    public sealed class ClientServices
    {
        /// <summary>
        /// Initializes the shared components of the client.
        /// </summary>
        /// <param name="audio">Choices of the player about the sound effects and the music.</param>
        /// <param name="music">Player of the background music.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public ClientServices(AudioSettings audio, MusicPlayer music)
        {
            Audio = audio ?? throw new ArgumentNullException(nameof(audio), LogMessages.NullDependencyMessage);
            Music = music ?? throw new ArgumentNullException(nameof(music), LogMessages.NullDependencyMessage);
            Gameplay = new GameplaySettings();
            ConnectionLoss = new ConnectionLossHandler(this);
        }

        /// <summary>
        /// Gets what the player chose about the sound effects and the music.
        /// </summary>
        public AudioSettings Audio { get; }

        /// <summary>
        /// Gets what the player chose about the help that the game gives while playing.
        /// </summary>
        public GameplaySettings Gameplay { get; }

        /// <summary>
        /// Gets the player of the background music, which keeps playing while the windows change.
        /// </summary>
        public MusicPlayer Music { get; }

        /// <summary>
        /// Gets the component that takes the player back to the login window when the connection is lost.
        /// </summary>
        public ConnectionLossHandler ConnectionLoss { get; }
    }
}
