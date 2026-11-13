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
        private readonly FriendPresence _anyPresence = new FriendPresence(AnyStatus, true);

        private static FriendPresence OnlinePresence =>
            new FriendPresence(Strings.MainMenu_Friends_StatusOnline, true);

        private static FriendPresence OfflinePresence =>
            new FriendPresence(Strings.MainMenu_Friends_StatusOffline, false);

        [Fact]
        public void Create_UnsortedNames_ReturnsThemOrderedByName()
        {
            FriendItem[] expected =
            {
                CreateItem(AnaName, AnaInitial, _anyPresence),
                CreateItem(BetoName, BetoInitial, _anyPresence),
                CreateItem(CesarName, CesarInitial, _anyPresence)
            };

            List<FriendItem> items = FriendItemFactory.Create(_friendNames, _anyPresence);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void Create_Name_ShowsItsInitial()
        {
            string[] names = { AnaName };
            FriendItem[] expected = { CreateItem(AnaName, AnaInitial, _anyPresence) };

            List<FriendItem> items = FriendItemFactory.Create(names, _anyPresence);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void CreateOnline_OnlineNameWithOtherCase_ReturnsOnlyThatFriend()
        {
            FriendItem[] expected =
            {
                CreateItem(BetoName, BetoInitial, OnlinePresence)
            };

            List<FriendItem> items = FriendItemFactory.CreateOnline(_friendNames, _onlineNames);

            Assert.Equivalent(expected, items, strict: true);
        }

        [Fact]
        public void CreateOffline_OneFriendOnline_ReturnsTheOtherFriends()
        {
            FriendItem[] expected =
            {
                CreateItem(AnaName, AnaInitial, OfflinePresence),
                CreateItem(CesarName, CesarInitial, OfflinePresence)
            };

            List<FriendItem> items = FriendItemFactory.CreateOffline(_friendNames, _onlineNames);

            Assert.Equivalent(expected, items, strict: true);
        }

        private static FriendItem CreateItem(string name, string initial, FriendPresence presence)
        {
            var item = new FriendItem
            {
                Name = name,
                Initial = initial,
                Status = presence.Status,
                IsOnline = presence.IsOnline
            };
            return item;
        }
    }
}
