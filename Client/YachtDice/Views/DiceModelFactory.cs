using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace YachtDice.Views
{
    /// <summary>
    /// Builds the 3D models of the dice: the faces of the cube and the viewport that shows them.
    /// </summary>
    internal static class DiceModelFactory
    {
        private const int BottomLeft = 0;
        private const int BottomRight = 1;
        private const int TopRight = 2;
        private const int TopLeft = 3;
        private const double NoMovement = 0;
        private const double Unit = 1;
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
        private const double ScreenUpZ = -1;
        private static readonly int[] QuadIndices =
        {
            BottomLeft, BottomRight, TopRight, BottomLeft, TopRight, TopLeft
        };

        /// <summary>
        /// Creates one face of a cube whose center is in the origin.
        /// </summary>
        /// <param name="normal">Direction that goes out of the face; it must have a length of one.</param>
        /// <param name="right">Direction of the right side of the face; it must be across the normal.</param>
        /// <param name="halfSize">Half of the side of the cube.</param>
        /// <returns>The mesh of the face, with the texture upright when the face is seen from outside.</returns>
        public static MeshGeometry3D CreateCubeFace(Vector3D normal, Vector3D right, double halfSize)
        {
            Vector3D up = Vector3D.CrossProduct(normal, right);
            var center = new Point3D(normal.X * halfSize, normal.Y * halfSize, normal.Z * halfSize);

            return CreateQuad(center, right * halfSize, up * halfSize);
        }

        /// <summary>
        /// Creates a square that lies on the table, facing up, centered in the origin.
        /// </summary>
        /// <param name="halfSize">Half of the side of the square.</param>
        /// <returns>The mesh of the square.</returns>
        public static MeshGeometry3D CreateFlatSquare(double halfSize)
        {
            var normal = new Vector3D(NoMovement, Unit, NoMovement);
            var right = new Vector3D(Unit, NoMovement, NoMovement);
            Vector3D up = Vector3D.CrossProduct(normal, right);

            return CreateQuad(new Point3D(NoMovement, NoMovement, NoMovement), right * halfSize, up * halfSize);
        }

        /// <summary>
        /// Creates the view with the camera and the lights, without dice.
        /// </summary>
        /// <param name="width">Width of the view in pixels.</param>
        /// <param name="height">Height of the view in pixels.</param>
        /// <returns>The view that receives the dice.</returns>
        public static Viewport3D CreateViewport(double width, double height)
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

        private static MeshGeometry3D CreateQuad(Point3D center, Vector3D right, Vector3D up)
        {
            Vector3D normal = Vector3D.CrossProduct(right, up);
            var mesh = new MeshGeometry3D();

            normal.Normalize();
            mesh.Positions.Add(center - right - up);
            mesh.Positions.Add(center + right - up);
            mesh.Positions.Add(center + right + up);
            mesh.Positions.Add(center - right + up);
            mesh.TextureCoordinates.Add(new Point(NoMovement, Unit));
            mesh.TextureCoordinates.Add(new Point(Unit, Unit));
            mesh.TextureCoordinates.Add(new Point(Unit, NoMovement));
            mesh.TextureCoordinates.Add(new Point(NoMovement, NoMovement));

            for (var corner = BottomLeft; corner <= TopLeft; corner++)
            {
                mesh.Normals.Add(normal);
            }

            foreach (int index in QuadIndices)
            {
                mesh.TriangleIndices.Add(index);
            }

            return mesh;
        }
    }
}
