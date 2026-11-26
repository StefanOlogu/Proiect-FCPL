using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Capstone.Core.Storage;
using Capstone.Core.ViewModels;

namespace Capstone.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var roster = new PersonalityRoster();
            var window = new MainWindow
            {
                DataContext = new BarViewModel(new RosterViewModel(roster), new NightShiftViewModel(roster)),
            };
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
