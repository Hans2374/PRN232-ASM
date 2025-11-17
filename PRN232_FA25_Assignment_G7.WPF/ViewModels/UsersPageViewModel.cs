using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PRN232_FA25_Assignment_G7.WPF.Helpers;
using PRN232_FA25_Assignment_G7.WPF.Models;
using PRN232_FA25_Assignment_G7.WPF.Services;
using PRN232_FA25_Assignment_G7.WPF.Views;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class UsersPageViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private ObservableCollection<UserResponse> _users;
    private ObservableCollection<UserResponse> _filteredUsers;
    private UserResponse? _selectedUser;
    private string _searchText = string.Empty;
    private bool _isLoading;

    public ObservableCollection<UserResponse> Users
    {
        get => _users;
        set
        {
            _users = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public ObservableCollection<UserResponse> FilteredUsers
    {
        get => _filteredUsers;
        set
        {
            _filteredUsers = value;
            OnPropertyChanged();
        }
    }

    public UserResponse? SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public string Title { get; set; } = "User Management";

    // Role-based permission
    public bool CanManageUsers => AuthSession.IsAdmin;

    public ICommand LoadUsersCommand { get; }
    public ICommand CreateUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeactivateUserCommand { get; }
    public ICommand RefreshCommand { get; }

    public UsersPageViewModel()
    {
        _apiClient = new ApiClient();
        _users = new ObservableCollection<UserResponse>();
        _filteredUsers = new ObservableCollection<UserResponse>();

        LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());
        CreateUserCommand = new RelayCommand(CreateUser, () => CanManageUsers);
        EditUserCommand = new RelayCommand(EditUser, () => SelectedUser != null && CanManageUsers);
        DeactivateUserCommand = new RelayCommand(async () => await DeactivateUserAsync(), () => SelectedUser != null && CanManageUsers);
        RefreshCommand = new RelayCommand(async () => await LoadUsersAsync());

        // Load users on initialization
        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            var users = await _apiClient.GetUsersAsync();
            Users = new ObservableCollection<UserResponse>(users);
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to load users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CreateUser()
    {
        var dialog = new UserFormDialog();
        var viewModel = new UserFormDialogViewModel(false);
        viewModel.CloseAction = async (success) =>
        {
            dialog.Close();
            if (success)
            {
                await LoadUsersAsync();
            }
        };
        dialog.DataContext = viewModel;
        dialog.ShowDialog();
    }

    private void EditUser()
    {
        if (SelectedUser == null) return;

        var dialog = new UserFormDialog();
        var viewModel = new UserFormDialogViewModel(true, SelectedUser);
        viewModel.CloseAction = async (success) =>
        {
            dialog.Close();
            if (success)
            {
                await LoadUsersAsync();
            }
        };
        dialog.DataContext = viewModel;
        dialog.ShowDialog();
    }

    private async Task DeactivateUserAsync()
    {
        if (SelectedUser == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to deactivate user '{SelectedUser.Username}'?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            IsLoading = true;
            var success = await _apiClient.DeactivateUserAsync(SelectedUser.Id);

            if (success)
            {
                ToastService.ShowSuccess("User deactivated successfully.");
                await LoadUsersAsync();
            }
            else
            {
                ToastService.ShowError("Failed to deactivate user.");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Error deactivating user: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredUsers = new ObservableCollection<UserResponse>(Users);
        }
        else
        {
            var searchLower = SearchText.ToLower();
            var filtered = Users.Where(u =>
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                u.FullName.ToLower().Contains(searchLower)
            );
            FilteredUsers = new ObservableCollection<UserResponse>(filtered);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
