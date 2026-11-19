using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace YachtDice.Views
{
    /// <summary>
    /// Creates the three-dimensional view of the table: a camera that looks down at the felt a little from the
    /// side, and the lights that give each face of a cube its own tone.
    /// </summary>
    internal static class DiceViewportFactory
    {
        private const double CameraHeight = 12.6;
        private const double CameraDistance = 3.6;
        private const double FieldOfView = 52.5;
        private const double NearPlane = 1;
        private const double FarPlane = 50;
        private const byte AmbientLevel = 150;
        private const byte SunLevel = 130;
        private const double SunSideways = -0.35;
        private const double SunDown = -1;
        private const double SunForward = -0.45;
        private const double NoMovement = 0;
        private const double ScreenUpZ = -1;

        /// <summary>
        /// Creates the view with the camera and the lights, without dice.
        /// </summary>
        /// <param name="width">Width of the view in pixels.</param>
        /// <param name="height">Height of the view in pixels.</param>
        /// <returns>The view that receives the dice.</returns>
        public static Viewport3D Create(double width, double height)
        {
            var camera = new PerspectiveCamera(
                new Point3D(NoMovement, CameraHeight, CameraDistance),
                new Vector3D(NoMovement, -CameraHeight, -CameraDistance),
                new Vector3D(NoMovement, NoMovement, ScreenUpZ),
                FieldOfView)
            {
                NearPlaneDistance = NearPlane,
                FarPlaneDistance = FarPlane
            };

            var lights = new Model3DGroup();

            lights.Children.Add(new AmbientLight(Color.FromRgb(AmbientLevel, AmbientLevel, AmbientLevel)));
            lights.Children.Add(new DirectionalLight(
                Color.FromRgb(SunLevel, SunLevel, SunLevel),
                new Vector3D(SunSideways, SunDown, SunForward)));

            var viewport = new Viewport3D { Width = width, Height = height, Camera = camera };

            viewport.Children.Add(new ModelVisual3D { Content = lights });

            return viewport;
        }
    }
}
