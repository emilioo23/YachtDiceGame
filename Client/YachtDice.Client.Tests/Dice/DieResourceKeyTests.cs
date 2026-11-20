using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// Rules of the resource name that each die face uses.
    /// </summary>
    public class DieResourceKeyTests
    {
        private const int FourthFace = 4;
        private const string FourthFaceKey = "Die_4";
        private const int SixthFace = 6;
        private const string SixthFaceKey = "Die_6";
        private const int SeventhFace = 7;
        private const string SeventhFaceKey = "Die_7";
        private const int NoFace = 0;
        private const string NoFaceKey = "Die_0";
        private const int NegativeFace = -1;
        private const string NegativeFaceKey = "Die_-1";
        private const string FirstFaceKey = "Die_1";

        [Fact]
        public void Create_FaceNumber_ReturnsTheNameWithThatNumber()
        {
            Assert.Equal(FourthFaceKey, DieResourceKey.Create(FourthFace));
        }

        [Fact]
        public void Create_DefaultValue_ReturnsTheNameOfTheFirstFace()
        {
            string key = DieResourceKey.Create(DieResourceKey.DefaultValue);

            Assert.Equal(FirstFaceKey, key);
        }

        [Fact]
        public void Create_HighestFace_ReturnsTheNameWithThatNumber()
        {
            string key = DieResourceKey.Create(SixthFace);

            Assert.Equal(SixthFaceKey, key);
        }

        [Fact]
        public void Create_Zero_ReturnsTheNameWithZero()
        {
            string key = DieResourceKey.Create(NoFace);

            Assert.Equal(NoFaceKey, key);
        }

        [Fact]
        public void Create_NegativeNumber_KeepsTheMinusSign()
        {
            string key = DieResourceKey.Create(NegativeFace);

            Assert.Equal(NegativeFaceKey, key);
        }

        [Fact]
        public void Create_NumberAboveTheFaces_ReturnsTheNameWithThatNumber()
        {
            string key = DieResourceKey.Create(SeventhFace);

            Assert.Equal(SeventhFaceKey, key);
        }
    }
}
