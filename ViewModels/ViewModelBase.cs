using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibraryManagementSystem.ViewModels;


/// Base class for all ViewModels implementing INotifyPropertyChanged

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    protected void SetStatus(string message, bool isError = false)
    {
        StatusMessage = message;
        IsError = isError;
    }

    private bool _isError;
    public bool IsError
    {
        get => _isError;
        set => SetProperty(ref _isError, value);
    }
}
