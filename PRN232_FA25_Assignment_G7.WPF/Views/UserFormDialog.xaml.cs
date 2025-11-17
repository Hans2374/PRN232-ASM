using System.Windows;
using PRN232_FA25_Assignment_G7.WPF.ViewModels;

namespace PRN232_FA25_Assignment_G7.WPF.Views;

public partial class UserFormDialog : Window
{
    public UserFormDialog()
    {
        InitializeComponent();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserFormDialogViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }
}
