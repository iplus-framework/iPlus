using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using Avalonia.Input;
using System.Collections.Generic;
using MsBox.Avalonia.Enums;
using Avalonia.ReactiveUI;
using ReactiveUI;
using gip.iplus.client.avui.Views;
using Avalonia.Data;

namespace gip.iplus.client.avui;

public partial class LoginWindow : ReactiveWindow<Settings>
{
    protected object _WaitOnOkClick = new object();
    int _CountAttempts = 0;

    private readonly Action _LoginAction;
    private readonly Action _ShowMainWindowAction;

    public LoginWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposable => { });
    }

    public LoginWindow(Action loginAction, Action mainAction) : this()
    {
        listboxInfo.ItemsSource = MsgDetails;
        listboxInfo.DisplayMemberBinding = new Avalonia.Data.Binding("Message");
        selTheme.ItemsSource = System.Enum.GetValues(typeof(eWpfTheme));
        _LoginAction = loginAction;
        _ShowMainWindowAction = mainAction;
    }

    private Settings UserSettings => DataContext as Settings;

    #region Eventhandler

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Monitor.Enter(_WaitOnOkClick);
        DoLoginAction();
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (_CollectionChangedSubscr && InfoMessage.MsgDetails != null)
        {
            _CollectionChangedSubscr = false;
            (InfoMessage.MsgDetails as ObservableCollection<Msg>).CollectionChanged -= _Messages_CollectionChanged;
        }
        selTheme.ItemsSource = null;
        base.OnUnloaded(e);
    }

    private async void DoLoginAction()
    {
        if (_LoginAction is not null)
        {
            await Task.Run(async () =>
            {
                try
                {
                    _LoginAction.Invoke();
                }
                catch (Exception ex)
                {
                    // Handle exceptions from the heavy load action
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                    });
                }
            });
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _ShowMainWindowAction?.Invoke();
            Close();
        });
    }
    #endregion

    #region Progress-Area
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

    void _Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e != null)
        {
            if (!this.listboxInfo.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => _Messages_CollectionChanged(sender, e), DispatcherPriority.Send);
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

    #region Login-Area
    /// <summary>
    /// 1. Call from App
    /// </summary>
    public async void DisplayLogin(bool display, String errorMsg)
    {
        if (!this.ProgressGrid.CheckAccess())
        {
            await Dispatcher.UIThread.InvokeAsync(() => DisplayLogin(display, errorMsg), DispatcherPriority.Send);
            return;
        }

        if (display)
        {
            CheckRestoreSameScreen.IsChecked = !ControlManager.RestoreWindowsOnSameScreen;
            CheckTouchScreen.IsChecked = ControlManager.TouchScreenMode;
            _CountAttempts++;
            if (_CountAttempts <= 1)
            {
                TextboxKey.Text = "";
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
                    userMsg = new Msg() { Message = String.Format("User {0} doesn't exist or wrong password!", this.User), MessageLevel = eMsgLevel.Info };
                }
                if (!String.IsNullOrEmpty(errorMsg))
                    userMsg.Message += " // " + errorMsg;

                Dispatcher.UIThread.Post(() =>
                    AsyncMessageBox.ShowMessageBox(userMsg.Message, "Info", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning), DispatcherPriority.Send);

                UserSettings.Password = "";
            }

            _CtrlPressed = false;
            _F1Pressed = false;

            ProgressGrid.IsVisible = false;
            LoginGrid.IsVisible = true;
        }
        else
        {
            Monitor.Enter(_WaitOnOkClick);
            LoginGrid.IsVisible = false;
            ProgressGrid.IsVisible = true;
        }
    }

    /// <summary>
    /// 2. Call from App and waits on User-OK-click
    /// </summary>
    public void WaitOnLoginResult()
    {
        Monitor.Enter(_WaitOnOkClick);
        Monitor.Exit(_WaitOnOkClick);
    }

    bool _IsF1Pressed = false;
    bool _IsLeftCtrlPressed = false;
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F1)
            _IsF1Pressed = true;
        if (e.Key == Key.LeftCtrl)
            _IsLeftCtrlPressed = true;
        base.OnKeyDown(e);
    }
    override protected void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.F1)
            _IsF1Pressed = false;
        if (e.Key == Key.LeftCtrl)
            _IsLeftCtrlPressed = false;
        base.OnKeyUp(e);    
    }

    /// <summary>
    /// 3. User Clicks OK => Signal on GetLoginResult
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLogin_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_Keyword))
        {
            if (_Keyword == TextboxKey.Text)
                return;
            //if (!License.VerifyRemoteLogin(TextboxKey.Text, _Keyword, Database.GlobalDatabase))
            //    return;
        }
        if (_IsLeftCtrlPressed)
            UserSettings.CtrlPressed = true;

        if (IsLoginWithControlLoad)
            UserSettings.CtrlPressed = true;

        if (_IsF1Pressed)
            UserSettings.F1Pressed = true;

        _WpfTheme = (eWpfTheme)System.Enum.Parse(typeof(eWpfTheme), selTheme.SelectedValue.ToString());
        _User = TextboxUser.Text;
        _Password = TextboxPassword.Text;
        _Keyword = TextboxKey.Text;
        label3.IsVisible = false;
        TextboxKey.IsVisible = false;
        ControlManager.RestoreWindowsOnSameScreen = CheckRestoreSameScreen.IsChecked.HasValue ? !CheckRestoreSameScreen.IsChecked.Value : true;
        ControlManager.TouchScreenMode = CheckTouchScreen.IsChecked.HasValue ? CheckTouchScreen.IsChecked.Value : false;

        Monitor.Exit(_WaitOnOkClick);
    }

    private void task_OnStatusChange(core.dbsyncer.Messages.BaseSyncMessage msg)
    {
        Msg internalMessage = new Msg();
        internalMessage.Message = msg.ToString();
        AddItems(new List<Msg>() { internalMessage });
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();

        // Unload-EreignisHandler mit Ereignis=null aufrufen
        // um Abbruch zu signalisieren
        //this.Login_Unloaded(this, null);

        Monitor.Exit(_WaitOnOkClick);
    }

    private string _Keyword = "";

    private string _User;
    public string User
    {
        get
        {
            return _User;
        }
    }

    private string _Password;
    public string Password
    {
        get
        {
            return _Password;
        }
    }



    private bool _CtrlPressed;
    public bool CtrlPressed
    {
        get
        {
            return _CtrlPressed;
        }
    }

    private bool _F1Pressed;
    public bool F1Pressed
    {
        get
        {
            return _F1Pressed;
        }
    }

    private eWpfTheme _WpfTheme;
    public eWpfTheme WpfTheme
    {
        get
        {
            return _WpfTheme;
        }
    }

    #endregion

    private void ButtonLogin_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right && e.KeyModifiers == KeyModifiers.Control)
        {
            if (labelTouchScreen.IsVisible)
            {
                label3.IsVisible = true;
                TextboxKey.IsVisible = true;
                labelTouchScreen.IsVisible = false;
                CheckTouchScreen.IsVisible = false;
            }
            else
            {
                label3.IsVisible = false;
                TextboxKey.IsVisible = false;
                labelTouchScreen.IsVisible = true;
                CheckTouchScreen.IsVisible = true;
            }
            //_Keyword = License.GenerateRemoteLoginCode();
            TextboxKey.Text = _Keyword;
            e.Handled = true;
            return;
        }
    }

    private bool _IsLoginWithControlLoad = false;
    internal bool IsLoginWithControlLoad
    {
        get
        {
            return _IsLoginWithControlLoad;
        }
        set
        {
            _IsLoginWithControlLoad = value;
        }
    }
}