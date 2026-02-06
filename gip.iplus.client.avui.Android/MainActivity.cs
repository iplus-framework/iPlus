using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace gip.iplus.client.avui.Android;

[Activity(
    Label = "gip.iplus.client.avui.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
 #if AVALONIAFORK
public class MainActivity : AvaloniaMainActivity
 #else   
public class MainActivity : AvaloniaMainActivity<App>
#endif
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

}
