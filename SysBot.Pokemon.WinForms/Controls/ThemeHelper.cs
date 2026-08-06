using System.ComponentModel;

namespace SysBot.Pokemon.WinForms;

internal static class ThemeHelper
{
    public static bool IsDarkThemeSafe()
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return false;

        try
        {
            return Program.IsDarkTheme;
        }
        catch
        {
            return false;
        }
    }
}
