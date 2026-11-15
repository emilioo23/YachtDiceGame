using System;
using System.Windows.Threading;

namespace YachtDice.Views
{
    /// <summary>
    /// The dice tumbling after a roll: shows a few frames one after another and then lets the dice land. A new
    /// roll or a new drawing of the board stops the tumbling that is still running.
    /// </summary>
    internal sealed class DiceRollAnimation
    {
        private const int FrameMilliseconds = 70;
        private const int NoFramesLeft = 0;

        private readonly DispatcherTimer _timer;
        private int _framesLeft;
        private Action _showTumblingFrame;
        private Action _land;

        /// <summary>
        /// Initializes the animation, stopped.
        /// </summary>
        public DiceRollAnimation()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FrameMilliseconds) };
            _timer.Tick += HandleTick;
        }

        /// <summary>
        /// Starts the tumbling.
        /// </summary>
        /// <param name="frameCount">Frames that the dice tumble before they land.</param>
        /// <param name="showTumblingFrame">Draws the dice in the middle of the roll.</param>
        /// <param name="land">Draws the dice with the values of the roll.</param>
        public void Play(int frameCount, Action showTumblingFrame, Action land)
        {
            _framesLeft = frameCount;
            _showTumblingFrame = showTumblingFrame;
            _land = land;
            _showTumblingFrame();
            _timer.Start();
        }

        /// <summary>
        /// Stops the tumbling without letting the dice land.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        private void HandleTick(object sender, EventArgs e)
        {
            _framesLeft--;

            if (_framesLeft > NoFramesLeft)
            {
                _showTumblingFrame();
            }
            else
            {
                _timer.Stop();
                _land();
            }
        }
    }
}
