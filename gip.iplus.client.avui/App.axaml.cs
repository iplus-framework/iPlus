using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.iplus.client.avui.Views;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.iplus.client.avui;

public partial class App : Application
{
    static ACStartUpRoot _StartUpManager = null;
    public static App _GlobalApp = null;
    private LoginWindow _LoginWindow = null;

    #region internal Delegates

    public static new App Current
    {
        get { return Application.Current as App; }
    }

    #endregion

    #region c'tors
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }
    #endregion

    #region Global Exception-Handler
    // Handle the UI exceptions by showing a dialog box, and asking the user whether
    // or not they wish to abort execution.
    // NOTE: This exception cannot be kept from terminating the application - it can only 
    // log the event, and inform the user about it. 
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Exception ex = (Exception)e.ExceptionObject;
            if (ACRoot.SRoot != null)
            {
                if (ACRoot.SRoot.Messages != null)
                {
                    StringBuilder desc = new StringBuilder();
                    StackTrace stackTrace = new StackTrace(ex, true);
                    for (int i = 0; i < stackTrace.FrameCount; i++)
                    {
                        StackFrame sf = stackTrace.GetFrame(i);
                        desc.AppendFormat(" Method: {0}", sf.GetMethod());
                        desc.AppendFormat(" File: {0}", sf.GetFileName());
                        desc.AppendFormat(" Line Number: {0}", sf.GetFileLineNumber());
                        desc.AppendLine();
                    }

                    ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "0", ex.Message);
                    if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
                        ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "0", ex.InnerException.Message);

                    string stackDesc = desc.ToString();
                    ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "Stacktrace", stackDesc);
                }
            }
        }
        catch (Exception exc)
        {
            try
            {
                Msg userMsg = new Msg() { Message = "Fatal Non-UI Error. Could not write the error to the event log. Reason: " + exc.Message, MessageLevel = eMsgLevel.Info };
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(userMsg, eMsgButton.OK, null);
                vbMessagebox.ShowMessageBox();
            }
            finally
            {
                if (ACRoot.SRoot != null)
                    ACRoot.SRoot.ACDeInit();
                // Ist notwendig, damit die Anwendung auch wirklich als Prozess beendet wird
                if (App._GlobalApp.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.Shutdown();
            }
        }
    }
    #endregion

    #region Startup

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _GlobalApp = this;
        _StartUpManager = new ACStartUpRoot(new core.wpfservices.avui.WPFServices());


        IClassicDesktopStyleApplicationLifetime desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        // Create the AutoSuspendHelper.
        //var suspension = new AutoSuspendHelper(ApplicationLifetime);
        //RxApp.SuspensionHost.CreateNewAppState = () => new Settings();
        //RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
        //// Load the saved view model state.
        //var state = RxApp.SuspensionHost.GetAppState<Settings>();
        //suspension.OnFrameworkInitializationCompleted();
        var state = new Settings();

        if (desktop != null)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();


            // Show splash screen window
            //_LoginWindow = new LoginWindow();
            _LoginWindow = new LoginWindow(
                async () =>
                {
                    await HandleLoginAndStartup();
                },
                () =>
                {
                    ControlManager.RegisterImplicitStyles(this);

                    if (desktop != null)
                    {
                        desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
                        // If login was successful, show main window
                        if (ACRoot.SRoot != null)
                        {
                            desktop.MainWindow = new MainWindow();
                            desktop.MainWindow.Show();
                        }
                        else
                        {
                            desktop.Shutdown();
                        }
                    }
                    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                    {
                        // If login was successful, show main window
                        if (ACRoot.SRoot != null)
                            singleViewPlatform.MainView = new MainView();
                    }
                }
            );
            {
                DataContext = state;
            };
            _LoginWindow.Show();
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    public void ShutdownApplication()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }


    /// <summary>
    /// Lädt die VarioiplusLogin- und Window1-Klasse und stellt die Interaktionslogik
    /// für das UI der VarioiplusLogin-Klasse.
    /// </summary>
    /// <param name="varioiplusLogin">Eine Instanz der VarioiplusLogin-Klasse</param>
    /// <remarks>Wird in einer Instanz des ApplicationInitializeDelegate verarbeitet.</remarks>
    private async Task HandleLoginAndStartup()
    {
        string[] cmLineArg = System.Environment.GetCommandLineArgs();

        bool RegisterACObjects = false;
        bool PropPersistenceOff = false;
        bool WCFOff = cmLineArg.Contains("/" + Const.StartupParamWCFOff);
        bool simulation = cmLineArg.Contains("/" + Const.StartupParamSimulation);
        bool fullscreen = cmLineArg.Contains("/" + Const.StartupParamFullscreen);
        string UserName = "";
        string PassWord = "";
        eWpfTheme wpfTheme = eWpfTheme.Gip; //gip.iplus.client.Properties.Settings.Default.WpfTheme;

        if (!cmLineArg.Contains("/autologin"))
        {
//            UserName = gip.iplus.client.Properties.Settings.Default.User;
//#if DEBUG
//            PassWord = gip.iplus.client.Properties.Settings.Default.Password;
//#endif
        }

        if (cmLineArg.Contains("-controlLoad=True"))
            _LoginWindow.IsLoginWithControlLoad = true;

        String errorMsg = "";
        for (int i = 0; i < 3; i++)
        {
            if (!cmLineArg.Contains("/autologin") || i > 0)
            {
                _LoginWindow.DisplayLogin(true, UserName, PassWord, wpfTheme, errorMsg);
                _LoginWindow.GetLoginResult(ref UserName, ref PassWord, ref RegisterACObjects, ref PropPersistenceOff);
                wpfTheme = _LoginWindow.WpfTheme;
                errorMsg = "";
                _LoginWindow.DisplayLogin(false, "", "", wpfTheme, errorMsg);
            }
            else
            {
                if (i > 0)
                    break;
                //UserName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                //PassWord = "autologin";
            }

            ControlManager.WpfTheme = wpfTheme;
            short result = _StartUpManager.LoginUser(UserName, PassWord, RegisterACObjects, PropPersistenceOff, ref errorMsg, WCFOff, simulation, fullscreen);
            if (result == 1)
            {
                if (!cmLineArg.Contains("/autologin"))
                {
//                    gip.iplus.client.Properties.Settings.Default.User = UserName;
//#if DEBUG
//                    gip.iplus.client.Properties.Settings.Default.Password = PassWord;
//#endif
//                    gip.iplus.client.Properties.Settings.Default.WpfTheme = wpfTheme;
//                    gip.iplus.client.Properties.Settings.Default.Save();
                }

                break;
            }
            // Keine Lizenz
            if (result == -1)
                break;
        }
    }


//    private void License_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
//    {
//        if (e != null && e.PropertyName == "IsTrial")
//        {
//            Dispatcher.Invoke(() =>
//            {
//                UpdateLicenseTitle();
//            }, DispatcherPriority.Normal);
//        }
//    }

//    private void UpdateLicenseTitle()
//    {
//        string info = "";
//#if DEBUG
//        info = "DEBUG";
//#else
//                if (ACRoot.SRoot.Environment.License.IsTrial)
//                    info = "Trial";
//                else if (ACRoot.SRoot.Environment.License.IsDeveloper)
//                    info = "Development";
//                else if (ACRoot.SRoot.Environment.License.IsRemoteDeveloper)
//                    info = "Remote development";
//#endif

//        Application.Current.MainWindow.Title = String.Format("iPlus{3}({0}, {1}) {2}", ACRoot.SRoot.Environment.User.VBUserName,
//                                                                  ACRoot.SRoot.Environment.DatabaseName, info, ACRoot.SRoot.Environment.License.LicensedToTitle);
//    }
    #endregion
}