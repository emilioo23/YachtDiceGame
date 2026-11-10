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
            bool found = AvatarCatalog.TryFind(AvatarCatalog.DefaultAvatarId, out AvatarInfo avatar);

            Assert.True(found);
            Assert.True(avatar.RequiredLevel <= NewPlayerLevel);
        }

        [Fact]
        public void TryFind_UnknownIdentifier_ReturnsFalse()
        {
            bool found = AvatarCatalog.TryFind(UnknownAvatarId, out AvatarInfo avatar);

            Assert.False(found);
            Assert.Null(avatar);
        }

        [Fact]
        public void GetAll_Always_HasSeveralAvatarsThatAreFreeForANewPlayer()
        {
            int freeAvatars = AvatarCatalog.GetAll().Count(avatar => avatar.RequiredLevel <= NewPlayerLevel);

            Assert.True(freeAvatars >= MinimumFreeAvatars);
        }

        [Fact]
        public void GetAll_Always_HasAvatarsThatNeedALevel()
        {
            Assert.Contains(AvatarCatalog.GetAll(), avatar => avatar.RequiredLevel > NewPlayerLevel);
        }

        [Fact]
        public void GetAll_Always_HasDifferentIdentifiersStartingAtTheDefaultAvatar()
        {
            var identifiers = AvatarCatalog.GetAll().Select(avatar => avatar.Id).ToList();

            Assert.Equal(identifiers.Count, identifiers.Distinct().Count());
            Assert.Equal(AvatarCatalog.DefaultAvatarId, identifiers[FirstAvatarPosition]);
        }

        [Fact]
        public void GetAll_ChangingTheReturnedList_DoesNotChangeTheCatalog()
        {
            var firstCopy = AvatarCatalog.GetAll();
            int originalCount = firstCopy.Count;

            firstCopy.Clear();

            Assert.Equal(originalCount, AvatarCatalog.GetAll().Count);
        }
    }
}
