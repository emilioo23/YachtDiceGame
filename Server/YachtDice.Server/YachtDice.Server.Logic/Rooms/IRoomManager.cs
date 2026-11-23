using System;
using System.Collections.Generic;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Rules of the rooms: who is inside each one, who controls it and when its game can start. The players
    /// are recognized by the identifier that the manager gives them when they enter, never by their name.
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="hostName">Name that the player who creates the room wants to show.</param>
        /// <returns>The state of the new room and the identifier of the host.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult CreateRoom(string hostName);

        /// <summary>
        /// Puts the player inside the room that has the code. If another player of the room already uses the
        /// name, a number is added to it. If the game already started, only a player who left it can enter,
        /// and gets the identifier and the avatar from before.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name that the player who enters wants to show.</param>
        /// <returns>Whether the player entered, the state of the room and the identifier of the player.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult Join(string roomCode, string playerName);

        /// <summary>
        /// Puts the player inside a room of the list of rooms. A private room also needs its code.
        /// </summary>
        /// <param name="playerName">Name that the player who enters wants to show.</param>
        /// <param name="roomAccess">Room of the list that the player wants to enter and its code.</param>
        /// <returns>Whether the player entered, the state of the room and the identifier of the player.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult JoinListed(string playerName, RoomAccess roomAccess);

        /// <summary>
        /// Gets the rooms that wait for players, public and private, ordered by the name of their host.
        /// </summary>
        /// <returns>A short description of each room whose game did not start.</returns>
        List<RoomSummary> GetOpenRooms();

        /// <summary>
        /// Looks for a game in progress that the player left and can still join again.
        /// </summary>
        /// <param name="playerName">Name that the player shows.</param>
        /// <returns>The code of the room, or an empty string if the player has no pending game.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        string FindPendingRoomCode(string playerName);

        /// <summary>
        /// Puts back the room of a game saved before the server stopped, with the same code. Every player who did
        /// not lose starts absent, so the menu offers each one to go back to the game. The method gives each saved
        /// player the identifier that the room and the game use.
        /// </summary>
        /// <param name="saved">Saved game.</param>
        /// <returns>True if the room was restored; false if a room with that code already exists.</returns>
        /// <exception cref="System.ArgumentNullException">The saved game is null.</exception>
        bool TryRestoreRoom(SavedGame saved);

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host. If
        /// the game already started, the player stays as an absent player and can come back. If nobody stays
        /// inside, the room is deleted.
        /// </summary>
        /// <param name="member">Room and identifier of the player who leaves.</param>
        /// <returns>Whether the player was inside and the state of the room after leaving.</returns>
        RoomResult Leave(RoomMember member);

        /// <summary>
        /// Takes out a player whose connection broke. It works like <see cref="Leave"/>, but a game in progress
        /// is kept while a person is absent, even if only computer players are inside, so the player can come
        /// back before the waiting time ends.
        /// </summary>
        /// <param name="member">Room and identifier of the player whose connection broke.</param>
        /// <returns>Whether the player was inside and the state of the room after the loss.</returns>
        RoomResult Disconnect(RoomMember member);

        /// <summary>
        /// Forgets a player who left a game in progress and did not come back in time, so the player cannot
        /// return to that game.
        /// </summary>
        /// <param name="member">Room and identifier of the absent player.</param>
        /// <returns>Whether the room exists and its state.</returns>
        RoomResult ForgetAbsentPlayer(RoomMember member);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="member">Room and identifier of the player who asks for the change.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change and the state of the room.</returns>
        RoomResult UpdateSettings(RoomMember member, RoomSettings settings);

        /// <summary>
        /// Marks the game of the room as started. Only the host can do it and the room needs enough players.
        /// </summary>
        /// <param name="member">Room and identifier of the player who starts the game.</param>
        /// <returns>Outcome of the start and the state of the room.</returns>
        RoomResult StartGame(RoomMember member);

        /// <summary>
        /// Saves whether a player is ready to start. It can only change before the game starts.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        /// <param name="isReady">True if the player is ready.</param>
        /// <returns>The new state of the room, or why the change was rejected.</returns>
        RoomResult SetReady(RoomMember member, bool isReady);

        /// <summary>
        /// Counts one more chat message with offensive words written by a player of a room.
        /// </summary>
        /// <param name="member">Room and identifier of the player who wrote the message.</param>
        /// <returns>
        /// The infractions of the player in the room, or zero if the room or the player do not exist.
        /// </returns>
        int RegisterChatInfraction(RoomMember member);

        /// <summary>
        /// Finds the host of a room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="host">The host, when the room exists.</param>
        /// <returns>True if the room exists.</returns>
        bool TryGetHost(string roomCode, out RoomPlayer host);

        /// <summary>
        /// Takes another player out of the room for good, also during the game. Only the host can do it. The
        /// expelled player cannot come back to a game in progress. A computer player is also removed with this
        /// method, but only before the game starts, because the game cannot continue without one of its players.
        /// </summary>
        /// <param name="member">Room and identifier of the host who expels the player.</param>
        /// <param name="targetPlayerId">Identifier of the player to expel.</param>
        /// <returns>Outcome of the expulsion and the state of the room after it.</returns>
        RoomResult KickPlayer(RoomMember member, string targetPlayerId);

        /// <summary>
        /// Makes another player of the room the host. Only the current host can do it.
        /// </summary>
        /// <param name="member">Room and identifier of the current host.</param>
        /// <param name="newHostId">Identifier of the player who becomes the host.</param>
        /// <returns>Outcome of the change and the state of the room after it.</returns>
        RoomResult TransferHost(RoomMember member, string newHostId);

        /// <summary>
        /// Adds a computer player to the room. Only the host can do it, before the game starts and while the
        /// room has free places. The computer player is always ready.
        /// </summary>
        /// <param name="member">Room and identifier of the host.</param>
        /// <param name="difficulty">How well the computer player plays.</param>
        /// <returns>Outcome of the change and the state of the room after it.</returns>
        RoomResult AddComputer(RoomMember member, ComputerDifficulty difficulty);

        /// <summary>
        /// Closes the rooms that waited fifteen minutes or more without starting their game.
        /// </summary>
        /// <param name="now">Current moment, in UTC.</param>
        /// <returns>The rooms that were closed, with the players who were inside, or an empty list.</returns>
        List<RoomDetails> CloseExpiredRooms(DateTime now);

        /// <summary>
        /// Looks for a player inside a room.
        /// </summary>
        /// <param name="member">Room and identifier of the player.</param>
        /// <param name="player">The player, or null if the room does not exist or the player is not inside.</param>
        /// <returns>True if the room exists and the player is inside.</returns>
        bool TryGetPlayer(RoomMember member, out RoomPlayer player);
    }
}
