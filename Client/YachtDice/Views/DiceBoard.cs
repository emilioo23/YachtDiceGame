using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;
using YachtDice.Client.Logic.Game;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Shows the five dice of the turn on the table in three dimensions. The dice that are not held are thrown
    /// from the place where they lie and fall in a new one; the ones that are held stay still, a little higher
    /// and with a gold mark. The board never decides a value: it only shows what the server sent.
    /// </summary>
    internal sealed class DiceBoard
    {
        private const double ViewportSize = 330;
        private const int ThrowMilliseconds = 1100;
        private const int StaggerMilliseconds = 80;
        private const double SlotJitterSpan = 0.3;
        private const double Half = 0.5;
        private const double FullTurnDegrees = 360;
        private const int MinTumbleTurns = 2;
        private const int ExtraTumbleTurns = 2;
        private const double InitialYawStep = 37;
        private const int NoValue = 0;
        private const string NullCanvasMessage = "The canvas where it is drawn cannot be null.";
        private const string NullResourcesMessage = "The element that holds the resources cannot be null.";

        private static readonly TimeSpan TotalDuration = TimeSpan.FromMilliseconds(
            ThrowMilliseconds + ((HeldDiceSelection.DiceCount - 1) * StaggerMilliseconds));

        private readonly Viewport3D _viewport;
        private readonly Die3D[] _dice = new Die3D[HeldDiceSelection.DiceCount];
        private readonly Dictionary<Visual3D, int> _indexByVisual = new Dictionary<Visual3D, int>();
        private readonly DiceThrowAnimation _animation = new DiceThrowAnimation();
        private readonly SecureRandom _random = new SecureRandom();

        /// <summary>
        /// Initializes the board inside a canvas.
        /// </summary>
        /// <param name="canvas">Canvas that receives the three-dimensional view of the table.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public DiceBoard(Canvas canvas, FrameworkElement resources)
        {
            Canvas host = canvas ?? throw new ArgumentNullException(nameof(canvas), NullCanvasMessage);
            FrameworkElement owner = resources ?? throw new ArgumentNullException(
                nameof(resources),
                NullResourcesMessage);
            DiceMaterials materials = DiceMaterials.Create(owner);

            _viewport = DiceViewportFactory.Create(ViewportSize);
            _viewport.Cursor = Cursors.Hand;

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                var die = new Die3D(materials);

                die.PlaceAt(DiceTableLayout.GetSlot(index), index * InitialYawStep);
                die.ShowValue(NoValue);
                _dice[index] = die;
                _indexByVisual[die.Body] = index;
                _viewport.Children.Add(die.Root);
            }

            _viewport.MouseLeftButtonUp += HandleViewportClick;
            host.Children.Add(_viewport);
        }

        /// <summary>
        /// Event triggered when the player clicks a die. It carries the position of the die.
        /// </summary>
        public event Action<int> DieClicked;

        /// <summary>
        /// Shows the dice still, in the place where they lie, with their values and their held marks. It stops
        /// a throw that is still running.
        /// </summary>
        /// <param name="values">Value of each die; a missing or zero value is a die not rolled yet.</param>
        /// <param name="isHeld">Tells if a die is held, given its position.</param>
        public void Render(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            _animation.Stop();

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                _dice[index].Settle();
                _dice[index].ShowValue(ReadValue(values, index));
                _dice[index].SetHeld(isHeld(index));
            }
        }

        /// <summary>
        /// Throws the dice that are not held. They leave the place where they lie, turn in the air, bounce and
        /// stay in a new place with the value of the roll on top.
        /// </summary>
        /// <param name="values">Value of each die, as the server rolled it.</param>
        /// <param name="isHeld">Tells if a die is held, given its position.</param>
        public void Roll(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            Render(values, isHeld);

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (!isHeld(index))
                {
                    _dice[index].StartThrow(CreateThrow(index));
                }
            }

            _animation.Play(TotalDuration, ShowFlight, Land);
        }

        private static int ReadValue(IReadOnlyList<int> values, int index)
        {
            return index < values.Count ? values[index] : NoValue;
        }

        private double NextOffset()
        {
            return (_random.NextDouble() - Half) * SlotJitterSpan;
        }

        private Vector3D CreateTumbleAxis()
        {
            return new Vector3D(_random.NextDouble() - Half, _random.NextDouble(), _random.NextDouble() - Half);
        }

        private DieThrow CreateThrow(int index)
        {
            DiceSlot slot = DiceTableLayout.GetSlot(index);
            int tumbleTurns = MinTumbleTurns + _random.Next(ExtraTumbleTurns);

            return new DieThrow
            {
                EndX = slot.X + NextOffset(),
                EndZ = slot.Z + NextOffset(),
                YawDegrees = _random.NextDouble() * FullTurnDegrees,
                TumbleAxis = CreateTumbleAxis(),
                TumbleDegrees = tumbleTurns * FullTurnDegrees
            };
        }

        private void ShowFlight(TimeSpan elapsed)
        {
            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (_dice[index].IsFlying)
                {
                    double delay = index * StaggerMilliseconds;

                    _dice[index].Fly((elapsed.TotalMilliseconds - delay) / ThrowMilliseconds);
                }
            }
        }

        private void Land()
        {
            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                _dice[index].Settle();
            }
        }

        private void HandleViewportClick(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hit = VisualTreeHelper.HitTest(_viewport, e.GetPosition(_viewport));
            var meshHit = hit as RayMeshGeometry3DHitTestResult;

            if (meshHit != null && _indexByVisual.TryGetValue(meshHit.VisualHit, out int index))
            {
                DieClicked?.Invoke(index);
            }
        }
    }
}
