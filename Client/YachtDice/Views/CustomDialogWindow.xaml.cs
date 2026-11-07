using System.Windows;
using System.Windows.Input;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Dialog window that shows a message to the player with one or two buttons.
    /// </summary>
    public partial class CustomDialogWindow : Window
    {
        /// <summary>
        /// Initializes the dialog with the given texts.
        /// </summary>
        /// <param name="dialogContent">Texts that the dialog shows.</param>
        public CustomDialogWindow(DialogContent dialogContent)
        {
            InitializeComponent();

            TitleText.Text = string.IsNullOrEmpty(dialogContent.Title) ? Strings.Auth_Logo : dialogContent.Title;
            MessageText.Text = dialogContent.Message;
            ShowButtonTexts(dialogContent);
        }

        /// <summary>
        /// Shows a message over a window and waits until the player closes it.
        /// </summary>
        /// <param name="owner">Window over which the dialog is shown.</param>
        /// <param name="message">Text of the message.</param>
        public static void ShowMessage(Window owner, string message)
        {
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = owner
            };

            dialog.ShowDialog();
        }

        private void ShowButtonTexts(DialogContent dialogContent)
        {
            if (!string.IsNullOrEmpty(dialogContent.PrimaryButtonText))
            {
                PrimaryButton.Content = dialogContent.PrimaryButtonText;
            }

            if (!string.IsNullOrEmpty(dialogContent.SecondaryButtonText))
            {
                SecondaryButton.Content = dialogContent.SecondaryButtonText;
                SecondaryButton.Visibility = Visibility.Visible;
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
