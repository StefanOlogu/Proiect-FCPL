using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Capstone.Core.ViewModels;

/// <summary>The INotifyPropertyChanged plumbing every view model shares.</summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Sets the field and announces it — unless it already had that value, then nothing happens.</summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
