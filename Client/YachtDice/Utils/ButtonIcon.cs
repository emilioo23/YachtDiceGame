using System;
using System.Windows;
using System.Windows.Media;

namespace YachtDice.Utils
{
    /// <summary>
    /// Puts a small drawing before the text of a button. The drawings live in their own dictionary, which is
    /// loaded once, so changing the theme does not remove them. Each button names its drawing in the XAML, for
    /// example <c>utils:ButtonIcon.Key="Settings"</c>, and the button styles draw it with the color of the text.
    /// </summary>
    public static class ButtonIcon
    {
        private const string IconsSource = "pack://application:,,,/Styles/Icons.xaml";

        private static readonly Lazy<ResourceDictionary> Icons = new Lazy<ResourceDictionary>(
            () => new ResourceDictionary { Source = new Uri(IconsSource, UriKind.Absolute) });

        /// <summary>
        /// Identifies the name of the drawing of a button.
        /// </summary>
        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
            "Key",
            typeof(string),
            typeof(ButtonIcon),
            new PropertyMetadata(null, HandleKeyChanged));

        /// <summary>
        /// Identifies the drawing that the button styles show before the text.
        /// </summary>
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
            "Geometry",
            typeof(Geometry),
            typeof(ButtonIcon),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets the name of the drawing of a button.
        /// </summary>
        /// <param name="element">Button that shows the drawing.</param>
        /// <returns>The name of the drawing.</returns>
        public static string GetKey(DependencyObject element)
        {
            return (string)element.GetValue(KeyProperty);
        }

        /// <summary>
        /// Sets the name of the drawing of a button.
        /// </summary>
        /// <param name="element">Button that shows the drawing.</param>
        /// <param name="value">Name of the drawing in the dictionary of icons.</param>
        public static void SetKey(DependencyObject element, string value)
        {
            element.SetValue(KeyProperty, value);
        }

        /// <summary>
        /// Gets the drawing that a button shows before its text.
        /// </summary>
        /// <param name="element">Button that shows the drawing.</param>
        /// <returns>The drawing, or null when the button has no icon.</returns>
        public static Geometry GetGeometry(DependencyObject element)
        {
            return (Geometry)element.GetValue(GeometryProperty);
        }

        /// <summary>
        /// Sets the drawing that a button shows before its text.
        /// </summary>
        /// <param name="element">Button that shows the drawing.</param>
        /// <param name="value">Drawing to show.</param>
        public static void SetGeometry(DependencyObject element, Geometry value)
        {
            element.SetValue(GeometryProperty, value);
        }

        private static void HandleKeyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            string key = (string)e.NewValue;
            Geometry geometry;

            if (key != null && Icons.Value.Contains(key))
            {
                geometry = (Geometry)Icons.Value[key];
            }
            else
            {
                geometry = Geometry.Empty;
            }

            SetGeometry(element, geometry);
        }
    }
}
