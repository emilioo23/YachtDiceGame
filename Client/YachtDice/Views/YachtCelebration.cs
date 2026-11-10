using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Animation that celebrates a Yacht over the table: a golden flash, a title that bounces in and
    /// confetti that falls. It only draws; the window decides when to play it.
    /// </summary>
    internal sealed class YachtCelebration
    {
        private const double HalfDivisor = 2;
        private const double CenterOrigin = 0.5;
        private const double NoScale = 0;
        private const double FullScale = 1;
        private const double TableLeft = 540;
        private const double TableWidth = 708;
        private const double TableCenterY = 300;
        private const double StageWidth = 1248;
        private const double StageHeight = 768;
        private const double TitleFontSize = 110;
        private const double SubtitleFontSize = 30;
        private const double TitleBlur = 36;
        private const double FlashSize = 300;
        private const double FlashStartScale = 0.2;
        private const double FlashEndScale = 3.2;
        private const double FlashSeconds = 0.9;
        private const double FlashOpacity = 0.85;
        private const double TitleSeconds = 0.8;
        private const double ConfettiWidth = 10;
        private const double ConfettiHeight = 16;
        private const double ConfettiMinSeconds = 1.6;
        private const double ConfettiExtraSeconds = 1.0;
        private const double ConfettiMaxDelay = 0.8;
        private const double ConfettiSpinDegrees = 720;
        private const double ConfettiStartTop = -40;
        private const double FadeStartSeconds = 3.2;
        private const double FadeSeconds = 0.6;
        private const double FullOpacity = 1;
        private const double NoOpacity = 0;
        private const double ElasticSpringiness = 4;
        private const double TitleCenterOffset = 80;
        private const int ConfettiCount = 70;
        private const int ElasticOscillations = 2;
        private const int FullCircleDegrees = 360;
        private const string GoldBrushKey = "BrushGold";
        private const string CreamBrushKey = "BrushCream";
        private const string DisplayFontKey = "FontDisplay";

        private static readonly string[] ConfettiBrushKeys =
        {
            "BrushGold",
            "BrushCream",
            "BrushDanger",
            "BrushFeltLight",
            "BrushWhite"
        };

        private readonly Canvas _canvas;
        private readonly FrameworkElement _resources;
        private readonly Random _random = new Random();

        private int _playId;

        /// <summary>
        /// Initializes the celebration over a canvas.
        /// </summary>
        /// <param name="canvas">Canvas that covers the game area and receives the drawing.</param>
        /// <param name="resources">Element that owns the brushes used by the drawing.</param>
        public YachtCelebration(Canvas canvas, FrameworkElement resources)
        {
            _canvas = canvas;
            _resources = resources;
        }

        /// <summary>
        /// Plays the celebration. If another one is still running, it is replaced.
        /// </summary>
        /// <param name="playerName">Name of the player who scored the Yacht.</param>
        /// <param name="points">Points that the Yacht gave.</param>
        public void Play(string playerName, int points)
        {
            _playId++;
            int currentId = _playId;

            _canvas.BeginAnimation(UIElement.OpacityProperty, null);
            _canvas.Opacity = FullOpacity;
            _canvas.Children.Clear();

            AddFlash();
            AddConfetti();
            AddTitle(string.Format(Strings.Game_Celebration_YachtPoints, playerName, points));
            StartFade(currentId);
        }

        private void StartFade(int playId)
        {
            var fade = new DoubleAnimation(FullOpacity, NoOpacity, TimeSpan.FromSeconds(FadeSeconds))
            {
                BeginTime = TimeSpan.FromSeconds(FadeStartSeconds),
                FillBehavior = FillBehavior.HoldEnd
            };

            fade.Completed += (sender, e) => Finish(playId);
            _canvas.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void Finish(int playId)
        {
            if (playId == _playId)
            {
                _canvas.BeginAnimation(UIElement.OpacityProperty, null);
                _canvas.Opacity = FullOpacity;
                _canvas.Children.Clear();
            }
        }

        private Brush FindBrush(string key)
        {
            return (Brush)_resources.FindResource(key);
        }

        private Color FindColor(string brushKey)
        {
            return ((SolidColorBrush)FindBrush(brushKey)).Color;
        }

        private void AddFlash()
        {
            var scale = new ScaleTransform(FlashStartScale, FlashStartScale);
            var flash = new Ellipse
            {
                Width = FlashSize,
                Height = FlashSize,
                Fill = new RadialGradientBrush(FindColor(GoldBrushKey), Colors.Transparent),
                Opacity = FlashOpacity,
                RenderTransform = scale,
                RenderTransformOrigin = new Point(CenterOrigin, CenterOrigin)
            };

            Canvas.SetLeft(flash, TableLeft + (TableWidth - FlashSize) / HalfDivisor);
            Canvas.SetTop(flash, TableCenterY - FlashSize / HalfDivisor);
            _canvas.Children.Add(flash);

            var grow = new DoubleAnimation(FlashStartScale, FlashEndScale, TimeSpan.FromSeconds(FlashSeconds));
            var vanish = new DoubleAnimation(FlashOpacity, NoOpacity, TimeSpan.FromSeconds(FlashSeconds));

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
            flash.BeginAnimation(UIElement.OpacityProperty, vanish);
        }

        private void AddTitle(string subtitle)
        {
            var scale = new ScaleTransform(NoScale, NoScale);
            StackPanel stage = CreateTitleStage(subtitle, scale);

            Canvas.SetLeft(stage, TableLeft);
            Canvas.SetTop(stage, TableCenterY - TitleCenterOffset);
            _canvas.Children.Add(stage);

            var bounce = new DoubleAnimation(NoScale, FullScale, TimeSpan.FromSeconds(TitleSeconds))
            {
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = ElasticOscillations,
                    Springiness = ElasticSpringiness
                }
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, bounce);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
        }

        private StackPanel CreateTitleStage(string subtitle, ScaleTransform scale)
        {
            var stage = new StackPanel
            {
                Width = TableWidth,
                RenderTransform = scale,
                RenderTransformOrigin = new Point(CenterOrigin, CenterOrigin)
            };

            stage.Children.Add(CreateTitleText(Strings.Game_Celebration_YachtTitle, TitleFontSize, GoldBrushKey));
            stage.Children.Add(CreateTitleText(subtitle, SubtitleFontSize, CreamBrushKey));

            return stage;
        }

        private TextBlock CreateTitleText(string text, double fontSize, string brushKey)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontFamily = (FontFamily)_resources.FindResource(DisplayFontKey),
                Foreground = FindBrush(brushKey),
                TextAlignment = TextAlignment.Center,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = TitleBlur,
                    ShadowDepth = 0,
                    Opacity = FullOpacity
                }
            };
        }

        private void AddConfetti()
        {
            for (var index = 0; index < ConfettiCount; index++)
            {
                AddConfettiPiece();
            }
        }

        private void AddConfettiPiece()
        {
            var rotation = new RotateTransform(0);
            var piece = new Rectangle
            {
                Width = ConfettiWidth,
                Height = ConfettiHeight,
                Fill = FindBrush(ConfettiBrushKeys[_random.Next(ConfettiBrushKeys.Length)]),
                RenderTransform = rotation,
                RenderTransformOrigin = new Point(CenterOrigin, CenterOrigin)
            };

            Canvas.SetLeft(piece, _random.NextDouble() * StageWidth);
            Canvas.SetTop(piece, ConfettiStartTop);
            _canvas.Children.Add(piece);

            TimeSpan delay = TimeSpan.FromSeconds(_random.NextDouble() * ConfettiMaxDelay);
            TimeSpan duration = TimeSpan.FromSeconds(ConfettiMinSeconds + _random.NextDouble() * ConfettiExtraSeconds);
            var fall = new DoubleAnimation(ConfettiStartTop, StageHeight, duration) { BeginTime = delay };
            var spin = new DoubleAnimation(0, ConfettiSpinDegrees + _random.Next(FullCircleDegrees), duration)
            {
                BeginTime = delay
            };

            piece.BeginAnimation(Canvas.TopProperty, fall);
            rotation.BeginAnimation(RotateTransform.AngleProperty, spin);
        }
    }
}
