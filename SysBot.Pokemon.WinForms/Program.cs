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
        Config = ConfigLoader.LoadConfig(use);

        PokeTradeBotSWSH.SeedChecker = new Z3SeedSearchHandler<PK8>();
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
#if NETCOREAPP
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();

        if (Config.Hub.DarkMode)
            Application.SetColorMode(SystemColorMode.Dark);

        Application.Run(new Main());
    }
}
