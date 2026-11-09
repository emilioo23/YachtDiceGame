using System;
using System.Collections.Generic;
using YachtDice.Server.Logic.Avatars;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Avatar lookup that keeps the avatars in memory, so the tests do not need a database.
    /// </summary>
    internal sealed class InMemoryAvatarLookup : IAvatarLookup
    {
        private readonly Dictionary<string, int> _avatars =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void SetAvatar(string displayName, int avatarId)
        {
            _avatars[displayName] = avatarId;
        }

        public int GetAvatarId(string displayName)
        {
            int avatarId;

            if (!_avatars.TryGetValue(displayName, out avatarId))
            {
                avatarId = AvatarCatalog.DefaultAvatarId;
            }

            return avatarId;
        }
    }
}
