using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace YachtDice.Views
{
    /// <summary>
    /// Draws the face of a die with the pips of its value.
    /// </summary>
    internal static class DiceFaceFactory
    {
        /// <summary>
        /// Size in pixels of the side of a die.
        /// </summary>
        public const int DiceSize = 80;

        private const int DiceCornerRadius = 12;
        private const int DicePipSize = 12;
        private const int PipGridSize = 3;
        private const int BorderThicknessNormal = 2;
        private const int BorderThicknessHeld = 3;
        private const int NoValue = 0;
        private const double UnrolledOpacity = 0.55;
        private const double RolledOpacity = 1;
        private const int PipIndexZero = 0;
        private const int PipIndexOne = 1;
        private const int PipIndexTwo = 2;
        private const int FaceOne = 1;
        private const int FaceTwo = 2;
        private const int FaceThree = 3;
        private const int FaceFour = 4;
        private const int FaceFive = 5;
        private const int FaceSix = 6;
        private const string ResourceBrushBorder = "BrushBorder";
        private const string ResourceShadowSoft = "ShadowSoft";
        private const string ResourceBrushWoodDark = "BrushWoodDark";
        private const string ResourceBrushGold = "BrushGold";

        private static readonly Dictionary<int, (int row, int column)[]> PipLayouts =
            new Dictionary<int, (int row, int column)[]>
            {
                { NoValue, new (int row, int column)[] { } },
                { FaceOne, new[] { (PipIndexOne, PipIndexOne) } },
                { FaceTwo, new[] { (PipIndexZero, PipIndexZero), (PipIndexTwo, PipIndexTwo) } },
                {
                    FaceThree,
                    new[] { (PipIndexZero, PipIndexZero), (PipIndexOne, PipIndexOne), (PipIndexTwo, PipIndexTwo) }
                },
                {
                    FaceFour,
                    new[]
                    {
                        (PipIndexZero, PipIndexZero),
                        (PipIndexZero, PipIndexTwo),
                        (PipIndexTwo, PipIndexZero),
                        (PipIndexTwo, PipIndexTwo)
                    }
                },
                {
                    FaceFive,
                    new[]
                    {
                        (PipIndexZero, PipIndexZero),
                        (PipIndexZero, PipIndexTwo),
                        (PipIndexOne, PipIndexOne),
                        (PipIndexTwo, PipIndexZero),
                        (PipIndexTwo, PipIndexTwo)
                    }
                },
                {
                    FaceSix,
                    new[]
                    {
                        (PipIndexZero, PipIndexZero),
                        (PipIndexZero, PipIndexTwo),
                        (PipIndexOne, PipIndexZero),
                        (PipIndexOne, PipIndexTwo),
                        (PipIndexTwo, PipIndexZero),
                        (PipIndexTwo, PipIndexTwo)
                    }
                }
            };

        /// <summary>
        /// Creates the face of a die.
        /// </summary>
        /// <param name="resources">Element that owns the brushes and the shadows used by the drawing.</param>
        /// <param name="value">Value of the die from one to six; zero draws a die that was not rolled yet.</param>
        /// <param name="isHeld">True to mark the die as one that the player keeps.</param>
        /// <returns>The border that contains the pips.</returns>
        public static Border Create(FrameworkElement resources, int value, bool isHeld)
        {
            var border = new Border
            {
                Width = DiceSize,
                Height = DiceSize,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(DiceCornerRadius),
                BorderBrush = (SolidColorBrush)resources.FindResource(ResourceBrushBorder),
                BorderThickness = new Thickness(BorderThicknessNormal),
                Effect = (DropShadowEffect)resources.FindResource(ResourceShadowSoft),
                Opacity = value == NoValue ? UnrolledOpacity : RolledOpacity,
                Child = CreatePipGrid(resources, value),
                Tag = value
            };

            if (isHeld)
            {
                border.BorderBrush = (SolidColorBrush)resources.FindResource(ResourceBrushGold);
                border.BorderThickness = new Thickness(BorderThicknessHeld);
            }

            return border;
        }

        private static Grid CreatePipGrid(FrameworkElement resources, int value)
        {
            var grid = new Grid { Margin = new Thickness(DiceCornerRadius) };

            for (var cellIndex = 0; cellIndex < PipGridSize; cellIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach ((int row, int column) in PipLayouts[value])
            {
                var pip = new Ellipse
                {
                    Width = DicePipSize,
                    Height = DicePipSize,
                    Fill = (SolidColorBrush)resources.FindResource(ResourceBrushWoodDark),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetRow(pip, row);
                Grid.SetColumn(pip, column);
                grid.Children.Add(pip);
            }

            return grid;
        }
    }
}
