using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Seconds of each turn, read from the configuration of the server according to the speed of the game.
    /// </summary>
    public class TurnDurationTests
    {
        private const int NormalTurnSeconds = 60;
        private const int FastTurnSeconds = 30;
        private const int NoSeconds = 0;

        [Theory]
        [InlineData(GameSpeed.Normal, NormalTurnSeconds)]
        [InlineData(GameSpeed.Fast, FastTurnSeconds)]
        public void GetSeconds_ConfiguredSpeed_ReturnsItsSeconds(GameSpeed speed, int expectedSeconds)
        {
            var turnDuration = new TurnDuration(CreateSecondsBySpeed());

            int seconds = turnDuration.GetSeconds(speed);

            Assert.Equal(expectedSeconds, seconds);
        }

        [Fact]
        public void GetSeconds_SpeedWithoutDuration_ThrowsArgumentOutOfRangeException()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int> { { GameSpeed.Normal, NormalTurnSeconds } };
            var turnDuration = new TurnDuration(secondsBySpeed);

            Assert.Throws<ArgumentOutOfRangeException>(() => turnDuration.GetSeconds(GameSpeed.Fast));
        }

        [Fact]
        public void Constructor_SpeedWithoutSeconds_ThrowsArgumentOutOfRangeException()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int> { { GameSpeed.Normal, NoSeconds } };

            Assert.Throws<ArgumentOutOfRangeException>(() => new TurnDuration(secondsBySpeed));
        }

        [Fact]
        public void Constructor_NullSeconds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TurnDuration(null));
        }

        [Fact]
        public void GetSeconds_DictionaryChangedAfterCreation_KeepsTheOriginalSeconds()
        {
            Dictionary<GameSpeed, int> secondsBySpeed = CreateSecondsBySpeed();
            var turnDuration = new TurnDuration(secondsBySpeed);
            secondsBySpeed[GameSpeed.Normal] = FastTurnSeconds;

            int seconds = turnDuration.GetSeconds(GameSpeed.Normal);

            Assert.Equal(NormalTurnSeconds, seconds);
        }

        private static Dictionary<GameSpeed, int> CreateSecondsBySpeed()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>
            {
                { GameSpeed.Normal, NormalTurnSeconds },
                { GameSpeed.Fast, FastTurnSeconds }
            };

            return secondsBySpeed;
        }
    }
}
