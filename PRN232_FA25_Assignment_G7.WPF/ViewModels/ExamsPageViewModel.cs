using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PRN232_FA25_Assignment_G7.WPF.ViewModels;

public class ExamsPageViewModel : INotifyPropertyChanged
{
    private string _title = "Exam Management";

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public ExamsPageViewModel()
    {
        Title = "Exam Management";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
