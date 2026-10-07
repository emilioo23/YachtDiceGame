using System.Windows;
using System.Windows.Input;
using YachtDice.Utils;

namespace YachtDice.Views
{
    public partial class CustomDialogWindow : Window
    {
        public CustomDialogWindow(DialogContentDto dialogContent)
        {
            InitializeComponent();
            
            TitleText.Text = string.IsNullOrEmpty(dialogContent.Title) ? "Yacht Dice" : dialogContent.Title;
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
