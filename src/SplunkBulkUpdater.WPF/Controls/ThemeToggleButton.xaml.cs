using SplunkBulkUpdater.WPF.Themes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.Controls;

public partial class ThemeToggleButton : UserControl
{
    private static readonly Geometry Moon = Geometry.Parse(
        "M 12,2 A 10,10 0 1 0 22,12 A 8,8 0 0 1 12,2 Z");

    private static readonly Geometry Sun = Geometry.Parse(
        "M 12,5 A 7,7 0 1 1 12,19 A 7,7 0 1 1 12,5 Z " +
        "M 12,0 L 12,2 M 12,22 L 12,24 M 0,12 L 2,12 M 22,12 L 24,12 " +
        "M 3.5,3.5 L 5,5 M 19,19 L 20.5,20.5 M 3.5,20.5 L 5,19 M 19,5 L 20.5,3.5");

    public ThemeToggleButton()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged += OnThemeChanged;
        Refresh();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= OnThemeChanged;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        ThemeManager.Toggle();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        ThemeIcon.Data = ThemeManager.IsDark ? Sun : Moon;
        ThemeLabel.Text = ThemeManager.IsDark
            ? "Light mode"
            : "Dark mode";

        ToggleButton.SetValue(
            System.Windows.Automation.AutomationProperties.NameProperty,
            ThemeManager.IsDark
                ? "Switch to light mode"
                : "Switch to dark mode");
    }
}