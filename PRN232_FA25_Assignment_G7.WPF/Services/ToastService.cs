using System.Windows;
using PRN232_FA25_Assignment_G7.WPF.Controls;
using PRN232_FA25_Assignment_G7.WPF.Helpers;

namespace PRN232_FA25_Assignment_G7.WPF.Services;

public static class ToastService
{
    private static ToastHost? _host;

    public static void RegisterHost(ToastHost host)
    {
        _host = host;
    }

    public static void Show(string message, ToastType type = ToastType.Info)
    {
        if (_host == null)
        {
            // Fallback to MessageBox if no host is registered
            MessageBox.Show(message, type.ToString(), MessageBoxButton.OK, 
                type == ToastType.Error ? MessageBoxImage.Error : MessageBoxImage.Information);
            return;
        }

        // Ensure UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            _host.ShowToast(message, type);
        });
    }

    public static void ShowSuccess(string message)
    {
        Show(message, ToastType.Success);
    }

    public static void ShowError(string message)
    {
        Show(message, ToastType.Error);
    }

    public static void ShowWarning(string message)
    {
        Show(message, ToastType.Warning);
    }

    public static void ShowInfo(string message)
    {
        Show(message, ToastType.Info);
    }

    public static void ClearAll()
    {
        if (_host != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _host.ClearAll();
            });
        }
    }
}
