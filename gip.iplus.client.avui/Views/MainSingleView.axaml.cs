using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.wpfservices.avui;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.iplus.client.avui;

public partial class MainSingleView : UserControl, IRootPageWPF, IFocusChangeListener
{
    public MainSingleView()
    {
        InitializeComponent();
        InitConnectionInfo();
    }

    private TopLevel _topLevel;
    private IInsetsManager _insetsManager;
    private IInputPane _inputPane;

    #region eventhandling
    ACMenuItem mainMenu;
    protected override void OnLoaded(RoutedEventArgs e)
    {
        string[] cmLineArg = System.Environment.GetCommandLineArgs();
        ACRoot.SRoot.RootPageWPF = this;

        ACClassDesign menuDesign = null;
        using (ACMonitor.Lock(ACRoot.SRoot.Database.QueryLock_1X000))
        {
            menuDesign = ACRoot.SRoot.Environment.User.MenuACClassDesign;
        }

        if (menuDesign == null)
        {
            ACClassDesign acClassDesign = ACRoot.SRoot.GetDesign(Global.ACKinds.DSDesignMenu);
            mainMenu = acClassDesign.GetMenuEntryWithCheck(ACRoot.SRoot);
        }
        else
        {

            using (ACMonitor.Lock(ACRoot.SRoot.Database.ContextIPlus.QueryLock_1X000))
            {
                mainMenu = ACRoot.SRoot.Environment.User.MenuACClassDesign.GetMenuEntryWithCheck(ACRoot.SRoot);
            }
        }

        if (mainMenu != null)
        {
            VBSideMenuItem rootItem = new VBSideMenuItem();
            CreateMenu(mainMenu.Items, rootItem.Items);

            MainMenu.Items.Add(rootItem);
        }
            

        this.DataContext = ACRoot.SRoot.Businessobjects;

        this.Content = MainScrollViewer;

        InitAlarms();
        base.OnLoaded(e);

        // Get TopLevel after the view is loaded (not in constructor!)
        _topLevel = TopLevel.GetTopLevel(this);

        if (_topLevel is not null)
        {
            _insetsManager = _topLevel.InsetsManager;
            _inputPane = _topLevel.InputPane;

            // Subscribe to keyboard state changes
            if (_inputPane is not null)
            {
                _inputPane.StateChanged += InputPane_StateChanged;
            }
        }
    }

    private void InputPane_StateChanged(object? sender, InputPaneStateEventArgs e)
    {
        if (_inputPane is not null &&
            _insetsManager is not null)
        {
            var safeArea = _insetsManager.SafeAreaPadding;
            var occludedArea = _inputPane.OccludedRect;

            // Combine safe area with keyboard height
            this.Padding = new Thickness(
                safeArea.Left,
                safeArea.Top,
                safeArea.Right,
                occludedArea.Height + safeArea.Bottom
            );

            Control focusedElement = _topLevel.FocusManager?.GetFocusedElement() as Control;
            if (focusedElement != null)
            {
                try
                {
                    PixelPoint position = focusedElement.PointToScreen(new Point(0, focusedElement.Bounds.Height));
                    MainScrollViewer.Offset = new Vector(0, position.Y - (_topLevel.Bounds.Height - occludedArea.Height));
                }
                catch (Exception)
                {

                }
            }
        }
    }

    public void CloseWindowFromThread()
    {
        //MainWindow mainWindow = Parent as MainWindow;
        //if (mainWindow != null)
        //    Dispatcher.UIThread.Post(() => mainWindow.Close());
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (_inputPane != null)
        {
            _inputPane.StateChanged -= InputPane_StateChanged;
            _inputPane = null;
        }

        _insetsManager = null;
        _topLevel = null;

        base.OnUnloaded(e);
    }

    #endregion

    #region Menü
    private void CreateMenu(ACMenuItemList mainMenu, ItemCollection items)
    {
        foreach (ACMenuItem acMenuItem in mainMenu)
        {
            if (acMenuItem.ACCaption == "-")
            {
                VBMenuSeparator _Seperator = new VBMenuSeparator();
                items.Add(_Seperator);
                continue;
            }

            VBSideMenuItem menuItem = new VBSideMenuItem(ContextACObject, acMenuItem);
            items.Add(menuItem);
            if (acMenuItem.Items != null && acMenuItem.Items.Count > 0)
            {
                CreateMenu(acMenuItem.Items, menuItem.Items);
            }
        }
    }
    #endregion


