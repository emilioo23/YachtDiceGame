using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana principal del juego donde se gestionan los turnos, lanzamientos de dados y puntuaciones.
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
        private const int LabelPaddingHorizontal = 10;
        private const int LabelPaddingVertical = 8;

        private readonly Random _random = new Random();
        private readonly int[] _diceValues = new int[DiceCount];
        private readonly bool[] _heldDice = new bool[DiceCount];
        private readonly Border[] _diceBorders = new Border[DiceCount];
        private readonly string _playerName;
        private int _rollsLeft;
        private int _totalScore;
        private TextBlock _totalScoreLabel;

        // Posiciones y rotaciones fijas para que los dados se vean "arrojados"
        // dentro del tazon, en vez de alineados en fila.
        private readonly (double left, double top, double angle)[] _dicePositions =
        {
            (150, 40,  -8),
            (230, 90,  10),
            (190, 150, -5),
            (90,  220, 12),
            (250, 230, -12),
        };

        /// <summary>
        /// Inicializa la ventana de juego para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que inicia la partida.</param>
        public GameWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            PlayerNameTextBlock.Text = playerName;
            PlayerInitialTextBlock.Text = playerName.Length > 0 ? playerName.Substring(0, 1).ToUpper() : "U";
            RoundInfoTextBlock.Text = string.Format(Strings.Game_RoundInfo, CurrentRoundMock, TotalRoundsMock);
            LanguageSwitcherControl.ReopenWindowFunc = () => new GameWindow(playerName);

            CreateDiceOnCanvas();
            LoadCategories();
            UpdateRollButton();
        }

        /// <summary>
        /// Navega a la pantalla de Resultados con el puntaje acumulado en esta partida.
        /// </summary>
        public void GoToResultsWindow()
        {
            var resultsWindow = new ResultsWindow(_playerName, _totalScore);
            resultsWindow.Show();
            this.Close();
        }

        private void CreateDiceOnCanvas()
        {
            for (int i = 0; i < DiceCount; i++)
            {
                var die = CreateDiceFace(MinDiceValue);
                die.Cursor = System.Windows.Input.Cursors.Hand;

                var position = _dicePositions[i];
                Canvas.SetLeft(die, position.left);
                Canvas.SetTop(die, position.top);
                die.RenderTransform = new RotateTransform(position.angle, DiceSize / 2, DiceSize / 2);

                int diceIndex = i; // captura local para el evento
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
                BorderBrush = (SolidColorBrush)FindResource("BrushBorder"),
                BorderThickness = new Thickness(2),
                Effect = (System.Windows.Media.Effects.DropShadowEffect)FindResource("ShadowSoft")
            };

            var grid = new Grid { Margin = new Thickness(DiceCornerRadius) };
            for (int i = 0; i < ScoreGridSize; i++)
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
                    Fill = (SolidColorBrush)FindResource("BrushWoodDark"),
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
            switch (value)
            {
                case 1: return new List<(int, int)> { (1, 1) };
                case 2: return new List<(int, int)> { (0, 0), (2, 2) };
                case 3: return new List<(int, int)> { (0, 0), (1, 1), (2, 2) };
                case 4: return new List<(int, int)> { (0, 0), (0, 2), (2, 0), (2, 2) };
                case 5: return new List<(int, int)> { (0, 0), (0, 2), (1, 1), (2, 0), (2, 2) };
                default: return new List<(int, int)> { (0, 0), (0, 2), (1, 0), (1, 2), (2, 0), (2, 2) };
            }
        }

        private void UpdateDiceFace(int diceIndex)
        {
            var newFace = CreateDiceFace(_diceValues[diceIndex]);
            var previousFace = _diceBorders[diceIndex];

            Canvas.SetLeft(newFace, Canvas.GetLeft(previousFace));
            Canvas.SetTop(newFace, Canvas.GetTop(previousFace));
            newFace.RenderTransform = previousFace.RenderTransform;
            newFace.Cursor = System.Windows.Input.Cursors.Hand;

            int diceIndexCopy = diceIndex;
            newFace.MouseLeftButtonUp += (s, e) => ToggleHoldDice(diceIndexCopy);

            if (_heldDice[diceIndex])
            {
                newFace.BorderBrush = (SolidColorBrush)FindResource("BrushGold");
                newFace.BorderThickness = new Thickness(3);
            }

            DiceCanvas.Children.Remove(previousFace);
            DiceCanvas.Children.Add(newFace);
            _diceBorders[diceIndex] = newFace;
        }

        private void ToggleHoldDice(int diceIndex)
        {
            if (_rollsLeft == 0)
            {
                return;
            }

            _heldDice[diceIndex] = !_heldDice[diceIndex];
            UpdateDiceFace(diceIndex);
        }

        private void RollButton_Click(object sender, RoutedEventArgs e)
        {
            if (_rollsLeft >= MaxRolls)
            {
                return;
            }

            for (int i = 0; i < DiceCount; i++)
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

        private void UpdateRollButton()
        {
            RollButton.Content = string.Format(Strings.Game_RollButton, _rollsLeft);
            RollButton.IsEnabled = _rollsLeft < MaxRolls;
        }

        private void LoadCategories()
        {
            AddCategoryRow(Strings.Game_CategoryOnes);
            AddCategoryRow(Strings.Game_CategoryTwos);
            AddCategoryRow(Strings.Game_CategoryThrees);
            AddCategoryRow(Strings.Game_CategoryFours);
            AddCategoryRow(Strings.Game_CategoryFives);
            AddCategoryRow(Strings.Game_CategorySixes);
            AddMetaRow(Strings.Game_CategorySum);
            AddMetaRow(Strings.Game_CategoryBonus);
            AddCategoryRow(Strings.Game_CategoryThreeOfAKind);
            AddCategoryRow(Strings.Game_CategoryFourOfAKind);
            AddCategoryRow(Strings.Game_CategoryFullHouse);
            AddCategoryRow(Strings.Game_CategorySmallStraight);
            AddCategoryRow(Strings.Game_CategoryLargeStraight);
            AddCategoryRow(Strings.Game_CategoryYacht);
            AddCategoryRow(Strings.Game_CategoryChance);
            AddMetaRow(Strings.Game_TotalLabel);
        }

        private Grid CreateBaseRow(string categoryName, Brush background)
        {
            var grid = new Grid { Background = background, Margin = new Thickness(0, 0, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CategoryColumnWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = categoryName,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("BrushText"),
                Padding = new Thickness(LabelPaddingHorizontal, LabelPaddingVertical, 0, LabelPaddingVertical),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            return grid;
        }

        private void AddCategoryRow(string categoryName)
        {
            var row = CreateBaseRow(categoryName, (SolidColorBrush)FindResource("BrushCream2"));

            var myScoreLabel = new TextBlock
            {
                Text = "",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("BrushFeltLight"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(myScoreLabel, 1);
            row.Children.Add(myScoreLabel);

            var opponentScoreLabel = new TextBlock
            {
                Text = "",
                FontSize = 13,
                Foreground = (SolidColorBrush)FindResource("BrushTextMuted"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(opponentScoreLabel, 2);
            row.Children.Add(opponentScoreLabel);

            row.Cursor = System.Windows.Input.Cursors.Hand;
            row.MouseLeftButtonUp += (s, e) => ScoreCategory(row, myScoreLabel);

            CategoriesPanel.Children.Add(row);
        }

        private void AddMetaRow(string text)
        {
            var row = CreateBaseRow(text, (SolidColorBrush)FindResource("BrushWood"));
            ((TextBlock)row.Children[0]).Foreground = Brushes.White;

            var myValueLabel = new TextBlock { Text = "0", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var opponentValueLabel = new TextBlock { Text = "0", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(myValueLabel, 1);
            Grid.SetColumn(opponentValueLabel, 2);
            row.Children.Add(myValueLabel);
            row.Children.Add(opponentValueLabel);

            if (text == Strings.Game_TotalLabel)
            {
                _totalScoreLabel = myValueLabel;
            }

            CategoriesPanel.Children.Add(row);
        }

        private void ScoreCategory(Grid row, TextBlock myScoreLabel)
        {
            if (_rollsLeft == 0)
            {
                MessageBox.Show("Primero tira los dados.");
                return;
            }
            if (myScoreLabel.Text != "")
            {
                return;
            }

            // Calculo simplificado: suma de los 5 dados.
            // La logica real de puntaje por categoria (Yacht, Full House, etc.)
            // se implementara cuando se programe el juego completo.
            int scoreObtained = 0;
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

            _rollsLeft = 0;
            for (int i = 0; i < DiceCount; i++)
            {
                _heldDice[i] = false;
                UpdateDiceFace(i);
            }
            UpdateRollButton();
        }

        private void LeaveMatchButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tu progreso no se guardara. Deseas salir?", "Salir de la partida",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var menuWindow = new MenuWindow(_playerName);
                menuWindow.Show();
                this.Close();
            }
        }

        private void FinishDemoLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GoToResultsWindow();
        }
    }
}