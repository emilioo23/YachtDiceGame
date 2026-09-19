using System;
using System.Windows;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana que muestra el marcador final y ganador al terminar una partida.
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private const int MinOpponentScoreGap = 10;
        private const int MaxOpponentScoreGapExclusive = 60;

        private readonly string _playerName;

        /// <summary>
        /// Inicializa la ventana de resultados con el jugador y el puntaje obtenido.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que jugo la partida.</param>
        /// <param name="playerScore">Puntaje final obtenido por el jugador.</param>
        public ResultsWindow(string playerName, int playerScore)
        {
            InitializeComponent();
            _playerName = playerName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResultsWindow(playerName, playerScore);

            WinnerTitleTextBlock.Text = string.Format(Strings.Results_WinnerTitle, playerName);
            ScoreSubtitleTextBlock.Text = string.Format(Strings.Results_ScoreSubtitle, playerScore);

            // TODO: Datos del rival simulado, mas adelante vendran del backend.
            var random = new Random();
            int scoreGap = random.Next(MinOpponentScoreGap, MaxOpponentScoreGapExclusive);
            OpponentScoreTextBlock.Text = Math.Max(0, playerScore - scoreGap).ToString();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new GameWindow(_playerName);
            gameWindow.Show();
            this.Close();
        }
    }
}