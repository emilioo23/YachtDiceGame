using System;
using System.Collections.Generic;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// The cells of the grid that have a pip in each face of a die.
    /// </summary>
    public static class DicePipLayout
    {
        private const int NoValue = 0;
        private const int FaceOne = 1;
        private const int FaceTwo = 2;
        private const int FaceThree = 3;
        private const int FaceFour = 4;
        private const int FaceFive = 5;
        private const int FaceSix = 6;
        private const int Low = 0;
        private const int Middle = 1;
        private const int High = 2;

        private static readonly Dictionary<int, PipCell[]> Layouts = new Dictionary<int, PipCell[]>
        {
            { NoValue, new PipCell[] { } },
            { FaceOne, new[] { new PipCell(Middle, Middle) } },
            { FaceTwo, new[] { new PipCell(Low, Low), new PipCell(High, High) } },
            { FaceThree, new[] { new PipCell(Low, Low), new PipCell(Middle, Middle), new PipCell(High, High) } },
            {
                FaceFour,
                new[] { new PipCell(Low, Low), new PipCell(Low, High), new PipCell(High, Low), new PipCell(High, High) }
            },
            {
                FaceFive,
                new[]
                {
                    new PipCell(Low, Low),
                    new PipCell(Low, High),
                    new PipCell(Middle, Middle),
                    new PipCell(High, Low),
                    new PipCell(High, High)
                }
            },
            {
                FaceSix,
                new[]
                {
                    new PipCell(Low, Low),
                    new PipCell(Low, High),
                    new PipCell(Middle, Low),
                    new PipCell(Middle, High),
                    new PipCell(High, Low),
                    new PipCell(High, High)
                }
            }
        };

        /// <summary>
        /// Gets the cells that have a pip in a face.
        /// </summary>
        /// <param name="value">Value of the face from one to six; zero is a die that was not rolled yet.</param>
        /// <returns>The cells with a pip; none when the value is zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The value is not between zero and six.</exception>
        public static IReadOnlyList<PipCell> GetCells(int value)
        {
            PipCell[] cells;

            if (!Layouts.TryGetValue(value, out cells))
            {
                throw new ArgumentOutOfRangeException(nameof(value), LogMessages.InvalidValueMessage);
            }

            return cells;
        }
    }
}
