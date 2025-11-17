using System.Windows;
using System.Windows.Controls;
using PRN232_FA25_Assignment_G7.WPF.Services;
using PRN232_FA25_Assignment_G7.WPF.ViewModels;

namespace PRN232_FA25_Assignment_G7.WPF.Views;

public partial class AdminDashboard : Window
{
    private readonly AdminDashboardViewModel _viewModel;

    public AdminDashboard()
    {
        InitializeComponent();
        _viewModel = (AdminDashboardViewModel)DataContext;
        _viewModel.NavigateAction = NavigateToPage;

        // Register ToastHost
        ToastService.RegisterHost(ToastHostControl);

        // Navigate to first available page based on role
        NavigateToDefaultPage();
    }

    private void NavigateToDefaultPage()
    {
        // Navigate based on role permissions
        if (_viewModel.CanAccessUsers)
        {
            NavigateToPage(typeof(UsersPage));
        }
        else if (_viewModel.CanAccessExams)
        {
            NavigateToPage(typeof(ExamsPage));
        }
        else if (_viewModel.CanAccessReports)
        {
            NavigateToPage(typeof(ReportsPage));
        }
    }

    private void NavigateToPage(Type pageType)
    {
        var page = Activator.CreateInstance(pageType) as Page;
        if (page != null)
        {
            ContentFrame.Navigate(page);
            UpdateActiveButton(pageType);
        }
    }

    private void UpdateActiveButton(Type pageType)
    {
        // Reset all buttons
        DashboardBtn.IsActive = false;
        UsersBtn.IsActive = false;
        ExamsBtn.IsActive = false;
        ReportsBtn.IsActive = false;

        // Set active button based on page type
        if (pageType == typeof(UsersPage))
            UsersBtn.IsActive = true;
        else if (pageType == typeof(ExamsPage))
            ExamsBtn.IsActive = true;
        else if (pageType == typeof(ReportsPage))
            ReportsBtn.IsActive = true;
    }
}
