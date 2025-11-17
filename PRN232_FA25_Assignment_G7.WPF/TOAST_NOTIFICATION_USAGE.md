# Toast Notification System - Usage Guide

## Overview
The Toast Notification system provides a modern, non-intrusive way to display success, error, warning, and info messages throughout the WPF application.

## Components

### 1. **ToastType Enum**
Location: `Helpers/ToastType.cs`

```csharp
public enum ToastType
{
    Success,  // Green background - for successful operations
    Error,    // Red background - for errors and failures
    Warning,  // Orange background - for warnings
    Info      // Blue background - for informational messages
}
```

### 2. **ToastNotification UserControl**
Location: `Controls/ToastNotification.xaml(.cs)`

Features:
- Rounded border with drop shadow
- Icon based on toast type (✓, ✕, ⚠, ℹ)
- Smooth fade-in and slide-up animation
- Smooth fade-out and slide-right animation
- Auto-dismiss after 3 seconds
- Manual close button
- Color-coded backgrounds

### 3. **ToastHost Container**
Location: `Controls/ToastHost.xaml(.cs)`

Features:
- Positioned in top-right corner
- Stacks multiple toasts vertically
- Maximum 4 visible toasts (older ones auto-dismiss)
- Automatically removes toasts after animation completes

### 4. **ToastService (Static Global Service)**
Location: `Services/ToastService.cs`

Thread-safe service for showing toasts from anywhere in the application.

## Setup

### Step 1: Add ToastHost to Your Window

In `AdminDashboard.xaml` (or any other window):

```xml
<Window xmlns:controls="clr-namespace:PRN232_FA25_Assignment_G7.WPF.Controls"
        ...>
    <Grid>
        <!-- Your main content -->
        <Frame x:Name="ContentFrame" />

        <!-- Toast overlay (must be last to appear on top) -->
        <controls:ToastHost x:Name="ToastHostControl"
                           Grid.Column="1"
                           HorizontalAlignment="Right"
                           VerticalAlignment="Top"
                           Margin="0,20,20,0"
                           Panel.ZIndex="999"/>
    </Grid>
</Window>
```

### Step 2: Register the ToastHost in Code-Behind

In `AdminDashboard.xaml.cs` constructor:

```csharp
using PRN232_FA25_Assignment_G7.WPF.Services;

public AdminDashboard()
{
    InitializeComponent();
    
    // Register the ToastHost
    ToastService.RegisterHost(ToastHostControl);
    
    // ... rest of initialization
}
```

## Usage Examples

### Basic Usage

```csharp
using PRN232_FA25_Assignment_G7.WPF.Services;
using PRN232_FA25_Assignment_G7.WPF.Helpers;

// Show different types of toasts
ToastService.Show("User created successfully", ToastType.Success);
ToastService.Show("Invalid credentials", ToastType.Error);
ToastService.Show("This action requires confirmation", ToastType.Warning);
ToastService.Show("New update available", ToastType.Info);
```

### Convenience Methods

```csharp
// Shorthand methods for each type
ToastService.ShowSuccess("Operation completed successfully!");
ToastService.ShowError("Failed to save changes.");
ToastService.ShowWarning("Unsaved changes will be lost.");
ToastService.ShowInfo("Loading data...");
```

### In ViewModels

#### LoginViewModel Example
```csharp
private async Task LoginAsync()
{
    try
    {
        // ... login logic
        ToastService.ShowSuccess($"Welcome back, {response.Username}!");
    }
    catch (HttpRequestException)
    {
        ToastService.ShowError("Invalid username or password.");
    }
    catch (Exception ex)
    {
        ToastService.ShowError($"An error occurred: {ex.Message}");
    }
}
```

#### UsersPageViewModel Example
```csharp
private async Task LoadUsersAsync()
{
    try
    {
        var users = await _apiClient.GetUsersAsync();
        Users = new ObservableCollection<UserResponse>(users);
        // Optional: show success for explicit actions
        // ToastService.ShowSuccess("Users loaded successfully");
    }
    catch (Exception ex)
    {
        ToastService.ShowError($"Failed to load users: {ex.Message}");
    }
}

private async Task DeactivateUserAsync()
{
    try
    {
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
        ToastService.ShowError($"Error: {ex.Message}");
    }
}
```

