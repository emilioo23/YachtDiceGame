using System.Windows;
using System.Windows.Media.Media3D;

namespace YachtDice.Views
{
    /// <summary>
    /// Creates the flat squares that make the faces of the cubes and the marks on the table.
    /// </summary>
    internal static class DiceMeshFactory
    {
        private const int BottomLeft = 0;
        private const int BottomRight = 1;
        private const int TopRight = 2;
        private const int TopLeft = 3;
        private const double NoMovement = 0;
        private const double Unit = 1;

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
