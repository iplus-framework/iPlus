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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (!_LoginFlowInitialized)
        {
            _LoginFlowInitialized = true;
            bool configured = HasDatabaseConnectionConfigured();
            Console.WriteLine($"LoginView: OnLoaded, database configured: {configured}");
            if (configured)
            {
                _ = DoLoginActionAsync();
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

        _LoginResultTcs?.TrySetResult(true);
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
        bool? singleViewEnabled = settings.Where(c => c.ACCaptionTranslation == nameof(SingleViewEnabled)).FirstOrDefault()?.Value as bool?;
        if (singleViewEnabled.HasValue)
            SingleViewEnabled.IsChecked = singleViewEnabled.Value;
        else
            SingleViewEnabled.IsChecked = true;
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

        ACValueItem singleViewVal = CommandLineHelper.Settings.Where(c => c.ACCaptionTranslation == nameof(SingleViewEnabled)).FirstOrDefault();
        if (singleViewVal == null)
        {
            SingleViewEnabled.IsChecked = true;
            singleViewVal = new ACValueItem(nameof(SingleViewEnabled), SingleViewEnabled.IsChecked, null);
            CommandLineHelper.Settings.Add(singleViewVal);
        }
        else
        {
            singleViewVal.Value = SingleViewEnabled.IsChecked;
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

        if (String.IsNullOrWhiteSpace(dbSource) && String.IsNullOrWhiteSpace(dbName))
            return;

        string connSettingsFormat = @"Integrated Security=True; Encrypt=False; data source={0}; initial catalog={1}; Trusted_Connection=False; persist security info=True; user id={2}; password={3}; multipleactiveresultsets=True; application name=iPlus";
        var setting = new ConnectionStringSettings(
                Database.C_DefaultContainerName,
                String.Format(connSettingsFormat, dbSource, dbName, dbUser, dbPass),
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

        // Diagnostic probe: verify the connection string resolves and whether SqlClient
        // can open a connection at all on this platform (Browser/WASM has no raw TCP!).
        try
        {
            Console.WriteLine($"LoginView: resolved ConnectionString: {Database.ConnectionString}");
            var probe = new System.Data.SqlClient.SqlConnection(Database.ConnectionString);
            probe.Open();
            Console.WriteLine("LoginView: probe connection OPENED successfully.");
            probe.Dispose();
        }
        catch (Exception ex)
        {
            var sb = new System.Text.StringBuilder($"LoginView: probe failed. {ex.GetType().Name}: {ex.Message}");
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.Append($"\n  inner: {inner.GetType().Name}: {inner.Message}");
                inner = inner.InnerException;
            }
            Console.WriteLine(sb.ToString());
        }
    }

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