using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Avalonia.Threading;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.layoutengine.avui.Helperclasses;
using gip.iplus.client.avui.Views;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace gip.iplus.client.avui;

public partial class LoginView : UserControl
{
    #region c'tors

    public LoginView()
    {
        InitializeComponent();
        //this.WhenActivated(disposable => { });
    }

    public LoginView(Func<Task> loginAction, Action mainAction, IEnumerable<IExternalLoginProvider> externalLoginProviders = null) : this()
    {
        listboxInfo.ItemsSource = MsgDetails;
        listboxInfo.DisplayMemberBinding = new Avalonia.Data.Binding("Message");
        selTheme.ItemsSource = System.Enum.GetValues(typeof(eWpfTheme));
        _LoginAction = loginAction;
        _ShowMainWindowAction = mainAction;
        _externalLoginProviders = (externalLoginProviders ?? Enumerable.Empty<IExternalLoginProvider>()).ToList();
    }

    #endregion

    #region Properties

    protected object _WaitOnOkClick = new object();
    int _CountAttempts = 0;

    /// <summary>
    /// True while the SettingsGrid is shown because no database connection is configured.
    /// The login action is deferred until the user has saved the connection settings.
    /// </summary>
    private bool _AwaitingSettingsSave;

    private readonly Func<Task> _LoginAction;
    private readonly Action _ShowMainWindowAction;
    private IReadOnlyList<IExternalLoginProvider> _externalLoginProviders = Array.Empty<IExternalLoginProvider>();
    private CancellationTokenSource _externalLoginCts;
    private int _externalLoginStarted = 0;
    private int _externalLoginCompleted = 0;

    private Settings UserSettings => DataContext as Settings;

    public event EventHandler LoginCancelled;
    public event EventHandler LoginStarted;

    private TopLevel _topLevel;
    private IInsetsManager _insetsManager;
    private IInputPane _inputPane;

    #region Properties => Login


    #endregion

    #region Properties => Progress

    public MsgWithDetails InfoMessage
    {
        get
        {
            return Messages.GlobalMsg;
        }
    }

    ObservableCollection<Msg> _Messages = null;
    private bool _CollectionChangedSubscr = false;
    public ObservableCollection<Msg> MsgDetails
    {
        get
        {
            if (_Messages == null)
            {
                if (InfoMessage.MsgDetails != null)
                {
                    if (InfoMessage.MsgDetails is ObservableCollection<Msg>)
                    {
                        _CollectionChangedSubscr = true;
                        (InfoMessage.MsgDetails as ObservableCollection<Msg>).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_Messages_CollectionChanged);
                    }
                }
                _Messages = new ObservableCollection<Msg>();
            }

            return _Messages;
        }
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Guards against OnLoaded firing multiple times (attach/detach cycles), which would
    /// start the login action twice or toggle the settings grid back.
    /// </summary>
    private bool _LoginFlowInitialized;

    /// <summary>
    /// Set while WaitOnLoginResult blocks; completed by ButtonLogin/ButtonCancel.
    /// Replaces the former Monitor.Enter/Exit choreography, which is fragile when
    /// OnLoaded fires more than once (reentrant monitor counts).
    /// </summary>
    private TaskCompletionSource<bool>? _LoginResultTcs;

    /// <summary>
    /// iPlus fork fix (browser WASM auto-login): a Login click that arrives BEFORE any
    /// WaitOnLoginResult waiter exists (the auto-login bypass clicks while the login
    /// action is still starting up) would silently lose its result — the later-created
    /// TCS would never be completed and the login action would hang forever. Remember
    /// the click here; WaitOnLoginResult consumes it instead of waiting.
    /// </summary>
    private bool _PendingLoginResult;

