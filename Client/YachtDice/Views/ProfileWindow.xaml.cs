using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using YachtDice.Client.Logic.Avatars;
using YachtDice.Client.Logic.Players;
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
        private const int DefaultTitleId = 0;
        private const double LockedOpacity = 0.6;
        private const string HeaderWithCountFormat = "{0} - {1}";
        private const string NoFlagCode = "";
        private const int FlagIconWidth = 24;
        private const int FlagIconHeight = 18;
        private const int FlagIconGap = 8;
        private const int NotFoundIndex = -1;

        private readonly PlayerSession _session;
        private readonly PlayerGateway _playerGateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(ProfileWindow));

        private List<AvatarRequirement> _catalog = new List<AvatarRequirement>();
        private List<AvatarOption> _options = new List<AvatarOption>();
        private int _level = AvatarResourceKey.DefaultAvatarId;
        private int _savedAvatarId = AvatarResourceKey.DefaultAvatarId;
        private int _selectedAvatarId = AvatarResourceKey.DefaultAvatarId;
        private int _savedTitleId = DefaultTitleId;
        private string _savedFlagCode = NoFlagCode;

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
            _savedTitleId = profile.TitleId;
            _savedFlagCode = profile.FlagCode ?? NoFlagCode;
            ShowFlags();
            ShowSelection();
            await ShowStatisticsAsync();
            await ShowAchievementsAsync();
        }

        private async Task ShowAchievementsAsync()
        {
            List<AchievementInfo> achievements = await _playerGateway.GetAchievementsAsync(_session.DisplayName);

            ShowTitles(achievements);
            ShowAchievementBadges(achievements);
        }

        private void ShowTitles(List<AchievementInfo> achievements)
        {
            TitleComboBox.Items.Clear();
            TitleComboBox.Items.Add(CreateTitleItem(DefaultTitleId, true));

            Func<AchievementInfo, bool> grantsTitle = candidate => candidate.HasTitle;

            foreach (AchievementInfo achievement in achievements.Where(grantsTitle))
            {
                TitleComboBox.Items.Add(CreateTitleItem(achievement.Id, achievement.IsUnlocked));
            }

            Func<ComboBoxItem, bool> isSavedTitle = item => (int)item.Tag == _savedTitleId;
            ComboBoxItem savedItem = TitleComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(isSavedTitle);

            TitleComboBox.SelectedItem = savedItem ?? TitleComboBox.Items[0];
        }

        private static ComboBoxItem CreateTitleItem(int titleId, bool isUnlocked)
        {
            string name = ProgressTexts.GetTitleName(titleId);

            return new ComboBoxItem
            {
                Tag = titleId,
                IsEnabled = isUnlocked,
                Content = isUnlocked ? name : string.Format(Strings.Profile_TitleLocked, name)
            };
        }

        private void ShowAchievementBadges(List<AchievementInfo> achievements)
        {
            Func<AchievementInfo, bool> isUnlocked = candidate => candidate.IsUnlocked;
            int unlockedCount = achievements.Count(isUnlocked);

            AchievementsHeaderTextBlock.Text = string.Format(
                HeaderWithCountFormat,
                Strings.Profile_Achievements_Header,
                string.Format(Strings.Profile_AchievementsFormat, unlockedCount, achievements.Count));
            AchievementsWrapPanel.Children.Clear();

            foreach (AchievementInfo achievement in achievements)
            {
                AchievementsWrapPanel.Children.Add(CreateAchievementBadge(achievement));
            }
        }

        private Border CreateAchievementBadge(AchievementInfo achievement)
        {
            var name = new TextBlock
            {
                Text = ProgressTexts.GetAchievementName(achievement.Id),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource(achievement.IsUnlocked ? "BrushFeltDark" : "BrushTextMuted")
            };

            return new Border
            {
                Child = name,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(12, 7, 12, 7),
                Margin = new Thickness(0, 0, 8, 8),
                Opacity = achievement.IsUnlocked ? 1 : LockedOpacity,
                ToolTip = ProgressTexts.GetAchievementDescription(achievement.Id),
                Background = (Brush)FindResource(achievement.IsUnlocked ? "BrushGoldButton" : "BrushBorder")
            };
        }

        private void ShowFlags()
        {
            Func<string, string> readName = ProgressTexts.GetFlagName;
            IEnumerable<string> sortedCodes = FlagCodes.GetAll().OrderBy(readName);

            FlagComboBox.Items.Clear();
            FlagComboBox.Items.Add(new ComboBoxItem { Tag = NoFlagCode, Content = Strings.Flag_None });

            foreach (string code in sortedCodes)
            {
                FlagComboBox.Items.Add(CreateFlagItem(code));
            }

            Func<ComboBoxItem, bool> isSavedFlag = item => (string)item.Tag == _savedFlagCode;

            FlagComboBox.SelectedItem = FlagComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(isSavedFlag)
                ?? FlagComboBox.Items[0];
        }

        private static ComboBoxItem CreateFlagItem(string code)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };

            row.Children.Add(new Image
            {
                Source = FlagImages.Get(code),
                Width = FlagIconWidth,
                Height = FlagIconHeight,
                Margin = new Thickness(0, 0, FlagIconGap, 0)
            });
            row.Children.Add(new TextBlock { Text = ProgressTexts.GetFlagName(code) });

            return new ComboBoxItem { Tag = code, Content = row };
        }

        private string GetSelectedFlagCode()
        {
            var selectedItem = FlagComboBox.SelectedItem as ComboBoxItem;

            return selectedItem == null ? _savedFlagCode : (string)selectedItem.Tag;
        }

        private void FlagSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = FlagSearchTextBox.Text.Trim();

            FlagSearchHintTextBlock.Visibility = FlagSearchTextBox.Text.Length == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            foreach (ComboBoxItem item in FlagComboBox.Items.Cast<ComboBoxItem>())
            {
                item.Visibility = IsFlagItemShown(item, query) ? Visibility.Visible : Visibility.Collapsed;
            }

            FlagComboBox.IsDropDownOpen = query.Length > 0;
        }

        private static bool IsFlagItemShown(ComboBoxItem item, string query)
        {
            var code = (string)item.Tag;

            if (code == NoFlagCode || query.Length == 0)
            {
                return true;
            }

            CompareOptions ignoreCaseAndAccents = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

            return CultureInfo.CurrentCulture.CompareInfo
                .IndexOf(ProgressTexts.GetFlagName(code), query, ignoreCaseAndAccents) != NotFoundIndex;
        }

        private void FlagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string code = GetSelectedFlagCode();
            bool hasFlag = FlagCodes.IsKnown(code);

            FlagPreviewImage.Source = hasFlag ? FlagImages.Get(code) : null;
            FlagPreviewImage.Visibility = hasFlag ? Visibility.Visible : Visibility.Collapsed;
        }

        private int GetSelectedTitleId()
        {
            var selectedItem = TitleComboBox.SelectedItem as ComboBoxItem;

            return selectedItem == null ? _savedTitleId : (int)selectedItem.Tag;
        }

        private void TitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TitlePreviewTextBlock.Text = ProgressTexts.GetTitleName(GetSelectedTitleId());
        }

        private async Task ShowStatisticsAsync()
        {
            PlayerStatistics statistics = await _playerGateway.GetStatisticsAsync(_session.DisplayName);

            StatGamesValueTextBlock.Text = statistics.GamesPlayed.ToString();
            StatWinsValueTextBlock.Text = statistics.Wins.ToString();
            StatBestScoreValueTextBlock.Text = statistics.BestScore.ToString();
            StatExperienceValueTextBlock.Text = statistics.Experience.ToString();
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
            await GatewayCallRunner.RunAsync(this, _logger, SaveChangesAsync);
        }

        private async Task SaveChangesAsync()
        {
            string newUsername = UsernameTextBox.Text.Trim();
            bool hasNewAvatar = _selectedAvatarId != _savedAvatarId;
            bool hasNewUsername = newUsername != _session.DisplayName;
            bool hasNewTitle = GetSelectedTitleId() != _savedTitleId;
            bool hasNewFlag = GetSelectedFlagCode() != _savedFlagCode;

            if (hasNewAvatar)
            {
                await SaveAvatarAsync();
            }

            if (hasNewTitle)
            {
                await SaveTitleAsync(hasNewAvatar);
            }

            if (hasNewFlag)
            {
                await SaveFlagAsync(hasNewAvatar || hasNewTitle);
            }

            if (hasNewUsername)
            {
                await SaveUsernameAsync(newUsername, hasNewAvatar || hasNewTitle || hasNewFlag);
            }

            if (!hasNewAvatar && !hasNewTitle && !hasNewFlag && !hasNewUsername)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }
        }

        private async Task SaveTitleAsync(bool isSavedMessageShown)
        {
            int titleId = GetSelectedTitleId();
            TitleChangeResultCode result = await _playerGateway.ChangeTitleAsync(_session.DisplayName, titleId);

            if (result == TitleChangeResultCode.Success)
            {
                _savedTitleId = titleId;

                if (!isSavedMessageShown)
                {
                    CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
                }
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_TitleSaveError);
            }
        }

        private async Task SaveFlagAsync(bool isSavedMessageShown)
        {
            string flagCode = GetSelectedFlagCode();
            FlagChangeResultCode result = await _playerGateway.ChangeFlagAsync(_session.DisplayName, flagCode);

            if (result == FlagChangeResultCode.Success)
            {
                _savedFlagCode = flagCode;

                if (!isSavedMessageShown)
                {
                    CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
                }
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_FlagSaveError);
            }
        }

        private async Task SaveUsernameAsync(string newUsername, bool isSavedMessageShown)
        {
            UsernameChangeResultCode result =
                await _playerGateway.ChangeUsernameAsync(_session.DisplayName, newUsername);

            if (result == UsernameChangeResultCode.Success)
            {
                ReopenWithNewUsername(newUsername, isSavedMessageShown);
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, UsernameChangeMessages.GetMessage(result));
            }
        }

        private void ReopenWithNewUsername(string newUsername, bool isSavedMessageShown)
        {
            var renamedSession = new PlayerSession(newUsername, _session.FirstName, _session.IsGuest);

            _session.Presence.Disconnect();

            if (!isSavedMessageShown)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }

            WindowNavigator.SwitchTo(this, new ProfileWindow(renamedSession));
        }

        private bool HasUnsavedChanges()
        {
            return _selectedAvatarId != _savedAvatarId
                || UsernameTextBox.Text.Trim() != _session.DisplayName
                || GetSelectedTitleId() != _savedTitleId
                || GetSelectedFlagCode() != _savedFlagCode;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasUnsavedChanges() && !CustomDialogWindow.Confirm(this, Strings.Profile_DiscardChangesConfirm))
            {
                return;
            }

            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}
