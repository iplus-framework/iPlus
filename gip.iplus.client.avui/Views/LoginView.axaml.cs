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

public partial class LoginView : UserControl
{
    #region c'tors

    public LoginView()
    {
        InitializeComponent();
        //this.WhenActivated(disposable => { });
    }

    public LoginView(Action loginAction, Action mainAction) : this()
    {
        listboxInfo.ItemsSource = MsgDetails;
        listboxInfo.DisplayMemberBinding = new Avalonia.Data.Binding("Message");
        selTheme.ItemsSource = System.Enum.GetValues(typeof(eWpfTheme));
        _LoginAction = loginAction;
        _ShowMainWindowAction = mainAction;
    }

    #endregion

    #region Properties

    protected object _WaitOnOkClick = new object();
    int _CountAttempts = 0;

    private readonly Action _LoginAction;
    private readonly Action _ShowMainWindowAction;

    private Settings UserSettings => DataContext as Settings;

    #region Properties => Login

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

    private eWpfTheme _WpfTheme;
    public eWpfTheme WpfTheme
    {
        get
        {
            return _WpfTheme;
        }
    }

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
            //Close();
        });
    }

    #region Methods => Login

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
                    userMsg = new Msg() { Message = String.Format("User {0} doesn't exist or wrong password!", this.User), MessageLevel = eMsgLevel.Info };
                }
                if (!String.IsNullOrEmpty(errorMsg))
                    userMsg.Message += " // " + errorMsg;

                Dispatcher.UIThread.Post(() =>
                    AsyncMessageBox.ShowMessageBox(userMsg.Message, "Info", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning), DispatcherPriority.Send);

                UserSettings.Password = "";
            }

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

    /// <summary>
    /// 3. User Clicks OK => Signal on GetLoginResult
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLogin_Click(object sender, RoutedEventArgs e)
    {
        //_WpfTheme = (eWpfTheme)System.Enum.Parse(typeof(eWpfTheme), selTheme.SelectedValue.ToString());
        _User = TextboxUser.Text;
        _Password = TextboxPassword.Text;

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
        //this.Close();

        // Unload-EreignisHandler mit Ereignis=null aufrufen
        // um Abbruch zu signalisieren
        //this.Login_Unloaded(this, null);

        Monitor.Exit(_WaitOnOkClick);
    }

    private void ButtonLogin_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        //if (e.InitialPressMouseButton == MouseButton.Right && e.KeyModifiers == KeyModifiers.Control)
        //{
        //    if (labelTouchScreen.IsVisible)
        //    {
        //        label3.IsVisible = true;
        //        TextboxKey.IsVisible = true;
        //        labelTouchScreen.IsVisible = false;
        //        CheckTouchScreen.IsVisible = false;
        //    }
        //    else
        //    {
        //        label3.IsVisible = false;
        //        TextboxKey.IsVisible = false;
        //        labelTouchScreen.IsVisible = true;
        //        CheckTouchScreen.IsVisible = true;
        //    }
        //    //_Keyword = License.GenerateRemoteLoginCode();
        //    TextboxKey.Text = _Keyword;
        //    e.Handled = true;
        //    return;
        //}
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
    }

    private void Image_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (ProgressGrid.IsVisible)
            return;

        LoadSettings();

        SettingsGrid.IsVisible = !SettingsGrid.IsVisible;
        LoginGrid.IsVisible = !SettingsGrid.IsVisible;
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

        CommandLineHelper.SaveSettings();
    }

    private void ButtonSave_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
    }

    #endregion

    #region Methods => Progress

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

    #endregion
}