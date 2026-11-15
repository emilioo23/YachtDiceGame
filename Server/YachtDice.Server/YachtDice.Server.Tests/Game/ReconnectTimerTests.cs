using System;
using System.Threading;
using Xunit;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Services;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// The 60 seconds that a player who left a game has to come back, counted for each player of each room.
    /// The tests use one second so they finish fast.
    /// </summary>
    public class ReconnectTimerTests
    {
        private const string RoomCode = "ABC123";
        private const string OtherRoomCode = "XYZ789";
        private const string PlayerId = "player-1";
        private const string OtherPlayerId = "player-2";
        private const int OneSecond = 1;
        private const int LongWaitMilliseconds = 5000;
        private const int ShortWaitMilliseconds = 2000;

        private readonly ReconnectTimer _timer = new ReconnectTimer();
        private readonly ManualResetEventSlim _expired = new ManualResetEventSlim();
        private readonly RoomMember _member = new RoomMember(RoomCode, PlayerId);

        [Fact]
        public void Start_TimeRunsOut_RunsTheAction()
        {
            _timer.Start(_member, OneSecond, _expired.Set);

            bool hasExpired = _expired.Wait(LongWaitMilliseconds);

            Assert.True(hasExpired);
        }

        [Fact]
        public void Cancel_BeforeTheTimeRunsOut_DoesNotRunTheAction()
        {
            _timer.Start(_member, OneSecond, _expired.Set);
            _timer.Cancel(_member);

            bool hasExpired = _expired.Wait(ShortWaitMilliseconds);

            Assert.False(hasExpired);
        }

        [Fact]
        public void CancelRoom_TwoPlayersOfTheRoom_DoesNotRunAnyAction()
        {
            _timer.Start(_member, OneSecond, _expired.Set);
            _timer.Start(new RoomMember(RoomCode, OtherPlayerId), OneSecond, _expired.Set);
            _timer.CancelRoom(RoomCode);

            bool hasExpired = _expired.Wait(ShortWaitMilliseconds);

            Assert.False(hasExpired);
        }

        [Fact]
        public void CancelRoom_PlayerOfAnotherRoom_StillRunsItsAction()
        {
            _timer.Start(new RoomMember(OtherRoomCode, OtherPlayerId), OneSecond, _expired.Set);
            _timer.CancelRoom(RoomCode);

            bool hasExpired = _expired.Wait(LongWaitMilliseconds);

            Assert.True(hasExpired);
        }

        [Fact]
        public void Start_SamePlayerTwice_ForgetsTheFirstAction()
        {
            var secondExpired = new ManualResetEventSlim();

            _timer.Start(_member, OneSecond, _expired.Set);
            _timer.Start(_member, OneSecond, secondExpired.Set);
            secondExpired.Wait(LongWaitMilliseconds);

            Assert.False(_expired.IsSet);
        }

        [Fact]
        public void Cancel_PlayerWithoutCountdown_DoesNotThrow()
        {
            Exception exception = Record.Exception(() => _timer.Cancel(_member));

            Assert.Null(exception);
        }
    }
}
