namespace SysBot.Pokemon.WinForms;

/// <summary>
/// Minimal modal progress dialog shown while an update downloads and installs.
/// </summary>
public sealed partial class UpdateProgressForm : Form
{
    public UpdateProgressForm()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (Owner is not null)
        {
            Location = new Point(
                Owner.Left + (Owner.Width - Width) / 2,
                Owner.Top + (Owner.Height - Height) / 2);
        }
        else
        {
            StartPosition = FormStartPosition.CenterScreen;
        }
    }

    /// <summary>
    /// Updates the progress bar. Pass null for indeterminate (e.g. content-length unknown),
    /// or 0-100 for a determinate percentage.
    /// </summary>
    public void ReportProgress(int? percent, string? status = null)
    {
        if (status is not null)
            StatusLabel.Text = status;

        if (percent is null)
        {
            ProgressBar.Style = ProgressBarStyle.Marquee;
        }
        else
        {
            ProgressBar.Style = ProgressBarStyle.Blocks;
            ProgressBar.Value = Math.Clamp(percent.Value, 0, 100);
        }
    }
}
