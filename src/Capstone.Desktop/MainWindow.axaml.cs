using Avalonia.Controls;
using Capstone.Core.ViewModels;

namespace Capstone.Desktop;

public partial class MainWindow : Window
{
    private bool _savedOnClose;

    public MainWindow() => InitializeComponent();

    private BarViewModel? Bar => DataContext as BarViewModel;

    // Loading is awaited, so the window is on screen (and responsive) while the disk works.
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (Bar is { } bar)
            await bar.Roster.LoadAsync(Path.Combine(AppContext.BaseDirectory, "data", "personalities.json"));
    }

    // Closing waits for the save: hold the close, save, then close for real.
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_savedOnClose || Bar is not { } bar) return;

        e.Cancel = true;
        await bar.Roster.SaveAsync();
        _savedOnClose = true;
        Close();
    }
}
