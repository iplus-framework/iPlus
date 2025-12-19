using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{ 
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDockingManager'}de{'VBDockingManager'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public partial class VBDockingManager : UserControl, IACInteractiveObject, IACObject, IVBGui, ICommandBindingOwner
    {
        public event EventHandler OnInitVBControlFinished;

        #region c'tors
        public VBDockingManager() : base()
        {
            InitializeComponent();
            _DockControl = new DockControl();
            this.Content = _DockControl;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            InitVBControl();
        }

        #endregion

        #region Methods iPlus-Extension

        #region Init and Release
        bool _Loaded = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Loaded)
                return;
            _Loaded = true;

            AddToComponentReference();

            _Factory = new Factory();
            _Factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };
            _Factory.DockableClosing += _Factory_DockableClosing;
            _Factory.DockableClosed += _Factory_DockableClosed;
            _Factory.WindowClosing += _Factory_WindowClosing;
            _Factory.WindowClosed += _Factory_WindowClosed;

            _MainLayout = new ProportionalDock() { Orientation = Orientation.Vertical };
            DocumentDock documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = false
            };

            documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
            ProportionalDock horizontalDock = new ProportionalDock() { Orientation = Orientation.Horizontal };
            horizontalDock.VisibleDockables.Add(documentDock);
            _MainLayout.VisibleDockables = _Factory.CreateList<IDockable>(horizontalDock);

            List<IDockable> tools2Unpin = new List<IDockable>();

            if (VBDesignList != null)
            {
                int count = 0;
                VBPropertyGridView gridView = null;
                VBDesignEditor designEditor = null;
                VBDesignItemTreeView logicalTreeView = null;
                foreach (Control uiElement in VBDesignList)
                {
                    count++;
                    ShowVBDesign(uiElement, tools2Unpin);
                    if (uiElement is VBPropertyGridView)
                        gridView = (uiElement as VBPropertyGridView);
                    else if (uiElement is VBDesignEditor)
                        designEditor = (uiElement as VBDesignEditor);
                    else if (uiElement is VBDesignItemTreeView)
                        logicalTreeView = (uiElement as VBDesignItemTreeView);
                }

                if ((designEditor != null) && (gridView != null))
                    designEditor.PropertyGridView = gridView;
                if ((designEditor != null) && (logicalTreeView != null))
                    designEditor.DesignItemTreeView = logicalTreeView;
            }

            IRootDock root = _Factory.CreateRootDock();
            root.VisibleDockables = _Factory.CreateList<IDockable>(_MainLayout);
            root.DefaultDockable = _MainLayout;
            //root.LeftPinnedDockables = factory.CreateList<IDockable>(tool3);

            _Factory.InitLayout(root);
            DockControl.Factory = _Factory;
            DockControl.Layout = root;
            tools2Unpin.ForEach(c => _Factory.PinDockable(c));

            var bindingVarioWPF = new Binding
            {
                Source = this.Root().RootPageWPF,
                Path = "VBDockingManagerFreezing",
                Mode = BindingMode.OneWay
            };
            this.Bind(VBDockingManager.MasterPageFreezeProperty, bindingVarioWPF);

            if (OnInitVBControlFinished != null)
            {
                OnInitVBControlFinished(this, new EventArgs());
            }
        }

        private void _Factory_WindowClosed(object sender, Dock.Model.Core.Events.WindowClosedEventArgs e)
        {
        }

        private void _Factory_WindowClosing(object sender, Dock.Model.Core.Events.WindowClosingEventArgs e)
        {
            //CloseAndRemoveVBDesign();
        }

        private void _Factory_DockableClosed(object sender, Dock.Model.Core.Events.DockableClosedEventArgs e)
        {
            var map = DesignToolMap.Where(c => c.Dockable == e.Dockable).FirstOrDefault();
            if (map != null)
            {
                VBDesignList.Remove(map.Design);
                DesignToolMap.Remove(map);
            }

        }

        private void _Factory_DockableClosing(object sender, Dock.Model.Core.Events.DockableClosingEventArgs e)
        {
            var map = DesignToolMap.Where(c => c.Dockable == e.Dockable).FirstOrDefault();
            if (map != null && map.State == ClosingState.None) // && VBDockingManager.GetIsCloseableBSORoot(map.Design))
            {
                map.State = ClosingState.GUI;
                map.Design.StopAutoStartComponent();
            }

        }

        protected virtual void DeInitVBControl(IACComponent bso = null)
        {
            if (VBDesignList != null)
            {
                VBDesignList.Clear();
            }
            this.ClearAllBindings();
            if (_DesignToolMap != null)
            {
                foreach (var map in _DesignToolMap)
                {
                    map.Design.OnContextACObjectChanged -= UiElementAsDataDesign_OnContextACObjectChanged;
                }
                _DesignToolMap = null;
            }
        }

        protected void AddToComponentReference()
        {
            if (ContextACObject is IACComponent)
            {
                var binding = new Binding
                {
                    Source = ContextACObject,
                    Path = Const.ACUrlCmdMessage,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDockingManager.ACUrlCmdMessageProperty, binding);

                binding = new Binding
                {
                    Source = ContextACObject,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDockingManager.ACCompInitStateProperty, binding);
            }
        }

        protected void RemoveFromComponentReference()
        {
            //BindingOperations.ClearBinding(this, VBDockingManager.ACUrlCmdMessageProperty);
            //BindingOperations.ClearBinding(this, VBDockingManager.ACCompInitStateProperty);
            this.ClearAllBindings();
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (ContextACObject != null && ContextACObject is IACComponent && (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
            {
                RemoveFromComponentReference();
                DeInitVBControl(ContextACObject as IACComponent);
            }
        }

        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null && ACUrlCmdMessage.ACUrl == Const.CmdFindGUI)
            {
                try
                {
                    IACObject invoker = (IACObject)ACUrlCmdMessage.ACParameter[0];
                    string filterVBControlClassName = (string)ACUrlCmdMessage.ACParameter[1];
                    string filterControlName = (string)ACUrlCmdMessage.ACParameter[2];
                    string filterVBContent = (string)ACUrlCmdMessage.ACParameter[3];
                    string filterACNameOfComponent = (string)ACUrlCmdMessage.ACParameter[4];
                    bool withDialogStack = (bool)ACUrlCmdMessage.ACParameter[5];

                    bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
                    bool filterControlNameSet = !String.IsNullOrEmpty(filterControlName);
                    bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
                    bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
                    if (!filterVBControlClassNameSet && !filterControlNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                        return;

                    if (ACUrlHelper.IsSearchedGUIInstance(ACIdentifier, filterVBControlClassName, filterControlName, filterVBContent, filterACNameOfComponent))
                    {
                        if (withDialogStack)
                        {
                            if (DialogStack.Any())
                                invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                        }
                        else
                            invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDockingManager", "OnACUrlMessagereceived", msg);
                }
            }
        }

        #endregion

        #endregion

        #region IACUrl Member

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (acUrl == Const.EventDeInit)
            {
                RemoveFromComponentReference();
                DeInitVBControl();
            }
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (acUrl == Const.EventDeInit)
            {
                return true;
            }
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public virtual string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        [ACMethodInfo("", "", 9999)]
        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public members

        #region Show Methods for Dynamic Instances

        #region Design
        [ACMethodInfo("", "en{'Modal Dialog'}de{'Modaler Dialog'}", 9999)]
        public void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false,
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDockingManager.SetContainer(vbDesign, Global.VBDesignContainer.ModalDialog);
            VBDockingManager.SetRibbonBarVisibility(vbDesign, ribbonVisibility);
            VBDockingManager.SetCloseButtonVisibility(vbDesign, closeButtonVisibility);
            List<IDockable> tools2Unpin = new List<IDockable>();
            ShowVBDesign(vbDesign, tools2Unpin, acCaption);
        }

        [ACMethodInfo("", "en{'Show Layout'}de{'Layout'}", 9999)]
        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState,
            Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDockingManager.SetContainer(vbDesign, containerType);
            VBDockingManager.SetDockState(vbDesign, dockState);
            VBDockingManager.SetDockPosition(vbDesign, dockPosition);
            VBDockingManager.SetIsCloseableBSORoot(vbDesign, isClosableBSORoot);
            VBDockingManager.SetRibbonBarVisibility(vbDesign, ribbonVisibility);
            VBDockingManager.SetCloseButtonVisibility(vbDesign, closeButtonVisibility);

            //VBDockingManager.SetWindowSize(vbDesign, defaultWindowSize);
            VBDesignList.Add(vbDesign);
            List<IDockable> tools2Unpin = new List<IDockable>();
            ShowVBDesign(vbDesign, tools2Unpin, vbDesign.VBContent);
        }

        public void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption, bool ribbonVisibilityOff = false, Global.VBDesignDockState dockState = Global.VBDesignDockState.Tabbed)
        {
            if (IsBSOManager && (ContextACObject != null))
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

                VBDockingManager.SetContainer(vbDesign, Global.VBDesignContainer.DockableWindow);
                VBDockingManager.SetDockState(vbDesign, dockState);
                VBDockingManager.SetDockPosition(vbDesign, Global.VBDesignDockPosition.Right);
                VBDockingManager.SetIsCloseableBSORoot(vbDesign, true);
                if (ribbonVisibilityOff)
                    VBDockingManager.SetRibbonBarVisibility(vbDesign, Global.ControlModes.Collapsed);
                else
                    VBDockingManager.SetRibbonBarVisibility(vbDesign, Global.ControlModes.Enabled);
                VBDockingManager.SetCloseButtonVisibility(vbDesign, Global.ControlModes.Enabled);
                if (ControlManager.TouchScreenMode)
                    VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                VBDesignList.Add(vbDesign);
                List<IDockable> tools2Unpin = new List<IDockable>();
                ShowVBDesign(vbDesign, tools2Unpin, string.IsNullOrEmpty(vbDesign.ACCaption) ? vbDesign.Name : vbDesign.ACCaption);
            }
        }
        #endregion

        private void ShowVBDesign(Control uiElement, List<IDockable> tools2Unpin, string acCaption = "")
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
                            (uiElement as Control).DataContext = this.ContextACObject;
                    }
                }
                if (uiElementAsDataContent.ContextACObject == null)
                    return;
            }

            VBDesign uiElementAsDataDesign = uiElement as VBDesign;
            if (uiElementAsDataDesign != null)
            {
                if (string.IsNullOrEmpty(acCaption) && !string.IsNullOrEmpty(uiElementAsDataDesign.CustomizedACCaption))
                    acCaption = uiElementAsDataDesign.CustomizedACCaption;
                // Rechteprüfung ob Design geöffnet werden darf
                if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
                {
                    acCaption = uiElementAsDataDesign.ContentACObject.ACCaption;
                    if (uiElementAsDataContent.ContextACObject is IACComponent)
                    {
                        if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                            return;
                    }
                }
                if (string.IsNullOrEmpty(acCaption))
                    acCaption = uiElementAsDataDesign.VBContent;
                if (string.IsNullOrEmpty(acCaption) && !string.IsNullOrEmpty(uiElementAsDataDesign.AutoStartACComponent))
                    acCaption = uiElementAsDataDesign.AutoStartACComponent;
                if (string.IsNullOrEmpty(acCaption) && !string.IsNullOrEmpty(uiElementAsDataDesign.ACIdentifier))
                    acCaption = uiElementAsDataDesign.ACIdentifier;

            }
            string title = VBDockingManager.GetWindowTitle(uiElement);
            if (!string.IsNullOrEmpty(title))
                acCaption = title;

            Global.VBDesignContainer containerType = VBDockingManager.GetContainer(uiElement);
            Global.VBDesignDockState dockState = VBDockingManager.GetDockState(uiElement);
            bool isCloseable = VBDockingManager.GetIsCloseableBSORoot(uiElement);
            Size size = VBDockingManager.GetWindowSize(uiElement);
            double toolMaxWidth = 400;
            double toolMaxHeight = 1200;
            if (size.Width > 0.0001)
                toolMaxWidth = size.Width;
            if (size.Height > 0.0001)
                toolMaxHeight = size.Height;

            Control content = uiElement;
            var ribbonVisibility = VBDockingManager.GetRibbonBarVisibility(uiElement);
            if (ribbonVisibility != Global.ControlModes.Hidden)
            {
                VBDockPanel dockPanel = new VBDockPanel();
                var ribbonControl = new VBRibbonBSODefault();
                ribbonControl.SetValue(DockPanel.DockProperty, Avalonia.Controls.Dock.Top);
                if (ribbonVisibility == Global.ControlModes.Collapsed)
                    ribbonControl.IsVisible = false;
                else
                    ribbonControl.IsVisible = true;
                if (uiElementAsDataDesign != null)
                {
                    Binding binding = new Binding
                    {
                        Source = uiElementAsDataDesign,
                        Path = nameof(VBDesign.BSOACComponent),
                        Mode = BindingMode.OneWay
                    };
                    ribbonControl.Bind(VBRibbonBSODefault.BSOACComponentProperty, binding);

                    binding = new Binding
                    {
                        Source = uiElementAsDataDesign,
                        Path = nameof(StyledElement.DataContext),
                        Mode = BindingMode.OneWay
                    };
                    ribbonControl.Bind(StyledElement.DataContextProperty, binding);
                }
                uiElement.SetValue(DockPanel.LastChildFillProperty, true);
                dockPanel.Children.Add(ribbonControl);
                dockPanel.Children.Add(uiElement);
                content = dockPanel;
            }

            ProportionalDock horizontalArea = null;
            DocumentDock documentDock = null;
            EnsureDocumentDockStructure(out horizontalArea, out documentDock);

            if (   containerType == Global.VBDesignContainer.TabItem
                || dockState == Global.VBDesignDockState.Tabbed)
            {
                DockableBase doc = null;
                if (isCloseable)
                {
                    doc = new Tool
                    {
                        Id = "Doc_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable
                    };
                }
                else
                {
                    doc = new Document
                    {
                        Id = "Doc_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable
                    };
                }

                if (uiElementAsDataDesign != null && !DesignToolMap.Where(c => c.Design == uiElementAsDataDesign).Any())
                {
                    uiElementAsDataDesign.OnContextACObjectChanged += UiElementAsDataDesign_OnContextACObjectChanged;
                    DesignToolMap.Add(new DockedDesignInfo() { Design = uiElementAsDataDesign, Dockable = doc });
                }

                if (!isCloseable && !_TabStripVisibleStyleIsSet)
                {
                    _TabStripVisibleStyleIsSet = true;
                    DockControl.Styles.Add(new Style(x => x.OfType<DocumentControl>().Template().OfType<DocumentTabStrip>().Name("PART_TabStrip"))
                    {
                        Setters = 
                        {
                            new Setter(Visual.IsVisibleProperty, false)
                        }
                    });
                }

                if (documentDock.VisibleDockables == null)
                    documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
                documentDock.VisibleDockables.Add(doc);
            }
            else if (containerType == Global.VBDesignContainer.DockableWindow)
            {
                Alignment alignment = TranslateAlignment(VBDockingManager.GetDockPosition(uiElement));
                if (alignment == Alignment.Top || alignment == Alignment.Bottom)
                {
                    Tool tool = new Tool
                    {
                        Id = "Tool_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        Content = content,
                        MaxWidth = toolMaxWidth,
                        MaxHeight = toolMaxHeight,
                        CanClose = isCloseable
                    };

                    if (uiElementAsDataDesign != null && !DesignToolMap.Where(c => c.Design == uiElementAsDataDesign).Any())
                    {
                        uiElementAsDataDesign.OnContextACObjectChanged += UiElementAsDataDesign_OnContextACObjectChanged;
                        DesignToolMap.Add(new DockedDesignInfo() { Design = uiElementAsDataDesign, Dockable = tool });
                    }

                    //if (this.VBDesignContent is VBDesign)
                    //    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);

                    ToolDock toolDock = MainLayout.VisibleDockables.OfType<ToolDock>().Where(c => c.Alignment == alignment).FirstOrDefault();

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            Alignment = alignment,
                            Proportion = 0.25,
                            VisibleDockables = _Factory.CreateList<IDockable>(tool),
                        };
                        toolDock.ActiveDockable = tool;
                        if (alignment == Alignment.Top)
                        {
                            MainLayout.VisibleDockables.Insert(0, new ProportionalDockSplitter());
                            MainLayout.VisibleDockables.Insert(0, toolDock);
                        }
                        else
                        {
                            MainLayout.VisibleDockables.Add(new ProportionalDockSplitter());
                            MainLayout.VisibleDockables.Add(toolDock);
                        }
                    }
                    else
                    {
                        toolDock.VisibleDockables.Add(tool);
                    }
                    if (dockState == Global.VBDesignDockState.AutoHideButton)
                        tools2Unpin.Add(tool);
                }
                else
                {
                    ToolDock toolDock = horizontalArea.VisibleDockables.OfType<ToolDock>().Where(c => c.Alignment == alignment).FirstOrDefault();

                    Tool tool = new Tool
                    {
                        Id = "Tool_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        Content = content,
                        MaxWidth = toolMaxWidth,
                        MaxHeight = toolMaxHeight,
                        CanClose = isCloseable,
                        CanFloat = isCloseable
                    };

                    if (uiElementAsDataDesign != null && !DesignToolMap.Where(c => c.Design == uiElementAsDataDesign).Any())
                    {
                        uiElementAsDataDesign.OnContextACObjectChanged += UiElementAsDataDesign_OnContextACObjectChanged;
                        DesignToolMap.Add(new DockedDesignInfo() { Design = uiElementAsDataDesign, Dockable = tool });
                    }

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            Alignment = alignment,
                            Proportion = 0.25,
                            VisibleDockables = _Factory.CreateList<IDockable>(tool),
                        };
                        toolDock.ActiveDockable = tool;
                        if (alignment == Alignment.Left)
                        {
                            horizontalArea.VisibleDockables.Insert(0, new ProportionalDockSplitter());
                            horizontalArea.VisibleDockables.Insert(0, toolDock);
                        }
                        else
                        {
                            horizontalArea.VisibleDockables.Add(new ProportionalDockSplitter());
                            horizontalArea.VisibleDockables.Add(toolDock);
                        }
                    }
                    else
                    {
                        toolDock.VisibleDockables.Add(tool);
                    }
                    if (dockState == Global.VBDesignDockState.AutoHideButton)
                        tools2Unpin.Add(tool);
                }

                //VBDockingContainerToolWindowVB toolWin = new VBDockingContainerToolWindowVB(this, uiElement);
                //// TODO: ToolWindow wird nicht angezeigt
                //// ContextACObject ist beim zweiten mal null
                //if (ContextACObject != null)
                //{
                //    SettingsVBDesignWndPos wndPos = this.Root().RootPageWPF.ReStoreSettingsWndPos(toolWin.ACIdentifier) as SettingsVBDesignWndPos;
                //    if (wndPos != null)
                //    {
                //        toolWin.Show(null, wndPos);
                //    }
                //    else
                //        toolWin.Show(null, null);
                //}
                //else
                //    toolWin.Show(null, null);
            }
            else if ((uiElementAsDataContent != null) && (containerType == Global.VBDesignContainer.ModalDialog))
            {
                // TODO:
                //VBWindowDialogRoot vbDialogRoot = new VBWindowDialogRoot(uiElementAsDataContent.ContextACObject, uiElement, this);
                ////vbDialogRoot.WindowStyle = System.Windows.WindowStyle.None;
                //if (vbDialogRoot.Owner == null)
                //{
                //    StyledElement dp = this;
                //    while (dp != null)
                //    {
                //        var foundParent = VBLogicalTreeHelper.FindParentObjectInLogicalTree(dp, typeof(Window));
                //        dp = foundParent as StyledElement;
                //        if (dp != null)
                //        {
                //            Window ownerWindow = dp as Window;
                //            if (ownerWindow != null && ownerWindow.IsLoaded)
                //            {
                //                //vbDialogRoot.Owner = ownerWindow;
                //                vbDialogRoot.Show(ownerWindow);
                //                break;
                //            }
                //            var parent = LogicalTreeHelper.GetParent(dp);
                //            dp = parent as StyledElement;
                //        }
                //    }
                //}
                //vbDialogRoot.Resources = this.Resources;

                //ACClassDesign acClassDesign = null;
                //if (string.IsNullOrEmpty(uiElementAsDataContent.VBContent))
                //{
                //    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                //}
                //else if (uiElementAsDataContent.VBContent[0] == '*')
                //{
                //    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(uiElementAsDataContent.VBContent.Substring(1));
                //}
                //if (acClassDesign == null)
                //    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);

                //if (acClassDesign != null)
                //{
                //    vbDialogRoot.Title = string.IsNullOrEmpty(acCaption) ? acClassDesign.ACCaption : acCaption;
                //    if (acClassDesign.VisualHeight > 0)
                //        vbDialogRoot.Height = acClassDesign.VisualHeight;
                //    if (acClassDesign.VisualWidth > 0)
                //        vbDialogRoot.Width = acClassDesign.VisualWidth;

                //    if (acClassDesign.VisualHeight > 0 || acClassDesign.VisualWidth > 0)
                //        vbDialogRoot.CanResize = false;
                //}
                //IRoot root = this.Root();
                //if (root != null && root.RootPageWPF != null && root.RootPageWPF.InFullscreen)
                //    vbDialogRoot.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //else
                //    vbDialogRoot.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                //DialogStack.Add(vbDialogRoot);
                //if (root != null && root.RootPageWPF != null && root.RootPageWPF is Window && vbDialogRoot.Owner == null)
                //    vbDialogRoot.Show(root.RootPageWPF as Window);
                //else
                //    vbDialogRoot.Show();
                //vbDialogRoot.ShowDialog();
            }
        }

        private void EnsureDocumentDockStructure(out ProportionalDock horizontalArea, out DocumentDock documentDock)
        {
            documentDock = null;
            horizontalArea = this.MainLayout.VisibleDockables.OfType<ProportionalDock>()
                                    .Where(c => c.Orientation == Orientation.Horizontal).FirstOrDefault();

            // All Documents (BSO's) have been closed including Docked windows.
            if (horizontalArea == null)
            {
                horizontalArea = new ProportionalDock() { Orientation = Orientation.Horizontal };
                int i = 0;
                IDockable previousDockable = null;
                ToolDock bottomDock = null;
                ToolDock topDock = null;
                foreach (var verticalDockElement in MainLayout.VisibleDockables)
                {
                    if (verticalDockElement is ToolDock toolDock)
                    {
                        if (toolDock.Alignment == Alignment.Top)
                            topDock = toolDock;
                        else if (toolDock.Alignment == Alignment.Bottom)
                        {
                            bottomDock = toolDock;
                            if (previousDockable is ProportionalDockSplitter)
                                i--;
                            break;
                        }
                    }
                    else if (verticalDockElement is DocumentDock)
                        documentDock = verticalDockElement as DocumentDock;

                    previousDockable = verticalDockElement;
                    i++;
                }

                // If only top without a ProportionalDockSplitter, then add a splitter before Document Dock
                if (topDock != null && bottomDock == null && !(previousDockable is ProportionalDockSplitter))
                {
                    MainLayout.VisibleDockables.Insert(i, new ProportionalDockSplitter());
                    i++;
                }

                if (documentDock == null)
                {
                    // Add document dock
                    documentDock = new DocumentDock
                    {
                        Id = "Documents",
                        IsCollapsable = false,
                        CanCreateDocument = false
                    };
                    documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
                    horizontalArea.VisibleDockables.Add(documentDock);
                }
                else
                {
                    MainLayout.VisibleDockables.Remove(documentDock);
                    i--;
                    horizontalArea.VisibleDockables.Add(documentDock);
                }

                MainLayout.VisibleDockables.Insert(i, horizontalArea);
                i++;

                if (bottomDock != null && !(previousDockable is ProportionalDockSplitter))
                    MainLayout.VisibleDockables.Insert(i, new ProportionalDockSplitter());
            }

            if (documentDock == null)
                documentDock = horizontalArea.VisibleDockables.OfType<DocumentDock>().FirstOrDefault();
            if (documentDock == null)
            {
                // Add document dock
                documentDock = new DocumentDock
                {
                    Id = "Documents",
                    IsCollapsable = false,
                    CanCreateDocument = false
                };
                documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
                horizontalArea.VisibleDockables.Add(documentDock);
            }
        }

        private void UiElementAsDataDesign_OnContextACObjectChanged(object sender, EventArgs e)
        {
            if (sender == null)
                return;
            VBDesign uiElementAsDataDesign = sender as VBDesign;
            if (uiElementAsDataDesign == null)
                return;

            var map = DesignToolMap.Where(c => c.Design == uiElementAsDataDesign).FirstOrDefault();
            if (map != null)
            {
                map.Dockable.Title = RefreshTitle(uiElementAsDataDesign);
                if (uiElementAsDataDesign.BSOACComponent != null)
                    uiElementAsDataDesign.BSOACComponent.PropertyChanged += Docked_BSOACComponent_PropertyChanged;
            }
        }

        private void Docked_BSOACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InitState" && sender is IACBSO bso)
            {
                var map = DesignToolMap.Where(c => c.Design.BSOACComponent == bso).FirstOrDefault();
                if (map != null && map.State == ClosingState.None && VBDockingManager.GetIsCloseableBSORoot(map.Design))
                {
                    map.State = ClosingState.BSO;
                    Factory.CloseDockable(map.Dockable);
                }
            }
        }

        public virtual string RefreshTitle(VBDesign vbDesign)
        {
            string acCaption = vbDesign.ACCaption;
            string title = VBDockingManager.GetWindowTitle(vbDesign);
            if (!string.IsNullOrEmpty(title))
                return title;
            if (vbDesign.ContextACObject != null)
            {
                if (!string.IsNullOrEmpty(vbDesign.CustomizedACCaption))
                    acCaption = vbDesign.CustomizedACCaption;
                else
                    acCaption = vbDesign.ContextACObject.ACCaption;
            }
            if (!string.IsNullOrEmpty(acCaption))
                return acCaption;
            if (vbDesign.DataContext is IACComponent acComponent
                && !string.IsNullOrEmpty(vbDesign.VBContent)
                && vbDesign.VBContent.StartsWith("*"))
            {
                ACClassDesign acClassDesign = acComponent.GetDesign(vbDesign.VBContent.Substring(1));
                if (acClassDesign != null)
                    return acClassDesign.ACCaption;
            }
            if (string.IsNullOrEmpty(acCaption))
                acCaption = vbDesign.VBContent;
            return acCaption;
        }


        public void SaveUserFloatingWindowSize(VBDockingContainerBase dockingContainer, Point ptFloatingWindow, Size sizeFloatingWindow)
        {
            if (ContextACObject == null)
                return;
            SettingsVBDesignWndPos wndPos = new SettingsVBDesignWndPos(dockingContainer.ACIdentifier, new Rect(ptFloatingWindow, sizeFloatingWindow));
            this.Root().RootPageWPF.StoreSettingsWndPos(wndPos);
        }

        bool _ReinitAtStartupDone = false;
        public void InitBusinessobjectsAtStartup()
        {
            if (ControlManager.TouchScreenMode && IsBSOManager)
                this.TabItemMinHeight = 35;

            if (_ReinitAtStartupDone)
                return;
            _ReinitAtStartupDone = true;

            if (MainLayout == null)
                return;

            foreach (IDockable dockable in MainLayout.VisibleDockables)
            {
                InitVBDesignsRecursively(dockable);
            }

            //foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            //{
            //    VBDesign vbDesign = toolWin.VBDesignContent as VBDesign;
            //    if (vbDesign != null)
            //    {
            //        if (ControlManager.TouchScreenMode && IsBSOManager)
            //            VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
            //        vbDesign.InitVBControl();
            //    }
            //    toolWin.ReInitDataContext();
            //}
            //foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            //{
            //    VBDesign vbDesign = tabbedDoc.VBDesignContent as VBDesign;
            //    if (vbDesign != null)
            //    {
            //        if (ControlManager.TouchScreenMode && IsBSOManager)
            //            VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
            //        vbDesign.InitVBControl();
            //    }
            //    tabbedDoc.ReInitDataContext();
            //}

            //if (_dockingBtnGroups != null)
            //{
            //    foreach (var dockgrps in this._dockingBtnGroups)
            //    {
            //        if (dockgrps.Buttons == null)
            //            continue;
            //        foreach (var button in dockgrps.Buttons)
            //        {
            //            button.RefreshTitle();
            //        }
            //    }
            //}
        }

        private void InitVBDesignsRecursively(IDockable dockable)
        {
            // Check if this dockable is a Tool with VBDesign content
            if (dockable is Tool tool && tool.Content is VBDesign vbDesignTool)
            {
                vbDesignTool.InitVBControl();
            }
            // Check if this dockable is a Document with VBDesign content
            else if (dockable is Document document && document.Content is VBDesign vbDesignDoc)
            {
                vbDesignDoc.InitVBControl();
            }
            // Check if this dockable has VisibleDockables (like ProportionalDock, ToolDock, DocumentDock)
            else if (dockable is IDock dock && dock.VisibleDockables != null)
            {
                foreach (IDockable childDockable in dock.VisibleDockables)
                {
                    InitVBDesignsRecursively(childDockable);
                }
            }
        }

        private void FillContextMenu(ItemsControl itemsControl, IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
        {
            foreach (var acMenuItem in acMenuItemList)
            {
                VBMenuItem vbMenuItem = new VBMenuItem(acElement.ContextACObject, acMenuItem);
                itemsControl.Items.Add(vbMenuItem);
                if (acMenuItem.Items != null && acMenuItem.Items.Count > 0)
                {
                    FillContextMenu(vbMenuItem, acElement, acMenuItem.Items);
                }
            }
        }

        List<IVBDialog> _DialogStack = null;
        public List<IVBDialog> DialogStack
        {
            get
            {
                if (_DialogStack == null)
                    _DialogStack = new List<IVBDialog>();
                return _DialogStack;
            }
        }

        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", 9999)]
        public void CloseTopDialog()
        {
            if (_DialogStack == null || _DialogStack.Count < 1)
                return;

            IVBDialog dialog = _DialogStack[_DialogStack.Count - 1];
            dialog.CloseDialog();
            OnCloseTopDialog(dialog);
        }

        internal void OnCloseTopDialog(IVBDialog dialog)
        {
            if (_DialogStack == null || _DialogStack.Count < 1)
                return;
            IVBDialog dialogTop = _DialogStack[_DialogStack.Count - 1];
            if (dialogTop == dialog)
            {
                _DialogStack.RemoveAt(_DialogStack.Count - 1);
            }
            else if (_DialogStack.Contains(dialog))
            {
                _DialogStack.Remove(dialog);
            }
        }
        #endregion

        #endregion

        #region Add and Remove DockingContainerTabbedDoc
        public void CloseAllTabs()
        {
            //foreach (var tab in PART_gridDocking.vbDockingPanelTabbedDoc.Documents.ToList())
            //{
            //    VBDockingContainerToolWindowVB tabbedDoc = tab as VBDockingContainerToolWindowVB;
            //    if (tabbedDoc != null)
            //    {
            //        if (VBDockingManager.GetIsCloseableBSORoot(tabbedDoc.VBDesignContent))
            //        {
            //            RemoveDockingContainerToolWindowTabbed(tabbedDoc);
            //            tabbedDoc.OnCloseWindow();
            //        }
            //    }
            //}
        }
        #endregion

        #region Persistence

        void RootPageWPF_VBDockingManagerFreezingEvent()
        {
            if (MasterPageFreeze == null)
                return;
            if (MasterPageFreeze.ControlSelectionState == ControlSelectionState.Off)
                FreezeActive = false;
            else
                FreezeActive = true;
        }

        // Double-Click for Freezing this
        void _PART_gridDocking_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!FreezeActive || (ContextACObject == null))
                return;
            //if (!(e.Source is VBDockingGrid))
            //    return;
            //if ((e.Source as VBDockingGrid).vbDockingPanelTabbedDoc.DockManager != this)
            //    return;
            SerializeVBDesignList();
            this.Root().RootPageWPF.DockingManagerFreezed(this);
            e.Handled = true;
        }

        #endregion

        public void ACAction(ACActionArgs actionArgs)
        {
            return;
        }

        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }
    }
}
