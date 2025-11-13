using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using CoreWCF.IdentityModel.Tokens;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.iplus.client.avui.Views;
using Org.BouncyCastle.Bcpg.OpenPgp;
using ReactiveUI;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace gip.iplus.client.avui;

public partial class App : Application
{
    static ACStartUpRoot _StartUpManager = null;
    public static App _GlobalApp = null;
    private LoginWindow _LoginWindow = null;
    private LoginView _LoginView = null;
    private Settings _AppSettings = null;

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

        if (desktop != null)
        {
            // Create the AutoSuspendHelper.
            var suspension = new AutoSuspendHelper(ApplicationLifetime);
            RxApp.SuspensionHost.CreateNewAppState = () => new Settings();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
            // Load the saved view model state.
            _AppSettings = RxApp.SuspensionHost.GetAppState<Settings>();
            suspension.OnFrameworkInitializationCompleted();

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();


            // Show splash screen window
            _LoginWindow = new LoginWindow(
                () =>
                {
                    HandleLoginAndStartup();
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
                            var mainWindow = new MainWindow();
                            UpdateLicenseTitle(mainWindow);
                            desktop.MainWindow = mainWindow;
                            desktop.MainWindow.Show();
                        }
                        else
                        {
                            desktop.Shutdown();
                        }
                    }
                }
            );
            _LoginWindow.DataContext = _AppSettings;
            _LoginWindow.Show();
        }
        else
        {
            ISingleViewApplicationLifetime singleViewPlatform = ApplicationLifetime as ISingleViewApplicationLifetime;
            if (singleViewPlatform != null)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                _LoginView = new LoginView(
                () =>
                {
                    HandleLoginAndStartup();
                },
                () =>
                {
                    ControlManager.RegisterImplicitStyles(this);

                    if (singleViewPlatform != null)
                    {
                        // If login was successful, show main window
                        if (ACRoot.SRoot != null)
                        {
                            singleViewPlatform.MainView = null;
                            singleViewPlatform.MainView = new MainSingleView();
                        }
                    }
                }
            );

                _AppSettings = new Settings();
                _LoginView.DataContext = _AppSettings;


                singleViewPlatform.MainView = _LoginView;
            }
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
    private void HandleLoginAndStartup()
    {
        string[] cmLineArg = System.Environment.GetCommandLineArgs();

        bool registerACObjects = false;
        bool propPersistenceOff = false;
        bool wcfOff = cmLineArg.Contains("/" + Const.StartupParamWCFOff);
        bool simulation = cmLineArg.Contains("/" + Const.StartupParamSimulation);
        bool fullscreen = cmLineArg.Contains("/" + Const.StartupParamFullscreen);

        if (ACRoot.SRoot != null)
            ACRoot.SRoot.Environment.License.PropertyChanged += License_PropertyChanged;

        if (!cmLineArg.Contains("/autologin"))
        {
#if DEBUG
            if (string.IsNullOrEmpty(_AppSettings.UserName))
                _AppSettings.UserName = "superuser";
            if (string.IsNullOrEmpty(_AppSettings.Password))
                _AppSettings.Password = "superuser";
#endif
        }

        if (cmLineArg.Contains("-controlLoad=True"))
            _LoginWindow.IsLoginWithControlLoad = true;

        String errorMsg = "";
        for (int i = 0; i < 3; i++)
        {
            if (!cmLineArg.Contains("/autologin") || i > 0)
            {
                if (_LoginWindow != null)
                {
                    _LoginWindow.DisplayLogin(true, errorMsg);
                    _LoginWindow.WaitOnLoginResult();
                    errorMsg = "";
                    _LoginWindow.DisplayLogin(false, errorMsg);
                }
                else if (_LoginView != null)
                {
                    _LoginView.DisplayLogin(true, errorMsg);
                    _LoginView.WaitOnLoginResult();

                    ACValueItemList settings = CommandLineHelper.Settings;

                    string dbSource = settings.Where(c => c.ACCaptionTranslation == nameof(LoginView.DatabaseSource)).FirstOrDefault()?.Value as string;
                    string dbName = settings.Where(c => c.ACCaptionTranslation == nameof(LoginView.DatabaseName)).FirstOrDefault()?.Value as string;
                    string dbUser = settings.Where(c => c.ACCaptionTranslation == nameof(LoginView.DatabaseUser)).FirstOrDefault()?.Value as string;
                    string dbPass = settings.Where(c => c.ACCaptionTranslation == nameof(LoginView.DatabasePassword)).FirstOrDefault()?.Value as string;

                    var config = CommandLineHelper.ConfigCurrentDir;
                    var existingConnection = config?.ConnectionStrings?.ConnectionStrings["iPlusV5_Entities"];

                    string connSettingsFormat = @"Integrated Security=True; Encrypt=False; data source={0}; initial catalog={1}; Trusted_Connection=False; persist security info=True; user id={2}; password={3}; multipleactiveresultsets=True; application name=iPlus";

                    var setting = new ConnectionStringSettings(
                            "iPlusV5_Entities",
                            string.Format(connSettingsFormat, dbSource, dbName, dbUser, dbPass),
                            "System.Data.SqlClient");

                    if (existingConnection != null)
                    {
                        existingConnection.ProviderName = setting.ProviderName;
                        existingConnection.ConnectionString = setting.ConnectionString;
                    }
                    else
                    {
                        config.ConnectionStrings.ConnectionStrings.Add(setting);
                    }

                    errorMsg = "";
                    _LoginView.DisplayLogin(false, errorMsg);
                }
            }
            else
            {
                if (i > 0)
                    break;
                // TODO: Windows oder Linux automatic login
                //UserName = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                //PassWord = "autologin";
            }

            ControlManager.WpfTheme = _AppSettings.WPFTheme;
            short result = _StartUpManager.LoginUser(_AppSettings.UserName, _AppSettings.Password, registerACObjects, propPersistenceOff, ref errorMsg, wcfOff, simulation, fullscreen);
            if (result == 1)
            {
                if (cmLineArg.Contains("/autologin"))
                    RxApp.SuspensionHost.ShouldPersistState = Observable.Never<IDisposable>();
                        
                    break;
            }
            // No License
            if (result == -1)
                break;
        }
    }


    private void License_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e != null && e.PropertyName == "IsTrial")
        {
            IClassicDesktopStyleApplicationLifetime desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop != null)
            {
                if (!Dispatcher.UIThread.CheckAccess())
                {
                    Dispatcher.UIThread.InvokeAsync(() => UpdateLicenseTitle(desktop.MainWindow), DispatcherPriority.Send);
                }
                else
                    UpdateLicenseTitle(desktop.MainWindow);
            }
        }
    }

    private void UpdateLicenseTitle(Window mainWindow)
    {
        string info = "";
#if DEBUG
        info = "DEBUG";
#else
                if (ACRoot.SRoot.Environment.License.IsTrial)
                    info = "Trial";
                else if (ACRoot.SRoot.Environment.License.IsDeveloper)
                    info = "Development";
                else if (ACRoot.SRoot.Environment.License.IsRemoteDeveloper)
                    info = "Remote development";
#endif

        mainWindow.Title = String.Format("iPlus{3}({0}, {1}) {2}", ACRoot.SRoot.Environment.User.VBUserName,
                                                                  ACRoot.SRoot.Environment.DatabaseName, info, ACRoot.SRoot.Environment.License.LicensedToTitle);
    }
    #endregion
}