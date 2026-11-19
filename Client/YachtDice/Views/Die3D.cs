using System;
using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Views
{
    /// <summary>
    /// One die of the scene: a cube with a pip face on each side and the soft shadow under it. It lies in a
    /// place, on the table or in the row under it, and it moves when the board gives it a throw or a glide. It
    /// never decides a value: the board tells it which face must be on top.
    /// </summary>
    internal sealed class Die3D
    {
        private const double CubeSize = 1.3;
        private const double Half = 0.5;
        private const double RestHeight = CubeSize * Half;
        private const double ShadowSize = 2;
        private const double ShadowLevel = 0.01;
        private const double NoTurn = 0;
        private const int FirstFace = DiceFaceOrientation.FirstFace;
        private const int FaceCount = DiceFaceOrientation.LastFace - DiceFaceOrientation.FirstFace + 1;

        private static readonly Vector3D UpAxis = new Vector3D(0, 1, 0);
        private static readonly Vector3D RightAxis = new Vector3D(1, 0, 0);
        private static readonly Vector3D DepthAxis = new Vector3D(0, 0, 1);

        private readonly DiceMaterials _materials;
        private readonly GeometryModel3D[] _faces = new GeometryModel3D[FaceCount];
        private readonly AxisAngleRotation3D _faceUp = new AxisAngleRotation3D(UpAxis, NoTurn);
        private readonly AxisAngleRotation3D _tumble = new AxisAngleRotation3D(UpAxis, NoTurn);
        private readonly AxisAngleRotation3D _yaw = new AxisAngleRotation3D(UpAxis, NoTurn);
        private readonly TranslateTransform3D _bodyPosition = new TranslateTransform3D();
        private readonly TranslateTransform3D _shadowPosition = new TranslateTransform3D();

        private double _startX;
        private double _startZ;
        private double _startYaw;
        private DieMotion _motion;

        /// <summary>
        /// Initializes the die, without a place and with blank faces.
        /// </summary>
        /// <param name="materials">Materials that the five dice share.</param>
        public Die3D(DiceMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            Body = new ModelVisual3D { Content = CreateBody(), Transform = CreateBodyTransform() };

            var shadow = new ModelVisual3D
            {
                Content = new GeometryModel3D(DiceMeshFactory.CreateFlatSquare(ShadowSize * Half), materials.Shadow),
                Transform = _shadowPosition
            };

            Root = new ModelVisual3D();
            Root.Children.Add(shadow);
            Root.Children.Add(Body);
        }

        /// <summary>
        /// Gets the visual that holds the cube. A click on it is a click on the die.
        /// </summary>
        public ModelVisual3D Body { get; }

        /// <summary>
        /// Gets the visual that holds the whole die: the shadow and the cube.
        /// </summary>
        public ModelVisual3D Root { get; }

        /// <summary>
        /// Gets the position of the die to the right of the center of the table.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// Gets the position of the die toward the player, from the center of the table.
        /// </summary>
        public double Z { get; private set; }

        /// <summary>
        /// Gets the angle in degrees that the die has around the vertical axis.
        /// </summary>
        public double YawDegrees { get; private set; }

        /// <summary>
        /// Gets the place where the die lies: on the table or in the row under it.
        /// </summary>
        public DiceLocation Location { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the die is in the middle of a movement.
        /// </summary>
        public bool IsMoving { get; private set; }

        /// <summary>
        /// Shows a face on top of the die.
        /// </summary>
        /// <param name="value">Value from one to six; zero shows a die without pips that was not rolled yet.</param>
        public void ShowValue(int value)
        {
            bool isBlank = value < FirstFace;
            FaceUpRotation faceUp = DiceFaceOrientation.GetFaceUp(isBlank ? FirstFace : value);

            for (var index = 0; index < FaceCount; index++)
            {
                _faces[index].Material = isBlank ? _materials.Blank : _materials.GetFace(index + FirstFace);
            }

            _faceUp.Axis = new Vector3D(faceUp.Axis.X, faceUp.Axis.Y, faceUp.Axis.Z);
            _faceUp.Angle = faceUp.Degrees;
        }

        /// <summary>
        /// Puts the die still in a place.
        /// </summary>
        /// <param name="slot">Place where the die lies.</param>
        /// <param name="yawDegrees">Angle in degrees that the die has around the vertical axis.</param>
        /// <param name="location">Whether the place is on the table or in the row under it.</param>
        public void PlaceAt(DiceSlot slot, double yawDegrees, DiceLocation location)
        {
            X = slot.X;
            Z = slot.Z;
            YawDegrees = yawDegrees;
            Location = location;
            IsMoving = false;
            _tumble.Angle = NoTurn;
            _yaw.Angle = yawDegrees;
            MoveTo(X, RestHeight, Z);
        }

        /// <summary>
        /// Starts a movement from the place where the die is now.
        /// </summary>
        /// <param name="motion">The throw or glide that the die makes.</param>
        public void Begin(DieMotion motion)
        {
            _motion = motion ?? throw new ArgumentNullException(nameof(motion));
            _startX = X;
            _startZ = Z;
            _startYaw = YawDegrees;
            IsMoving = true;
        }

        /// <summary>
        /// Puts the die where it is a time after the start of the animation.
        /// </summary>
        /// <param name="elapsedMilliseconds">Milliseconds that passed since the start of the animation.</param>
        public void Advance(double elapsedMilliseconds)
        {
            double progress = (elapsedMilliseconds - _motion.DelayMilliseconds) / _motion.DurationMilliseconds;

            if (_motion.IsThrow)
            {
                Fly(progress);
            }
            else
            {
                Glide(progress);
            }
        }

        /// <summary>
        /// Leaves the die still where its movement ends. It does nothing when the die is not moving.
        /// </summary>
        public void Settle()
        {
            if (IsMoving)
            {
                PlaceAt(_motion.End, _motion.YawDegrees, _motion.Location);
            }
        }

        private static Vector3D FindRight(Vector3D normal)
        {
            return Math.Abs(normal.X) > Half ? DepthAxis : RightAxis;
        }

        private static double Blend(double from, double to, double share)
        {
            return from + ((to - from) * share);
        }

        private void Fly(double progress)
        {
            double gather = DiceThrowPath.Gather(progress);
            double scatter = DiceThrowPath.Scatter(progress);
            double handX = Blend(_startX, _motion.Hand.X, gather);
            double handZ = Blend(_startZ, _motion.Hand.Z, gather);

            _tumble.Axis = _motion.TumbleAxis;
            _tumble.Angle = DiceThrowPath.TumbleDegrees(_motion.TumbleDegrees, progress);
            _yaw.Angle = Blend(_startYaw, _motion.YawDegrees, scatter);
            MoveTo(
                Blend(handX, _motion.End.X, scatter),
                DiceThrowPath.Height(progress, RestHeight),
                Blend(handZ, _motion.End.Z, scatter));
        }

        private void Glide(double progress)
        {
            double travel = DiceThrowPath.GlideTravel(progress);

            _yaw.Angle = Blend(_startYaw, _motion.YawDegrees, travel);
            MoveTo(
                Blend(_startX, _motion.End.X, travel),
                DiceThrowPath.GlideHeight(progress, RestHeight),
                Blend(_startZ, _motion.End.Z, travel));
        }

        private Model3DGroup CreateBody()
        {
            var body = new Model3DGroup();

            for (var index = 0; index < FaceCount; index++)
            {
                DiceVector normal = DiceFaceOrientation.GetNormal(index + FirstFace);
                var direction = new Vector3D(normal.X, normal.Y, normal.Z);
                var mesh = DiceMeshFactory.CreateCubeFace(direction, FindRight(direction), CubeSize * Half);

                _faces[index] = new GeometryModel3D(mesh, _materials.Blank);
                body.Children.Add(_faces[index]);
            }

            return body;
        }

        private Transform3D CreateBodyTransform()
        {
            var group = new Transform3DGroup();

            group.Children.Add(new RotateTransform3D(_faceUp));
            group.Children.Add(new RotateTransform3D(_tumble));
            group.Children.Add(new RotateTransform3D(_yaw));
            group.Children.Add(_bodyPosition);

            return group;
        }

        private void MoveTo(double x, double y, double z)
        {
            _bodyPosition.OffsetX = x;
            _bodyPosition.OffsetY = y;
            _bodyPosition.OffsetZ = z;
            _shadowPosition.OffsetX = x;
            _shadowPosition.OffsetY = ShadowLevel;
            _shadowPosition.OffsetZ = z;
        }
    }
}
