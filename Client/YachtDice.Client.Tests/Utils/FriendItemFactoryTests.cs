using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Rows of the lists of friends shown in the menu.
    /// </summary>
    public class FriendItemFactoryTests
    {
        private const string AnaName = "Ana";
        private const string BetoName = "Beto";
        private const string CesarName = "Cesar";
        private const string LowerCaseBetoName = "beto";
        private const string AnaInitial = "A";
        private const string AnyStatus = "Status";

        private readonly string[] _friendNames = { CesarName, AnaName, BetoName };
        private readonly string[] _onlineNames = { LowerCaseBetoName };

        [Fact]
        public void Create_UnsortedNames_ReturnsThemOrderedByName()
        {
            List<FriendItem> items = FriendItemFactory.Create(_friendNames, AnyStatus, true);

            Assert.Equal(new[] { AnaName, BetoName, CesarName }, items.Select(item => item.Name).ToArray());
        }

        [Fact]
        public void Create_Name_ShowsItsInitial()
        {
            string[] names = { AnaName };

            List<FriendItem> items = FriendItemFactory.Create(names, AnyStatus, true);

            Assert.Equal(AnaInitial, items.Single().Initial);
        }

        [Fact]
        public void CreateOnline_OnlineNameWithOtherCase_ReturnsOnlyThatFriend()
        {
            List<FriendItem> items = FriendItemFactory.CreateOnline(_friendNames, _onlineNames);

            Assert.Equal(new[] { BetoName }, items.Select(item => item.Name).ToArray());
        }

        [Fact]
        public void CreateOnline_OnlineFriend_ShowsTheOnlineStatus()
        {
            List<FriendItem> items = FriendItemFactory.CreateOnline(_friendNames, _onlineNames);

            Assert.Equal(Strings.MainMenu_Friends_StatusOnline, items.Single().Status);
        }

        [Fact]
        public void CreateOffline_OneFriendOnline_ReturnsTheOtherFriends()
        {
            List<FriendItem> items = FriendItemFactory.CreateOffline(_friendNames, _onlineNames);

            Assert.Equal(new[] { AnaName, CesarName }, items.Select(item => item.Name).ToArray());
        }

        [Fact]
        public void CreateOffline_OfflineFriend_IsNotMarkedAsOnline()
        {
            List<FriendItem> items = FriendItemFactory.CreateOffline(_friendNames, _onlineNames);

            Assert.DoesNotContain(items, item => item.IsOnline);
        }
    }
}
