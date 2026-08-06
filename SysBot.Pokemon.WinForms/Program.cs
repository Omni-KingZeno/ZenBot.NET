using PKHeX.Core;
using SysBot.Pokemon.Z3;

namespace SysBot.Pokemon.WinForms;

internal static class Program
{
    public static bool IsDarkTheme => Config.Hub.DarkMode;
    public static readonly ProgramConfig Config;

    static Program()
    {
        var cmd = Environment.GetCommandLineArgs();
        var use = Array.Find(cmd, z => z.EndsWith(".json"));
        var cfg = Config = ConfigLoader.LoadConfig(use);

#if NETCOREAPP
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif

    if (cfg.Hub.DarkMode)
            Application.SetColorMode(SystemColorMode.Dark);

        PokeTradeBotSWSH.SeedChecker = new Z3SeedSearchHandler<PK8>();
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        Application.Run(new Main());
    }
}
