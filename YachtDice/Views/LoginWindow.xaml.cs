using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.Resources;
using YachtDice.Services;
using YachtDice.Utils;
using YachtDiceGame.Data;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana inicial para la autenticación de usuarios y acceso al sistema.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string GuestDisplayName = "Invitado";
        private const string ActivePlayerState = "Activo";
        private const string FriendCodePrefix = "FRD-";
        private const int FriendCodeLength = 6;

        private static readonly TwoFactorCodeService _codeService = new TwoFactorCodeService();

        private readonly TwoFactorDeliveryService _deliveryService;

        /// <summary>
        /// Inicializa la ventana de inicio de sesión y registro.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            _deliveryService = new TwoFactorDeliveryService(_codeService);
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginTabButton.Style = (Style)FindResource("TabButtonActive");
            RegisterTabButton.Style = (Style)FindResource("TabButtonInactive");
            AuthSubmitButton.Content = Strings.Auth_LoginButton;
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterTabButton.Style = (Style)FindResource("TabButtonActive");
            LoginTabButton.Style = (Style)FindResource("TabButtonInactive");
            AuthSubmitButton.Content = Strings.Auth_CreateAccountButton;
        }

        private async void AuthSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            bool isLogin = LoginPanel.Visibility == Visibility.Visible;

            if (isLogin)
            {
                await ProcessLoginAsync();
            }
            else
            {
                await ProcessRegisterAsync();
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(GuestDisplayName);
            menuWindow.Show();
            this.Close();
        }

        private void ForgotPasswordLink_Click(object sender, MouseButtonEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            this.Close();
        }

        private async Task ProcessLoginAsync()
        {
            string email = EmailTextBox.Text.Trim();
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Auth_ErrorRequiredFields }).ShowDialog();
                return;
            }

            List<Player> matches = await FindPlayersByEmailAsync(email);

            if (matches.Count == 0)
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D13_Login }).ShowDialog();
                return;
            }

            Player player = matches[0];

            if (!PasswordHasher.Verify(password, player.PwdHash))
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D13_Login }).ShowDialog();
                return;
            }

            if (_codeService.IsLocked(player.Email))
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D19_Validation2FA }).ShowDialog();
                return;
            }

            await SendCodeAndOpenVerificationAsync(player);
        }

        private async Task<List<Player>> FindPlayersByEmailAsync(string email)
        {
            using (var context = new YachtDiceContext())
            {
                return await context.Player.Where(p => p.Email == email).Take(1).ToListAsync();
            }
        }

        private async Task SendCodeAndOpenVerificationAsync(Player player)
        {
            AuthSubmitButton.IsEnabled = false;

            bool wasSent = await _deliveryService.SendCodeAsync(player.Email, player.DisplayName);

            if (wasSent)
            {
                var twoFactorWindow = new TwoFactorWindow(player.Email, player.DisplayName, _codeService);
                twoFactorWindow.Show();
                this.Close();
            }
            else
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_ErrorSendFailed }).ShowDialog();
                AuthSubmitButton.IsEnabled = true;
            }
        }

        private async Task ProcessRegisterAsync()
        {
            if (!HasRequiredRegisterFields())
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Auth_ErrorRequiredFields }).ShowDialog();
                return;
            }

            Player newPlayer = CreatePlayerFromForm();
            string conflictMessage = await SaveNewPlayerAsync(newPlayer);

            if (conflictMessage.Length == 0)
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Auth_SuccessRegister }).ShowDialog();
                LoginTabButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                new CustomDialogWindow(new DialogContentDto { Message = conflictMessage }).ShowDialog();
            }
        }

        private bool HasRequiredRegisterFields()
        {
            return !string.IsNullOrWhiteSpace(FirstNameTextBox.Text)
                && !string.IsNullOrWhiteSpace(LastNameTextBox.Text)
                && !string.IsNullOrWhiteSpace(UsernameTextBox.Text)
                && !string.IsNullOrWhiteSpace(RegisterEmailTextBox.Text)
                && !string.IsNullOrWhiteSpace(RegisterPasswordBox.Password);
        }

        private Player CreatePlayerFromForm()
        {
            string username = UsernameTextBox.Text.Trim();

            return new Player
            {
                AvatarId = 1,
                Username = username,
                Email = RegisterEmailTextBox.Text.Trim(),
                PwdHash = PasswordHasher.ComputeHash(RegisterPasswordBox.Password),
                State = ActivePlayerState,
                RegDate = DateTime.Now,
                XP = 0,
                Level = 1,
                DisplayName = username,
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                FriendCode = CreateFriendCode()
            };
        }

        private async Task<string> SaveNewPlayerAsync(Player newPlayer)
        {
            string conflictMessage = string.Empty;

            using (var context = new YachtDiceContext())
            {
                if (await context.Player.AnyAsync(p => p.Email == newPlayer.Email))
                {
                    conflictMessage = Strings.Dialogs_D16_Register;
                }
                else if (await context.Player.AnyAsync(p => p.Username == newPlayer.Username))
                {
                    conflictMessage = Strings.Dialogs_D15_Register;
                }
                else
                {
                    context.Player.Add(newPlayer);
                    await context.SaveChangesAsync();
                }
            }

            return conflictMessage;
        }

        private static string CreateFriendCode()
        {
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, FriendCodeLength).ToUpper();

            return FriendCodePrefix + randomPart;
        }
    }
}