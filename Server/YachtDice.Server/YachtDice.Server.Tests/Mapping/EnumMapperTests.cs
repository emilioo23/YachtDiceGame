using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Services.Mapping;
using Contracts = YachtDice.Server.Contracts;
using Logic = YachtDice.Server.Logic;

namespace YachtDice.Server.Tests.Mapping
{
    /// <summary>
    /// The results of the logic travel to the client as the enum of the contract with the same name. These
    /// tests check that every value of the logic has its value in the contract, so a value added on one side
    /// only is found here and not when a player gets an error.
    /// </summary>
    public class EnumMapperTests
    {
        [Fact]
        public void Map_LockedForTheDay_ReturnsTheSameValueOfTheContract()
        {
            Contracts.Authentication.LoginResultCode result = EnumMapper
                .Map<Logic.Authentication.LoginStatus, Contracts.Authentication.LoginResultCode>(
                    Logic.Authentication.LoginStatus.LockedForTheDay);

            Assert.Equal(Contracts.Authentication.LoginResultCode.LockedForTheDay, result);
        }

        [Fact]
        public void Map_EveryGameSpeed_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<Logic.Rooms.GameSpeed, Contracts.Rooms.GameSpeed>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryComputerDifficulty_HasAValueInTheContract()
        {
            List<string> missingNames =
                FindMissingNames<Logic.Rooms.ComputerDifficulty, Contracts.Rooms.ComputerDifficulty>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryScoreCategory_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<Logic.Game.ScoreCategory, Contracts.Game.ScoreCategory>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryAvatarChangeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Avatars.AvatarChangeStatus,
                Contracts.Players.AvatarChangeResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryRoomStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<Logic.Rooms.RoomStatus, Contracts.Rooms.RoomResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryLoginStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Authentication.LoginStatus,
                Contracts.Authentication.LoginResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryPasswordResetStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Authentication.PasswordResetStatus,
                Contracts.Authentication.PasswordResetResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryRegistrationResult_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Authentication.RegistrationResult,
                Contracts.Players.RegisterResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryFriendActionStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Friends.FriendActionStatus,
                Contracts.Friends.FriendActionResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryFriendRequestStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Friends.FriendRequestStatus,
                Contracts.Friends.FriendRequestResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryInvitationStatus_OnlyMissesAllowedBecauseTheServiceDecidesIt()
        {
            var namesExpected = new List<string> { nameof(Logic.Friends.InvitationStatus.Allowed) };

            List<string> missingNames = FindMissingNames<
                Logic.Friends.InvitationStatus,
                Contracts.Friends.InvitationResultCode>();

            Assert.Equal(namesExpected, missingNames);
        }

        [Fact]
        public void Map_EveryGameActionStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<Logic.Game.GameActionStatus, Contracts.Game.GameResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryGameStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<Logic.Game.GameStatus, Contracts.Game.GameStatus>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryAccountDeletionStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Players.AccountDeletionStatus,
                Contracts.Players.AccountDeletionResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryFlagChangeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Players.FlagChangeStatus,
                Contracts.Players.FlagChangeResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryTitleChangeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Players.TitleChangeStatus,
                Contracts.Players.TitleChangeResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryUsernameChangeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.Players.UsernameChangeStatus,
                Contracts.Players.UsernameChangeResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryResendCodeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.TwoFactor.ResendCodeStatus,
                Contracts.TwoFactor.ResendResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryTwoFactorChangeStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.TwoFactor.TwoFactorChangeStatus,
                Contracts.TwoFactor.TwoFactorChangeResultCode>();

            Assert.Empty(missingNames);
        }

        [Fact]
        public void Map_EveryTwoFactorValidationStatus_HasAValueInTheContract()
        {
            List<string> missingNames = FindMissingNames<
                Logic.TwoFactor.TwoFactorValidationStatus,
                Contracts.TwoFactor.VerificationResultCode>();

            Assert.Empty(missingNames);
        }

        private static List<string> FindMissingNames<TSource, TTarget>()
            where TSource : struct, Enum
            where TTarget : struct, Enum
        {
            var targetNames = new HashSet<string>(Enum.GetNames(typeof(TTarget)));
            Func<string, bool> isMissing = name => !targetNames.Contains(name);

            return Enum.GetNames(typeof(TSource)).Where(isMissing).ToList();
        }
    }
}
