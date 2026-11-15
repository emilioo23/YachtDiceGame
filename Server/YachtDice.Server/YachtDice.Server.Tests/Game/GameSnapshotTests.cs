using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// What the copy of a finished game says when its result could not be saved.
    /// </summary>
    public class GameSnapshotTests
    {
        private const string PlayerId = "player-a";
        private const string PlayerName = "Emilio";
        private const int ExperienceGained = 280;
        private const int NoExperience = 0;

        [Fact]
        public void MarkResultNotSaved_PlayerWithExperience_TakesAwayTheExperienceAndMarksTheFailure()
        {
            var snapshot = new GameSnapshot();
            var player = new PlayerSnapshot(PlayerId, PlayerName, true) { ExperienceGained = ExperienceGained };

            snapshot.Players.Add(player);
            var snapshotExpected = new
            {
                HasResultSaveFailed = true,
                Players = new[] { new { ExperienceGained = NoExperience } }
            };

            snapshot.MarkResultNotSaved();

            Assert.Equivalent(snapshotExpected, snapshot);
        }
    }
}
