using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using gip.iplus.client.avui;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        // Opt-in incremental design loading for the browser: WASM is single-threaded,
        // so synchronous VBVisual load cascades freeze the whole tab the same way they
        // trigger the Android ANR. Deferring each design load to a Background-priority
        // tick keeps the UI responsive (see VBDesign/VBVisual LoadDesign).
        gip.core.layoutengine.avui.VBDesign.IncrementalDesignLoading = true;
        return BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}