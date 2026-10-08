using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
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
        private const int UsernameMaxLength = 20;
        private const int EmailMaxLength = 100;
        private const int NameMaxLength = 50;
        private const string EntityErrorLogMessage = "Error de conexión a la base de datos mediante Entity Framework.";
        private const string SqlErrorLogMessage = "Error de conexión de red o servidor SQL no disponible.";
        private const string UnexpectedErrorLogMessage = "Excepción inesperada en la autenticación.";
        private const string InvalidCredentialsLogMessage = "Intento de inicio de sesión con credenciales inválidas.";
        private const string LockedAccountLogMessage = "Intento de inicio de sesión con la verificación en dos pasos bloqueada.";
        private const string PlayerRegisteredLogMessage = "Jugador registrado correctamente.";

        // El servicio es estático para que los códigos sobrevivan al reabrir la ventana (cambio de idioma o volver).
        private static readonly TwoFactorCodeService _codeService = new TwoFactorCodeService();

        private readonly AppLogger _logger = new AppLogger(typeof(LoginWindow));
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
            try
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
            catch (EntityException ex)
            {
                _logger.Error(ex, EntityErrorLogMessage);
                ShowDialogMessage(Strings.Dialogs_DatabaseConnectionError);
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, SqlErrorLogMessage);
                ShowDialogMessage(Strings.Dialogs_DatabaseConnectionError);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedErrorLogMessage);
                ShowDialogMessage(Strings.Dialogs_UnexpectedError);
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
                ShowDialogMessage(Strings.Auth_ErrorRequiredFields);
                return;
            }

            List<Player> matches = await FindPlayersByEmailAsync(email);
            bool areCredentialsValid = matches.Count > 0 && PasswordHasher.Verify(password, matches[0].PwdHash);

            if (!areCredentialsValid)
            {
                _logger.Warning(InvalidCredentialsLogMessage);
                ShowDialogMessage(Strings.Dialogs_D13_Login);
            }
            else if (_codeService.IsLocked(matches[0].Email))
            {
                _logger.Warning(LockedAccountLogMessage);
                ShowDialogMessage(Strings.Dialogs_D19_Validation2FA);
            }
            else
            {
                await SendCodeAndOpenVerificationAsync(matches[0]);
            }
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
                ShowDialogMessage(Strings.TwoFactor_ErrorSendFailed);
                AuthSubmitButton.IsEnabled = true;
            }
        }

        private async Task ProcessRegisterAsync()
        {
            string formError = GetRegisterFormError();

            if (formError.Length > 0)
            {
                ShowDialogMessage(formError);
                return;
            }

            Player newPlayer = CreatePlayerFromForm();
            string conflictMessage = await SaveNewPlayerAsync(newPlayer);

            if (string.IsNullOrEmpty(conflictMessage))
            {
                _logger.Info(PlayerRegisteredLogMessage);
                ShowDialogMessage(Strings.Auth_SuccessRegister);
                LoginTabButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                ShowDialogMessage(conflictMessage);
            }
        }

        private string GetRegisterFormError()
        {
            string errorMessage = string.Empty;

            if (!HasRequiredRegisterFields())
            {
                errorMessage = Strings.Auth_ErrorRequiredFields;
            }
            else if (!HasValidFieldLengths())
            {
                errorMessage = Strings.Auth_ErrorFieldTooLong;
            }
            else if (!IsValidEmail(RegisterEmailTextBox.Text.Trim()))
            {
                errorMessage = Strings.Auth_ErrorInvalidEmail;
            }

            return errorMessage;
        }

        private bool HasRequiredRegisterFields()
        {
            return !string.IsNullOrWhiteSpace(FirstNameTextBox.Text)
                && !string.IsNullOrWhiteSpace(LastNameTextBox.Text)
                && !string.IsNullOrWhiteSpace(UsernameTextBox.Text)
                && !string.IsNullOrWhiteSpace(RegisterEmailTextBox.Text)
                && !string.IsNullOrWhiteSpace(RegisterPasswordBox.Password);
        }

        private bool HasValidFieldLengths()
        {
            return UsernameTextBox.Text.Trim().Length <= UsernameMaxLength
                && RegisterEmailTextBox.Text.Trim().Length <= EmailMaxLength
                && FirstNameTextBox.Text.Trim().Length <= NameMaxLength
                && LastNameTextBox.Text.Trim().Length <= NameMaxLength;
        }

        private static bool IsValidEmail(string email)
        {
            bool isValid;

            try
            {
                var address = new MailAddress(email);
                isValid = address.Address == email;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
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

        /// <summary>
        /// Muestra un cuadro de diálogo centrado en la ventana actual con el mensaje especificado.
        /// </summary>
        /// <param name="message">Texto que se mostrará en el diálogo.</param>
        private void ShowDialogMessage(string message)
        {
            var dialog = new CustomDialogWindow(new DialogContentDto { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }
    }
}