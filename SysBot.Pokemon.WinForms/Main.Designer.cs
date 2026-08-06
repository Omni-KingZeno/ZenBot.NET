using SysBot.Pokemon.WinForms;
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
            Tab_Bots = new TabPage();
            FLP_Bots = new FlowLayoutPanel();
            FLP_BotCreator = new FlowLayoutPanel();
            B_New = new DrawableButton();
            TB_IP = new TextBox();
            NUD_Port = new TextBox();
            CB_Protocol = new ComboBox();
            CB_Routine = new ComboBox();
            FLP_Line = new FlowLayoutPanel();
            Tab_Hub = new TabPage();
            PG_Hub = new PropertyGrid();
            B_UpdateCheck = new DrawableButton();
            Tab_Logs = new TabPage();
            RTB_Logs = new RichTextBox();
            B_Stop = new DrawableButton();
            B_Start = new DrawableButton();
            B_RebootStop = new DrawableButton();
            TC_Main.SuspendLayout();
            Tab_Bots.SuspendLayout();
            FLP_Bots.SuspendLayout();
            FLP_BotCreator.SuspendLayout();
            Tab_Hub.SuspendLayout();
            Tab_Logs.SuspendLayout();
            SuspendLayout();
            // 
            // TC_Main
            // 
            TC_Main.Controls.Add(Tab_Bots);
            TC_Main.Controls.Add(Tab_Hub);
            TC_Main.Controls.Add(Tab_Logs);
            TC_Main.Dock = DockStyle.Fill;
            TC_Main.DrawMode = TabDrawMode.OwnerDrawFixed;
            TC_Main.ItemSize = new Size(96, 32);
            TC_Main.Location = new Point(0, 0);
            TC_Main.Margin = new Padding(0);
            TC_Main.Name = "TC_Main";
            TC_Main.SelectedIndex = 0;
            TC_Main.Size = new Size(765, 248);
            TC_Main.SizeMode = TabSizeMode.Fixed;
            TC_Main.TabIndex = 3;
            // 
            // Tab_Bots
            // 
            Tab_Bots.Controls.Add(FLP_Bots);
            Tab_Bots.Location = new Point(4, 36);
            Tab_Bots.Margin = new Padding(4, 3, 4, 3);
            Tab_Bots.Name = "Tab_Bots";
            Tab_Bots.Size = new Size(757, 208);
            Tab_Bots.TabIndex = 0;
            Tab_Bots.Text = "Bots";
            Tab_Bots.UseVisualStyleBackColor = true;
            // 
            // FLP_Bots
            // 
            FLP_Bots.BorderStyle = BorderStyle.FixedSingle;
            FLP_Bots.Controls.Add(FLP_BotCreator);
            FLP_Bots.Controls.Add(FLP_Line);
            FLP_Bots.Dock = DockStyle.Fill;
            FLP_Bots.Location = new Point(0, 0);
            FLP_Bots.Margin = new Padding(0);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Size = new Size(757, 208);
            FLP_Bots.TabIndex = 9;
            FLP_Bots.Resize += FLP_Bots_Resize;
            // 
            // FLP_BotCreator
            // 
            FLP_BotCreator.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            FLP_BotCreator.BackColor = SystemColors.Control;
            FLP_BotCreator.Controls.Add(B_New);
            FLP_BotCreator.Controls.Add(TB_IP);
            FLP_BotCreator.Controls.Add(NUD_Port);
            FLP_BotCreator.Controls.Add(CB_Protocol);
            FLP_BotCreator.Controls.Add(CB_Routine);
            FLP_Bots.SetFlowBreak(FLP_BotCreator, true);
            FLP_BotCreator.Location = new Point(0, 0);
            FLP_BotCreator.Margin = new Padding(0);
            FLP_BotCreator.Name = "FLP_BotCreator";
            FLP_BotCreator.Size = new Size(65535, 33);
            FLP_BotCreator.TabIndex = 12;
            // 
            // B_New
            // 
            B_New.FlatStyle = FlatStyle.Flat;
            B_New.Location = new Point(4, 4);
            B_New.Margin = new Padding(4);
            B_New.Name = "B_New";
            B_New.Size = new Size(63, 25);
            B_New.TabIndex = 0;
            B_New.Text = "Add";
            B_New.UseVisualStyleBackColor = true;
            B_New.Click += B_New_Click;
            // 
            // TB_IP
            // 
            TB_IP.Location = new Point(71, 4);
            TB_IP.Margin = new Padding(0, 4, 4, 4);
            TB_IP.Name = "TB_IP";
            TB_IP.Size = new Size(134, 23);
            TB_IP.TabIndex = 8;
            TB_IP.Text = "192.168.0.1";
            // 
            // NUD_Port
            // 
            NUD_Port.Location = new Point(209, 4);
            NUD_Port.Margin = new Padding(0, 4, 4, 4);
            NUD_Port.Name = "NUD_Port";
            NUD_Port.ReadOnly = true;
            NUD_Port.Size = new Size(67, 23);
            NUD_Port.TabIndex = 6;
            NUD_Port.Text = "6000";
            // 
            // CB_Protocol
            // 
            CB_Protocol.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Protocol.FormattingEnabled = true;
            CB_Protocol.Location = new Point(280, 4);
            CB_Protocol.Margin = new Padding(0, 4, 4, 4);
            CB_Protocol.Name = "CB_Protocol";
            CB_Protocol.Size = new Size(67, 23);
            CB_Protocol.TabIndex = 10;
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            // 
            // CB_Routine
            // 
            CB_Routine.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Routine.FormattingEnabled = true;
            CB_Routine.Location = new Point(351, 4);
            CB_Routine.Margin = new Padding(0, 4, 4, 4);
            CB_Routine.Name = "CB_Routine";
            CB_Routine.Size = new Size(117, 23);
            CB_Routine.TabIndex = 7;
            // 
            // FLP_Line
            // 
            FLP_Line.BackColor = SystemColors.ControlDarkDark;
            FLP_Bots.SetFlowBreak(FLP_Line, true);
            FLP_Line.Location = new Point(0, 33);
            FLP_Line.Margin = new Padding(0);
            FLP_Line.Name = "FLP_Line";
            FLP_Line.Size = new Size(65535, 1);
            FLP_Line.TabIndex = 5;
            // 
            // Tab_Hub
            // 
            Tab_Hub.Controls.Add(PG_Hub);
            Tab_Hub.Controls.Add(B_UpdateCheck);
            Tab_Hub.Location = new Point(4, 36);
            Tab_Hub.Margin = new Padding(4, 3, 4, 3);
            Tab_Hub.Name = "Tab_Hub";
            Tab_Hub.Size = new Size(757, 208);
            Tab_Hub.TabIndex = 2;
            Tab_Hub.Text = "Hub";
            Tab_Hub.UseVisualStyleBackColor = true;
            // 
            // PG_Hub
            // 
            PG_Hub.BackColor = SystemColors.Control;
            PG_Hub.Dock = DockStyle.Fill;
            PG_Hub.Location = new Point(0, 0);
            PG_Hub.Margin = new Padding(4, 3, 4, 3);
            PG_Hub.Name = "PG_Hub";
            PG_Hub.PropertySort = PropertySort.Categorized;
            PG_Hub.Size = new Size(757, 178);
            PG_Hub.TabIndex = 0;
            // 
            // B_UpdateCheck
            // 
            B_UpdateCheck.Dock = DockStyle.Bottom;
            B_UpdateCheck.FlatStyle = FlatStyle.Flat;
            B_UpdateCheck.Location = new Point(0, 178);
            B_UpdateCheck.Margin = new Padding(0);
            B_UpdateCheck.Name = "B_UpdateCheck";
            B_UpdateCheck.Size = new Size(757, 30);
            B_UpdateCheck.TabIndex = 1;
            B_UpdateCheck.Text = "Check For Update";
            B_UpdateCheck.UseVisualStyleBackColor = true;
            B_UpdateCheck.Click += B_Update_Click;
            // 
            // Tab_Logs
            // 
            Tab_Logs.Controls.Add(RTB_Logs);
            Tab_Logs.Location = new Point(4, 36);
            Tab_Logs.Margin = new Padding(4, 3, 4, 3);
            Tab_Logs.Name = "Tab_Logs";
            Tab_Logs.Size = new Size(757, 208);
            Tab_Logs.TabIndex = 1;
            Tab_Logs.Text = "Logs";
            Tab_Logs.UseVisualStyleBackColor = true;
            // 
            // RTB_Logs
            // 
            RTB_Logs.BorderStyle = BorderStyle.None;
            RTB_Logs.Dock = DockStyle.Fill;
            RTB_Logs.HideSelection = false;
            RTB_Logs.Location = new Point(0, 0);
            RTB_Logs.Margin = new Padding(4, 3, 4, 3);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new Size(757, 208);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            // 
            // B_Stop
            // 
            B_Stop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Stop.FlatStyle = FlatStyle.Flat;
            B_Stop.Location = new Point(560, 2);
            B_Stop.Margin = new Padding(0);
            B_Stop.Name = "B_Stop";
            B_Stop.Size = new Size(80, 29);
            B_Stop.TabIndex = 4;
            B_Stop.Text = "Stop All";
            B_Stop.UseVisualStyleBackColor = true;
            B_Stop.Click += B_Stop_Click;
            // 
            // B_Start
            // 
            B_Start.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Start.FlatStyle = FlatStyle.Flat;
            B_Start.Location = new Point(480, 2);
            B_Start.Margin = new Padding(0);
            B_Start.Name = "B_Start";
            B_Start.Size = new Size(80, 29);
            B_Start.TabIndex = 3;
            B_Start.Text = "Start All";
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // B_RebootStop
            // 
            B_RebootStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_RebootStop.FlatStyle = FlatStyle.Flat;
            B_RebootStop.Location = new Point(640, 2);
            B_RebootStop.Margin = new Padding(0);
            B_RebootStop.Name = "B_RebootStop";
            B_RebootStop.Size = new Size(121, 29);
            B_RebootStop.TabIndex = 3;
            B_RebootStop.Text = "Reboot And Stop";
            B_RebootStop.UseVisualStyleBackColor = true;
            B_RebootStop.Click += B_RebootStop_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(765, 248);
            Controls.Add(B_Stop);
            Controls.Add(B_Start);
            Controls.Add(B_RebootStop);
            Controls.Add(TC_Main);
            Icon = Resources.icon;
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(600, 287);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SysBot: Pokémon";
            FormClosing += Main_FormClosing;
            TC_Main.ResumeLayout(false);
            Tab_Bots.ResumeLayout(false);
            FLP_Bots.ResumeLayout(false);
            FLP_BotCreator.ResumeLayout(false);
            FLP_BotCreator.PerformLayout();
            Tab_Hub.ResumeLayout(false);
            Tab_Logs.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private DrawableTabControl TC_Main;
        private TabPage Tab_Bots;
        private TabPage Tab_Logs;
        private RichTextBox RTB_Logs;
        private TabPage Tab_Hub;
        private PropertyGrid PG_Hub;
        private DrawableButton B_Stop;
        private DrawableButton B_Start;
        private DrawableButton B_RebootStop;
        private TextBox TB_IP;
        private ComboBox CB_Routine;
        private TextBox NUD_Port;
        private DrawableButton B_New;
        private FlowLayoutPanel FLP_Bots;
        private ComboBox CB_Protocol;
        private FlowLayoutPanel FLP_BotCreator;
        private FlowLayoutPanel FLP_Line;
        private DrawableButton B_UpdateCheck;
    }
}

