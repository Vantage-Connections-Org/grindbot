using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace GrindBot;

/// The whole config surface as a window. Edits apply live and save themselves
/// to config.json — there is no OK/Cancel, what you see is what's running.
public sealed class SettingsWindow : Window
{
    /// Steps the "Every" slider snaps to, 5 seconds up to 5 hours.
    private static readonly double[] Intervals =
    {
        5, 10, 15, 20, 30, 45,
        60, 90, 120, 180, 300, 600, 900, 1200, 1800, 2700,
        3600, 5400, 7200, 10800, 14400, 18000,
    };

    private static readonly Color Ink = Color.FromRgb(0x14, 0x14, 0x17);
    private static readonly Color Panel = Color.FromRgb(0x1E, 0x1E, 0x22);
    private static readonly Color Line = Color.FromRgb(0x33, 0x33, 0x3A);
    private static readonly Color Fg = Color.FromRgb(0xEC, 0xEC, 0xF0);
    private static readonly Color Dim = Color.FromRgb(0x9A, 0x9A, 0xA4);

    private readonly Settings _settings;
    private readonly Action _onPreview;
    private bool _syncing;

    private readonly ContentControl _previewHost = new();
    private readonly StackPanel _timingRows = new();
    private readonly StackPanel _faceRow = new();
    private readonly TextBlock _errorText = new();
    private readonly TextBlock _messageCount = new();
    private readonly List<Action> _sync = new();
    private string _lookSignature = "";
    private string _dwellSignature = "";

    public SettingsWindow(Settings settings, Action onPreview)
    {
        _settings = settings;
        _onPreview = onPreview;

        Title = "GrindBot Settings";
        Width = 540;
        Height = 760;
        MinWidth = 480;
        Background = Ink.Brush();
        Foreground = Fg.Brush();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
        FontSize = 13;

        var stack = new StackPanel { Margin = new Thickness(18) };
        stack.Children.Add(BuildPreview());
        stack.Children.Add(BuildTiming());
        stack.Children.Add(BuildSizeAndPosition());
        stack.Children.Add(BuildColors());
        stack.Children.Add(BuildFaces());
        stack.Children.Add(BuildMessages());
        stack.Children.Add(BuildFooter());

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = stack,
        };

