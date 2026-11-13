using System.Collections.Generic;
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
        private const string BetoInitial = "B";
        private const string CesarInitial = "C";
        private const string AnyStatus = "Status";

        private readonly string[] _friendNames = { CesarName, AnaName, BetoName };
        private readonly string[] _onlineNames = { LowerCaseBetoName };

        [Fact]
        public void Create_UnsortedNames_ReturnsThemOrderedByName()
        {
            FriendItem[] expected =
            {
                CreateItem(AnaName, AnaInitial, AnyStatus, true),
                CreateItem(BetoName, BetoInitial, AnyStatus, true),
                CreateItem(CesarName, CesarInitial, AnyStatus, true)
            };

            List<FriendItem> items = FriendItemFactory.Create(_friendNames, AnyStatus, true);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void Create_Name_ShowsItsInitial()
        {
            string[] names = { AnaName };
            FriendItem[] expected = { CreateItem(AnaName, AnaInitial, AnyStatus, true) };

            List<FriendItem> items = FriendItemFactory.Create(names, AnyStatus, true);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void CreateOnline_OnlineNameWithOtherCase_ReturnsOnlyThatFriend()
        {
            FriendItem[] expected =
            {
                CreateItem(BetoName, BetoInitial, Strings.MainMenu_Friends_StatusOnline, true)
            };

            List<FriendItem> items = FriendItemFactory.CreateOnline(_friendNames, _onlineNames);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void CreateOffline_OneFriendOnline_ReturnsTheOtherFriends()
        {
            FriendItem[] expected =
            {
                CreateItem(AnaName, AnaInitial, Strings.MainMenu_Friends_StatusOffline, false),
                CreateItem(CesarName, CesarInitial, Strings.MainMenu_Friends_StatusOffline, false)
            };

            List<FriendItem> items = FriendItemFactory.CreateOffline(_friendNames, _onlineNames);

            Assert.Equivalent(expected, items, strict: true);
        }

        private static FriendItem CreateItem(string name, string initial, string status, bool isOnline)
        {
            var item = new FriendItem
            {
                Name = name,
                Initial = initial,
                Status = status,
                IsOnline = isOnline
            };
            return item;
        }
    }
}