    // =======================================================================
    // TEMPORARY TEST BYPASS (SQL Tunnel Option B debugging) — REMOVE once the
    // browser login works end-to-end. See docs/SqlTunnelProvider-Design-Draft.md §10.
    // Rationale: Avalonia renders to canvas, so agent browser tools cannot type into
    // the textboxes, and WASM settings do not persist between reloads. This bypass
    // injects the connection settings + login credentials programmatically and
    // presses Login automatically on startup.
    // =======================================================================
    private const bool AutoLoginBypassEnabled = false; // TEMP: agent-driven Android debugging
    private const string BypassDbSource = "sqltunnel";               // placeholder; relay config decides the real target
    private const string BypassDbName = "iPlusV5";
    private const string BypassDbUser = "gip";
    private const string BypassDbPassword = "netspirit";
    // WebSocket transport (Android/desktop), NOT the HTTP sync shim
    private const string BypassTunnelUrl = "ws://192.168.0.196:8751/sqltunnel"; 
    //private const string BypassTunnelUrl = "http://localhost:8751/sqltunnel-sync"; 
    private const string BypassLoginUser = "superuser";              // iPlus login credentials
    private const string BypassLoginPassword = "superuser";          // adjust if different
    // true = mobile "hamburger" mode (MainSingleView/DUMainMobile);
    // false = desktop mode (MainView with menu bar/DUMain) — also on browser.
    private const bool BypassSingleViewMode = true;

