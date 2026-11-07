using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using YachtDice.ChatServiceReference;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the match is played: dice, scorecard and chat.
    /// </summary>
    public partial class GameWindow : Window
    {
        private const int DiceCount = 5;
        private const int MaxRolls = 3;
        private const int MinDiceValue = 1;
        private const int MaxDiceValueExclusive = 7;
        private const int DiceSize = 80;
        private const int DiceCornerRadius = 12;
        private const int DicePipSize = 12;
        private const int ScoreGridSize = 3;
        private const double ScoredRowOpacity = 0.75;
        private const int CurrentRoundMock = 1;
        private const int TotalRoundsMock = 12;
        private const int CategoryColumnWidth = 200;
        private const int PlayerColumnWeight = 1;
        private const int LabelPaddingHorizontal = 10;
        private const int LabelPaddingVertical = 8;
        private const int FontSizeSmall = 12;
        private const int FontSizeNormal = 13;
        private const int BorderThicknessNormal = 2;
        private const int BorderThicknessSelected = 3;
        private const int RowMarginBottom = 2;
        private const int InitialScore = 0;
        private const int EmptyLength = 0;
        private const int LocalPlayerColumn = 1;
        private const int FirstOpponentColumn = 2;
        private const int PipGridIndexZero = 0;
        private const int PipGridIndexOne = 1;
        private const int PipGridIndexTwo = 2;
        private const int DiceFaceOne = 1;
        private const int DiceFaceTwo = 2;
        private const int DiceFaceThree = 3;
        private const int DiceFaceFour = 4;
        private const int DiceFaceFive = 5;
        private const string ResourceBrushBorder = "BrushBorder";
        private const string ResourceShadowSoft = "ShadowSoft";
        private const string ResourceBrushWoodDark = "BrushWoodDark";
        private const string ResourceBrushGold = "BrushGold";
        private const string ResourceBrushText = "BrushText";
        private const string ResourceBrushTextMuted = "BrushTextMuted";
        private const string ResourceBrushCream2 = "BrushCream2";
        private const string ResourceBrushFeltLight = "BrushFeltLight";
        private const string ResourceBrushWood = "BrushWood";

        private const int DefaultPipFace = 6;
        private const double FirstDieLeft = 150;
        private const double FirstDieTop = 40;
        private const double FirstDieAngle = -8;
        private const double SecondDieLeft = 230;
        private const double SecondDieTop = 90;
        private const double SecondDieAngle = 10;
        private const double ThirdDieLeft = 190;
        private const double ThirdDieTop = 150;
        private const double ThirdDieAngle = -5;
        private const double FourthDieLeft = 90;
        private const double FourthDieTop = 220;
        private const double FourthDieAngle = 12;
        private const double FifthDieLeft = 250;
        private const double FifthDieTop = 230;
        private const double FifthDieAngle = -12;

        private static readonly Dictionary<int, (int row, int column)[]> _pipLayouts =
            new Dictionary<int, (int row, int column)[]>
            {
                { DiceFaceOne, new[] { (PipGridIndexOne, PipGridIndexOne) } },
                {
                    DiceFaceTwo,
                    new[] { (PipGridIndexZero, PipGridIndexZero), (PipGridIndexTwo, PipGridIndexTwo) }
                },
                {
                    DiceFaceThree,
                    new[]
                    {
                        (PipGridIndexZero, PipGridIndexZero),
                        (PipGridIndexOne, PipGridIndexOne),
                        (PipGridIndexTwo, PipGridIndexTwo)
                    }
                },
                {
                    DiceFaceFour,
                    new[]
                    {
                        (PipGridIndexZero, PipGridIndexZero),
                        (PipGridIndexZero, PipGridIndexTwo),
                        (PipGridIndexTwo, PipGridIndexZero),
                        (PipGridIndexTwo, PipGridIndexTwo)
                    }
                },
                {
                    DiceFaceFive,
                    new[]
                    {
                        (PipGridIndexZero, PipGridIndexZero),
                        (PipGridIndexZero, PipGridIndexTwo),
                        (PipGridIndexOne, PipGridIndexOne),
                        (PipGridIndexTwo, PipGridIndexZero),
                        (PipGridIndexTwo, PipGridIndexTwo)
                    }
                },
                {
                    DefaultPipFace,
                    new[]
                    {
                        (PipGridIndexZero, PipGridIndexZero),
                        (PipGridIndexZero, PipGridIndexTwo),
                        (PipGridIndexOne, PipGridIndexZero),
                        (PipGridIndexOne, PipGridIndexTwo),
                        (PipGridIndexTwo, PipGridIndexZero),
                        (PipGridIndexTwo, PipGridIndexTwo)
                    }
                }
            };

        private readonly Random _random = new Random();
        private readonly int[] _diceValues = new int[DiceCount];
        private readonly bool[] _heldDice = new bool[DiceCount];
        private readonly Border[] _diceBorders = new Border[DiceCount];
        private readonly string _playerName;
        private readonly RoomGateway _roomGateway;
        private readonly ChatGateway _chatGateway;
        private readonly bool _isChatAvailable;

        private readonly (double left, double top, double angle)[] _dicePositions =
        {
            (FirstDieLeft, FirstDieTop, FirstDieAngle),
            (SecondDieLeft, SecondDieTop, SecondDieAngle),
            (ThirdDieLeft, ThirdDieTop, ThirdDieAngle),
            (FourthDieLeft, FourthDieTop, FourthDieAngle),
            (FifthDieLeft, FifthDieTop, FifthDieAngle)
        };

        private bool _isLeavingOnClose = true;
        private List<MatchPlayer> _matchPlayers;
        private int _rollsLeft;
        private int _totalScore;
        private TextBlock _totalScoreLabel;

        /// <summary>
        /// Initializes the game window of a room whose game has just started.
        /// </summary>
        /// <param name="roomGateway">Connection of the player to the room.</param>
        /// <param name="chatGateway">Connection of the player to the chat of the room.</param>
        /// <param name="isChatAvailable">Whether the player could join the chat of the room.</param>
        public GameWindow(RoomGateway roomGateway, ChatGateway chatGateway, bool isChatAvailable)
        {
            InitializeComponent();

            _roomGateway = roomGateway;
            _chatGateway = chatGateway;
            _isChatAvailable = isChatAvailable;
            _playerName = roomGateway.PlayerName;
            _matchPlayers = MatchPlayerFactory.Create(roomGateway.CurrentRoom.Players, _playerName);
            MatchPlayersItemsControl.ItemsSource = _matchPlayers;

            RoundInfoTextBlock.Text = string.Format(Strings.Game_Topbar_RoundInfo, CurrentRoundMock, TotalRoundsMock);
            LanguageSwitcherControl.ReopenWindowFunc = ReopenWindow;

            CreateDiceOnCanvas();
            LoadCategories();
            UpdateRollButton();

            StartChat();
            _roomGateway.RoomChanged += HandleRoomChanged;
            Closed += HandleWindowClosed;
        }

        private void GoToResultsWindow()
        {
            WindowNavigator.SwitchTo(this, new ResultsWindow(_roomGateway.Session, _totalScore));
        }

        private Window ReopenWindow()
        {
            _isLeavingOnClose = false;

            return new GameWindow(_roomGateway, _chatGateway, _isChatAvailable);
        }

        private void StartChat()
        {
            ChatPanelControl.AddMessage(
                Strings.Game_Chat_SystemName,
                Strings.Game_Chat_MatchStartedMessage,
                ChatMessageKind.System);

            if (_isChatAvailable)
            {
                ChatPanelControl.MessageSubmitted += HandleMessageSubmitted;

                foreach (ChatMessage message in _chatGateway.Subscribe(HandleMessageReceived))
                {
                    ShowChatMessage(message);
                }
            }
            else
            {
                ChatPanelControl.AddMessage(
                    Strings.Game_Chat_SystemName,
                    Strings.Game_Chat_Unavailable,
                    ChatMessageKind.System);
                ChatPanelControl.DisableSending();
            }
        }

        private void HandleRoomChanged(RoomState room, RoomEventType eventType)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowRoomPlayers(room)));
        }

        private void ShowRoomPlayers(RoomState room)
        {
            _matchPlayers = MatchPlayerFactory.UpdatePresence(_matchPlayers, room.Players);
            MatchPlayersItemsControl.ItemsSource = _matchPlayers;
        }

        private void ShowChatMessage(ChatMessage message)
        {
            ChatMessageKind kind = message.Author == _playerName ? ChatMessageKind.Own : ChatMessageKind.Other;

            ChatPanelControl.AddMessage(message.Author, message.Content, kind);
        }

        private void HandleMessageReceived(ChatMessage message)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowChatMessage(message)));
        }

        private void HandleMessageSubmitted(string content)
        {
            _ = SendChatMessageAsync(content);
        }

        private async Task SendChatMessageAsync(string content)
        {
            try
            {
                await _chatGateway.SendAsync(content);
            }
            catch (GatewayException)
            {
                ChatPanelControl.AddMessage(
                    Strings.Game_Chat_SystemName,
                    Strings.Game_Chat_SendFailed,
                    ChatMessageKind.System);
            }
        }

        private async void HandleWindowClosed(object sender, EventArgs e)
        {
            _chatGateway.Unsubscribe(HandleMessageReceived);
            _roomGateway.RoomChanged -= HandleRoomChanged;

            if (_isLeavingOnClose)
            {
                await _chatGateway.LeaveAsync();
                await _roomGateway.LeaveRoomAsync();
            }
        }

        private void CreateDiceOnCanvas()
        {
            for (int diceIndex = EmptyLength; diceIndex < DiceCount; diceIndex++)
            {
                Border die = CreateDiceFace(MinDiceValue);
                die.Cursor = Cursors.Hand;

                (double left, double top, double angle) position = _dicePositions[diceIndex];
                Canvas.SetLeft(die, position.left);
                Canvas.SetTop(die, position.top);
                die.RenderTransform = new RotateTransform(position.angle, DiceSize / 2, DiceSize / 2);

                int toggledIndex = diceIndex;
                die.MouseLeftButtonUp += (sender, e) => ToggleHoldDice(toggledIndex);

                _diceBorders[diceIndex] = die;
                _diceValues[diceIndex] = MinDiceValue;
                DiceCanvas.Children.Add(die);
            }
        }

        private Border CreateDiceFace(int value)
        {
            var border = new Border
            {
                Width = DiceSize,
                Height = DiceSize,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(DiceCornerRadius),
                BorderBrush = (SolidColorBrush)FindResource(ResourceBrushBorder),
                BorderThickness = new Thickness(BorderThicknessNormal),
                Effect = (DropShadowEffect)FindResource(ResourceShadowSoft)
            };

            var grid = new Grid { Margin = new Thickness(DiceCornerRadius) };

            for (int cellIndex = EmptyLength; cellIndex < ScoreGridSize; cellIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach ((int row, int column) in GetPipPositions(value))
            {
                var pip = new Ellipse
                {
                    Width = DicePipSize,
                    Height = DicePipSize,
                    Fill = (SolidColorBrush)FindResource(ResourceBrushWoodDark),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetRow(pip, row);
                Grid.SetColumn(pip, column);
                grid.Children.Add(pip);
            }

            border.Child = grid;
            border.Tag = value;

            return border;
        }

        private static IEnumerable<(int row, int column)> GetPipPositions(int value)
        {
            (int row, int column)[] layout;

            if (!_pipLayouts.TryGetValue(value, out layout))
            {
                layout = _pipLayouts[DefaultPipFace];
            }

            return layout;
        }

        private void UpdateDiceFace(int diceIndex)
        {
            Border newFace = CreateDiceFace(_diceValues[diceIndex]);
            Border previousFace = _diceBorders[diceIndex];

            Canvas.SetLeft(newFace, Canvas.GetLeft(previousFace));
            Canvas.SetTop(newFace, Canvas.GetTop(previousFace));
            newFace.RenderTransform = previousFace.RenderTransform;
            newFace.Cursor = Cursors.Hand;

            int diceIndexCopy = diceIndex;
            newFace.MouseLeftButtonUp += (sender, e) => ToggleHoldDice(diceIndexCopy);

            if (_heldDice[diceIndex])
            {
                newFace.BorderBrush = (SolidColorBrush)FindResource(ResourceBrushGold);
                newFace.BorderThickness = new Thickness(BorderThicknessSelected);
            }

            DiceCanvas.Children.Remove(previousFace);
            DiceCanvas.Children.Add(newFace);
            _diceBorders[diceIndex] = newFace;
        }

        private void ToggleHoldDice(int diceIndex)
        {
            if (_rollsLeft > EmptyLength)
            {
                _heldDice[diceIndex] = !_heldDice[diceIndex];
                UpdateDiceFace(diceIndex);
            }
        }

        private void RollButton_Click(object sender, RoutedEventArgs e)
        {
            if (_rollsLeft < MaxRolls)
            {
                for (int diceIndex = EmptyLength; diceIndex < DiceCount; diceIndex++)
                {
                    if (!_heldDice[diceIndex])
                    {
                        _diceValues[diceIndex] = _random.Next(MinDiceValue, MaxDiceValueExclusive);
                        UpdateDiceFace(diceIndex);
                    }
                }

                _rollsLeft++;
                UpdateRollButton();
            }
        }

        private void UpdateRollButton()
        {
            RollButton.Content = string.Format(Strings.Game_Dice_ButtonRoll, _rollsLeft);
            RollButton.IsEnabled = _rollsLeft < MaxRolls;
        }

        private void LoadCategories()
        {
            AddCategoryRow(Strings.Game_Category_Ones);
            AddCategoryRow(Strings.Game_Category_Twos);
            AddCategoryRow(Strings.Game_Category_Threes);
            AddCategoryRow(Strings.Game_Category_Fours);
            AddCategoryRow(Strings.Game_Category_Fives);
            AddCategoryRow(Strings.Game_Category_Sixes);
            AddMetaRow(Strings.Game_Category_Sum);
            AddMetaRow(Strings.Game_Category_Bonus);
            AddCategoryRow(Strings.Game_Category_ThreeOfAKind);
            AddCategoryRow(Strings.Game_Category_FourOfAKind);
            AddCategoryRow(Strings.Game_Category_FullHouse);
            AddCategoryRow(Strings.Game_Category_SmallStraight);
            AddCategoryRow(Strings.Game_Category_LargeStraight);
            AddCategoryRow(Strings.Game_Category_Yacht);
            AddCategoryRow(Strings.Game_Category_Chance);
            AddMetaRow(Strings.Game_Scorecard_Total);
        }

        private Grid CreateBaseRow(string categoryName, Brush background)
        {
            var grid = new Grid
            {
                Background = background,
                Margin = new Thickness(EmptyLength, EmptyLength, EmptyLength, RowMarginBottom)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CategoryColumnWidth) });

            for (int playerIndex = EmptyLength; playerIndex < _matchPlayers.Count; playerIndex++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(PlayerColumnWeight, GridUnitType.Star)
                });
            }

            var label = new TextBlock
            {
                Text = categoryName,
                FontSize = FontSizeSmall,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource(ResourceBrushText),
                Padding = new Thickness(
                    LabelPaddingHorizontal,
                    LabelPaddingVertical,
                    EmptyLength,
                    LabelPaddingVertical),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(label, EmptyLength);
            grid.Children.Add(label);

            return grid;
        }

        private void AddCategoryRow(string categoryName)
        {
            Grid row = CreateBaseRow(categoryName, (SolidColorBrush)FindResource(ResourceBrushCream2));
            var felt = (SolidColorBrush)FindResource(ResourceBrushFeltLight);
            var muted = (SolidColorBrush)FindResource(ResourceBrushTextMuted);
            TextBlock myScoreLabel = CreateScoreLabel(string.Empty, felt, FontWeights.Bold);

            AddScoreCell(row, myScoreLabel, LocalPlayerColumn);

            for (int column = FirstOpponentColumn; column <= _matchPlayers.Count; column++)
            {
                AddScoreCell(row, CreateScoreLabel(string.Empty, muted, FontWeights.Normal), column);
            }

            row.Cursor = Cursors.Hand;
            row.MouseLeftButtonUp += (sender, e) => ScoreCategory(row, myScoreLabel);

            CategoriesPanel.Children.Add(row);
        }

        private void AddMetaRow(string text)
        {
            Grid row = CreateBaseRow(text, (SolidColorBrush)FindResource(ResourceBrushWood));
            string initialScoreText = InitialScore.ToString();
            TextBlock myValueLabel = CreateScoreLabel(initialScoreText, Brushes.White, FontWeights.Bold);

            ((TextBlock)row.Children[EmptyLength]).Foreground = Brushes.White;
            AddScoreCell(row, myValueLabel, LocalPlayerColumn);

            for (int column = FirstOpponentColumn; column <= _matchPlayers.Count; column++)
            {
                AddScoreCell(row, CreateScoreLabel(initialScoreText, Brushes.White, FontWeights.Bold), column);
            }

            if (text == Strings.Game_Scorecard_Total)
            {
                _totalScoreLabel = myValueLabel;
            }

            CategoriesPanel.Children.Add(row);
        }

        private static TextBlock CreateScoreLabel(string text, Brush foreground, FontWeight fontWeight)
        {
            var label = new TextBlock
            {
                Text = text,
                FontSize = FontSizeNormal,
                FontWeight = fontWeight,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            return label;
        }

        private static void AddScoreCell(Grid row, TextBlock label, int column)
        {
            Grid.SetColumn(label, column);
            row.Children.Add(label);
        }

        private void ScoreCategory(Grid row, TextBlock myScoreLabel)
        {
            if (_rollsLeft > EmptyLength && myScoreLabel.Text == string.Empty)
            {
                int scoreObtained = EmptyLength;

                foreach (int value in _diceValues)
                {
                    scoreObtained += value;
                }

                _totalScore += scoreObtained;
                myScoreLabel.Text = scoreObtained.ToString();
                row.Opacity = ScoredRowOpacity;

                if (_totalScoreLabel != null)
                {
                    _totalScoreLabel.Text = _totalScore.ToString();
                }

                _rollsLeft = EmptyLength;

                for (int diceIndex = EmptyLength; diceIndex < DiceCount; diceIndex++)
                {
                    _heldDice[diceIndex] = false;
                    UpdateDiceFace(diceIndex);
                }

                UpdateRollButton();
            }
            else if (_rollsLeft == EmptyLength)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Game_Dice_HintPress);
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
                WindowNavigator.SwitchTo(this, new MenuWindow(_roomGateway.Session));
            }
        }

        private void FinishDemoLink_Click(object sender, MouseButtonEventArgs e)
        {
            GoToResultsWindow();
        }
    }
}