using System;
using Xunit;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Client.Tests.Dice
{
    /// <summary>
    /// Where each face of the cube is and the turn that leaves it on top.
    /// </summary>
    public class DiceFaceOrientationTests
    {
        private const int RoundingDecimals = 6;
        private const int NoFace = 0;
        private const int FaceSeven = 7;
        private const int OppositeFacesSum = 7;

        private static readonly DiceVector UpDirection = new DiceVector(0, 1, 0);

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void GetFaceUp_EachFace_PutsTheFaceOnTop(int face)
        {
            FaceUpRotation rotation = DiceFaceOrientation.GetFaceUp(face);

            DiceVector upResult = rotation.Rotate(DiceFaceOrientation.GetNormal(face)).Round(RoundingDecimals);

            Assert.Equivalent(UpDirection, upResult);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GetNormal_OppositeFace_PointsToTheOppositeDirection(int face)
        {
            DiceVector normalExpected = DiceFaceOrientation.GetNormal(face).Negate();

            DiceVector normalResult = DiceFaceOrientation.GetNormal(OppositeFacesSum - face);

            Assert.Equivalent(normalExpected, normalResult);
        }

        [Theory]
        [InlineData(NoFace)]
        [InlineData(FaceSeven)]
        public void GetNormal_ValueThatIsNotAFace_ThrowsArgumentOutOfRangeException(int face)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceFaceOrientation.GetNormal(face));
        }

        [Theory]
        [InlineData(NoFace)]
        [InlineData(FaceSeven)]
        public void GetFaceUp_ValueThatIsNotAFace_ThrowsArgumentOutOfRangeException(int face)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DiceFaceOrientation.GetFaceUp(face));
        }
    }
}
