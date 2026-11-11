using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Avatars;

namespace YachtDice.Server.Tests.Avatars
{
    /// <summary>
    /// Rules of the list of avatars: a few are free for everybody and the others need a level.
    /// </summary>
    public class AvatarCatalogTests
    {
        private const int NewPlayerLevel = 1;
        private const int MinimumFreeAvatars = 4;
        private const int UnknownAvatarId = 9999;
        private const int FirstAvatarPosition = 0;

        [Fact]
        public void TryFind_DefaultAvatar_IsFreeForANewPlayer()
        {
            var expected = new { IsFound = true, IsFree = true };

            bool found = AvatarCatalog.TryFind(AvatarCatalog.DefaultAvatarId, out UnlockableAvatar avatar);

            var actual = new { IsFound = found, IsFree = avatar.RequiredLevel <= NewPlayerLevel };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryFind_UnknownIdentifier_ReturnsFalse()
        {
            var expected = new { IsFound = false, HasAvatar = false };

            bool found = AvatarCatalog.TryFind(UnknownAvatarId, out UnlockableAvatar avatar);

            var actual = new { IsFound = found, HasAvatar = avatar != null };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetAll_Always_HasSeveralAvatarsThatAreFreeForANewPlayer()
        {
            Func<UnlockableAvatar, bool> isAvailableForNewPlayer = avatar => avatar.RequiredLevel <= NewPlayerLevel;
            int freeAvatars = AvatarCatalog.GetAll().Count(isAvailableForNewPlayer);

            Assert.True(freeAvatars >= MinimumFreeAvatars);
        }

        [Fact]
        public void GetAll_Always_HasAvatarsThatNeedALevel()
        {
            Predicate<UnlockableAvatar> isLockedForNewPlayer = avatar => avatar.RequiredLevel > NewPlayerLevel;
            Assert.Contains(AvatarCatalog.GetAll(), isLockedForNewPlayer);
        }

        [Fact]
        public void GetAll_Always_HasDifferentIdentifiers()
        {
            List<int> identifiers = AvatarCatalog.GetAll().Select(avatar => avatar.Id).ToList();

            Assert.Equal(identifiers.Count, identifiers.Distinct().Count());
        }

        [Fact]
        public void GetAll_Always_StartsAtTheDefaultAvatar()
        {
            List<int> identifiers = AvatarCatalog.GetAll().Select(avatar => avatar.Id).ToList();

            Assert.Equal(AvatarCatalog.DefaultAvatarId, identifiers[FirstAvatarPosition]);
        }

        [Fact]
        public void GetAll_ChangingTheReturnedList_DoesNotChangeTheCatalog()
        {
            List<UnlockableAvatar> firstCopy = AvatarCatalog.GetAll();
            int originalCount = firstCopy.Count;

            firstCopy.Clear();

            Assert.Equal(originalCount, AvatarCatalog.GetAll().Count);
        }
    }
}
