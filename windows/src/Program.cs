using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;

namespace GrindBot;

/// The AppKit AppDelegate, rewritten: tray icon instead of a status item,
/// DispatcherTimers instead of NSTimers, everything else the same shape.
public sealed class Program : System.Windows.Application
{
    private readonly Settings _settings = new(Config.Load());
    private readonly MessageDeck _deck = new();

    private PopupWindow _popup = null!;
    private WinForms.NotifyIcon _tray = null!;
    private WinForms.ToolStripMenuItem _pauseItem = null!;
    private readonly List<WinForms.ToolStripMenuItem> _faceItems = new();
    private WinForms.ToolStripMenuItem _idleItem = null!;
    private DispatcherTimer _timer = null!;
    private DispatcherTimer _idlePoll = null!;
    private DispatcherTimer _saveDebounce = null!;
    private bool _pending;          // a scheduled message is waiting for you to go quiet
    private SettingsWindow? _settingsWindow;
    private Config? _applied;
    private bool _paused;
    private System.Drawing.Icon? _trayIcon;

    [STAThread]
    public static int Main()
    {
        // One robot is enough.
        using var single = new Mutex(true, @"Local\GrindBot.SingleInstance", out bool fresh);
        if (!fresh)
        {
            WinForms.MessageBox.Show("GrindBot is already running — look for the robot in your tray.",
                "GrindBot", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
            return 0;
        }

        // Before `new Program()`: the Config.Load() field initializer needs the
        // files on disk already.
        Paths.SeedUserFiles();

        WinForms.Application.EnableVisualStyles();
        WinForms.Application.SetCompatibleTextRenderingDefault(false);

        var app = new Program { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        return app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            WinForms.MessageBox.Show(args.Exception.ToString(), "GrindBot crashed",
                WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
            Log.Write("UNHANDLED " + args.Exception);
            args.Handled = true;
        };

        _deck.Reload(_settings.Cfg);
        Log.Write($"loaded {Paths.NextToApp("config.json")} — {_deck.Count} messages");

        _popup = new PopupWindow(_settings);
        _popup.Show();

        BuildTray();
        _applied = _settings.Cfg.Clone();
        ObserveSettings();
        StartTimer();
        SyncIdlePoll(_settings.Cfg);

        SystemEvents.DisplaySettingsChanged += (_, _) =>
            Dispatcher.BeginInvoke(new Action(() => _popup.Reposition()), DispatcherPriority.Background);

        // `GrindBot.exe --settings` opens the settings window straight away,
        // which beats hunting for the tray icon on a fresh install.
        if (Environment.GetCommandLineArgs()
            .Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase)))
            ShowSettings();

        var kickoff = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.2) };
        kickoff.Tick += (_, _) =>
        {
            kickoff.Stop();
            if (!_paused) Scheduled();
        };
        kickoff.Start();
    }

    // MARK: - Live settings

    private void ObserveSettings()
    {
        _saveDebounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _saveDebounce.Tick += (_, _) =>
        {
            _saveDebounce.Stop();
            _settings.SaveError = _settings.Cfg.Save();
            _settingsWindow?.ShowSaveError(_settings.SaveError);
        };

        _settings.Changed += cfg =>
        {
            Apply(cfg);
            _saveDebounce.Stop();
            _saveDebounce.Start();
        };
    }

    private void Apply(Config cfg)
    {
        var old = _applied;
        _applied = cfg.Clone();

        if (old is null) { _popup.Rebuild(cfg); _deck.Reload(cfg); RefreshTray(cfg); return; }

        bool looksDifferent =
            old.scale != cfg.scale || old.position != cfg.position
            || old.face != cfg.face || old.accent != cfg.accent
            || old.shell != cfg.shell || old.visor != cfg.visor
            || old.margin != cfg.margin || old.maxBubbleWidth != cfg.maxBubbleWidth
            || old.theme != cfg.theme;

        if (looksDifferent) _popup.Rebuild(cfg);
        else if (old.screenIndex != cfg.screenIndex) _popup.Reposition(cfg);

        if (old.messagesFile != cfg.messagesFile || old.shuffle != cfg.shuffle)
            _deck.Reload(cfg);

        // Only on a real change — otherwise every slider tick restarts the countdown.
        if (old.intervalSeconds != cfg.intervalSeconds && !_paused)
            StartTimer(cfg);

        if (old.accent != cfg.accent || old.shell != cfg.shell)
            RefreshTray(cfg);

        if (old.idleOnly != cfg.idleOnly || old.idleSeconds != cfg.idleSeconds)
            SyncIdlePoll(cfg);

        _idleItem.Checked = cfg.idleOnly;

        foreach (var item in _faceItems)
            item.Checked = (string?)item.Tag == cfg.face.ToString();

        _settingsWindow?.SyncFromConfig();
    }

    // MARK: - Tray

    private void BuildTray()
    {
        var menu = new WinForms.ContextMenuStrip { ShowImageMargin = false };

        menu.Items.Add(Item("Settings…", (_, _) => ShowSettings()));
        menu.Items.Add(Item("Say something now", (_, _) => Next()));
        _pauseItem = Item("Turn off", (_, _) => TogglePause());
        menu.Items.Add(_pauseItem);

        var faceRoot = new WinForms.ToolStripMenuItem("Face");
        foreach (Face face in Enum.GetValues<Face>())
        {
            var name = face.ToString();
            var item = Item(char.ToUpper(name[0]) + name.Substring(1), (s, _) =>
            {
                var picked = (Face)Enum.Parse(typeof(Face), (string)((WinForms.ToolStripMenuItem)s!).Tag!);
                _settings.Mutate(c => c.face = picked);
            });
            item.Tag = name;
            item.Checked = face == _settings.Cfg.face;
            faceRoot.DropDownItems.Add(item);
            _faceItems.Add(item);
        }
        menu.Items.Add(faceRoot);

        _idleItem = Item("Only when I'm idle", (_, _) =>
            _settings.Mutate(c => c.idleOnly = !c.idleOnly));
        _idleItem.Checked = _settings.Cfg.idleOnly;
        _idleItem.ToolTipText = "Hold messages until you've stopped typing, clicking and scrolling";
        menu.Items.Add(_idleItem);

        menu.Items.Add(new WinForms.ToolStripSeparator());
        var startup = new WinForms.ToolStripMenuItem("Start with Windows") { Checked = RunAtLogin.Enabled };
        startup.Click += (_, _) =>
        {
            var error = RunAtLogin.Set(!startup.Checked);
            if (error is not null)
                WinForms.MessageBox.Show("Couldn't change the startup setting:\n" + error, "GrindBot");
            startup.Checked = RunAtLogin.Enabled;
        };
        menu.Items.Add(startup);

        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(Item("Reload config && messages", (_, _) => Reload()));
        menu.Items.Add(Item("Edit messages…", (_, _) => OpenFile(_settings.Cfg.messagesFile)));
        menu.Items.Add(Item("Edit config.json…", (_, _) =>
        {
            // Nothing saved yet? Write it first, so the item never no-ops.
            if (Paths.NextToApp("config.json") is null) _settings.Cfg.Save();
            OpenFile("config.json");
        }));

        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(Item("Quit", (_, _) => Quit()));

        _tray = new WinForms.NotifyIcon
        {
            Text = "GrindBot",
            ContextMenuStrip = menu,
            Visible = true,
        };
        RefreshTray(_settings.Cfg);

        // Left click opens the same menu; double click fires a message.
        _tray.MouseClick += (_, e) =>
        {
            if (e.Button == WinForms.MouseButtons.Left) menu.Show(WinForms.Cursor.Position);
        };
        _tray.DoubleClick += (_, _) => Next();
    }

    private static WinForms.ToolStripMenuItem Item(string text, EventHandler onClick)
    {
        var item = new WinForms.ToolStripMenuItem(text);
        item.Click += onClick;
        return item;
    }

    private void RefreshTray(Config cfg)
    {
        var old = _trayIcon;
        _trayIcon = TrayArt.Make(cfg.AccentColor, ColorX.FromHex(cfg.shell), _paused);
        _tray.Icon = _trayIcon;
        _tray.Text = _paused ? "GrindBot — off" : "GrindBot";
        old?.Dispose();
    }

    // MARK: - Speaking

    private void StartTimer(Config? cfg = null)
    {
        cfg ??= _settings.Cfg;
        _timer?.Stop();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, cfg.intervalSeconds)) };
        _timer.Tick += (_, _) => Scheduled();
        _timer.Start();
    }

    /// A message the timer asked for. Held back while you're still working, so
    /// the robot interrupts a lull rather than a sentence. Anything you ask for
    /// by hand — the menu, the settings button, unpausing — skips this.
    private void Scheduled()
    {
        if (Gated())
        {
            Log.Write($"held — idle {Idle.Seconds():0.0}s of {_settings.Cfg.idleSeconds:0}s");
            _pending = true;
            return;
        }
        Next();
    }

    private bool Gated()
    {
        var cfg = _settings.Cfg;
        return cfg.idleOnly && Idle.Seconds() < cfg.idleSeconds;
    }

    /// Only runs while the gate is on. The moment you go quiet, the held
    /// message lands, and the interval re-spaces from there rather than
    /// firing again a second later.
    private void SyncIdlePoll(Config cfg)
    {
        _idlePoll?.Stop();
        if (!cfg.idleOnly) { _pending = false; return; }

        _idlePoll = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _idlePoll.Tick += (_, _) =>
        {
            if (!_pending || _paused || Gated()) return;
            Log.Write($"released — idle {Idle.Seconds():0.0}s");
            _pending = false;
            StartTimer();
            Next();
        };
        _idlePoll.Start();
    }

    private void Next()
    {
        var message = _deck.Next();
        if (message is null)
        {
            _popup.Say($"{_settings.Cfg.messagesFile} is empty — add one line per message.");
            return;
        }
        _popup.Say(message);
    }

    // MARK: - Actions

    /// Off = no timer, nothing on screen. On = fire one now, then resume.
    private void TogglePause()
    {
        _paused = !_paused;
        _pauseItem.Text = _paused ? "Turn on" : "Turn off";
        RefreshTray(_settings.Cfg);
        if (_paused)
        {
            _timer?.Stop();
            _pending = false;
            _popup.HideNow();
        }
        else
        {
            StartTimer();
            Next();
        }
    }

    private void Reload()
    {
        _settings.Cfg = Config.Load();       // fires Apply through Changed
        _deck.Reload(_settings.Cfg);
        _popup.Rebuild(_settings.Cfg);
        StartTimer();
        Next();
    }

    private void OpenFile(string name)
    {
        var path = Paths.NextToApp(name);
        if (path is null) return;
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception e)
        {
            WinForms.MessageBox.Show($"Couldn't open {path}:\n{e.Message}", "GrindBot");
        }
    }

    public void ShowSettings()
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(_settings, onPreview: Next);
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        _settingsWindow.Show();
        if (_settingsWindow.WindowState == WindowState.Minimized)
            _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    private void Quit()
    {
        _tray.Visible = false;
        _tray.Dispose();
        _trayIcon?.Dispose();
        Shutdown();
    }
}
