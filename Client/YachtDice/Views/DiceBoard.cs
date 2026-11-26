using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;
using YachtDice.Client.Logic.Game;
using YachtDice.Client.Logic.Logging;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Shows the five dice of the turn in three dimensions. A roll gathers the dice that are not held in the
    /// hand, near the edge of the table, and throws them: they turn in the air, bounce and land in free places
    /// of the table. The dice that the player holds wait in a row under the table, and a die goes there or
    /// comes back with a short hop when it is held or released. The board never decides a value: it only
    /// shows what the server sent.
    /// </summary>
    internal sealed class DiceBoard
    {
        private const double ViewportWidth = 413;
        private const double ViewportHeight = 560;
        private const double ViewportOffsetX = -62;
        private const double ViewportOffsetY = -135;
        private const double ThrowMilliseconds = 1300;
        private const double GlideMilliseconds = 380;
        private const double GlideStaggerMilliseconds = 45;
        private const double HandDistance = 3;
        private const double HandSpread = 0.9;
        private const double Half = 0.5;
        private const double FullTurnDegrees = 360;
        private const int MinTumbleTurns = 2;
        private const int ExtraTumbleTurns = 2;
        private const double SameSpotTolerance = 0.01;
        private const double NoDuration = 0;
        private const double NoYaw = 0;
        private const int NoValue = 0;
        private const int IdleFaceOffset = 1;

        private readonly Viewport3D _viewport;
        private readonly Die3D[] _dice = new Die3D[HeldDiceSelection.DiceCount];
        private readonly Dictionary<Visual3D, int> _indexByVisual = new Dictionary<Visual3D, int>();
        private readonly DiceThrowAnimation _animation = new DiceThrowAnimation();
        private readonly SecureRandom _random = new SecureRandom();

        /// <summary>
        /// Initializes the board inside a canvas.
        /// </summary>
        /// <param name="canvas">Canvas of the table; the three-dimensional view is bigger than the canvas, so
        /// the row under the table fits in it. Before the first roll each die shows a different face only as
        /// decoration; the real value always comes from the server.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public DiceBoard(Canvas canvas, FrameworkElement resources)
        {
            Canvas host = canvas ?? throw new ArgumentNullException(nameof(canvas), LogMessages.NullCanvasMessage);
            FrameworkElement owner = resources ?? throw new ArgumentNullException(
                nameof(resources),
                LogMessages.NullResourcesMessage);
            DiceMaterials materials = DiceMaterials.Create(owner);

            _viewport = DiceViewportFactory.Create(ViewportWidth, ViewportHeight);
            _viewport.Cursor = Cursors.Hand;
            Canvas.SetLeft(_viewport, ViewportOffsetX);
            Canvas.SetTop(_viewport, ViewportOffsetY);

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                var die = new Die3D(materials);

                die.PlaceAt(DiceTableLayout.GetRowSlot(index), NoYaw, DiceLocation.Table);
                die.ShowValue(ReadValue(Array.Empty<int>(), index));
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
        /// Shows the dice with their values. The dice that are held go to the row under the table and the ones
        /// that are not held lie on the table; a die that changes place does it with a short hop. It stops a
        /// throw that is still running.
        /// </summary>
        /// <param name="values">Value of each die; a missing or zero value is a die not rolled yet, which
        /// shows a decorative face.</param>
        /// <param name="isHeld">Tells if a die is held, given its position.</param>
        public void Render(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            PrepareDice(values);

            int rackCount = CountHeld(isHeld);
            List<DiceSlot> occupied = FindTablePlaces(isHeld);
            double finish = NoDuration;
            var rackPosition = 0;

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (isHeld(index))
                {
                    finish = Math.Max(finish, SendToRack(index, rackPosition, rackCount));
                    rackPosition++;
                }
                else
                {
                    finish = Math.Max(finish, ReturnToTable(index, occupied));
                }
            }

            Play(finish);
        }

        /// <summary>
        /// Throws the dice that are not held. They go to the hand, fly turning in the air, bounce and land in free
        /// places of the table with the value of the roll on top. The dice that are held stay in the row.
        /// </summary>
        /// <param name="values">Value of each die, as the server rolled it.</param>
        /// <param name="isHeld">Tells if a die is held, given its position.</param>
        public void Roll(IReadOnlyList<int> values, Func<int, bool> isHeld)
        {
            PrepareDice(values);

            int rackCount = CountHeld(isHeld);
            var landed = new List<DiceSlot>();
            double finish = NoDuration;
            var rackPosition = 0;

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (isHeld(index))
                {
                    finish = Math.Max(finish, SendToRack(index, rackPosition, rackCount));
                    rackPosition++;
                }
                else
                {
                    finish = Math.Max(finish, Throw(index, landed));
                }
            }

            Play(finish);
        }

        private static int ReadValue(IReadOnlyList<int> values, int index)
        {
            bool isRolled = index < values.Count && values[index] > NoValue;

            return isRolled ? values[index] : index + IdleFaceOffset;
        }

        private static int CountHeld(Func<int, bool> isHeld)
        {
            var count = 0;

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (isHeld(index))
                {
                    count++;
                }
            }

            return count;
        }

        private static DieMotion CreateGlide(DiceSlot end, DiceLocation location, double delayMilliseconds)
        {
            return new DieMotion
            {
                IsThrow = false,
                End = end,
                Location = location,
                DelayMilliseconds = delayMilliseconds,
                DurationMilliseconds = GlideMilliseconds
            };
        }

        private void PrepareDice(IReadOnlyList<int> values)
        {
            _animation.Stop();

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                _dice[index].Settle();
                _dice[index].ShowValue(ReadValue(values, index));
            }
        }

        private List<DiceSlot> FindTablePlaces(Func<int, bool> isHeld)
        {
            var places = new List<DiceSlot>();

            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (!isHeld(index) && _dice[index].Location == DiceLocation.Table)
                {
                    places.Add(new DiceSlot(_dice[index].X, _dice[index].Z));
                }
            }

            return places;
        }

        private double SendToRack(int index, int position, int count)
        {
            Die3D die = _dice[index];
            DiceSlot slot = DiceTableLayout.GetRackSlot(position, count);
            bool isThere = die.Location == DiceLocation.Rack
                && Math.Abs(die.X - slot.X) < SameSpotTolerance
                && Math.Abs(die.Z - slot.Z) < SameSpotTolerance;
            double finish = NoDuration;

            if (!isThere)
            {
                DieMotion motion = CreateGlide(slot, DiceLocation.Rack, position * GlideStaggerMilliseconds);

                motion.YawDegrees = NoYaw;
                die.Begin(motion);
                finish = motion.FinishMilliseconds;
            }

            return finish;
        }

        private double ReturnToTable(int index, List<DiceSlot> occupied)
        {
            Die3D die = _dice[index];
            double finish = NoDuration;

            if (die.Location == DiceLocation.Rack)
            {
                DiceSlot slot = DiceLanding.Choose(occupied, _random.NextDouble);
                DieMotion motion = CreateGlide(slot, DiceLocation.Table, NoDuration);

                occupied.Add(slot);
                motion.YawDegrees = NextYaw();
                die.Begin(motion);
                finish = motion.FinishMilliseconds;
            }

            return finish;
        }

        private double Throw(int index, List<DiceSlot> landed)
        {
            DiceSlot slot = DiceLanding.Choose(landed, _random.NextDouble);
            int tumbleTurns = MinTumbleTurns + _random.Next(ExtraTumbleTurns);
            var motion = new DieMotion
            {
                IsThrow = true,
                End = slot,
                Location = DiceLocation.Table,
                YawDegrees = NextYaw(),
                Hand = new DiceSlot(NextSpread(), HandDistance + NextSpread()),
                TumbleAxis = CreateTumbleAxis(),
                TumbleDegrees = tumbleTurns * FullTurnDegrees,
                DelayMilliseconds = NoDuration,
                DurationMilliseconds = ThrowMilliseconds
            };

            landed.Add(slot);
            _dice[index].Begin(motion);

            return motion.FinishMilliseconds;
        }

        private double NextYaw()
        {
            return _random.NextDouble() * FullTurnDegrees;
        }

        private double NextSpread()
        {
            return (_random.NextDouble() - Half) * HandSpread;
        }

        private Vector3D CreateTumbleAxis()
        {
            return new Vector3D(_random.NextDouble() - Half, _random.NextDouble(), _random.NextDouble() - Half);
        }

        private void Play(double finishMilliseconds)
        {
            if (finishMilliseconds > NoDuration)
            {
                _animation.Play(TimeSpan.FromMilliseconds(finishMilliseconds), ShowFrame, Land);
            }
        }

        private void ShowFrame(TimeSpan elapsed)
        {
            for (var index = 0; index < HeldDiceSelection.DiceCount; index++)
            {
                if (_dice[index].IsMoving)
                {
                    _dice[index].Advance(elapsed.TotalMilliseconds);
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
