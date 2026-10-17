#nullable enable
// CS0162: the AutoLoginBypassEnabled test bypass (const false) makes its call sites
// unreachable. TEMPORARY — remove this pragma together with the bypass sections once
// agent-driven auto-login is no longer needed.
#pragma warning disable CS0162
// VSTHRD002/VSTHRD103: LoginView intentionally contains synchronous wrappers over async
// cores (ApplyConnectionSettingsFromHelper, TryAcquireTunnelToken, Entra device-code
// polling, the connection probe) — they only run on platforms with real threads
// (desktop/Android background threads); Browser/WASM uses the await-based cores.
#pragma warning disable VSTHRD002
#pragma warning disable VSTHRD103
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

    public LoginView(Func<Task> loginAction, Action mainAction, IEnumerable<IExternalLoginProvider>? externalLoginProviders = null) : this()
    {
        listboxInfo.ItemsSource = MsgDetails;
        listboxInfo.DisplayMemberBinding = new Avalonia.Data.Binding("Message");
        selTheme.ItemsSource = System.Enum.GetValues(typeof(eWpfTheme));
        _LoginAction = loginAction;
        _ShowMainWindowAction = mainAction;
        _externalLoginProviders = (externalLoginProviders ?? Enumerable.Empty<IExternalLoginProvider>()).ToList();
        LoginStatusCallback = SetStatusText;
    }

    #region Login progress reporting (status line + listboxInfo)

    /// <summary>
    /// Static hook so static helpers (Entra device-code flow) can update the status
    /// line of the progress card. Set in the constructor.
    /// </summary>
    private static Action<string>? LoginStatusCallback;

    private void SetStatusText(string text)
    {
        if (statusInfo == null)
            return;
        if (Dispatcher.UIThread.CheckAccess())
            statusInfo.Text = text;
        else
            Dispatcher.UIThread.Post(() => statusInfo.Text = text);
    }

    private static void SetLoginStatus(string text)
    {
        LoginStatusCallback?.Invoke(text);
        LogLoginInfo(text);
    }

    /// <summary>
    /// Writes a login progress message into Messages.GlobalMsg — the collection the
    /// listboxInfo of the progress card is bound to, so the user sees what is currently
    /// being loaded (instead of only Console.WriteLine in the browser dev tools).
    /// </summary>
    private static void LogLoginInfo(string message)
    {
        try
        {
            Msg msg = new Msg() { Message = message, MessageLevel = eMsgLevel.Info };
            void Add()
            {
                Messages.GlobalMsg.MsgDetails.Add(msg);
            }
            if (Dispatcher.UIThread.CheckAccess())
                Add();
            else
                Dispatcher.UIThread.Post(Add);
        }
        catch { }
    }

    #endregion

    #region Responsive layout (orientation + screen size)

    /// <summary>Width threshold above which the login card is centered with border/shadow.</summary>
    private const double WideScreenWidth = 700;

    private TopLevel? _responsiveTopLevel = null;
    private IDisposable? _clientSizeSubscription = null;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            _responsiveTopLevel = topLevel;
            _clientSizeSubscription = topLevel.GetObservable(TopLevel.ClientSizeProperty)
                .Subscribe(UpdateResponsiveLayout);
            UpdateResponsiveLayout(topLevel.ClientSize);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _clientSizeSubscription?.Dispose();
        _clientSizeSubscription = null;
        _responsiveTopLevel = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Sets pseudo-classes on the root grid that drive the responsive XAML styles:
    /// landscape/portrait (logo left of the fields vs. above them) and wide/narrow
    /// (centered card with border + shadow vs. full-span card).
    /// </summary>
    private void UpdateResponsiveLayout(Size clientSize)
    {
        if (RootGrid == null)
            return;
        bool landscape = clientSize.Width > clientSize.Height;
        bool wide = clientSize.Width >= WideScreenWidth;
        SetResponsiveClass(RootGrid, "landscape", landscape);
        SetResponsiveClass(RootGrid, "portrait", !landscape);
        SetResponsiveClass(RootGrid, "wide", wide);
        SetResponsiveClass(RootGrid, "narrow", !wide);
    }

    private static void SetResponsiveClass(StyledElement element, string className, bool active)
    {
        if (active)
            element.Classes.Add(className);
        else
            element.Classes.Remove(className);
    }

    #endregion

    #endregion

    #region Properties

    protected object _WaitOnOkClick = new object();
    int _CountAttempts = 0;

    /// <summary>
    /// True while the SettingsGrid is shown because no database connection is configured.
    /// The login action is deferred until the user has saved the connection settings.
    /// </summary>
    private bool _AwaitingSettingsSave;

    private readonly Func<Task>? _LoginAction;
    private readonly Action? _ShowMainWindowAction;
    private IReadOnlyList<IExternalLoginProvider> _externalLoginProviders = Array.Empty<IExternalLoginProvider>();
    private CancellationTokenSource? _externalLoginCts;
    private int _externalLoginStarted = 0;
    private int _externalLoginCompleted = 0;

    private Settings UserSettings => (DataContext as Settings)!;

    public event EventHandler? LoginCancelled;
    public event EventHandler? LoginStarted;

    private TopLevel? _topLevel;
    private IInsetsManager? _insetsManager;
    private IInputPane? _inputPane;

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

    ObservableCollection<Msg>? _Messages = null;
    private bool _CollectionChangedSubscr = false;
    public ObservableCollection<Msg> MsgDetails
    {
        get
        {
            if (_Messages == null)
            {
                if (InfoMessage.MsgDetails is ObservableCollection<Msg> msgDetails)
                {
                    _CollectionChangedSubscr = true;
                    msgDetails.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_Messages_CollectionChanged);
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

    /// <summary>
    /// True while/after the Login click deferred <see cref="ApplyConnectionSettingsFromHelper"/>
    /// to a background thread because the Entra device-code flow blocks for minutes
    /// (Thread.Sleep polling loop) — running it on the UI thread triggers an Android ANR
    /// ("app not responding"). The App.axaml.cs LoginStarted handler checks this flag and
    /// skips its own synchronous apply; the deferred task completes the login-result TCS
    /// after the token has been acquired.
    /// </summary>
    public static bool EntraApplyDeferred { get; private set; }

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
    //private const string BypassTunnelUrl = "ws://192.168.10.4:8751/sqltunnel";
    private const string BypassTunnelUrl = "http://localhost:8751/sqltunnel-sync"; 
    private const string BypassLoginUser = "superuser";              // iPlus login credentials
    private const string BypassLoginPassword = "superuser";          // adjust if different
    // Entra ID stage 1 (device-code flow). When BypassEntraClientId is set, the tunnel
    // token is acquired from Entra instead of the relay's local issuer. Relay must run
    // with SQLTUNNEL_TOKEN_ISSUER=Entra SQLTUNNEL_ENTRA_AUDIENCE=<same client id>.
    private const string BypassEntraClientId = "03a3ca8b-9b97-4c9b-a628-8bc05bd25ee6";
    private const string BypassEntraTenantId = "6305329f-b091-4d11-9c3b-32242f90e767"; // test tenant; production: "organizations"
    private const string BypassEntraScope = null;                    // default: {clientId}/.default
    // true = mobile "hamburger" mode (MainSingleView/DUMainMobile);
    // false = desktop mode (MainView with menu bar/DUMain) — also on browser.
    private const bool BypassSingleViewMode = true;

    private static void SetBypassSetting(string name, object value)
    {
        ACValueItem? item = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == name).FirstOrDefault();
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
        if (!String.IsNullOrWhiteSpace(BypassEntraClientId))
        {
            SetBypassSetting("EntraClientId", BypassEntraClientId);
            SetBypassSetting("EntraTenantId", BypassEntraTenantId);
            if (!String.IsNullOrWhiteSpace(BypassEntraScope))
                SetBypassSetting("EntraScope", BypassEntraScope);
        }
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
                    // Browser/WASM: fire the async core (the sync wrapper would deadlock the
                    // single-threaded runtime on the awaited credential delivery). The
                    // deferred apply in ButtonLogin_Click below re-runs it idempotently.
                    if (OperatingSystem.IsBrowser())
                        _ = ApplyConnectionSettingsCoreAsync();
                    else
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

    private void InputPane_StateChanged(object? sender, InputPaneStateEventArgs e)
    {
        if (_inputPane is not null &&
            _insetsManager is not null &&
            _topLevel is not null)
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

            Control? focusedElement = _topLevel.FocusManager?.GetFocusedElement() as Control;
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

        if (_CollectionChangedSubscr && InfoMessage.MsgDetails is ObservableCollection<Msg> msgDetails)
        {
            _CollectionChangedSubscr = false;
            msgDetails.CollectionChanged -= _Messages_CollectionChanged;
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
            // WASM is single-threaded: Task.Run executes inline on the JS thread, so the
            // login action (ACRoot init + EF loads over the blocking sync-XHR tunnel)
            // FREEZES the UI (animation, progress bar) until the main view appears.
            // Paint the final status BEFORE that happens so the user knows what is going on.
            SetLoginStatus("Loading application — please wait (the display may freeze briefly)...");
            await Task.Delay(150); // let the status + listbox render before the blocking load
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
                Msg? msgLogon = Messages.GlobalMsg.MsgDetails.Where(c => c.Message == "DB-Connection failed!").FirstOrDefault();
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
    public async Task WaitOnLoginResultAsync()
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

        // Show the progress card (with listboxInfo) instead of the login fields — the
        // user must see that loading is underway and what is currently being loaded.
        LoginGrid.IsVisible = false;
        SettingsGrid.IsVisible = false;
        ProgressGrid.IsVisible = true;

        // Entra device-code flow: the token acquisition blocks (RFC 8628 polling loop with
        // Thread.Sleep) and must NOT run on the UI thread — Android kills the app with
        // "not responding" after ~5 s. Browser/WASM: the apply must also be deferred —
        // the runtime is single-threaded, so the sync wrapper (blocking waits inside the
        // async core) would deadlock the JS event loop. Defer the whole apply (token +
        // config registration) and complete the login-result handshake when it finishes.
        // The device code is shown to the user in a MessageBox (see onUserCode below).
        if (!EntraApplyDeferred && (EntraDeviceCodeFlowConfigured() || OperatingSystem.IsBrowser()))
        {
            EntraApplyDeferred = true;
            Console.WriteLine("LoginView: deferring connection apply (Entra device-code flow) to background thread.");
            _ = Task.Run(async () =>
            {
                try
                {
                    // Async core: on Browser/WASM the runtime is single-threaded — blocking
                    // waits (sync wrapper) would deadlock; awaits keep the JS event loop free.
                    await ApplyConnectionSettingsCoreAsync();
                }
                catch (Exception exApply)
                {
                    var msg = exApply.Message;
                    var inner = exApply.InnerException;
                    while (inner != null) { msg += " Inner: " + inner.Message; inner = inner.InnerException; }
                    Console.WriteLine($"LoginView: deferred ApplyConnectionSettingsFromHelper failed. {msg}\n{exApply}");
                }
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (_LoginResultTcs != null)
                        _LoginResultTcs?.TrySetResult(true);
                    else
                        _PendingLoginResult = true;
                });
            });
            return;
        }

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

    /// <summary>
    /// True when the configured settings select the Entra ID device-code flow for stage 1
    /// (EntraClientId present) on a platform that supports it (not Browser/WASM).
    /// </summary>
    private static bool EntraDeviceCodeFlowConfigured()
    {
        // Browser/WASM supported since §9.26: the async device-code flow (await-based)
        // works on the single-threaded WASM runtime; the user code is shown in a MessageBox
        // and the user completes sign-in on a second device.
        string? entraClientId = CommandLineHelper.Settings
            .Where(c => c.ACCaptionTranslation == "EntraClientId").FirstOrDefault()?.Value as string;
        return !String.IsNullOrWhiteSpace(entraClientId);
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
        CancellationTokenSource? cts = _externalLoginCts;
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
        EntraClientId.Text = settings.Where(c => c.ACCaptionTranslation == "EntraClientId").FirstOrDefault()?.Value as string;
        EntraTenantId.Text = settings.Where(c => c.ACCaptionTranslation == "EntraTenantId").FirstOrDefault()?.Value as string;
        bool? singleViewEnabled = settings.Where(c => c.ACCaptionTranslation == nameof(MobileModeEnabled)).FirstOrDefault()?.Value as bool?;
        
        if (singleViewEnabled.HasValue)
            MobileModeEnabled.IsChecked = singleViewEnabled.Value;
        else
            MobileModeEnabled.IsChecked = true;

        bool? acceptUntrusted = settings.Where(c => c.ACCaptionTranslation == nameof(AcceptUntrustedRelayCertificates)).FirstOrDefault()?.Value as bool?;
        AcceptUntrustedRelayCertificates.IsChecked = acceptUntrusted ?? false;
        ApplyUntrustedCertSwitch(AcceptUntrustedRelayCertificates.IsChecked == true);

        // Temporary setting for debugging with agent with playwright. Needs to bee removed after testing:
        if (AutoLoginBypassEnabled)
            MobileModeEnabled.IsChecked = false;
    }

    private void SwitchSettingsView()
    {
        SettingsGrid.IsVisible = !SettingsGrid.IsVisible;
        LoginGrid.IsVisible = !SettingsGrid.IsVisible;
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e)
    {
        if (ProgressGrid.IsVisible)
            return;

        LoadSettings();

        SwitchSettingsView();
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        ACValueItem? dbSourceVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseSource)).FirstOrDefault();
        if (dbSourceVal == null)
        {
            dbSourceVal = new ACValueItem(nameof(DatabaseSource), DatabaseSource.Text, null);
            CommandLineHelper.Settings.Add(dbSourceVal);
        }
        else
        {
            dbSourceVal.Value = DatabaseSource.Text;
        }

        ACValueItem? dbNameVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseName)).FirstOrDefault();
        if (dbNameVal == null)
        {
            dbNameVal = new ACValueItem(nameof(DatabaseName), DatabaseName.Text, null);
            CommandLineHelper.Settings.Add(dbNameVal);
        }
        else
        {
            dbNameVal.Value = DatabaseName.Text;
        }

        ACValueItem? dbUserVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseUser)).FirstOrDefault();
        if (dbUserVal == null)
        {
            dbUserVal = new ACValueItem(nameof(DatabaseUser), DatabaseUser.Text, null);
            CommandLineHelper.Settings.Add(dbUserVal);
        }
        else
        {
            dbUserVal.Value = DatabaseUser.Text;
        }

        ACValueItem? dbPasswordVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabasePassword)).FirstOrDefault();
        if (dbPasswordVal == null)
        {
            dbPasswordVal = new ACValueItem(nameof(DatabasePassword), DatabasePassword.Text, null);
            CommandLineHelper.Settings.Add(dbPasswordVal);
        }
        else
        {
            dbPasswordVal.Value = DatabasePassword.Text;
        }

        ACValueItem? dbTunnelUrlVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseTunnelUrl)).FirstOrDefault();
        if (dbTunnelUrlVal == null)
        {
            dbTunnelUrlVal = new ACValueItem(nameof(DatabaseTunnelUrl), DatabaseTunnelUrl.Text, null);
            CommandLineHelper.Settings.Add(dbTunnelUrlVal);
        }
        else
        {
            dbTunnelUrlVal.Value = DatabaseTunnelUrl.Text;
        }

        ACValueItem? entraClientIdVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == "EntraClientId").FirstOrDefault();
        if (entraClientIdVal == null)
        {
            entraClientIdVal = new ACValueItem("EntraClientId", EntraClientId.Text, null);
            CommandLineHelper.Settings.Add(entraClientIdVal);
        }
        else
        {
            entraClientIdVal.Value = EntraClientId.Text;
        }

        ACValueItem? entraTenantIdVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == "EntraTenantId").FirstOrDefault();
        if (entraTenantIdVal == null)
        {
            entraTenantIdVal = new ACValueItem("EntraTenantId", EntraTenantId.Text, null);
            CommandLineHelper.Settings.Add(entraTenantIdVal);
        }
        else
        {
            entraTenantIdVal.Value = EntraTenantId.Text;
        }

        ACValueItem? singleViewVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(MobileModeEnabled)).FirstOrDefault();
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

        ACValueItem? acceptUntrustedVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(AcceptUntrustedRelayCertificates)).FirstOrDefault();
        if (acceptUntrustedVal == null)
        {
            acceptUntrustedVal = new ACValueItem(nameof(AcceptUntrustedRelayCertificates), AcceptUntrustedRelayCertificates.IsChecked, null);
            CommandLineHelper.Settings.Add(acceptUntrustedVal);
        }
        else
        {
            acceptUntrustedVal.Value = AcceptUntrustedRelayCertificates.IsChecked;
        }
        ApplyUntrustedCertSwitch(AcceptUntrustedRelayCertificates.IsChecked == true);

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
    /// Clears the persisted Entra token cache (access + refresh token). The next login
    /// runs the interactive device-code flow again. Also clears the in-process cache.
    /// Static so the desktop LoginWindow can invoke it too (Ctrl+Click on Cancel).
    /// </summary>
    public static void ResetEntraTokenCache()
    {
        try
        {
            lock (_EntraTokenCacheLock)
            {
                _EntraTokenCache.Clear();
            }
            ACValueItem? item = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == EntraTokenCacheSetting).FirstOrDefault();
            if (item != null)
            {
                CommandLineHelper.Settings.Remove(item);
                SaveSettingsOnUiThread();
            }
            Console.WriteLine("LoginView: Entra token cache reset.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: could not reset Entra token cache. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void ButtonResetEntraToken_Click(object sender, RoutedEventArgs e)
    {
        ResetEntraTokenCache();
    }

    /// <summary>
    /// Injects the connection settings entered in the SettingsGrid (persisted via
    /// CommandLineHelper.Settings) into the app configuration, so that
    /// <see cref="Database.ConnectionString"/> resolves them. Mirrors
    /// gip.iplus.client.avui.Android/MainActivity.LoginView_LoginStarted.
    /// </summary>
    /// <summary>
    /// Synchronous wrapper for <see cref="ApplyConnectionSettingsCoreAsync"/>. Safe on
    /// platforms with real threads (Android/desktop). On Browser/WASM the caller MUST use
    /// the async core (single-threaded runtime — blocking would deadlock); the App
    /// LoginStarted handler already skips this wrapper when the Entra flow defers the
    /// apply (EntraApplyDeferred).
    /// </summary>
    public void ApplyConnectionSettingsFromHelper()
    {
        ApplyConnectionSettingsCoreAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Async core of the connection apply. Await-based so it also runs on Browser/WASM
    /// (single-threaded): the Entra device-code flow and the SQL credential delivery use
    /// awaitable HttpClient calls instead of blocking waits.
    /// </summary>
    public async Task ApplyConnectionSettingsCoreAsync()
    {
        SetLoginStatus("Applying connection settings...");
        ACValueItemList settings = CommandLineHelper.Settings;

        string? dbSource = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseSource)).FirstOrDefault()?.Value as string;
        string? dbName = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseName)).FirstOrDefault()?.Value as string;
        string? dbUser = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseUser)).FirstOrDefault()?.Value as string;
        string? dbPass = settings.Where(c => c.ACCaptionTranslation == nameof(DatabasePassword)).FirstOrDefault()?.Value as string;
        string? dbTunnelUrl = settings.Where(c => c.ACCaptionTranslation == nameof(DatabaseTunnelUrl)).FirstOrDefault()?.Value as string;

        // Desktop fallback (docs §9.24): when no SettingsGrid values exist, take the
        // connection values from the iPlusV5_Entities entry in ConnectionStrings.config /
        // App.config so the tunnel token + SQL credential delivery also apply there.
        if (String.IsNullOrWhiteSpace(dbSource) && String.IsNullOrWhiteSpace(dbName))
        {
            string? cfg = null;
            try
            {
                cfg = ConfigurationManager.ConnectionStrings?[Database.C_DefaultContainerName]?.ConnectionString;
            }
            catch { /* ConfigurationManager unavailable — skip fallback */ }
            if (!String.IsNullOrWhiteSpace(cfg))
            {
                ParseConfigConnectionString(cfg, out dbSource, out dbName, out dbUser, out dbPass, out dbTunnelUrl);
                if (!String.IsNullOrWhiteSpace(dbSource))
                    Console.WriteLine($"LoginView: using connection values from config file (source={dbSource}, tunnel={(String.IsNullOrWhiteSpace(dbTunnelUrl) ? "none" : "configured")}).");
            }
        }

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
        // Packet Size=32767 (TDS maximum): large result sets (e.g. ACClassPropertyList in
        // BSOiPlusStudio) then need far fewer TDS packets. This matters especially for the
        // browser sync-XHR tunnel, where every relay poll is a blocking round trip — fewer,
        // larger packets let the relay fill its 64 KB chunks in one go.
        string connSettingsFormat = (useSqlAuth
            ? @"Integrated Security=False; Encrypt=False; data source={0}; initial catalog={1}; Trusted_Connection=False; persist security info=True; user id={2}; password={3}; application name=iPlus; multipleactiveresultsets=True; Packet Size=32767"
            : @"Integrated Security=True; Encrypt=False; data source={0}; initial catalog={1}; application name=iPlus; multipleactiveresultsets=True; Packet Size=32767");
        string connectionString = String.Format(connSettingsFormat, dbSource, dbName, dbUser, dbPass);
        string? tunnelUrlWithAuth = null;
        string? acquiredToken = null;
        string? resolvedTunnelUrl = null;

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

            string? token = null;
            string? entraClientId = settings.Where(c => c.ACCaptionTranslation == "EntraClientId").FirstOrDefault()?.Value as string;
            // Desktop fallback (docs §9.24): when the settings store has no Entra keys,
            // read them from <appSettings> in App.config — the desktop has no SettingsGrid
            // for them (EntraClientId / EntraTenantId / EntraScope).
            if (String.IsNullOrWhiteSpace(entraClientId))
                entraClientId = GetAppConfigValue("EntraClientId");
            if (!String.IsNullOrWhiteSpace(entraClientId))
            {
                // Stage 1 via Microsoft Entra ID (real IdP): device-code flow — no MSAL
                // dependency, works on Android/desktop/browser (async core). The relay must
                // run with SQLTUNNEL_TOKEN_ISSUER=Entra and matching SQLTUNNEL_ENTRA_AUDIENCE.
                SetLoginStatus("Acquiring Entra ID token (device code sign-in)...");
                string? entraTenant = settings.Where(c => c.ACCaptionTranslation == "EntraTenantId").FirstOrDefault()?.Value as string;
                string? entraScope = settings.Where(c => c.ACCaptionTranslation == "EntraScope").FirstOrDefault()?.Value as string;
                if (String.IsNullOrWhiteSpace(entraTenant))
                    entraTenant = GetAppConfigValue("EntraTenantId");
                if (String.IsNullOrWhiteSpace(entraScope))
                    entraScope = GetAppConfigValue("EntraScope");
                token = await TryAcquireEntraTokenDeviceCodeAsync(
                    String.IsNullOrWhiteSpace(entraTenant) ? "organizations" : entraTenant.Trim(),
                    entraClientId.Trim(),
                    String.IsNullOrWhiteSpace(entraScope) ? entraClientId.Trim() + "/.default" : entraScope.Trim(),
                    code => ShowEntraUserCode(code),
                    relayProxyBase: OperatingSystem.IsBrowser() ? TunnelUrlToHttpBase(tunnelUrl) : null);
            }
            else
            {
                string? iPlusUser = UserSettings?.UserName;
                string? iPlusPass = UserSettings?.Password;
                SetLoginStatus("Signing in to SQL tunnel with iPlus credentials...");
                token = TryAcquireTunnelToken(tunnelUrl, iPlusUser, iPlusPass);
            }

            if (String.IsNullOrEmpty(token))
                SetLoginStatus("Sign-in failed — continuing without a tunnel token.");
            else
                SetLoginStatus("Sign-in OK.");

            // SQL credential delivery (docs §9.24): when the relay is configured with a
            // server-side SQL login, authenticated clients fetch it here and replace the
            // placeholder credentials — no SQL password is stored in the client config.
            if (!String.IsNullOrEmpty(token))
            {
                SetLoginStatus("Fetching SQL credentials from relay...");
                connectionString = await TryApplySqlCredentialsFromRelayAsync(tunnelUrl, token, connectionString);
            }
            acquiredToken = token;
            resolvedTunnelUrl = tunnelUrl;

            var tunnelWithAuth = new System.Text.StringBuilder(tunnelUrl);
            if (!String.IsNullOrWhiteSpace(entraClientId))
            {
                // Entra mode: the JWT is the credential — no iPlus credentials in the URL.
                tunnelWithAuth.Append("?authtoken=").Append(Uri.EscapeDataString(token ?? ""));
            }
            else
            {
                string? iPlusUser = UserSettings?.UserName;
                string? iPlusPass = UserSettings?.Password;
                tunnelWithAuth.Append("?iplususer=").Append(Uri.EscapeDataString(iPlusUser ?? ""));
                if (!String.IsNullOrEmpty(iPlusPass))
                    tunnelWithAuth.Append("&ipluspass=").Append(Uri.EscapeDataString(iPlusPass));
                if (!String.IsNullOrEmpty(token))
                    tunnelWithAuth.Append("&authtoken=").Append(Uri.EscapeDataString(token));
            }
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

        Console.WriteLine($"LoginView: registering connection string — config={(config != null ? $"HasFile={config.HasFile}" : "null")}, existing={(existingConnection != null)}");
        SetLoginStatus("Registering connection string...");

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

        // Multiple EF contexts (e.g. iPlusMESV5_Entities in the MES client): the relay is a
        // blind TCP proxy to ONE SQL server — the initial catalog travels inside the TDS
        // login packet, so ONE tunnel serves ANY number of contexts on that server. Apply
        // the same Tunnel URL + relay-delivered credentials to every additional registered
        // container name (and to every other entry already present in the config file).
        if (tunnelUrlWithAuth != null)
        {
            await ApplyTunnelToAdditionalConnectionsAsync(
                config,
                resolvedTunnelUrl!,
                tunnelUrlWithAuth,
                acquiredToken,
                Database.ConnectionString);
        }

        // Diagnostic probe: verify the connection string resolves and whether SqlClient
        // can open a connection at all on this platform. Uses the forked Microsoft.Data.SqlClient
        // (System.Data.SqlClient has no WebSocket tunnel support and no raw TCP on WASM).
        try
        {
            // SECURITY: never print the connection string — it contains credentials and is
            // visible to anyone opening the browser dev tools. Log only non-sensitive shape.
            Console.WriteLine($"LoginView: resolved ConnectionString (length={Database.ConnectionString?.Length ?? 0}, tunnel={(Database.ConnectionString ?? "").Contains("Tunnel URL") || (Database.ConnectionString ?? "").Contains("tunnel")})");
            var probe = new Microsoft.Data.SqlClient.SqlConnection(Database.ConnectionString);
            SetLoginStatus("Testing database connection...");
            probe.Open();
            SetLoginStatus("Database connection OK.");
            Console.WriteLine("LoginView: probe connection OPENED successfully.");
            probe.Dispose();
        }
        catch (Exception ex)
        {
            SetLoginStatus("Database connection FAILED: " + ex.Message);
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
    /// Desktop/tunnel preparation for config-file-driven clients (docs §9.24): the
    /// LoginWindow (Avalonia desktop) has no connection logic of its own — it goes
    /// straight to Database.ConnectionString from ConnectionStrings.config. This method
    /// runs BEFORE any Database static init and, when the config's iPlusV5_Entities
    /// entry carries a Tunnel URL, acquires the stage-1 token (Entra via &lt;appSettings&gt;),
    /// fetches the delivered SQL credentials and registers the tokenized connection
    /// string. No-op when the settings store drives the connection (LoginView path) or
    /// no tunnel is configured. Must be called from a background thread (device-code
    /// polling blocks).
    /// </summary>
    public static void PrepareTunnelConnectionFromConfig()
    {
        try
        {
            // When the settings store drives the connection, ApplyConnectionSettingsFromHelper
            // (LoginView path) handles everything — avoid double processing.
            var settings = CommandLineHelper.Settings;
            bool hasSettings =
                settings.Any(c => c.ACCaptionTranslation == nameof(DatabaseSource) && !String.IsNullOrWhiteSpace(c.Value as string)) ||
                settings.Any(c => c.ACCaptionTranslation == nameof(DatabaseName) && !String.IsNullOrWhiteSpace(c.Value as string));
            if (hasSettings)
                return;

            ConnectionStringSettings? cfgSetting = null;
            try
            {
                cfgSetting = ConfigurationManager.ConnectionStrings?[Database.C_DefaultContainerName];
            }
            catch { /* ConfigurationManager unavailable — nothing to prepare */ }
            string? cfg = cfgSetting?.ConnectionString;
            if (String.IsNullOrWhiteSpace(cfg))
                return;

            ParseConfigConnectionString(cfg, out string? dbSource, out string? dbName, out string? dbUser, out string? dbPass, out string? dbTunnelUrl);
            if (String.IsNullOrWhiteSpace(dbTunnelUrl))
                return; // no tunnel — plain config connection, nothing to do
            string tunnelUrl = dbTunnelUrl.Trim();
            int queryIdx = tunnelUrl.IndexOf('?');
            if (queryIdx >= 0)
                tunnelUrl = tunnelUrl.Substring(0, queryIdx);

            // Stage 1 token: Entra via <appSettings> (the desktop has no settings store
            // for the Entra keys). Local-issuer mode would need iPlus credentials, which
            // the config-file path does not carry.
            string? entraClientId = GetAppConfigValue("EntraClientId");
            string? token = null;
            if (!String.IsNullOrWhiteSpace(entraClientId))
            {
                string? entraTenant = GetAppConfigValue("EntraTenantId");
                string? entraScope = GetAppConfigValue("EntraScope");
                token = TryAcquireEntraTokenDeviceCode(
                    String.IsNullOrWhiteSpace(entraTenant) ? "organizations" : entraTenant.Trim(),
                    entraClientId.Trim(),
                    String.IsNullOrWhiteSpace(entraScope) ? entraClientId.Trim() + "/.default" : entraScope.Trim(),
                    code => ShowEntraUserCode(code));
            }
            if (String.IsNullOrEmpty(token))
            {
                Console.WriteLine("LoginView: no tunnel token acquired — the relay will reject the connection if RequireOAuth is set.");
                return;
            }

            // SQL credential delivery (§9.24): replace the placeholder credentials.
            TryApplySqlCredentialsFromRelay(tunnelUrl, token, ref cfg);

            // Tokenize the Tunnel URL IN the connection string: the Database static
            // constructor (triggered by the Database.TunnelUrl setter below) opens the
            // global context via ModifiedConnectionString — the tunnel must already be
            // part of the connection string, otherwise SqlClient dials "sqltunnel" via
            // raw TCP (DNS failure). ApplyTunnelUrl skips appending when the keyword is
            // already present, so no duplication occurs.
            cfg = System.Text.RegularExpressions.Regex.Replace(
                cfg, @"(?i)(Tunnel\s+URL\s*=\s*)[^;]*",
                m => m.Groups[1].Value + tunnelUrl + "?authtoken=" + Uri.EscapeDataString(token));

            // Register under the default name so Database.ConnectionString picks it up.
            // The global ConfigurationManager.ConnectionStrings collection is READ-ONLY on
            // desktop (ConfigurationErrorsException "The configuration is read only") —
            // use the editable CommandLineHelper.ConfigCurrentDir (vbiplus.config / exe
            // config), which Database.ConnectionString prefers anyway.
            var config = CommandLineHelper.ConfigCurrentDir;
            if (config == null)
            {
                try
                {
                    string configPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "vbiplus.config");
                    if (!System.IO.File.Exists(configPath))
                        System.IO.File.WriteAllText(configPath,
                            "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><connectionStrings /></configuration>");
                    config = CommandLineHelper.ConfigCurrentDir;
                }
                catch (Exception exConfig)
                {
                    Console.WriteLine($"LoginView: could not create vbiplus.config. {exConfig.GetType().Name}: {exConfig.Message}");
                }
            }

            var setting = new ConnectionStringSettings(
                    Database.C_DefaultContainerName, cfg, "System.Data.SqlClient");
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
                Console.WriteLine("LoginView: no editable configuration available — tunnel connection string could not be registered.");
                return;
            }

            // Set the static TunnelUrl as well (for code paths that build connection strings
            // without the keyword). NOTE: this triggers the Database static constructor —
            // the config entry above must already contain the tokenized Tunnel URL.
            Database.TunnelUrl = tunnelUrl + "?authtoken=" + Uri.EscapeDataString(token);
            Console.WriteLine("LoginView: tunnel connection prepared from config file (token + credentials applied).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: PrepareTunnelConnectionFromConfig failed. {ex.GetType().Name}: {ex.Message}");
        }
    }

    #region Multiple EF contexts (SQL Tunnel applies to all of them)

    /// <summary>
    /// Additional connection container names (app-specific EF contexts, e.g.
    /// <c>iPlusMESV5_Entities</c> in the MES client) that the SQL Tunnel and the
    /// relay-delivered SQL credentials should also be applied to. App clients add their
    /// names here BEFORE the login (e.g. in Program.Main). Additionally, every OTHER
    /// entry already present in the connection-strings config is rewritten automatically
    /// (desktop ConnectionStrings.config), so on the desktop no registration is needed.
    /// The relay is a blind TCP proxy to ONE SQL server — the initial catalog travels
    /// inside the TDS login packet, so one tunnel serves any number of contexts.
    /// Syntax: <c>"Name"</c> or <c>"Name|InitialCatalog"</c>. The catalog override is used
    /// on platforms WITHOUT ConnectionStrings.config (Android/browser), where the
    /// connection string is derived from the default container — without an override it
    /// would inherit the default's initial catalog. Example:
    /// <c>Add("ExampleV5_Entities|ExampleV5")</c> when the context targets the database
    /// <c>ExampleV5</c> instead of the default container's database.
    /// </summary>
    public static readonly System.Collections.Generic.List<string> AdditionalConnectionContainerNames =
        new System.Collections.Generic.List<string>();

    /// <summary>
    /// Applies the Tunnel URL (with auth) and the relay-delivered SQL credentials to all
    /// additional connection strings: every name in <see cref="AdditionalConnectionContainerNames"/>
    /// plus every other entry found in the editable config or the global ConfigurationManager
    /// (excluding the default container, which the main flow already handled).
    /// </summary>
    private static async Task ApplyTunnelToAdditionalConnectionsAsync(
        System.Configuration.Configuration? config,
        string tunnelUrl,
        string tunnelUrlWithAuth,
        string? token,
        string? defaultConnectionString = null)
    {
        try
        {
            // Candidate names: explicitly registered app contexts + everything already in
            // the config files (desktop ConnectionStrings.config lists e.g.
            // iPlusV5_Entities AND iPlusMESV5_Entities).
            var names = new System.Collections.Generic.List<string>();
            var catalogOverrides = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void AddName(string n)
            {
                if (String.IsNullOrWhiteSpace(n))
                    return;
                // "Name|Catalog" syntax: an app may register an additional EF context with
                // an explicit initial catalog for platforms without ConnectionStrings.config
                // (Android/browser). There the connection string is DERIVED from the default
                // container, and without an override it would inherit the default's catalog —
                // wrong when the context targets a different database on the same server.
                string catalog = null;
                int pipeIdx = n.IndexOf('|');
                if (pipeIdx > 0)
                {
                    catalog = n.Substring(pipeIdx + 1).Trim();
                    n = n.Substring(0, pipeIdx).Trim();
                }
                if (!String.IsNullOrWhiteSpace(catalog))
                    catalogOverrides[n] = catalog;
                if (!names.Contains(n))
                    names.Add(n);
            }
            foreach (string n in AdditionalConnectionContainerNames)
                AddName(n);
            try
            {
                var cfgEntries = config?.ConnectionStrings?.ConnectionStrings;
                if (cfgEntries != null)
                    foreach (ConnectionStringSettings s in cfgEntries)
                        AddName(s.Name);
            }
            catch { /* config unavailable */ }
            try
            {
                var globalEntries = ConfigurationManager.ConnectionStrings;
                if (globalEntries != null)
                    foreach (ConnectionStringSettings s in globalEntries)
                        AddName(s.Name);
            }
            catch { /* ConfigurationManager unavailable */ }

            foreach (string name in names)
            {
                if (String.IsNullOrWhiteSpace(name) || name == Database.C_DefaultContainerName)
                    continue;

                string? cs = null;
                try { cs = config?.ConnectionStrings?.ConnectionStrings[name]?.ConnectionString; } catch { }
                if (String.IsNullOrWhiteSpace(cs))
                {
                    try { cs = ConfigurationManager.ConnectionStrings?[name]?.ConnectionString; } catch { }
                }
                if (String.IsNullOrWhiteSpace(cs))
                {
                    // Android/browser: no ConnectionStrings.config exists. Derive the
                    // additional context from the default container's rewritten connection
                    // string — the relay is a blind proxy to ONE server, and the initial
                    // catalog travels inside the TDS login packet, so the same string works.
                    if (!String.IsNullOrWhiteSpace(defaultConnectionString))
                    {
                        cs = defaultConnectionString;
                        // Apply the "Name|Catalog" override when registered: replace the
                        // default container's initial catalog with the context's own database.
                        if (catalogOverrides.TryGetValue(name, out string overrideCatalog)
                            && !String.IsNullOrWhiteSpace(overrideCatalog))
                        {
                            cs = System.Text.RegularExpressions.Regex.Replace(
                                cs, @"(?i)(initial\s+catalog\s*=\s*)[^;]*", m => m.Groups[1].Value + overrideCatalog);
                            Console.WriteLine($"LoginView: additional connection '{name}' derived from default container with catalog override '{overrideCatalog}'.");
                        }
                        else
                        {
                            Console.WriteLine($"LoginView: additional connection '{name}' derived from default container.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"LoginView: additional connection '{name}' not found in any config — skipped (register its catalog in ConnectionStrings.config).");
                        continue;
                    }
                }

                // Strip any pre-existing Tunnel URL segment, then append the fresh one.
                cs = System.Text.RegularExpressions.Regex.Replace(
                    cs, @"(?i);?\s*Tunnel\s+URL\s*=\s*[^;]*", "");
                cs = cs.TrimEnd(';', ' ');

                // Relay-delivered credentials (docs §9.24): rewrite user id/password and
                // flip Integrated Security so the delivered login is actually used.
                if (!String.IsNullOrEmpty(token))
                    cs = await TryApplySqlCredentialsFromRelayAsync(tunnelUrl, token, cs);

                cs += "; Tunnel URL=" + tunnelUrlWithAuth;
                RegisterConnectionString(name, cs, config);
                Console.WriteLine($"LoginView: tunnel applied to additional connection '{name}'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: ApplyTunnelToAdditionalConnectionsAsync failed. {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers a connection string under the given container name: prefers the editable
    /// CommandLineHelper.ConfigCurrentDir (vbiplus.config / exe config), falls back to the
    /// global ConfigurationManager collection (read-only on some platforms — best effort).
    /// </summary>
    private static void RegisterConnectionString(string containerName, string connectionString, System.Configuration.Configuration? config)
    {
        var setting = new ConnectionStringSettings(containerName, connectionString, "System.Data.SqlClient");
        var existing = config?.ConnectionStrings?.ConnectionStrings[containerName];
        if (existing != null)
        {
            existing.ProviderName = setting.ProviderName;
            existing.ConnectionString = setting.ConnectionString;
            return;
        }
        if (config?.ConnectionStrings?.ConnectionStrings != null)
        {
            config.ConnectionStrings.ConnectionStrings.Add(setting);
            return;
        }
        try
        {
            var globalConnections = ConfigurationManager.ConnectionStrings;
            var existingGlobal = globalConnections?[containerName];
            if (existingGlobal != null)
            {
                existingGlobal.ProviderName = setting.ProviderName;
                existingGlobal.ConnectionString = setting.ConnectionString;
            }
            else
            {
                globalConnections?.Add(setting);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: could not register connection '{containerName}' globally. {ex.GetType().Name}: {ex.Message}");
        }
    }

    #endregion

    /// <summary>
    /// Parses a connection string into the values the tunnel flow needs. Unknown keys
    /// are ignored; keys are matched case-insensitively; values are trimmed.
    /// </summary>
    private static void ParseConfigConnectionString(string cfg, out string? dbSource, out string? dbName, out string? dbUser, out string? dbPass, out string? dbTunnelUrl)
    {
        dbSource = dbName = dbUser = dbPass = dbTunnelUrl = null;
        foreach (string part in cfg.Split(';'))
        {
            int eq = part.IndexOf('=');
            if (eq <= 0) continue;
            string key = part.Substring(0, eq).Trim();
            string val = part.Substring(eq + 1).Trim();
            if (key.Equals("data source", StringComparison.OrdinalIgnoreCase)) dbSource = val;
            else if (key.Equals("initial catalog", StringComparison.OrdinalIgnoreCase) || key.Equals("database", StringComparison.OrdinalIgnoreCase)) dbName = val;
            else if (key.Equals("user id", StringComparison.OrdinalIgnoreCase)) dbUser = val;
            else if (key.Equals("password", StringComparison.OrdinalIgnoreCase)) dbPass = val;
            else if (key.Equals("Tunnel URL", StringComparison.OrdinalIgnoreCase)) dbTunnelUrl = val;
        }
    }

    /// <summary>
    /// Normalizes a ws(s):// Tunnel URL into the http(s):// base of the relay host
    /// (for the token/credential/Entra-proxy endpoints).
    /// </summary>
    private static string TunnelUrlToHttpBase(string tunnelUrl)
    {
        string baseUrl = tunnelUrl;
        if (baseUrl.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
            baseUrl = "http://" + baseUrl.Substring(5);
        else if (baseUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
            baseUrl = "https://" + baseUrl.Substring(6);
        int q = baseUrl.IndexOf('?');
        if (q >= 0)
            baseUrl = baseUrl.Substring(0, q);
        // Authority only: the Tunnel URL may carry a path (e.g. /sqltunnel-sync) — the
        // endpoint paths are appended to the host root, not to that path.
        return new Uri(baseUrl).GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    /// <summary>
    /// Reads a value from &lt;appSettings&gt; in App.config (desktop clients). Returns
    /// null when the key is absent or ConfigurationManager is unavailable (WASM).
    /// </summary>
    private static string? GetAppConfigValue(string key)
    {
        try
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// SQL credential delivery (docs §9.24): fetches the server-side SQL login from the
    /// relay's <c>GET /sqltunnel-sqlcred</c> endpoint (bearer-token protected) and
    /// replaces the user id/password in the connection string, so no SQL password is
    /// stored client-side. No-op when the relay has no delivery configured (404) or the
    /// request fails — the connection string keeps its configured credentials then.
    /// Synchronous wrapper — only call from platforms with real threads (desktop);
    /// Browser/WASM must use <see cref="TryApplySqlCredentialsFromRelayAsync"/>.
    /// </summary>
    private static void TryApplySqlCredentialsFromRelay(string tunnelUrl, string token, ref string connectionString)
    {
        connectionString = TryApplySqlCredentialsFromRelayAsync(tunnelUrl, token, connectionString).GetAwaiter().GetResult();
    }

    #region TLS: self-signed relay certificates (docs SqlTunnel-OptionB-Implementation.md §9.28)

    private static bool _UntrustedCertSwitchApplied = false;

    /// <summary>
    /// True when relay certificates that fail chain validation (self-signed) are accepted.
    /// Sources: the AppContext switch (runtimeconfig.json / AppContext.SetSwitch) or the
    /// environment variable IPLUS_ACCEPT_UNTRUSTED_CERT=1. The env var also sets the
    /// AppContext switch so the SqlClient wss:// transport honors it too.
    /// </summary>
    private static bool AcceptUntrustedRelayCertificatesLegacy
    {
        get
        {
            if (!_UntrustedCertSwitchApplied)
            {
                _UntrustedCertSwitchApplied = true;
                if (System.Environment.GetEnvironmentVariable("IPLUS_ACCEPT_UNTRUSTED_CERT") == "1")
                    AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.AcceptUntrustedTunnelCertificates", true);
            }
            if (AppContext.TryGetSwitch("Switch.Microsoft.Data.SqlClient.AcceptUntrustedTunnelCertificates", out bool v) && v)
                return true;
            return System.Environment.GetEnvironmentVariable("IPLUS_ACCEPT_UNTRUSTED_CERT") == "1";
        }
    }

    /// <summary>
    /// Propagates the "accept untrusted relay certificates" choice to the SqlClient
    /// tunnel transport (AppContext switch) and the relay HTTP client. Must run before
    /// the first connection is opened.
    /// </summary>
    private static void ApplyUntrustedCertSwitch(bool enabled)
    {
        AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.AcceptUntrustedTunnelCertificates", enabled);
    }

    private static bool AcceptUntrustedRelayCertificatesEnabled
    {
        get
        {
            ACValueItemList settings = CommandLineHelper.Settings;
            return settings.Where(c => c.ACCaptionTranslation == nameof(AcceptUntrustedRelayCertificates)).FirstOrDefault()?.Value is bool b && b;
        }
    }

    /// <summary>HttpClient for relay calls; accepts untrusted certificates when enabled (not on browser — there the browser owns TLS).</summary>
    private static System.Net.Http.HttpClient CreateRelayHttpClient()
    {
        if ((AcceptUntrustedRelayCertificatesEnabled || AcceptUntrustedRelayCertificatesLegacy) && !OperatingSystem.IsBrowser())
        {
            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            return new System.Net.Http.HttpClient(handler);
        }
        return new System.Net.Http.HttpClient();
    }

    #endregion

    /// <summary>
    /// Await-based credential delivery — safe on Browser/WASM (single-threaded runtime).
    /// Returns the (possibly rewritten) connection string; unchanged when delivery is
    /// unavailable or fails.
    /// </summary>
    private static async Task<string> TryApplySqlCredentialsFromRelayAsync(string tunnelUrl, string token, string connectionString)
    {
        try
        {
            string credBaseUrl = tunnelUrl;
            if (credBaseUrl.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
                credBaseUrl = "http://" + credBaseUrl.Substring(5);
            else if (credBaseUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
                credBaseUrl = "https://" + credBaseUrl.Substring(6);
            Uri credUri = new Uri(new Uri(credBaseUrl), "/sqltunnel-sqlcred");

            using System.Net.Http.HttpClient http = CreateRelayHttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, credUri);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using var resp = await http.SendAsync(req);
            if ((int)resp.StatusCode != 200)
            {
                Console.WriteLine($"LoginView: relay has no SQL credential delivery (HTTP {(int)resp.StatusCode}) — keeping configured credentials.");
                return connectionString;
            }
            string json = await resp.Content.ReadAsStringAsync();
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
            string? user = doc.RootElement.TryGetProperty("user", out var u) && u.ValueKind == System.Text.Json.JsonValueKind.String ? u.GetString() : null;
            string? password = doc.RootElement.TryGetProperty("password", out var p) && p.ValueKind == System.Text.Json.JsonValueKind.String ? p.GetString() : null;
            if (String.IsNullOrEmpty(user) || password == null)
                return connectionString;

            // Rewrite user id/password and force SQL auth (Integrated Security would
            // make SqlClient ignore the injected credentials and attempt SSPI).
            // MatchEvaluator avoids '$' in the credentials being read as substitution tokens.
            // When the keys are absent (empty username/password in settings), append them.
            var userExpr = new System.Text.RegularExpressions.Regex(@"(?i)(user id\s*=\s*)[^;]*");
            var pwdExpr = new System.Text.RegularExpressions.Regex(@"(?i)(password\s*=\s*)[^;]*");
            connectionString = userExpr.IsMatch(connectionString)
                ? userExpr.Replace(connectionString, m => m.Groups[1].Value + user)
                : connectionString + $";User ID={user}";
            connectionString = pwdExpr.IsMatch(connectionString)
                ? pwdExpr.Replace(connectionString, m => m.Groups[1].Value + password)
                : connectionString + $";Password={password}";
            connectionString = System.Text.RegularExpressions.Regex.Replace(
                connectionString, @"(?i)Integrated\s+Security\s*=\s*True", "Integrated Security=False");
            Console.WriteLine($"LoginView: SQL credentials delivered by relay for user '{user}'.");
            return connectionString;
        }
        catch (Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            for (var e = (Exception?)ex; e != null; e = e.InnerException)
                sb.Append(e.GetType().Name).Append(": ").Append(e.Message).Append(" <- ");
            Console.WriteLine($"LoginView: could not fetch SQL credentials from relay. {sb}");
            return connectionString;
        }
    }

    /// <summary>
    /// Stage 1: acquires an OAuth-style bearer token from the relay host's
    /// <c>POST /sqltunnel-auth/token</c> endpoint (v1 local issuer validating iPlus
    /// credentials). Browser/WASM uses a synchronous XHR (the JS event loop cannot be
    /// blocked from .NET); all other platforms (Android/desktop — real threads) use a
    /// synchronous HttpClient call. Returns null when no token could be obtained (relay
    /// without RequireOAuth, offline) — the tunnel then works unauthenticated or fails
    /// server-side with a clear 401.
    /// </summary>
    private static string? TryAcquireTunnelToken(string tunnelUrl, string? iPlusUser, string? iPlusPassword)
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
                text = doc.RootElement.GetProperty("text").GetString()!;
            }
            else
            {
                // Android/desktop: real threads exist, a blocking call is fine here
                // (ApplyConnectionSettingsFromHelper is synchronous anyway).
                using System.Net.Http.HttpClient httpClient = CreateRelayHttpClient();
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
            string token = tokenDoc.RootElement.GetProperty("token").GetString()!;
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

    #region Methods => SQL Tunnel auth: Microsoft Entra ID (stage 1, real IdP)

    /// <summary>
    /// In-process cache for the Entra token: ApplyConnectionSettingsFromHelper runs more
    /// than once per app start (deferred apply + LoginStarted handler) — the device-code
    /// flow must only prompt the user ONCE. Keyed by clientId+tenant so different configs
    /// don't collide. PERSISTENT storage (settings file) is handled separately via
    /// LoadEntraTokenCache/SaveEntraTokenCache — the access token survives app restarts
    /// and is silently refreshed via the stored refresh token (no UI).
    /// </summary>
    private static readonly Dictionary<string, string> _EntraTokenCache = new();
    private static readonly object _EntraTokenCacheLock = new object();

    /// <summary>Settings key holding the persisted Entra token cache (JSON).</summary>
    private const string EntraTokenCacheSetting = "EntraTokenCache";

    private sealed class EntraTokenCacheEntry
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public long ExpiresOnUnix { get; set; }   // access token expiry (UTC seconds)
    }

    /// <summary>
    /// Loads the persisted token cache entry for a tenant|clientId|scope key from the
    /// app settings file (persisted on Android/desktop; on Browser/WASM the device-code
    /// flow is not used anyway). Returns null when absent or unparsable.
    /// </summary>
    private static EntraTokenCacheEntry? LoadEntraTokenCache(string cacheKey)
    {
        try
        {
            string? json = CommandLineHelper.Settings
                .Where(c => c.ACCaptionTranslation == EntraTokenCacheSetting).FirstOrDefault()?.Value as string;
            if (String.IsNullOrEmpty(json))
                return null;
            json = EntraTokenProtector.Unprotect(json);
            if (String.IsNullOrEmpty(json))
                return null;
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(cacheKey, out System.Text.Json.JsonElement el))
                return null;
            return new EntraTokenCacheEntry
            {
                AccessToken = el.TryGetProperty("accessToken", out var a) && a.ValueKind == System.Text.Json.JsonValueKind.String ? a.GetString() : null,
                RefreshToken = el.TryGetProperty("refreshToken", out var r) && r.ValueKind == System.Text.Json.JsonValueKind.String ? r.GetString() : null,
                ExpiresOnUnix = el.TryGetProperty("expiresOn", out var e) && e.TryGetInt64(out long l) ? l : 0,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: could not read persisted Entra token cache. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Persists the token cache entry (access + refresh token + expiry) in the app
    /// settings file. Best-effort: a read-only settings store just means the user
    /// signs in again on the next start.
    /// </summary>
    private static void SaveEntraTokenCache(string cacheKey, string accessToken, string? refreshToken, long expiresOnUnix)
    {
        try
        {
            // Read the existing store so multiple keys can coexist. NOTE: built with raw
            // JsonDocument/string concatenation — reflection-based JsonSerializer is
            // DISABLED in the WASM build (JsonSerializerIsReflectionDisabled).
            string? existing = CommandLineHelper.Settings
                .Where(c => c.ACCaptionTranslation == EntraTokenCacheSetting).FirstOrDefault()?.Value as string;
            var entries = new List<string>();
            if (!String.IsNullOrEmpty(existing))
            {
                string plain = EntraTokenProtector.Unprotect(existing);
                if (!String.IsNullOrEmpty(plain))
                {
                    // A corrupt/legacy entry must not abort the save — start fresh then.
                    try
                    {
                        using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(plain);
                        foreach (System.Text.Json.JsonProperty prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Name == cacheKey)
                                continue;
                            entries.Add(Quote(prop.Name) + ":" + prop.Value.GetRawText());
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        entries.Clear();
                    }
                }
            }
            // JsonEncodedText.Encode escapes for JSON but does NOT add the surrounding
            // quotes — wrap manually.
            string Quote(string? s) => "\"" + System.Text.Json.JsonEncodedText.Encode(s ?? "").ToString() + "\"";
            string newEntry = "{\"accessToken\":" + Quote(accessToken) +
                              ",\"refreshToken\":" + Quote(refreshToken) +
                              ",\"expiresOn\":" + expiresOnUnix.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}";
            entries.Add(Quote(cacheKey) + ":" + newEntry);
            string json = "{" + String.Join(",", entries) + "}";
            json = EntraTokenProtector.Protect(json);
            ACValueItem? item = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == EntraTokenCacheSetting).FirstOrDefault();
            if (item == null)
                CommandLineHelper.Settings.Add(new ACValueItem(EntraTokenCacheSetting, json, null));
            else
                item.Value = json;
            SaveSettingsOnUiThread();
            Console.WriteLine("LoginView: Entra token cache persisted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: could not persist Entra token cache. {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Persists the settings on the UI thread when running in the browser: the
    /// localStorage bridge (<see cref="BrowserSettingsStorage"/>) is a JS interop
    /// ([JSImport]) call, which is only allowed on the browser's main thread — the
    /// token cache is written from the deferred background apply task, where a direct
    /// call would throw and be silently swallowed by SaveSettings' catch-all.
    /// </summary>
    private static void SaveSettingsOnUiThread()
    {
        if (OperatingSystem.IsBrowser())
            Dispatcher.UIThread.Post(() => CommandLineHelper.SaveSettings());
        else
            CommandLineHelper.SaveSettings();
    }

    /// <summary>
    /// Shows the device code to the user IN THE UI (MessageBox) — Console.WriteLine is not
    /// visible to an end user on Android/desktop. Called from the background polling thread.
    /// </summary>
    private static void ShowEntraUserCode(string userCode)
    {
        // Shown in the progress card (status line + listbox) instead of a MessageBox —
        // a fire-and-forget MessageBox cannot be closed programmatically when the
        // sign-in completes and would stay on screen forever.
        string message = "Waiting for Microsoft Entra ID sign-in: open https://microsoft.com/devicelogin (on any device) and enter the code: " + userCode;
        SetLoginStatus(message);
    }

    /// <summary>
    /// Await-based variant of <see cref="TryAcquireEntraTokenDeviceCode"/> — works on ALL
    /// platforms including Browser/WASM (§9.26): no Thread.Sleep polling and no blocking
    /// waits, so the single-threaded WASM runtime stays responsive while the user completes
    /// the device-code sign-in on a second device. Same cache semantics (in-process +
    /// persistent encrypted cache + silent refresh).
    /// </summary>
    private static async Task<string?> TryAcquireEntraTokenDeviceCodeAsync(string tenant, string clientId, string scope, Action<string>? onUserCode = null, string? relayProxyBase = null)
    {
        string cacheKey = tenant + "|" + clientId + "|" + scope;
        lock (_EntraTokenCacheLock)
        {
            if (_EntraTokenCache.TryGetValue(cacheKey, out string? cached) && cached != null)
                return cached;
        }
        try
        {
            string instance = "https://login.microsoftonline.com";
            // Browser/WASM: login.microsoftonline.com sends no CORS headers on the
            // device-code/token endpoints — the browser blocks direct fetches. Route the
            // calls through the relay's /sqltunnel-entra-proxy (docs §9.26);
            // relayProxyBase is the http(s):// base of the relay from the Tunnel URL.
            string entraBaseUrl = relayProxyBase != null
                ? relayProxyBase.TrimEnd('/') + "/sqltunnel-entra-proxy"
                : instance;
            using System.Net.Http.HttpClient http = new System.Net.Http.HttpClient();
            http.Timeout = TimeSpan.FromSeconds(30);

            // 1. Persisted cache: a still-valid access token is reused silently (no UI).
            EntraTokenCacheEntry? persisted = LoadEntraTokenCache(cacheKey);
            if (persisted != null && !String.IsNullOrEmpty(persisted.AccessToken)
                && persisted.ExpiresOnUnix > DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds())
            {
                Console.WriteLine("LoginView: Entra token reused from persistent cache.");
                lock (_EntraTokenCacheLock) { _EntraTokenCache[cacheKey] = persisted.AccessToken; }
                return persisted.AccessToken;
            }

            // 2. Expired access token but refresh token present: silent refresh (no UI).
            if (persisted != null && !String.IsNullOrEmpty(persisted.RefreshToken))
            {
                string? refreshed = await TryRefreshEntraTokenAsync(http, instance, tenant, clientId, scope, persisted.RefreshToken!, cacheKey, relayProxyBase);
                if (refreshed != null)
                    return refreshed;
                Console.WriteLine("LoginView: silent refresh failed — falling back to device-code sign-in.");
            }

            // 3. Interactive device-code flow (RFC 8628), await-based polling.
            using System.Net.Http.HttpResponseMessage deviceCodeResp = await http.PostAsync($"{entraBaseUrl}/{tenant}/oauth2/v2.0/devicecode",
                new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["scope"] = scope,
                }));
            if ((int)deviceCodeResp.StatusCode != 200)
            {
                Console.WriteLine($"LoginView: Entra device-code request failed: HTTP {(int)deviceCodeResp.StatusCode} {await deviceCodeResp.Content.ReadAsStringAsync()}");
                return null;
            }
            using System.Text.Json.JsonDocument dc = System.Text.Json.JsonDocument.Parse(await deviceCodeResp.Content.ReadAsStringAsync());
            string deviceCode = dc.RootElement.GetProperty("device_code").GetString()!;
            string userCode = dc.RootElement.GetProperty("user_code").GetString()!;
            string verificationUri = dc.RootElement.GetProperty("verification_uri").GetString()!;
            int interval = dc.RootElement.TryGetProperty("interval", out System.Text.Json.JsonElement iv) && iv.TryGetInt32(out int ivV) ? ivV : 5;
            int expires_in = dc.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ex) && ex.TryGetInt32(out int exV) ? exV : 900;

            Console.WriteLine("============================================================");
            Console.WriteLine("LoginView: Entra ID sign-in required.");
            Console.WriteLine($"LoginView: Open {verificationUri} and enter code: {userCode}");
            Console.WriteLine("============================================================");
            try
            {
                onUserCode?.Invoke(userCode);
            }
            catch { }
            if (!OperatingSystem.IsBrowser())
            {
                // Best effort: open the browser automatically (desktop/Android). On WASM the
                // app IS the browser — the user completes sign-in on a second device.
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(verificationUri) { UseShellExecute = true });
                }
                catch { }
            }

            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(expires_in);
            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(interval * 1000);
                using System.Net.Http.HttpResponseMessage poll = await http.PostAsync($"{entraBaseUrl}/{tenant}/oauth2/v2.0/token",
                    new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["client_id"] = clientId,
                        ["device_code"] = deviceCode,
                    }));
                string body = await poll.Content.ReadAsStringAsync();
                if ((int)poll.StatusCode == 200)
                {
                    using System.Text.Json.JsonDocument tok = System.Text.Json.JsonDocument.Parse(body);
                    string accessToken = tok.RootElement.GetProperty("access_token").GetString()!;
                    string? refreshToken = tok.RootElement.TryGetProperty("refresh_token", out System.Text.Json.JsonElement rt) && rt.ValueKind == System.Text.Json.JsonValueKind.String ? rt.GetString() : null;
                    long expiresOn = DateTimeOffset.UtcNow.AddSeconds(
                        tok.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ei) && ei.TryGetInt32(out int eiV) ? eiV : 3600).ToUnixTimeSeconds();
                    Console.WriteLine("LoginView: Entra token acquired.");
                    lock (_EntraTokenCacheLock)
                    {
                        _EntraTokenCache[cacheKey] = accessToken;
                    }
                    SaveEntraTokenCache(cacheKey, accessToken, refreshToken, expiresOn);
                    return accessToken;
                }
                using System.Text.Json.JsonDocument err = System.Text.Json.JsonDocument.Parse(body);
                string? error = err.RootElement.TryGetProperty("error", out System.Text.Json.JsonElement er) && er.ValueKind == System.Text.Json.JsonValueKind.String ? er.GetString() : null;
                if (error == "authorization_pending" || error == "slow_down")
                    continue;
                Console.WriteLine($"LoginView: Entra token polling failed: {error}");
                return null;
            }
            Console.WriteLine("LoginView: Entra device-code flow timed out.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: Entra token acquisition failed. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Await-based silent token refresh via the OAuth 2.0 refresh_token grant (no user
    /// interaction). Updates the persistent cache on success; returns null on failure
    /// (caller falls back to the interactive device-code flow).
    /// </summary>
    private static async Task<string?> TryRefreshEntraTokenAsync(System.Net.Http.HttpClient http, string instance, string tenant,
        string clientId, string scope, string refreshToken, string cacheKey, string? relayProxyBase = null)
    {
        try
        {
            string entraBaseUrl = relayProxyBase != null
                ? relayProxyBase.TrimEnd('/') + "/sqltunnel-entra-proxy"
                : instance;
            using System.Net.Http.HttpResponseMessage resp = await http.PostAsync($"{entraBaseUrl}/{tenant}/oauth2/v2.0/token",
                new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = clientId,
                    ["refresh_token"] = refreshToken,
                    ["scope"] = scope,
                }));
            string body = await resp.Content.ReadAsStringAsync();
            if ((int)resp.StatusCode != 200)
            {
                Console.WriteLine($"LoginView: Entra refresh failed: HTTP {(int)resp.StatusCode}");
                return null;
            }
            using System.Text.Json.JsonDocument tok = System.Text.Json.JsonDocument.Parse(body);
            string accessToken = tok.RootElement.GetProperty("access_token").GetString()!;
            // Entra may rotate the refresh token — store the new one when present.
            string? newRefresh = tok.RootElement.TryGetProperty("refresh_token", out System.Text.Json.JsonElement rt) && rt.ValueKind == System.Text.Json.JsonValueKind.String ? rt.GetString() : refreshToken;
            long expiresOn = DateTimeOffset.UtcNow.AddSeconds(
                tok.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ei) && ei.TryGetInt32(out int eiV) ? eiV : 3600).ToUnixTimeSeconds();
            Console.WriteLine("LoginView: Entra token silently refreshed.");
            lock (_EntraTokenCacheLock)
            {
                _EntraTokenCache[cacheKey] = accessToken;
            }
            SaveEntraTokenCache(cacheKey, accessToken, newRefresh, expiresOn);
            return accessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: Entra refresh failed. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Stage 1 via Microsoft Entra ID using the OAuth 2.0 device authorization grant
    /// (RFC 8628). No MSAL dependency; works on Android and desktop (real threads).
    /// The user opens verification_uri and enters user_code; we poll the token endpoint
    /// until authorization completes. The resulting JWT is passed to the relay as the
    /// stage-1 bearer token (relay validates signature/issuer/audience against the
    /// tenant's JWKS). Not supported on Browser/WASM (no background polling thread).
    /// MUST be called from a background thread — the polling loop blocks.
    /// </summary>
    private static string? TryAcquireEntraTokenDeviceCode(string tenant, string clientId, string scope, Action<string>? onUserCode = null)
    {
        string cacheKey = tenant + "|" + clientId + "|" + scope;
        lock (_EntraTokenCacheLock)
        {
            if (_EntraTokenCache.TryGetValue(cacheKey, out string? cached) && cached != null)
                return cached;
        }
        try
        {
            if (OperatingSystem.IsBrowser())
            {
                Console.WriteLine("LoginView: Entra device-code flow is not supported on Browser/WASM.");
                return null;
            }
            string instance = "https://login.microsoftonline.com";
            using System.Net.Http.HttpClient http = new System.Net.Http.HttpClient();
            http.Timeout = TimeSpan.FromSeconds(30);

            // 1. Persisted cache: a still-valid access token is reused silently (no UI).
            EntraTokenCacheEntry? persisted = LoadEntraTokenCache(cacheKey);
            if (persisted != null && !String.IsNullOrEmpty(persisted.AccessToken)
                && persisted.ExpiresOnUnix > DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds())
            {
                Console.WriteLine("LoginView: Entra token reused from persistent cache.");
                lock (_EntraTokenCacheLock) { _EntraTokenCache[cacheKey] = persisted.AccessToken; }
                return persisted.AccessToken;
            }

            // 2. Expired access token but refresh token present: silent refresh (no UI).
            if (persisted != null && !String.IsNullOrEmpty(persisted.RefreshToken))
            {
                string? refreshed = TryRefreshEntraToken(http, instance, tenant, clientId, scope, persisted.RefreshToken!, cacheKey);
                if (refreshed != null)
                    return refreshed;
                Console.WriteLine("LoginView: silent refresh failed — falling back to device-code sign-in.");
            }

            // 3. Interactive device-code flow.
            var deviceCodeResp = http.PostAsync($"{instance}/{tenant}/oauth2/v2.0/devicecode",
                new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["scope"] = scope,
                })).GetAwaiter().GetResult();
            if ((int)deviceCodeResp.StatusCode != 200)
            {
                Console.WriteLine($"LoginView: Entra device-code request failed: HTTP {(int)deviceCodeResp.StatusCode} {deviceCodeResp.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
                return null;
            }
            using System.Text.Json.JsonDocument dc = System.Text.Json.JsonDocument.Parse(deviceCodeResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            string deviceCode = dc.RootElement.GetProperty("device_code").GetString()!;
            string userCode = dc.RootElement.GetProperty("user_code").GetString()!;
            string verificationUri = dc.RootElement.GetProperty("verification_uri").GetString()!;
            int interval = dc.RootElement.TryGetProperty("interval", out System.Text.Json.JsonElement iv) && iv.TryGetInt32(out int ivV) ? ivV : 5;
            int expires_in = dc.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ex) && ex.TryGetInt32(out int exV) ? exV : 900;

            Console.WriteLine("============================================================");
            Console.WriteLine("LoginView: Entra ID sign-in required.");
            Console.WriteLine($"LoginView: Open {verificationUri} and enter code: {userCode}");
            Console.WriteLine("============================================================");
            try
            {
                // Show the code in the app UI (MessageBox) — the console is invisible to users.
                onUserCode?.Invoke(userCode);
            }
            catch { }
            try
            {
                // Best effort: open the browser automatically (desktop/Android).
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(verificationUri) { UseShellExecute = true });
            }
            catch { }

            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(expires_in);
            while (DateTime.UtcNow < deadline)
            {
                Thread.Sleep(interval * 1000);
                using System.Net.Http.HttpResponseMessage poll = http.PostAsync($"{instance}/{tenant}/oauth2/v2.0/token",
                    new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["client_id"] = clientId,
                        ["device_code"] = deviceCode,
                    })).GetAwaiter().GetResult();
                string body = poll.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if ((int)poll.StatusCode == 200)
                {
                    using System.Text.Json.JsonDocument tok = System.Text.Json.JsonDocument.Parse(body);
                    string accessToken = tok.RootElement.GetProperty("access_token").GetString()!;
                    string? refreshToken = tok.RootElement.TryGetProperty("refresh_token", out System.Text.Json.JsonElement rt) && rt.ValueKind == System.Text.Json.JsonValueKind.String ? rt.GetString() : null;
                    long expiresOn = DateTimeOffset.UtcNow.AddSeconds(
                        tok.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ei) && ei.TryGetInt32(out int eiV) ? eiV : 3600).ToUnixTimeSeconds();
                    Console.WriteLine("LoginView: Entra token acquired.");
                    lock (_EntraTokenCacheLock)
                    {
                        _EntraTokenCache[cacheKey] = accessToken;
                    }
                    SaveEntraTokenCache(cacheKey, accessToken, refreshToken, expiresOn);
                    return accessToken;
                }
                using System.Text.Json.JsonDocument err = System.Text.Json.JsonDocument.Parse(body);
                string? error = err.RootElement.TryGetProperty("error", out System.Text.Json.JsonElement er) && er.ValueKind == System.Text.Json.JsonValueKind.String ? er.GetString() : null;
                if (error == "authorization_pending" || error == "slow_down")
                    continue;
                Console.WriteLine($"LoginView: Entra token polling failed: {error}");
                return null;
            }
            Console.WriteLine("LoginView: Entra device-code flow timed out.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: Entra token acquisition failed. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Silent token refresh via the OAuth 2.0 refresh_token grant (no user interaction).
    /// Updates the persistent cache on success; returns null on failure (caller falls
    /// back to the interactive device-code flow).
    /// </summary>
    private static string? TryRefreshEntraToken(System.Net.Http.HttpClient http, string instance, string tenant,
        string clientId, string scope, string refreshToken, string cacheKey)
    {
        try
        {
            using System.Net.Http.HttpResponseMessage resp = http.PostAsync($"{instance}/{tenant}/oauth2/v2.0/token",
                new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = clientId,
                    ["refresh_token"] = refreshToken,
                    ["scope"] = scope,
                })).GetAwaiter().GetResult();
            string body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if ((int)resp.StatusCode != 200)
            {
                Console.WriteLine($"LoginView: Entra refresh failed: HTTP {(int)resp.StatusCode}");
                return null;
            }
            using System.Text.Json.JsonDocument tok = System.Text.Json.JsonDocument.Parse(body);
            string accessToken = tok.RootElement.GetProperty("access_token").GetString()!;
            // Entra may rotate the refresh token — store the new one when present.
            string? newRefresh = tok.RootElement.TryGetProperty("refresh_token", out System.Text.Json.JsonElement rt) && rt.ValueKind == System.Text.Json.JsonValueKind.String ? rt.GetString() : refreshToken;
            long expiresOn = DateTimeOffset.UtcNow.AddSeconds(
                tok.RootElement.TryGetProperty("expires_in", out System.Text.Json.JsonElement ei) && ei.TryGetInt32(out int eiV) ? eiV : 3600).ToUnixTimeSeconds();
            Console.WriteLine("LoginView: Entra token silently refreshed.");
            lock (_EntraTokenCacheLock)
            {
                _EntraTokenCache[cacheKey] = accessToken;
            }
            SaveEntraTokenCache(cacheKey, accessToken, newRefresh, expiresOn);
            return accessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LoginView: Entra refresh failed. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    #endregion

    #endregion

    #region Methods => Progress

    void _Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

    private void AddItems(IList? newItems)
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
                listboxInfo.ScrollIntoView(listboxInfo.SelectedItem!);
            }
        }
    }

    #endregion

    #endregion
}