using Xunit;
using YachtDice.Client.Logic.Audio;

namespace YachtDice.Client.Tests.Audio
{
    /// <summary>
    /// What the player chose about the sound effects and the music.
    /// </summary>
    public class AudioSettingsTests
    {
        private const int AboveMaximum = 150;
        private const int BelowMinimum = -20;
        private const int ValidVolume = 70;
        private const int HalfVolume = 50;
        private const double HalfPlayerVolume = 0.5;
        private const double SilentPlayerVolume = 0.0;
        private const int ExpectedChangeCount = 1;

        [Fact]
        public void New_Default_HasTheEffectsEnabled()
        {
            var settings = new AudioSettings();

            Assert.True(settings.AreEffectsEnabled);
        }

        [Fact]
        public void New_Default_HasTheMusicNotMuted()
        {
            var settings = new AudioSettings();

            Assert.False(settings.IsMusicMuted);
        }

        [Fact]
        public void New_Default_HasTheDefaultMusicVolume()
        {
            var settings = new AudioSettings();

            Assert.Equal(AudioSettings.DefaultMusicVolume, settings.MusicVolume);
        }

        [Fact]
        public void SetEffectsEnabled_False_TurnsTheEffectsOff()
        {
            var settings = new AudioSettings();

            settings.SetEffectsEnabled(false);

            Assert.False(settings.AreEffectsEnabled);
        }

        [Fact]
        public void SetMusicMuted_True_MutesTheMusic()
        {
            var settings = new AudioSettings();

            settings.SetMusicMuted(true);

            Assert.True(settings.IsMusicMuted);
        }

        [Fact]
        public void SetMusicVolume_ValidVolume_StoresTheVolume()
        {
            var settings = new AudioSettings();

            settings.SetMusicVolume(ValidVolume);

            Assert.Equal(ValidVolume, settings.MusicVolume);
        }

        [Fact]
        public void SetMusicVolume_AboveTheMaximum_StoresTheMaximum()
        {
            var settings = new AudioSettings();

            settings.SetMusicVolume(AboveMaximum);

            Assert.Equal(AudioSettings.MaxVolume, settings.MusicVolume);
        }

        [Fact]
        public void SetMusicVolume_BelowTheMinimum_StoresTheMinimum()
        {
            var settings = new AudioSettings();

            settings.SetMusicVolume(BelowMinimum);

            Assert.Equal(AudioSettings.MinVolume, settings.MusicVolume);
        }

        [Fact]
        public void GetMusicPlayerVolume_HalfVolume_ReturnsOneHalf()
        {
            var settings = new AudioSettings();

            settings.SetMusicVolume(HalfVolume);

            Assert.Equal(HalfPlayerVolume, settings.GetMusicPlayerVolume());
        }

        [Fact]
        public void GetMusicPlayerVolume_MutedMusic_ReturnsZero()
        {
            var settings = new AudioSettings();

            settings.SetMusicMuted(true);

            Assert.Equal(SilentPlayerVolume, settings.GetMusicPlayerVolume());
        }

        [Fact]
        public void GetMusicPlayerVolume_UnmutedAgain_ReturnsTheVolumeBack()
        {
            var settings = new AudioSettings();

            settings.SetMusicVolume(HalfVolume);
            settings.SetMusicMuted(true);
            settings.SetMusicMuted(false);

            Assert.Equal(HalfPlayerVolume, settings.GetMusicPlayerVolume());
        }

        [Fact]
        public void SetMusicVolume_AnyVolume_RaisesChangedOnce()
        {
            var settings = new AudioSettings();
            var changeCount = 0;

            settings.Changed += () => changeCount++;
            settings.SetMusicVolume(ValidVolume);

            Assert.Equal(ExpectedChangeCount, changeCount);
        }
    }
}
