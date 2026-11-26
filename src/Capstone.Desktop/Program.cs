using Avalonia;

namespace Capstone.Desktop;

// Night Shift at Bar "La Faleză", in a window. Run it with:
//   dotnet run --project src/Capstone.Desktop
internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Rider's XAML previewer calls this too.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
