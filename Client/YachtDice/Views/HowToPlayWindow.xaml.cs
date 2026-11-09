using System.Windows;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that explains the rules of the game: how a turn works and how each category scores, with
    /// examples drawn with dice.
    /// </summary>
    public partial class HowToPlayWindow : Window
    {
        private readonly PlayerSession _session;
        private readonly bool _isDialog;

        /// <summary>
        /// Initializes the window with the rules in the language that is active.
        /// </summary>
        /// <param name="session">Session of the player, to go back to the menu.</param>
        public HowToPlayWindow(PlayerSession session)
            : this()
        {
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new HowToPlayWindow(session);
        }

        /// <summary>
        /// Initializes the window to be shown over another window, for example during a match. The other
        /// window stays open and the player returns to it with the back button.
        /// </summary>
        /// <param name="owner">Window that stays open behind the rules.</param>
        public HowToPlayWindow(Window owner)
            : this()
        {
            Owner = owner;
            _isDialog = true;
            LanguageSwitcherControl.Visibility = Visibility.Collapsed;
        }

        private HowToPlayWindow()
        {
            InitializeComponent();

            StepsItemsControl.ItemsSource = HowToPlayContent.GetTurnSteps();
            UpperRulesItemsControl.ItemsSource = HowToPlayContent.GetUpperRules();
            LowerRulesItemsControl.ItemsSource = HowToPlayContent.GetLowerRules();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDialog)
            {
                Close();
            }
            else
            {
                WindowNavigator.SwitchTo(this, new MenuWindow(_session));
            }
        }
    }
}
