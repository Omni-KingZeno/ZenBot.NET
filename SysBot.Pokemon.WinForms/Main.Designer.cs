using SysBot.Pokemon.WinForms.Properties;

namespace SysBot.Pokemon.WinForms
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TC_Main = new DrawableTabControl();
            Tab_Bots = new System.Windows.Forms.TabPage();
            CB_Protocol = new System.Windows.Forms.ComboBox();
            FLP_Bots = new System.Windows.Forms.FlowLayoutPanel();
            TB_IP = new System.Windows.Forms.TextBox();
            CB_Routine = new System.Windows.Forms.ComboBox();
            NUD_Port = new System.Windows.Forms.TextBox();
            B_New = new SysBot.Pokemon.WinForms.DrawableButton();
            Tab_Hub = new System.Windows.Forms.TabPage();
            PG_Hub = new System.Windows.Forms.PropertyGrid();
            Tab_Logs = new System.Windows.Forms.TabPage();
            RTB_Logs = new System.Windows.Forms.RichTextBox();
            B_Stop = new SysBot.Pokemon.WinForms.DrawableButton();
            B_Start = new SysBot.Pokemon.WinForms.DrawableButton();
            B_RebootStop = new SysBot.Pokemon.WinForms.DrawableButton();
            TC_Main.SuspendLayout();
            Tab_Bots.SuspendLayout();
            Tab_Hub.SuspendLayout();
            Tab_Logs.SuspendLayout();
            SuspendLayout();
            // 
            // TC_Main
            // 
            TC_Main.Controls.Add(Tab_Bots);
            TC_Main.Controls.Add(Tab_Hub);
            TC_Main.Controls.Add(Tab_Logs);
            TC_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            TC_Main.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            TC_Main.Location = new System.Drawing.Point(0, 0);
            TC_Main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TC_Main.Name = "TC_Main";
            TC_Main.SelectedIndex = 0;
            TC_Main.Size = new System.Drawing.Size(712, 418);
            TC_Main.TabIndex = 3;
            // 
            // Tab_Bots
            // 
            Tab_Bots.Controls.Add(CB_Protocol);
            Tab_Bots.Controls.Add(FLP_Bots);
            Tab_Bots.Controls.Add(TB_IP);
            Tab_Bots.Controls.Add(CB_Routine);
            Tab_Bots.Controls.Add(NUD_Port);
            Tab_Bots.Controls.Add(B_New);
            Tab_Bots.Location = new System.Drawing.Point(4, 25);
            Tab_Bots.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Bots.Name = "Tab_Bots";
            Tab_Bots.Size = new System.Drawing.Size(704, 389);
            Tab_Bots.TabIndex = 0;
            Tab_Bots.Text = "Bots";
            Tab_Bots.UseVisualStyleBackColor = true;
            // 
            // CB_Protocol
            // 
            CB_Protocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CB_Protocol.FormattingEnabled = true;
            CB_Protocol.Location = new System.Drawing.Point(289, 6);
            CB_Protocol.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CB_Protocol.Name = "CB_Protocol";
            CB_Protocol.Size = new System.Drawing.Size(67, 23);
            CB_Protocol.TabIndex = 10;
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            // 
            // FLP_Bots
            // 
            FLP_Bots.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            FLP_Bots.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            FLP_Bots.Location = new System.Drawing.Point(0, 37);
            FLP_Bots.Margin = new System.Windows.Forms.Padding(0);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Size = new System.Drawing.Size(704, 350);
            FLP_Bots.TabIndex = 9;
            FLP_Bots.Resize += FLP_Bots_Resize;
            // 
            // TB_IP
            // 
            TB_IP.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TB_IP.Location = new System.Drawing.Point(74, 8);
            TB_IP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TB_IP.Name = "TB_IP";
            TB_IP.Size = new System.Drawing.Size(134, 20);
            TB_IP.TabIndex = 8;
            TB_IP.Text = "192.168.0.1";
            // 
            // CB_Routine
            // 
            CB_Routine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            CB_Routine.FormattingEnabled = true;
            CB_Routine.Location = new System.Drawing.Point(364, 6);
            CB_Routine.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CB_Routine.Name = "CB_Routine";
            CB_Routine.Size = new System.Drawing.Size(117, 23);
            CB_Routine.TabIndex = 7;
            // 
            // NUD_Port
            // 
            NUD_Port.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            NUD_Port.Location = new System.Drawing.Point(215, 8);
            NUD_Port.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NUD_Port.Name = "NUD_Port";
            NUD_Port.Size = new System.Drawing.Size(67, 20);
            NUD_Port.TabIndex = 6;
            NUD_Port.Text = "6000";
            // 
            // B_New
            // 
            B_New.Location = new System.Drawing.Point(4, 7);
            B_New.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            B_New.Name = "B_New";
            B_New.Size = new System.Drawing.Size(63, 23);
            B_New.TabIndex = 0;
            B_New.Text = "Add";
            B_New.UseVisualStyleBackColor = true;
            B_New.Click += B_New_Click;
            // 
            // Tab_Hub
            // 
            Tab_Hub.Controls.Add(PG_Hub);
            Tab_Hub.Location = new System.Drawing.Point(4, 25);
            Tab_Hub.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Hub.Name = "Tab_Hub";
            Tab_Hub.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Hub.Size = new System.Drawing.Size(704, 389);
            Tab_Hub.TabIndex = 2;
            Tab_Hub.Text = "Hub";
            Tab_Hub.UseVisualStyleBackColor = true;
            // 
            // PG_Hub
            // 
            PG_Hub.BackColor = System.Drawing.SystemColors.Control;
            PG_Hub.Dock = System.Windows.Forms.DockStyle.Fill;
            PG_Hub.Location = new System.Drawing.Point(4, 3);
            PG_Hub.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PG_Hub.Name = "PG_Hub";
            PG_Hub.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            PG_Hub.Size = new System.Drawing.Size(696, 383);
            PG_Hub.TabIndex = 0;
            // 
            // Tab_Logs
            // 
            Tab_Logs.Controls.Add(RTB_Logs);
            Tab_Logs.Location = new System.Drawing.Point(4, 25);
            Tab_Logs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Tab_Logs.Name = "Tab_Logs";
            Tab_Logs.Size = new System.Drawing.Size(704, 389);
            Tab_Logs.TabIndex = 1;
            Tab_Logs.Text = "Logs";
            Tab_Logs.UseVisualStyleBackColor = true;
            // 
            // RTB_Logs
            // 
            RTB_Logs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            RTB_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
            RTB_Logs.HideSelection = false;
            RTB_Logs.Location = new System.Drawing.Point(0, 0);
            RTB_Logs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new System.Drawing.Size(704, 389);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            // 
            // B_Stop
            // 
            B_Stop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            B_Stop.Location = new System.Drawing.Point(495, 0);
            B_Stop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            B_Stop.Name = "B_Stop";
            B_Stop.Size = new System.Drawing.Size(69, 22);
            B_Stop.TabIndex = 4;
            B_Stop.Text = "Stop All";
            B_Stop.UseVisualStyleBackColor = true;
            B_Stop.Click += B_Stop_Click;
            // 
            // B_Start
            // 
            B_Start.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            B_Start.Location = new System.Drawing.Point(412, 0);
            B_Start.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            B_Start.Name = "B_Start";
            B_Start.Size = new System.Drawing.Size(69, 22);
            B_Start.TabIndex = 3;
            B_Start.Text = "Start All";
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // B_RebootStop
            // 
            B_RebootStop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            B_RebootStop.Location = new System.Drawing.Point(578, 0);
            B_RebootStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            B_RebootStop.Name = "B_RebootStop";
            B_RebootStop.Size = new System.Drawing.Size(121, 22);
            B_RebootStop.TabIndex = 3;
            B_RebootStop.Text = "Reboot And Stop";
            B_RebootStop.UseVisualStyleBackColor = true;
            B_RebootStop.Click += B_RebootStop_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(712, 418);
            Controls.Add(B_Stop);
            Controls.Add(B_Start);
            Controls.Add(B_RebootStop);
            Controls.Add(TC_Main);
            Icon = Resources.icon;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Main";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "ManuBot: Pokémon";
            FormClosing += Main_FormClosing;
            TC_Main.ResumeLayout(false);
            Tab_Bots.ResumeLayout(false);
            Tab_Bots.PerformLayout();
            Tab_Hub.ResumeLayout(false);
            Tab_Logs.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private SysBot.Pokemon.WinForms.DrawableTabControl TC_Main;
        private System.Windows.Forms.TabPage Tab_Bots;
        private System.Windows.Forms.TabPage Tab_Logs;
        private System.Windows.Forms.RichTextBox RTB_Logs;
        private System.Windows.Forms.TabPage Tab_Hub;
        private System.Windows.Forms.PropertyGrid PG_Hub;
        private SysBot.Pokemon.WinForms.DrawableButton B_Stop;
        private SysBot.Pokemon.WinForms.DrawableButton B_Start;
        private SysBot.Pokemon.WinForms.DrawableButton B_RebootStop;
        private System.Windows.Forms.TextBox TB_IP;
        private System.Windows.Forms.ComboBox CB_Routine;
        private System.Windows.Forms.TextBox NUD_Port;
        private SysBot.Pokemon.WinForms.DrawableButton B_New;
        private System.Windows.Forms.FlowLayoutPanel FLP_Bots;
        private System.Windows.Forms.ComboBox CB_Protocol;
    }
}

