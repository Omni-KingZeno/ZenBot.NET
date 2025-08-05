using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms;

public class DrawableButton : Button
{
    private bool _hovered = false;

    public static Color NormalBackColor { get; set; } = Color.FromArgb(48, 48, 48);
    public static Color HoverBackColor { get; set; } = Color.FromArgb(64, 64, 64);
    public static Color BorderColor { get; set; } = Color.FromArgb(100, 100, 100);
    public static int BorderRadius { get; set; } = 5;
    public static int BorderThickness { get; set; } = 1;

    public DrawableButton()
    {
        if (Program.IsDarkTheme)
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = NormalBackColor;
            ForeColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Default;
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (Program.IsDarkTheme)
        {
            _hovered = true;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (Program.IsDarkTheme)
        {
            _hovered = false;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!Program.IsDarkTheme)
        {
            base.OnPaint(e);
            return;
        }

        Color parentBackColor = Parent?.BackColor ?? Color.FromArgb(32, 32, 32);
        using (var bg = new SolidBrush(parentBackColor))
            e.Graphics.FillRectangle(bg, ClientRectangle);

        RectangleF borderRect = new(
            ClientRectangle.X + 0.5f,
            ClientRectangle.Y + 0.5f,
            ClientRectangle.Width - 1f,
            ClientRectangle.Height - 1f
        );

        using var path = RoundedRect(borderRect, BorderRadius);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (var brush = new SolidBrush(_hovered ? HoverBackColor : NormalBackColor))
            e.Graphics.FillPath(brush, path);

        using (var pen = new Pen(BorderColor, 1f))
            e.Graphics.DrawPath(pen, path);

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            ClientRectangle,
            ForeColor,
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
