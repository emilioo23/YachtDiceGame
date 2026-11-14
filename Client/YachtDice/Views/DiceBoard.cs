using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YachtDice.Views
{
    /// <summary>
    /// Draws the five dice of the turn on the table. It draws the dice again each time the server sends a
    /// state, so it never decides a value.
    /// </summary>
    internal sealed class DiceBoard
    {
        private const int DiceCount = 5;
        private const int NoValue = 0;
        private const double HalfDivisor = 2;
        private const double FirstDieLeft = 150;
        private const double FirstDieTop = 40;
        private const double FirstDieAngle = -8;
        private const double SecondDieLeft = 230;
        private const double SecondDieTop = 90;
        private const double SecondDieAngle = 10;
        private const double ThirdDieLeft = 190;
        private const double ThirdDieTop = 150;
        private const double ThirdDieAngle = -5;
        private const double FourthDieLeft = 90;
        private const double FourthDieTop = 220;
        private const double FourthDieAngle = 12;
        private const double FifthDieLeft = 250;
        private const double FifthDieTop = 230;
        private const double FifthDieAngle = -12;

        private static readonly (double left, double top, double angle)[] DicePositions =
        {
            (FirstDieLeft, FirstDieTop, FirstDieAngle),
            (SecondDieLeft, SecondDieTop, SecondDieAngle),
            (ThirdDieLeft, ThirdDieTop, ThirdDieAngle),
            (FourthDieLeft, FourthDieTop, FourthDieAngle),
            (FifthDieLeft, FifthDieTop, FifthDieAngle)
        };

        private readonly Canvas _canvas;
        private readonly FrameworkElement _resources;

        /// <summary>
        /// Initializes the board inside a canvas.
        /// </summary>
        /// <param name="canvas">Canvas that receives the dice.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public DiceBoard(Canvas canvas, FrameworkElement resources)
        {
            _canvas = canvas;
            _resources = resources;
        }

        /// <summary>
        /// Event triggered when the player clicks a die. It carries the position of the die.
        /// </summary>
        public event Action<int> DieClicked;

        /// <summary>
        /// Draws the dice again.
        /// </summary>
        /// <param name="values">Value of each die; a missing or zero value is a die not rolled yet.</param>
        /// <param name="isHeld">Tells if a die is held, given its position.</param>
        public void Render(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            _canvas.Children.Clear();

            for (var index = 0; index < DiceCount; index++)
            {
                int value = index < values.Count ? values[index] : NoValue;

                _canvas.Children.Add(CreateDie(index, value, isHeld(index)));
            }
        }

        private Border CreateDie(int index, int value, bool isHeld)
        {
            Border die = DiceFaceFactory.Create(_resources, value, isHeld);
            (double left, double top, double angle) position = DicePositions[index];
            double center = DiceFaceFactory.DiceSize / HalfDivisor;

            Canvas.SetLeft(die, position.left);
            Canvas.SetTop(die, position.top);
            die.RenderTransform = new RotateTransform(position.angle, center, center);
            die.Cursor = Cursors.Hand;
            die.MouseLeftButtonUp += (sender, e) => DieClicked?.Invoke(index);

            return die;
        }
    }
}
