using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace gip.core.layoutengine.avui
{
    [TemplatePart(Name = "PART_BorderFreeze", Type = typeof(Border))]
    [TemplatePart(Name = "PART_btnPanelLeft", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelRight", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelTop", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelBottom", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_gridDocking", Type = typeof(VBDockingGrid))]
    [TemplatePart(Name = "PART_panelFront", Type = typeof(DockPanel))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDockingManager'}de{'VBDockingManager'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public partial class VBDockingManager : ContentControl, IVBDockDropSurface, IACInteractiveObject, IACObject, IVBGui
    {
        public event EventHandler OnInitVBControlFinished;


        #region c'tors
        bool _themeApplied = false;
        public VBDockingManager() : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            object partObj = (object)GetTemplateChild("PART_BorderFreeze");
            if ((partObj != null) && (partObj is Border))
            {
                _PART_BorderFreeze = ((Border)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelLeft");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelLeft = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelRight");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelRight = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelTop");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelTop = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelBottom");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelBottom = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_gridDocking");
            if ((partObj != null) && (partObj is VBDockingGrid))
            {
                _PART_gridDocking = ((VBDockingGrid)partObj);
                _PART_gridDocking.MouseEnter += OnHideAutoHidePane;
                _PART_gridDocking.MouseDown += new MouseButtonEventHandler(_PART_gridDocking_MouseDown);
            }

            partObj = (object)GetTemplateChild("PART_panelFront");
            if ((partObj != null) && (partObj is DockPanel))
            {
                _PART_panelFront = ((DockPanel)partObj);
            }
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
            if (_Loaded || PART_gridDocking == null)
                return;
            _Loaded = true;

            AddToComponentReference();

            DragPanelServices.Register(this);

            _overlayWindow = new VBDockingOverlayWindow(this);

            PART_gridDocking.AttachDockManager(this);
            PART_gridDocking.vbDockingPanelTabbedDoc.Show();
            AddSelectionChangedHandler();

            if (VBDesignList != null)
            {
                int count = 0;
                VBPropertyGridView gridView = null;
                VBDesignEditor designEditor = null;
                VBDesignItemTreeView logicalTreeView = null;
                foreach (Control uiElement in VBDesignList)
                {
                    count++;
                    ShowVBDesign(uiElement);
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

            Binding bindingVarioWPF = new Binding();
            bindingVarioWPF.Source = this.Root().RootPageWPF;
            bindingVarioWPF.Path = "VBDockingManagerFreezing";
            bindingVarioWPF.Mode = BindingMode.OneWay;
            this.Bind(VBDockingManager.MasterPageFreezeProperty, bindingVarioWPF);

            AddSelectionChangedHandler();
            if (OnInitVBControlFinished != null)
            {
                OnInitVBControlFinished(this, new EventArgs());
            }
        }

        protected virtual void DeInitVBControl(IACComponent bso = null)
        {
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList.ToList())
            {
                if (content.VBDockingPanel is VBDockingPanelToolWindow)
                    RemoveDockingPanelToolWindow(content.VBDockingPanel as VBDockingPanelToolWindow);
                content.Close();
                content.DeInitVBControl();
            }
            foreach (VBDockingContainerTabbedDoc docContent in TabbedDocContainerList.ToList())
            {
                docContent.CloseTab();
                //docContent.DeInitVBControl();
            }
            if (_overlayWindow != null)
                _overlayWindow.Close();

            if (PART_gridDocking != null)
            {
                PART_gridDocking.MouseEnter -= OnHideAutoHidePane;
                PART_gridDocking.MouseDown -= _PART_gridDocking_MouseDown;
                PART_gridDocking.DeInitVBControl();
            }

            if (_dockingBtnGroups != null)
            {
                foreach (VBDockingButtonGroup group in _dockingBtnGroups)
                {
                    foreach (VBDockingButton button in group.Buttons)
                    {
                        if (button.DockingContainerToolWindow != null)
                        {
                            button.DockingContainerToolWindow.DeInitVBControl();
                            button.DockingContainerToolWindow = null;
                        }
                    }
                    group.Buttons.Clear();
                }
                _dockingBtnGroups.Clear();
            }

            if (VBDesignList != null)
            {
                VBDesignList.Clear();

            }
            RemoveSelectionChangedHandler();
            BindingOperations.ClearBinding(this, VBDockingManager.MasterPageFreezeProperty);
            this.ClearAllBindings();
            _dockingBtnGroups = null;
            _overlayWindow = null;

            _tempPane = null;
            _currentButton = null;
            _ParentWindow = null;
            _dragPanelServices = null;
            _overlayWindow = null;
            _PART_BorderFreeze = null;
            _PART_btnPanelLeft = null;
            _PART_btnPanelRight = null;
            _PART_btnPanelTop = null;
            _PART_btnPanelBottom = null;
            _PART_gridDocking = null;
            _PART_panelFront = null;
        }

        protected void AddToComponentReference()
        {
            if (ContextACObject is IACComponent)
            {
                Binding binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = Const.ACUrlCmdMessage;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBDockingManager.ACUrlCmdMessageProperty, binding);

                binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBDockingManager.ACCompInitStateProperty, binding);
            }
        }

        protected void RemoveFromComponentReference()
        {
            BindingOperations.ClearBinding(this, VBDockingManager.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDockingManager.ACCompInitStateProperty);
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
                                invoker.ACUrlCommand(Const.CmdFindGUIResult,this);
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
            ShowVBDesign(vbDesign, acCaption);
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
            ShowVBDesign(vbDesign);
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
                ShowVBDesign(vbDesign);
            }
        }
        #endregion

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
                            (uiElement as Control).DataContext = this.ContextACObject;
                    }
                }
                if (uiElementAsDataContent.ContextACObject == null)
                    return;
            }

            if (uiElement is VBDesign)
            {
                VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
                // Rechtepr�fung ob Design ge�ffnet werden darf
                if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
                {
                    if (uiElementAsDataContent.ContextACObject is IACComponent)
                    {
                        if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                            return;
                    }
                }
            }

            Global.VBDesignContainer containerType = VBDockingManager.GetContainer(uiElement);

            if (containerType == Global.VBDesignContainer.TabItem)
            {
                VBDockingContainerTabbedDoc tabDoc = new VBDockingContainerTabbedDoc(this, uiElement);
                AddDockingContainerTabbedDoc_GetDockingPanel(tabDoc);
                tabDoc.Show();
            }
            else if (containerType == Global.VBDesignContainer.DockableWindow)
            {
                VBDockingContainerToolWindowVB toolWin = new VBDockingContainerToolWindowVB(this, uiElement);
                // TODO: ToolWindow wird nicht angezeigt
                // ContextACObject ist beim zweiten mal null
                if (ContextACObject != null)
                {
                    SettingsVBDesignWndPos wndPos = this.Root().RootPageWPF.ReStoreSettingsWndPos(toolWin.ACIdentifier) as SettingsVBDesignWndPos;
                    if (wndPos != null)
                    {
                        toolWin.Show(null, wndPos);
                    }
                    else
                        toolWin.Show(null, null);
                }
                else
                    toolWin.Show(null, null);
            }
            else if ((uiElementAsDataContent != null) && (containerType == Global.VBDesignContainer.ModalDialog))
            {
                VBWindowDialogRoot vbDialogRoot = new VBWindowDialogRoot(uiElementAsDataContent.ContextACObject, uiElement, this);
                //vbDialogRoot.WindowStyle = System.Windows.WindowStyle.None;
                if (vbDialogRoot.Owner == null)
                {
                    AvaloniaObject dp = this;
                    while (dp != null)
                    {
                        dp = VBLogicalTreeHelper.FindParentObjectInLogicalTree(dp, typeof(Window));
                        if (dp != null)
                        {
                            Window ownerWindow = dp as Window;
                            if (ownerWindow.IsLoaded)
                            {
                                //vbDialogRoot.Owner = ownerWindow;
                                vbDialogRoot.Show(ownerWindow);
                                break;
                            }
                            dp = LogicalTreeHelper.GetParent(dp);
                        }
                    }
                }
                vbDialogRoot.Resources = this.Resources;

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
                if (root != null && root.RootPageWPF != null && root.RootPageWPF is Window && vbDialogRoot.Owner == null)
                    vbDialogRoot.Show(root.RootPageWPF as Window);
                else
                    vbDialogRoot.Show();
                //vbDialogRoot.ShowDialog();
            }
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

            if (_ReinitAtStartupDone || vbDockingPanelTabbedDoc == null)
                return;
            _ReinitAtStartupDone = true;

            foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            {
                VBDesign vbDesign = toolWin.VBDesignContent as VBDesign;
                if (vbDesign != null)
                {
                    if (ControlManager.TouchScreenMode && IsBSOManager)
                        VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                    vbDesign.InitVBControl();
                }
                toolWin.ReInitDataContext();
            }
            foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            {
                VBDesign vbDesign = tabbedDoc.VBDesignContent as VBDesign;
                if (vbDesign != null)
                {
                    if (ControlManager.TouchScreenMode && IsBSOManager)
                        VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                    vbDesign.InitVBControl();
                }
                tabbedDoc.ReInitDataContext();
            }

            if (_dockingBtnGroups != null)
            {
                foreach (var dockgrps in this._dockingBtnGroups)
                {
                    if (dockgrps.Buttons == null)
                        continue;
                    foreach (var button in dockgrps.Buttons)
                    {
                        button.RefreshTitle();
                    }
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

        public void CloseAndRemoveVBDesign(Control uiElement)
        {
            if (uiElement == null)
                return;
            if (!(uiElement is VBDesign))
                return;
            VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
            if (VBDockingManager.GetIsCloseableBSORoot(uiElementAsDataDesign))
                uiElementAsDataDesign.StopAutoStartComponent();
            VBDesignList.Remove(uiElement);
            this.Focus();
        }
        #endregion

        #endregion

        #region Add and Remove DockingContainerToolWindow

        internal void AddDockingContainerToolWindow(VBDockingContainerBase container)
        {
            if (!ToolWindowContainerList.Contains(container))
                ToolWindowContainerList.Add(container);
        }


        internal VBDockingPanelTabbedDoc AddDockingContainerToolWindow_GetDockingPanel(VBDockingContainerBase content)
        {
            System.Diagnostics.Debug.Assert(!ToolWindowContainerList.Contains(content));
            System.Diagnostics.Debug.Assert(!PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Contains(content));

            if (!PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Contains(content))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.AddDockingContainerToolWindow(content);
                PART_gridDocking.ArrangeLayout();
            }

            return PART_gridDocking.vbDockingPanelTabbedDoc;
        }


        internal void RemoveDockingContainerToolWindow(VBDockingContainerBase container)
        {
            ToolWindowContainerList.Remove(container);
        }
        #endregion

        #region Add and Remove DockingPanelToolWindow

        internal void AddDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            PART_gridDocking.Add(panel);
            AttachDockingPanelToolWindowEvents(panel);
        }


        internal void RemoveDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            PART_gridDocking.Remove(panel);
            DetachDockingPanelToolWindowEvents(panel);
        }
        #endregion

        #region Add and Remove DockingContainerTabbedDoc

        public VBDockingPanelTabbedDoc AddDockingContainerTabbedDoc_GetDockingPanel(VBDockingContainerTabbedDoc container)
        {
            if (!PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(container))
                PART_gridDocking.vbDockingPanelTabbedDoc.AddDockingContainerTabbedDoc(container);

            return PART_gridDocking.vbDockingPanelTabbedDoc;
        }

        public bool RemoveDockingContainerTabbedDoc(VBDockingContainerTabbedDoc container)
        {
            if (PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(container))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.RemoveDockingContainerTabbedDoc(container);
                return true;
            }
            return false;
        }

        public bool RemoveDockingContainerToolWindowTabbed(VBDockingContainerToolWindow toolWindow)
        {
            if (PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(toolWindow))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.RemoveDockingContainerToolWindow(toolWindow);
                return true;
            }
            return false;
        }

        public void CloseAllTabs()
        {
            foreach (var tab in PART_gridDocking.vbDockingPanelTabbedDoc.Documents.ToList())
            {
                VBDockingContainerToolWindowVB tabbedDoc = tab as VBDockingContainerToolWindowVB;
                if (tabbedDoc != null)
                {
                    if (VBDockingManager.GetIsCloseableBSORoot(tabbedDoc.VBDesignContent))
                    {
                        RemoveDockingContainerToolWindowTabbed(tabbedDoc);
                        tabbedDoc.OnCloseWindow();
                    }
                }
            }
        }
        #endregion

        #region Eventhandling

        private void OnUnloaded(object sender, EventArgs e)
        {
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList)
                content.Close();
            foreach (VBDockingContainerTabbedDoc docContent in TabbedDocContainerList)
                docContent.Close();

            _overlayWindow.Close();
        }


        internal void AttachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged += new EventHandler(DockingPanelToolWindow_OnStateChanged);

            PART_gridDocking.AttachDockingPanelToolWindowEvents(panel);
        }


        internal void DetachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged -= new EventHandler(DockingPanelToolWindow_OnStateChanged);

            PART_gridDocking.DetachDockingPanelToolWindowEvents(panel);
        }


        void DockingPanelToolWindow_OnStateChanged(object sender, EventArgs e)
        {
            VBDockingPanelToolWindow pane = sender as VBDockingPanelToolWindow;
            if (pane.State == VBDockingPanelState.AutoHide)
            {
                HideDockingPanelToolWindow_AsDockingButton(pane);
                ShowTempPanel(false);
                HideTempPanel(true);
            }
            else if (pane.State == VBDockingPanelState.Docked && _currentButton != null)
            {
                this._tempPane.ChangeState(VBDockingPanelState.Docked);
            }
        }
        #endregion

        #region DockingButtons

        List<VBDockingButtonGroup> _dockingBtnGroups = new List<VBDockingButtonGroup>();

        private void HideDockingPanelToolWindow_AsDockingButton(VBDockingPanelToolWindow panel)
        {
            VBDockingButtonGroup buttonGroup = null;

                buttonGroup = new VBDockingButtonGroup();
                buttonGroup.Dock = panel.Dock;


            foreach (VBDockingContainerToolWindow container in panel.ContainerToolWindowsList)
            {
                VBDockingButton btn = new VBDockingButton();
                btn.DockingContainerToolWindow = container;
                btn.DockingButtonGroup = buttonGroup;

                if (_currentButton == null)
                    _currentButton = btn;
                buttonGroup.Buttons.Add(btn);
                //if (!isNewGroup)
                //{
                //    MakeNewDockingButtonVisible(buttonGroup, btn);
                //}
            }
            
            {
                _dockingBtnGroups.Add(buttonGroup);
                MakeDockingButtonsVisible(buttonGroup);
            }
        }

        private void MakeNewDockingButtonVisible(VBDockingButtonGroup group, VBDockingButton btn)
        {
            btn.MouseEnter += new MouseEventHandler(OnShowAutoHidePanel);
            Border br = new Border();
            br.Width = br.Height = 10;
            switch (group.Dock)
            {
                case Dock.Left:
                    btn.LayoutTransform = new RotateTransform(90);
                    PART_btnPanelLeft.Children.Add(btn);
                    PART_btnPanelLeft.Children.Add(br);
                    break;
                case Dock.Right:
                    btn.LayoutTransform = new RotateTransform(90);
                    PART_btnPanelRight.Children.Add(btn);
                    PART_btnPanelRight.Children.Add(br);
                    break;
                case Dock.Top:
                    PART_btnPanelTop.Children.Add(btn);
                    PART_btnPanelTop.Children.Add(br);
                    break;
                case Dock.Bottom:
                    PART_btnPanelBottom.Children.Add(btn);
                    PART_btnPanelBottom.Children.Add(br);
                    break;
            }
        }

        /// <summary>
        /// Add a group of docking buttons to the relative border stack panel
        /// </summary>
        /// <param name="group">Group to add</param>
        private void MakeDockingButtonsVisible(VBDockingButtonGroup group)
        {
            foreach (VBDockingButton btn in group.Buttons)
                btn.MouseEnter += new MouseEventHandler(OnShowAutoHidePanel);

            Border br = new Border();
            br.Width = br.Height = 10;
            switch (group.Dock)
            {
                case Dock.Left:
                    foreach (VBDockingButton btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        PART_btnPanelLeft.Children.Add(btn);
                    }
                    PART_btnPanelLeft.Children.Add(br);
                    break;
                case Dock.Right:
                    foreach (VBDockingButton btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        PART_btnPanelRight.Children.Add(btn);
                    }
                    PART_btnPanelRight.Children.Add(br);
                    break;
                case Dock.Top:
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelTop.Children.Add(btn);
                    PART_btnPanelTop.Children.Add(br);
                    break;
                case Dock.Bottom:
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelBottom.Children.Add(btn);
                    PART_btnPanelBottom.Children.Add(br);
                    break;
            }


        }

        /// <summary>
        /// Remove a group of docking buttons from the relative border stack panel
        /// </summary>
        /// <param name="group">Group to remove</param>
        private void HideDockingButtons(VBDockingButtonGroup group)
        {
            if (group.Buttons.Count <= 0)
                return;
            foreach (VBDockingButton btn in group.Buttons)
                btn.MouseEnter -= new MouseEventHandler(OnShowAutoHidePanel);

            switch (group.Dock)
            {
                case Dock.Left:
                    PART_btnPanelLeft.Children.RemoveAt(PART_btnPanelLeft.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelLeft.Children.Remove(btn);
                    break;
                case Dock.Right:
                    PART_btnPanelRight.Children.RemoveAt(PART_btnPanelRight.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelRight.Children.Remove(btn);
                    break;
                case Dock.Top:
                    PART_btnPanelTop.Children.RemoveAt(PART_btnPanelTop.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelTop.Children.Remove(btn);
                    break;
                case Dock.Bottom:
                    PART_btnPanelBottom.Children.RemoveAt(PART_btnPanelBottom.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelBottom.Children.Remove(btn);
                    break;
            }
        }

        public void RemoveDockingButton(VBDockingContainerToolWindow ofWindow)
        {
            VBDockingButtonGroup groupToRemove = null;
            foreach (VBDockingButtonGroup group in _dockingBtnGroups)
            {
                var queryButton = group.Buttons.Where(c => c.DockingContainerToolWindow == ofWindow);
                if (queryButton.Any())
                {
                    VBDockingButton button = queryButton.First();
                    switch (group.Dock)
                    {
                        case Dock.Left:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelLeft, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Right:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelRight, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Top:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelTop, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Bottom:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelBottom, button, group);
                            groupToRemove = group;
                            break;
                    }
                }
            }
            if (groupToRemove != null)
                _dockingBtnGroups.Remove(groupToRemove);
        }

        private void RemoveDockingButtonFromStackPanel(StackPanel stackPanel, VBDockingButton button, VBDockingButtonGroup group)
        {
            bool found = false;
            Border br = null;
            foreach (Control child in stackPanel.Children)
            {
                if (found)
                {
                    if (child is Border)
                        br = child as Border;
                    break;
                }
                if (child == button)
                {
                    found = true;
                }
            }
            if (found)
            {
                stackPanel.Children.Remove(button);
                if (br != null)
                    stackPanel.Children.Remove(br);
                group.Buttons.Remove(button);
            }
        }
        #endregion

        #region Overlay Panel

        VBDockingPanelToolWindowOverlay _tempPane = null;

        VBDockingButton _currentButton;


        void OnShowAutoHidePanel(object sender, MouseEventArgs e)
        {
            if (_currentButton == sender)
                return;

            HideTempPanel(true);

            _currentButton = sender as VBDockingButton;

            ShowTempPanel(true);
        }


        void OnHideAutoHidePane(object sender, MouseEventArgs e)
        {
            HideTempPanel(true);
        }


        private void HideTempPanel(bool smooth)
        {
            if (_tempPane != null)
            {
                VBDockingPanelToolWindow pane = PART_gridDocking.GetVBDockingPanelFromContainer(_tempPane.ContainerToolWindowsList[0]) as VBDockingPanelToolWindow;
                bool right_left = false;
                double length = 0.0;

                switch (_currentButton.DockingButtonGroup.Dock)
                {
                    case Dock.Left:
                    case Dock.Right:
                        if (_tempPanelAnimation != null)
                            pane.PaneWidth = _lengthAnimation;
                        else
                            pane.PaneWidth = _tempPane.Width;
                        length = _tempPane.Width;
                        right_left = true;
                        break;
                    case Dock.Top:
                    case Dock.Bottom:
                        if (_tempPanelAnimation != null)
                            pane.PaneHeight = _lengthAnimation;
                        else
                            pane.PaneHeight = _tempPane.Height;
                        length = _tempPane.Height;
                        right_left = false;
                        break;
                }

                _tempPane.OnStateChanged -= new EventHandler(_tempPane_OnStateChanged);

                if (smooth)
                {
                    HideOverlayPanel(length, right_left);
                }
                else
                {
                    ForceHideOverlayPanel();
                    PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, null);
                    PART_panelFront.Children.Clear();
                    PART_panelFront.Opacity = 0.0;
                    _tempPane.Close();
                }

                _tempPane.DeInitVBControl();
                _currentButton = null;
                _tempPane = null;
            }

        }


        private void ShowTempPanel(bool smooth)
        {

            _tempPane = new VBDockingPanelToolWindowOverlay(this, _currentButton.DockingContainerToolWindow, _currentButton.DockingButtonGroup.Dock);
            _tempPane.OnStateChanged += new EventHandler(_tempPane_OnStateChanged);

            VBDockingPanelToolWindow pane = PART_gridDocking.GetVBDockingPanelFromContainer(_currentButton.DockingContainerToolWindow) as VBDockingPanelToolWindow;
            pane.SetDefaultWithFromVBDesign(_currentButton.DockingContainerToolWindow);
            PART_panelFront.Children.Clear();
            _tempPane.SetValue(DockPanel.DockProperty, _currentButton.DockingButtonGroup.Dock);
            PART_panelFront.Children.Add(_tempPane);
            VBDockingSplitter splitter = null;
            bool right_left = false;
            double length = 0.0;
            this.Focus();

            switch (_currentButton.DockingButtonGroup.Dock)
            {
                case Dock.Left:
                    splitter = new VBDockingSplitter(_tempPane, null, VBDockSplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    right_left = true;
                    break;
                case Dock.Right:
                    splitter = new VBDockingSplitter(null, _tempPane, VBDockSplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    right_left = true;
                    break;
                case Dock.Top:
                    splitter = new VBDockingSplitter(_tempPane, null, VBDockSplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    right_left = false;
                    break;
                case Dock.Bottom:
                    splitter = new VBDockingSplitter(null, _tempPane, VBDockSplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    right_left = false;
                    break;
            }

            splitter.SetValue(DockPanel.DockProperty, _currentButton.DockingButtonGroup.Dock);
            PART_panelFront.Children.Add(splitter);

            if (smooth)
                ShowOverlayPanel(length, right_left);
            else
            {
                if (right_left)
                    _tempPane.Width = length;
                else
                    _tempPane.Height = length;
                PART_panelFront.Opacity = 1.0;
            }

        }

        void _tempPane_OnStateChanged(object sender, EventArgs e)
        {
            VBDockingPanelBase panel = PART_gridDocking.GetVBDockingPanelFromContainer(_currentButton.DockingContainerToolWindow);

            if (_currentButton != null)
            {
                switch (_currentButton.DockingButtonGroup.Dock)
                {
                    case Dock.Left:
                    case Dock.Right:
                        panel.PaneWidth = _tempPane.PaneWidth;
                        break;
                    case Dock.Top:
                    case Dock.Bottom:
                        panel.PaneHeight = _tempPane.PaneHeight;
                        break;
                }

                //if ((sender as VBDockingPanelToolWindow).State == VBDockingPanelState.Docked || (sender as VBDockingPanelToolWindow).State == VBDockingPanelState.TabbedDocument)
                {
                    HideDockingButtons(_currentButton.DockingButtonGroup);
                    _dockingBtnGroups.Remove(_currentButton.DockingButtonGroup);
                }
            }



            bool showOriginalPane = (_tempPane.State == VBDockingPanelState.Docked);

            HideTempPanel(false);

            if (showOriginalPane)
                panel.Show();
            else
                panel.Hide();
        }


        #region Temporary pane animation methods

        bool _leftRightAnimation = false;

        double _lengthAnimation = 0;

        VBDockingPanelToolWindowOverlay _tempPanelAnimation;

        DoubleAnimation _animation;


        void ShowOverlayPanel(double length, bool left_right)
        {
            ForceHideOverlayPanel();

            _leftRightAnimation = left_right;
            _tempPanelAnimation = _tempPane;
            _lengthAnimation = length;

            _animation = new DoubleAnimation();
            _animation.From = 0.0;
            _animation.To = length;
            _animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            _animation.Completed += new EventHandler(ShowOverlayPanel_Completed);
            if (_leftRightAnimation)
                _tempPanelAnimation.BeginAnimation(Control.WidthProperty, _animation);
            else
                _tempPanelAnimation.BeginAnimation(Control.HeightProperty, _animation);

            DoubleAnimation anOpacity = new DoubleAnimation();
            anOpacity.From = 0.0;
            anOpacity.To = 1.0;
            anOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, anOpacity);
        }


        void ShowOverlayPanel_Completed(object sender, EventArgs e)
        {
            _animation.Completed -= new EventHandler(ShowOverlayPanel_Completed);

            if (_tempPanelAnimation != null)
            {
                if (_leftRightAnimation)
                {
                    _tempPanelAnimation.BeginAnimation(Control.WidthProperty, null);
                    _tempPanelAnimation.Width = _lengthAnimation;
                }
                else
                {
                    _tempPanelAnimation.BeginAnimation(Control.HeightProperty, null);
                    _tempPanelAnimation.Height = _lengthAnimation;
                }
            }

            _tempPanelAnimation = null;

            this.Focus();
        }


        void HideOverlayPanel(double length, bool left_right)
        {
            _leftRightAnimation = left_right;
            _tempPanelAnimation = _tempPane;
            if (Double.IsNaN(length))
                length = 200;
            _lengthAnimation = length;

            _animation = new DoubleAnimation();
            _animation.From = length;
            _animation.To = 0.0;
            _animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            _animation.Completed += new EventHandler(HideOverlayPanel_Completed);

            if (left_right)
                _tempPanelAnimation.BeginAnimation(Control.WidthProperty, _animation);
            else
                _tempPanelAnimation.BeginAnimation(Control.HeightProperty, _animation);

            DoubleAnimation anOpacity = new DoubleAnimation();
            anOpacity.From = 1.0;
            anOpacity.To = 0.0;
            anOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, anOpacity);
        }


        void HideOverlayPanel_Completed(object sender, EventArgs e)
        {
            ForceHideOverlayPanel();
            try
            {
                if (PART_panelFront != null)
                    PART_panelFront.Children.Clear();
            }
            catch (InvalidOperationException ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDockingManager", "HideOverlayPanel_Completed", msg);
            }
            //FocusManager.SetIsFocusScope(this, true);
            this.Focus();
        }


        void ForceHideOverlayPanel()
        {
            if (_tempPanelAnimation != null)
            {
                _animation.Completed -= new EventHandler(HideOverlayPanel_Completed);
                _animation.Completed -= new EventHandler(ShowOverlayPanel_Completed);
                if (_leftRightAnimation)
                {
                    _tempPanelAnimation.BeginAnimation(Control.WidthProperty, null);
                    _tempPanelAnimation.Width = 0;
                }
                else
                {
                    _tempPanelAnimation.BeginAnimation(Control.HeightProperty, null);
                    _tempPanelAnimation.Height = 0;
                }

            }
        }
        #endregion

        #endregion

        #region DragDrop Operations

        private Window _ParentWindow = null;
        public Window ParentWindow
        {
            get
            {
                if (_ParentWindow == null)
                {
                    AvaloniaObject rootObject = gip.core.layoutengine.avui.ControlManager.GetHighestControlInLogicalTree(this);
                    if (rootObject is Window)
                        _ParentWindow = rootObject as Window;
                }
                return _ParentWindow;
            }
            set
            {
                _ParentWindow = value;
            }
        }


        internal void MoveTo(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel, Dock relativeDock)
        {
            PART_gridDocking.MoveTo(sourcePanel, destinationPanel, relativeDock);
        }


        internal void MoveInto(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel)
        {
            PART_gridDocking.MoveInto(sourcePanel, destinationPanel);
        }


        public bool Drag(VBWindowDockingUndocked floatingWindow, Point point, Point offset)
        {
            if (!Focusable)
            {
                e.Pointer.Capture(this);
                {
                    if (ParentWindow is VBDockingContainerToolWindow)
                    {
                        VBDockingContainerToolWindow dockContainerToolW = ParentWindow as VBDockingContainerToolWindow;
                        if (dockContainerToolW.VBDockingPanel is VBDockingPanelTabbedDoc)
                            floatingWindow.SetOwner((dockContainerToolW.VBDockingPanel as VBDockingPanelTabbedDoc).DockManager.ParentWindow);
                        else
                            floatingWindow.SetOwner(ParentWindow);
                    }
                    else
                        floatingWindow.SetOwner(ParentWindow);
                    DragPanelServices.StartDrag(floatingWindow, point, offset);

                    return true;
                }
            }

            return false;
        }


        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (Focusable)
                DragPanelServices.MoveDrag(this.PointToScreen(e.GetPosition(this)));
            base.OnPreviewMouseMove(e);
        }


        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (Focusable)
            {
                DragPanelServices.EndDrag(this.PointToScreen(e.GetPosition(this)));
                if (e.Pointer.Captured == this)
                    e.Pointer.Capture(null);
            }
            base.OnPreviewMouseUp(e);
        }

        VBDockDragPanelServices _dragPanelServices;

        internal VBDockDragPanelServices DragPanelServices
        {
            get
            {
                if (_dragPanelServices == null)
                    _dragPanelServices = new VBDockDragPanelServices(this);

                return _dragPanelServices;
            }
        }
        #endregion

        #region IDropSurface


        public Rect SurfaceRectangle
        {
            get
            { 
                return new Rect(this.PointToScreen(new Point(0, 0)).ToPoint(1), new Size(this.Bounds.Width, this.Bounds.Height));
            }
        }


        VBDockingOverlayWindow _overlayWindow;
        internal VBDockingOverlayWindow OverlayWindow
        {
            get
            {
                return _overlayWindow;
            }
        }


        public void OnDockDragEnter(Point point)
        {
            OverlayWindow.Position = new PixelPoint(this.PointToScreen(new Point(0, 0)).X, this.PointToScreen(new Point(0, 0)).Y);
            OverlayWindow.Width = this.Bounds.Width;
            OverlayWindow.Height = this.Bounds.Height;
            OverlayWindow.Show(DragPanelServices.FloatingWindow);
        }


        public void OnDockDragOver(Point point)
        {

        }


        public void OnDockDragLeave(Point point)
        {
            //_overlayWindow.Owner = null;
            _overlayWindow.Hide();
            if (ParentWindow != null)
                ParentWindow.Activate();
            if (!IsFocused)
                this.Focus();
        }


        public bool OnDockDrop(Point point)
        {
            return false;
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
        void _PART_gridDocking_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!FreezeActive || (ContextACObject == null))
                return;
            if (!(e.Source is VBDockingGrid))
                return;
            if ((e.Source as VBDockingGrid).vbDockingPanelTabbedDoc.DockManager != this)
                return;
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

        private void PersistDockStateToDesignList()
        {
            PART_gridDocking.PersistStateToVBDesignContent();
            foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            {
                toolWin.PersistStateToVBDesignContent();
            }
            foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            {
                tabbedDoc.PersistStateToVBDesignContent();
            }
        }

        public string SerializeVBDesignList()
        {
            if (VBDesignList == null)
                return "";

            string xaml = "";

            KeyValuePair<string, ACxmlnsInfo> nsThis = Layoutgenerator.GetNamespaceInfo(this);
            if (nsThis.Value == null)
                return "";


            PersistDockStateToDesignList();
            string thisTypeName = this.GetType().Name;

            #region LINQ to XML
            XNamespace xNsThis = nsThis.Value.XMLNameSpace;
            XNamespace xNsX = ACxmlnsResolver.xNamespaceWPF;
            XDocument xDoc = new XDocument();
            XElement xElementRoot = new XElement(xNsThis + thisTypeName);
            foreach (KeyValuePair<string, ACxmlnsInfo> kvp in ACxmlnsResolver.NamespacesDict)
            {
                string key = kvp.Key.Trim();
                if (!String.IsNullOrEmpty(key))
                    xElementRoot.Add(new XAttribute(XNamespace.Xmlns + key, kvp.Value.XMLNameSpace));
            }
            xElementRoot.Add(new XAttribute(xNsX + "Name", this.Name));
            xDoc.Add(xElementRoot);

            foreach (Control uiElement in VBDesignList)
            {
                KeyValuePair<string, ACxmlnsInfo> nsControl = Layoutgenerator.GetNamespaceInfo(uiElement);
                if (nsControl.Value == null)
                    continue;
                XNamespace xNsUI = nsControl.Value.XMLNameSpace;
                XElement xElement = new XElement(xNsUI + uiElement.GetType().Name,
                    new XAttribute(xNsThis + thisTypeName + ".Container", VBDockingManager.GetContainer(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockState", VBDockingManager.GetDockState(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockPosition", VBDockingManager.GetDockPosition(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".RibbonBarVisibility", VBDockingManager.GetRibbonBarVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".IsCloseableBSORoot", VBDockingManager.GetIsCloseableBSORoot(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".CloseButtonVisibility", VBDockingManager.GetCloseButtonVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DisableDockingOnClick", VBDockingManager.GetDisableDockingOnClick(uiElement).ToString())
                    );
                Size size = VBDockingManager.GetWindowSize(uiElement);
                xElement.Add(new XAttribute(xNsThis + thisTypeName + ".WindowSize", String.Format("{0},{1}", size.Width, size.Height)));
                if (uiElement is Control)
                    xElement.Add(new XAttribute(xNsX + "Name", (uiElement as Control).Name));
                if (uiElement is IVBSerialize)
                    (uiElement as IVBSerialize).AddSerializableAttributes(xElement);
                xElementRoot.Add(xElement);
            }
            VBDesign parentDesign = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBDesign)) as VBDesign;
            if (parentDesign != null)
            {
                parentDesign.UpdateDesignOfCurrentUser(xElementRoot, IsBSOManager);
            }
            xaml = xDoc.ToString();
            #endregion

            return xaml;
        }


        public string GetLayoutAsXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("DockingLibrary_Layout"));
            PART_gridDocking.Serialize(doc, doc.DocumentElement);
            return doc.OuterXml;
        }

        public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            PART_gridDocking.Deserialize(this, doc.ChildNodes[0], getContentHandler);

            List<VBDockingPanelBase> addedPanes = new List<VBDockingPanelBase>();
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList)
            {
                VBDockingPanelToolWindow pane = content.VBDockingPanel as VBDockingPanelToolWindow;
                if (pane != null && !addedPanes.Contains(pane))
                {
                    if (pane.State == VBDockingPanelState.AutoHide)
                    {
                        addedPanes.Add(pane);
                        HideDockingPanelToolWindow_AsDockingButton(pane);
                    }
                }
            }

            _currentButton = null;
        }
    }


}
