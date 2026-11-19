using System;
using System.Windows.Media.Media3D;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Views
{
    /// <summary>
    /// One die of the table: a cube with a pip face on each side, the shadow under it and the gold mark that it
    /// has when the player keeps it. It lies in a place of the table and, when it is thrown, it flies to a new
    /// place. It never decides a value: the board tells it which face must be on top.
    /// </summary>
    internal sealed class Die3D
    {
        private const double CubeSize = 1.7;
        private const double Half = 0.5;
        private const double RestHeight = CubeSize * Half;
        private const double HeldLift = 0.45;
        private const double NoLift = 0;
        private const double ShadowSize = 2.3;
        private const double MarkerSize = 2.1;
        private const double ShadowLevel = 0.01;
        private const double MarkerLevel = 0.02;
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
        private readonly TranslateTransform3D _markerPosition = new TranslateTransform3D();
        private readonly ModelVisual3D _marker;
        private readonly GeometryModel3D _markerModel;

        private bool _isHeld;
        private double _restX;
        private double _restZ;
        private double _restYaw;
        private double _startYaw;
        private DieThrow _throw;

        /// <summary>
        /// Initializes the die, without a place and with blank faces.
        /// </summary>
        /// <param name="materials">Materials that the five dice share.</param>
        public Die3D(DiceMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            Body = new ModelVisual3D { Content = CreateBody(), Transform = CreateBodyTransform() };
            _markerModel = new GeometryModel3D(DiceMeshFactory.CreateFlatSquare(MarkerSize * Half), materials.Marker);
            _marker = new ModelVisual3D { Transform = _markerPosition };

            var shadow = new ModelVisual3D
            {
                Content = new GeometryModel3D(DiceMeshFactory.CreateFlatSquare(ShadowSize * Half), materials.Shadow),
                Transform = _shadowPosition
            };

            Root = new ModelVisual3D();
            Root.Children.Add(shadow);
            Root.Children.Add(_marker);
            Root.Children.Add(Body);
        }

        /// <summary>
        /// Gets the visual that holds the cube. A click on it is a click on the die.
        /// </summary>
        public ModelVisual3D Body { get; }

        /// <summary>
        /// Gets the visual that holds the whole die: the shadow, the mark and the cube.
        /// </summary>
        public ModelVisual3D Root { get; }

        /// <summary>
        /// Gets a value indicating whether the die is in the air.
        /// </summary>
        public bool IsFlying { get; private set; }

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
        /// Puts the die still in a place of the table.
        /// </summary>
        /// <param name="slot">Place where the die lies.</param>
        /// <param name="yawDegrees">Angle in degrees that the die has around the vertical axis.</param>
        public void PlaceAt(DiceSlot slot, double yawDegrees)
        {
            _restX = slot.X;
            _restZ = slot.Z;
            _restYaw = yawDegrees;
            IsFlying = false;
            _tumble.Angle = NoTurn;
            _yaw.Angle = yawDegrees;
            MoveTo(_restX, RestHeight + GetLift(), _restZ);
        }

        /// <summary>
        /// Lifts the die and shows the gold mark under it, or puts it down again.
        /// </summary>
        /// <param name="isHeld">True when the player keeps the die for the next roll.</param>
        public void SetHeld(bool isHeld)
        {
            _isHeld = isHeld;
            _marker.Content = isHeld ? _markerModel : null;

            if (!IsFlying)
            {
                MoveTo(_restX, RestHeight + GetLift(), _restZ);
            }
        }

        /// <summary>
        /// Prepares a throw: the die will leave the place where it lies.
        /// </summary>
        /// <param name="plan">Where the die lands and how it turns.</param>
        public void StartThrow(DieThrow plan)
        {
            _throw = plan ?? throw new ArgumentNullException(nameof(plan));
            _startYaw = _restYaw;
            IsFlying = true;
        }

        /// <summary>
        /// Puts the die in the middle of its throw.
        /// </summary>
        /// <param name="progress">Progress of the throw, from zero to one.</param>
        public void Fly(double progress)
        {
            double travel = DiceThrowPath.Travel(progress);
            double x = _restX + ((_throw.EndX - _restX) * travel);
            double z = _restZ + ((_throw.EndZ - _restZ) * travel);

            _tumble.Axis = _throw.TumbleAxis;
            _tumble.Angle = DiceThrowPath.TumbleDegrees(_throw.TumbleDegrees, progress);
            _yaw.Angle = _startYaw + ((_throw.YawDegrees - _startYaw) * travel);
            MoveTo(x, DiceThrowPath.Height(progress, RestHeight), z);
        }

        /// <summary>
        /// Leaves the die still where its throw ends. It does nothing when the die is not in the air.
        /// </summary>
        public void Settle()
        {
            if (IsFlying)
            {
                PlaceAt(new DiceSlot(_throw.EndX, _throw.EndZ), _throw.YawDegrees);
            }
        }

        private static Vector3D FindRight(Vector3D normal)
        {
            return Math.Abs(normal.X) > Half ? DepthAxis : RightAxis;
        }

        private double GetLift()
        {
            return _isHeld ? HeldLift : NoLift;
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
            _markerPosition.OffsetX = x;
            _markerPosition.OffsetY = MarkerLevel;
            _markerPosition.OffsetZ = z;
        }
    }
}
