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
            new PropertyMetadata(
                Brushes.Firebrick,
                OnMessageBrushChanged));

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
        UpdateStatusColor();
    }

    private static void OnMessageBrushChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        ((StatusMessageControl)d).UpdateStatusColor();
    }

    private void UpdateStatusColor()
    {
        if (StatusText is null)
        {
            return;
        }

        // Convert the existing view-model status brushes to theme colors.
        var color = (MessageBrush as SolidColorBrush)?.Color;

        var key = color == Brushes.DarkGreen.Color
            ? "StatusSuccessBrush"
            : color == Brushes.DarkOrange.Color
                ? "StatusWarningBrush"
                : color == Brushes.Firebrick.Color
                    ? "StatusErrorBrush"
                    : color == Brushes.DimGray.Color
                        ? "StatusNeutralBrush"
                        : null;

        if (key is not null)
        {
            StatusText.SetResourceReference(
                TextBlock.ForegroundProperty,
                key);
        }
        else
        {
            StatusText.Foreground = MessageBrush;
        }
    }
}