    private static void SetBypassSetting(string name, object value)
    {
        ACValueItem item = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == name).FirstOrDefault();
        if (item == null)
        {
            CommandLineHelper.Settings.Add(new ACValueItem(name, value, null));
        }
        else
        {
            item.Value = value;
        }
    }

    private void ApplyAutoLoginBypass()
    {
        SetBypassSetting(nameof(DatabaseSource), BypassDbSource);
        SetBypassSetting(nameof(DatabaseName), BypassDbName);
        SetBypassSetting(nameof(DatabaseUser), BypassDbUser);
        SetBypassSetting(nameof(DatabasePassword), BypassDbPassword);
        SetBypassSetting(nameof(DatabaseTunnelUrl), BypassTunnelUrl);
        // Force desktop mode (menu bar, DUMain) while the bypass is active: a previously
        // saved MobileModeEnabled=true persists in localStorage (BrowserSettingsStorage)
        // and would otherwise override the bypass default on every reload.
        SetBypassSetting(nameof(MobileModeEnabled), true);
        try
        {
            // Persist the injected connection settings (on Browser/WASM this goes to
            // localStorage via CommandLineHelper.SaveSettingsToStorage, see BrowserSettingsStorage).
            CommandLineHelper.SaveSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: could not persist bypass settings. {ex.GetType().Name}: {ex.Message}");
        }
        Console.WriteLine("LoginView: AUTO-LOGIN BYPASS active — settings injected, login will start automatically.");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (!_LoginFlowInitialized)
        {
            _LoginFlowInitialized = true;
            if (AutoLoginBypassEnabled)
            {
                ApplyAutoLoginBypass();
                // Apply connection settings (incl. tunnel token + Database.TunnelUrl)
                // BEFORE any Database static init: ACRoot.SRoot access below triggers
                // the Database static constructor which opens the global context — it
                // must already carry the Tunnel URL or it fails with a DNS error that
                // then stays cached in the type initializer.
                UserSettings.UserName = BypassLoginUser;
                UserSettings.Password = BypassLoginPassword;
                try
                {
                    ApplyConnectionSettingsFromHelper();
                }
                catch (Exception exApply)
                {
                    var msg = exApply.Message;
                    var inner = exApply.InnerException;
                    while (inner != null) { msg += " Inner: " + inner.Message; inner = inner.InnerException; }
                    Console.WriteLine($"LoginView: early ApplyConnectionSettingsFromHelper failed. {msg}\n{exApply}");
                }
            }
            bool configured = HasDatabaseConnectionConfigured();
            Console.WriteLine($"LoginView: OnLoaded, database configured: {configured}");
            if (configured)
            {
                _ = DoLoginActionAsync();
                if (AutoLoginBypassEnabled)
                {
                    // Fire LoginStarted + complete the login-result TCS, exactly like a
                    // physical click on the Login button would.
                    UserSettings.UserName = BypassLoginUser;
                    UserSettings.Password = BypassLoginPassword;
                    ButtonLogin_Click(this, new RoutedEventArgs());
                }
            }
            else
            {
                // No database connection configured (e.g. Browser/WASM or first start without
                // ConnectionStrings.config): show the SettingsGrid first and start the login
                // action only after the user has saved the connection settings.
                LoadSettings();
                SwitchSettingsView();
                _AwaitingSettingsSave = true;
            }
        }
        base.OnLoaded(e);

        // Get TopLevel after the view is loaded (not in constructor!)
        _topLevel = TopLevel.GetTopLevel(this);

        if (_topLevel != null)
        {
            _insetsManager = _topLevel.InsetsManager;
            _inputPane = _topLevel.InputPane;

            // Subscribe to keyboard state changes
            if (_inputPane != null)
            {
                _inputPane.StateChanged += InputPane_StateChanged;
            }
        }
    }

    /// <summary>
    /// Checks whether a database connection is available: either the user has already
    /// stored connection settings (SettingsGrid / settingsiPlus.config, used by Android
    /// and Browser clients) or the app config contains the iPlusV5_Entities connection
    /// string (desktop clients with ConnectionStrings.config / App.config).
    /// </summary>
    private bool HasDatabaseConnectionConfigured()
    {
        try
        {
            string? dbSource = CommandLineHelper.Settings
                .Where(c => c.ACCaptionTranslation == nameof(DatabaseSource))
                .FirstOrDefault()?.Value as string;
            if (!String.IsNullOrWhiteSpace(dbSource))
                return true;

            var setting = ConfigurationManager.ConnectionStrings[Database.C_DefaultContainerName];
            return !String.IsNullOrWhiteSpace(setting?.ConnectionString);
        }
        catch (Exception)
        {
            // ConfigurationManager may be unavailable/empty on some platforms - treat as not configured.
            return false;
        }
    }

    private void InputPane_StateChanged(object sender, InputPaneStateEventArgs e)
    {
        if (_inputPane is not null &&
            _insetsManager is not null)
        {
            var safeArea = _insetsManager.SafeAreaPadding;
            var occludedArea = _inputPane.OccludedRect;

            double topPadding = this.Padding.Top;
            // Combine safe area with keyboard height
            this.Padding = new Thickness(
                safeArea.Left,
                topPadding,
                safeArea.Right,
                occludedArea.Height
            );

            Control focusedElement = _topLevel.FocusManager?.GetFocusedElement() as Control;
            if (focusedElement != null)
            {
                try
                {
                    PixelPoint position = focusedElement.PointToScreen(new Point(0, focusedElement.Bounds.Height));
                    scrollViewer.Offset = new Vector(0, position.Y - _topLevel.Bounds.Height);
                }
                catch (Exception)
                {

                }
            }
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        StopExternalLoginWatcher();

        if (_CollectionChangedSubscr && InfoMessage.MsgDetails != null)
        {
            _CollectionChangedSubscr = false;
            (InfoMessage.MsgDetails as ObservableCollection<Msg>).CollectionChanged -= _Messages_CollectionChanged;
        }
        selTheme.ItemsSource = null;

        if (_inputPane != null)
        {
            _inputPane.StateChanged -= InputPane_StateChanged;
            _inputPane = null;
        }

        _insetsManager = null;
        _topLevel = null;


        base.OnUnloaded(e);
    }

    private async Task DoLoginActionAsync()
    {
        Console.WriteLine($"LoginView: DoLoginActionAsync starting (loginAction: {_LoginAction != null})");
        if (_LoginAction is not null)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await _LoginAction.Invoke();
                }
                catch (Exception ex)
                {
                    // Handle exceptions from the heavy load action
                    Console.WriteLine($"LoginView: login action failed. {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                    });
                }
            });
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _ShowMainWindowAction?.Invoke();
            //Close();
        });
    }

    #region Methods => Login

    /// <summary>
    /// 1. Call from App
    /// </summary>
    public void DisplayLogin(bool display, String errorMsg)
    {
        if (!this.ProgressGrid.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => DisplayLogin(display, errorMsg), DispatcherPriority.Send);
            return;
        }

        if (display)
        {
            _CountAttempts++;
            if (_CountAttempts <= 1)
            {
                //#if DEBUG
                //                TextboxPassword.Text = _Password;
                //#endif

                //selTheme.SelectedValue = new Binding() { Path = "WPFTheme" };
            }
            else
            {
                Msg msgLogon = Messages.GlobalMsg.MsgDetails.Where(c => c.Message == "DB-Connection failed!").FirstOrDefault();
                Msg userMsg;
                if (msgLogon != null)
                {
                    userMsg = new Msg() { Message = "Cannot connect to database. Check your connection string or rights connecting the database!", MessageLevel = eMsgLevel.Info };
                }
                else
                {
                    userMsg = new Msg() { Message = String.Format("User {0} doesn't exist or wrong password!", this.UserSettings.UserName), MessageLevel = eMsgLevel.Info };
                }
                if (!String.IsNullOrEmpty(errorMsg))
                    userMsg.Message += " // " + errorMsg;

                AsyncMessageBox.BeginMessageBoxAsync(userMsg.Message, "Info", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);

                UserSettings.Password = "";
            }

            Interlocked.Exchange(ref _externalLoginCompleted, 0);
            StartExternalLoginWatcher();

            ProgressGrid.IsVisible = false;
            LoginGrid.IsVisible = true;
        }
        else
        {
            StopExternalLoginWatcher();
            LoginGrid.IsVisible = false;
            ProgressGrid.IsVisible = true;
        }
    }

    /// <summary>
    /// 2. Call from App and waits (asynchronously - WASM forbids synchronous blocking waits!)
    /// on User-OK-click
    /// </summary>
    public async Task WaitOnLoginResult()
    {
        Console.WriteLine("LoginView: waiting for login result...");
        if (_PendingLoginResult)
        {
            // A Login click already happened before this waiter was created (auto-login
            // bypass) — consume it instead of waiting for a click that will never come.
            _PendingLoginResult = false;
            Console.WriteLine("LoginView: login result received: True (pending click)");
            return;
        }
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _LoginResultTcs = tcs;
        bool result = await tcs.Task;
        Console.WriteLine($"LoginView: login result received: {result}");
    }

    /// <summary>
    /// 3. User Clicks OK => Signal on GetLoginResult
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLogin_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("LoginView: Login button clicked");
        StopExternalLoginWatcher();
        try
        {
            LoginStarted?.Invoke(this, new EventArgs());
        }
        catch (Exception ex)
        {
            // Never skip the release below, otherwise the login action deadlocks.
            Console.WriteLine($"LoginView: LoginStarted handler failed. {ex.GetType().Name}: {ex.Message}");
        }

        if (_LoginResultTcs != null)
        {
            _LoginResultTcs?.TrySetResult(true);
        }
        else
        {
            // No waiter yet (login action still starting up) — remember the click.
            _PendingLoginResult = true;
        }
    }

    private void task_OnStatusChange(core.dbsyncer.Messages.BaseSyncMessage msg)
    {
        Msg internalMessage = new Msg();
        internalMessage.Message = msg.ToString();
        AddItems(new List<Msg>() { internalMessage });
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        StopExternalLoginWatcher();

        UserSettings.UserName = string.Empty;
        UserSettings.Password = string.Empty;
        
        if (LoginCancelled != null)
            LoginCancelled.Invoke(this, new EventArgs());

        _LoginResultTcs?.TrySetResult(false);
    }

    private void StartExternalLoginWatcher()
    {
        if (_externalLoginProviders.Count == 0)
            return;

        if (Interlocked.Exchange(ref _externalLoginStarted, 1) == 1)
            return;

        _externalLoginCts = new CancellationTokenSource();
        _ = ExternalLoginLoopSafeAsync(_externalLoginCts.Token);
    }

    private async Task ExternalLoginLoopSafeAsync(CancellationToken token)
    {
        try
        {
            await ExternalLoginLoopAsync(token);
        }
        catch
        {
            // Keep manual login available even if provider loop fails unexpectedly.
        }
    }

    private async Task ExternalLoginLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (IExternalLoginProvider provider in _externalLoginProviders)
            {
                ExternalLoginCredentials credentials = new ExternalLoginCredentials(string.Empty, string.Empty, provider.Name);
                try
                {
                    credentials = await provider.TryGetCredentialsAsync(token) ?? credentials;
                }
                catch
                {
                    // Keep manual login available even if provider calls fail intermittently.
                }

                if (!string.IsNullOrWhiteSpace(credentials.UserName)
                    && Interlocked.Exchange(ref _externalLoginCompleted, 1) == 0)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested || !LoginGrid.IsVisible || UserSettings == null)
                            return;

                        UserSettings.UserName = credentials.UserName;
                        UserSettings.Password = credentials.Password;

                        _LoginResultTcs?.TrySetResult(true);
                    });
                    return;
                }

                if (token.IsCancellationRequested)
                    return;
            }

            try
            {
                await Task.Delay(600, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }

    private void StopExternalLoginWatcher()
    {
        CancellationTokenSource cts = _externalLoginCts;
        _externalLoginCts = null;

        if (cts != null)
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch
            {
            }
        }

        Interlocked.Exchange(ref _externalLoginStarted, 0);
    }

    #endregion

    #region Methods => Settings

    private void LoadSettings()
    {
        ACValueItemList settings = CommandLineHelper.Settings;

        DatabaseSource.Text = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseSource)).FirstOrDefault()?.Value as string;
        DatabaseName.Text = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseName)).FirstOrDefault()?.Value as string;
        DatabaseUser.Text = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseUser)).FirstOrDefault()?.Value as string;
        DatabasePassword.Text = settings.Where(c => c.ACCaptionTranslation == nameof(DatabasePassword)).FirstOrDefault()?.Value as string;
        DatabaseTunnelUrl.Text = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseTunnelUrl)).FirstOrDefault()?.Value as string;
        bool? singleViewEnabled = settings.Where(c => c.ACCaptionTranslation == nameof(MobileModeEnabled)).FirstOrDefault()?.Value as bool?;
        
        if (singleViewEnabled.HasValue)
            MobileModeEnabled.IsChecked = singleViewEnabled.Value;
        else
            MobileModeEnabled.IsChecked = true;

        // Temporary setting for debugging with agent with playwright. Needs to bee removed after testing:
        if (AutoLoginBypassEnabled)
            MobileModeEnabled.IsChecked = false;
    }

    private void SwitchSettingsView()
    {
        SettingsGrid.IsVisible = !SettingsGrid.IsVisible;
        LoginGrid.IsVisible = !SettingsGrid.IsVisible;

        if (SettingsGrid.IsVisible)
            iPlusLogo.Height = 150;
        else
            iPlusLogo.Height = 400;
    }

    private void Image_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (ProgressGrid.IsVisible)
            return;

        LoadSettings();

        SwitchSettingsView();
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        ACValueItem dbSourceVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseSource)).FirstOrDefault();
        if (dbSourceVal == null)
        {
            dbSourceVal = new ACValueItem(nameof(DatabaseSource), DatabaseSource.Text, null);
            CommandLineHelper.Settings.Add(dbSourceVal);
        }
        else
        {
            dbSourceVal.Value = DatabaseSource.Text;
        }

        ACValueItem dbNameVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseName)).FirstOrDefault();
        if (dbNameVal == null)
        {
            dbNameVal = new ACValueItem(nameof(DatabaseName), DatabaseName.Text, null);
            CommandLineHelper.Settings.Add(dbNameVal);
        }
        else
        {
            dbNameVal.Value = DatabaseName.Text;
        }

        ACValueItem dbUserVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseUser)).FirstOrDefault();
        if (dbUserVal == null)
        {
            dbUserVal = new ACValueItem(nameof(DatabaseUser), DatabaseUser.Text, null);
            CommandLineHelper.Settings.Add(dbUserVal);
        }
        else
        {
            dbUserVal.Value = DatabaseUser.Text;
        }

        ACValueItem dbPasswordVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabasePassword)).FirstOrDefault();
        if (dbPasswordVal == null)
        {
            dbPasswordVal = new ACValueItem(nameof(DatabasePassword), DatabasePassword.Text, null);
            CommandLineHelper.Settings.Add(dbPasswordVal);
        }
        else
        {
            dbPasswordVal.Value = DatabasePassword.Text;
        }

        ACValueItem dbTunnelUrlVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseTunnelUrl)).FirstOrDefault();
        if (dbTunnelUrlVal == null)
        {
            dbTunnelUrlVal = new ACValueItem(nameof(DatabaseTunnelUrl), DatabaseTunnelUrl.Text, null);
            CommandLineHelper.Settings.Add(dbTunnelUrlVal);
        }
        else
        {
            dbTunnelUrlVal.Value = DatabaseTunnelUrl.Text;
        }

        ACValueItem singleViewVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(MobileModeEnabled)).FirstOrDefault();
        if (singleViewVal == null)
        {
            // Store the user's current choice — do NOT force true here (that discarded
            // an unchecked "desktop mode" selection on the first save).
            singleViewVal = new ACValueItem(nameof(MobileModeEnabled), MobileModeEnabled.IsChecked, null);
            CommandLineHelper.Settings.Add(singleViewVal);
        }
        else
        {
            singleViewVal.Value = MobileModeEnabled.IsChecked;
        }

        // Switch back to the login grid first, so the user always gets feedback
        // even if persisting the settings fails (e.g. read-only file system on WASM).
        SwitchSettingsView();

        try
        {
            CommandLineHelper.SaveSettings();
        }
        catch (Exception ex)
        {
            // Persisting is best-effort: on Browser/WASM the virtual file system may be
            // read-only or non-persistent. The settings remain in memory for this session
            // (ApplyConnectionSettingsFromHelper reads them from CommandLineHelper.Settings).
            Console.WriteLine($"LoginView: could not persist settings. {ex.GetType().Name}: {ex.Message}");
        }

        // If the settings grid was shown on startup because no connection was configured,
        // start the (deferred) login action now that the user has saved the settings.
        Console.WriteLine($"LoginView: Save clicked, awaitingSettingsSave: {_AwaitingSettingsSave}");
        if (_AwaitingSettingsSave)
        {
            _AwaitingSettingsSave = false;
            _ = DoLoginActionAsync();
        }
    }

    /// <summary>
    /// Injects the connection settings entered in the SettingsGrid (persisted via
    /// CommandLineHelper.Settings) into the app configuration, so that
    /// <see cref="Database.ConnectionString"/> resolves them. Mirrors
    /// gip.iplus.client.avui.Android/MainActivity.LoginView_LoginStarted.
    /// </summary>
    public void ApplyConnectionSettingsFromHelper()
    {
        ACValueItemList settings = CommandLineHelper.Settings;

        string? dbSource = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseSource)).FirstOrDefault()?.Value as string;
        string? dbName = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseName)).FirstOrDefault()?.Value as string;
        string? dbUser = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseUser)).FirstOrDefault()?.Value as string;
        string? dbPass = settings.Where(c => c.ACCaptionTranslation == nameof(DatabasePassword)).FirstOrDefault()?.Value as string;
        string? dbTunnelUrl = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseTunnelUrl)).FirstOrDefault()?.Value as string;

        if (String.IsNullOrWhiteSpace(dbSource) && String.IsNullOrWhiteSpace(dbName))
            return;

        // SQL auth when credentials are entered; Integrated Security only when they are not.
        // With Integrated Security=True SqlClient IGNORES user id/password and attempts an
        // SSPI/Windows login — on Browser/WASM there is no SSPI support, so the login stalls
        // forever after the server's SSPI token (observed 2026-10-10: prelogin OK, login7
        // sent, server SSPI token received, then endless recv polling with no progress).
        bool useSqlAuth = !String.IsNullOrWhiteSpace(dbUser) || !String.IsNullOrWhiteSpace(dbPass);
        // MARS re-enabled (2026-10-10): the original browser stall was NOT the MARS demux
        // itself but SniPacket.ReadFromStreamAsync queueing its continuation to the thread
        // pool (TaskScheduler.Default), which does not exist on single-threaded WASM. The
        // fork now runs the continuation inline for synchronously-completing reads, so MARS
        // works on the browser like the desktop sync path (verified: desktop spike sync+MARS
        // over the relay = SPIKE OK). iPlus core requires MARS (nested queries in
        // ACActivator/ACInit would otherwise fail with "open DataReader" errors).
        string connSettingsFormat = (useSqlAuth
            ? @"Integrated Security=False; Encrypt=False; data source={0}; initial catalog={1}; Trusted_Connection=False; persist security info=True; user id={2}; password={3}; application name=iPlus; multipleactiveresultsets=True"
            : @"Integrated Security=True; Encrypt=False; data source={0}; initial catalog={1}; application name=iPlus; multipleactiveresultsets=True");
        string connectionString = String.Format(connSettingsFormat, dbSource, dbName, dbUser, dbPass);
        string tunnelUrlWithAuth = null;

        // SQL Tunnel (browser/WASM): when a tunnel URL is configured, the forked
        // Microsoft.Data.SqlClient routes the TDS stream over the WebSocket relay and the
        // data source is only a placeholder (the relay's SqlServer/SqlPort config decides
        // the real target).
        // Tunnel auth (docs/SqlTunnel-OptionB-Implementation.md §9.21): the iPlus login
        // credentials (stage 2) and an OAuth-style bearer token (stage 1, acquired from the
        // relay's /sqltunnel-auth/token endpoint) are appended as query parameters to the
        // Tunnel URL; the fork strips them and forwards them out-of-band to the relay.
        if (!String.IsNullOrWhiteSpace(dbTunnelUrl))
        {
            string tunnelUrl = dbTunnelUrl.Trim();
            int queryIdx = tunnelUrl.IndexOf('?');
            if (queryIdx >= 0)
                tunnelUrl = tunnelUrl.Substring(0, queryIdx);

            string iPlusUser = UserSettings?.UserName;
            string iPlusPass = UserSettings?.Password;
            string token = TryAcquireTunnelToken(tunnelUrl, iPlusUser, iPlusPass);

            var tunnelWithAuth = new System.Text.StringBuilder(tunnelUrl);
            tunnelWithAuth.Append("?iplususer=").Append(Uri.EscapeDataString(iPlusUser ?? ""));
            if (!String.IsNullOrEmpty(iPlusPass))
                tunnelWithAuth.Append("&ipluspass=").Append(Uri.EscapeDataString(iPlusPass));
            if (!String.IsNullOrEmpty(token))
                tunnelWithAuth.Append("&authtoken=").Append(Uri.EscapeDataString(token));
            connectionString += "; Tunnel URL=" + tunnelWithAuth.ToString();
            tunnelUrlWithAuth = tunnelWithAuth.ToString();
            // NOTE: do NOT touch any Database static member here (including
            // Database.TunnelUrl) — that would trigger the Database static constructor
            // before the connection string below is registered in the config section.
        }

        var setting = new ConnectionStringSettings(
                Database.C_DefaultContainerName,
                connectionString,
                "System.Data.SqlClient");

        var config = CommandLineHelper.ConfigCurrentDir;
        if (config == null)
        {
            // Browser/WASM: there is no exe configuration. Create a minimal vbiplus.config
            // in the current (in-memory) directory so that CommandLineHelper.ConfigCurrentDir
            // can open it and the connection string can be registered in-memory.
            try
            {
                string configPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "vbiplus.config");
                if (!System.IO.File.Exists(configPath))
                    System.IO.File.WriteAllText(configPath,
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><connectionStrings /></configuration>");
                config = CommandLineHelper.ConfigCurrentDir;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoginView: could not create vbiplus.config. {ex.GetType().Name}: {ex.Message}");
            }
        }

        var existingConnection = config?.ConnectionStrings?.ConnectionStrings[Database.C_DefaultContainerName];

        if (existingConnection != null)
        {
            existingConnection.ProviderName = setting.ProviderName;
            existingConnection.ConnectionString = setting.ConnectionString;
        }
        else if (config != null)
        {
            config.ConnectionStrings.ConnectionStrings.Add(setting);
        }
        else
        {
            // No editable configuration available (e.g. Browser/WASM without a config file):
            // register the connection string in the global ConfigurationManager collection,
            // which Database.ConnectionString falls back to.
            var globalConnections = ConfigurationManager.ConnectionStrings;
            if (globalConnections != null)
            {
                var existingGlobal = globalConnections[Database.C_DefaultContainerName];
                if (existingGlobal != null)
                {
                    existingGlobal.ProviderName = setting.ProviderName;
                    existingGlobal.ConnectionString = setting.ConnectionString;
                }
                else
                {
                    globalConnections.Add(setting);
                }
            }
        }

        // NOTE: Database.ModifiedConnectionString / Database.ConnectionString now append
        // Database.TunnelUrl (set above), so app-specific contexts (ACClassTaskQueue etc.)
        // get the Tunnel URL even though the global ConfigurationManager collection is
        // read-only on Android and cannot be registered here.

        // Register the tunnel URL statically AFTER the config section entry exists:
        // this is the first Database static touch and triggers the Database static
        // constructor (global context) — the config must already be resolvable.
        if (tunnelUrlWithAuth != null)
            Database.TunnelUrl = tunnelUrlWithAuth;

        // Diagnostic probe: verify the connection string resolves and whether SqlClient
        // can open a connection at all on this platform. Uses the forked Microsoft.Data.SqlClient
        // (System.Data.SqlClient has no WebSocket tunnel support and no raw TCP on WASM).
        try
        {
            Console.WriteLine($"LoginView: resolved ConnectionString: {Database.ConnectionString}");
            var probe = new Microsoft.Data.SqlClient.SqlConnection(Database.ConnectionString);
            probe.Open();
            Console.WriteLine("LoginView: probe connection OPENED successfully.");
            probe.Dispose();
        }
        catch (Exception ex)
        {
            // Full stack traces are essential for diagnosing WASM-only failures
            // (e.g. PlatformNotSupportedException from Monitor/WaitHandle waits).
            Console.WriteLine($"LoginView: probe failed. {ex}");
            var inner = ex.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"  inner: {inner}");
                inner = inner.InnerException;
            }
        }
    }

    #region Methods => SQL Tunnel auth (docs/SqlTunnel-OptionB-Implementation.md §9.21)

    /// <summary>
    /// JS interop: synchronous XMLHttpRequest (window.iPlusSyncXhr bridge in index.html).
    /// Returns JSON {"status": int, "text": string}. Throws PNSE on non-browser platforms —
    /// only call inside <see cref="OperatingSystem.IsBrowser"/>.
    /// </summary>
    [System.Runtime.InteropServices.JavaScript.JSImport("globalThis.iPlusSyncXhr")]
    private static partial string SyncXhr(string method, string url, string body);

    /// <summary>
    /// Stage 1: acquires an OAuth-style bearer token from the relay host's
    /// <c>POST /sqltunnel-auth/token</c> endpoint (v1 local issuer validating iPlus
    /// credentials). Browser/WASM uses a synchronous XHR (the JS event loop cannot be
    /// blocked from .NET); all other platforms (Android/desktop — real threads) use a
    /// synchronous HttpClient call. Returns null when no token could be obtained (relay
    /// without RequireOAuth, offline) — the tunnel then works unauthenticated or fails
    /// server-side with a clear 401.
    /// </summary>
    private static string TryAcquireTunnelToken(string tunnelUrl, string iPlusUser, string iPlusPassword)
    {
        try
        {
            if (String.IsNullOrEmpty(iPlusUser))
                return null;

            // The token endpoint is plain HTTP(S). A ws(s):// Tunnel URL must be normalized,
            // otherwise Android's HttpClient (java.net.URL) throws MalformedURLException
            // "unknown protocol: ws".
            string tokenBaseUrl = tunnelUrl;
            if (tokenBaseUrl.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
                tokenBaseUrl = "http://" + tokenBaseUrl.Substring(5);
            else if (tokenBaseUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
                tokenBaseUrl = "https://" + tokenBaseUrl.Substring(6);
            Uri tokenUri = new Uri(new Uri(tokenBaseUrl), "/sqltunnel-auth/token");
            // Manual JSON (no JsonSerializer.Serialize): reflection-based serialization is
            // disabled in the WASM runtime (JsonSerializerIsReflectionDisabled).
            // JsonEncodedText already includes the surrounding quotes.
            string body = "{\"username\":\"" + System.Text.Json.JsonEncodedText.Encode(iPlusUser ?? "") +
                          "\",\"password\":\"" + System.Text.Json.JsonEncodedText.Encode(iPlusPassword ?? "") + "\"}";

            string text;
            int status;
            if (OperatingSystem.IsBrowser())
            {
                string result = SyncXhr("POST", tokenUri.ToString(), body);
                using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(result);
                status = doc.RootElement.GetProperty("status").GetInt32();
                text = doc.RootElement.GetProperty("text").GetString();
            }
            else
            {
                // Android/desktop: real threads exist, a blocking call is fine here
                // (ApplyConnectionSettingsFromHelper is synchronous anyway).
                using System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                using System.Net.Http.HttpResponseMessage response = httpClient.PostAsync(tokenUri,
                    new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                status = (int)response.StatusCode;
                text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (status != 200)
            {
                Console.WriteLine($"LoginView: tunnel token acquisition failed with HTTP {status} (relay may not require OAuth).");
                return null;
            }
            using System.Text.Json.JsonDocument tokenDoc = System.Text.Json.JsonDocument.Parse(text);
            string token = tokenDoc.RootElement.GetProperty("token").GetString();
            Console.WriteLine("LoginView: tunnel token acquired.");
            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: tunnel token acquisition failed. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    #endregion

    #endregion

    #region Methods => Progress

    void _Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e != null)
        {
            if (!this.listboxInfo.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => _Messages_CollectionChanged(sender, e), DispatcherPriority.Send);
                return;
            }
            AddItems(e.NewItems);
        }
    }

    private void AddItems(IList newItems)
    {
        if (_Messages != null)
        {
            if (newItems != null)
            {
                foreach (Msg msg in newItems)
                {
                    _Messages.Add(msg);
                }
            }

            if (listboxInfo.Items.Count > 0)
            {
                listboxInfo.SelectedIndex = listboxInfo.Items.Count - 1;
                listboxInfo.ScrollIntoView(listboxInfo.SelectedItem);
            }
        }
    }

    #endregion

    #endregion
}