using System;
using System.Collections.Generic;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// Where each face of the cube is and the turn that leaves a face on top. The faces that are opposite add
    /// seven, as in a real die.
    /// </summary>
    public static class DiceFaceOrientation
    {
        /// <summary>
        /// Value of the first face of a die.
        /// </summary>
        public const int FirstFace = 1;

        /// <summary>
        /// Value of the last face of a die.
        /// </summary>
        public const int LastFace = 6;

        private const int FaceOne = 1;
        private const int FaceTwo = 2;
        private const int FaceThree = 3;
        private const int FaceFour = 4;
        private const int FaceFive = 5;
        private const int FaceSix = 6;
        private const double NoTurnDegrees = 0;
        private const double QuarterTurnDegrees = 90;
        private const double HalfTurnDegrees = 180;

        private static readonly DiceVector XAxis = new DiceVector(1, 0, 0);
        private static readonly DiceVector YAxis = new DiceVector(0, 1, 0);
        private static readonly DiceVector ZAxis = new DiceVector(0, 0, 1);

        private static readonly Dictionary<int, DiceVector> Normals = new Dictionary<int, DiceVector>
        {
            { FaceOne, YAxis },
            { FaceTwo, ZAxis },
            { FaceThree, XAxis },
            { FaceFour, XAxis.Negate() },
            { FaceFive, ZAxis.Negate() },
            { FaceSix, YAxis.Negate() }
        };

        private static readonly Dictionary<int, FaceUpRotation> FaceUpRotations =
            new Dictionary<int, FaceUpRotation>
            {
                { FaceOne, new FaceUpRotation(YAxis, NoTurnDegrees) },
                { FaceTwo, new FaceUpRotation(XAxis, -QuarterTurnDegrees) },
                { FaceThree, new FaceUpRotation(ZAxis, QuarterTurnDegrees) },
                { FaceFour, new FaceUpRotation(ZAxis, -QuarterTurnDegrees) },
                { FaceFive, new FaceUpRotation(XAxis, QuarterTurnDegrees) },
                { FaceSix, new FaceUpRotation(XAxis, HalfTurnDegrees) }
            };

        /// <summary>
        /// Gets the direction that a face points to when the die is in its model position, with the face one on
        /// top.
        /// </summary>
        /// <param name="face">Value of the face, from one to six.</param>
        /// <returns>The direction that goes out of the face.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a face of a die.</exception>
        public static DiceVector GetNormal(int face)
        {
            DiceVector normal;

            if (!Normals.TryGetValue(face, out normal))
            {
                throw new ArgumentOutOfRangeException(nameof(face), LogMessages.InvalidFaceMessage);
            }

            return normal;
        }

        /// <summary>
        /// Gets the turn that leaves a face on top of the die.
        /// </summary>
        /// <param name="face">Value of the face, from one to six.</param>
        /// <returns>The rotation to apply to the cube in its model position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a face of a die.</exception>
        public static FaceUpRotation GetFaceUp(int face)
        {
            FaceUpRotation rotation;

            if (!FaceUpRotations.TryGetValue(face, out rotation))
            {
                throw new ArgumentOutOfRangeException(nameof(face), LogMessages.InvalidFaceMessage);
            }

            return rotation;
        }
    }
}
