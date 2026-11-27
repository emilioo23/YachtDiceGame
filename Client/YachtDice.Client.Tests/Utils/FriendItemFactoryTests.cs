using System;
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
        private const int AnaAvatarId = 3;
        private const int BetoAvatarId = 5;
        private const int CesarAvatarId = 7;

        private readonly string[] _friendNames = { CesarName, AnaName, BetoName };
        private readonly string[] _onlineNames = { LowerCaseBetoName };
        private readonly FriendPresence _anyPresence = new FriendPresence(AnyStatus, true);
        private readonly Dictionary<string, int> _friends = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { CesarName, CesarAvatarId },
            { AnaName, AnaAvatarId },
            { BetoName, BetoAvatarId }
        };

        private static FriendPresence OnlinePresence =>
            new FriendPresence(Strings.MainMenu_Friends_StatusOnline, true);

        private static FriendPresence OfflinePresence =>
            new FriendPresence(Strings.MainMenu_Friends_StatusOffline, false);

        [Fact]
        public void Create_UnsortedNames_ReturnsThemOrderedByName()
        {
            FriendItem[] itemsExpected =
            {
                CreateItem(AnaName, AnaInitial, _anyPresence),
                CreateItem(BetoName, BetoInitial, _anyPresence),
                CreateItem(CesarName, CesarInitial, _anyPresence)
            };

            List<FriendItem> items = FriendItemFactory.Create(_friendNames, _anyPresence);

            Assert.Equivalent(itemsExpected, items, strict: true);
        }

        [Fact]
        public void Create_Name_ShowsItsInitial()
        {
            string[] names = { AnaName };
            FriendItem[] itemsExpected = { CreateItem(AnaName, AnaInitial, _anyPresence) };

            List<FriendItem> items = FriendItemFactory.Create(names, _anyPresence);

            Assert.Equivalent(itemsExpected, items, strict: true);
        }

        [Fact]
        public void CreateOnline_OnlineNameWithOtherCase_ReturnsOnlyThatFriend()
        {
            FriendItem beto = CreateItem(BetoName, BetoInitial, OnlinePresence);
            beto.AvatarId = BetoAvatarId;
            FriendItem[] itemsExpected = { beto };

            List<FriendItem> items = FriendItemFactory.CreateOnline(_friends, _onlineNames);

            Assert.Equivalent(itemsExpected, items, strict: true);
        }

        [Fact]
        public void CreateOffline_OneFriendOnline_ReturnsTheOtherFriends()
        {
            FriendItem ana = CreateItem(AnaName, AnaInitial, OfflinePresence);
            FriendItem cesar = CreateItem(CesarName, CesarInitial, OfflinePresence);
            ana.AvatarId = AnaAvatarId;
            cesar.AvatarId = CesarAvatarId;
            FriendItem[] itemsExpected = { ana, cesar };

            List<FriendItem> items = FriendItemFactory.CreateOffline(_friends, _onlineNames);

            Assert.Equivalent(itemsExpected, items, strict: true);
        }

        [Fact]
        public void CreateOnline_FriendWithAvatar_ShowsThatAvatar()
        {
            List<FriendItem> items = FriendItemFactory.CreateOnline(_friends, _onlineNames);

            Assert.Equal(BetoAvatarId, items[0].AvatarId);
        }

        [Fact]
        public void CreateOffline_EveryoneOnline_ReturnsNoFriends()
        {
            string[] everyoneOnline = { AnaName, BetoName, CesarName };

            List<FriendItem> items = FriendItemFactory.CreateOffline(_friends, everyoneOnline);

            Assert.Empty(items);
        }

        [Fact]
        public void Create_RequestWithoutAvatar_LeavesTheAvatarUnknown()
        {
            const int UnknownAvatarId = 0;
            string[] names = { AnaName };

            List<FriendItem> items = FriendItemFactory.Create(names, _anyPresence);

            Assert.Equal(UnknownAvatarId, items[0].AvatarId);
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
