using System;

namespace YachtDice.Client.Logic.Audio
{
    /// <summary>
    /// What the player chose about the sound of the game: the sound effects on or off, and the music muted
    /// or not with its volume. It does not play anything; the windows read it and the players of sound apply it.
    /// </summary>
    public sealed class AudioSettings
    {
        /// <summary>
        /// Lowest volume of the music.
        /// </summary>
        public const int MinVolume = 0;

        /// <summary>
        /// Highest volume of the music.
        /// </summary>
        public const int MaxVolume = 100;

        /// <summary>
        /// Volume of the music when the application starts.
        /// </summary>
        public const int DefaultMusicVolume = 40;

        private int _musicVolume = DefaultMusicVolume;

        /// <summary>
        /// Raised when the player changes any of the choices.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Gets a value indicating whether the sound effects (clicks, dice, points) are turned on.
        /// </summary>
        public bool AreEffectsEnabled { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether the music is muted.
        /// </summary>
        public bool IsMusicMuted { get; private set; }

        /// <summary>
        /// Gets the volume of the music, from <see cref="MinVolume"/> to <see cref="MaxVolume"/>.
        /// </summary>
        public int MusicVolume
        {
            get { return _musicVolume; }
        }

        /// <summary>
        /// Turns the sound effects on or off.
        /// </summary>
        /// <param name="areEnabled">True to play the sound effects; false to keep them silent.</param>
        public void SetEffectsEnabled(bool areEnabled)
        {
            AreEffectsEnabled = areEnabled;
            Changed?.Invoke();
        }

        /// <summary>
        /// Mutes or unmutes the music.
        /// </summary>
        /// <param name="isMuted">True to mute the music; false to hear it again.</param>
        public void SetMusicMuted(bool isMuted)
        {
            IsMusicMuted = isMuted;
            Changed?.Invoke();
        }

        /// <summary>
        /// Sets the volume of the music. A value out of the range is moved to the nearest limit.
        /// </summary>
        /// <param name="volume">Volume, from <see cref="MinVolume"/> to <see cref="MaxVolume"/>.</param>
        public void SetMusicVolume(int volume)
        {
            _musicVolume = Math.Max(MinVolume, Math.Min(MaxVolume, volume));
            Changed?.Invoke();
        }

        /// <summary>
        /// Gets the volume that the player of the music must use: zero when muted, otherwise the volume as a
        /// fraction of the highest one.
        /// </summary>
        /// <returns>A number from 0 to 1.</returns>
        public double GetMusicPlayerVolume()
        {
            double volume = 0;

            if (!IsMusicMuted)
            {
                volume = (double)_musicVolume / MaxVolume;
            }

            return volume;
        }
    }
}
