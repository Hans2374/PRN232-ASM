using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PRN232_FA25_Assignment_G7.WPF.Helpers;
using PRN232_FA25_Assignment_G7.WPF.Models;
using PRN232_FA25_Assignment_G7.WPF.Services;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand { get; }

    public Action? OnLoginSuccess { get; set; }

    public LoginViewModel()
    {
        _apiClient = new ApiClient();
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
    }

    private async Task LoginAsync()
    {
        try
        {
            // Clear previous error
            ErrorMessage = string.Empty;

            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                return;
            }

            IsLoading = true;

            // Prepare login request
            var loginRequest = new LoginRequest
            {
                Username = Username,
                Password = Password
            };

            // Call API
            var response = await _apiClient.PostAsync<AuthResponse>("/api/auth/login", loginRequest);

            if (response != null)
            {
                // Save session
                AuthSession.SetSession(
                    response.Token,
                    response.Username,
                    response.Role,
                    response.FullName,
                    response.UserId
                );

                // Set authorization header for future requests
                _apiClient.SetAuthorizationToken(response.Token);

                // Trigger success callback
                OnLoginSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Invalid username or password.";
            ToastService.ShowError("Invalid username or password.");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            ToastService.ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
