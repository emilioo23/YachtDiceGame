using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.ChatServiceReference;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Chat of a room, used in the waiting room and in the match. Once it is attached to the chat connection of
    /// the player, it shows the messages that arrive and sends the ones that the player writes. It can be hidden
    /// with a button; while it is hidden, it shows a notice with the number of messages that were not read, and
    /// a click on the notice shows the chat again.
    /// </summary>
    public partial class ChatPanel : UserControl
    {
        private const int NoUnreadMessages = 0;
        private const int EmptyLength = 0;
        private const int MessageMarginBottom = 4;
        private const int MaxMessageLength = 200;
        private const int AvatarSize = 20;
        private const int AvatarGap = 6;

        private ChatGateway _gateway;
        private string _playerId;
        private bool _isSubscribed;
        private bool _isCollapsed;
        private int _unreadCount;

        /// <summary>
        /// Initializes the chat panel, expanded and without messages.
        /// </summary>
        public ChatPanel()
        {
            InitializeComponent();
            MessageTextBox.MaxLength = MaxMessageLength;
        }

        /// <summary>
        /// Gets or sets the height of the chat while it is shown. A parent that gives the chat a fixed place
        /// uses it to decide how much space the messages have.
        /// </summary>
        public double PanelHeight
        {
            get { return ExpandedCard.Height; }
            set { ExpandedCard.Height = value; }
        }

        /// <summary>
        /// Adds a message at the end of the chat. If the chat is hidden and the message was written by another
        /// player, it counts as unread and the notice shows it.
        /// </summary>
        /// <param name="author">Name of the player who wrote the message, or of the game for a notice.</param>
        /// <param name="content">Text of the message.</param>
        /// <param name="kind">Who wrote the message.</param>
        public void AddMessage(string author, string content, ChatMessageKind kind)
        {
            AddMessage(new ChatEntry
            {
                Author = author,
                Content = content,
                Kind = kind,
                AvatarId = ChatEntry.NoAvatar
            });
        }

        /// <summary>
        /// Adds a message at the end of the chat, with the avatar of its author next to the name. If the chat
        /// is hidden and the message was written by another player, it counts as unread and the notice shows it.
        /// </summary>
        /// <param name="entry">Message to show.</param>
        public void AddMessage(ChatEntry entry)
        {
            MessagesPanel.Children.Add(CreateMessageBlock(entry));
            MessagesScrollViewer.ScrollToEnd();

            if (_isCollapsed && entry.Kind == ChatMessageKind.Other)
            {
                _unreadCount++;
                ShowUnreadNotice(entry.Author, entry.Content);
            }
        }

        /// <summary>
        /// Connects the chat with the connection of the player. It shows the messages that arrived before and
        /// the new ones as they come. If the chat is not available, it tells the player and stops the writing.
        /// </summary>
        /// <param name="chat">Connection of the player to the chat of the room and whether it is available.</param>
        /// <param name="playerId">Identifier of the player, to know which messages are their own.</param>
        public void Attach(RoomChat chat, string playerId)
        {
            _gateway = chat.Gateway;
            _playerId = playerId;

            if (chat.IsAvailable)
            {
                _isSubscribed = true;

                foreach (ChatMessage message in _gateway.Subscribe(HandleMessageReceived))
                {
                    ShowChatMessage(message);
                }
            }
            else
            {
                AddMessage(Strings.Game_Chat_SystemName, Strings.Game_Chat_Unavailable, ChatMessageKind.System);
                DisableSending();
            }
        }

        /// <summary>
        /// Stops showing the messages of the chat connection. The connection stays open for the next window.
        /// </summary>
        public void Detach()
        {
            if (_isSubscribed)
            {
                _isSubscribed = false;
                _gateway.Unsubscribe(HandleMessageReceived);
            }
        }

        private void DisableSending()
        {
            MessageTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private void ShowChatMessage(ChatMessage message)
        {
            ChatMessageKind kind = message.AuthorId == _playerId ? ChatMessageKind.Own : ChatMessageKind.Other;

            AddMessage(new ChatEntry
            {
                Author = message.Author,
                Content = message.Content,
                Kind = kind,
                AvatarId = message.AvatarId
            });
        }

        private void HandleMessageReceived(ChatMessage message)
        {
            Dispatcher.BeginInvoke(new Action(() => ShowChatMessage(message)));
        }

        private async Task SendMessageAsync(string content)
        {
            try
            {
                await _gateway.SendAsync(content);
            }
            catch (GatewayException)
            {
                AddMessage(Strings.Game_Chat_SystemName, Strings.Game_Chat_SendFailed, ChatMessageKind.System);
            }
        }

        private TextBlock CreateMessageBlock(ChatEntry entry)
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(EmptyLength, EmptyLength, EmptyLength, MessageMarginBottom),
                TextWrapping = TextWrapping.Wrap
            };

            if (entry.AvatarId != ChatEntry.NoAvatar)
            {
                textBlock.Inlines.Add(CreateAvatarInline(entry.AvatarId));
            }

            textBlock.Inlines.Add(new Run(entry.Author + Strings.Game_Chat_AuthorSeparator)
            {
                FontWeight = FontWeights.Bold,
                Foreground = GetAuthorBrush(entry.Kind)
            });

            textBlock.Inlines.Add(new Run(entry.Content)
            {
                Foreground = GetContentBrush(entry.Kind)
            });

            return textBlock;
        }

        private Brush GetAuthorBrush(ChatMessageKind kind)
        {
            string brushKey;

            if (kind == ChatMessageKind.Own)
            {
                brushKey = ThemeResourceKeys.FeltLightBrushKey;
            }
            else
            {
                brushKey = GetContentBrushKey(kind);
            }

            return (Brush)FindResource(brushKey);
        }

        private Brush GetContentBrush(ChatMessageKind kind)
        {
            return (Brush)FindResource(GetContentBrushKey(kind));
        }

        private static InlineUIContainer CreateAvatarInline(int avatarId)
        {
            var image = new Image
            {
                Source = AvatarImages.Get(avatarId),
                Width = AvatarSize,
                Height = AvatarSize,
                Margin = new Thickness(EmptyLength, EmptyLength, AvatarGap, EmptyLength)
            };

            return new InlineUIContainer(image) { BaselineAlignment = BaselineAlignment.Center };
        }

        private static string GetContentBrushKey(ChatMessageKind kind)
        {
            return kind == ChatMessageKind.System ? ThemeResourceKeys.DangerBrushKey : ThemeResourceKeys.TextBrushKey;
        }

        private void ShowUnreadNotice(string author, string content)
        {
            UnreadCountTextBlock.Text = _unreadCount.ToString();
            UnreadBadge.Visibility = Visibility.Visible;
            PreviewTextBlock.Text = author + Strings.Game_Chat_AuthorSeparator + content;
        }

        private void Expand()
        {
            _isCollapsed = false;
            _unreadCount = NoUnreadMessages;
            UnreadBadge.Visibility = Visibility.Collapsed;
            PreviewTextBlock.Text = string.Empty;
            CollapsedBar.Visibility = Visibility.Collapsed;
            ExpandedCard.Visibility = Visibility.Visible;
            MessagesScrollViewer.ScrollToEnd();
            MessageTextBox.Focus();
        }

        private void Collapse()
        {
            _isCollapsed = true;
            ExpandedCard.Visibility = Visibility.Collapsed;
            CollapsedBar.Visibility = Visibility.Visible;
        }

        private void SubmitMessage()
        {
            string content = MessageTextBox.Text.Trim();

            if (content.Length > EmptyLength && _isSubscribed)
            {
                MessageTextBox.Clear();
                _ = SendMessageAsync(content);
            }
        }

        private void HideChatButton_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
        }

        private void CollapsedBar_Click(object sender, MouseButtonEventArgs e)
        {
            Expand();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitMessage();
                e.Handled = true;
            }
        }
    }
}