#### UserFormDialogViewModel Example
```csharp
private async Task SaveAsync()
{
    try
    {
        if (IsEditMode)
        {
            var result = await _apiClient.UpdateUserAsync(UserId.Value, request);
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
}
```

### Advanced Features

#### Clear All Toasts
```csharp
// Remove all visible toasts immediately
ToastService.ClearAll();
```

#### Thread Safety
The service automatically handles UI thread marshalling:

```csharp
// Safe to call from background threads
Task.Run(() => 
{
    // Do some work...
    ToastService.ShowSuccess("Background task completed!");
});
```

## Best Practices

### ✅ DO:
- Use `ShowSuccess` for successful CRUD operations
- Use `ShowError` for exceptions and failed operations
- Use `ShowWarning` for confirmable actions or potential issues
- Use `ShowInfo` for general notifications
- Keep messages concise (one line preferred)
- Show toasts for user-initiated actions

### ❌ DON'T:
- Don't show toasts for every background operation
- Don't use for critical errors that require user action (use MessageBox)
- Don't show multiple toasts for the same event
- Avoid very long messages (max ~60 characters recommended)

## Customization

### Change Auto-Dismiss Duration
In `ToastNotification.xaml.cs`, modify the timer:

```csharp
// Change 3000ms (3 seconds) to desired duration
_autoCloseTimer = new System.Threading.Timer(_ =>
{
    Dispatcher.Invoke(Hide);
}, null, 5000, System.Threading.Timeout.Infinite); // Now 5 seconds
```

### Change Max Visible Toasts
In `ToastHost.xaml.cs`:

```csharp
private const int MaxVisibleToasts = 6; // Increase from 4 to 6
```

### Customize Colors
In `ToastNotification.xaml.cs`, modify the `UpdateAppearance()` method:

```csharp
case ToastType.Success:
    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Tailwind green-500
    break;
```

## Integration Checklist

- [x] ToastType enum created
- [x] ToastNotification UserControl created
- [x] ToastHost container created
- [x] ToastService static service created
- [x] ToastHost added to AdminDashboard.xaml
- [x] ToastService registered in AdminDashboard.xaml.cs
- [x] LoginViewModel updated to use ToastService
- [x] UsersPageViewModel updated to use ToastService
- [x] UserFormDialogViewModel updated to use ToastService

## Testing

1. Run the application
2. Try logging in with invalid credentials → Should show error toast
3. Create a new user → Should show success toast
4. Edit a user → Should show success toast
5. Deactivate a user → Should show success toast
6. Trigger an error (e.g., disconnect API) → Should show error toast
7. Test multiple toasts appearing simultaneously

## Troubleshooting

### Toast doesn't appear
- Ensure `ToastService.RegisterHost()` is called in window constructor
- Verify `ToastHost` is placed with `Panel.ZIndex="999"` in XAML
- Check that `ToastHost` is not hidden behind other UI elements

### Toast appears but animation is broken
- Verify EasingFunction resources are defined in ToastNotification.xaml
- Check that Storyboard resources are present

### Toast appears in wrong location
- Adjust `HorizontalAlignment`, `VerticalAlignment`, and `Margin` in ToastHost placement
- Ensure Grid.Column is set correctly for multi-column layouts

## Screenshots

Success Toast:
```
┌────────────────────────────────┐
│ ✓  User created successfully   │
└────────────────────────────────┘
```

Error Toast:
```
┌────────────────────────────────┐
│ ✕  Invalid credentials         │
└────────────────────────────────┘
```

Multiple Toasts:
```
┌────────────────────────────────┐
│ ✓  User updated successfully   │
└────────────────────────────────┘
┌────────────────────────────────┐
│ ℹ  Loading data...             │
└────────────────────────────────┘
```
