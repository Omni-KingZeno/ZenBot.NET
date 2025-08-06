using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms;

public class DrawableButton : Button
{
    private bool _hovered = false;

    public static Color DarkNormalBackColor { get; set; } = Color.FromArgb(48, 48, 48);
    public static Color DarkHoverBackColor { get; set; } = Color.FromArgb(64, 64, 64);
    public static Color DarkBorderColor { get; set; } = Color.FromArgb(100, 100, 100);
    public static Color DarkForeColor { get; set; } = Color.White;

    public static Color LightNormalBackColor { get; set; } = Color.White;
    public static Color LightHoverBackColor { get; set; } = Color.FromArgb(230, 230, 230);
    public static Color LightBorderColor { get; set; } = Color.FromArgb(180, 180, 180);
    public static Color LightForeColor { get; set; } = Color.Black;

    public static int BorderRadius { get; set; } = 5;
    public static int BorderThickness { get; set; } = 1;

    public DrawableButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Cursor = Cursors.Default;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        bool dark = Program.IsDarkTheme;
        Color normalBack = dark ? DarkNormalBackColor : LightNormalBackColor;
        Color hoverBack = dark ? DarkHoverBackColor : LightHoverBackColor;
        Color borderColor = dark ? DarkBorderColor : Color.FromArgb(160, 160, 160);
        Color foreColor = dark ? DarkForeColor : LightForeColor;
        Color parentBackColor = Parent?.BackColor ?? (dark ? Color.FromArgb(32, 32, 32) : Color.White);

        RectangleF borderRect = new(
            ClientRectangle.X + 0.5f,
            ClientRectangle.Y + 0.5f,
            ClientRectangle.Width - 1f,
            ClientRectangle.Height - 1f
        );

        using var path = RoundedRect(borderRect, BorderRadius);
        Region = new Region(path);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(parentBackColor))
            e.Graphics.FillRectangle(bg, ClientRectangle);

        using (var brush = new SolidBrush(_hovered ? hoverBack : normalBack))
            e.Graphics.FillPath(brush, path);

        if (dark)
        {
            using var pen = new Pen(borderColor, BorderThickness);
            e.Graphics.DrawPath(pen, path);
        }
        else
        {
            using (var penShadow = new Pen(Color.FromArgb(180, 180, 180), 1f))
                e.Graphics.DrawPath(penShadow, path);

            using var penHighlight = new Pen(Color.FromArgb(220, 220, 220), 1f);
            float r = BorderRadius;
            e.Graphics.DrawArc(penHighlight, borderRect.X, borderRect.Y, r * 2, r * 2, 180, 90);
            e.Graphics.DrawArc(penHighlight, borderRect.X, borderRect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            e.Graphics.DrawLine(penHighlight, borderRect.X + r, borderRect.Y + 0.5f, borderRect.Right - r, borderRect.Y + 0.5f);
            e.Graphics.DrawLine(penHighlight, borderRect.X + 0.5f, borderRect.Y + r, borderRect.X + 0.5f, borderRect.Bottom - r);
        }

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            ClientRectangle,
            foreColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        );
    }

    private static GraphicsPath RoundedRect(RectangleF bounds, int radius)
    {
        float r = radius;
        var path = new GraphicsPath();
        if (r <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }
        path.AddArc(bounds.X, bounds.Y, r * 2, r * 2, 180, 90);
        path.AddArc(bounds.Right - r * 2, bounds.Y, r * 2, r * 2, 270, 90);
        path.AddArc(bounds.Right - r * 2, bounds.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}
