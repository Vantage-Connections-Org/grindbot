using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace GrindBot;

/// Small helpers for laying shapes out on a Canvas the way SwiftUI's stacks
/// ended up placing them — every number here is a literal from RobotView.swift.
internal static class Draw
{
    public static T At<T>(this T el, double x, double y) where T : UIElement
    {
        Canvas.SetLeft(el, x);
        Canvas.SetTop(el, y);
        return el;
    }

    public static Rectangle Box(double w, double h, Brush fill, double radius = 0)
    {
        var r = new Rectangle { Width = w, Height = h, Fill = fill };
        if (radius > 0) { r.RadiusX = radius; r.RadiusY = radius; }
        return r;
    }

    /// SwiftUI Capsule: fully rounded on the short axis.
    public static Rectangle Capsule(double w, double h, Brush fill) =>
        Box(w, h, fill, Math.Min(w, h) / 2);

    public static Ellipse Circle(double d, Brush fill) =>
        new() { Width = d, Height = d, Fill = fill };

    public static T Stroked<T>(this T s, Color c, double thickness) where T : Shape
    {
        s.Stroke = c.Brush();
        s.StrokeThickness = thickness;
        return s;
    }

    /// SwiftUI .shadow(color:radius:) — a centred glow, not an offset drop.
    public static T Glow<T>(this T el, Color color, double opacity, double radius) where T : UIElement
    {
        el.Effect = new DropShadowEffect
        {
            Color = color,
            Opacity = opacity,
            BlurRadius = radius * 2,
            ShadowDepth = 0,
            RenderingBias = RenderingBias.Quality,
        };
        return el;
    }

    public static T DropShadow<T>(this T el, Color color, double opacity, double radius, double dy) where T : UIElement
    {
        el.Effect = new DropShadowEffect
        {
            Color = color,
            Opacity = opacity,
            BlurRadius = radius * 2,
            ShadowDepth = dy,
            Direction = 270,          // straight down, matching SwiftUI's +y
            RenderingBias = RenderingBias.Quality,
        };
        return el;
    }

    public static LinearGradientBrush VGradient(Color top, Color bottom)
    {
        var b = new LinearGradientBrush(top, bottom, new Point(0.5, 0), new Point(0.5, 1));
        b.Freeze();
        return b;
    }
}

/// What shows inside the visor. Add a face: add a `case` here and to `Face`.
/// Drawn into a 44x30 box (times scale), matching the visor panel.
public sealed class FaceVisual : Canvas
{
    public const double BaseWidth = 44, BaseHeight = 30;

    private readonly List<ScaleTransform> _lids = new();
    private readonly double s;
    private readonly Color accent;

    public FaceVisual(RobotStyle style)
    {
        s = style.Scale;
        accent = style.Accent;
        Width = BaseWidth * s;
        Height = BaseHeight * s;
        IsHitTestVisible = false;

        switch (style.Face)
        {
            case Face.visor: BuildVisor(); break;
            case Face.cyclops: BuildCyclops(); break;
            case Face.pixel: BuildPixel(); break;
            case Face.angry: BuildAngry(); break;
            case Face.dot: BuildDot(); break;
        }
    }

    /// Blinking squashes the eyes vertically — shared by every face.
    public void SetBlink(bool blink)
    {
        var to = blink ? 0.12 : 1.0;
        var anim = new DoubleAnimation(to, TimeSpan.FromSeconds(0.09))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
        };
        foreach (var t in _lids) t.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
    }

    /// Squash about the element's own centre, the way scaleEffect(anchor:.center) does.
    private T Lid<T>(T el) where T : FrameworkElement
    {
        var t = new ScaleTransform(1, 1);
        el.RenderTransformOrigin = new Point(0.5, 0.5);
        el.RenderTransform = t;
        _lids.Add(t);
        return el;
    }

    private Shape Eye(Shape shape) => Lid(shape.Glow(accent, 0.9, 5 * s));

    // Two glowing capsules — the original.
    private void BuildVisor()
    {
        Children.Add(Eye(Draw.Capsule(8 * s, 12 * s, accent.Brush())).At(9.5 * s, 9 * s));
        Children.Add(Eye(Draw.Capsule(8 * s, 12 * s, accent.Brush())).At(26.5 * s, 9 * s));
    }

    // One big lens with a darker iris ring.
    private void BuildCyclops()
    {
        var lens = new Canvas { Width = 17 * s, Height = 17 * s };
        lens.Children.Add(Draw.Circle(17 * s, accent.Brush()).Glow(accent, 0.9, 6 * s).At(0, 0));
        lens.Children.Add(Draw.Circle(17 * s, Brushes.Transparent)
            .Stroked(Colors.Black.WithAlpha(0.45), 2.5 * s).At(0, 0));
        lens.Children.Add(Draw.Circle(5 * s, Colors.Black.WithAlpha(0.55).Brush()).At(6 * s, 6 * s));
        Children.Add(Lid(lens).At(13.5 * s, 6.5 * s));
    }

    // 8-bit: square eyes over a three-block mouth.
    private void BuildPixel()
    {
        Children.Add(Eye(Draw.Box(7 * s, 7 * s, accent.Brush())).At(11.5 * s, 8.25 * s));
        Children.Add(Eye(Draw.Box(7 * s, 7 * s, accent.Brush())).At(25.5 * s, 8.25 * s));
        var mouth = accent.WithAlpha(0.75).Brush();
        for (int i = 0; i < 3; i++)
            Children.Add(Draw.Box(4 * s, 3 * s, mouth).At((14 + i * 6) * s, 18.75 * s));
    }

    // Angled brows over narrowed eyes.
    private void BuildAngry()
    {
        Children.Add(Eye(Draw.Capsule(8 * s, 9 * s, accent.Brush())).At(9.5 * s, 12.5 * s));
        Children.Add(Eye(Draw.Capsule(8 * s, 9 * s, accent.Brush())).At(26.5 * s, 12.5 * s));
        Children.Add(Brow(18).At(6.5 * s, 6.5 * s));
        Children.Add(Brow(-18).At(25.5 * s, 6.5 * s));
    }

    private Rectangle Brow(double degrees)
    {
        var b = Draw.Capsule(12 * s, 3 * s, accent.Brush()).Glow(accent, 0.7, 3 * s);
        b.RenderTransformOrigin = new Point(0.5, 0.5);
        b.RenderTransform = new RotateTransform(degrees);
        return b;
    }

    // Round eyes and a smile.
    private void BuildDot()
    {
        Children.Add(Eye(Draw.Circle(7 * s, accent.Brush())).At(10.5 * s, 7 * s));
        Children.Add(Eye(Draw.Circle(7 * s, accent.Brush())).At(24.5 * s, 7 * s));

        // Smile: one quadratic dipping well below its own box, as in Smile.path.
        var fig = new PathFigure { StartPoint = new Point(0, 0), IsClosed = false };
        fig.Segments.Add(new QuadraticBezierSegment(new Point(8 * s, 10.8 * s), new Point(16 * s, 0), true));
        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        geo.Freeze();

        var smile = new Path
        {
            Data = geo,
            Stroke = accent.Brush(),
            StrokeThickness = 2.2 * s,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
        };
        smile.Glow(accent, 0.7, 3 * s);
        Children.Add(smile.At(14 * s, 17 * s));
    }
}

