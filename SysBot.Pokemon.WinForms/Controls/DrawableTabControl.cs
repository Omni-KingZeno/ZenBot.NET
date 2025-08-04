using System.Drawing;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms;

public class DrawableTabControl : TabControl
{
    public DrawableTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        bool dark = Program.IsDarkTheme;

        Color stripColor = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Color pageColor = dark ? Color.FromArgb(32, 32, 32) : Color.White;
        Pen borderPen = dark ? Pens.Black : Pens.Gray;

        // Strip
        int tabHeight = ItemSize.Height;
        Rectangle stripRect = new(0, 0, Width, tabHeight + 2);
        using (var b = new SolidBrush(stripColor))
            e.Graphics.FillRectangle(b, stripRect);

        // Tabs
        for (int i = 0; i < TabCount; i++)
            DrawTab(e.Graphics, i, dark);

        // Lower border
        e.Graphics.DrawLine(borderPen, 0, tabHeight + 1, Width, tabHeight + 1);

        // Page background
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

        // Tab Colors
        Color tabColor = dark
            ? (selected ? Color.FromArgb(48, 48, 48) : Color.FromArgb(32, 32, 32))
            : (selected ? Color.FromArgb(224, 224, 224) : Color.White);

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
}