        SyncFromConfig();
    }

    private Config Cfg => _settings.Cfg;

    private void Edit(Action<Config> change)
    {
        if (_syncing) return;
        _settings.Mutate(change);
    }

    public void ShowSaveError(string? error)
    {
        _errorText.Text = error ?? "";
        _errorText.Visibility = error is null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// Push the live config back into every control without re-firing edits.
    /// The rebuilt blocks are gated on what they actually depend on — rebuilding
    /// them every tick would yank a slider out from under the cursor mid-drag.
    public void SyncFromConfig()
    {
        _syncing = true;
        try
        {
            foreach (var s in _sync) s();

            var look = Cfg.accent + Cfg.shell + Cfg.visor + Cfg.face;
            if (look != _lookSignature)
            {
                _lookSignature = look;
                RefreshPreview();
                RefreshFaceRow();
            }
            if (Cfg.dwellMode != _dwellSignature)
            {
                _dwellSignature = Cfg.dwellMode;
                RefreshTimingRows();
            }
        }
        finally { _syncing = false; }
    }

    // MARK: - Live preview

    private UIElement BuildPreview()
    {
        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(new TextBlock
        {
            Text = "Live preview",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Fg.Brush(),
        });
        text.Children.Add(new TextBlock
        {
            Text = "Changes apply immediately and save to:",
            Foreground = Dim.Brush(),
            FontSize = 11.5,
            Margin = new Thickness(0, 2, 0, 0),
        });
        // The whole point of a settings file is knowing where it is.
        text.Children.Add(new TextBlock
        {
            Text = Paths.ResolveWrite("config.json"),
            Foreground = Dim.Brush(),
            FontSize = 11,
            FontFamily = new FontFamily("Consolas, Segoe UI"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 1, 0, 8),
        });

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(Button("Show a message now", () => _onPreview()));
        buttons.Children.Add(Button("Open folder", () =>
        {
            var path = Paths.ResolveWrite("config.json");
            try { Process.Start("explorer.exe", "/select,\"" + path + "\""); } catch { }
        }));
        text.Children.Add(buttons);

        _previewHost.Width = 80;
        _previewHost.VerticalAlignment = VerticalAlignment.Center;

        var row = new DockPanel { LastChildFill = true, Margin = new Thickness(14) };
        DockPanel.SetDock(_previewHost, Dock.Left);
        row.Children.Add(_previewHost);
        row.Children.Add(text);

        return new Border
        {
            Background = Panel.Brush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(0, 0, 0, 14),
            Child = row,
        };
    }

    private void RefreshPreview()
    {
        var style = new RobotStyle(Cfg.AccentColor, ColorX.FromHex(Cfg.shell),
                                   ColorX.FromHex(Cfg.visor), 0.85, Cfg.face);
        _previewHost.Content = new RobotVisual(style)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    // MARK: - Sections

    private UIElement BuildTiming()
    {
        var body = new StackPanel();

        // Snapped to sensible steps: a linear 5s–5h slider would make
        // everything under a minute impossible to hit.
        body.Children.Add(SliderRow("Every", 0, Intervals.Length - 1, 1,
            get: () => NearestIntervalIndex(Cfg.intervalSeconds),
            set: v => Edit(c => c.intervalSeconds = Intervals[(int)Math.Round(v)]),
            format: _ => HumanDuration(Cfg.intervalSeconds)));

        var mode = new StackPanel { Margin = new Thickness(0, 6, 0, 2) };
        mode.Children.Add(Label("Time on screen"));
        var lengthRadio = Radio("Scale with message length", "dwell",
            () => Edit(c => c.dwellMode = "length"));
        var fixedRadio = Radio("Same for every message", "dwell",
            () => Edit(c => c.dwellMode = "fixed"));
        mode.Children.Add(lengthRadio);
        mode.Children.Add(fixedRadio);
        _sync.Add(() =>
        {
            lengthRadio.IsChecked = Cfg.dwellMode != "fixed";
            fixedRadio.IsChecked = Cfg.dwellMode == "fixed";
        });
        body.Children.Add(mode);

        body.Children.Add(_timingRows);

        // Idle gate. The threshold reads as dead weight when the gate is off, so
        // it greys out rather than vanishing — the number is still worth seeing.
        var idle = new CheckBox
        {
            Content = "Only speak once I've gone quiet",
            Foreground = Fg.Brush(),
            Margin = new Thickness(0, 10, 0, 0),
            ToolTip = "No typing, clicking or scrolling for the time below",
        };
        idle.Checked += (_, _) => Edit(c => c.idleOnly = true);
        idle.Unchecked += (_, _) => Edit(c => c.idleOnly = false);
        body.Children.Add(idle);

        var idleRow = SliderRow("Quiet for", 10, 600, 5,
            () => Cfg.idleSeconds,
            v => Edit(c => c.idleSeconds = v),
            HumanDuration);
        body.Children.Add(idleRow);

        _sync.Add(() =>
        {
            idle.IsChecked = Cfg.idleOnly;
            idleRow.IsEnabled = Cfg.idleOnly;
        });

        body.Children.Add(SliderRow("Typing speed", 0, 0.1, 0.002,
            get: () => Cfg.typeSpeed,
            set: v => Edit(c => c.typeSpeed = v),
            format: v => v <= 0.0005 ? "instant" : v.ToString("0.000", CultureInfo.InvariantCulture) + "s/char"));

        return Group("Timing", body);
    }

    /// The dwell controls swap out with the mode, as the SwiftUI `if` did.
    private void RefreshTimingRows()
    {
        _timingRows.Children.Clear();
        if (Cfg.dwellMode == "fixed")
        {
            _timingRows.Children.Add(SliderRow("Duration", 1, 20, 0.5,
                () => Cfg.dwellSeconds, v => Edit(c => c.dwellSeconds = v),
                v => v.ToString("0.0", CultureInfo.InvariantCulture) + "s", live: true));
        }
        else
        {
            _timingRows.Children.Add(SliderRow("Minimum", 1, 10, 0.1,
                () => Cfg.dwellBase, v => Edit(c => c.dwellBase = v),
                v => v.ToString("0.0", CultureInfo.InvariantCulture) + "s", live: true));
            _timingRows.Children.Add(SliderRow("Per character", 0, 0.2, 0.005,
                () => Cfg.dwellPerCharacter, v => Edit(c => c.dwellPerCharacter = v),
                v => v.ToString("0.000", CultureInfo.InvariantCulture) + "s", live: true));
            _timingRows.Children.Add(SliderRow("Maximum", 2, 30, 0.5,
                () => Cfg.dwellMax, v => Edit(c => c.dwellMax = v),
                v => v.ToString("0.0", CultureInfo.InvariantCulture) + "s", live: true));
        }
    }

    private UIElement BuildSizeAndPosition()
    {
        var body = new StackPanel();

        body.Children.Add(SliderRow("Size", 0.5, 4, 0.05,
            () => Cfg.scale, v => Edit(c => c.scale = v),
            v => v.ToString("0.00", CultureInfo.InvariantCulture) + "×"));

        var corners = new (string Label, Corner Value)[]
        {
            ("Bottom right", Corner.bottomRight),
            ("Bottom left", Corner.bottomLeft),
            ("Top right", Corner.topRight),
            ("Top left", Corner.topLeft),
        };
        body.Children.Add(ComboRow("Corner",
            corners.Select(c => (object)c.Label).ToList(),
            () => Array.FindIndex(corners, c => c.Value == Cfg.position),
            i => Edit(c => c.position = corners[i].Value)));

        var displays = new List<object> { "Follow keyboard focus" };
        var indices = new List<int> { -1 };
        var screens = Screens.Ordered();
        for (int i = 0; i < screens.Count; i++)
        {
            var b = screens[i].Bounds;
            displays.Add($"{i + 1} — {b.Width}×{b.Height}{(screens[i].Primary ? " (primary)" : "")}");
            indices.Add(i);
        }
        body.Children.Add(ComboRow("Display", displays,
            () => Math.Max(0, indices.IndexOf(Cfg.screenIndex)),
            i => Edit(c => c.screenIndex = indices[i])));

        body.Children.Add(SliderRow("Edge margin", 0, 120, 1,
            () => Cfg.margin, v => Edit(c => c.margin = v),
            v => ((int)v) + "px"));
        body.Children.Add(SliderRow("Bubble width", 140, 600, 10,
            () => Cfg.maxBubbleWidth, v => Edit(c => c.maxBubbleWidth = v),
            v => ((int)v) + "px"));

        var themes = new List<object> { "Follow Windows", "Dark", "Light" };
        var themeKeys = new[] { "auto", "dark", "light" };
        body.Children.Add(ComboRow("Bubble theme", themes,
            () => Math.Max(0, Array.IndexOf(themeKeys, Cfg.theme)),
            i => Edit(c => c.theme = themeKeys[i])));

        return Group("Size & position", body);
    }

    private UIElement BuildColors()
    {
        var body = new StackPanel();
        body.Children.Add(ColorRow("Accent — eyes, antenna, chest",
            () => Cfg.accent, hex => Edit(c => c.accent = hex)));
        body.Children.Add(ColorRow("Shell — body plastic",
            () => Cfg.shell, hex => Edit(c => c.shell = hex)));
        body.Children.Add(ColorRow("Visor — face panel",
            () => Cfg.visor, hex => Edit(c => c.visor = hex)));

        var reset = Button("Reset colors", () =>
        {
            var d = new Config();
            Edit(c => { c.accent = d.accent; c.shell = d.shell; c.visor = d.visor; });
        });
        reset.HorizontalAlignment = HorizontalAlignment.Right;
        reset.Margin = new Thickness(0, 6, 0, 0);
        body.Children.Add(reset);

        return Group("Colors", body);
    }

    private UIElement BuildFaces()
    {
        _faceRow.Orientation = Orientation.Horizontal;
        return Group("Face", _faceRow);
    }

    private void RefreshFaceRow()
    {
        _faceRow.Children.Clear();
        foreach (Face face in Enum.GetValues<Face>())
        {
            bool selected = Cfg.face == face;
            var style = new RobotStyle(Cfg.AccentColor, ColorX.FromHex(Cfg.shell),
                                       ColorX.FromHex(Cfg.visor), 1.0, face);

            var swatch = new Border
            {
                Background = ColorX.FromHex(Cfg.visor).Brush(),
                CornerRadius = new CornerRadius(7),
                Width = FaceVisual.BaseWidth,
                Height = FaceVisual.BaseHeight,
                Child = new FaceVisual(style),
            };

            var caption = new TextBlock
            {
                Text = face.ToString(),
                FontSize = 10.5,
                Foreground = Dim.Brush(),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0),
            };

            var cell = new StackPanel { Margin = new Thickness(6) };
            cell.Children.Add(swatch);
            cell.Children.Add(caption);

            var button = new Button
            {
                Content = cell,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1.5),
                BorderBrush = (selected ? Cfg.AccentColor : Colors.Transparent).Brush(),
                Background = (selected ? Cfg.AccentColor.WithAlpha(0.22) : Colors.Transparent).Brush(),
                Foreground = Fg.Brush(),
                Tag = face,
            };
            button.Click += (s, _) => Edit(c => c.face = (Face)((Button)s!).Tag);
            _faceRow.Children.Add(button);
        }
    }

    private UIElement BuildMessages()
    {
        var body = new StackPanel();

        var files = Paths.DiscoverMessageFiles();
        foreach (var f in Cfg.MessageFiles())
            if (!files.Contains(f, StringComparer.OrdinalIgnoreCase)) files.Insert(0, f);

        // "Everything" saves as a flag rather than a frozen list, so a pack you
        // drop in later is picked up without revisiting this screen.
        var all = new CheckBox
        {
            Content = "Everything — draw from every file below",
            Foreground = Fg.Brush(),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6),
            ToolTip = "Lines shared between files are only counted once",
        };
        all.Checked += (_, _) => Edit(c => c.allMessageFiles = true);
        all.Unchecked += (_, _) => Edit(c => c.allMessageFiles = false);
        body.Children.Add(all);

        var boxes = new List<CheckBox>();
        var list = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
        foreach (var file in files)
        {
            var name = file;
            var box = new CheckBox
            {
                Content = name,
                Foreground = Fg.Brush(),
                Margin = new Thickness(0, 3, 0, 3),
                Tag = name,
            };
            void Toggle()
            {
                if (_syncing) return;
                var picked = boxes.Where(b => b.IsChecked == true)
                                  .Select(b => (string)b.Tag!)
                                  .ToList();
                // Never leave the deck with nothing to say.
                if (picked.Count == 0) { box.IsChecked = true; return; }
                Edit(c =>
                {
                    c.messageFileList = picked.Count > 1 ? picked : null;
                    c.messagesFile = picked[0];
                });
            }
            box.Checked += (_, _) => Toggle();
            box.Unchecked += (_, _) => Toggle();
            boxes.Add(box);
            list.Children.Add(box);
        }
        body.Children.Add(list);

        _sync.Add(() =>
        {
            all.IsChecked = Cfg.allMessageFiles;
            var active = new HashSet<string>(Cfg.MessageFiles(), StringComparer.OrdinalIgnoreCase);
            foreach (var b in boxes)
            {
                b.IsChecked = Cfg.allMessageFiles || active.Contains((string)b.Tag!);
                // Under "Everything" they show the truth but stop being controls.
                b.IsEnabled = !Cfg.allMessageFiles;
            }
            _messageCount.Text = DescribeDeck();
        });

        _messageCount.Foreground = Dim.Brush();
        _messageCount.FontSize = 11.5;
        _messageCount.Margin = new Thickness(0, 8, 0, 0);
        body.Children.Add(_messageCount);

        var shuffle = new CheckBox
        {
            Content = "Shuffle instead of playing in order",
            Foreground = Fg.Brush(),
            Margin = new Thickness(0, 8, 0, 4),
        };
        shuffle.Checked += (_, _) => Edit(c => c.shuffle = true);
        shuffle.Unchecked += (_, _) => Edit(c => c.shuffle = false);
        _sync.Add(() => shuffle.IsChecked = Cfg.shuffle);
        body.Children.Add(shuffle);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
        buttons.Children.Add(Button("Edit first file…", () => Open(First())));
        buttons.Children.Add(Button("Show in Explorer", () =>
        {
            var path = First();
            if (path is null) return;
            try { Process.Start("explorer.exe", "/select,\"" + path + "\""); } catch { }
        }));
        body.Children.Add(buttons);

        return Group("Messages", body);
    }

    private string? First() =>
        Paths.NextToApp(Cfg.MessageFiles().FirstOrDefault() ?? "messages.txt");

    /// Reads the selection off disk so the count reflects what will actually
    /// play, deduped, rather than the sum of the files.
    private string _deckKey = "";
    private string _deckText = "";

    private string DescribeDeck()
    {
        // SyncFromConfig runs on every config mutation, so this is hit once per
        // slider tick while dragging. Only touch the disk when the selection
        // itself changed, not when a colour moved.
        var key = Cfg.MessageFilesKey();
        if (key == _deckKey && _deckText.Length > 0) return _deckText;

        var deck = new MessageDeck();
        deck.Reload(Cfg);
        var n = Cfg.MessageFiles().Count;
        _deckKey = key;
        _deckText = $"{deck.Count} messages from {n} file{(n == 1 ? "" : "s")}";
        return _deckText;
    }

    /// Called when the files on disk may have changed underneath us (Reload
    /// messages), so the next describe actually re-reads.
    public void InvalidateDeckDescription() => _deckKey = "";

    private UIElement BuildFooter()
    {
        var stack = new StackPanel();
        _errorText.Foreground = Color.FromRgb(0xFF, 0x6B, 0x6B).Brush();
        _errorText.FontSize = 11.5;
        _errorText.TextWrapping = TextWrapping.Wrap;
        _errorText.Visibility = Visibility.Collapsed;
        _errorText.Margin = new Thickness(0, 0, 0, 8);
        stack.Children.Add(_errorText);

        var reset = Button("Reset everything to defaults", () =>
        {
            if (_syncing) return;
            _settings.Cfg = new Config();
        });
        reset.HorizontalAlignment = HorizontalAlignment.Left;
        stack.Children.Add(reset);
        return stack;
    }

    // MARK: - Control helpers

    /// Labelled group box — keeps each section visually separated.
    private static UIElement Group(string title, UIElement content)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = Fg.Brush(),
            Margin = new Thickness(2, 0, 0, 6),
        });
        stack.Children.Add(new Border
        {
            Background = Panel.Brush(),
            BorderBrush = Line.Brush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(12),
            Child = content,
        });
        return new StackPanel { Margin = new Thickness(0, 0, 0, 14), Children = { stack } };
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        Foreground = Fg.Brush(),
        VerticalAlignment = VerticalAlignment.Center,
    };

    private static Button Button(string text, Action onClick)
    {
        var b = new Button
        {
            Content = text,
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 8, 0),
            Cursor = Cursors.Hand,
            Background = Color.FromRgb(0x2E, 0x2E, 0x36).Brush(),
            Foreground = Fg.Brush(),
            BorderBrush = Line.Brush(),
            BorderThickness = new Thickness(1),
        };
        b.Click += (_, _) => onClick();
        return b;
    }

    private RadioButton Radio(string text, string group, Action onPick)
    {
        var r = new RadioButton
        {
            Content = text,
            GroupName = group,
            Foreground = Fg.Brush(),
            Margin = new Thickness(0, 4, 0, 0),
        };
        r.Checked += (_, _) => onPick();
        return r;
    }

    private Grid Row(string title, UIElement control, UIElement? trailing = null)
    {
        var g = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(118) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(trailing is null ? 0 : 92) });

        var label = Label(title);
        Grid.SetColumn(label, 0);
        g.Children.Add(label);

        Grid.SetColumn(control, 1);
        g.Children.Add(control);

        if (trailing is not null)
        {
            Grid.SetColumn(trailing, 2);
            g.Children.Add(trailing);
        }
        return g;
    }

    private Grid SliderRow(string title, double min, double max, double step,
        Func<double> get, Action<double> set, Func<double, string> format, bool live = false)
    {
        var slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            TickFrequency = step,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };
        var value = new TextBlock
        {
            Foreground = Dim.Brush(),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Consolas, Segoe UI"),
            FontSize = 12,
        };

        slider.ValueChanged += (_, e) =>
        {
            value.Text = format(e.NewValue);
            if (_syncing) return;
            set(e.NewValue);
            value.Text = format(slider.Value);
        };

        void Pull()
        {
            slider.Value = get();
            value.Text = format(slider.Value);
        }
        // Rows rebuilt on the fly (the dwell block) sync themselves right away
        // instead of joining the shared list, which would leak entries.
        if (live) Pull(); else _sync.Add(Pull);

        return Row(title, slider, value);
    }

    private Grid ComboRow(string title, List<object> items, Func<int> get, Action<int> set)
    {
        var combo = new ComboBox
        {
            ItemsSource = items,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Colors.Black.Brush(),
        };
        combo.SelectionChanged += (_, _) =>
        {
            if (_syncing || combo.SelectedIndex < 0) return;
            set(combo.SelectedIndex);
        };
        _sync.Add(() => combo.SelectedIndex = get());
        return Row(title, combo);
    }

    /// WPF ships no color picker, so: a swatch, a hex field, and R/G/B.
    private UIElement ColorRow(string title, Func<string> get, Action<string> set)
    {
        var swatch = new Border
        {
            Width = 34,
            Height = 22,
            CornerRadius = new CornerRadius(5),
            BorderBrush = Line.Brush(),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var hex = new TextBox
        {
            Width = 84,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            FontFamily = new FontFamily("Consolas, Segoe UI"),
            Background = Color.FromRgb(0x2A, 0x2A, 0x31).Brush(),
            Foreground = Fg.Brush(),
            BorderBrush = Line.Brush(),
        };

        var channels = new Slider[3];
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        strip.Children.Add(swatch);
        strip.Children.Add(hex);

        void Push(Color c)
        {
            if (_syncing) return;
            set(c.ToHex());
        }

        for (int i = 0; i < 3; i++)
        {
            var s = new Slider
            {
                Minimum = 0,
                Maximum = 255,
                Width = 76,
                IsSnapToTickEnabled = true,
                TickFrequency = 1,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                ToolTip = new[] { "Red", "Green", "Blue" }[i],
            };
            channels[i] = s;
            strip.Children.Add(s);
        }

        void FromChannels()
        {
            if (_syncing) return;
            var c = Color.FromRgb((byte)channels[0].Value, (byte)channels[1].Value, (byte)channels[2].Value);
            hex.Text = c.ToHex();
            swatch.Background = c.Brush();
            Push(c);
        }
        foreach (var s in channels) s.ValueChanged += (_, _) => FromChannels();

        hex.LostFocus += (_, _) => CommitHex();
        hex.KeyDown += (_, e) => { if (e.Key == Key.Enter) CommitHex(); };

        void CommitHex()
        {
            if (_syncing) return;
            var c = ColorX.FromHex(hex.Text);
            hex.Text = c.ToHex();
            swatch.Background = c.Brush();
            _syncing = true;
            channels[0].Value = c.R; channels[1].Value = c.G; channels[2].Value = c.B;
            _syncing = false;
            Push(c);
        }

        _sync.Add(() =>
        {
            var c = ColorX.FromHex(get());
            hex.Text = c.ToHex();
            swatch.Background = c.Brush();
            channels[0].Value = c.R; channels[1].Value = c.G; channels[2].Value = c.B;
        });

        var stack = new StackPanel { Margin = new Thickness(0, 4, 0, 6) };
        stack.Children.Add(new TextBlock { Text = title, Foreground = Fg.Brush() });
        strip.Margin = new Thickness(0, 5, 0, 0);
        stack.Children.Add(strip);
        return stack;
    }

    private static void Open(string? path)
    {
        if (path is null) return;
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
    }

    private static double NearestIntervalIndex(double target)
    {
        int best = 0;
        for (int i = 1; i < Intervals.Length; i++)
            if (Math.Abs(Intervals[i] - target) < Math.Abs(Intervals[best] - target)) best = i;
        return best;
    }

    public static string HumanDuration(double v)
    {
        if (v < 60) return (int)v + "s";
        if (v < 3600)
        {
            int m = (int)v / 60, sec = (int)v % 60;
            return sec == 0 ? m + "m" : m + "m " + sec + "s";
        }
        int h = (int)v / 3600, mm = ((int)v % 3600) / 60;
        return mm == 0 ? h + "h" : h + "h " + mm + "m";
    }
}
