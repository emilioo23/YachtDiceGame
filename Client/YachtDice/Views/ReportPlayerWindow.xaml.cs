using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.FriendServiceReference;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Dialog where the player chooses the reason to report another player and writes an optional comment.
    /// </summary>
    public partial class ReportPlayerWindow : Window
    {
        private const int CommentMaxLength = 300;
        private const string NullSessionMessage = "The session of the player cannot be null.";
        private const string NullReportedNameMessage = "The name of the reported player cannot be null.";

        private readonly PlayerSession _session;
        private readonly string _reportedName;
        private readonly FriendGateway _friendGateway = new FriendGateway();

        /// <summary>
        /// Initializes the dialog to report a player.
        /// </summary>
        /// <param name="session">Session of the player who reports.</param>
        /// <param name="reportedName">Name of the player who is reported.</param>
        /// <exception cref="ArgumentNullException">The session is null.</exception>
        public ReportPlayerWindow(PlayerSession session, string reportedName)
        {
            InitializeComponent();
            _session = session ?? throw new ArgumentNullException(nameof(session), NullSessionMessage);
            _reportedName = reportedName
                ?? throw new ArgumentNullException(nameof(reportedName), NullReportedNameMessage);

            TitleTextBlock.Text = string.Format(Strings.Report_TitleFormat, reportedName);
            CommentTextBox.MaxLength = CommentMaxLength;
            ReasonComboBox.ItemsSource = CreateReasonOptions();
            ReasonComboBox.SelectedIndex = 0;
        }

        private static List<KeyValuePair<ReportReason, string>> CreateReasonOptions()
        {
            Func<ReportReason, KeyValuePair<ReportReason, string>> createOption =
                reason => new KeyValuePair<ReportReason, string>(reason, ReportResultMessages.GetReasonName(reason));

            return Enum.GetValues(typeof(ReportReason)).Cast<ReportReason>().Select(createOption).ToList();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            await SendReportAsync();
            SendButton.IsEnabled = true;
        }

        private async Task SendReportAsync()
        {
            var report = new PlayerReport
            {
                ReportedName = _reportedName,
                Reason = (ReportReason)ReasonComboBox.SelectedValue,
                Comment = CommentTextBox.Text.Trim()
            };

            try
            {
                ReportResultCode result = await _friendGateway.ReportPlayerAsync(_session.SessionToken, report);
                CustomDialogWindow.ShowMessage(this, ReportResultMessages.GetMessage(result));
                CloseIfFinished(result);
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void CloseIfFinished(ReportResultCode result)
        {
            if (result != ReportResultCode.InvalidComment)
            {
                DialogResult = result == ReportResultCode.Sent;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
