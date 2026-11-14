using System;
using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// The five dice of a roll: how many show each face, their sum and the dice that the hand rejects.
    /// </summary>
    public class DiceHandTests
    {
        private const int One = 1;
        private const int Two = 2;
        private const int Three = 3;
        private const int Four = 4;
        private const int Five = 5;
        private const int Six = 6;
        private const int TooHighFace = 7;
        private const int ThreeDice = 3;
        private const int FullHouseSum = 13;
        private const int FourOfAKindCount = 4;
        private const int NoFace = 0;

        private static readonly int[] FullHouseDice = { Two, Two, Three, Three, Three };

        [Fact]
        public void CountFace_FullHouse_ReturnsHowManyDiceShowTheFace()
        {
            var hand = new DiceHand(FullHouseDice);

            int threes = hand.CountFace(Three);

            Assert.Equal(ThreeDice, threes);
        }

        [Fact]
        public void Sum_FullHouse_AddsAllTheDice()
        {
            var hand = new DiceHand(FullHouseDice);

            int sum = hand.Sum;

            Assert.Equal(FullHouseSum, sum);
        }

        [Fact]
        public void IsYacht_FiveEqualDice_ReturnsTrue()
        {
            var hand = new DiceHand(new[] { Six, Six, Six, Six, Six });

            bool isYacht = hand.IsYacht;

            Assert.True(isYacht);
        }

        [Fact]
        public void GetFaceRepeatedAtLeast_NoFaceRepeatedEnough_ReturnsZero()
        {
            var hand = new DiceHand(FullHouseDice);

            int face = hand.GetFaceRepeatedAtLeast(FourOfAKindCount);

            Assert.Equal(NoFace, face);
        }

        [Fact]
        public void ShowsEachFaceOnce_LittleStraight_ReturnsTrue()
        {
            var hand = new DiceHand(new[] { Five, Three, One, Two, Four });

            bool isStraight = hand.ShowsEachFaceOnce(One, Five);

            Assert.True(isStraight);
        }

        [Fact]
        public void Constructor_NullDice_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DiceHand(null));
        }

        [Fact]
        public void Constructor_FourDice_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DiceHand(new[] { One, Two, Three, Five }));
        }

        [Fact]
        public void Constructor_FaceAboveSix_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new DiceHand(new[] { One, Two, Three, Five, TooHighFace }));
        }
    }
}
