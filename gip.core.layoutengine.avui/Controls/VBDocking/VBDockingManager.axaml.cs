using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
using System.Reflection;
using System.Threading.Tasks;

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
            _Factory.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };
            _Factory.DockableClosing += _Factory_DockableClosing;
            _Factory.DockableClosed += _Factory_DockableClosed;
            _Factory.WindowClosing += _Factory_WindowClosing;
            _Factory.WindowClosed += _Factory_WindowClosed;

            _MainLayout = new ProportionalDock() { Orientation = Orientation.Vertical, DockGroup = DockGroupId };
            DocumentDock documentDock = new DocumentDock
            {
                Id = "Documents",
                IsCollapsable = false,
                CanCreateDocument = false,
                DockGroup = DockGroupId
            };

            documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
            ProportionalDock horizontalDock = new ProportionalDock() { Orientation = Orientation.Horizontal, DockGroup = DockGroupId };
            horizontalDock.VisibleDockables.Add(documentDock);
            _MainLayout.VisibleDockables = _Factory.CreateList<IDockable>(horizontalDock);

            List<IDockable> tools2Unpin = new List<IDockable>();

            if (VBDesignList != null)
            {
                int count = 0;
                foreach (Control uiElement in VBDesignList)
                {
                    count++;
                    ShowVBDesign(uiElement, tools2Unpin);
                }
                ConnectDesignerTools();
            }

            IRootDock root = _Factory.CreateRootDock();
            root.DockGroup = DockGroupId;
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

        internal void ConnectDesignerTools(VBDesignEditor designEditor = null)
        {
            if (VBDesignList != null)
            {
                if (designEditor == null)
                    designEditor = VBDesignList.OfType<VBDesignEditor>().FirstOrDefault();
                if (designEditor == null)
                    return;
                foreach (Control uiElement in VBDesignList)
                {
                    if (uiElement is VBPropertyGridView gridView)
                        designEditor.PropertyGridView = gridView;
                    else if (uiElement is VBDesignItemTreeView logicalTreeView)
                        designEditor.DesignItemTreeView = logicalTreeView;
                    else if (uiElement is VBDesign vBDesign)
                        vBDesign.OuterDesignEditor = designEditor;
                }

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

        [ACMethodInfo("", "en{'Modal Dialog'}de{'Modaler Dialog'}", 9999)]
        public async Task ShowDialogAsync(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false,
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDockingManager.SetContainer(vbDesign, Global.VBDesignContainer.ModalDialog);
            VBDockingManager.SetRibbonBarVisibility(vbDesign, ribbonVisibility);
            VBDockingManager.SetCloseButtonVisibility(vbDesign, closeButtonVisibility);
            List<IDockable> tools2Unpin = new List<IDockable>();
            await ShowVBDesignAsync(vbDesign, tools2Unpin, acCaption);
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
            double toolWidth = 400;
            double toolHeight = 1200;
            if (size.Width > 0.0001)
                toolWidth = size.Width;
            if (size.Height > 0.0001)
                toolHeight = size.Height;
            double toolMinWidth = toolWidth * 0.5;
            double toolMinHeight = toolHeight * 0.5;
            double toolMaxWidth = toolWidth * 2;
            double toolMaxHeight = toolHeight * 2;

            PixelRect? pixelRect = null;
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null && topLevel.Screens != null && topLevel.Screens.Primary != null)
                pixelRect = topLevel.Screens.Primary.Bounds;

// #if AVALONIAFORK
//             if (pixelRect.HasValue)
//             {
//                 toolMaxWidth = pixelRect.Value.Width * 0.9;
//                 toolMaxHeight = pixelRect.Value.Height * 0.9;
//             }
// #else
//             if (pixelRect.HasValue)
//             {
//                 if (toolMaxWidth > pixelRect.Value.Width * 0.9)
//                     toolMaxWidth = pixelRect.Value.Width * 0.9;
//                 if (toolMaxHeight > pixelRect.Value.Height * 0.9)
//                     toolMaxHeight = pixelRect.Value.Height * 0.9;
//             }
// #endif

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
                        DockGroup = DockGroupId,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable,
                        //MaxWidth = toolMaxWidth,
                        //MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                    };
                }
                else
                {
                    doc = new Document
                    {
                        Id = "Doc_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        DockGroup = DockGroupId,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable,
                        // MaxWidth = toolMaxWidth,
                        // MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                    };
                }

                RegisterDesignDockable(uiElementAsDataDesign, doc);

                if (!isCloseable)
                {
                    UpdateTabStripVisibility();
                }

                bool activateNewTab = isCloseable
                    && uiElementAsDataDesign != null
                    && !string.IsNullOrEmpty(uiElementAsDataDesign.AutoStartACComponent);
                AddAndActivateDocumentDockable(documentDock, doc, activateNewTab);
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
                        DockGroup = DockGroupId,
                        Content = content,
                        MaxWidth = toolMaxWidth,
                        MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                        CanClose = isCloseable
                    };

                    RegisterDesignDockable(uiElementAsDataDesign, tool);

                    //if (this.VBDesignContent is VBDesign)
                    //    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);

                    ToolDock toolDock = MainLayout.VisibleDockables.OfType<ToolDock>().Where(c => c.Alignment == alignment).FirstOrDefault();

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            DockGroup = DockGroupId,
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
                        DockGroup = DockGroupId,
                        Content = content,
// #if AVALONIAFORK
//                         MaxWidth = toolMaxWidth,
//                         MaxHeight = toolMaxHeight,
//                         // Width = toolWidth,
//                         // Height = toolHeight,
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #else
//                         MaxWidth = toolMaxWidth,
//                         MaxHeight = toolMaxHeight,
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                        CanClose = isCloseable,
                        CanFloat = isCloseable
                    };

                    RegisterDesignDockable(uiElementAsDataDesign, tool);

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            DockGroup = DockGroupId,
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

        private async Task ShowVBDesignAsync(Control uiElement, List<IDockable> tools2Unpin, string acCaption = "")
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
            double toolWidth = 400;
            double toolHeight = 1200;
            if (size.Width > 0.0001)
                toolWidth = size.Width;
            if (size.Height > 0.0001)
                toolHeight = size.Height;
            double toolMinWidth = toolWidth * 0.5;
            double toolMinHeight = toolHeight * 0.5;
            double toolMaxWidth = toolWidth * 2;
            double toolMaxHeight = toolHeight * 2;

            PixelRect? pixelRect = null;
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null && topLevel.Screens != null && topLevel.Screens.Primary != null)
                pixelRect = topLevel.Screens.Primary.Bounds;

