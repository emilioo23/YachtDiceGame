using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
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
        private const int LabelPaddingHorizontal = 10;
        private const int LabelPaddingVertical = 8;
        private const int FontSizeSmall = 12;
        private const int FontSizeNormal = 13;
        private const int BorderThicknessNormal = 2;
        private const int BorderThicknessSelected = 3;
        private const int RowMarginBottom = 2;
        private const int ChatMessageMarginBottom = 4;
        private const string DefaultZeroText = "0";
        private const int EmptyLength = 0;
        private const int FirstCharacterIndex = 0;
        private const int SingleCharacterLength = 1;
        private const string DefaultPlayerInitial = "U";
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
        private const string ResourceBrushDanger = "BrushDanger";
        private const string ChatSenderSeparator = ": ";

        private readonly Random _random = new Random();
        private readonly int[] _diceValues = new int[DiceCount];
        private readonly bool[] _heldDice = new bool[DiceCount];
        private readonly Border[] _diceBorders = new Border[DiceCount];
        private readonly string _playerName;

        private readonly (double left, double top, double angle)[] _dicePositions =
        {
            (150, 40,  -8),
            (230, 90,  10),
            (190, 150, -5),
            (90,  220, 12),
            (250, 230, -12),
        };

        private int _rollsLeft;
        private int _totalScore;
        private TextBlock _totalScoreLabel;

        public GameWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            PlayerNameTextBlock.Text = playerName;

            PlayerInitialTextBlock.Text = playerName.Length > EmptyLength
                ? playerName.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper()
                : DefaultPlayerInitial;

            RoundInfoTextBlock.Text = string.Format(Strings.Game_Topbar_RoundInfo, CurrentRoundMock, TotalRoundsMock);
            LanguageSwitcherControl.ReopenWindowFunc = () => new GameWindow(playerName);

            CreateDiceOnCanvas();
            LoadCategories();
            UpdateRollButton();

            AppendMessageToChat(Strings.Game_Chat_SystemName, Strings.Game_Chat_MatchStartedMessage, true);
        }

        public void GoToResultsWindow()
        {
            var resultsWindow = new ResultsWindow(_playerName, _totalScore);
            resultsWindow.Show();
            this.Close();
        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {
            SendChatMessage();
        }

        private void ChatInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendChatMessage();
                e.Handled = true;
            }
        }

        private void SendChatMessage()
        {
            var messageText = ChatInputTextBox.Text.Trim();

            if (messageText.Length > EmptyLength)
            {
                ChatInputTextBox.Clear();
                AppendMessageToChat(_playerName, messageText, false);
            }
        }

        private void AppendMessageToChat(string sender, string message, bool isSystem)
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(EmptyLength, EmptyLength, EmptyLength, ChatMessageMarginBottom),
                TextWrapping = TextWrapping.Wrap
            };

            var textColor = isSystem
                ? (SolidColorBrush)FindResource(ResourceBrushDanger)
                : (SolidColorBrush)FindResource(ResourceBrushText);

            var senderRun = new Run(sender + ChatSenderSeparator)
            {
                FontWeight = FontWeights.Bold,
                Foreground = textColor
            };

            var messageRun = new Run(message)
            {
                Foreground = textColor
            };

            textBlock.Inlines.Add(senderRun);
            textBlock.Inlines.Add(messageRun);

            ChatMessagesPanel.Children.Add(textBlock);
            ChatScrollViewer.ScrollToEnd();
        }

        private void CreateDiceOnCanvas()
        {
            for (int i = EmptyLength; i < DiceCount; i++)
            {
                var die = CreateDiceFace(MinDiceValue);
                die.Cursor = Cursors.Hand;

                var position = _dicePositions[i];
                Canvas.SetLeft(die, position.left);
                Canvas.SetTop(die, position.top);
                die.RenderTransform = new RotateTransform(position.angle, DiceSize / 2, DiceSize / 2);

                int diceIndex = i;
                die.MouseLeftButtonUp += (s, e) => ToggleHoldDice(diceIndex);

                _diceBorders[i] = die;
                _diceValues[i] = MinDiceValue;
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

            for (int i = EmptyLength; i < ScoreGridSize; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach (var (row, column) in GetPipPositions(value))
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

        private List<(int row, int column)> GetPipPositions(int value)
        {
            var positions = new List<(int row, int column)>();

            switch (value)
            {
                case DiceFaceOne:
                    positions.Add((PipGridIndexOne, PipGridIndexOne));
                    break;
                case DiceFaceTwo:
                    positions.Add((PipGridIndexZero, PipGridIndexZero));
                    positions.Add((PipGridIndexTwo, PipGridIndexTwo));
                    break;
                case DiceFaceThree:
                    positions.Add((PipGridIndexZero, PipGridIndexZero));
                    positions.Add((PipGridIndexOne, PipGridIndexOne));
                    positions.Add((PipGridIndexTwo, PipGridIndexTwo));
                    break;
                case DiceFaceFour:
                    positions.Add((PipGridIndexZero, PipGridIndexZero));
                    positions.Add((PipGridIndexZero, PipGridIndexTwo));
                    positions.Add((PipGridIndexTwo, PipGridIndexZero));
                    positions.Add((PipGridIndexTwo, PipGridIndexTwo));
                    break;
                case DiceFaceFive:
                    positions.Add((PipGridIndexZero, PipGridIndexZero));
                    positions.Add((PipGridIndexZero, PipGridIndexTwo));
                    positions.Add((PipGridIndexOne, PipGridIndexOne));
                    positions.Add((PipGridIndexTwo, PipGridIndexZero));
                    positions.Add((PipGridIndexTwo, PipGridIndexTwo));
                    break;
                default:
                    positions.Add((PipGridIndexZero, PipGridIndexZero));
                    positions.Add((PipGridIndexZero, PipGridIndexTwo));
                    positions.Add((PipGridIndexOne, PipGridIndexZero));
                    positions.Add((PipGridIndexOne, PipGridIndexTwo));
                    positions.Add((PipGridIndexTwo, PipGridIndexZero));
                    positions.Add((PipGridIndexTwo, PipGridIndexTwo));
                    break;
            }

            return positions;
        }

        private void UpdateDiceFace(int diceIndex)
        {
            var newFace = CreateDiceFace(_diceValues[diceIndex]);
            var previousFace = _diceBorders[diceIndex];

            Canvas.SetLeft(newFace, Canvas.GetLeft(previousFace));
            Canvas.SetTop(newFace, Canvas.GetTop(previousFace));
            newFace.RenderTransform = previousFace.RenderTransform;
            newFace.Cursor = Cursors.Hand;

            int diceIndexCopy = diceIndex;
            newFace.MouseLeftButtonUp += (s, e) => ToggleHoldDice(diceIndexCopy);

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
                for (int i = EmptyLength; i < DiceCount; i++)
                {
                    if (!_heldDice[i])
                    {
                        _diceValues[i] = _random.Next(MinDiceValue, MaxDiceValueExclusive);
                        UpdateDiceFace(i);
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
            AddCategoryRow(Strings.Game_CategoryOnes);
            AddCategoryRow(Strings.Game_Category_Twos);
            AddCategoryRow(Strings.Game_Category_Threes);
            AddCategoryRow(Strings.Game_CategoryFours);
            AddCategoryRow(Strings.Game_CategoryFives);
            AddCategoryRow(Strings.Game_CategorySixes);
            AddMetaRow(Strings.Game_CategorySum);
            AddMetaRow(Strings.Game_Category_Bonus);
            AddCategoryRow(Strings.Game_Category_ThreesOfAKind);
            AddCategoryRow(Strings.Game_Category_FourOfAKind);
            AddCategoryRow(Strings.Game_CategoryFullHouse);
            AddCategoryRow(Strings.Game_Category_SmallStraight);
            AddCategoryRow(Strings.Game_Category_LargeStraight);
            AddCategoryRow(Strings.Game_CategoryYacht);
            AddCategoryRow(Strings.Game_CategoryChance);
            AddMetaRow(Strings.Game_Scorecard_Total);
        }

        private Grid CreateBaseRow(string categoryName, Brush background)
        {
            var grid = new Grid { Background = background, Margin = new Thickness(EmptyLength, EmptyLength, EmptyLength, RowMarginBottom) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CategoryColumnWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = categoryName,
                FontSize = FontSizeSmall,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource(ResourceBrushText),
                Padding = new Thickness(LabelPaddingHorizontal, LabelPaddingVertical, EmptyLength, LabelPaddingVertical),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(label, EmptyLength);
            grid.Children.Add(label);

            return grid;
        }

        private void AddCategoryRow(string categoryName)
        {
            var row = CreateBaseRow(categoryName, (SolidColorBrush)FindResource(ResourceBrushCream2));

            var myScoreLabel = new TextBlock
            {
                Text = string.Empty,
                FontSize = FontSizeNormal,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource(ResourceBrushFeltLight),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(myScoreLabel, 1);
            row.Children.Add(myScoreLabel);

            var opponentScoreLabel = new TextBlock
            {
                Text = string.Empty,
                FontSize = FontSizeNormal,
                Foreground = (SolidColorBrush)FindResource(ResourceBrushTextMuted),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(opponentScoreLabel, 2);
            row.Children.Add(opponentScoreLabel);

            row.Cursor = Cursors.Hand;
            row.MouseLeftButtonUp += (s, e) => ScoreCategory(row, myScoreLabel);

            CategoriesPanel.Children.Add(row);
        }

        private void AddMetaRow(string text)
        {
            var row = CreateBaseRow(text, (SolidColorBrush)FindResource(ResourceBrushWood));
            ((TextBlock)row.Children[EmptyLength]).Foreground = Brushes.White;

            var myValueLabel = new TextBlock
            {
                Text = DefaultZeroText,
                FontSize = FontSizeNormal,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var opponentValueLabel = new TextBlock
            {
                Text = DefaultZeroText,
                FontSize = FontSizeNormal,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(myValueLabel, 1);
            Grid.SetColumn(opponentValueLabel, 2);
            row.Children.Add(myValueLabel);
            row.Children.Add(opponentValueLabel);

            if (text == Strings.Game_Scorecard_Total)
            {
                _totalScoreLabel = myValueLabel;
            }

            CategoriesPanel.Children.Add(row);
        }

        private void ScoreCategory(Grid row, TextBlock myScoreLabel)
        {
            if (_rollsLeft > EmptyLength && myScoreLabel.Text == string.Empty)
            {
                int scoreObtained = EmptyLength;

                foreach (var value in _diceValues)
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

                for (int i = EmptyLength; i < DiceCount; i++)
                {
                    _heldDice[i] = false;
                    UpdateDiceFace(i);
                }

                UpdateRollButton();
            }
            else if (_rollsLeft == EmptyLength)
            {
                var dialogContent = new DialogContent
                {
                    Message = Strings.Game_Dice_HintPress
                };

                new CustomDialogWindow(dialogContent).ShowDialog();
            }
        }

        private void LeaveMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogContent = new DialogContent
            {
                Title = Strings.Game_Sidebar_ButtonLeave,
                Message = Strings.Dialogs_D06_LeaveMatch,
                PrimaryButtonText = Strings.Dialogs_OKButton,
                SecondaryButtonText = Strings.Dialogs_IgnoreButton
            };

            var result = new CustomDialogWindow(dialogContent).ShowDialog();

            if (result == true)
            {
                var menuWindow = new MenuWindow(_playerName);
                menuWindow.Show();
                this.Close();
            }
        }

        private void FinishDemoLink_Click(object sender, MouseButtonEventArgs e)
        {
            GoToResultsWindow();
        }
    }
}