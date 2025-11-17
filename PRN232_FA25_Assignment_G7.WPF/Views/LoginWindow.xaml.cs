using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PRN232_FA25_Assignment_G7.WPF.ViewModels;

namespace PRN232_FA25_Assignment_G7.WPF.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        
        _viewModel = new LoginViewModel();
        _viewModel.OnLoginSuccess = OnLoginSuccess;
        DataContext = _viewModel;
        
        // Focus username textbox on load
        Loaded += (s, e) => UsernameTextBox.Focus();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }

    private void OnLoginSuccess()
    {
        // Open Admin Dashboard
        var adminDashboard = new AdminDashboard();
        adminDashboard.Show();
        
        // Close login window
        Close();
    }
}

// Value Converters
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
