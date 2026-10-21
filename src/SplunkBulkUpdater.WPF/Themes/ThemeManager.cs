using System.IO;
using System.Windows;

namespace SplunkBulkUpdater.WPF.Themes;

/// <summary>Changes the palette without reloading the shared styles.</summary>
public static class ThemeManager
{
    private const string LightPalette = "Resources/Colors.xaml";
    private const string DarkPalette = "Resources/DarkColors.xaml";

    private static readonly string PreferenceFile = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "SplunkBulkUpdater",
        "theme.txt");

    public static bool IsDark { get; private set; }

    public static event EventHandler? ThemeChanged;

    public static void Initialize()
    {
        try
        {
            if (File.Exists(PreferenceFile) &&
                File.ReadAllText(PreferenceFile)
                    .Trim()
                    .Equals("dark", StringComparison.OrdinalIgnoreCase))
            {
                SetDarkMode(true);
            }
        }
        catch (IOException)
        {
            // The application can still run with the default theme.
        }
        catch (UnauthorizedAccessException)
        {
            // The application can still run with the default theme.
        }
    }

    public static void Toggle()
    {
        SetDarkMode(!IsDark);

        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(PreferenceFile)!);

            File.WriteAllText(
                PreferenceFile,
                IsDark ? "dark" : "light");
        }
        catch (IOException)
        {
            // Keep the active theme even if the preference cannot be saved.
        }
        catch (UnauthorizedAccessException)
        {
            // Keep the active theme even if the preference cannot be saved.
        }
    }

    public static void SetDarkMode(bool dark)
    {
        if (IsDark == dark)
        {
            return;
        }

        var dictionaries =
            System.Windows.Application.Current.Resources.MergedDictionaries;

        var oldPalette = IsDark ? DarkPalette : LightPalette;
        var index = -1;

        for (var i = 0; i < dictionaries.Count; i++)
        {
            if (dictionaries[i].Source?.OriginalString.EndsWith(
                    oldPalette,
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Missing theme palette: {oldPalette}");
        }

        dictionaries[index] = new ResourceDictionary
        {
            Source = new Uri(
                dark ? DarkPalette : LightPalette,
                UriKind.Relative)
        };

        IsDark = dark;
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }
}