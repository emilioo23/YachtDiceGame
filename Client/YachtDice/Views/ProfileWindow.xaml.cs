using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Client.Logic.Avatars;
using YachtDice.Client.Logic.Logging;
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
        private const int NoQueryLength = 0;
        private const double BadgeFontSize = 14;
        private const double BadgeCornerRadius = 16;
        private const double BadgeHorizontalPadding = 12;
        private const double BadgeVerticalPadding = 7;
        private const double BadgeGap = 8;
        private const double UnlockedOpacity = 1;
        private const string UnlockedTextBrushKey = "BrushFeltDark";
        private const string LockedTextBrushKey = "BrushTextMuted";
        private const string UnlockedBackgroundBrushKey = "BrushGoldButton";
        private const string LockedBackgroundBrushKey = "BrushBorder";

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
            UsernameTextBox.MaxLength = RegistrationLimits.UsernameMaxLength;
            _session = session ?? throw new ArgumentNullException(nameof(session), LogMessages.NullSessionMessage);
            UsernameTextBox.Text = session.DisplayName;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ProfileWindow(session);

            if (session.IsGuest)
            {
                LockEditing();
            }

            _ = GatewayCallRunner.RunAsync(this, _logger, LoadAvatarsAsync);
        }

        private void LockEditing()
        {
            UsernameTextBox.IsEnabled = false;
            AvatarOptionsItemsControl.IsEnabled = false;
            TitleComboBox.IsEnabled = false;
            FlagSearchTextBox.IsEnabled = false;
            FlagComboBox.IsEnabled = false;
            SaveChangesButton.IsEnabled = false;
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
            List<Achievement> achievements = await _playerGateway.GetAchievementsAsync(
                _session.SessionToken,
                _session.DisplayName);

            ShowTitles(achievements);
            ShowAchievementBadges(achievements);
        }

        private void ShowTitles(List<Achievement> achievements)
        {
            TitleComboBox.Items.Clear();
            TitleComboBox.Items.Add(CreateTitleItem(DefaultTitleId, true));

            Func<Achievement, bool> grantsTitle = candidate => candidate.HasTitle;

            foreach (Achievement achievement in achievements.Where(grantsTitle))
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
            string content = name;

            if (!isUnlocked)
            {
                content = string.Format(Strings.Profile_TitleLocked, name);
            }

            return new ComboBoxItem
            {
                Tag = titleId,
                IsEnabled = isUnlocked,
                Content = content
            };
        }

        private void ShowAchievementBadges(List<Achievement> achievements)
        {
            Func<Achievement, bool> isUnlocked = candidate => candidate.IsUnlocked;
            int unlockedCount = achievements.Count(isUnlocked);

            AchievementsHeaderTextBlock.Text = string.Format(
                HeaderWithCountFormat,
                Strings.Profile_Achievements_Header,
                string.Format(Strings.Profile_AchievementsFormat, unlockedCount, achievements.Count));
            AchievementsWrapPanel.Children.Clear();

            foreach (Achievement achievement in achievements)
            {
                AchievementsWrapPanel.Children.Add(CreateAchievementBadge(achievement));
            }
        }

        private Border CreateAchievementBadge(Achievement achievement)
        {
            string textBrushKey = achievement.IsUnlocked ? UnlockedTextBrushKey : LockedTextBrushKey;
            string backgroundBrushKey = achievement.IsUnlocked ? UnlockedBackgroundBrushKey : LockedBackgroundBrushKey;
            var name = new TextBlock
            {
                Text = ProgressTexts.GetAchievementName(achievement.Id),
                FontSize = BadgeFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource(textBrushKey)
            };

            return new Border
            {
                Child = name,
                CornerRadius = new CornerRadius(BadgeCornerRadius),
                Padding = new Thickness(
                    BadgeHorizontalPadding,
                    BadgeVerticalPadding,
                    BadgeHorizontalPadding,
                    BadgeVerticalPadding),
                Margin = new Thickness(0, 0, BadgeGap, BadgeGap),
                Opacity = achievement.IsUnlocked ? UnlockedOpacity : LockedOpacity,
                ToolTip = ProgressTexts.GetAchievementDescription(achievement.Id),
                Background = (Brush)FindResource(backgroundBrushKey)
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

            FlagSearchHintTextBlock.Visibility = FlagSearchTextBox.Text.Length == NoQueryLength
                ? Visibility.Visible
                : Visibility.Collapsed;

            foreach (ComboBoxItem item in FlagComboBox.Items.Cast<ComboBoxItem>())
            {
                item.Visibility = IsFlagItemShown(item, query) ? Visibility.Visible : Visibility.Collapsed;
            }

            Func<ComboBoxItem, bool> isShownCountry = IsShownCountry;
            bool hasQuery = query.Length > NoQueryLength;
            bool hasMatches = FlagComboBox.Items.Cast<ComboBoxItem>().Any(isShownCountry);
            bool hasNoResults = hasQuery && !hasMatches;

            FlagNoResultsTextBlock.Visibility = hasNoResults ? Visibility.Visible : Visibility.Collapsed;
            FlagComboBox.IsDropDownOpen = hasQuery && hasMatches;
        }

        private static bool IsShownCountry(ComboBoxItem item)
        {
            var code = (string)item.Tag;

            return code != NoFlagCode && item.Visibility == Visibility.Visible;
        }

        private static bool IsFlagItemShown(ComboBoxItem item, string query)
        {
            var code = (string)item.Tag;
            bool isShown = code == NoFlagCode || query.Length == NoQueryLength;

            if (!isShown)
            {
                CompareOptions ignoreCaseAndAccents = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

                isShown = CultureInfo.CurrentCulture.CompareInfo
                    .IndexOf(ProgressTexts.GetFlagName(code), query, ignoreCaseAndAccents) != NotFoundIndex;
            }

            return isShown;
        }

        private void FlagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string code = GetSelectedFlagCode();
            bool hasFlag = FlagCodes.IsKnown(code);

            FlagPreviewImage.Source = null;
            FlagPreviewImage.Visibility = Visibility.Collapsed;

            if (hasFlag)
            {
                FlagPreviewImage.Source = FlagImages.Get(code);
                FlagPreviewImage.Visibility = Visibility.Visible;
            }
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
                await _playerGateway.ChangeAvatarAsync(_session.SessionToken, _selectedAvatarId);

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
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_UsernameRequired);
            }
            else
            {
                await GatewayCallRunner.RunAsync(this, _logger, SaveChangesAsync);
            }
        }

        private async Task SaveChangesAsync()
        {
            string newUsername = UsernameTextBox.Text.Trim();
            bool hasNewAvatar = _selectedAvatarId != _savedAvatarId;
            bool hasNewUsername = newUsername != _session.DisplayName;
            bool hasNewTitle = GetSelectedTitleId() != _savedTitleId;
            bool hasNewFlag = GetSelectedFlagCode() != _savedFlagCode;
            bool hasNewLook = hasNewAvatar || hasNewTitle;
            bool hasNewLookOrFlag = hasNewLook || hasNewFlag;

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
                await SaveFlagAsync(hasNewLook);
            }

            if (hasNewUsername)
            {
                await SaveUsernameAsync(newUsername, hasNewLookOrFlag);
            }

            if (!hasNewLookOrFlag && !hasNewUsername)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }
        }

        private async Task SaveTitleAsync(bool isSavedMessageShown)
        {
            int titleId = GetSelectedTitleId();
            TitleChangeResultCode result = await _playerGateway.ChangeTitleAsync(_session.SessionToken, titleId);

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
            FlagChangeResultCode result = await _playerGateway.ChangeFlagAsync(_session.SessionToken, flagCode);

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
                await _playerGateway.ChangeUsernameAsync(_session.SessionToken, newUsername);

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
            var renamedSession = new PlayerSession(newUsername, _session.FirstName, _session.Services)
            {
                IsGuest = _session.IsGuest,
                SessionToken = _session.SessionToken
            };

            _session.Presence.Disconnect();

            if (!isSavedMessageShown)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
            }

            WindowNavigator.SwitchTo(this, new ProfileWindow(renamedSession));
        }

        private bool HasUnsavedChanges()
        {
            bool hasNewLook = _selectedAvatarId != _savedAvatarId || GetSelectedTitleId() != _savedTitleId;
            bool hasNewIdentity = UsernameTextBox.Text.Trim() != _session.DisplayName
                || GetSelectedFlagCode() != _savedFlagCode;

            return hasNewLook || hasNewIdentity;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            bool canLeave = !HasUnsavedChanges()
                || CustomDialogWindow.Confirm(this, Strings.Profile_DiscardChangesConfirm);

            if (canLeave)
            {
                WindowNavigator.SwitchTo(this, new MenuWindow(_session));
            }
        }
    }
}
