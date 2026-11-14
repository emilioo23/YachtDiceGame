using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using Logic = YachtDice.Server.Logic;

    /// <summary>
    /// Converts the avatar data of the logic layer into the data contracts.
    /// </summary>
    internal static class AvatarMapper
    {
        /// <summary>
        /// Creates the profile that is sent back to the client.
        /// </summary>
        /// <param name="profile">Profile produced by the logic layer.</param>
        /// <returns>Answer for the client.</returns>
        public static Contracts.PlayerProfile ToContract(Logic.Avatars.PlayerProfile profile)
        {
            var contract = new Contracts.PlayerProfile
            {
                AvatarId = profile.AvatarId,
                Level = profile.Level,
                Experience = profile.Experience,
                LevelStartExperience = profile.LevelStartExperience,
                NextLevelExperience = profile.NextLevelExperience
            };

            return contract;
        }

        /// <summary>
        /// Creates the list of avatars that is sent back to the client.
        /// </summary>
        /// <param name="avatars">Avatars of the catalog.</param>
        /// <returns>Answer for the client.</returns>
        public static List<Contracts.UnlockableAvatar> ToContracts(List<Logic.Avatars.UnlockableAvatar> avatars)
        {
            Func<Logic.Avatars.UnlockableAvatar, Contracts.UnlockableAvatar> createUnlockableAvatar = ToContract;
            List<Contracts.UnlockableAvatar> contracts = avatars
                .Select(createUnlockableAvatar)
                .ToList();

            return contracts;
        }

        /// <summary>
        /// Converts the result of changing an avatar into the code that the client receives.
        /// </summary>
        /// <param name="status">Result produced by the logic layer.</param>
        /// <returns>The code for the client.</returns>
        public static Contracts.AvatarChangeResultCode ToResultCode(Logic.Avatars.AvatarChangeStatus status)
        {
            return EnumMapper.Map<Logic.Avatars.AvatarChangeStatus, Contracts.AvatarChangeResultCode>(status);
        }

        /// <summary>
        /// Converts an avatar of the catalog into the version that travels to the clients.
        /// </summary>
        /// <param name="avatar">Avatar of the catalog in the logic layer.</param>
        /// <returns>The avatar for the clients.</returns>
        private static Contracts.UnlockableAvatar ToContract(Logic.Avatars.UnlockableAvatar avatar)
        {
            var contract = new Contracts.UnlockableAvatar
            {
                Id = avatar.Id,
                RequiredLevel = avatar.RequiredLevel
            };

            return contract;
        }
    }
}
