using System.Windows;
using System.Windows.Input;
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

        /// <summary>
        /// Initializes the window with the rules in the language that is active.
        /// </summary>
        /// <param name="session">Session of the player, to go back to the menu.</param>
        public HowToPlayWindow(PlayerSession session)
        {
            InitializeComponent();
            _session = session;
            LanguageSwitcherControl.ReopenWindowFunc = () => new HowToPlayWindow(session);

            StepsItemsControl.ItemsSource = HowToPlayContent.GetTurnSteps();
            UpperRulesItemsControl.ItemsSource = HowToPlayContent.GetUpperRules();
            LowerRulesItemsControl.ItemsSource = HowToPlayContent.GetLowerRules();
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}
