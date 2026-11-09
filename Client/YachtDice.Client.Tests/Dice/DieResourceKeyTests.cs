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

        [Fact]
        public void Create_FaceNumber_ReturnsTheNameWithThatNumber()
        {
            Assert.Equal(FourthFaceKey, DieResourceKey.Create(FourthFace));
        }
    }
}
