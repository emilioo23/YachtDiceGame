using System.Collections.Generic;
using System.Threading.Tasks;
using YachtDice.Client.Logic.Players;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.PlayerServiceReference;
using YachtDice.Views;

namespace YachtDice.Utils
{
    /// <summary>
    /// Shows a notice for each achievement that the player unlocked since the last time it checked.
    /// </summary>
    public class AchievementNotifier
    {
        private readonly PlayerSession _session;
        private readonly PlayerGateway _gateway = new PlayerGateway();

        /// <summary>
        /// Initializes the notifier for the player of a session.
        /// </summary>
        /// <param name="session">Session of the player.</param>
        public AchievementNotifier(PlayerSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Reads the achievements of the player and shows a notice for each one that is new. The first time it
        /// only remembers them, so the player is not told about achievements from before.
        /// </summary>
        /// <returns>A task that finishes when the notices are shown.</returns>
        /// <exception cref="GatewayException">The server reported an error or could not be reached.</exception>
        public async Task CheckAsync()
        {
            if (!_session.IsGuest)
            {
                List<AchievementInfo> achievements = await _gateway.GetAchievementsAsync(_session.DisplayName);
                List<int> currentIds = FindUnlockedIds(achievements);

                if (_session.KnownAchievementIds != null)
                {
                    ShowNotices(AchievementChanges.FindNewlyUnlocked(_session.KnownAchievementIds, currentIds));
                }

                _session.KnownAchievementIds = currentIds;
            }
        }

        private static List<int> FindUnlockedIds(List<AchievementInfo> achievements)
        {
            var unlockedIds = new List<int>();

            foreach (AchievementInfo achievement in achievements)
            {
                if (achievement.IsUnlocked)
                {
                    unlockedIds.Add(achievement.Id);
                }
            }

            return unlockedIds;
        }

        private static void ShowNotices(List<int> newIds)
        {
            for (int position = 0; position < newIds.Count; position++)
            {
                new AchievementToastWindow(newIds[position], position).Show();
            }
        }
    }
}
