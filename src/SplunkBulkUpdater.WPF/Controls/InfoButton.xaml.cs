using System.Windows;
using System.Windows.Controls;

namespace SplunkBulkUpdater.WPF.Controls;

public partial class InfoButton : UserControl
{
    public static readonly DependencyProperty InfoTextProperty =
        DependencyProperty.Register(
            nameof(InfoText),
            typeof(string),
            typeof(InfoButton),
            new FrameworkPropertyMetadata(string.Empty));

    public InfoButton()
    {
        InitializeComponent();
    }

    public string InfoText
    {
        get => (string)GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }
}