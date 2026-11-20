using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using YachtDice.Client.Logic.Connection;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Connection
{
    /// <summary>
    /// The repeated attempts to get back the connection of a match, with a fake attempt and a fake clock.
    /// </summary>
    public class ReconnectLoopTests
    {
        private const int NoFailures = 0;
        private const int OneFailure = 1;
        private const int TwoFailures = 2;
        private const int NeverWorks = int.MaxValue;
        private const int NoSeconds = 0;
        private const int NoAttempts = 0;
        private const int OneAttempt = 1;
        private const int FirstAttempt = 1;
        private const int SecondAttempt = 2;
        private const int ThirdAttempt = 3;

        private readonly List<int> _waits = new List<int>();
        private readonly List<int> _startedAttempts = new List<int>();
        private int _failuresBeforeSuccess = NoFailures;
        private int _elapsedSeconds = NoSeconds;
        private int _attempts = NoAttempts;

        [Fact]
        public void Constructor_NullTryRestore_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReconnectLoop(null, WaitAsync, GetElapsedSeconds));
        }

        [Fact]
        public void Constructor_NullWait_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReconnectLoop(TryRestoreAsync, null, GetElapsedSeconds));
        }

        [Fact]
        public void Constructor_NullElapsedSeconds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReconnectLoop(TryRestoreAsync, WaitAsync, null));
        }

        [Fact]
        public async Task RunAsync_FirstAttemptWorks_ReturnsTrue()
        {
            ReconnectLoop loop = CreateLoop();

            bool isRestored = await loop.RunAsync();

            Assert.True(isRestored);
        }

        [Fact]
        public async Task RunAsync_FirstAttemptWorks_DoesNotWait()
        {
            ReconnectLoop loop = CreateLoop();

            await loop.RunAsync();

            Assert.Empty(_waits);
        }

        [Fact]
        public async Task RunAsync_SecondAttemptWorks_ReturnsTrue()
        {
            _failuresBeforeSuccess = OneFailure;
            ReconnectLoop loop = CreateLoop();

            bool isRestored = await loop.RunAsync();

            Assert.True(isRestored);
        }

        [Fact]
        public async Task RunAsync_SecondAttemptWorks_WaitsOnceTheRetryInterval()
        {
            _failuresBeforeSuccess = OneFailure;
            ReconnectLoop loop = CreateLoop();

            await loop.RunAsync();

            Assert.Equal(new[] { ReconnectLoop.RetryIntervalSeconds }, _waits);
        }

        [Fact]
        public async Task RunAsync_ThirdAttemptWorks_ReportsTheNumberOfEachAttempt()
        {
            _failuresBeforeSuccess = TwoFailures;
            ReconnectLoop loop = CreateLoop();
            loop.AttemptStarted += _startedAttempts.Add;

            await loop.RunAsync();

            Assert.Equal(new[] { FirstAttempt, SecondAttempt, ThirdAttempt }, _startedAttempts);
        }

        [Fact]
        public async Task RunAsync_NeverWorks_ReturnsFalse()
        {
            _failuresBeforeSuccess = NeverWorks;
            ReconnectLoop loop = CreateLoop();

            bool isRestored = await loop.RunAsync();

            Assert.False(isRestored);
        }

        [Fact]
        public async Task RunAsync_NeverWorks_StopsWhenTheServerStopsWaiting()
        {
            _failuresBeforeSuccess = NeverWorks;
            ReconnectLoop loop = CreateLoop();
            int attemptsInTheTimeOfTheServer = ReconnectWaitTracker.ReconnectSeconds
                / ReconnectLoop.RetryIntervalSeconds;

            await loop.RunAsync();

            Assert.Equal(attemptsInTheTimeOfTheServer, _attempts);
        }

        [Fact]
        public async Task RunAsync_TimeAlreadyOver_DoesNotTry()
        {
            _elapsedSeconds = ReconnectWaitTracker.ReconnectSeconds;
            ReconnectLoop loop = CreateLoop();

            await loop.RunAsync();

            Assert.Equal(NoAttempts, _attempts);
        }

        private ReconnectLoop CreateLoop()
        {
            return new ReconnectLoop(TryRestoreAsync, WaitAsync, GetElapsedSeconds);
        }

        private Task<bool> TryRestoreAsync()
        {
            _attempts += OneAttempt;

            return Task.FromResult(_attempts > _failuresBeforeSuccess);
        }

        private Task WaitAsync(int seconds)
        {
            _waits.Add(seconds);
            _elapsedSeconds += seconds;

            return Task.CompletedTask;
        }

        private int GetElapsedSeconds()
        {
            return _elapsedSeconds;
        }
    }
}
