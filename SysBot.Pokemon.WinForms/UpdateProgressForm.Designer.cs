namespace SysBot.Pokemon.WinForms
{
    partial class UpdateProgressForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            LayoutPanel = new TableLayoutPanel();
            StatusLabel = new Label();
            ProgressBar = new ProgressBar();
            LayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // LayoutPanel
            // 
            LayoutPanel.ColumnCount = 1;
            LayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            LayoutPanel.Controls.Add(StatusLabel, 0, 0);
            LayoutPanel.Controls.Add(ProgressBar, 0, 1);
            LayoutPanel.Dock = DockStyle.Fill;
            LayoutPanel.Location = new Point(0, 0);
            LayoutPanel.Name = "LayoutPanel";
            LayoutPanel.Padding = new Padding(12, 12, 12, 20);
            LayoutPanel.RowCount = 2;
            LayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 58.3333321F));
            LayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 41.6666679F));
            LayoutPanel.Size = new Size(400, 118);
            LayoutPanel.TabIndex = 0;
            // 
            // StatusLabel
            // 
            StatusLabel.Dock = DockStyle.Fill;
            StatusLabel.Location = new Point(12, 12);
            StatusLabel.Margin = new Padding(0, 0, 0, 8);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(376, 42);
            StatusLabel.TabIndex = 0;
            StatusLabel.Text = "Downloading update...";
            StatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ProgressBar
            // 
            ProgressBar.Dock = DockStyle.Fill;
            ProgressBar.Location = new Point(15, 65);
            ProgressBar.MarqueeAnimationSpeed = 30;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(370, 30);
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 1;
            // 
            // UpdateProgressForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 118);
            ControlBox = false;
            Controls.Add(LayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateProgressForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Updating ZenBot";
            LayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel LayoutPanel;
        private Label StatusLabel;
        private ProgressBar ProgressBar;
    }
}
