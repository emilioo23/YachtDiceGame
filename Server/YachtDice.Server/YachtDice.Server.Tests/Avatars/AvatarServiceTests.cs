using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Avatars
{
    /// <summary>
    /// Tests of the avatars and the catalog of unlockable avatars.
    /// </summary>
    public class AvatarServiceTests
    {
        private const string PlayerName = "Emilio";
        private const string UnknownName = "Nobody";
        private const string BlankName = "  ";
        private const int PlayerId = 1;
        private const int PlayerLevel = 5;
        private const int PlayerExperience = 420;
        private const int CurrentAvatarId = 2;
        private const int UnlockedAvatarId = 6;
        private const int LockedAvatarId = 8;
        private const int UnknownAvatarId = 9999;
        private const int NewPlayerLevel = 1;
        private const int GuestLevel = 0;
        private const string NoFlagCode = "";
        private const int NoExperience = 0;
        private const int SecondLevelExperience = 100;
        private const int ThirdLevelExperience = 200;
        private const int FourthLevelExperience = 300;
        private const int FifthLevelExperience = 400;
        private const int SixthLevelExperience = 500;
        private const int MinimumFreeAvatars = 4;
        private const int FirstAvatarPosition = 0;
        private const int LastLevelOfTheGame = 20;
        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly AvatarService _service;

        /// <summary>
        /// Prepares the service with a player of level five who uses the second avatar.
        /// </summary>
        public AvatarServiceTests()
        {
            _service = new AvatarService(_players, CreateLevelTable());
            AddPlayer();
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AvatarService(null, CreateLevelTable()));
        }

        [Fact]
        public void Constructor_NullLevelTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AvatarService(_players, null));
        }

        [Fact]
        public void GetLevel_RegisteredPlayer_ReturnsTheLevelOfThePlayer()
        {
            int level = _service.GetLevel(PlayerName);

            Assert.Equal(PlayerLevel, level);
        }

        [Fact]
        public void GetLevel_Guest_ReturnsZero()
        {
            int level = _service.GetLevel(UnknownName);

            Assert.Equal(GuestLevel, level);
        }

        [Fact]
        public void GetAvatarId_RegisteredPlayer_ReturnsTheAvatarOfThePlayer()
        {
            int avatarId = _service.GetAvatarId(PlayerName);

            Assert.Equal(CurrentAvatarId, avatarId);
        }

        [Fact]
        public void GetAvatarId_UnknownPlayer_ReturnsTheDefaultAvatar()
        {
            int avatarId = _service.GetAvatarId(UnknownName);

            Assert.Equal(AvatarService.DefaultAvatarId, avatarId);
        }

        [Fact]
        public void GetAvatarId_BlankName_ReturnsTheDefaultAvatar()
        {
            int avatarId = _service.GetAvatarId(BlankName);

            Assert.Equal(AvatarService.DefaultAvatarId, avatarId);
        }

        [Fact]
        public void GetProfile_RegisteredPlayer_ReturnsAvatarLevelAndExperience()
        {
            PlayerProfile profileExpected = new PlayerProfile
            {
                AvatarId = CurrentAvatarId,
                Level = PlayerLevel,
                Experience = PlayerExperience,
                LevelStartExperience = FifthLevelExperience,
                NextLevelExperience = SixthLevelExperience,
                FlagCode = NoFlagCode
            };

            PlayerProfile profile = _service.GetProfile(PlayerName);

            Assert.Equivalent(profileExpected, profile);
        }

        [Fact]
        public void GetProfile_UnknownPlayer_ReturnsTheProfileOfANewPlayer()
        {
            PlayerProfile profileExpected = new PlayerProfile
            {
                AvatarId = AvatarService.DefaultAvatarId,
                Level = NewPlayerLevel,
                Experience = NoExperience,
                LevelStartExperience = NoExperience,
                NextLevelExperience = SecondLevelExperience,
                FlagCode = NoFlagCode
            };

            PlayerProfile profile = _service.GetProfile(UnknownName);

            Assert.Equivalent(profileExpected, profile);
        }

        [Fact]
        public void ChangeAvatar_UnlockedAvatar_ReturnsSuccess()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equal(AvatarChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeAvatar_UnlockedAvatar_SavesTheNewAvatar()
        {
            Player savedPlayerExpected = CreatePlayer(UnlockedAvatarId);

            _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equivalent(savedPlayerExpected, _players.Players.Single());
        }

        [Fact]
        public void ChangeAvatar_LockedAvatar_ReturnsAvatarLocked()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, LockedAvatarId);

            Assert.Equal(AvatarChangeStatus.AvatarLocked, status);
        }

        [Theory]
        [InlineData(LockedAvatarId)]
        [InlineData(UnknownAvatarId)]
        public void ChangeAvatar_AvatarThatCannotBeChosen_KeepsTheCurrentOne(int avatarId)
        {
            Player savedPlayerExpected = CreatePlayer(CurrentAvatarId);

            _service.ChangeAvatar(PlayerName, avatarId);

            Assert.Equivalent(savedPlayerExpected, _players.Players.Single());
        }

        [Fact]
        public void ChangeAvatar_ExperienceThatReachesTheRequiredLevel_UnlocksTheAvatar()
        {
            AvatarService.TryFind(UnlockedAvatarId, out UnlockableAvatar avatar);
            _players.Players.Single().XP = CreateLevelTable().GetLevelStart(avatar.RequiredLevel);

            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equal(AvatarChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeAvatar_PlayerWithoutExperience_IsTreatedAsLevelOne()
        {
            _players.Players.Single().XP = null;

            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equal(AvatarChangeStatus.AvatarLocked, status);
        }

        [Fact]
        public void ChangeAvatar_UnknownAvatar_ReturnsAvatarNotFound()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnknownAvatarId);

            Assert.Equal(AvatarChangeStatus.AvatarNotFound, status);
        }

        [Fact]
        public void ChangeAvatar_UnknownPlayer_ReturnsPlayerNotFound()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(UnknownName, CurrentAvatarId);

            Assert.Equal(AvatarChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void TryFind_DefaultAvatar_IsFreeForANewPlayer()
        {
            var searchExpected = new { IsFound = true, IsFree = true };

            bool isFound = AvatarService.TryFind(AvatarService.DefaultAvatarId, out UnlockableAvatar avatar);

            var searchResult = new { IsFound = isFound, IsFree = avatar.RequiredLevel <= NewPlayerLevel };
            Assert.Equal(searchExpected, searchResult);
        }

        [Fact]
        public void TryFind_UnknownIdentifier_ReturnsFalse()
        {
            var searchExpected = new { IsFound = false, HasAvatar = false };

            bool isFound = AvatarService.TryFind(UnknownAvatarId, out UnlockableAvatar avatar);

            var searchResult = new { IsFound = isFound, HasAvatar = avatar != null };
            Assert.Equal(searchExpected, searchResult);
        }

        [Fact]
        public void GetAll_Always_HasSeveralAvatarsThatAreFreeForANewPlayer()
        {
            Func<UnlockableAvatar, bool> isAvailableForNewPlayer = avatar => avatar.RequiredLevel <= NewPlayerLevel;
            int freeAvatars = AvatarService.GetAll().Count(isAvailableForNewPlayer);

            Assert.True(freeAvatars >= MinimumFreeAvatars);
        }

        [Fact]
        public void GetAll_Always_HasAvatarsThatNeedALevel()
        {
            Predicate<UnlockableAvatar> isLockedForNewPlayer = avatar => avatar.RequiredLevel > NewPlayerLevel;
            Assert.Contains(AvatarService.GetAll(), isLockedForNewPlayer);
        }

        [Fact]
        public void GetAll_Always_HasDifferentIdentifiers()
        {
            List<int> identifiers = AvatarService.GetAll().Select(avatar => avatar.Id).ToList();

            Assert.Equal(identifiers.Count, identifiers.Distinct().Count());
        }

        [Fact]
        public void GetAll_Always_StartsAtTheDefaultAvatar()
        {
            List<int> identifiers = AvatarService.GetAll().Select(avatar => avatar.Id).ToList();

            Assert.Equal(AvatarService.DefaultAvatarId, identifiers[FirstAvatarPosition]);
        }

        [Fact]
        public void GetAll_Always_HasConsecutiveIdentifiersFromTheDefaultAvatar()
        {
            Func<UnlockableAvatar, int> readId = avatar => avatar.Id;
            List<int> identifiersExpected = Enumerable
                .Range(AvatarService.DefaultAvatarId, AvatarService.GetAll().Count)
                .ToList();

            List<int> identifiers = AvatarService.GetAll().Select(readId).ToList();

            Assert.Equal(identifiersExpected, identifiers);
        }

        [Fact]
        public void GetAll_Always_NeverNeedsMoreThanTheLastLevelOfTheGame()
        {
            Func<UnlockableAvatar, int> readLevel = avatar => avatar.RequiredLevel;

            int highestRequiredLevel = AvatarService.GetAll().Max(readLevel);

            Assert.True(highestRequiredLevel <= LastLevelOfTheGame);
        }

        [Fact]
        public void GetAll_ChangingTheReturnedList_DoesNotChangeTheCatalog()
        {
            List<UnlockableAvatar> firstCopy = AvatarService.GetAll();
            int originalCount = firstCopy.Count;

            firstCopy.Clear();

            Assert.Equal(originalCount, AvatarService.GetAll().Count);
        }

        private static Player CreatePlayer(int avatarId)
        {
            var player = new Player
            {
                Id = PlayerId,
                Username = PlayerName,
                DisplayName = PlayerName,
                AvatarId = avatarId,
                XP = PlayerExperience
            };
            return player;
        }

        private static LevelTable CreateLevelTable()
        {
            int[] thresholds =
            {
                NoExperience,
                SecondLevelExperience,
                ThirdLevelExperience,
                FourthLevelExperience,
                FifthLevelExperience,
                SixthLevelExperience
            };

            return new LevelTable(thresholds);
        }

        private void AddPlayer()
        {
            _players.Add(CreatePlayer(CurrentAvatarId));
        }
    }
}
