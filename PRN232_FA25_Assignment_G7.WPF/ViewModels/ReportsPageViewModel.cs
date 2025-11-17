using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class ReportsPageViewModel : INotifyPropertyChanged
{
    private string _title = "Reports & Analytics";

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public ReportsPageViewModel()
    {
        Title = "Reports & Analytics";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
