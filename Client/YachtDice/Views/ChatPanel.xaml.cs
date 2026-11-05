using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Models;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Chat of the match. It can be hidden with a button; while it is hidden, it shows a notice with the number
    /// of messages that were not read, and a click on the notice shows the chat again.
    /// </summary>
    public partial class ChatPanel : UserControl
    {
        private const int NoUnreadMessages = 0;
        private const int EmptyLength = 0;
        private const int MessageMarginBottom = 4;
        private const string AuthorSeparator = ": ";

        private bool _isCollapsed;
        private int _unreadCount;

        /// <summary>
        /// Initializes the chat panel, expanded and without messages.
        /// </summary>
        public ChatPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event triggered when the player asks to send a message. The argument is the text, already trimmed.
        /// </summary>
        public event Action<string> MessageSubmitted;

        /// <summary>
        /// Adds a message at the end of the chat. If the chat is hidden and the message was written by another
        /// player, it counts as unread and the notice shows it.
        /// </summary>
        /// <param name="author">Name of the player who wrote the message, or of the game for a notice.</param>
        /// <param name="content">Text of the message.</param>
        /// <param name="kind">Who wrote the message.</param>
        public void AddMessage(string author, string content, ChatMessageKind kind)
        {
            MessagesPanel.Children.Add(CreateMessageBlock(author, content, kind));
            MessagesScrollViewer.ScrollToEnd();

            if (_isCollapsed && kind == ChatMessageKind.Other)
            {
                _unreadCount++;
                ShowUnreadNotice(author, content);
            }
        }

        /// <summary>
        /// Stops the player from writing, for example when the server does not deliver the messages.
        /// </summary>
        public void DisableSending()
        {
            MessageTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private TextBlock CreateMessageBlock(string author, string content, ChatMessageKind kind)
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(EmptyLength, EmptyLength, EmptyLength, MessageMarginBottom),
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.Inlines.Add(new Run(author + AuthorSeparator)
            {
                FontWeight = FontWeights.Bold,
                Foreground = GetAuthorBrush(kind)
            });

            textBlock.Inlines.Add(new Run(content)
            {
                Foreground = GetContentBrush(kind)
            });

            return textBlock;
        }

        private Brush GetAuthorBrush(ChatMessageKind kind)
        {
            string brushKey = kind == ChatMessageKind.Own ? ResourceKeys.FeltLightBrushKey : GetContentBrushKey(kind);

            return (Brush)FindResource(brushKey);
        }

        private Brush GetContentBrush(ChatMessageKind kind)
        {
            return (Brush)FindResource(GetContentBrushKey(kind));
        }

        private static string GetContentBrushKey(ChatMessageKind kind)
        {
            return kind == ChatMessageKind.System ? ResourceKeys.DangerBrushKey : ResourceKeys.TextBrushKey;
        }

        private void ShowUnreadNotice(string author, string content)
        {
            UnreadCountTextBlock.Text = _unreadCount.ToString();
            UnreadBadge.Visibility = Visibility.Visible;
            PreviewTextBlock.Text = author + AuthorSeparator + content;
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

            if (content.Length > EmptyLength)
            {
                MessageTextBox.Clear();
                MessageSubmitted?.Invoke(content);
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
