using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace YachtDice.Utils
{
    /// <summary>
    /// Lets the player press Enter in a text box to press the button of that same form. It is used where a window
    /// has several forms, so the default button of the window cannot be used: each box names its own button in
    /// the XAML, for example <c>utils:EnterKeySubmit.Button="{Binding ElementName=JoinRoomButton}"</c>.
    /// </summary>
    public static class EnterKeySubmit
    {
        /// <summary>
        /// Identifies the button that Enter presses.
        /// </summary>
        public static readonly DependencyProperty ButtonProperty = DependencyProperty.RegisterAttached(
            "Button",
            typeof(ButtonBase),
            typeof(EnterKeySubmit),
            new PropertyMetadata(null, HandleButtonChanged));

        /// <summary>
        /// Gets the button that Enter presses from a text box.
        /// </summary>
        /// <param name="element">Text box of the form.</param>
        /// <returns>The button of the form.</returns>
        public static ButtonBase GetButton(DependencyObject element)
        {
            return (ButtonBase)element.GetValue(ButtonProperty);
        }

        /// <summary>
        /// Sets the button that Enter presses from a text box.
        /// </summary>
        /// <param name="element">Text box of the form.</param>
        /// <param name="value">Button of the form.</param>
        public static void SetButton(DependencyObject element, ButtonBase value)
        {
            element.SetValue(ButtonProperty, value);
        }

        private static void HandleButtonChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (element is UIElement box)
            {
                box.KeyDown -= HandleKeyDown;
                box.KeyDown += HandleKeyDown;
            }
        }

        private static void HandleKeyDown(object sender, KeyEventArgs e)
        {
            ButtonBase button = GetButton((DependencyObject)sender);
            bool canPress = e.Key == Key.Enter && button != null && button.IsEnabled;

            if (canPress)
            {
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }
        }
    }
}
