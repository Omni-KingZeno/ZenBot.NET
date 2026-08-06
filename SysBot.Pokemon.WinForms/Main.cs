using System.Diagnostics;
using System.Runtime.InteropServices;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms;

public sealed partial class Main : Form
{
    private readonly List<PokeBotState> Bots = [];
    private readonly IPokeBotRunner RunningEnvironment;
    private readonly ProgramConfig Config = Program.Config;

    public Main()
    {
        InitializeComponent();

        RunningEnvironment = GetRunner(Config);
        {
            foreach (var bot in Config.Bots)
            {
                bot.Initialize();
                AddBot(bot);
            }
        }
        if (Program.IsDarkTheme)
        {

            foreach (var tab in new[] { Tab_Bots, Tab_Logs, Tab_Hub })
                tab.BackColor = Color.FromArgb(32, 32, 32);

            foreach (var text in new[] { TB_IP, NUD_Port })
                text.BorderStyle = BorderStyle.FixedSingle;
        }

        RTB_Logs.MaxLength = 32_767; // character length
        LoadControls();
        Text = $"{Text} ({Config.Hub.Mode})";
        Task.Run(BotMonitor);

        InitUtil.InitializeStubs(Config.Hub.Mode);

        if (Config.Hub.DarkMode)
        {
            foreach (TabPage tab in TC_Main.TabPages)
                tab.UseVisualStyleBackColor = false;
        }

        if (Config is not { Width: 0, Height: 0 })
        {
            Width = Config.Width;
            Height = Config.Height;
        }

        B_New.Height = CB_Protocol.Height;
        FLP_BotCreator.Height = B_New.Height + B_New.Margin.Vertical;

        if (Environment.GetCommandLineArgs().Contains("--updated", StringComparer.OrdinalIgnoreCase))
        {
            Shown += (s, e) =>
            {
                ForceToForeground();
                MessageBox.Show(this,
                    "Update Successful!",
                    "Update Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private const int SW_RESTORE = 9;

    /// <summary>
    /// Forces this window to the foreground even when Windows' foreground-lock would normally
    /// block it (e.g. when relaunched by a helper script rather than direct user interaction).
    /// Works by briefly attaching this thread's input state to the current foreground thread's,
    /// which is one of the few conditions under which SetForegroundWindow is honored.
    /// </summary>
    private void ForceToForeground()
    {
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;

        IntPtr foregroundWindow = GetForegroundWindow();
        uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, out _);
        uint thisThreadId = GetCurrentThreadId();

        bool attached = foregroundThreadId != thisThreadId && AttachThreadInput(foregroundThreadId, thisThreadId, true);

        try
        {
            ShowWindow(Handle, SW_RESTORE);
            SetForegroundWindow(Handle);
            Activate();
        }
        finally
        {
            if (attached)
                AttachThreadInput(foregroundThreadId, thisThreadId, false);
        }
    }

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
    {
        base.ScaleControl(factor, specified);
        TC_Main.ItemSize = new((int)(TC_Main.ItemSize.Width * factor.Width), (int)(TC_Main.ItemSize.Height * factor.Height));
    }


    private static IPokeBotRunner GetRunner(ProgramConfig cfg) => cfg.Hub.Mode switch
    {
        ProgramMode.LGPE => new PokeBotRunnerImpl<PB7>(cfg.Hub, new BotFactory7LGPE()),
        ProgramMode.SWSH => new PokeBotRunnerImpl<PK8>(cfg.Hub, new BotFactory8SWSH()),
        ProgramMode.BDSP => new PokeBotRunnerImpl<PB8>(cfg.Hub, new BotFactory8BS()),
        ProgramMode.LA => new PokeBotRunnerImpl<PA8>(cfg.Hub, new BotFactory8LA()),
        ProgramMode.SV => new PokeBotRunnerImpl<PK9>(cfg.Hub, new BotFactory9SV()),
        ProgramMode.LZA => new PokeBotRunnerImpl<PA9>(cfg.Hub, new BotFactory9LZA()),
        _ => throw new IndexOutOfRangeException("Unsupported mode."),
    };

    private async Task BotMonitor()
    {
        while (!Disposing)
        {
            try
            {
                foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                    c.ReadState();
            }
            catch
            {
                // Updating the collection by adding/removing bots will change the iterator
                // Can try a for-loop or ToArray, but those still don't prevent concurrent mutations of the array.
                // Just try, and if failed, ignore. Next loop will be fine. Locks on the collection are kinda overkill, since this task is not critical.
            }
            await Task.Delay(2_000).ConfigureAwait(false);
        }
    }

    private void LoadControls()
    {
        PG_Hub.SelectedObject = RunningEnvironment.Config;

        var routines = Enum.GetValues<PokeRoutineType>().Where(z => RunningEnvironment.SupportsRoutine(z));
        var list = routines.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
        CB_Routine.DisplayMember = nameof(ComboItem.Text);
        CB_Routine.ValueMember = nameof(ComboItem.Value);
        CB_Routine.DataSource = list;
        CB_Routine.SelectedValue = (int)PokeRoutineType.FlexTrade; // default option

        var protocols = Enum.GetValues<SwitchProtocol>();
        var listP = protocols.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
        CB_Protocol.DisplayMember = nameof(ComboItem.Text);
        CB_Protocol.ValueMember = nameof(ComboItem.Value);
        CB_Protocol.DataSource = listP;
        CB_Protocol.SelectedIndex = (int)SwitchProtocol.WiFi; // default option

        LogUtil.Forwarders.Add(new TextBoxForwarder(RTB_Logs));
    }

    private ProgramConfig GetCurrentConfiguration()
    {
        Config.Bots = [.. Bots];
        return Config;
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveCurrentConfig();
        var bots = RunningEnvironment;
        if (!bots.IsRunning)
            return;

        async Task WaitUntilNotRunning()
        {
            while (bots.IsRunning)
                await Task.Delay(10).ConfigureAwait(false);
        }

        // Try to let all bots hard-stop before ending execution of the entire program.
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        bots.StopAll();
        Task.WhenAny(WaitUntilNotRunning(), Task.Delay(5_000)).ConfigureAwait(true).GetAwaiter().GetResult();
    }

    private void SaveCurrentConfig()
    {
        var cfg = GetCurrentConfiguration();
        cfg.Width = Width;
        cfg.Height = Height;
        ConfigLoader.Save(cfg);
    }

    private async void B_Start_Click(object sender, EventArgs e)
    {
        SaveCurrentConfig();
        CheckForUpdate();

        LogUtil.LogInfo("Starting all bots...", "Form");
        RunningEnvironment.InitializeStart();
        SendAll(BotControlCommand.Start);
        Tab_Logs.Select();

        if (Bots.Count == 0)
            WinFormsUtil.Alert("No bots configured, but all supporting services have been started.");
    }

    private void SendAll(BotControlCommand cmd)
    {
        foreach (var c in FLP_Bots.Controls.OfType<BotController>())
            c.SendCommand(cmd, false);

        EchoUtil.Echo($"All bots have been issued a command to {cmd}.");
    }

    private void B_Stop_Click(object sender, EventArgs e)
    {
        var env = RunningEnvironment;
        if (!env.IsRunning && (ModifierKeys & Keys.Alt) == 0)
        {
            WinFormsUtil.Alert("Nothing is currently running.");
            return;
        }

        var cmd = BotControlCommand.Stop;

        if ((ModifierKeys & Keys.Control) != 0 || (ModifierKeys & Keys.Shift) != 0) // either, because remembering which can be hard
        {
            if (env.IsRunning)
            {
                WinFormsUtil.Alert("Commanding all bots to Idle.", "Press Stop (without a modifier key) to hard-stop and unlock control, or press Stop with the modifier key again to resume.");
                cmd = BotControlCommand.Idle;
            }
            else
            {
                WinFormsUtil.Alert("Commanding all bots to resume their original task.", "Press Stop (without a modifier key) to hard-stop and unlock control.");
                cmd = BotControlCommand.Resume;
            }
        }
        SendAll(cmd);
    }

    private void B_RebootStop_Click(object sender, EventArgs e)
    {
        if (RunningEnvironment.IsRunning)
        {
            B_Stop_Click(sender, e);
            Task.Run(async () => { await Task.Delay(3_000).ConfigureAwait(false); });
        }

        SaveCurrentConfig();
        LogUtil.LogInfo("Restarting all the consoles...", "Form");
        RunningEnvironment.InitializeStart();
        SendAll(BotControlCommand.RebootAndStop);
        Tab_Logs.Select();

        if (Bots.Count == 0)
            WinFormsUtil.Alert("No bots configured, but all supporting services have been issued the reboot command.");
    }

    private void B_New_Click(object sender, EventArgs e)
    {
        var cfg = CreateNewBotConfig();
        if (!AddBot(cfg))
        {
            WinFormsUtil.Alert("Unable to add bot; ensure details are valid and not duplicate with an already existing bot.");
            return;
        }
        System.Media.SystemSounds.Asterisk.Play();
    }

    private bool AddBot(PokeBotState cfg)
    {
        if (!cfg.IsValid())
            return false;

        if (Bots.Any(z => z.Connection.Equals(cfg.Connection)))
            return false;

        PokeRoutineExecutorBase newBot;
        try
        {
            Console.WriteLine($"Current Mode ({Config.Hub.Mode}) does not support this type of bot ({cfg.CurrentRoutineType}).");
            newBot = RunningEnvironment.CreateBotFromConfig(cfg);
        }
        catch
        {
            return false;
        }

        try
        {
            RunningEnvironment.Add(newBot);
        }
        catch (ArgumentException ex)
        {
            WinFormsUtil.Error(ex.Message);
            return false;
        }

        AddBotControl(cfg);
        Bots.Add(cfg);
        return true;
    }

    private void AddBotControl(PokeBotState cfg)
    {
        var row = new BotController { Width = FLP_Bots.Width, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        row.Initialize(RunningEnvironment, cfg);
        FLP_Bots.Controls.Add(row);
        FLP_Bots.SetFlowBreak(row, true);
        row.AddClickHandler(() =>
        {
            var details = cfg.Connection;
            TB_IP.Text = details.IP;
            NUD_Port.Text = details.Port.ToString();
            CB_Protocol.SelectedIndex = (int)details.Protocol;
            CB_Routine.SelectedValue = (int)cfg.InitialRoutine;
        });

        row.Remove += (s, e) =>
        {
            Bots.Remove(row.State);
            RunningEnvironment.Remove(row.State, !RunningEnvironment.Config.SkipConsoleBotCreation);
            FLP_Bots.Controls.Remove(row);
        };
    }

    private PokeBotState CreateNewBotConfig()
    {
        var ip = TB_IP.Text;
        var port = int.TryParse(NUD_Port.Text, out var p) ? p : 6000;
        var cfg = BotConfigUtil.GetConfig<SwitchConnectionConfig>(ip, port);
        cfg.Protocol = (SwitchProtocol)WinFormsUtil.GetIndex(CB_Protocol);

        var pk = new PokeBotState { Connection = cfg };
        var type = (PokeRoutineType)WinFormsUtil.GetIndex(CB_Routine);
        pk.Initialize(type);
        return pk;
    }

    private void FLP_Bots_Resize(object sender, EventArgs e)
    {
        foreach (var c in FLP_Bots.Controls.OfType<BotController>())
            c.Width = FLP_Bots.Width;
    }

    private void CB_Protocol_SelectedIndexChanged(object sender, EventArgs e)
    {
        var isWifi = CB_Protocol.SelectedIndex == 0;
        TB_IP.Visible = isWifi;
        NUD_Port.ReadOnly = isWifi;
        NUD_Port.Visible = !isWifi;

        if (isWifi)
            NUD_Port.Text = "6000";
    }

    public async void CheckForUpdate()
    {
        var update = await UpdateManager.CheckAsync().ConfigureAwait(true);
        if (!update.IsUpToDate)
        {
            var canAutoUpdate = update.AssetDownloadUrl is not null;
            var prompt = canAutoUpdate
                ? $"A newer version of ZenBot is available (v{update.LatestVersion}). Update and restart now?"
                : $"A newer version of ZenBot is available (v{update.LatestVersion}), but no downloadable asset was found. Open the release page?";

            var result = MessageBox.Show(this,
                prompt,
                "Update Available!",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (canAutoUpdate)
                {
                    using var progressForm = new UpdateProgressForm();
                    Enabled = false;
                    progressForm.Show(this);

                    var progress = new Progress<int?>(percent =>
                    {
                        switch (percent)
                        {
                            case 101: // sentinel: download finished, now staging/applying the update
                                progressForm.ReportProgress(100, "Applying update...");
                                break;
                            case int p:
                                progressForm.ReportProgress(p, $"Downloading update... {p}%");
                                break;
                            default:
                                progressForm.ReportProgress(null, "Downloading update...");
                                break;
                        }
                    });

                    try
                    {
                        await UpdateManager.ApplyUpdateAsync(update.AssetDownloadUrl!, progress).ConfigureAwait(true);                        
                    }
                    // ApplyUpdateAsync exits the process on success; if we get here, it failed.
                    catch (Exception ex)
                    {
                        LogUtil.LogInfo($"Auto-update failed: {ex.Message}", "Update Check");
                        WinFormsUtil.Alert("Automatic update failed. Please update manually from the releases page.");
                    }
                    finally
                    {
                        progressForm.Close();
                        Enabled = true;
                    }
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = update.ReleaseUrl,
                        UseShellExecute = true
                    });
                    Application.Exit();
                    return;
                }
            }
        }
    }
}
