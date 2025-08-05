using System;
using System.Drawing;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms;

public class DrawableTabControl : TabControl
{
    private int _hoveredTabIndex = -1;

    public DrawableTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        SetStyle(ControlStyles.UserPaint, true);
        this.MouseMove += DrawableTabControl_MouseMove;
        this.MouseLeave += DrawableTabControl_MouseLeave;
    }

    private void DrawableTabControl_MouseMove(object? sender, MouseEventArgs e)
    {
        for (int i = 0; i < TabCount; i++)
        {
            if (GetTabRect(i).Contains(e.Location))
            {
                if (_hoveredTabIndex != i)
                {
                    _hoveredTabIndex = i;
                    Invalidate();
                }
                return;
            }
        }
        if (_hoveredTabIndex != -1)
        {
            _hoveredTabIndex = -1;
            Invalidate();
        }
    }

    private void DrawableTabControl_MouseLeave(object? sender, EventArgs e)
    {
        if (_hoveredTabIndex != -1)
        {
            _hoveredTabIndex = -1;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        bool dark = Program.IsDarkTheme;

        Color stripColor = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Color pageColor = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Pen borderPen = dark ? Pens.Black : Pens.Gray;

        int tabHeight = ItemSize.Height;
        Rectangle stripRect = new(0, 0, Width, tabHeight + 2);
        using (var b = new SolidBrush(stripColor))
            e.Graphics.FillRectangle(b, stripRect);

        for (int i = 0; i < TabCount; i++)
            DrawTab(e.Graphics, i, dark);

        e.Graphics.DrawLine(borderPen, 0, tabHeight + 1, Width, tabHeight + 1);

        if (SelectedTab != null)
        {
            Rectangle pageRect = DisplayRectangle;
            pageRect.Inflate(2, 2);
            using var pageBrush = new SolidBrush(pageColor);
            e.Graphics.FillRectangle(pageBrush, pageRect);
        }
    }

    private void DrawTab(Graphics g, int index, bool dark)
    {
        Rectangle rect = GetTabRect(index);
        bool selected = (index == SelectedIndex);
        bool hovered = (index == _hoveredTabIndex);

        Color tabColor = dark
            ? (selected ? Color.FromArgb(48, 48, 48) : Color.FromArgb(32, 32, 32))
            : (selected ? Color.FromArgb(224, 224, 224) : Color.White);

        if (hovered && !selected)
            tabColor = dark ? Lighten(tabColor, 0.15f) : Color.FromArgb(240, 240, 240);

        Color textColor = dark ? Color.White : Color.Black;

        using (var b = new SolidBrush(tabColor))
            g.FillRectangle(b, rect);

        TextRenderer.DrawText(
            g,
            TabPages[index].Text,
            Font,
            rect,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        );
    }

    private static Color Lighten(Color color, float amount)
    {
        int r = color.R + (int)((255 - color.R) * amount);
        int g = color.G + (int)((255 - color.G) * amount);
        int b = color.B + (int)((255 - color.B) * amount);
        return Color.FromArgb(color.A, Math.Min(r, 255), Math.Min(g, 255), Math.Min(b, 255));
    }
}