/// The robot. Body and shell are shared; only what sits inside the visor
/// changes between faces, so a new face is one `case` in `FaceVisual`.
public sealed class RobotVisual : Grid
{
    /// The VStack is 90 tall; PopupView gives it a 64-wide frame.
    public const double BaseWidth = 64, BaseHeight = 90;

    private readonly FaceVisual _face;
    private readonly TranslateTransform _bob = new();

    public RobotVisual(RobotStyle style, bool bob = true)
    {
        var s = style.Scale;
        Width = BaseWidth * s;
        Height = BaseHeight * s;
        IsHitTestVisible = false;

        var shell = Draw.VGradient(style.Shell.Adjust(0.02), style.Shell.Adjust(-0.13));
        var trim = style.Shell.Adjust(-0.20).Brush();
        var trimLight = style.Shell.Adjust(-0.17).Brush();
        var armBrush = style.Shell.Adjust(-0.16).Brush();
        var outline = Colors.Black.WithAlpha(0.10);

        var body = new Canvas { Width = BaseWidth * s, Height = BaseHeight * s };
        const double cx = BaseWidth / 2;

        // Antenna: stalk then bulb, both overshooting the 16pt band on purpose.
        body.Children.Add(Draw.Capsule(2.5 * s, 12 * s, trim).At((cx - 1.25) * s, -3 * s));
        body.Children.Add(Draw.Circle(8 * s, style.Accent.Brush())
            .Glow(style.Accent, 0.9, 6 * s).At((cx - 4) * s, -8 * s));

        // Head, then ears on top of it — they are overlays in the SwiftUI original.
        body.Children.Add(Draw.Box(54 * s, 44 * s, shell, 13 * s)
            .Stroked(outline, 1).At(5 * s, 16 * s));
        body.Children.Add(Draw.Capsule(5 * s, 14 * s, trimLight).At(3 * s, 31 * s));
        body.Children.Add(Draw.Capsule(5 * s, 14 * s, trimLight).At(56 * s, 31 * s));
        body.Children.Add(Draw.Box(44 * s, 30 * s, style.Visor.Brush(), 9 * s).At(10 * s, 23 * s));

        _face = new FaceVisual(style);
        body.Children.Add(_face.At(10 * s, 23 * s));

        // Glass highlight across the top of the visor.
        var gloss = Draw.VGradient(Colors.White.WithAlpha(0.18), Colors.White.WithAlpha(0));
        body.Children.Add(Draw.Box(38 * s, 14 * s, gloss, 9 * s).At(13 * s, 25 * s));

        // Neck.
        body.Children.Add(Draw.Box(8 * s, 4 * s, trim).At((cx - 4) * s, 60 * s));

        // Torso and chest light, then the arms over the top.
        body.Children.Add(Draw.Box(42 * s, 26 * s, shell, 10 * s)
            .Stroked(outline, 1).At(11 * s, 64 * s));
        body.Children.Add(Draw.Circle(7 * s, style.Accent.WithAlpha(0.9).Brush())
            .Glow(style.Accent, 0.8, 4 * s).At((cx - 3.5) * s, 73.5 * s));
        body.Children.Add(Draw.Capsule(5 * s, 15 * s, armBrush).At(7 * s, 68.5 * s));
        body.Children.Add(Draw.Capsule(5 * s, 15 * s, armBrush).At(52 * s, 68.5 * s));

        body.RenderTransform = _bob;
        Children.Add(body);

        this.DropShadow(Colors.Black, 0.28, 10 * s, 5 * s);

        if (bob) StartBob(s);
    }

    public void SetBlink(bool blink) => _face.SetBlink(blink);

    /// A Forever animation keeps its clock alive in WPF's timing tree even after
    /// the visual is dropped, so a replaced robot has to be told to stop.
    public void StopBob() => _bob.BeginAnimation(TranslateTransform.YProperty, null);

    /// The idle float: +3 to -3 and back, forever.
    private void StartBob(double s)
    {
        var anim = new DoubleAnimation(3 * s, -3 * s, TimeSpan.FromSeconds(1.6))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut },
        };
        _bob.BeginAnimation(TranslateTransform.YProperty, anim);
    }
}
