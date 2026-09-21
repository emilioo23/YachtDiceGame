using System.Windows;

namespace YachtDice.Views
{
    public partial class CustomDialogWindow : Window
    {
        public CustomDialogWindow(string title, string message, string primaryButtonText, string secondaryButtonText = null)
        {
            InitializeComponent();

            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            PrimaryButton.Content = primaryButtonText;

            if (string.IsNullOrWhiteSpace(secondaryButtonText))
            {
                SecondaryButton.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(PrimaryButton, 3);
            }
            else
            {
                SecondaryButton.Content = secondaryButtonText;
            }
        }

        private void PrimaryButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = true;
            Close();
        }

        private void SecondaryButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = false;
            Close();
        }
    }
}