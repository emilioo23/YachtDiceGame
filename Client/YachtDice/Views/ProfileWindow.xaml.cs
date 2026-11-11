using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.Client.Logic.Avatars;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window to view and edit the profile information and the statistics of the player.
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private const int NoLevelRequired = 0;

        private readonly PlayerSession _session;
        private readonly PlayerGateway _playerGateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(ProfileWindow));

        private List<AvatarRequirement> _catalog = new List<AvatarRequirement>();
        private List<AvatarOption> _options = new List<AvatarOption>();
        private int _level = AvatarResourceKey.DefaultAvatarId;
        private int _savedAvatarId = AvatarResourceKey.DefaultAvatarId;
        private int _selectedAvatarId = AvatarResourceKey.DefaultAvatarId;

        /// <summary>
        /// Initializes the profile window for the given player and starts loading the avatars.
        /// </summary>
        /// <param name="session">Session of the player whose profile is being edited.</param>
        public ProfileWindow(PlayerSession session)
        {
            InitializeComponent();
            _session = session;
            UsernameTextBox.Text = session.DisplayName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ProfileWindow(session);

            _ = GatewayCallRunner.RunAsync(this, _logger, LoadAvatarsAsync);
        }

        private static AvatarRequirement ToRequirement(UnlockableAvatar avatar)
        {
            return new AvatarRequirement { Id = avatar.Id, RequiredLevel = avatar.RequiredLevel };
        }

        private async Task LoadAvatarsAsync()
        {
            PlayerProfile profile = await _playerGateway.GetProfileAsync(_session.DisplayName);
            List<UnlockableAvatar> catalog = await _playerGateway.GetAvatarCatalogAsync();

            _catalog = catalog.Select(ToRequirement).ToList();
            _level = profile.Level;
            _savedAvatarId = profile.AvatarId;
            _selectedAvatarId = profile.AvatarId;

            LevelTextBlock.Text = string.Format(Strings.Profile_LevelFormat, profile.Level, profile.Experience);
            ShowSelection();
        }

        private void ShowSelection()
        {
            _options = AvatarOptionBuilder.Build(_catalog, _level, _selectedAvatarId);

            AvatarPreviewImage.Source = AvatarImages.Get(_selectedAvatarId);
            AvatarOptionsItemsControl.ItemsSource = _options;
        }

        private async Task SaveAvatarAsync()
        {
            AvatarChangeResultCode result =
                await _playerGateway.ChangeAvatarAsync(_session.DisplayName, _selectedAvatarId);

            if (result == AvatarChangeResultCode.Success)
            {
                _savedAvatarId = _selectedAvatarId;
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, AvatarChangeMessages.GetMessage(result, GetRequiredLevel()));
            }
        }

        private int GetRequiredLevel()
        {
            Func<AvatarOption, bool> isSameId = candidate => candidate.Id == _selectedAvatarId;
            AvatarOption option = _options.FirstOrDefault(isSameId);

            return option == null ? NoLevelRequired : option.RequiredLevel;
        }

        private void AvatarOption_Click(object sender, MouseButtonEventArgs e)
        {
            int avatarId = (int)((FrameworkElement)sender).Tag;
            Func<AvatarOption, bool> isSameId = candidate => candidate.Id == avatarId;
            AvatarOption option = _options.First(isSameId);

            if (option.IsUnlocked)
            {
                _selectedAvatarId = avatarId;
                ShowSelection();
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, string.Format(Strings.Profile_AvatarLocked, option.RequiredLevel));
            }
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAvatarId == _savedAvatarId)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }
            else
            {
                await GatewayCallRunner.RunAsync(this, _logger, SaveAvatarAsync);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}
