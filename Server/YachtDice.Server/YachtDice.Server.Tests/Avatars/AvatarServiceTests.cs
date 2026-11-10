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
            PlayerProfile profile = _service.GetProfile(PlayerName);

            Assert.Equal(CurrentAvatarId, profile.AvatarId);
            Assert.Equal(PlayerLevel, profile.Level);
            Assert.Equal(PlayerExperience, profile.Experience);
        }

        [Fact]
        public void GetProfile_UnknownPlayer_ReturnsTheProfileOfANewPlayer()
        {
            PlayerProfile profile = _service.GetProfile(UnknownName);

            Assert.Equal(AvatarCatalog.DefaultAvatarId, profile.AvatarId);
            Assert.Equal(NewPlayerLevel, profile.Level);
            Assert.Equal(NoExperience, profile.Experience);
        }

        [Fact]
        public void ChangeAvatar_UnlockedAvatar_SavesTheNewAvatar()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, UnlockedAvatarId);

            Assert.Equal(AvatarChangeStatus.Success, status);
            Assert.Equal(UnlockedAvatarId, _players.Players.Single().AvatarId);
        }

        [Fact]
        public void ChangeAvatar_LockedAvatar_ReturnsAvatarLockedAndKeepsTheCurrentOne()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(PlayerName, LockedAvatarId);

            Assert.Equal(AvatarChangeStatus.AvatarLocked, status);
            Assert.Equal(CurrentAvatarId, _players.Players.Single().AvatarId);
        }

        [Fact]
        public void ChangeAvatar_LevelEqualToTheRequiredOne_UnlocksTheAvatar()
        {
            AvatarCatalog.TryFind(UnlockedAvatarId, out AvatarInfo avatar);
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
            Assert.Equal(CurrentAvatarId, _players.Players.Single().AvatarId);
        }

        [Fact]
        public void ChangeAvatar_UnknownPlayer_ReturnsPlayerNotFound()
        {
            AvatarChangeStatus status = _service.ChangeAvatar(UnknownName, CurrentAvatarId);

            Assert.Equal(AvatarChangeStatus.PlayerNotFound, status);
        }

        private void AddPlayer(int level)
        {
            _players.Add(new Player
            {
                Id = PlayerId,
                Username = PlayerName,
                DisplayName = PlayerName,
                AvatarId = CurrentAvatarId,
                Level = level,
                XP = PlayerExperience
            });
        }
    }
}
