using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace GrindBot;

/// Rounded rect with a tail on one side, drawn as a single continuous path so
/// the outline has no seam where the tail meets the body.
public static class Bubble
{
    public static Geometry Path(Rect rect, double scale, bool flipped)
    {
        double s = scale;
        double r = 17 * s;
        double tail = 11 * s;
        double bodyMaxX = rect.Right - tail;
        double tailTop = rect.Bottom - 36 * s;

        // A very short bubble would otherwise put the tail above its own body.
        tailTop = Math.Max(rect.Top + r, Math.Min(tailTop, rect.Bottom - r - 19 * s));
        r = Math.Min(r, Math.Min(rect.Width - tail, rect.Height) / 2);

        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(new Point(rect.Left + r, rect.Top), true, true);
            c.LineTo(new Point(bodyMaxX - r, rect.Top), true, true);
            Arc(c, new Point(bodyMaxX, rect.Top + r), r);
            c.LineTo(new Point(bodyMaxX, tailTop), true, true);
            c.LineTo(new Point(rect.Right, tailTop + 10 * s), true, true);
            c.LineTo(new Point(bodyMaxX, tailTop + 19 * s), true, true);
            c.LineTo(new Point(bodyMaxX, rect.Bottom - r), true, true);
            Arc(c, new Point(bodyMaxX - r, rect.Bottom), r);
            c.LineTo(new Point(rect.Left + r, rect.Bottom), true, true);
            Arc(c, new Point(rect.Left, rect.Bottom - r), r);
            c.LineTo(new Point(rect.Left, rect.Top + r), true, true);
            Arc(c, new Point(rect.Left + r, rect.Top), r);
        }
        g.Freeze();

        if (!flipped) return g;

        // Mirror about the vertical centre line for left-hand screen corners.
        var mirrored = g.Clone();
        mirrored.Transform = new MatrixTransform(-1, 0, 0, 1, rect.Width, 0);
        mirrored.Freeze();
        return mirrored;
    }

    private static void Arc(StreamGeometryContext c, Point to, double r) =>
        c.ArcTo(to, new Size(r, r), 0, false, SweepDirection.Clockwise, true, true);
}

/// Draws the bubble at whatever size the text ended up needing.
internal sealed class BubbleBackground : FrameworkElement
{
    private readonly double _scale;
    private readonly bool _flipped;
    private readonly Brush _fill;
    private readonly Pen _pen;

    public BubbleBackground(double scale, bool flipped, Color fill, Color stroke)
    {
        _scale = scale;
        _flipped = flipped;
        _fill = fill.Brush();
        _pen = new Pen(stroke.Brush(), 1);
        _pen.Freeze();
        IsHitTestVisible = false;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo info)
    {
        base.OnRenderSizeChanged(info);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (RenderSize.Width <= 1 || RenderSize.Height <= 1) return;
        var rect = new Rect(0.5, 0.5, RenderSize.Width - 1, RenderSize.Height - 1);
        dc.DrawGeometry(_fill, _pen, Bubble.Path(rect, _scale, _flipped));
    }
}

/// Windows hands back monitors in device order, which puts the primary
/// wherever it feels like. config.json documents 0 as the primary display, so
/// the list is reordered to match — and the settings picker uses the same one.
public static class Screens
{
    public static List<WinForms.Screen> Ordered()
    {
        var all = WinForms.Screen.AllScreens;
        var primary = WinForms.Screen.PrimaryScreen;
        var list = new List<WinForms.Screen>();
        if (primary is not null) list.Add(primary);
        foreach (var s in all)
            if (primary is null || !s.Equals(primary)) list.Add(s);
        if (list.Count == 0 && all.Length > 0) list.Add(all[0]);
        return list;
    }
}

/// Reads the message file and hands out messages in order, or shuffled.
public sealed class MessageDeck
{
    private readonly Random _rng = new();
    private List<string> _messages = new();
    private List<string> _queue = new();
    private bool _shuffle;

    public int Count => _messages.Count;

    /// Files are concatenated in order. messages.txt ships identical to
    /// packs/classic.txt, so drawing from everything would otherwise weight
    /// those five lines double — dedupe on the line itself, first one wins.
    public void Reload(Config cfg)
    {
        _shuffle = cfg.shuffle;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        _messages = cfg.MessageFiles()
            .SelectMany(Read)
            .Where(seen.Add)
            .ToList();
        _queue = new List<string>();
    }

