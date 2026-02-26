using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using System;

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

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        BackRequested += MainActivity_BackRequested;
        
        if (this.Content != null && this.Content is UserControl) 
        {
            UserControl rootControl = (UserControl)this.Content;
            LoginView? loginView = rootControl.Content as LoginView;

            if (loginView != null)
            {
                loginView.LoginCancelled += (s, e) =>
                {
                    FinishAffinity();
                    Java.Lang.JavaSystem.Exit(0);
                };
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        BackRequested -= MainActivity_BackRequested;
    }

    private void MainActivity_BackRequested(object? sender, AndroidBackRequestedEventArgs e)
    {
        e.Handled = true;

        if (this.Content != null && this.Content is UserControl)
        {
            UserControl rootControl = (UserControl)this.Content;
            MainSingleView? mainSingleView = rootControl.Content as MainSingleView;

            if (mainSingleView != null)
            {
                if (mainSingleView.CanClose)
                {
                    FinishAffinity();
                    Java.Lang.JavaSystem.Exit(0);
                    return;
                }

                mainSingleView.BackButton_Click(sender, new Avalonia.Interactivity.RoutedEventArgs());
            }
        }
    }
}
