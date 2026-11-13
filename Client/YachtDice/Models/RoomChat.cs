using YachtDice.Gateway;

namespace YachtDice.Models
{
    /// <summary>
    /// The connection of the player to the chat of a room and whether the player could join that chat.
    /// </summary>
    public sealed class RoomChat
    {
        /// <summary>
        /// Initializes the chat of the room.
        /// </summary>
        /// <param name="gateway">Connection of the player to the chat of the room.</param>
        /// <param name="isAvailable">Whether the player could join the chat of the room.</param>
        public RoomChat(ChatGateway gateway, bool isAvailable)
        {
            Gateway = gateway;
            IsAvailable = isAvailable;
        }

        /// <summary>
        /// Gets the connection of the player to the chat of the room.
        /// </summary>
        public ChatGateway Gateway { get; }

        /// <summary>
        /// Gets a value indicating whether the player could join the chat of the room.
        /// </summary>
        public bool IsAvailable { get; }
    }
}