    #region Layout
    //VBDesign _RootVBDesign = null;

    private void InitAlarms()
    {
        foreach (ACComponent childComp in ACRoot.SRoot.ACComponentChilds)
        {
            if (childComp is ApplicationManagerProxy || childComp is ACComponentManager)
            {
                IACPropertyNetBase alarmProperty = childComp.GetPropertyNet("HasAlarms") as IACPropertyNetBase;
                if (alarmProperty != null)
                    alarmProperty.PropertyChanged += alarmProperty_PropertyChanged;
                alarmProperty = childComp.GetPropertyNet("AlarmsAsText") as IACPropertyNetBase;
                if (alarmProperty != null)
                    alarmProperty.PropertyChanged += alarmProperty_PropertyChanged;

            }
        }
        RefreshWarningIcon();
    }

    void alarmProperty_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        RefreshWarningIcon();
    }

    void RefreshWarningIcon()
    {
        if (!this.WarningIcon.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(new Action(RefreshWarningIcon), DispatcherPriority.Send);
            return;
        }

        bool hasAlarms = false;
        foreach (ACComponent childComp in ACRoot.SRoot.ACComponentChilds)
        {
            if (childComp is ApplicationManagerProxy || childComp is ACComponentManager)
            {
                IACPropertyNetBase alarmProperty = childComp.GetPropertyNet("HasAlarms") as IACPropertyNetBase;
                IACPropertyNetBase alarmText = childComp.GetPropertyNet("AlarmsAsText") as IACPropertyNetBase;
                if (alarmProperty != null && alarmText != null)
                {
                    if ((bool)alarmProperty.Value)
                    {
                        hasAlarms = true;
                        break;
                    }
                }
            }
        }
        if (hasAlarms)
        {
            WarningIcon.IsVisible = true;
        }
        else
        {
            WarningIcon.IsVisible = false;
        }
    }


    IACComponent _CurrentACComponent = null;
    public IACComponent CurrentACComponent
    {
        get
        {
            if (_CurrentACComponent == null)
                return ACRoot.SRoot;
            return _CurrentACComponent;
        }
        set
        {
            _CurrentACComponent = value;
        }

    }

    public object WPFApplication
    {
        get
        {
            return App._GlobalApp;
        }
    }

    public object DispatcherInvoke(Action action)
    {
        Dispatcher.UIThread.Invoke(action, DispatcherPriority.Normal);
        return null;
    }

    public object DispatcherInvokeRemoteCmd(Action action, string acUrl, IACInteractiveObject obj = null, bool isMethodInvoc = true)
    {
        RemoteCommandManager.Instance.AddNewRemoteCommand(obj, acUrl, isMethodInvoc);
        return DispatcherInvoke(action);
    }

    public void StartBusinessobjectByACCommand(ACCommand acCommand)
    {
        if (MainScrollViewer == null)
            return;

        if (MainSplitView.IsPaneOpen)
            MainSplitView.IsPaneOpen = false;

        bool ribbonVisibilityOff = false;
        string caption = "";
        ACMenuItem menuItem = acCommand as ACMenuItem;
        if (menuItem != null)
        {
            ribbonVisibilityOff = menuItem.RibbonOff;
            caption = menuItem.UseACCaption ? menuItem.ACCaption : "";
        }

        StartBusinessobject(acCommand.GetACUrl(), acCommand.ParameterList, caption);
    }

    public List<Control> _VBDesignList = new List<Control>();
    [Content]
    public List<Control> VBDesignList
    {
        get
        {
            return _VBDesignList;
        }
        set
        {
            _VBDesignList = value;
        }
    }

    public void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption = "")
    {
        if (ContextACObject != null)
        {
            if (acUrl.IndexOf('#') != -1)
            {
                string checkACUrl = acUrl.Replace("#", "\\?");
                var x = ContextACObject.ACUrlCommand(checkACUrl);
                if (x != null)
                    return;
            }
            VBDesign vbDesign = new VBDesign();
            vbDesign.Name = String.Format("BSO{0}", VBDesignList.Count);
            if (!string.IsNullOrEmpty(acCaption))
            {
                vbDesign.ACCaption = acCaption;
                vbDesign.CustomizedACCaption = acCaption;
            }

            vbDesign.AutoStartACComponent = acUrl;
            vbDesign.AutoStartParameter = parameterList;


            VBDesignList.Add(vbDesign);
            ShowVBDesign(vbDesign);
        }
    }

    private void ShowVBDesign(Control uiElement, string acCaption = "")
    {
        if (uiElement == null)
            return;
        IVBContent uiElementAsDataContent = null;
        if (uiElement is IVBContent)
        {
            uiElementAsDataContent = (uiElement as IVBContent);
            if (uiElementAsDataContent.ContextACObject == null)
            {
                if (uiElement is Control)
                {
                    if ((uiElement as Control).DataContext == null)
                        (uiElement as Control).DataContext = this.DataContext;
                }
            }
            if (uiElementAsDataContent.ContextACObject == null)
                return;
        }

        if (uiElement is VBDesign)
        {
            VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
            uiElementAsDataDesign.BSOACComponent = uiElement.DataContext as Businessobjects;

            // Rechteprüfung ob Design geöffnet werden darf
            if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
            {
                if (uiElementAsDataContent.ContextACObject is IACComponent)
                {
                    if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                        return;
                }
            }
        }

        MainContentControl.AddMainDesign(uiElement as VBDesign);
    }

    #endregion

    private void InitConnectionInfo()
    {
        Communications wcfManager = ACRoot.SRoot.GetChildComponent("Communications") as Communications;
        if (wcfManager == null)
            return;
        if (wcfManager.WCFClientManager != null)
        {
            Binding bindingClientIcon = new Binding();
            bindingClientIcon.Source = wcfManager.WCFClientManager;
            bindingClientIcon.Path = nameof(WCFClientManager.ConnectionQuality);
            ClientConnIcon.Bind(VBConnectionState.ConnectionQualityProperty, bindingClientIcon);

            Binding bindingClientText = new Binding();
            bindingClientText.Source = wcfManager.WCFClientManager;
            bindingClientText.Path = nameof(WCFClientManager.ConnectionShortInfo);
            ClientConnText.Bind(VBTextBlock.TextProperty, bindingClientText);
        }
    }


    //private void ClientConnIcon_DoubleTapped(object sender, TappedEventArgs e)
    //{
    //    if (e.KeyModifiers == KeyModifiers.Control)
    //    {
    //        if (!ACRoot.SRoot.Environment.User.IsSuperuser)
    //            return;
    //        WCFClientManager channelManager = ACRoot.SRoot.ACUrlCommand("?\\Communications\\WCFClientManager") as WCFClientManager;
    //        if (channelManager != null)
    //        {
    //            channelManager.BroadcastShutdownAllClients();
    //        }
    //    }
    //    else
    //    {
    //        //if (DockingManager == null)
    //        //    return;
    //        ACComponent channelManager = (ACComponent)ACRoot.SRoot.ACUrlCommand("?\\Communications\\WCFClientManager");
    //        //if (channelManager != null)
    //        //    DockingManager.ShowDialog(channelManager, "ConnectionInfo", "", false);
    //    }
    //}


    #region IRootPageWPF
    delegate Global.MsgResult ShowMsgBoxDelegate(Msg msg, eMsgButton msgButton);
    public async Task<Global.MsgResult> ShowMsgBoxAsync(Msg msg, Global.MsgResult defaultResult, eMsgButton msgButton)
    {
        // Workaround: Wenn MessageBox in OnApplyTemplate aufgerufen wird, dann findet eine Exception statt weil die Nachrichtenverarbeitungsschleife des Dispatchers noch deaktiviert ist
        // Das findet man über den Zugriff auf eine interne Member heraus:
        //System.Reflection.MemberInfo[] infos = typeof(Dispatcher).GetMember("_disableProcessingCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //Type typeDispatcher = typeof(Dispatcher);
        //FieldInfo fieldInfo = typeDispatcher.GetField("_disableProcessingCount", BindingFlags.NonPublic | BindingFlags.Instance);
        //int _disableProcessingCount = 0;
        //if (fieldInfo != null)
        //{
        //    _disableProcessingCount = (int)fieldInfo.GetValue(this.Dispatcher);
        //}
        //if (Dispatcher.UIThread.CheckAccess())
        //{
        //    try
        //    {
        //        return ShowMsgBoxInternAsync(msg, msgButton);
        //    }
        //    catch (InvalidOperationException /*iopEx*/)
        //    {
        //        DispatcherOperation<Global.MsgResult> op = Dispatcher.UIThread.InvokeAsync<Global.MsgResult>(() => ShowMsgBoxInternAsync(msg, msgButton), DispatcherPriority.Normal);
        //        op.Wait();
        //        return (Global.MsgResult)op.Result;
        //    }
        //}
        //else
        {
            return await ShowMsgBoxInternAsync(msg, defaultResult, msgButton);
        }
    }

    public Global.MsgResult ShowMsgBox(Msg msg, Global.MsgResult defaultResult, eMsgButton msgButton)
    {
        // Workaround: Wenn MessageBox in OnApplyTemplate aufgerufen wird, dann findet eine Exception statt weil die Nachrichtenverarbeitungsschleife des Dispatchers noch deaktiviert ist
        // Das findet man über den Zugriff auf eine interne Member heraus:
        //System.Reflection.MemberInfo[] infos = typeof(Dispatcher).GetMember("_disableProcessingCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //Type typeDispatcher = typeof(Dispatcher);
        //FieldInfo fieldInfo = typeDispatcher.GetField("_disableProcessingCount", BindingFlags.NonPublic | BindingFlags.Instance);
        //int _disableProcessingCount = 0;
        //if (fieldInfo != null)
        //{
        //    _disableProcessingCount = (int)fieldInfo.GetValue(this.Dispatcher);
        //}
        //if (Dispatcher.UIThread.CheckAccess())
        //{
        //    try
        //    {
        //        return ShowMsgBoxInternAsync(msg, msgButton);
        //    }
        //    catch (InvalidOperationException /*iopEx*/)
        //    {
        //        DispatcherOperation<Global.MsgResult> op = Dispatcher.UIThread.InvokeAsync<Global.MsgResult>(() => ShowMsgBoxInternAsync(msg, msgButton), DispatcherPriority.Normal);
        //        op.Wait();
        //        return (Global.MsgResult)op.Result;
        //    }
        //}
        //else
        {
            return Global.MsgResult.OK;
        }
    }

    private async Task<Global.MsgResult> ShowMsgBoxInternAsync(Msg msg, Global.MsgResult defaultResult, eMsgButton msgButton)
    {
        VBMessageBox msgBox = new VBMessageBox(msg, defaultResult, msgButton);
        return await msgBox.ShowMessageAsync();
    }

    public void StoreSettingsWndPos(object settingsVBDesignWndPos)
    {
        //if ((settingsVBDesignWndPos == null) || !(settingsVBDesignWndPos is SettingsVBDesignWndPos))
        //    return;
        //SettingsVBDesignWndPos wndPos = settingsVBDesignWndPos as SettingsVBDesignWndPos;
        //if (Properties.Settings.Default.DockWndPositions == null)
        //    Properties.Settings.Default.DockWndPositions = new SettingsVBDesignWndPosList();
        //var query = Properties.Settings.Default.DockWndPositions.Where(c => c.ACIdentifier == wndPos.ACIdentifier);
        //if (query.Count() > 0)
        //{
        //    query.First().WndRect = wndPos.WndRect;
        //}
        //else
        //{
        //    Properties.Settings.Default.DockWndPositions.Add(wndPos);
        //}
    }

    public object ReStoreSettingsWndPos(string acName)
    {
        //if (String.IsNullOrEmpty(acName))
        //    return null;
        //if (Properties.Settings.Default.DockWndPositions == null)
        //{
        //    Properties.Settings.Default.DockWndPositions = new SettingsVBDesignWndPosList();
        //    return null;
        //}
        //var query = Properties.Settings.Default.DockWndPositions.Where(c => c.ACIdentifier == acName);
        //if (query.Count() <= 0)
        //    return null;
        //return query.First();
        return null;
    }


    public string OpenFileDialog(string filter, string initialDirectory = null, bool restoreDirectory = true)
    {
        MainWindow mainWindow = Parent as MainWindow;
        if (mainWindow == null)
            return null;

        return Dispatcher.UIThread.InvokeAsync<string>(() => MediaControllerProxy.OpenFileDialog(mainWindow.StorageProvider, false, initialDirectory, false, null, null)).GetAwaiter().GetResult();
    }

    public string SaveFileDialog(string filter, string initialDirectory = null, bool restoreDirectory = true)
    {
        MainWindow mainWindow = Parent as MainWindow;
        if (mainWindow == null)
            return null;

        return Dispatcher.UIThread.InvokeAsync<string>(() => MediaControllerProxy.SaveFileDialog(mainWindow.StorageProvider, initialDirectory, filter)).GetAwaiter().GetResult();

    }
    #endregion

    #region IACUrl Member
    /// <summary>
    /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
    /// 1. get references to components,
    /// 2. query property values,
    /// 3. execute method calls,
    /// 4. start and stop Components,
    /// 5. and send messages to other components.
    /// </summary>
    /// <param name="acUrl">String that adresses a command</param>
    /// <param name="acParameter">Parameters if a method should be invoked</param>
    /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
    public object ACUrlCommand(string acUrl, params Object[] acParameter)
    {
        switch (acUrl)
        {
            case Const.CmdShowMsgBox:
                return ShowMsgBoxAsync(acParameter[0] as Msg, (Global.MsgResult)acParameter[1], (eMsgButton)acParameter[2]);
            case Const.CmdStartBusinessobject:
                if (acParameter.Count() > 1)
                {
                    StartBusinessobject(acParameter[0] as string, acParameter[1] as ACValueList);
                }
                else
                {
                    StartBusinessobject(acParameter[0] as string, null);
                }
                return null;
            default:
                return null;
        }
    }

    /// <summary>
    /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
    /// </summary>
    /// <param name="acUrl">String that adresses a command</param>
    /// <param name="acParameter">Parameters if a method should be invoked</param>
    /// <returns>true if ACUrlCommand can be invoked</returns>
    public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
    {
        return true;
    }

    /// <summary>
    /// Returns the parent object
    /// </summary>
    /// <value>Reference to the parent object</value>
    public IACObject ParentACObject
    {
        get
        {
            return Parent as IACObject;
        }
    }

    /// <summary>
    /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
    /// </summary>
    /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
    /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
    /// <param name="source">The Source for WPF-Databinding</param>
    /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
    /// <param name="rightControlMode">Information about access rights for the requested object</param>
    /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
    public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
    {
        return false;
    }

    public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
    {
        return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
    }

    public string GetACUrlComponent(IACObject rootACObject = null)
    {
        return ACIdentifier;
    }

    /// <summary>
    /// Returns a ACUrl relatively to the passed object.
    /// If the passed object is null then the absolute path is returned
    /// </summary>
    /// <param name="rootACObject">Object for creating a realtive path to it</param>
    /// <returns>ACUrl as string</returns>
    [ACMethodInfo("", "", 9999)]
    public string GetACUrl(IACObject rootACObject = null)
    {
        return ACIdentifier;
    }

    /// <summary>
    /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
    /// </summary>
    /// <value> A nullable list ob IACObjects.</value>
    public IEnumerable<IACObject> ACContentList
    {
        get
        {
            return null;
        }
    }

    /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
    /// <value>  Translated description</value>
    [ACPropertyInfo(9999)]
    public string ACCaption
    {
        get
        {
            return null;
        }
    }

    /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
    /// <value>The Unique Identifier as string</value>
    [ACPropertyInfo(9999)]
    public string ACIdentifier
    {
        get
        {
            return this.Name;
        }
    }
    #endregion

    #region IRootPageWPF Member

    #region Blidschirm für nächsten Start einfrieren
    public static readonly StyledProperty<WPFControlSelectionEventArgs> VBDockingManagerFreezingProperty = AvaloniaProperty.Register<MainView, WPFControlSelectionEventArgs>(nameof(VBDockingManagerFreezing));
    public WPFControlSelectionEventArgs VBDockingManagerFreezing
    {
        get { return (WPFControlSelectionEventArgs)GetValue(VBDockingManagerFreezingProperty); }
        set { SetValue(VBDockingManagerFreezingProperty, value); }
    }

    // 1. Click auf StatusBar-Icon von Benutzer
    private void FreezeScreenIcon_Click(object sender, RoutedEventArgs e)
    {
        //FreezeScreenIcon.SwitchControlSelectionState();
        //if (FreezeScreenIcon.ControlSelectionActive)
        //    VBDockingManagerFreezing = new WPFControlSelectionEventArgs(ControlSelectionState.FrameSearch);
        //else
        //    VBDockingManagerFreezing = new WPFControlSelectionEventArgs(ControlSelectionState.Off);
    }

    // 2. Aufruf vom Dockingmanager, dass Rahmen geklickt worden ist => Schalte Modus aus
    public void DockingManagerFreezed(object dockingManager)
    {
        //FreezeScreenIcon.SwitchControlSelectionState();
        //if (FreezeScreenIcon.ControlSelectionActive)
        //    VBDockingManagerFreezing = new WPFControlSelectionEventArgs(ControlSelectionState.FrameSearch);
        //else
        //    VBDockingManagerFreezing = new WPFControlSelectionEventArgs(ControlSelectionState.Off);
    }
    #endregion

    #region Editierung des VB-Designs

    public static readonly StyledProperty<WPFControlSelectionEventArgs> VBDesignEditingProperty = AvaloniaProperty.Register<MainView, WPFControlSelectionEventArgs>(nameof(VBDesignEditing));
    public WPFControlSelectionEventArgs VBDesignEditing
    {
        get { return (WPFControlSelectionEventArgs)GetValue(VBDesignEditingProperty); }
        set { SetValue(VBDesignEditingProperty, value); }
    }

    // 1. Click auf StatusBar-Icon von Benutzer
    // 3. Click wenn Editierung zu Ende ist
    private void EditVBDesignIcon_Click(object sender, RoutedEventArgs e)
    {
        //EditVBDesignIcon.SwitchControlSelectionState();
        //if (EditVBDesignIcon.ControlSelectionActive)
        //    VBDesignEditing = new WPFControlSelectionEventArgs(ControlSelectionState.FrameSearch);
        //else
        //    VBDesignEditing = new WPFControlSelectionEventArgs(ControlSelectionState.Off);
    }

    // 2. Aufruf von VBDesign, dass Rahmen geklickt worden ist
    public void VBDesignEditingActivated(object vbDesign)
    {
        //if (!EditVBDesignIcon.ControlSelectionActive)
        //    EditVBDesignIcon.SwitchControlSelectionState();
        //VBDesignEditing = new WPFControlSelectionEventArgs(ControlSelectionState.FrameSelected);
    }
    #endregion

    #endregion


    #region IACInteractiveObject Member
    /// <summary>
    /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
    /// </summary>
    /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
    public IACType ACType
    {
        get;
    }


    /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
    /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
    /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
    /// <value>Relative or absolute ACUrl</value>
    [Category("VBControl")]
    public string VBContent
    {
        get
        {
            return null;
        }
    }

    /// <summary>
    /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
    /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
    /// </summary>
    /// <value>Root</value>
    public IACObject ContextACObject
    {
        get
        {
            return ACRoot.SRoot;
        }
    }

    /// <summary>
    /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
    /// </summary>
    /// <param name="actionArgs">Information about the type of interaction and the source</param>
    public void ACAction(ACActionArgs actionArgs)
    {
    }

    /// <summary>
    /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
    /// </summary>
    /// <param name="actionArgs">Information about the type of interaction and the source</param>
    /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
    public bool IsEnabledACAction(ACActionArgs actionArgs)
    {
        return false;
    }
    #endregion


    public bool SuppressOpenMessageBoxes { get; set; }

    public double Zoom => 1;

    public bool InFullscreen => true;

    #region Touchscreen
    private void OnPinchGesture(object sender, PinchEventArgs e)
    {
        //sldZoom.Value += e.AngleDelta;
    }
    #endregion

    private async Task WarningIcon_DoubleTapped(object sender, TappedEventArgs e)
    {
        IACComponent bsoAlarmExplorer = ACRoot.SRoot.Businessobjects.StartComponent("BSOAlarmExplorer", this, null) as IACComponent;
        if (bsoAlarmExplorer != null)
        {
            bsoAlarmExplorer.ACUrlCommand("!ShowAlarmExplorer");
            await bsoAlarmExplorer.Stop();
        }
    }

    public FocusBSOResult FocusBSO(IACBSO bso)
    {
        return FocusBSOResult.NotFocusable;
    }

    public void SwitchFullScreen()
    {
    }

    public void BackButton_Click(object sender, RoutedEventArgs e)
    {
        MainContentControl.CloseDesign();
    }

    public bool CanClose
    {
        get => MainContentControl.IsMainDesignActive;
    }

    #region IFocusChangeListener

    // This method doesn't exist in nuget packages of Avalonia version 11.3.8
    //protected override void OnLosingFocus(FocusChangingEventArgs e)
    //{
    //    base.OnLosingFocus(e);
    //    LastFocusedElement = e.OldFocusedElement;
    //}

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        LastFocusedElement = e.Source as IInputElement;
    }

    public IInputElement LastFocusedElement { get; private set; }
    #endregion
}
