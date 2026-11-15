using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using YachtDice.Client.Logic.Game;
using YachtDice.Client.Logic.Rooms;
using YachtDice.GameServiceReference;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the match is played: dice, scorecard and chat. The server decides everything about the
    /// game, so the window only shows the state that the server sends and asks for the moves of the player.
    /// </summary>
    public partial class GameWindow : Window, IClosePromptingWindow
    {
        private const int MaxRolls = 3;
        private const int NoRolls = 0;
        private const int NoText = 0;
        private const int NoWaitingPlayers = 0;
        private const string NameSeparator = ", ";
        private const int ClockIntervalSeconds = 1;

        private readonly string _playerId;
        private readonly RoomGateway _roomGateway;
        private readonly RoomChat _chat;
        private readonly GameGateway _gameGateway;
        private readonly DiceBoard _diceBoard;
        private readonly ScorecardGrid _scorecard;
        private readonly YachtCelebration _celebration;
        private readonly HeldDiceSelection _heldDice = new HeldDiceSelection();
        private readonly StateVersionGate _versionGate = new StateVersionGate();
        private readonly TurnCountdown _countdown = new TurnCountdown();
        private readonly ReconnectWaitTracker _reconnectWait = new ReconnectWaitTracker();
        private readonly SoundEffectPlayer _sounds = new SoundEffectPlayer();
        private readonly DispatcherTimer _clock = new DispatcherTimer();
        private readonly GameConnectionRecovery _connectionRecovery;
        private readonly Func<Task<bool>> _recoverConnection;

        private bool _isLeavingOnClose = true;
        private bool _isClosed;
        private bool _isCloseAllowed;
        private bool _isAskingToClose;
        private bool _isBusy;
        private bool _isReconnecting;
        private string _hostId;
        private List<MatchParticipant> _presentPlayers;
        private List<MatchPlayer> _matchPlayers;
        private GameState _state;

        /// <summary>
        /// Initializes the game window of a room whose game has just started.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room.</param>
        /// <param name="chat">Connection of the player to the chat of the room and whether it is available.</param>
        public GameWindow(RoomGateway roomGateway, RoomChat chat)
        {
            InitializeComponent();

            _roomGateway = roomGateway;
            _chat = chat;
            _playerId = roomGateway.PlayerId;
            _gameGateway = new GameGateway(new RoomMembership(_playerId, roomGateway.CurrentRoom.Code));
            _connectionRecovery = new GameConnectionRecovery(roomGateway, chat, _gameGateway);
            _recoverConnection = RecoverConnectionAsync;
            _diceBoard = new DiceBoard(DiceCanvas, this);
            _scorecard = new ScorecardGrid(CategoriesPanel, this);
            _celebration = new YachtCelebration(CelebrationCanvas, this);
            _diceBoard.DieClicked += HandleDieClick;

            ShowRoom(roomGateway.CurrentRoom);
            ConnectGame();
            StartChat();
            _roomGateway.RoomChanged += HandleRoomChanged;
            Closing += HandleWindowClosing;
            Closed += HandleWindowClosed;
        }

        private TurnPhase CurrentPhase => TurnPhaseResolver.ResolvePhase(
            _state.Status == GameStatus.Finished,
            _state.CurrentPlayerId,
            _playerId);

        private bool IsLocalTurn => _state != null && CurrentPhase == TurnPhase.LocalTurn;

        private bool CanAct => IsLocalTurn && !_isBusy && !_isReconnecting;

        private bool CanRoll => CanAct && _state.RollsLeft > NoRolls;

        private bool CanHold => CanRoll && _state.RollsLeft < MaxRolls;

        private static List<MatchParticipant> ToParticipants(RoomState room)
        {
            return room.Players.Select(player => new MatchParticipant(player.Id, player.DisplayName)).ToList();
        }

        private void ShowRoom(RoomState room)
        {
            _matchPlayers = MatchPlayerFactory.CreateForRoom(room, _playerId);
            MatchPlayersItemsControl.ItemsSource = _matchPlayers;
            _presentPlayers = ToParticipants(room);
            _hostId = room.HostId;
            ApplyHostControls(room);
            LanguageSwitcherControl.ReopenWindowFunc = ReopenWindow;
        }

        private void ConnectGame()
        {
            _gameGateway.GameChanged += HandleGameChanged;
            ConnectionLossHandler.Watch(_gameGateway);
            _connectionRecovery.AttemptStarted += HandleRecoveryAttempt;
            _connectionRecovery.Restored += HandleConnectionRestored;
            ConnectionLossHandler.UseRecovery(_recoverConnection);
            _clock.Interval = TimeSpan.FromSeconds(ClockIntervalSeconds);
            _clock.Tick += HandleClockTick;
            RollButton.IsEnabled = false;
            Loaded += HandleWindowLoaded;
        }

        private void RememberLeftMatch()
        {
            _roomGateway.Session.LastOfferedMatchCode = _roomGateway.CurrentRoom.Code;
        }

        private Window ReopenWindow()
        {
            _isLeavingOnClose = false;

            return new GameWindow(_roomGateway, _chat);
        }

        private void StartChat()
        {
            ChatPanelControl.AddMessage(
                Strings.Game_Chat_SystemName,
                Strings.Game_Chat_MatchStartedMessage,
                ChatMessageKind.System);

            ChatPanelControl.Attach(_chat, _playerId);
        }

        private async void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            string errorMessage = await JoinGameAsync();

            if (!_isClosed)
            {
                ContinueAfterJoin(errorMessage);
            }
        }

        private void ContinueAfterJoin(string errorMessage)
        {
            if (errorMessage.Length > NoText)
            {
                CustomDialogWindow.ShowMessage(this, errorMessage);
                WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
            }
            else
            {
                _clock.Start();
            }
        }

        private async Task<string> JoinGameAsync()
        {
            var errorMessage = string.Empty;

            try
            {
                JoinGameResult result = await _gameGateway.JoinAsync();

                if (result.Code == GameResultCode.Success)
                {
                    AcceptState(result.State);
                }
                else
                {
                    errorMessage = GameResultMessages.GetMessage(result.Code);
                }
            }
            catch (GatewayException ex)
            {
                errorMessage = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            return errorMessage;
        }

        private void HandleGameChanged(GameState state, GameEventType eventType)
        {
            Dispatcher.BeginInvoke(new Action(() => ApplyChange(state, eventType)));
        }

        private void ApplyChange(GameState state, GameEventType eventType)
        {
            if (!_isClosed && _versionGate.TryAccept(state.Version))
            {
                AnnounceEvent(state, eventType);
                ShowState(state);
            }
        }

        private void AcceptState(GameState state)
        {
            if (!_isClosed && _versionGate.TryAccept(state.Version))
            {
                ShowState(state);
            }
        }

        private void ShowState(GameState state)
        {
            var isNewTurn = _state == null || _state.TurnNumber != state.TurnNumber;

            _state = state;

            if (isNewTurn)
            {
                _heldDice.Clear();
            }

            _countdown.Reset(state.TurnSecondsLeft);
            _reconnectWait.Update(FindAbsentPlayerIds(state));
            RenderState();

            if (isNewTurn && IsLocalTurn)
            {
                _sounds.Play(SoundEffect.YourTurn);
            }

            if (CurrentPhase == TurnPhase.Finished)
            {
                GoToResultsWindow();
            }
        }

        private void RenderState()
        {
            TurnTextBlock.Text = GetTurnText();
            HintTextBlock.Text = GetHintText();
            RoundInfoTextBlock.Text = string.Format(
                Strings.Game_Topbar_RoundInfo,
                _state.Round,
                Enum.GetValues(typeof(ScoreCategory)).Length);

            RenderTimer();
            RenderReconnectBanner();
            RenderDice();
            _scorecard.Render(_state, _matchPlayers, HandleCategoryClick);
            RenderRollButton();
        }

        private string GetTurnText()
        {
            string name = FindPlayerName(_state.CurrentPlayerId);
            var texts = new Dictionary<TurnPhase, string>
            {
                { TurnPhase.Finished, Strings.Game_Topbar_GameOver },
                { TurnPhase.Paused, Strings.Game_Topbar_Paused },
                { TurnPhase.LocalTurn, Strings.Game_Topbar_YourTurn },
                { TurnPhase.OpponentTurn, string.Format(Strings.Game_Topbar_TurnOf, name) }
            };

            return texts[CurrentPhase];
        }

        private string GetHintText()
        {
            string name = FindPlayerName(_state.CurrentPlayerId);
            TurnHint hint = TurnPhaseResolver.ResolveHint(
                CurrentPhase,
                _state.RollsLeft,
                MaxRolls);
            var texts = new Dictionary<TurnHint, string>
            {
                { TurnHint.None, string.Empty },
                { TurnHint.PressRoll, Strings.Game_Dice_HintPress },
                { TurnHint.ChooseOrRoll, Strings.Game_Dice_HintChoose },
                { TurnHint.PickBox, Strings.Game_Dice_HintPickBox },
                { TurnHint.WaitOpponent, string.Format(Strings.Game_Dice_HintOpponent, name) }
            };

            return texts[hint];
        }

        private string FindPlayerName(string playerId)
        {
            Func<PlayerScorecard, bool> isSamePlayerId = player => player.PlayerId == playerId;
            PlayerScorecard card = _state.Players.FirstOrDefault(isSamePlayerId);

            var name = string.Empty;

            if (card != null)
            {
                name = card.Name;
            }

            return name;
        }

        private void RenderTimer()
        {
            var isRunning = CurrentPhase == TurnPhase.LocalTurn || CurrentPhase == TurnPhase.OpponentTurn;

            if (isRunning)
            {
                TimerTextBlock.Text = string.Format(Strings.Game_Timer_SecondsLeft, _countdown.SecondsLeft);
            }
            else
            {
                TimerTextBlock.Text = string.Empty;
            }
        }

        private void HandleClockTick(object sender, EventArgs e)
        {
            if (_state != null)
            {
                _countdown.Tick();
                _reconnectWait.Tick();
                RenderTimer();
                RenderReconnectBanner();
            }
        }

        private static IEnumerable<string> FindAbsentPlayerIds(GameState state)
        {
            Func<PlayerScorecard, bool> isAbsent = player => !player.IsPresent;
            Func<PlayerScorecard, string> getId = player => player.PlayerId;

            return state.Players.Where(isAbsent).Select(getId).ToList();
        }

        private async Task<bool> RecoverConnectionAsync()
        {
            _isReconnecting = true;
            RollButton.IsEnabled = false;

            bool isRecovered = await _connectionRecovery.RecoverAsync();

            _isReconnecting = false;

            if (!_isClosed)
            {
                LinkBanner.Visibility = Visibility.Collapsed;
                RenderRollButtonIfStateKnown();
            }

            return isRecovered;
        }

        private void HandleRecoveryAttempt(int attempt)
        {
            if (!_isClosed)
            {
                LinkTextBlock.Text = string.Format(Strings.Game_Link_ReconnectingFormat, attempt);
                LinkBanner.Visibility = Visibility.Visible;
            }
        }

        private void HandleConnectionRestored(GameState state)
        {
            if (!_isClosed)
            {
                AcceptState(state);
                AddNotice(Strings.Game_Chat_ConnectionRestored);
            }
        }

        private void RenderReconnectBanner()
        {
            IReadOnlyList<string> waiting = _reconnectWait.GetWaitingPlayerIds();

            if (waiting.Count > NoWaitingPlayers && CurrentPhase != TurnPhase.Finished)
            {
                Func<string, string> findName = FindPlayerName;
                Func<string, int> findSeconds = _reconnectWait.GetSecondsLeft;

                ReconnectTextBlock.Text = string.Format(
                    Strings.Game_Reconnect_WaitingFormat,
                    string.Join(NameSeparator, waiting.Select(findName)),
                    waiting.Min(findSeconds));
                ReconnectBanner.Visibility = Visibility.Visible;
            }
            else
            {
                ReconnectBanner.Visibility = Visibility.Collapsed;
            }
        }

        private void RenderDice()
        {
            _diceBoard.Render(_state.Dice, IsDieHeld);
        }

        private bool IsDieHeld(int index)
        {
            var isHeld = index < _state.HeldDice.Length && _state.HeldDice[index];

            if (IsLocalTurn)
            {
                isHeld = _heldDice.IsHeld(index);
            }

            return isHeld;
        }

        private void RenderRollButtonIfStateKnown()
        {
            if (_state != null)
            {
                RenderRollButton();
            }
        }

        private void RenderRollButton()
        {
            RollButton.Content = string.Format(Strings.Game_Dice_ButtonRoll, MaxRolls - _state.RollsLeft);
            RollButton.IsEnabled = CanRoll;
        }

        private void HandleDieClick(int index)
        {
            if (_state != null && _heldDice.Toggle(index, CanHold))
            {
                RenderDice();
            }
        }

        private async void RollButton_Click(object sender, RoutedEventArgs e)
        {
            await RunActionAsync(() => _gameGateway.RollDiceAsync(_heldDice.ToArray()));
        }

        private async void HandleCategoryClick(ScoreCategory category)
        {
            if (IsLocalTurn)
            {
                await RunActionAsync(() => _gameGateway.SelectCategoryAsync(category));
            }
        }

        private async Task RunActionAsync(Func<Task<GameResultCode>> action)
        {
            if (!_isBusy && !_isReconnecting)
            {
                _isBusy = true;
                RollButton.IsEnabled = false;

                string errorMessage = await TryExecuteAsync(action);

                _isBusy = false;

                if (!_isClosed)
                {
                    RenderRollButton();
                    ShowActionError(errorMessage);
                }
            }
        }

        private async Task<string> TryExecuteAsync(Func<Task<GameResultCode>> action)
        {
            var errorMessage = string.Empty;

            try
            {
                GameResultCode result = await action();

                if (result != GameResultCode.Success)
                {
                    errorMessage = GameResultMessages.GetMessage(result);
                }
            }
            catch (GatewayException ex)
            {
                errorMessage = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            return errorMessage;
        }

        private void ShowActionError(string errorMessage)
        {
            if (errorMessage.Length > NoText)
            {
                CustomDialogWindow.ShowMessage(this, errorMessage);
            }
        }

        private void AnnounceEvent(GameState state, GameEventType eventType)
        {
            bool hasPreviousState = _state != null;
            bool isTurnTimedOut = hasPreviousState && eventType == GameEventType.TurnTimedOut;
            bool isCategoryScored = hasPreviousState && eventType == GameEventType.CategoryScored;

            if (hasPreviousState && eventType == GameEventType.DiceRolled)
            {
                _sounds.Play(SoundEffect.DiceRoll);
            }

            if (isTurnTimedOut)
            {
                AddNotice(string.Format(Strings.Game_Chat_TurnTimedOut, FindPlayerName(_state.CurrentPlayerId)));
            }

            if (isCategoryScored)
            {
                AnnounceYachtBonus(state);
                CelebrateYacht(state);
            }

            if (hasPreviousState && eventType == GameEventType.PlayerForfeited)
            {
                AnnounceForfeits();
            }
        }

        private void AnnounceForfeits()
        {
            foreach (string playerId in _reconnectWait.TakeExpiredPlayerIds())
            {
                AddNotice(string.Format(Strings.Game_Chat_PlayerForfeited, FindPlayerName(playerId)));
            }
        }

        private void AnnounceYachtBonus(GameState state)
        {
            bool hasNewBonus = HasNewYachtBonus(state);

            if (hasNewBonus)
            {
                AddNotice(string.Format(Strings.Game_Chat_YachtBonus, FindPlayerName(_state.CurrentPlayerId)));
            }
        }

        private void CelebrateYacht(GameState state)
        {
            string scorerId = _state.CurrentPlayerId;
            CategoryScore before = FindYachtScore(_state, scorerId);
            CategoryScore after = FindYachtScore(state, scorerId);
            bool isFilledBefore = before != null && before.IsFilled;
            bool isCelebrationDue = after != null
                && YachtCelebrationRule.IsCelebrationDue(isFilledBefore, after.IsFilled, after.Points);

            if (isCelebrationDue)
            {
                _celebration.Play(FindPlayerName(scorerId), after.Points);
                _sounds.Play(SoundEffect.Yacht);
            }
            else
            {
                _sounds.Play(SoundEffect.Score);
            }
        }

        private static CategoryScore FindYachtScore(GameState state, string playerId)
        {
            Func<PlayerScorecard, bool> isSamePlayerId = player => player.PlayerId == playerId;
            PlayerScorecard card = state.Players.FirstOrDefault(isSamePlayerId);
            Func<CategoryScore, bool> isSameCategory = score => score.Category == ScoreCategory.Yacht;

            return card?.Scores.FirstOrDefault(isSameCategory);
        }

        private bool HasNewYachtBonus(GameState state)
        {
            string scorerId = _state.CurrentPlayerId;
            Func<PlayerScorecard, bool> isScorer = player => player.PlayerId == scorerId;
            PlayerScorecard before = _state.Players.FirstOrDefault(isScorer);
            PlayerScorecard after = state.Players.FirstOrDefault(isScorer);

            bool hasBothScorecards = before != null && after != null;

            return hasBothScorecards && after.YachtBonus > before.YachtBonus;
        }

        private void GoToResultsWindow()
        {
            IEnumerable<StandingEntry> entries = _state.Players.Select(CreateStandingEntry);

            if (_state.HasResultSaveFailed)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Game_ResultNotSaved);
            }

            _sounds.Play(SoundEffect.GameOver);
            RememberLeftMatch();
            WindowNavigator.SwitchTo(this, new ResultsWindow(
                _roomGateway.Session,
                StandingsBuilder.Build(entries),
                _playerId));
        }

        private static StandingEntry CreateStandingEntry(PlayerScorecard card)
        {
            return new StandingEntry(card.PlayerId, card.Name, card.Total) { ExperienceGained = card.ExperienceGained };
        }

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            Dispatcher.BeginInvoke(new Action(() => ApplyRoomChange(room, eventType)));
        }

        private void ApplyRoomChange(RoomState room, RoomEventType eventType)
        {
            if (!_isClosed)
            {
                ShowRoomChange(room, eventType);
            }
        }

        private void ShowRoomChange(RoomState room, RoomEventType eventType)
        {
            if (eventType == RoomEventType.PlayerKicked && !IsInside(room))
            {
                LeaveBecauseKicked();
            }
            else
            {
                AnnouncePresence(room, eventType);
                ShowRoomPlayers(room);
                ApplyHostControls(room);
            }
        }

        private bool IsInside(RoomState room)
        {
            Func<RoomPlayer, bool> isSameId = player => player.Id == _playerId;

            return room.Players.Any(isSameId);
        }

        private void LeaveBecauseKicked()
        {
            CustomDialogWindow.ShowMessage(this, Strings.Lobby_KickedMessage);
            WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
        }

        private void AnnouncePresence(RoomState room, RoomEventType eventType)
        {
            List<MatchParticipant> currentPlayers = ToParticipants(room);

            foreach (MatchParticipant departed in PresenceComparer.FindDeparted(_presentPlayers, currentPlayers))
            {
                AddNotice(PresenceMessages.GetDepartureMessage(eventType, departed.Name));
            }

            foreach (MatchParticipant arrived in PresenceComparer.FindArrived(_presentPlayers, currentPlayers))
            {
                AddNotice(PresenceMessages.GetReturnMessage(arrived.Name));
            }

            _presentPlayers = currentPlayers;
            AnnounceNewHost(room);
        }

        private void AnnounceNewHost(RoomState room)
        {
            Func<RoomPlayer, bool> isSameId = player => player.Id == room.HostId;
            RoomPlayer host = room.Players.FirstOrDefault(isSameId);

            if (host != null && room.HostId != _hostId)
            {
                AddNotice(string.Format(Strings.Lobby_Chat_NewHost, host.DisplayName));
            }

            _hostId = room.HostId;
        }

        private void AddNotice(string text)
        {
            ChatPanelControl.AddMessage(Strings.Game_Chat_SystemName, text, ChatMessageKind.System);
        }

        private void ApplyHostControls(RoomState room)
        {
            KickPlayerButton.Visibility = room.HostId == _playerId ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowRoomPlayers(RoomState room)
        {
            _matchPlayers = MatchPlayerFactory.UpdatePresence(_matchPlayers, room.Players);
            MatchPlayersItemsControl.ItemsSource = _matchPlayers;
        }

        /// <summary>
        /// Makes the window close without asking the player, because the program decided to close it.
        /// </summary>
        public void AllowClose()
        {
            _isCloseAllowed = true;
        }

        private void HandleWindowClosing(object sender, CancelEventArgs e)
        {
            if (!_isCloseAllowed && _state != null && CurrentPhase != TurnPhase.Finished)
            {
                e.Cancel = true;

                if (!_isAskingToClose)
                {
                    _isAskingToClose = true;
                    Dispatcher.BeginInvoke(new Action(AskToClose));
                }
            }
        }

        private void AskToClose()
        {
            string message = string.Format(
                Strings.Dialogs_CloseWindowDuringMatch,
                ReconnectWaitTracker.ReconnectSeconds);
            bool isConfirmed = CustomDialogWindow.Confirm(this, message);

            _isAskingToClose = false;

            if (isConfirmed && !_isClosed)
            {
                WindowNavigator.Close(this);
            }
        }

        private async void HandleWindowClosed(object sender, EventArgs e)
        {
            _isClosed = true;
            _clock.Stop();
            ConnectionLossHandler.ReleaseRecovery(_recoverConnection);
            ChatPanelControl.Detach();
            _roomGateway.RoomChanged -= HandleRoomChanged;
            _gameGateway.GameChanged -= HandleGameChanged;
            await _gameGateway.CloseAsync();

            if (_isLeavingOnClose)
            {
                await LobbyNavigator.LeaveRoomAsync(_chat, _roomGateway);
            }
        }

        private void LeaveMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogContent = new DialogContent
            {
                Title = Strings.Game_Sidebar_ButtonLeave,
                Message = Strings.Dialogs_LeaveMatch,
                PrimaryButtonText = Strings.Dialogs_ButtonOK,
                SecondaryButtonText = Strings.Dialogs_ButtonIgnore
            };

            if (new CustomDialogWindow(dialogContent).ShowDialog() == true)
            {
                RememberLeftMatch();
                WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
            }
        }

        private void HowToPlayButton_Click(object sender, RoutedEventArgs e)
        {
            new HowToPlayWindow(this).ShowDialog();
        }

        private void KickPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Func<MatchPlayer, bool> isActiveOpponent = player => !player.IsLocalPlayer && !player.HasLeft;
            List<MatchPlayer> candidates = _matchPlayers
                .Where(isActiveOpponent)
                .ToList();

            if (candidates.Count == NoText)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Game_KickNoPlayers);
            }
            else
            {
                ShowKickMenu(candidates);
            }
        }

        private void ShowKickMenu(IEnumerable<MatchPlayer> candidates)
        {
            var menu = new ContextMenu { PlacementTarget = KickPlayerButton, Placement = PlacementMode.Top };

            foreach (MatchPlayer candidate in candidates)
            {
                var item = new MenuItem { Header = candidate.Name, Tag = candidate.Id };
                item.Click += KickMenuItem_Click;
                menu.Items.Add(item);
            }

            menu.IsOpen = true;
        }

        private async void KickMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)sender;
            string question = string.Format(Strings.Lobby_ConfirmKick, item.Header);

            if (CustomDialogWindow.Confirm(this, question))
            {
                await KickAsync((string)item.Tag);
            }
        }

        private async Task KickAsync(string targetPlayerId)
        {
            var errorMessage = string.Empty;

            try
            {
                RoomResultCode result = await _roomGateway.KickPlayerAsync(targetPlayerId);

                if (result != RoomResultCode.Success)
                {
                    errorMessage = RoomResultMessages.GetMessage(result);
                }
            }
            catch (GatewayException ex)
            {
                errorMessage = GatewayErrorMessages.GetMessage(ex.ErrorCode);
            }

            ShowActionError(errorMessage);
        }
    }
}
