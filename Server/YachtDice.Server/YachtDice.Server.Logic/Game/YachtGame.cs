using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The rules of one game: whose turn it is, the dice, the rolls left and the scorecards. It never
    /// trusts the caller: every action says who does it, and the game rejects the action when it is not
    /// allowed. It is not thread-safe: <see cref="GameManager"/> only uses it while it holds its lock.
    /// </summary>
    internal sealed class YachtGame
    {
        private const int NoPlayerIndex = -1;
        private const int NoRolls = 0;
        private const int FirstTurnNumber = 1;
        private const int RoundOffset = 1;

        private readonly List<GamePlayer> _players;
        private readonly IDieRoller _dieRoller;
        private readonly ScoreCalculator _calculator = new ScoreCalculator();
        private readonly int[] _dice = new int[DiceRules.DiceCount];
        private readonly bool[] _heldDice = new bool[DiceRules.DiceCount];
        private int _turnIndex;
        private int _rollsMade;
        private int _version;

        /// <summary>
        /// Initializes a game where the first participant plays the first turn.
        /// </summary>
        /// <param name="participants">Players of the game, in the order of the turns.</param>
        /// <param name="dieRoller">Rolls the dice of the game.</param>
        /// <param name="turnSeconds">Seconds that a player has to play a turn.</param>
        public YachtGame(IEnumerable<GameParticipant> participants, IDieRoller dieRoller, int turnSeconds)
        {
            _players = participants
                .Select(participant => new GamePlayer(participant, new ScoreCard(_calculator)))
                .ToList();
            _dieRoller = dieRoller;
            TurnSeconds = turnSeconds;
            TurnNumber = FirstTurnNumber;
            _turnIndex = NoPlayerIndex;
            _turnIndex = FindNextPlayerIndex();
        }

        /// <summary>
        /// Gets the seconds that a player has to play a turn.
        /// </summary>
        public int TurnSeconds { get; }

        /// <summary>
        /// Gets the number of the turn. It grows each time the turn passes to another player.
        /// </summary>
        public int TurnNumber { get; private set; }

        /// <summary>
        /// Gets the phase of the game.
        /// </summary>
        public GameStatus Status { get; private set; }

        /// <summary>
        /// Tells whether a player takes part in the game.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>True if the player is in the game.</returns>
        public bool HasPlayer(string playerId)
        {
            Func<GamePlayer, bool> isSamePlayerId = player => player.PlayerId == playerId;

            return _players.Any(isSamePlayerId);
        }

        /// <summary>
        /// Rolls the dice for the player whose turn it is. The first roll of a turn rolls the five dice and
        /// the next rolls only roll the dice that are not held.
        /// </summary>
        /// <param name="playerId">Player who rolls.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>The outcome of the action.</returns>
        public GameActionStatus Roll(string playerId, IReadOnlyList<bool> heldDice)
        {
            var status = ValidateTurn(playerId);

            if (status == GameActionStatus.Success)
            {
                status = RollValidated(heldDice);
            }

            return status;
        }

        /// <summary>
        /// Writes the points of the current dice in a box and passes the turn to the next player.
        /// </summary>
        /// <param name="playerId">Player who chooses the box.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>The outcome of the action.</returns>
        public GameActionStatus Score(string playerId, ScoreCategory category)
        {
            var status = ValidateTurn(playerId);

            if (status == GameActionStatus.Success)
            {
                status = ScoreValidated(category);
            }

            return status;
        }

        /// <summary>
        /// Plays the turn of the current player when the time is over: rolls if the player did not roll yet
        /// and writes the points in the box that gives more points (see <see cref="AutoPlayChooser"/>).
        /// </summary>
        /// <returns>The outcome of the action.</returns>
        public GameActionStatus AutoPlay()
        {
            var status = GameActionStatus.GameFinished;

            if (Status == GameStatus.InProgress && _turnIndex != NoPlayerIndex)
            {
                if (_rollsMade == NoRolls)
                {
                    RollAll();
                }

                status = ScoreValidated(ChooseBestCategory());
            }

            return status;
        }

        /// <summary>
        /// Marks that a player is not connected. If it was the turn of that player, the turn passes on.
        /// </summary>
        /// <param name="playerId">Player who left.</param>
        public void MarkAbsent(string playerId)
        {
            SetPresence(playerId, false);
        }

        /// <summary>
        /// Marks that a player came back. If nobody could play, the player gets the turn.
        /// </summary>
        /// <param name="playerId">Player who returned.</param>
        public void MarkPresent(string playerId)
        {
            SetPresence(playerId, true);
        }

        /// <summary>
        /// Removes a player for good, for example because the host kicked the player.
        /// </summary>
        /// <param name="playerId">Player to remove.</param>
        public void Remove(string playerId)
        {
            Predicate<GamePlayer> isSamePlayerId = player => player.PlayerId == playerId;
            var index = _players.FindIndex(isSamePlayerId);

            if (index != NoPlayerIndex)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Creates a copy of the current state of the game.
        /// </summary>
        /// <returns>The snapshot.</returns>
        public GameSnapshot CreateSnapshot()
        {
            _version++;

            var snapshot = new GameSnapshot
            {
                Version = _version,
                Status = Status,
                CurrentPlayerId = GetCurrentPlayerId(),
                TurnNumber = TurnNumber,
                Round = GetCurrentRound(),
                RollsLeft = DiceRules.RollsPerTurn - _rollsMade,
                TurnSeconds = TurnSeconds
            };

            FillDice(snapshot);
            FillPlayers(snapshot);

            return snapshot;
        }

        private static PlayerSnapshot CreatePlayerSnapshot(GamePlayer player)
        {
            var snapshot = new PlayerSnapshot(player.PlayerId, player.Name, player.IsPresent)
            {
                UpperSubtotal = player.Card.UpperSubtotal,
                UpperBonus = player.Card.UpperBonus,
                YachtBonus = player.Card.YachtBonus,
                Total = player.Card.Total
            };

            foreach (var category in ScoreCategories.All)
            {
                snapshot.Scores.Add(
                    new ScoreEntry(category, player.Card.IsFilled(category), player.Card.GetScore(category)));
            }

            return snapshot;
        }

        private static void MarkWinners(GameSnapshot snapshot)
        {
            var bestTotal = snapshot.Players.Max(player => player.Total);

            foreach (var player in snapshot.Players)
            {
                player.IsWinner = player.Total == bestTotal;
            }
        }

        private static void AwardExperience(GameSnapshot snapshot)
        {
            foreach (var player in snapshot.Players)
            {
                if (player.IsPresent)
                {
                    player.ExperienceGained = ExperienceRules.CalculateGain(player.Total, player.IsWinner);
                }
            }
        }

        private GameActionStatus ValidateTurn(string playerId)
        {
            GameActionStatus status;

            if (Status == GameStatus.Finished)
            {
                status = GameActionStatus.GameFinished;
            }
            else if (!HasPlayer(playerId))
            {
                status = GameActionStatus.PlayerNotInGame;
            }
            else if (_turnIndex == NoPlayerIndex || _players[_turnIndex].PlayerId != playerId)
            {
                status = GameActionStatus.NotYourTurn;
            }
            else
            {
                status = GameActionStatus.Success;
            }

            return status;
        }

        private GameActionStatus RollValidated(IReadOnlyList<bool> heldDice)
        {
            GameActionStatus status;

            if (_rollsMade >= DiceRules.RollsPerTurn)
            {
                status = GameActionStatus.NoRollsLeft;
            }
            else if (_rollsMade == NoRolls)
            {
                RollAll();
                status = GameActionStatus.Success;
            }
            else if (heldDice.Count != DiceRules.DiceCount)
            {
                status = GameActionStatus.InvalidHeldDice;
            }
            else
            {
                RollFree(heldDice);
                status = GameActionStatus.Success;
            }

            return status;
        }

        private GameActionStatus ScoreValidated(ScoreCategory category)
        {
            GameActionStatus status;

            if (_rollsMade == NoRolls)
            {
                status = GameActionStatus.MustRollFirst;
            }
            else
            {
                status = ApplyScore(category);
            }

            return status;
        }

        private GameActionStatus ApplyScore(ScoreCategory category)
        {
            var scoreStatus = _players[_turnIndex].Card.Score(category, _dice);
            var status = GameActionStatus.Success;

            if (scoreStatus == ScoreStatus.AlreadyFilled)
            {
                status = GameActionStatus.CategoryFilled;
            }
            else if (scoreStatus == ScoreStatus.NotAllowed)
            {
                status = GameActionStatus.CategoryNotAllowed;
            }
            else
            {
                AdvanceTurn();
            }

            return status;
        }

        private ScoreCategory ChooseBestCategory()
        {
            return AutoPlayChooser.Choose(_players[_turnIndex].Card.GetOptions(_dice));
        }

        private void RollAll()
        {
            RollFree(new bool[DiceRules.DiceCount]);
        }

        private void RollFree(IReadOnlyList<bool> heldDice)
        {
            for (var index = 0; index < DiceRules.DiceCount; index++)
            {
                _heldDice[index] = heldDice[index];

                if (!heldDice[index])
                {
                    _dice[index] = _dieRoller.Roll();
                }
            }

            _rollsMade++;
        }

        private void AdvanceTurn()
        {
            _turnIndex = FindNextPlayerIndex();
            _rollsMade = NoRolls;
            TurnNumber++;
            Array.Clear(_dice, 0, _dice.Length);
            Array.Clear(_heldDice, 0, _heldDice.Length);
            UpdateStatus();
        }

        private int FindNextPlayerIndex()
        {
            var nextIndex = NoPlayerIndex;

            for (var step = 1; step <= _players.Count && nextIndex == NoPlayerIndex; step++)
            {
                var candidate = (_turnIndex + step + _players.Count) % _players.Count;

                if (_players[candidate].CanPlay)
                {
                    nextIndex = candidate;
                }
            }

            return nextIndex;
        }

        private void UpdateStatus()
        {
            var isSomeonePresent = _players.Any(player => player.IsPresent);

            if (_turnIndex == NoPlayerIndex && isSomeonePresent)
            {
                Status = GameStatus.Finished;
            }
        }

        private void SetPresence(string playerId, bool isPresent)
        {
            Func<GamePlayer, bool> isSamePlayerId = candidate => candidate.PlayerId == playerId;
            var player = _players.FirstOrDefault(isSamePlayerId);

            if (player != null && Status == GameStatus.InProgress)
            {
                player.IsPresent = isPresent;
                RefreshTurnAfterPresenceChange();
            }
        }

        private void RefreshTurnAfterPresenceChange()
        {
            var isCurrentPlayerBlocked = _turnIndex != NoPlayerIndex && !_players[_turnIndex].CanPlay;

            if (isCurrentPlayerBlocked || _turnIndex == NoPlayerIndex)
            {
                AdvanceTurn();
            }
        }

        private void RemoveAt(int index)
        {
            var isCurrentPlayer = index == _turnIndex;

            _players.RemoveAt(index);

            if (index <= _turnIndex)
            {
                _turnIndex--;
            }

            if (isCurrentPlayer)
            {
                AdvanceTurn();
            }
        }

        private string GetCurrentPlayerId()
        {
            var playerId = string.Empty;

            if (_turnIndex != NoPlayerIndex)
            {
                playerId = _players[_turnIndex].PlayerId;
            }

            return playerId;
        }

        private int GetCurrentRound()
        {
            var round = RoundOffset;

            if (_turnIndex != NoPlayerIndex)
            {
                var openCount = _players[_turnIndex].Card.GetOpenCategories().Count;
                round = ScoreCategories.All.Count - openCount + RoundOffset;
            }

            return round;
        }

        private void FillDice(GameSnapshot snapshot)
        {
            if (_rollsMade > NoRolls)
            {
                foreach (var die in _dice)
                {
                    snapshot.Dice.Add(die);
                }

                foreach (var held in _heldDice)
                {
                    snapshot.HeldDice.Add(held);
                }

                foreach (var option in _players[_turnIndex].Card.GetOptions(_dice))
                {
                    snapshot.Options.Add(option);
                }
            }
        }

        private void FillPlayers(GameSnapshot snapshot)
        {
            foreach (var player in _players)
            {
                snapshot.Players.Add(CreatePlayerSnapshot(player));
            }

            if (Status == GameStatus.Finished)
            {
                MarkWinners(snapshot);
                AwardExperience(snapshot);
            }
        }
    }
}
