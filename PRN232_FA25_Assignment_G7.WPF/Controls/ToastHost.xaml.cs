using System.Windows.Controls;
using PRN232_FA25_Assignment_G7.WPF.Helpers;

namespace PRN232_FA25_Assignment_G7.WPF.Controls;

public partial class ToastHost : UserControl
{
    private const int MaxVisibleToasts = 4;

    public ToastHost()
    {
        InitializeComponent();
    }

    public void ShowToast(string message, ToastType type)
    {
        // Create new toast notification
        var toast = new ToastNotification
        {
            Message = message,
            ToastType = type,
            IsHitTestVisible = true
        };

        // Handle toast closed event
        toast.Closed += (s, e) =>
        {
            ToastContainer.Children.Remove(toast);
        };

        // Add to container
        ToastContainer.Children.Insert(0, toast);

        // Remove oldest toast if exceeding max count
        while (ToastContainer.Children.Count > MaxVisibleToasts)
        {
            var oldestToast = ToastContainer.Children[ToastContainer.Children.Count - 1] as ToastNotification;
            oldestToast?.Hide();
        }
    }

    public void ClearAll()
    {
        foreach (var child in ToastContainer.Children.OfType<ToastNotification>().ToList())
        {
            child.Hide();
        }
    }
}
