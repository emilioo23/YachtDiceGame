using System.Windows;
using System.Windows.Controls;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Field for a password with an eye button. By default the characters are hidden; the button shows them so
    /// the player can see what is being typed, and a second click hides them again.
    /// </summary>
    public partial class PasswordRevealBox : UserControl
    {
        private bool _isRevealed;

        /// <summary>
        /// Initializes the field with the password hidden.
        /// </summary>
        public PasswordRevealBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the password that the player typed, whether the characters are hidden or shown.
        /// </summary>
        public string Password
        {
            get { return _isRevealed ? VisiblePasswordBox.Text : HiddenPasswordBox.Password; }
        }

        private static void FocusAtEnd(TextBox textBox)
        {
            textBox.Focus();
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void Reveal()
        {
            _isRevealed = true;
            VisiblePasswordBox.Text = HiddenPasswordBox.Password;
            HiddenPasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;
            SlashLine.Visibility = Visibility.Visible;
            RevealButton.ToolTip = Strings.Auth_HidePassword;
            FocusAtEnd(VisiblePasswordBox);
        }

        private void Hide()
        {
            _isRevealed = false;
            HiddenPasswordBox.Password = VisiblePasswordBox.Text;
            VisiblePasswordBox.Clear();
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            HiddenPasswordBox.Visibility = Visibility.Visible;
            SlashLine.Visibility = Visibility.Collapsed;
            RevealButton.ToolTip = Strings.Auth_ShowPassword;
            HiddenPasswordBox.Focus();
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRevealed)
            {
                Hide();
            }
            else
            {
                Reveal();
            }
        }
    }
}
