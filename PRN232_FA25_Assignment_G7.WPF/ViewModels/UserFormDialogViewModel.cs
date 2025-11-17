using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PRN232_FA25_Assignment_G7.WPF.Helpers;
using PRN232_FA25_Assignment_G7.WPF.Models;
using PRN232_FA25_Assignment_G7.WPF.Services;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class UserFormDialogViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _role = "Examiner";
    private bool _isActive = true;
    private bool _isSaving;

    public bool IsEditMode { get; }
    public Guid? UserId { get; }
    public Action<bool>? CloseAction { get; set; }

    public string DialogTitle => IsEditMode ? "Edit User" : "Create New User";

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public string FullName
    {
        get => _fullName;
        set
        {
            _fullName = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
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

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            _isSaving = value;
            OnPropertyChanged();
        }
    }

    public List<string> AvailableRoles { get; } = new List<string>
    {
        "Admin",
        "Manager",
        "Moderator",
        "Examiner"
    };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    // Constructor for Create mode
    public UserFormDialogViewModel(bool isEditMode, UserResponse? user = null)
    {
        _apiClient = new ApiClient();
        IsEditMode = isEditMode;

        if (isEditMode && user != null)
        {
            UserId = user.Id;
            Username = user.Username;
            FullName = user.FullName;
            Email = user.Email;
            Role = user.Role;
            IsActive = user.IsActive;
        }

        SaveCommand = new RelayCommand(async () => await SaveAsync(), () => CanSave());
        CancelCommand = new RelayCommand(Cancel);
    }

    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(Username)) return false;
        if (string.IsNullOrWhiteSpace(FullName)) return false;
        if (string.IsNullOrWhiteSpace(Email)) return false;
        if (string.IsNullOrWhiteSpace(Role)) return false;
        if (!IsEditMode && string.IsNullOrWhiteSpace(Password)) return false;
        return !IsSaving;
    }

    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;

            if (IsEditMode)
            {
                // Update existing user
                var request = new UpdateUserRequest
                {
                    FullName = FullName,
                    Email = Email,
                    Role = Role,
                    IsActive = IsActive
                };

                var result = await _apiClient.UpdateUserAsync(UserId!.Value, request);

                if (result != null)
                {
                    ToastService.ShowSuccess("User updated successfully.");
                    CloseAction?.Invoke(true);
                }
                else
                {
                    ToastService.ShowError("Failed to update user.");
                }
            }
            else
            {
                // Create new user
                var request = new CreateUserRequest
                {
                    Username = Username,
                    Password = Password,
                    FullName = FullName,
                    Email = Email,
                    Role = Role
                };

                var result = await _apiClient.CreateUserAsync(request);

                if (result != null)
                {
                    ToastService.ShowSuccess("User created successfully.");
                    CloseAction?.Invoke(true);
                }
                else
                {
                    ToastService.ShowError("Failed to create user.");
                }
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Error saving user: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void Cancel()
    {
        CloseAction?.Invoke(false);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
