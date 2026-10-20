using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SplunkBulkUpdater.WPF.Controls;

public partial class StatusMessageControl : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(StatusMessageControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessageBrushProperty =
        DependencyProperty.Register(
            nameof(MessageBrush),
            typeof(Brush),
            typeof(StatusMessageControl),
            new PropertyMetadata(Brushes.Firebrick));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public Brush MessageBrush
    {
        get => (Brush)GetValue(MessageBrushProperty);
        set => SetValue(MessageBrushProperty, value);
    }

    public StatusMessageControl()
    {
        InitializeComponent();
    }
}