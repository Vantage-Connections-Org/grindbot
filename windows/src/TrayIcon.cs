using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using D = System.Drawing;
using Media = System.Windows.Media;

namespace GrindBot;

/// The 🤖 in the menu bar becomes a drawn tray icon here — same robot, same
/// accent and shell colors, so recoloring the bot recolors the icon too.
internal static class TrayArt
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr handle);

    public static Icon Make(Media.Color accentM, Media.Color shellM, bool asleep)
    {
        var accent = D.Color.FromArgb(accentM.R, accentM.G, accentM.B);
        var shell = D.Color.FromArgb(shellM.R, shellM.G, shellM.B);
        if (asleep)
        {
            accent = D.Color.FromArgb(120, 130, 140);
            shell = D.Color.FromArgb(150, 152, 158);
        }

        const int N = 32;
        using var bmp = new Bitmap(N, N);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(D.Color.Transparent);

            // Antenna.
            using (var pen = new Pen(accent, 2.5f)) g.DrawLine(pen, 16, 3, 16, 8);
            using (var b = new SolidBrush(accent)) g.FillEllipse(b, 13, 0.5f, 5.5f, 5.5f);

            // Head.
            using (var b = new SolidBrush(shell))
            using (var path = Rounded(new RectangleF(4, 7, 24, 20), 6))
                g.FillPath(b, path);

            // Visor.
            using (var b = new SolidBrush(D.Color.FromArgb(255, 28, 28, 30)))
            using (var path = Rounded(new RectangleF(7, 10.5f, 18, 13), 4))
                g.FillPath(b, path);

            // Eyes.
            using (var b = new SolidBrush(accent))
            {
                if (asleep)
                {
                    using var pen = new Pen(accent, 2f);
                    g.DrawLine(pen, 10, 17, 14.5f, 17);
                    g.DrawLine(pen, 17.5f, 17, 22, 17);
                }
                else
                {
                    g.FillEllipse(b, 10, 13.5f, 4.5f, 7);
                    g.FillEllipse(b, 17.5f, 13.5f, 4.5f, 7);
                }
            }

            // Shoulders, so the silhouette reads as a robot at 16px.
            using (var b = new SolidBrush(D.Color.FromArgb(255,
                Math.Max(0, shell.R - 40), Math.Max(0, shell.G - 40), Math.Max(0, shell.B - 40))))
            using (var path = Rounded(new RectangleF(8, 27, 16, 5), 2))
                g.FillPath(b, path);
        }

        var handle = bmp.GetHicon();
        try
        {
            // Clone off the handle so the icon survives DestroyIcon.
            using var temp = Icon.FromHandle(handle);
            return (Icon)temp.Clone();
        }
        finally { DestroyIcon(handle); }
    }

    private static GraphicsPath Rounded(RectangleF r, float radius)
    {
        float d = radius * 2;
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}
