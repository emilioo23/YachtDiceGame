using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Client.Logic.Game;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Draws the five dice of the turn on the table. It draws the dice again each time the server sends a
    /// state, so it never decides a value.
    /// </summary>
    internal sealed class DiceBoard
    {
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
        private const int TumblingFrames = 9;
        private const int FaceCount = 6;
        private const int FirstFace = 1;
        private const int MaxTumblingTilt = 40;
        private const double LandingScale = 1.12;
        private const int LandingMilliseconds = 120;
        private const string NullValueMessage = "A value that the dice board needs cannot be null.";

        private static readonly (double left, double top, double angle)[] DicePositions =
        {
            (FirstDieLeft, FirstDieTop, FirstDieAngle),
            (SecondDieLeft, SecondDieTop, SecondDieAngle),
            (ThirdDieLeft, ThirdDieTop, ThirdDieAngle),
            (FourthDieLeft, FourthDieTop, FourthDieAngle),
            (FifthDieLeft, FifthDieTop, FifthDieAngle)
        };

        private static readonly TimeSpan LandingDuration = TimeSpan.FromMilliseconds(LandingMilliseconds);

        private readonly Canvas _canvas;
        private readonly FrameworkElement _resources;
        private readonly DiceRollAnimation _rollAnimation = new DiceRollAnimation();
        private readonly ElementAnimator _animator = new ElementAnimator();
        private readonly SecureRandom _random = new SecureRandom();

        /// <summary>
        /// Initializes the board inside a canvas.
        /// </summary>
        /// <param name="canvas">Canvas that receives the dice.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public DiceBoard(Canvas canvas, FrameworkElement resources)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas), NullValueMessage);
            _resources = resources ?? throw new ArgumentNullException(nameof(resources), NullValueMessage);
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
            _rollAnimation.Stop();
            _canvas.Children.Clear();

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                _canvas.Children.Add(CreateDie(index, ReadValue(values, index), isHeld(index)));
            }
        }

        public void Roll(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            _rollAnimation.Play(
                TumblingFrames,
                () => ShowTumblingFrame(values, isHeld),
                () => Land(values, isHeld));
        }

        private static int ReadValue(IReadOnlyList<int> values, int index)
        {
            return index < values.Count ? values[index] : NoValue;
        }

        private void ShowTumblingFrame(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            _canvas.Children.Clear();

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                bool isRolling = !isHeld(index);
                int value = ReadValue(values, index);

                if (isRolling)
                {
                    value = _random.Next(FaceCount) + FirstFace;
                }

                Border die = CreateDie(index, value, isHeld(index));

                if (isRolling)
                {
                    ((RotateTransform)die.RenderTransform).Angle += _random.Next(MaxTumblingTilt) - MaxTumblingTilt / 2;
                }

                _canvas.Children.Add(die);
            }
        }

        private void Land(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            Render(values, isHeld);

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (!isHeld(index))
                {
                    _animator.Pulse(_canvas.Children[index], LandingScale, LandingDuration);
                }
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
