using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Rules to change the username of a player: it must be valid, polite and not used by another player.
    /// </summary>
    public class ProfileServiceTests
    {
        private const string CurrentUsername = "emilio";
        private const string OtherUsername = "gabriel";
        private const string NewUsername = "emilio_23";
        private const string SecondNewUsername = "emilio_24";
        private const int TenDays = 10;
        private const int FortyDays = 40;
        private const string UnknownName = "ramiro_lopez";
        private const string BlankName = "  ";
        private const string PaddedNewUsername = "  emilio_23  ";
        private const string InvalidUsername = "emilio 23!";
        private const string OffensiveWord = "badword";
        private const string UpperCaseCurrentUsername = "EMILIO";
        private const int CurrentPlayerId = 1;
        private const int OtherPlayerId = 2;
        private const int FirstGameAchievementId = 1;
        private const int FirstWinAchievementId = 2;
        private const int CollectorAchievementId = 22;
        private const int UnknownTitleId = 99;
        private const int WinningScore = 95;
        private const string SavedTitleText = "2";
        private const string NoTitleText = "";
        private const string MexicoFlagCode = "mx";
        private const string SpainFlagCodeInUpperCase = "ES";
        private const string SpainFlagCode = "es";
        private const string OneLetterFlagCode = "m";
        private const string ThreeLetterFlagCode = "mex";
        private const string SymbolFlagCode = "1!";
        private const string NoFlagCode = "";
        private const int TwoPlayers = 2;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameHistoryRepository _history = new InMemoryGameHistoryRepository();
        private readonly ProfileService _service;

        /// <summary>
        /// Prepares the service with two registered players.
        /// </summary>
        public ProfileServiceTests()
        {
            _service = new ProfileService(
                _players,
                new OffensiveWordFilter(new[] { OffensiveWord }),
                new PlayerStatisticsService(_players, _history));
            _players.Add(CreatePlayer(CurrentPlayerId, CurrentUsername));
            _players.Add(CreatePlayer(OtherPlayerId, OtherUsername));
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfileService(
                null,
                new OffensiveWordFilter(new string[0]),
                new PlayerStatisticsService(_players, _history)));
        }

        [Fact]
        public void Constructor_NullWordFilter_ThrowsArgumentNullException()
        {
            var statistics = new PlayerStatisticsService(_players, _history);

            Assert.Throws<ArgumentNullException>(() => new ProfileService(_players, null, statistics));
        }

        [Fact]
        public void Constructor_NullStatisticsService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ProfileService(_players, new OffensiveWordFilter(new string[0]), null));
        }

        [Fact]
        public void GetAchievements_AnyPlayer_ReturnsEveryAchievement()
        {
            List<AchievementStatus> achievements = _service.GetAchievements(CurrentUsername);

            Assert.Equal(AchievementRules.GetAll().Count, achievements.Count);
        }

        [Fact]
        public void GetAchievements_PlayerWithoutGames_HasNoUnlockedAchievements()
        {
            List<int> unlockedIds = FindUnlockedIds();

            Assert.Empty(unlockedIds);
        }

        [Fact]
        public void GetAchievements_PlayerWhoWonOneGame_UnlocksTheFirstGameAndTheFirstWin()
        {
            _history.AddPlayedGame(CurrentPlayerId, WinningScore, true);
            var expectedIds = new List<int> { FirstGameAchievementId, FirstWinAchievementId };

            List<int> unlockedIds = FindUnlockedIds();

            Assert.Equal(expectedIds, unlockedIds);
        }

        [Fact]
        public void ChangeTitle_DefaultTitle_ReturnsSuccess()
        {
            TitleChangeStatus status = _service.ChangeTitle(CurrentUsername, AchievementRules.DefaultTitleId);

            Assert.Equal(TitleChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeTitle_UnlockedTitle_ReturnsSuccess()
        {
            _history.AddPlayedGame(CurrentPlayerId, WinningScore, true);

            TitleChangeStatus status = _service.ChangeTitle(CurrentUsername, FirstWinAchievementId);

            Assert.Equal(TitleChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeTitle_UnlockedTitle_SavesTheTitle()
        {
            _history.AddPlayedGame(CurrentPlayerId, WinningScore, true);

            _service.ChangeTitle(CurrentUsername, FirstWinAchievementId);

            Assert.Equal(SavedTitleText, _players.FindTitle(CurrentPlayerId));
        }

        [Fact]
        public void ChangeTitle_LockedTitle_ReturnsTitleLocked()
        {
            TitleChangeStatus status = _service.ChangeTitle(CurrentUsername, FirstWinAchievementId);

            Assert.Equal(TitleChangeStatus.TitleLocked, status);
        }

        [Fact]
        public void ChangeTitle_LockedTitle_DoesNotSaveAnything()
        {
            _service.ChangeTitle(CurrentUsername, FirstWinAchievementId);

            Assert.Equal(NoTitleText, _players.FindTitle(CurrentPlayerId));
        }

        [Fact]
        public void ChangeTitle_UnknownTitle_ReturnsTitleNotFound()
        {
            TitleChangeStatus status = _service.ChangeTitle(CurrentUsername, UnknownTitleId);

            Assert.Equal(TitleChangeStatus.TitleNotFound, status);
        }

        [Fact]
        public void ChangeTitle_AchievementWithoutTitle_ReturnsTitleNotFound()
        {
            TitleChangeStatus status = _service.ChangeTitle(CurrentUsername, CollectorAchievementId);

            Assert.Equal(TitleChangeStatus.TitleNotFound, status);
        }

        [Fact]
        public void ChangeTitle_UnknownPlayer_ReturnsPlayerNotFound()
        {
            TitleChangeStatus status = _service.ChangeTitle(UnknownName, AchievementRules.DefaultTitleId);

            Assert.Equal(TitleChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeFlag_ValidCode_ReturnsSuccess()
        {
            FlagChangeStatus status = _service.ChangeFlag(CurrentUsername, MexicoFlagCode);

            Assert.Equal(FlagChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeFlag_ValidCode_SavesTheCode()
        {
            _service.ChangeFlag(CurrentUsername, MexicoFlagCode);

            Assert.Equal(MexicoFlagCode, _players.FindFlagCode(CurrentPlayerId));
        }

        [Fact]
        public void ChangeFlag_CodeInUpperCase_SavesItInLowerCase()
        {
            _service.ChangeFlag(CurrentUsername, SpainFlagCodeInUpperCase);

            Assert.Equal(SpainFlagCode, _players.FindFlagCode(CurrentPlayerId));
        }

        [Fact]
        public void ChangeFlag_EmptyCode_RemovesTheFlag()
        {
            _service.ChangeFlag(CurrentUsername, MexicoFlagCode);

            _service.ChangeFlag(CurrentUsername, NoFlagCode);

            Assert.Equal(NoFlagCode, _players.FindFlagCode(CurrentPlayerId));
        }

        [Fact]
        public void ChangeFlag_NullCode_ReturnsSuccess()
        {
            FlagChangeStatus status = _service.ChangeFlag(CurrentUsername, null);

            Assert.Equal(FlagChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeFlag_OneLetter_ReturnsInvalidFlag()
        {
            FlagChangeStatus status = _service.ChangeFlag(CurrentUsername, OneLetterFlagCode);

            Assert.Equal(FlagChangeStatus.InvalidFlag, status);
        }

        [Fact]
        public void ChangeFlag_ThreeLetters_ReturnsInvalidFlag()
        {
            FlagChangeStatus status = _service.ChangeFlag(CurrentUsername, ThreeLetterFlagCode);

            Assert.Equal(FlagChangeStatus.InvalidFlag, status);
        }

        [Fact]
        public void ChangeFlag_DigitAndSymbol_ReturnsInvalidFlag()
        {
            FlagChangeStatus status = _service.ChangeFlag(CurrentUsername, SymbolFlagCode);

            Assert.Equal(FlagChangeStatus.InvalidFlag, status);
        }

        [Fact]
        public void ChangeFlag_InvalidCode_DoesNotSaveAnything()
        {
            _service.ChangeFlag(CurrentUsername, ThreeLetterFlagCode);

            Assert.Equal(NoFlagCode, _players.FindFlagCode(CurrentPlayerId));
        }

        [Fact]
        public void ChangeFlag_UnknownPlayer_ReturnsPlayerNotFound()
        {
            FlagChangeStatus status = _service.ChangeFlag(UnknownName, MexicoFlagCode);

            Assert.Equal(FlagChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeUsername_ChangedTenDaysAgo_ReturnsChangedRecently()
        {
            _players.FindByName(CurrentUsername).Single().LastUserChange = DateTime.UtcNow.AddDays(-TenDays);

            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(UsernameChangeStatus.ChangedRecently, status);
        }

        [Fact]
        public void ChangeUsername_ChangedFortyDaysAgo_ReturnsSuccess()
        {
            _players.FindByName(CurrentUsername).Single().LastUserChange = DateTime.UtcNow.AddDays(-FortyDays);

            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_SameUsernameAfterARecentChange_ReturnsSuccess()
        {
            _players.FindByName(CurrentUsername).Single().LastUserChange = DateTime.UtcNow.AddDays(-TenDays);

            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, CurrentUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_SecondChangeRightAway_ReturnsChangedRecently()
        {
            _service.ChangeUsername(CurrentUsername, NewUsername);

            UsernameChangeStatus status = _service.ChangeUsername(NewUsername, SecondNewUsername);

            Assert.Equal(UsernameChangeStatus.ChangedRecently, status);
        }

        [Fact]
        public void ChangeUsername_FreeValidName_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_FreeValidName_SavesTheNewUsername()
        {
            _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(NewUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_FreeValidName_UpdatesTheDisplayName()
        {
            _service.ChangeUsername(CurrentUsername, NewUsername);

            Assert.Equal(NewUsername, _players.Players[0].DisplayName);
        }

        [Fact]
        public void ChangeUsername_NameWithSpaces_SavesItWithoutThem()
        {
            _service.ChangeUsername(CurrentUsername, PaddedNewUsername);

            Assert.Equal(NewUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_UnknownPlayer_ReturnsPlayerNotFound()
        {
            UsernameChangeStatus status = _service.ChangeUsername(UnknownName, NewUsername);

            Assert.Equal(UsernameChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeUsername_BlankCurrentName_ReturnsPlayerNotFound()
        {
            UsernameChangeStatus status = _service.ChangeUsername(BlankName, NewUsername);

            Assert.Equal(UsernameChangeStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ChangeUsername_InvalidCharacters_ReturnsInvalidUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, InvalidUsername);

            Assert.Equal(UsernameChangeStatus.InvalidUsername, status);
        }

        [Fact]
        public void ChangeUsername_NullNewUsername_ReturnsInvalidUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, null);

            Assert.Equal(UsernameChangeStatus.InvalidUsername, status);
        }

        [Fact]
        public void ChangeUsername_OffensiveWord_ReturnsOffensiveUsername()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, OffensiveWord);

            Assert.Equal(UsernameChangeStatus.OffensiveUsername, status);
        }

        [Fact]
        public void ChangeUsername_UsedByAnotherPlayer_ReturnsUsernameAlreadyTaken()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, OtherUsername);

            Assert.Equal(UsernameChangeStatus.UsernameAlreadyTaken, status);
        }

        [Fact]
        public void ChangeUsername_UsedByAnotherPlayer_KeepsTheCurrentUsername()
        {
            _service.ChangeUsername(CurrentUsername, OtherUsername);

            Assert.Equal(CurrentUsername, _players.Players[0].Username);
        }

        [Fact]
        public void ChangeUsername_SameNameWithOtherCase_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, UpperCaseCurrentUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void ChangeUsername_SameName_ReturnsSuccess()
        {
            UsernameChangeStatus status = _service.ChangeUsername(CurrentUsername, CurrentUsername);

            Assert.Equal(UsernameChangeStatus.Success, status);
        }

        [Fact]
        public void DeleteAccount_ExistingPlayer_ReturnsSuccess()
        {
            AccountDeletionStatus status = _service.DeleteAccount(CurrentUsername);

            Assert.Equal(AccountDeletionStatus.Success, status);
        }

        [Fact]
        public void DeleteAccount_ExistingPlayer_RemovesThePlayer()
        {
            _service.DeleteAccount(CurrentUsername);

            Assert.Empty(_players.FindByUsername(CurrentUsername));
        }

        [Fact]
        public void DeleteAccount_ExistingPlayer_KeepsTheOtherPlayers()
        {
            _service.DeleteAccount(CurrentUsername);

            Assert.Single(_players.FindByUsername(OtherUsername));
        }

        [Fact]
        public void DeleteAccount_UsernameInUpperCase_RemovesThePlayer()
        {
            _service.DeleteAccount(UpperCaseCurrentUsername);

            Assert.Empty(_players.FindByUsername(CurrentUsername));
        }

        [Fact]
        public void DeleteAccount_UnknownPlayer_ReturnsPlayerNotFound()
        {
            AccountDeletionStatus status = _service.DeleteAccount(UnknownName);

            Assert.Equal(AccountDeletionStatus.PlayerNotFound, status);
        }

        [Fact]
        public void DeleteAccount_UnknownPlayer_KeepsEveryPlayer()
        {
            _service.DeleteAccount(UnknownName);

            Assert.Equal(TwoPlayers, _players.Players.Count);
        }

        [Fact]
        public void DeleteAccount_BlankName_ReturnsPlayerNotFound()
        {
            AccountDeletionStatus status = _service.DeleteAccount(BlankName);

            Assert.Equal(AccountDeletionStatus.PlayerNotFound, status);
        }

        [Fact]
        public void DeleteAccount_CalledTwice_ReturnsPlayerNotFoundTheSecondTime()
        {
            _service.DeleteAccount(CurrentUsername);

            AccountDeletionStatus status = _service.DeleteAccount(CurrentUsername);

            Assert.Equal(AccountDeletionStatus.PlayerNotFound, status);
        }

        private List<int> FindUnlockedIds()
        {
            Func<AchievementStatus, bool> isUnlocked = achievement => achievement.IsUnlocked;
            Func<AchievementStatus, int> readId = achievement => achievement.Id;

            return _service.GetAchievements(CurrentUsername).Where(isUnlocked).Select(readId).ToList();
        }

        private static Player CreatePlayer(int id, string username)
        {
            var player = new Player
            {
                Id = id,
                Username = username,
                DisplayName = username
            };

            return player;
        }
    }
}
