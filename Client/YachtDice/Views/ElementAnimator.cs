using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YachtDice.Views
{
    /// <summary>
    /// Short animations shared by the windows, so each window does not build its own. A pulse makes an element
    /// grow or shrink for a moment and come back to its size: a button that is pressed shrinks a little and a die
    /// that lands grows a little.
    /// </summary>
    public sealed class ElementAnimator
    {
        private const double NormalScale = 1;
        private const double CenterPoint = 0.5;

        /// <summary>
        /// Makes the element change its size for a moment and come back.
        /// </summary>
        /// <param name="element">Element to animate.</param>
        /// <param name="peakScale">Size at the middle of the pulse; less than 1 shrinks and more than 1 grows.</param>
        /// <param name="duration">Time to reach the peak; it takes the same to come back.</param>
        public void Pulse(UIElement element, double peakScale, TimeSpan duration)
        {
            ScaleTransform scale = FindOrAddScale(element);
            var pulse = new DoubleAnimation(NormalScale, peakScale, duration)
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, pulse);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, pulse);
        }

        private static ScaleTransform FindOrAddScale(UIElement element)
        {
            ScaleTransform scale = element.RenderTransform as ScaleTransform;

            if (scale == null && element.RenderTransform is TransformGroup group)
            {
                scale = FindScale(group);
            }

            if (scale == null)
            {
                scale = new ScaleTransform();
                element.RenderTransform = CombineWithScale(element.RenderTransform, scale);
                element.RenderTransformOrigin = new Point(CenterPoint, CenterPoint);
            }

            return scale;
        }

        private static ScaleTransform FindScale(TransformGroup group)
        {
            ScaleTransform found = null;

            foreach (Transform transform in group.Children)
            {
                found = found ?? transform as ScaleTransform;
            }

            return found;
        }

        private static Transform CombineWithScale(Transform current, ScaleTransform scale)
        {
            Transform combined = scale;

            if (current != null && current != Transform.Identity)
            {
                var group = new TransformGroup();

                group.Children.Add(scale);
                group.Children.Add(current);
                combined = group;
            }

            return combined;
        }
    }
}
