using System.Collections.Generic;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Keeps the games that are being played, one for each room, and applies the actions of the players.
    /// Every method is thread-safe.
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// Starts the game of a room. If the room already had a game, the new one replaces it.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="participants">Players of the game, in the order of the turns.</param>
        /// <param name="speed">Speed chosen for the game, which sets the time of each turn.</param>
        /// <returns>The state of the new game.</returns>
        GameSnapshot Start(string roomCode, IEnumerable<GameParticipant> participants, GameSpeed speed);

        /// <summary>
        /// Gets the state of the game of a room for a player who takes part in it.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        /// <returns>The state of the game, or the reason why it could not be read.</returns>
        GameActionResult GetState(RoomMember member);

        /// <summary>
        /// Rolls the dice for the player whose turn it is.
        /// </summary>
        /// <param name="member">Room and identifier of the player who rolls.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>The state of the game after the roll, or the reason why it was rejected.</returns>
        GameActionResult Roll(RoomMember member, IReadOnlyList<bool> heldDice);

        /// <summary>
        /// Writes the points of the current dice in a box and passes the turn.
        /// </summary>
        /// <param name="member">Room and identifier of the player who chooses the box.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>The state of the game after the move, or the reason why it was rejected.</returns>
        GameActionResult Score(RoomMember member, ScoreCategory category);

        /// <summary>
        /// Plays the turn of the current player because the time is over. Nothing happens if the turn
        /// already changed, which means that the player played in time.
        /// </summary>
        /// <param name="turn">Turn that ran out of time, with the code of its room and its number.</param>
        /// <returns>The state of the game after the move, or the reason why nothing was done.</returns>
        /// <exception cref="System.ArgumentNullException">The turn is null.</exception>
        GameActionResult AutoPlay(GameTurn turn);

        /// <summary>
        /// Marks that a player is not connected to the game.
        /// </summary>
        /// <param name="member">Room and identifier of the player who left.</param>
        /// <returns>The state of the game, or the reason why it could not be read.</returns>
        GameActionResult MarkAbsent(RoomMember member);

        /// <summary>
        /// Marks that a player came back to the game.
        /// </summary>
        /// <param name="member">Room and identifier of the player who returned.</param>
        /// <returns>The state of the game, or the reason why it could not be read.</returns>
        GameActionResult MarkPresent(RoomMember member);

        /// <summary>
        /// Removes a player from the game for good, for example because the host kicked the player.
        /// </summary>
        /// <param name="member">Room and identifier of the player to remove.</param>
        /// <returns>The state of the game, or the reason why it could not be read.</returns>
        GameActionResult Remove(RoomMember member);

        /// <summary>
        /// Deletes the game of a room, for example because the room was closed.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        void End(string roomCode);
    }
}
