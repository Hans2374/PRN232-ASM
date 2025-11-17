using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PRN232_FA25_Assignment_G7.WPF.Helpers;
using PRN232_FA25_Assignment_G7.WPF.Services;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class AdminDashboardViewModel : INotifyPropertyChanged
{
    private string _username;
    private string _role;
    private Type? _currentPageType;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public string Role
    {
        get => _role;
        set
        {
            _role = value;
            OnPropertyChanged();
        }
    }

    public Type? CurrentPageType
    {
        get => _currentPageType;
        set
        {
            _currentPageType = value;
            OnPropertyChanged();
        }
    }

    // Role-based visibility properties
    public bool CanAccessUsers => AuthSession.IsAdmin || AuthSession.IsManager;
    public bool CanAccessExams => AuthSession.IsAdmin || AuthSession.IsMod || AuthSession.IsExaminer;
    public bool CanAccessReports => AuthSession.IsAdmin || AuthSession.IsManager;

    public Action<Type>? NavigateAction { get; set; }

    public ICommand NavigateUsersCommand { get; }
    public ICommand NavigateExamsCommand { get; }
    public ICommand NavigateReportsCommand { get; }
    public ICommand LogoutCommand { get; }

    public AdminDashboardViewModel()
    {
        _username = AuthSession.CurrentUser?.FullName ?? "User";
        _role = AuthSession.CurrentUser?.Role ?? "Unknown";

        NavigateUsersCommand = new RelayCommand(NavigateToUsers, () => CanAccessUsers);
        NavigateExamsCommand = new RelayCommand(NavigateToExams, () => CanAccessExams);
        NavigateReportsCommand = new RelayCommand(NavigateToReports, () => CanAccessReports);
        LogoutCommand = new RelayCommand(Logout);
    }

    private void NavigateToUsers()
    {
        NavigateAction?.Invoke(typeof(Views.UsersPage));
        CurrentPageType = typeof(Views.UsersPage);
    }

    private void NavigateToExams()
    {
        NavigateAction?.Invoke(typeof(Views.ExamsPage));
        CurrentPageType = typeof(Views.ExamsPage);
    }

    private void NavigateToReports()
    {
        NavigateAction?.Invoke(typeof(Views.ReportsPage));
        CurrentPageType = typeof(Views.ReportsPage);
    }

    private void Logout()
    {
        var result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            AuthSession.Clear();

            // Close current dashboard window
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is Views.AdminDashboard)
                ?.Close();

            // Open login window
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
