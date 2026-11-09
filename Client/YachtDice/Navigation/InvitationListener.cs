using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YachtDice.Models;
using YachtDice.PresenceServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using YachtDice.Views;

namespace YachtDice.Navigation
{
    /// <summary>
    /// Shows the invitations to a room that the player receives while a window is open. The player can join
    /// the room or reject the invitation. The invitations that arrive while another one is on the screen wait
    /// for their turn. A window creates one listener, starts it when it opens and stops it when it closes.
    /// </summary>
    public sealed class InvitationListener
    {
        private readonly Window _owner;
        private readonly PlayerSession _session;
        private readonly Queue<RoomInvitation> _pendingInvitations = new Queue<RoomInvitation>();
        private bool _isShowingInvitation;
        private bool _isStopped = true;

        /// <summary>
        /// Initializes a listener for the window of the player.
        /// </summary>
        /// <param name="owner">Window over which the invitations are shown.</param>
        /// <param name="session">Session of the player who receives the invitations.</param>
        public InvitationListener(Window owner, PlayerSession session)
        {
            _owner = owner;
            _session = session;
        }

        /// <summary>
        /// Gets or sets the code of the room where the player is waiting. An invitation to that same room is
        /// ignored, and an invitation to another room warns that the player will leave this one. It is empty if
        /// the player is not inside a room.
        /// </summary>
        public string CurrentRoomCode { get; set; } = string.Empty;

        /// <summary>
        /// Starts to show the invitations. A guest has no friends, so nothing is listened for a guest.
        /// </summary>
        public void Start()
        {
            if (!_session.IsGuest && _isStopped)
            {
                _isStopped = false;
                _session.Presence.InvitationReceived += HandleInvitationReceived;
            }
        }

        /// <summary>
        /// Stops showing the invitations and forgets the ones that were waiting.
        /// </summary>
        public void Stop()
        {
            _isStopped = true;
            _session.Presence.InvitationReceived -= HandleInvitationReceived;
            _pendingInvitations.Clear();
        }

        private void HandleInvitationReceived(RoomInvitation invitation)
        {
            _owner.Dispatcher.BeginInvoke(new Func<Task>(() => ShowAsync(invitation)));
        }

        private async Task ShowAsync(RoomInvitation invitation)
        {
            if (!_isStopped)
            {
                _pendingInvitations.Enqueue(invitation);
            }

            if (!_isShowingInvitation)
            {
                _isShowingInvitation = true;
                await AnswerPendingInvitationsAsync();
                _isShowingInvitation = false;
            }
        }

        private async Task AnswerPendingInvitationsAsync()
        {
            while (_pendingInvitations.Any() && !_isStopped)
            {
                RoomInvitation invitation = _pendingInvitations.Dequeue();

                if (!IsCurrentRoom(invitation) && AskToJoin(invitation))
                {
                    await LobbyNavigator.JoinRoomAndOpenAsync(_owner, _session, invitation.RoomCode);
                }
            }
        }

        private bool IsCurrentRoom(RoomInvitation invitation)
        {
            return string.Equals(CurrentRoomCode, invitation.RoomCode, StringComparison.OrdinalIgnoreCase);
        }

        private bool AskToJoin(RoomInvitation invitation)
        {
            var content = new DialogContent
            {
                Title = Strings.Invitations_Received_Title,
                Message = CreateMessage(invitation),
                PrimaryButtonText = Strings.Invitations_Received_Join,
                SecondaryButtonText = Strings.Invitations_Received_Reject
            };
            var dialog = new CustomDialogWindow(content)
            {
                Owner = _owner
            };

            return dialog.ShowDialog() == true;
        }

        private string CreateMessage(RoomInvitation invitation)
        {
            string message = string.Format(Strings.Invitations_Received_Message, invitation.SenderName);

            if (!string.IsNullOrEmpty(CurrentRoomCode))
            {
                message += Environment.NewLine + Strings.Invitations_Received_LeaveNotice;
            }

            return message;
        }
    }
}
