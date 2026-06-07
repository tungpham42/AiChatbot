using Avalonia;
using System;
using Velopack; // 1. Add the Velopack namespace

namespace AiChatbot;

class Program
{
    [STAThread]
    public static void Main(string[] args) 
    {
        // 2. Add this line BEFORE your Avalonia app starts!
        // This allows Velopack to intercept setup events like creating shortcuts.
        VelopackApp.Build().Run();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .LogToTrace();
}