    public string? Next()
    {
        if (_messages.Count == 0) return null;
        if (_queue.Count == 0)
            _queue = _shuffle ? _messages.OrderBy(_ => _rng.Next()).ToList() : new List<string>(_messages);
        var next = _queue[0];
        _queue.RemoveAt(0);
        return next;
    }

    /// One message per line; blank lines and `#` comments ignored.
    private static List<string> Read(string name)
    {
        var path = Paths.NextToApp(name);
        if (path is null) return new List<string>();
        try
        {
            return File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#"))
                .ToList();
        }
        catch { return new List<string>(); }
    }
}

/// The overlay itself: borderless, click-through, always on top, and pinned to
/// a screen corner. Rebuilt from scratch whenever the config changes, which is
/// cheap and keeps the layout code a straight read of PopupView.swift.
public sealed class PopupWindow : Window
{
    /// Window size at scale 1. Everything inside scales with it.
    public const double BaseWidth = 440, BaseHeight = 230;

    private readonly Settings _settings;
    private readonly DispatcherTimer _blinkTimer;
    private readonly DispatcherTimer _typeTimer;
    private readonly DispatcherTimer _hideTimer;
    private readonly DispatcherTimer _blinkOff;

    private RobotVisual? _robot;
    private TextBlock? _label;
    private FrameworkElement? _content;
    private ScaleTransform _scaleT = new(1, 1);
    private TranslateTransform _slideT = new();

    private List<string> _typing = new();
    private int _typeIndex;
    private bool _visible;

    public PopupWindow(Settings settings)
    {
        _settings = settings;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        Focusable = false;
        IsHitTestVisible = false;
        Title = "GrindBot";
        SnapsToDevicePixels = true;
        UseLayoutRounding = false;
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);

        SourceInitialized += (_, _) => { Native.MakeOverlay(this); Reposition(); };
        DpiChanged += (_, _) => Dispatcher.BeginInvoke(new Action(() => Reposition()), DispatcherPriority.Loaded);

        _blinkTimer = new DispatcherTimer(DispatcherPriority.Normal)
        { Interval = TimeSpan.FromSeconds(2.4) };
        _blinkTimer.Tick += (_, _) =>
        {
            if (!_visible) return;
            _robot?.SetBlink(true);
            _blinkOff!.Stop();
            _blinkOff.Start();
        };
        _blinkTimer.Start();

        _blinkOff = new DispatcherTimer(DispatcherPriority.Normal)
        { Interval = TimeSpan.FromSeconds(0.13) };
        _blinkOff.Tick += (_, _) => { _blinkOff.Stop(); _robot?.SetBlink(false); };

        _typeTimer = new DispatcherTimer(DispatcherPriority.Normal);
        _typeTimer.Tick += (_, _) => TypeOne();

        _hideTimer = new DispatcherTimer(DispatcherPriority.Normal);
        _hideTimer.Tick += (_, _) => { _hideTimer.Stop(); Retract(); };

