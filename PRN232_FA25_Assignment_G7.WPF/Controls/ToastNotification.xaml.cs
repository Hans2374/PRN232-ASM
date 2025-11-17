using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PRN232_FA25_Assignment_G7.WPF.Helpers;

namespace PRN232_FA25_Assignment_G7.WPF.Controls;

public partial class ToastNotification : UserControl
{
    private System.Threading.Timer? _autoCloseTimer;

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(ToastNotification), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToastTypeProperty =
        DependencyProperty.Register(nameof(ToastType), typeof(ToastType), typeof(ToastNotification), new PropertyMetadata(ToastType.Info, OnToastTypeChanged));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ToastType ToastType
    {
        get => (ToastType)GetValue(ToastTypeProperty);
        set => SetValue(ToastTypeProperty, value);
    }

    public event EventHandler? Closed;

    public ToastNotification()
    {
        InitializeComponent();
        UpdateAppearance();
    }

    private static void OnToastTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToastNotification toast)
        {
            toast.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        switch (ToastType)
        {
            case ToastType.Success:
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38A169"));
                IconText.Text = "✓";
                break;
            case ToastType.Error:
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53E3E"));
                IconText.Text = "✕";
                break;
            case ToastType.Warning:
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DD6B20"));
                IconText.Text = "⚠";
                break;
            case ToastType.Info:
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));
                IconText.Text = "ℹ";
                break;
        }
    }

    private void ToastNotification_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start show animation
        var showStoryboard = (Storyboard)FindResource("ShowAnimation");
        showStoryboard.Begin(this);

        // Auto-dismiss after 3 seconds
        _autoCloseTimer = new System.Threading.Timer(_ =>
        {
            Dispatcher.Invoke(Hide);
        }, null, 3000, System.Threading.Timeout.Infinite);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    public void Hide()
    {
        _autoCloseTimer?.Dispose();
        _autoCloseTimer = null;

        var hideStoryboard = (Storyboard)FindResource("HideAnimation");
        hideStoryboard.Begin(this);
    }

    private void HideAnimation_Completed(object? sender, EventArgs e)
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
