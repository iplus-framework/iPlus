// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace gip.tool.installerAndUpdater
{
    internal static class Program
    {
        // Avalonia must be initialised before any UI type is touched, so nothing
        // else may run here.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                if (OperatingSystem.IsLinux())
                {
                    // Avalonia's X11 backend expects GDK_BACKEND=x11; a Wayland
                    // session would otherwise make gtk_init_check fail.
                    string? gdkBackend = System.Environment.GetEnvironmentVariable("GDK_BACKEND");
                    if (string.IsNullOrWhiteSpace(gdkBackend)
                        || gdkBackend.Contains("wayland", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Environment.SetEnvironmentVariable("GDK_BACKEND", "x11");
                    }
                }

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Startup failed: " + ex);
            }
        }

        // Avalonia configuration; also used by the visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI(_ => { })
                .RegisterReactiveUIViewsFromEntryAssembly()
                .WithInterFont()
                .LogToTrace();
        }
    }
}
