using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Views
{
    /// <summary>
    /// The materials that the dice share: the seven faces (blank and one to six) and the shadow under each die.
    /// They are created once and frozen, so the five dice use the same ones.
    /// </summary>
    internal sealed class DiceMaterials
    {
        private const string PipBrushKey = "BrushWoodDark";
        private const string EdgeBrushKey = "BrushBorder";
        private const double FaceSize = 100;
        private const double EdgeThickness = 3;
        private const double PipRadius = 9;
        private const double PipStart = 22;
        private const double PipStep = 28;
        private const byte ShadowAlpha = 120;
        private const byte Transparent = 0;
        private const int NoValue = 0;

        private readonly Material[] _faces;

        private DiceMaterials(Material[] faces, Material shadow)
        {
            _faces = faces;
            Shadow = shadow;
        }

        /// <summary>
        /// Gets the material of a face without pips, used by a die that was not rolled yet.
        /// </summary>
        public Material Blank => _faces[NoValue];

        /// <summary>
        /// Gets the soft dark material that is drawn under every die.
        /// </summary>
        public Material Shadow { get; }

        /// <summary>
        /// Creates the materials with the colors of the current theme.
        /// </summary>
        /// <param name="resources">Element that owns the brushes of the theme.</param>
        /// <returns>The materials, ready to be used by the dice.</returns>
        public static DiceMaterials Create(FrameworkElement resources)
        {
            Color pipColor = ReadColor(resources, PipBrushKey);
            Color edgeColor = ReadColor(resources, EdgeBrushKey);
            var faces = new Material[DiceFaceOrientation.LastFace + 1];

            for (var value = NoValue; value <= DiceFaceOrientation.LastFace; value++)
            {
                faces[value] = Freeze(new DiffuseMaterial(CreateFaceBrush(value, pipColor, edgeColor)));
            }

            return new DiceMaterials(faces, CreateShadow());
        }

        /// <summary>
        /// Gets the material of a face.
        /// </summary>
        /// <param name="value">Value of the face, from one to six.</param>
        /// <returns>The material with the pips of the value.</returns>
        public Material GetFace(int value)
        {
            return _faces[value];
        }

        private static Color ReadColor(FrameworkElement resources, string key)
        {
            return ((SolidColorBrush)resources.FindResource(key)).Color;
        }

        private static Material Freeze(Material material)
        {
            material.Freeze();

            return material;
        }

        private static Material CreateShadow()
        {
            var center = Color.FromArgb(ShadowAlpha, Transparent, Transparent, Transparent);
            var edge = Color.FromArgb(Transparent, Transparent, Transparent, Transparent);

            return Freeze(new DiffuseMaterial(new RadialGradientBrush(center, edge)));
        }

        private static Brush CreateFaceBrush(int value, Color pipColor, Color edgeColor)
        {
            var group = new DrawingGroup();
            var edge = new Pen(new SolidColorBrush(edgeColor), EdgeThickness);
            var square = new RectangleGeometry(new Rect(NoValue, NoValue, FaceSize, FaceSize));

            group.Children.Add(new GeometryDrawing(Brushes.White, edge, square));

            foreach (PipCell cell in DiceFaces.GetCells(value))
            {
                var center = new Point(PipStart + (cell.Column * PipStep), PipStart + (cell.Row * PipStep));
                var pip = new EllipseGeometry(center, PipRadius, PipRadius);

                group.Children.Add(new GeometryDrawing(new SolidColorBrush(pipColor), null, pip));
            }

            var brush = new DrawingBrush(group) { Stretch = Stretch.Fill };

            brush.Freeze();

            return brush;
        }
    }
}
