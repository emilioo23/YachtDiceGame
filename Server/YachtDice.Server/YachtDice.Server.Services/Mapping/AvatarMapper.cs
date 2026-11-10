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
                Experience = profile.Experience
            };

            return contract;
        }

        /// <summary>
        /// Creates the list of avatars that is sent back to the client.
        /// </summary>
        /// <param name="avatars">Avatars of the catalog.</param>
        /// <returns>Answer for the client.</returns>
        public static List<Contracts.AvatarInfo> ToContracts(List<Logic.Avatars.AvatarInfo> avatars)
        {
            List<Contracts.AvatarInfo> contracts = avatars
                .Select(avatar => new Contracts.AvatarInfo { Id = avatar.Id, RequiredLevel = avatar.RequiredLevel })
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
    }
}
