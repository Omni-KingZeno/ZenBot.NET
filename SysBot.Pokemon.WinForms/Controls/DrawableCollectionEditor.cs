using System.ComponentModel.Design;

namespace SysBot.Pokemon.WinForms;

public class DrawableCollectionEditor(Type type) : CollectionEditor(type)
{
    public static readonly Type[] CollectionContainers =
    [
        typeof(RemoteControlAccessList),
        typeof(LegalitySettings),
    ];

    public static readonly Type[] SupportedCollections =
    [
        typeof(RemoteControlAccess),
        typeof(PKHeX.Core.GameVersion),
        typeof(PKHeX.Core.EncounterTypeGroup)
    ];

    protected override CollectionForm CreateCollectionForm()
    {
        var form = base.CreateCollectionForm();
        if (Program.IsDarkTheme)
        {
            foreach (Control control in form.Controls)
                SetDarkrecursive(control);

            SetListBoxOwnerDraw(form);
        }
        return form;
    }

    private static void SetListBoxOwnerDraw(Control control)
    {
        foreach (Control c in control.Controls)
        {
            if (c is ListBox lb)
            {
                lb.DrawMode = DrawMode.OwnerDrawFixed;
                lb.DrawItem -= ListBox_DrawItemDark;
                lb.DrawItem += ListBox_DrawItemDark;
            }
            else
            {
                SetListBoxOwnerDraw(c);
            }
        }
    }

    private static void SetDarkrecursive(Control control)
    {
        if (control is Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DrawableButton.DarkNormalBackColor;
            btn.ForeColor = Color.White;
        }
        foreach (Control child in control.Controls)
            SetDarkrecursive(child);
    }

    private static void ListBox_DrawItemDark(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ListBox lb)
            return;

        if (e.Index < 0 || e.Index >= lb.Items.Count)
            return;

        e.DrawBackground();

        int iconSize = Math.Min(e.Bounds.Height - 4, 14);
        Rectangle iconRect = new(e.Bounds.Left + 2, e.Bounds.Top + (e.Bounds.Height - iconSize) / 2, iconSize, iconSize);

        using (var brush = new SolidBrush(Color.LightGray))
            e.Graphics.FillRectangle(brush, iconRect);

        using (var pen = new Pen(Color.Gray))
            e.Graphics.DrawRectangle(pen, iconRect);

        string cardinality = (e.Index + 1).ToString();
        using var numBrush = new SolidBrush(Color.Black);
        var numFont = e.Font!;
        var numSize = e.Graphics.MeasureString(cardinality, numFont);
        var numPos = new PointF(
            iconRect.Left + (iconRect.Width - numSize.Width) / 2,
            iconRect.Top + (iconRect.Height - numSize.Height) / 2
        );
        e.Graphics.DrawString(cardinality, numFont, numBrush, numPos);

        string text = lb.Items[e.Index]?.ToString() ?? string.Empty;
        int textOffset = iconRect.Right + 4;
        Rectangle textRect = new(textOffset, e.Bounds.Top, e.Bounds.Right - textOffset, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, text, e.Font, textRect, SystemColors.ControlText, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

        e.DrawFocusRectangle();
    }
}
