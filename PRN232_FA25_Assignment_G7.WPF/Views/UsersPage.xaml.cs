using System.Windows.Controls;
using PRN232_FA25_Assignment_G7.WPF.ViewModels;

namespace PRN232_FA25_Assignment_G7.WPF.Views;

public partial class UsersPage : Page
{
    public UsersPage()
    {
        InitializeComponent();
        DataContext = new UsersPageViewModel();
    }
}
