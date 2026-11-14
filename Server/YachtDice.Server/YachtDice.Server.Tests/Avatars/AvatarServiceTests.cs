using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Avatars
{
    /// <summary>
    /// Rules to choose an avatar: it must exist and the player must have reached its level.
    /// </summary>
    public class AvatarServiceTests
    {
        private const string PlayerName = "Emilio";
        private const string UnknownName = "Carlos";
        private const string BlankName = "  ";
        private const int PlayerId = 1;
        private const int PlayerLevel = 5;
        private const int PlayerExperience = 420;
        private const int CurrentAvatarId = 2;
        private const int UnlockedAvatarId = 6;
        private const int LockedAvatarId = 8;
        private const int UnknownAvatarId = 9999;
        private const int NewPlayerLevel = 1;
        private const int NoExperience = 0;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly AvatarService _service;

        /// <summary>
        /// Prepares the service with a player of level five who uses the second avatar.
        /// </summary>
        public AvatarServiceTests()
        {
            _service = new AvatarService(_players);
            AddPlayer(PlayerLevel);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AvatarService(null));
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

            Assert.Equal(AvatarCatalog.DefaultAvatarId, avatarId);
        }

        [Fact]
        public void GetAvatarId_BlankName_ReturnsTheDefaultAvatar()
        {
            int avatarId = _service.GetAvatarId(BlankName);

            Assert.Equal(AvatarCatalog.DefaultAvatarId, avatarId);
        }

        [Fact]
        public void GetProfile_RegisteredPlayer_ReturnsAvatarLevelAndExperience()
        {
            PlayerProfile expected = new PlayerProfile
            {
                AvatarId = CurrentAvatarId,
                Level = PlayerLevel,
                Experience = PlayerExperience
            };

            PlayerProfile profile = _service.GetProfile(PlayerName);

            Assert.Equivalent(expected, profile);
        }

        [Fact]
        public void GetProfile_UnknownPlayer_ReturnsTheProfileOfANewPlayer()
        {
            PlayerProfile expected = new PlayerProfile
            {
                AvatarId = AvatarCatalog.DefaultAvatarId,
                Level = NewPlayerLevel,
                Experience = NoExperience
            };

            PlayerProfile profile = _service.GetProfile(UnknownName);

            Assert.Equivalent(expected, profile);
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
            Player expected = CreatePlayer(PlayerLevel, UnlockedAvatarId);

            _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equivalent(expected, _players.Players.Single());
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
            Player expected = CreatePlayer(PlayerLevel, CurrentAvatarId);

            _service.ChangeAvatar(PlayerName, avatarId);

            Assert.Equivalent(expected, _players.Players.Single());
        }

        [Fact]
        public void ChangeAvatar_LevelEqualToTheRequiredOne_UnlocksTheAvatar()
        {
            AvatarCatalog.TryFind(UnlockedAvatarId, out UnlockableAvatar avatar);
            _players.Players.Single().Level = avatar.RequiredLevel;

            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equal(AvatarChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeAvatar_PlayerWithoutLevel_IsTreatedAsLevelOne()
        {
            _players.Players.Single().Level = null;

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

        private static Player CreatePlayer(int level, int avatarId)
        {
            var player = new Player
            {
                Id = PlayerId,
                Username = PlayerName,
                DisplayName = PlayerName,
                AvatarId = avatarId,
                Level = level,
                XP = PlayerExperience
            };
            return player;
        }

        private void AddPlayer(int level)
        {
            _players.Add(CreatePlayer(level, CurrentAvatarId));
        }
    }
}