// #if AVALONIAFORK
//             if (pixelRect.HasValue)
//             {
//                 toolMaxWidth = pixelRect.Value.Width * 0.9;
//                 toolMaxHeight = pixelRect.Value.Height * 0.9;
//             }
// #else
//             if (pixelRect.HasValue)
//             {
//                 if (toolMaxWidth > pixelRect.Value.Width * 0.9)
//                     toolMaxWidth = pixelRect.Value.Width * 0.9;
//                 if (toolMaxHeight > pixelRect.Value.Height * 0.9)
//                     toolMaxHeight = pixelRect.Value.Height * 0.9;
//             }
// #endif

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

            if (containerType == Global.VBDesignContainer.TabItem
                && dockState == Global.VBDesignDockState.Tabbed)
            {
                DockableBase doc = null;
                if (isCloseable)
                {
                    doc = new Tool
                    {
                        Id = "Doc_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        DockGroup = DockGroupId,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable,
                        //MaxWidth = toolMaxWidth,
                        //MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                    };
                }
                else
                {
                    doc = new Document
                    {
                        Id = "Doc_" + uiElement.GetHashCode().ToString(),
                        Title = acCaption,
                        DockGroup = DockGroupId,
                        Content = content,
                        CanClose = isCloseable,
                        CanFloat = isCloseable,
                        MaxWidth = toolMaxWidth,
                        MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                    };
                }

                RegisterDesignDockable(uiElementAsDataDesign, doc);

                if (!isCloseable)
                {
                    UpdateTabStripVisibility();
                }

                bool activateNewTab = isCloseable
                    && uiElementAsDataDesign != null
                    && !string.IsNullOrEmpty(uiElementAsDataDesign.AutoStartACComponent);
                AddAndActivateDocumentDockable(documentDock, doc, activateNewTab);
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
                        DockGroup = DockGroupId,
                        Content = content,
                        MaxWidth = toolMaxWidth,
                        MaxHeight = toolMaxHeight,
// #if AVALONIAFORK
//                         // Width = toolWidth,
//                         // Height = toolHeight,
// #else
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                        CanClose = isCloseable
                    };

                    RegisterDesignDockable(uiElementAsDataDesign, tool);

                    //if (this.VBDesignContent is VBDesign)
                    //    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);

                    ToolDock toolDock = MainLayout.VisibleDockables.OfType<ToolDock>().Where(c => c.Alignment == alignment).FirstOrDefault();

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            DockGroup = DockGroupId,
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
                        DockGroup = DockGroupId,
                        Content = content,
// #if AVALONIAFORK
//                         MaxWidth = toolMaxWidth,
//                         MaxHeight = toolMaxHeight,
//                         // Width = toolWidth,
//                         // Height = toolHeight,
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #else
//                         MaxWidth = toolMaxWidth,
//                         MaxHeight = toolMaxHeight,
//                         MinWidth = toolMinWidth,
//                         MinHeight = toolMinHeight,
// #endif
                        CanClose = isCloseable,
                        CanFloat = isCloseable
                    };

                    RegisterDesignDockable(uiElementAsDataDesign, tool);

                    if (toolDock == null)
                    {
                        toolDock = new ToolDock
                        {
                            Id = "ToolDock_" + alignment.ToString(),
                            DockGroup = DockGroupId,
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
                VBWindowDialogRoot vbDialogRoot = new VBWindowDialogRoot(uiElementAsDataContent.ContextACObject, uiElement, this);
                //vbDialogRoot.WindowStyle = System.Windows.WindowStyle.None;
                if (vbDialogRoot.Owner == null)
                {
                    StyledElement dp = this;
                    while (dp != null)
                    {
                        var foundParent = VBLogicalTreeHelper.FindParentObjectInLogicalTree(dp, typeof(Window));
                        dp = foundParent as StyledElement;
                        if (dp != null)
                        {
                            Window ownerWindow = dp as Window;
                            if (ownerWindow != null && ownerWindow.IsLoaded)
                            {
                                //vbDialogRoot.Owner = ownerWindow;
                                vbDialogRoot.Show(ownerWindow);
                                break;
                            }
                            var parent = LogicalTreeHelper.GetParent(dp);
                            dp = parent as StyledElement;
                        }
                    }
                }

                ACClassDesign acClassDesign = null;
                if (string.IsNullOrEmpty(uiElementAsDataContent.VBContent))
                {
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                }
                else if (uiElementAsDataContent.VBContent[0] == '*')
                {
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(uiElementAsDataContent.VBContent.Substring(1));
                }
                if (acClassDesign == null)
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);

                if (acClassDesign != null)
                {
                    vbDialogRoot.Title = string.IsNullOrEmpty(acCaption) ? acClassDesign.ACCaption : acCaption;
                    if (acClassDesign.VisualHeight > 0)
                        vbDialogRoot.Height = acClassDesign.VisualHeight;
                    if (acClassDesign.VisualWidth > 0)
                        vbDialogRoot.Width = acClassDesign.VisualWidth;

                    if (acClassDesign.VisualHeight > 0 || acClassDesign.VisualWidth > 0)
                        vbDialogRoot.CanResize = false;
                }
                IRoot root = this.Root();
                if (root != null && root.RootPageWPF != null && root.RootPageWPF.InFullscreen)
                    vbDialogRoot.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                else
                    vbDialogRoot.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                DialogStack.Add(vbDialogRoot);
                
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime classicDesktop)
                {
                    await vbDialogRoot.ShowDialog(classicDesktop.MainWindow);
                }
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
                horizontalArea = new ProportionalDock() { Orientation = Orientation.Horizontal, DockGroup = DockGroupId };
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
                        CanCreateDocument = false,
                        DockGroup = DockGroupId
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
                    CanCreateDocument = false,
                    DockGroup = DockGroupId
                };
                documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
                horizontalArea.VisibleDockables.Add(documentDock);
            }
        }

        private void EnsureTabStripStyle(bool tabsVisible)
        {
            if (_tabStripStyle == null || _tabStripVisibleSetter == null)
            {
                _tabStripVisibleSetter = new Setter(Visual.IsVisibleProperty, tabsVisible);
                _tabStripStyle = new Style(x => x
                    .OfType<DocumentControl>()
                    .Template()
                    .OfType<DocumentTabStrip>()
                    .Name("PART_TabStrip"))
                {
                    Setters = { _tabStripVisibleSetter }
                };

                DockControl.Styles.Add(_tabStripStyle);
            }
            else
            {
                _tabStripVisibleSetter.Value = tabsVisible;
            }
        }

        private void UpdateTabStripVisibility()
        {
            bool tabsVisible = VBDesignList.Count > 1;
            if (tabsVisible)
                tabsVisible = VBDesignList.Where(c => VBDockingManager.GetContainer(c) == Global.VBDesignContainer.TabItem || VBDockingManager.GetDockState(c) == Global.VBDesignDockState.Tabbed).Count() > 1;
            EnsureTabStripStyle(tabsVisible);
        }

        private void AddAndActivateDocumentDockable(DocumentDock documentDock, DockableBase doc, bool activateNewTab)
        {
            if (documentDock == null || doc == null)
                return;

            // During initial layout construction DockControl.Layout is not assigned yet,
            // so we cannot rely on factory activation plumbing.
            if (DockControl?.Layout == null)
            {
                if (documentDock.VisibleDockables == null)
                    documentDock.VisibleDockables = _Factory.CreateList<IDockable>();
                documentDock.VisibleDockables.Add(doc);
                if (activateNewTab)
                    documentDock.ActiveDockable = doc;
                return;
            }

            _Factory.AddDockable(documentDock, doc);
            if (activateNewTab)
            {
                _Factory.SetActiveDockable(doc);
                _Factory.SetFocusedDockable(documentDock, doc);
            }
        }

        private void RegisterDesignDockable(VBDesign design, DockableBase dockable)
        {
            if (design == null || dockable == null)
                return;

            var map = DesignToolMap.Where(c => c.Design == design).FirstOrDefault();
            if (map == null)
            {
                design.OnContextACObjectChanged += UiElementAsDataDesign_OnContextACObjectChanged;
                map = new DockedDesignInfo { Design = design, Dockable = dockable };
                DesignToolMap.Add(map);
            }
            else
            {
                map.Dockable = dockable;
            }

            // For AutoStart designs the real child context is often not resolved yet.
            // Keep the existing dock title until BSOACComponent is available, otherwise
            // fallback resolution can incorrectly overwrite it with manager caption.
            if (!IsDeferredAutoStartDesign(design))
            {
                // Deferred tab content may initialize later; force the best available title immediately.
                var title = RefreshTitle(design);
                if (!string.IsNullOrEmpty(title))
                    dockable.Title = title;
            }
            else
            {
                // Try to resolve caption from the target businessobject class metadata.
                var autoStartCaption = TryResolveAutoStartCaption(design);
                if (!string.IsNullOrEmpty(autoStartCaption))
                    dockable.Title = autoStartCaption;
            }

            if (design.BSOACComponent != null)
            {
                design.BSOACComponent.PropertyChanged -= Docked_BSOACComponent_PropertyChanged;
                design.BSOACComponent.PropertyChanged += Docked_BSOACComponent_PropertyChanged;
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
                {
                    uiElementAsDataDesign.BSOACComponent.PropertyChanged -= Docked_BSOACComponent_PropertyChanged;
                    uiElementAsDataDesign.BSOACComponent.PropertyChanged += Docked_BSOACComponent_PropertyChanged;
                }
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

            if (!string.IsNullOrEmpty(vbDesign.CustomizedACCaption))
                return vbDesign.CustomizedACCaption;

            if (vbDesign.ContentACObject != null && !string.IsNullOrEmpty(vbDesign.ContentACObject.ACCaption))
                return vbDesign.ContentACObject.ACCaption;

            var autoStartCaption = TryResolveAutoStartCaption(vbDesign);
            if (!string.IsNullOrEmpty(autoStartCaption))
                return autoStartCaption;

            // For VBContent path bindings (without '*'), resolve the target design directly
            // from DataContext so startup-restored auto-hide buttons can get the final title
            // before deferred content initialization runs.
            if (vbDesign.DataContext != null
                && !string.IsNullOrEmpty(vbDesign.VBContent)
                && !vbDesign.VBContent.StartsWith("*"))
            {
                var boundDesign = ResolveFromPropertyPath(vbDesign.DataContext, vbDesign.VBContent) as IACObjectDesign;
                if (boundDesign != null && !string.IsNullOrEmpty(boundDesign.ACCaption))
                    return boundDesign.ACCaption;
            }

            // Resolve potential design caption without waiting for VBDesign to be attached/initialized.
            if (vbDesign.DataContext is IACComponent acComponent)
            {
                ACClassDesign acClassDesign = null;

                if (!string.IsNullOrEmpty(vbDesign.VBContent) && vbDesign.VBContent.StartsWith("*"))
                {
                    acClassDesign = acComponent.GetDesign(vbDesign.VBContent.Substring(1));
                }
                else if (string.IsNullOrEmpty(vbDesign.VBContent))
                {
                    // Parent is often not known yet while deferred tab content is not realized,
                    // so try mobile first and then desktop as best-effort early lookup.
                    acClassDesign = acComponent.GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMainMobile)
                                   ?? acComponent.GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                }

                if (acClassDesign != null && !string.IsNullOrEmpty(acClassDesign.ACCaption))
                    return acClassDesign.ACCaption;
            }

            if (vbDesign.ContextACObject != null)
            {
                acCaption = vbDesign.ContextACObject.ACCaption;
            }

            if (!string.IsNullOrEmpty(acCaption))
                return acCaption;

            if (string.IsNullOrEmpty(acCaption))
                acCaption = vbDesign.VBContent;

            return acCaption;
        }

        private object ResolveFromPropertyPath(object source, string propertyPath)
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyPath))
                return null;

            object current = source;
            string[] parts = propertyPath.Split('.');
            foreach (string part in parts)
            {
                if (current == null || string.IsNullOrWhiteSpace(part))
                    return null;

                var property = current.GetType().GetProperty(part, BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                    return null;

                current = property.GetValue(current);
            }

            return current;
        }


        //public void SaveUserFloatingWindowSize(VBDockingContainerBase dockingContainer, Point ptFloatingWindow, Size sizeFloatingWindow)
        //{
        //    if (ContextACObject == null)
        //        return;
        //    SettingsVBDesignWndPos wndPos = new SettingsVBDesignWndPos(dockingContainer.ACIdentifier, new Rect(ptFloatingWindow, sizeFloatingWindow));
        //    this.Root().RootPageWPF.StoreSettingsWndPos(wndPos);
        //}

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

            // Auto-hidden tools are pinned and not part of MainLayout.VisibleDockables.
            // Initialize them as well so their caption/title is ready before first preview click.
            if (DockControl?.Layout is IRootDock rootDock)
            {
                InitVBDesignsFromCollection(rootDock.LeftPinnedDockables);
                InitVBDesignsFromCollection(rootDock.RightPinnedDockables);
                InitVBDesignsFromCollection(rootDock.TopPinnedDockables);
                InitVBDesignsFromCollection(rootDock.BottomPinnedDockables);
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

        private void InitVBDesignsFromCollection(IEnumerable<IDockable> dockables)
        {
            if (dockables == null)
                return;

            foreach (var dockable in dockables)
            {
                InitVBDesignsRecursively(dockable);
            }
        }

        private VBDesign TryGetVBDesignFromDockableContent(object content)
        {
            if (content is VBDesign vbDesign)
                return vbDesign;

            if (content is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    var nested = TryGetVBDesignFromDockableContent(child);
                    if (nested != null)
                        return nested;
                }
            }

            if (content is ContentControl contentControl)
            {
                var nested = TryGetVBDesignFromDockableContent(contentControl.Content);
                if (nested != null)
                    return nested;
            }

            if (content is Decorator decorator)
            {
                var nested = TryGetVBDesignFromDockableContent(decorator.Child);
                if (nested != null)
                    return nested;
            }

            if (content is AvaloniaObject avaloniaObject)
            {
                return VBVisualTreeHelper.FindChildObjectInVisualTree(avaloniaObject, typeof(VBDesign)) as VBDesign;
            }

            return null;
        }

        private bool CanInitializeDesignAtStartup(VBDesign vbDesign)
        {
            if (vbDesign == null)
                return false;

            // AutoStart designs require BSOACComponent for ACUrlCommand invocation.
            // During startup restore this can still be null for pinned/auto-hide tools.
            if (IsDeferredAutoStartDesign(vbDesign))
                return false;

            return true;
        }

        private bool IsDeferredAutoStartDesign(VBDesign vbDesign)
        {
            return vbDesign != null
                && !string.IsNullOrEmpty(vbDesign.AutoStartACComponent)
                && vbDesign.BSOACComponent == null;
        }

        private string TryResolveAutoStartCaption(VBDesign vbDesign)
        {
            if (vbDesign == null || string.IsNullOrWhiteSpace(vbDesign.AutoStartACComponent))
                return null;

            var autoStartIdentifier = ExtractAutoStartIdentifier(vbDesign.AutoStartACComponent);
            if (string.IsNullOrEmpty(autoStartIdentifier))
                return null;

            ACClass classInfo = null;

            if (vbDesign.DataContext is IACComponent dataComponent && dataComponent.ACType is ACClass parentClass)
            {
                classInfo = parentClass.ACClass_ParentACClass?.FirstOrDefault(c => c.ACIdentifier == autoStartIdentifier);
            }

            if (classInfo == null && Database.GlobalDatabase != null)
            {
                classInfo = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACIdentifier == autoStartIdentifier);
            }

            return string.IsNullOrEmpty(classInfo?.ACCaption) ? null : classInfo.ACCaption;
        }

        private string ExtractAutoStartIdentifier(string autoStartAcUrl)
        {
            if (string.IsNullOrWhiteSpace(autoStartAcUrl))
                return null;

            var token = autoStartAcUrl.Trim();

            var hashIndex = token.LastIndexOf('#');
            if (hashIndex >= 0 && hashIndex + 1 < token.Length)
                token = token.Substring(hashIndex + 1);

            var slashIndex = token.LastIndexOf('\\');
            if (slashIndex >= 0 && slashIndex + 1 < token.Length)
                token = token.Substring(slashIndex + 1);

            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        private void UpdateDockableTitleFromDesign(IDockable dockable, VBDesign vbDesign)
        {
            if (dockable == null || vbDesign == null)
                return;

            // Keep the title that came from persisted state/creation until AutoStart context is resolved.
            if (IsDeferredAutoStartDesign(vbDesign))
            {
                var autoStartCaption = TryResolveAutoStartCaption(vbDesign);
                if (!string.IsNullOrEmpty(autoStartCaption) && dockable.Title != autoStartCaption)
                    dockable.Title = autoStartCaption;
                return;
            }

            var title = RefreshTitle(vbDesign);
            if (!string.IsNullOrEmpty(title) && dockable.Title != title)
                dockable.Title = title;
        }

        private void InitVBDesignsRecursively(IDockable dockable)
        {
            // Check if this dockable is a Tool with VBDesign content
            if (dockable is Tool tool)
            {
                var vbDesignTool = TryGetVBDesignFromDockableContent(tool.Content);
                if (vbDesignTool != null)
                {
                    UpdateDockableTitleFromDesign(dockable, vbDesignTool);
                    if (CanInitializeDesignAtStartup(vbDesignTool))
                        vbDesignTool.InitVBControl();
                }
            }
            // Check if this dockable is a Document with VBDesign content
            else if (dockable is Document document)
            {
                var vbDesignDoc = TryGetVBDesignFromDockableContent(document.Content);
                if (vbDesignDoc != null)
                {
                    UpdateDockableTitleFromDesign(dockable, vbDesignDoc);
                    if (CanInitializeDesignAtStartup(vbDesignDoc))
                        vbDesignDoc.InitVBControl();
                }
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
