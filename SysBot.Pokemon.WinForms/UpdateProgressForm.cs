namespace SysBot.Pokemon.WinForms;

/// <summary>
/// Minimal modal progress dialog shown while an update downloads and installs.
/// </summary>
public sealed class UpdateProgressForm : Form
{
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;

    public UpdateProgressForm()
    {
        Text = "Updating ZenBot";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.Manual;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ClientSize = new Size(360, 90);

        _statusLabel = new Label
        {
            Text = "Downloading update...",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(12, 12),
            Size = new Size(336, 20),
        };

        _progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Style = ProgressBarStyle.Marquee, // switches to Blocks once real progress is known
            MarqueeAnimationSpeed = 30,
            Location = new Point(12, 40),
            Size = new Size(336, 24),
        };

        Controls.Add(_statusLabel);
        Controls.Add(_progressBar);
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
            _statusLabel.Text = status;

        if (percent is null)
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
        }
        else
        {
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Value = Math.Clamp(percent.Value, 0, 100);
        }
    }
}
