using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the games in progress that are saved for a restart of the server. A game
    /// in progress is a room with the Playing status, a game without an end date, one scorecard for each player and
    /// the turn in TurnState with its dice in RolledDie. The order of the turns is the order of JoinDate.
    /// </summary>
    public class GameCheckpointRepository : IGameCheckpointRepository
    {
        private const string NullCheckpointMessage = "The game in progress cannot be null.";
        private const string SaveFailedMessage = "Database error occurred while saving the game in progress.";
        private const string QueryFailedMessage = "Database error occurred while reading the games in progress.";
        private const string RemoveFailedMessage = "Database error occurred while removing the game in progress.";
        private const int DefaultMaxPlayers = 4;
        private const int NoRolls = 0;
        private const int NoYachtBonus = 0;

        /// <summary>
        /// Saves the current state of a game in progress, or creates it the first time.
        /// </summary>
        /// <param name="checkpoint">State of the game.</param>
        /// <exception cref="ArgumentNullException">The game is null.</exception>
        /// <exception cref="DataAccessException">The game could not be saved.</exception>
        public void Save(GameCheckpointRecord checkpoint)
        {
            if (checkpoint == null)
            {
                throw new ArgumentNullException(nameof(checkpoint), NullCheckpointMessage);
            }

            DatabaseOperation.Save(context => ApplyCheckpoint(context, checkpoint), SaveFailedMessage);
        }

        /// <summary>
        /// Gets every game in progress that was saved.
        /// </summary>
        /// <returns>The games, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The games could not be read.</exception>
        public List<GameCheckpointRecord> FindAll()
        {
            List<GameCheckpointRecord> checkpoints = DatabaseOperation.Query(SelectCheckpoints, QueryFailedMessage);

            return checkpoints;
        }

        /// <summary>
        /// Removes a saved game in progress, for example because it finished or its room was closed.
        /// </summary>
        /// <param name="roomCode">Code of the room of the game.</param>
        /// <exception cref="DataAccessException">The game could not be removed.</exception>
        public void Remove(string roomCode)
        {
            DatabaseOperation.Save(context => RemoveCheckpoint(context, roomCode), RemoveFailedMessage);
        }

        private static IQueryable<Room> QueryPlayingRooms(YachtDiceContext context)
        {
            string playingStatus = RoomRecordStatus.Playing.ToString();
            Expression<Func<Room, bool>> isPlaying = room => room.Status == playingStatus;

            return context.Room
                .Include(room => room.RoomPlayer.Select(roomPlayer => roomPlayer.Player))
                .Include(room => room.GameHistory.Select(
                    game => game.Scorecard.Select(card => card.ScorecardEntry.Select(entry => entry.Category))))
                .Include(room => room.GameHistory.Select(
                    game => game.TurnState.Select(turn => turn.RolledDie)))
                .Where(isPlaying);
        }

        private static Room FindPlayingRoom(YachtDiceContext context, string roomCode)
        {
            Expression<Func<Room, bool>> hasCode = room => room.Code == roomCode;

            return QueryPlayingRooms(context).FirstOrDefault(hasCode);
        }

        private static bool IsSameName(string firstName, string secondName)
        {
            return string.Equals(firstName, secondName, StringComparison.OrdinalIgnoreCase);
        }

        private static void ApplyCheckpoint(YachtDiceContext context, GameCheckpointRecord checkpoint)
        {
            context.Category.Load();
            Room room = FindPlayingRoom(context, checkpoint.RoomCode);

            if (room == null)
            {
                room = CreateRoom(context, checkpoint);
            }

            if (room != null)
            {
                GameHistory game = room.GameHistory.First();

                foreach (CheckpointPlayerRecord player in checkpoint.Players)
                {
                    UpdatePlayer(context, game, player);
                }

                UpdateTurn(context, game, checkpoint);
            }
        }

        private static Room CreateRoom(YachtDiceContext context, GameCheckpointRecord checkpoint)
        {
            Func<Player, bool> isFound = account => account != null;
            List<Player> accounts = checkpoint.Players
                .Select(player => FindAccount(context, player.PlayerName))
                .Where(isFound)
                .ToList();
            Room room = null;

            if (accounts.Any())
            {
                room = BuildRoom(checkpoint, accounts);
                context.Room.Add(room);
            }

            return room;
        }

        private static Room BuildRoom(GameCheckpointRecord checkpoint, List<Player> accounts)
        {
            DateTime now = DateTime.UtcNow;
            RoomRecordType roomType = checkpoint.IsPrivate ? RoomRecordType.Private : RoomRecordType.Public;
            var room = new Room
            {
                HostPlayerId = accounts.First().Id,
                Code = checkpoint.RoomCode,
                RoomName = checkpoint.RoomName ?? string.Empty,
                MaxPlayers = checkpoint.MaxPlayers,
                RoomType = roomType.ToString(),
                Status = RoomRecordStatus.Playing.ToString(),
                CreateDate = now,
                GameSpeed = checkpoint.Speed
            };
            var game = new GameHistory { Room = room };

            for (int index = 0; index < accounts.Count; index++)
            {
                room.RoomPlayer.Add(new RoomPlayer
                {
                    Player = accounts[index],
                    PlayerId = accounts[index].Id,
                    JoinDate = now.AddSeconds(index),
                    Status = RoomPlayerRecordStatus.Playing.ToString()
                });
                game.Scorecard.Add(new Scorecard
                {
                    Player = accounts[index],
                    PlayerId = accounts[index].Id,
                    UpperBonus = false,
                    YachtBonusCt = NoYachtBonus
                });
            }

            room.GameHistory.Add(game);

            return room;
        }

        private static Player FindAccount(YachtDiceContext context, string playerName)
        {
            Expression<Func<Player, bool>> hasName = account => account.DisplayName == playerName;

            return context.Player.FirstOrDefault(hasName);
        }

        private static void UpdatePlayer(YachtDiceContext context, GameHistory game, CheckpointPlayerRecord player)
        {
            Func<Scorecard, bool> isCardOfPlayer = card => IsSameName(card.Player.DisplayName, player.PlayerName);
            Func<RoomPlayer, bool> isSamePlayer =
                candidate => IsSameName(candidate.Player.DisplayName, player.PlayerName);
            Func<ScoreRecord, bool> isUsed = score => score.IsUsed;
            Scorecard scorecard = game.Scorecard.FirstOrDefault(isCardOfPlayer);
            RoomPlayer roomPlayer = game.Room.RoomPlayer.FirstOrDefault(isSamePlayer);

            if (scorecard != null && roomPlayer != null)
            {
                RoomPlayerRecordStatus status =
                    player.HasForfeited ? RoomPlayerRecordStatus.Forfeited : RoomPlayerRecordStatus.Playing;

                roomPlayer.Status = status.ToString();
                scorecard.UpperBonus = player.HasUpperBonus;
                scorecard.YachtBonusCt = player.YachtBonusCount;

                foreach (ScoreRecord score in player.Scores.Where(isUsed))
                {
                    SaveEntry(context, scorecard, score);
                }
            }
        }

        private static void SaveEntry(YachtDiceContext context, Scorecard scorecard, ScoreRecord score)
        {
            Func<Category, bool> hasName = candidate => candidate.Name == score.CategoryName;
            Category category = context.Category.Local.FirstOrDefault(hasName);

            if (category != null)
            {
                Func<ScorecardEntry, bool> isOfCategory = entry => entry.CategoryId == category.Id;
                ScorecardEntry savedEntry = scorecard.ScorecardEntry.FirstOrDefault(isOfCategory);

                if (savedEntry == null)
                {
                    scorecard.ScorecardEntry.Add(new ScorecardEntry
                    {
                        Category = category,
                        CategoryId = category.Id,
                        Points = score.Points,
                        IsUsed = true
                    });
                }
                else
                {
                    savedEntry.Points = score.Points;
                }
            }
        }

        private static void UpdateTurn(YachtDiceContext context, GameHistory game, GameCheckpointRecord checkpoint)
        {
            Func<RoomPlayer, bool> isCurrentPlayer =
                roomPlayer => IsSameName(roomPlayer.Player.DisplayName, checkpoint.CurrentPlayerName);
            RoomPlayer currentPlayer = game.Room.RoomPlayer.FirstOrDefault(isCurrentPlayer);
            TurnState turn = game.TurnState.FirstOrDefault();

            if (currentPlayer == null && turn != null)
            {
                RemoveTurn(context, turn);
            }
            else if (currentPlayer != null)
            {
                if (turn == null)
                {
                    turn = new TurnState();
                    game.TurnState.Add(turn);
                }

                turn.Player = currentPlayer.Player;
                turn.PlayerId = currentPlayer.PlayerId;
                turn.RollNum = checkpoint.RollsMade;
                SaveDice(context, turn, checkpoint.Dice);
            }
        }

        private static void SaveDice(YachtDiceContext context, TurnState turn, List<RolledDieRecord> dice)
        {
            var rolledPositions = new HashSet<int>(dice.Select(die => die.Position));
            Func<RolledDie, bool> isNotRolled = savedDie => !rolledPositions.Contains(savedDie.DiePosition);

            context.RolledDie.RemoveRange(turn.RolledDie.Where(isNotRolled).ToList());

            foreach (RolledDieRecord die in dice)
            {
                Func<RolledDie, bool> isSamePosition = candidate => candidate.DiePosition == die.Position;
                RolledDie savedDie = turn.RolledDie.FirstOrDefault(isSamePosition);

                if (savedDie == null)
                {
                    savedDie = new RolledDie { DiePosition = die.Position };
                    turn.RolledDie.Add(savedDie);
                }

                savedDie.Value = die.Value;
                savedDie.IsHeld = die.IsHeld;
            }
        }

        private static void RemoveTurn(YachtDiceContext context, TurnState turn)
        {
            context.RolledDie.RemoveRange(turn.RolledDie.ToList());
            context.TurnState.Remove(turn);
        }

        private static void RemoveCheckpoint(YachtDiceContext context, string roomCode)
        {
            Room room = FindPlayingRoom(context, roomCode);

            if (room != null)
            {
                foreach (GameHistory game in room.GameHistory.ToList())
                {
                    RemoveGame(context, game);
                }

                context.RoomPlayer.RemoveRange(room.RoomPlayer.ToList());
                context.Room.Remove(room);
            }
        }

        private static void RemoveGame(YachtDiceContext context, GameHistory game)
        {
            foreach (TurnState turn in game.TurnState.ToList())
            {
                RemoveTurn(context, turn);
            }

            foreach (Scorecard scorecard in game.Scorecard.ToList())
            {
                context.ScorecardEntry.RemoveRange(scorecard.ScorecardEntry.ToList());
                context.Scorecard.Remove(scorecard);
            }

            context.GameHistory.Remove(game);
        }

        private static List<GameCheckpointRecord> SelectCheckpoints(YachtDiceContext context)
        {
            Func<Room, bool> hasGame = room => room.GameHistory.Any();
            List<Room> rooms = QueryPlayingRooms(context).ToList();

            return rooms.Where(hasGame).Select(ToRecord).ToList();
        }

        private static GameCheckpointRecord ToRecord(Room room)
        {
            GameHistory game = room.GameHistory.First();
            TurnState turn = game.TurnState.FirstOrDefault();
            string privateType = RoomRecordType.Private.ToString();
            var record = new GameCheckpointRecord
            {
                RoomCode = room.Code,
                RoomName = room.RoomName,
                MaxPlayers = room.MaxPlayers ?? DefaultMaxPlayers,
                IsPrivate = room.RoomType == privateType,
                Speed = room.GameSpeed ?? string.Empty,
                Players = room.RoomPlayer
                    .OrderBy(roomPlayer => roomPlayer.JoinDate)
                    .Select(roomPlayer => ToPlayerRecord(game, roomPlayer))
                    .ToList()
            };

            if (turn != null)
            {
                record.CurrentPlayerName = turn.Player.DisplayName;
                record.RollsMade = turn.RollNum ?? NoRolls;
                record.Dice = turn.RolledDie
                    .OrderBy(savedDie => savedDie.DiePosition)
                    .Select(ToDieRecord)
                    .ToList();
            }

            return record;
        }

        private static CheckpointPlayerRecord ToPlayerRecord(GameHistory game, RoomPlayer roomPlayer)
        {
            Func<Scorecard, bool> isCardOfPlayer = card => card.PlayerId == roomPlayer.PlayerId;
            Scorecard scorecard = game.Scorecard.FirstOrDefault(isCardOfPlayer);
            string forfeitedStatus = RoomPlayerRecordStatus.Forfeited.ToString();
            var record = new CheckpointPlayerRecord
            {
                PlayerName = roomPlayer.Player.DisplayName,
                HasForfeited = roomPlayer.Status == forfeitedStatus
            };

            if (scorecard != null)
            {
                record.HasUpperBonus = scorecard.UpperBonus ?? false;
                record.YachtBonusCount = scorecard.YachtBonusCt ?? NoYachtBonus;
                record.Scores = scorecard.ScorecardEntry.Select(ToScoreRecord).ToList();
            }

            return record;
        }

        private static ScoreRecord ToScoreRecord(ScorecardEntry entry)
        {
            return new ScoreRecord
            {
                CategoryName = entry.Category.Name,
                Points = entry.Points,
                IsUsed = entry.IsUsed ?? true
            };
        }

        private static RolledDieRecord ToDieRecord(RolledDie savedDie)
        {
            return new RolledDieRecord
            {
                Position = savedDie.DiePosition,
                Value = savedDie.Value,
                IsHeld = savedDie.IsHeld
            };
        }
    }
}
