using System.Linq;
using System.Windows;
using YachtDice.Views;

namespace YachtDice.Utils
{
    public static class DialogService
    {
        private static Window GetActiveWindow()
        {
            return Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) 
                   ?? Application.Current.MainWindow;
        }

        public static void ShowMessage(string title, string message, string okButtonText)
        {
            var dialog = new CustomDialogWindow(title, message, okButtonText)
            {
                Owner = GetActiveWindow()
            };
            
            dialog.ShowDialog();
        }

        public static bool ShowConfirmation(string title, string message, string confirmButtonText, string cancelButtonText)
        {
            var dialog = new CustomDialogWindow(title, message, confirmButtonText, cancelButtonText)
            {
                Owner = GetActiveWindow()
            };
            
            return dialog.ShowDialog() == true;
        }
    }
}