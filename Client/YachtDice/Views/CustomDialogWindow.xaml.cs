using System.Windows;
using System.Windows.Input;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Dialog window that shows a message to the player with one or two buttons.
    /// </summary>
    public partial class CustomDialogWindow : Window
    {
        private const string DefaultTitle = "Yacht Dice";

        /// <summary>
        /// Initializes the dialog with the given texts.
        /// </summary>
        /// <param name="dialogContent">Texts that the dialog shows.</param>
        public CustomDialogWindow(DialogContent dialogContent)
        {
            InitializeComponent();

            TitleText.Text = string.IsNullOrEmpty(dialogContent.Title) ? DefaultTitle : dialogContent.Title;
            MessageText.Text = dialogContent.Message;
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
