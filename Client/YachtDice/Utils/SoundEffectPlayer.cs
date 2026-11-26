using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Resources;
using YachtDice.Client.Logic.Audio;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Utils
{
    /// <summary>
    /// Plays the sound effects of the game when the player has the sounds turned on. A sound that cannot be
    /// played is only written to the log, because the game works the same without it.
    /// </summary>
    public sealed class SoundEffectPlayer
    {
        private const string SoundUriFormat = "pack://application:,,,/Resources/Sounds/{0}.wav";

        private readonly Dictionary<SoundEffect, SoundPlayer> _players = new Dictionary<SoundEffect, SoundPlayer>();
        private readonly AppLogger _logger = new AppLogger(typeof(SoundEffectPlayer));
        private readonly AudioSettings _audio;

        /// <summary>
        /// Initializes the player of the sound effects.
        /// </summary>
        /// <param name="audio">Choices of the player about the sound effects.</param>
        public SoundEffectPlayer(AudioSettings audio)
        {
            _audio = audio ?? throw new ArgumentNullException(nameof(audio), LogMessages.NullAudioMessage);
        }

        /// <summary>
        /// Plays the sound effect, unless the player turned the sounds off.
        /// </summary>
        /// <param name="effect">Sound effect to play.</param>
        public void Play(SoundEffect effect)
        {
            if (_audio.AreEffectsEnabled)
            {
                try
                {
                    GetPlayer(effect).Play();
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is IOException)
                {
                    _logger.Error(ex, LogMessages.PlayFailedLogMessage);
                }
            }
        }

        private SoundPlayer GetPlayer(SoundEffect effect)
        {
            if (!_players.TryGetValue(effect, out SoundPlayer player))
            {
                player = CreatePlayer(effect);
                _players[effect] = player;
            }

            return player;
        }

        private static SoundPlayer CreatePlayer(SoundEffect effect)
        {
            var uri = new Uri(string.Format(SoundUriFormat, effect));
            StreamResourceInfo resource = Application.GetResourceStream(uri);
            var content = new MemoryStream();

            resource.Stream.CopyTo(content);
            content.Position = 0;

            return new SoundPlayer(content);
        }
    }
}