        Rebuild(_settings.Cfg);
    }

    // MARK: - Building the view

    /// Straight port of PopupView.body: robot hugs the screen edge, bubble
    /// points back at it, and the whole arrangement mirrors on the left.
    public void Rebuild(Config cfg)
    {
        double s = cfg.scale;
        bool onLeft = cfg.position.IsLeft();
        bool onTop = cfg.position.IsTop();
        bool dark = Theme.IsDark(cfg.theme);
        string text = _label?.Text ?? "";

        Width = BaseWidth * s;
        Height = BaseHeight * s;

        _robot = new RobotVisual(cfg.RobotStyle) { VerticalAlignment = VerticalAlignment.Bottom };

        _label = new TextBlock
        {
            Text = text,
            FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI"),
            FontSize = 14.5 * s,
            FontWeight = FontWeights.Medium,
            Foreground = Theme.BubbleText(dark).Brush(),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness((onLeft ? 24 : 16) * s, 12 * s, (onLeft ? 16 : 24) * s, 12 * s),
        };

        var plate = new BubbleBackground(s, onLeft, Theme.BubbleFill(dark), Theme.BubbleStroke(dark));
        plate.DropShadow(Colors.Black, 0.25, 14 * s, 6 * s);

        var bubble = new Grid
        {
            MaxWidth = cfg.maxBubbleWidth * s,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(onLeft ? 8 * s : 0, 0, onLeft ? 0 : 8 * s, 14 * s),
        };
        bubble.Children.Add(plate);
        bubble.Children.Add(_label);

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        if (onLeft) { row.Children.Add(_robot); row.Children.Add(bubble); }
        else { row.Children.Add(bubble); row.Children.Add(_robot); }

        var host = new Grid
        {
            Margin = new Thickness(cfg.margin * s / 1.5, 18 * s, cfg.margin * s / 1.5, 18 * s),
            HorizontalAlignment = onLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            VerticalAlignment = onTop ? VerticalAlignment.Top : VerticalAlignment.Bottom,
        };
        host.Children.Add(row);

        // Grows out of its own corner, the way the SwiftUI anchor did.
        _scaleT = new ScaleTransform(_visible ? 1 : 0.9, _visible ? 1 : 0.9);
        _slideT = new TranslateTransform(0, _visible ? 0 : (onTop ? -16 * s : 16 * s));
        var group = new TransformGroup();
        group.Children.Add(_scaleT);
        group.Children.Add(_slideT);
        host.RenderTransform = group;
        host.RenderTransformOrigin = new Point(onLeft ? 0 : 1, onTop ? 0 : 1);
        host.Opacity = _visible ? 1 : 0;

        _content = host;
        Content = host;
        UpdateLayout();
        Reposition(cfg);
    }

    // MARK: - Placement

    public void Reposition(Config? cfg = null)
    {
        cfg ??= _settings.Cfg;
        var screen = PickScreen(cfg.screenIndex);
        var area = screen.WorkingArea;
        var (w, h) = Native.PhysicalSize(this);
        if (w == 0 || h == 0) return;

        int x = cfg.position.IsLeft() ? area.Left : area.Right - w;
        int y = cfg.position.IsTop() ? area.Top : area.Bottom - h;
        Native.MoveTo(this, x, y);
    }

    /// Pinned by default; -1 follows whatever window has focus, which is the
    /// closest Windows has to macOS's "main screen".
    private static WinForms.Screen PickScreen(int index)
    {
        var screens = Screens.Ordered();
        if (index >= 0 && index < screens.Count) return screens[index];
        var fg = Native.GetForegroundWindow();
        if (fg != IntPtr.Zero)
        {
            try { return WinForms.Screen.FromHandle(fg); } catch { }
        }
        return screens[0];
    }

    // MARK: - Speaking

    /// Type it out, hold it, take it away.
    public void Say(string text)
    {
        var cfg = _settings.Cfg;
        _hideTimer.Stop();
        _typeTimer.Stop();

        _typing = TextX.Graphemes(text);
        _typeIndex = 0;

        if (cfg.typeSpeed > 0)
        {
            _label!.Text = "";
            _typeTimer.Interval = TimeSpan.FromSeconds(cfg.typeSpeed);
            _typeTimer.Start();
        }
        else
        {
            _label!.Text = text;
        }

        Native.BumpTopmost(this);
        Present();
        Log.Write($"say '{text}' for {cfg.Dwell(text):0.0}s");

        _hideTimer.Interval = TimeSpan.FromSeconds(Math.Max(0.3, cfg.Dwell(text)));
        _hideTimer.Start();
    }

    /// Pull the bubble off screen now, cancelling any pending hide.
    public void HideNow()
    {
        _hideTimer.Stop();
        _typeTimer.Stop();
        Retract(0.25);
    }

    private void TypeOne()
    {
        if (_typeIndex >= _typing.Count) { _typeTimer.Stop(); return; }
        _label!.Text += _typing[_typeIndex++];
    }

    private void Present()
    {
        if (_content is null) return;
        _visible = true;
        var dur = TimeSpan.FromSeconds(0.42);
        // A touch of overshoot stands in for SwiftUI's spring(0.42, 0.7).
        var ease = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.25 };

        _content.BeginAnimation(OpacityProperty,
            new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
        _scaleT.BeginAnimation(ScaleTransform.ScaleXProperty,
            new DoubleAnimation(1, dur) { EasingFunction = ease });
        _scaleT.BeginAnimation(ScaleTransform.ScaleYProperty,
            new DoubleAnimation(1, dur) { EasingFunction = ease });
        _slideT.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(0, dur) { EasingFunction = ease });
    }

    private void Retract(double seconds = 0.35)
    {
        if (_content is null) return;
        _visible = false;
        var cfg = _settings.Cfg;
        var dur = TimeSpan.FromSeconds(seconds);
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

        _content.BeginAnimation(OpacityProperty, new DoubleAnimation(0, dur) { EasingFunction = ease });
        _scaleT.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.9, dur) { EasingFunction = ease });
        _scaleT.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.9, dur) { EasingFunction = ease });
        _slideT.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(cfg.position.IsTop() ? -16 * cfg.scale : 16 * cfg.scale, dur) { EasingFunction = ease });
        _robot?.SetBlink(false);
    }
}
