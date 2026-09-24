using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using SkiaSharp;

namespace gip.iplus.client.avui.Desktop;

public static class TestFontDebug
{
    public static void Run()
    {
        Console.WriteLine("=== Running Font Diagnostics ===");
        try
        {
            var skFmDefault = SKFontManager.Default;
            Console.WriteLine($"Default FontManager: count={skFmDefault.GetFontFamilies().Length}, first='{skFmDefault.GetFontFamilies().FirstOrDefault()}'");

            var skFmCustom = SKFontManager.CreateDefault();
            Console.WriteLine($"CreateDefault FontManager: count={skFmCustom.GetFontFamilies().Length}, first='{skFmCustom.GetFontFamilies().FirstOrDefault()}'");

            var fontconfigPath = Environment.GetEnvironmentVariable("FONTCONFIG_PATH");
            var fontconfigFile = Environment.GetEnvironmentVariable("FONTCONFIG_FILE");
            Console.WriteLine($"FONTCONFIG_PATH='{fontconfigPath}', FONTCONFIG_FILE='{fontconfigFile}'");

            // Test loading font from file directly
            if (File.Exists("/usr/share/fonts/truetype/msttcorefonts/arial.ttf"))
            {
                using var stream = File.OpenRead("/usr/share/fonts/truetype/msttcorefonts/arial.ttf");
                var tf = skFmDefault.CreateTypeface(stream);
                Console.WriteLine($"CreateTypeface from arial.ttf: {(tf != null ? tf.FamilyName : "null")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Skia exception: {ex}");
        }

        Console.WriteLine("=== Done Font Diagnostics ===");
    }
}
