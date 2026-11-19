using System;
using System.Diagnostics;
using System.Windows.Media;

namespace YachtDice.Views
{
    /// <summary>
    /// The clock of the throw of the dice: it calls the board once for each frame that the screen draws, with
    /// the time that passed, until the total time is over. A new throw or a new drawing of the board stops the
    /// one that is still running.
    /// </summary>
    internal sealed class DiceThrowAnimation
    {
        private readonly Stopwatch _clock = new Stopwatch();
        private TimeSpan _total;
        private Action<TimeSpan> _showFrame;
        private Action _finish;
        private bool _isRunning;

        /// <summary>
        /// Starts the animation, or starts it again if it was running.
        /// </summary>
        /// <param name="total">Time that the animation lasts.</param>
        /// <param name="showFrame">Draws the dice for the time that passed since the start.</param>
        /// <param name="finish">Leaves the dice still, when the time is over.</param>
        public void Play(TimeSpan total, Action<TimeSpan> showFrame, Action finish)
        {
            _total = total;
            _showFrame = showFrame;
            _finish = finish;
            _clock.Restart();

            if (!_isRunning)
            {
                CompositionTarget.Rendering += HandleRendering;
                _isRunning = true;
            }
        }

        /// <summary>
        /// Stops the animation without calling the end of it.
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                CompositionTarget.Rendering -= HandleRendering;
                _clock.Stop();
                _isRunning = false;
            }
        }

        private void HandleRendering(object sender, EventArgs e)
        {
            TimeSpan elapsed = _clock.Elapsed;

            if (elapsed < _total)
            {
                _showFrame(elapsed);
            }
            else
            {
                Stop();
                _finish();
            }
        }
    }
}
