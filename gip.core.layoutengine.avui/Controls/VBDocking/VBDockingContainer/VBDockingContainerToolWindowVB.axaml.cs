using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using System;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a container for tool window in docking container. Use with <see cref="VBDockingManager"/>.
    /// </summary>
    partial class VBDockingContainerToolWindowVB : VBDockingContainerToolWindow, IVBDialog
    {
        #region c'tors
        public VBDockingContainerToolWindowVB()
        {
        }

        public VBDockingContainerToolWindowVB(VBDockingManager dockManager, Control vbDesignContent)
            : base(dockManager, vbDesignContent)
        {
            InitializeComponent();
            OnToolWindowLoaded();
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            base.DeInitVBControl(bso);

            if (_RootPanelDialog != null)
            {
                _RootPanelDialog.Children.Clear();
            }
            _RootPanelDialog = null;
        }

        #endregion

        #region Init and Load
        void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnToolWindowLoaded();
        }

        bool IsCreated = false;
        void OnToolWindowLoaded()
        {
            if (IsCreated
                || (ContextACObject == null)
                || (VBDesignContent == null))
                return;

            VBDockPanel dockPanel = new VBDockPanel();
            GenerateContentLayout(dockPanel);
            _RootPanelDialog.Children.Add(dockPanel);

            RefreshTitle();
            IsCreated = true;
        }

        private string GetCustomizedACCaption()
        {
            string customizedCaption = null;
            if (VBDesignContent != null && VBDesignContent is VBDesign && !string.IsNullOrEmpty((VBDesignContent as VBDesign).CustomizedACCaption))
            {
                customizedCaption = (VBDesignContent as VBDesign).CustomizedACCaption;
            }
            return customizedCaption;
        }

        public override void RefreshTitle()
        {
            if (VBContent == null)
            {
                if (ContextACObject != null)
                {
                    string customizedCaption = GetCustomizedACCaption();
                    if (!string.IsNullOrEmpty(customizedCaption))
                    {
                        Title = customizedCaption;
                    }
                    else
                    {
                        Title = ContextACObject.ACCaption;
                    }
                    if (ContextACObject is INotifyPropertyChanged)
                    {
                        (ContextACObject as INotifyPropertyChanged).PropertyChanged -= VBDockingContainerToolWindowVB_PropertyChanged;
                        (ContextACObject as INotifyPropertyChanged).PropertyChanged += VBDockingContainerToolWindowVB_PropertyChanged;
                    }
                }
                else
                    base.RefreshTitle();
            }
            else
            {
                base.RefreshTitle();
            }
        }

        public override void ReInitDataContext()
        {
            if (VBDockingPanel != null)
                VBDockingPanel.Show(this);
            if (_RootPanelDialog.Children != null && _RootPanelDialog.Children.Count > 0)
            {
                VBDockPanel dockPanel = _RootPanelDialog.Children[0] as VBDockPanel;
                if (dockPanel != null)
                    dockPanel.DataContext = ContextACObject;
            }
            RefreshTitle();
        }

        void VBDockingContainerToolWindowVB_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Const.ACCaptionPrefix)
            {
                IACObject contextObj = sender as IACObject;
                if (VBDesignContent != null)
                {
                    string customizedCaption = GetCustomizedACCaption();
                    if (!string.IsNullOrEmpty(customizedCaption))
                    {
                        Title = customizedCaption;
                    }
                    else
                    {
                        Title = contextObj.ACCaption;
                    }
                }
            }
        }

        public void OnTabItemMouseDown(object sender, PointerPressedEventArgs e)
        {
            if ((_VBRibbon != null) && (e.Source is Button))
            {
                Button button = (Button)e.Source;
                if (button.Name == "PART_RibbonSwitchButton")
                {
                    if (!_VBRibbon.IsVisible)
                    {
                        _VBRibbon.IsVisible = true;
                        // Call SetRibbonBarVisibility for persistance of user-Design
                        if (VBDesignContent != null)
                            VBDockingManager.SetRibbonBarVisibility(VBDesignContent, Global.ControlModes.Enabled);
                    }
                    else
                    {
                        _VBRibbon.IsVisible = false;
                        // Call SetRibbonBarVisibility for persistance of user-Design
                        if (VBDesignContent != null)
                            VBDockingManager.SetRibbonBarVisibility(VBDesignContent, Global.ControlModes.Collapsed);
                    }
                }
                else if ((button.Name == "PART_CloseButton") && (VBDesignContent != null))
                {
                    if (BSOACComponent != null && !BSOACComponent.ACSaveOrUndoChanges())
                        return;
                    if (VBDockingManager.GetIsCloseableBSORoot(VBDesignContent))
                    {
                        if (DockManager != null)
                            DockManager.RemoveDockingContainerToolWindowTabbed(this);
                        OnCloseWindow();
                    }
                }
            }
        }
        #endregion

        #region IDialog Members
        public void CloseDialog()
        {
            try
            {
                this.Close();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDockingContainerToolWindowVB", "CloseDialog", msg);
            }
        }
        #endregion

        #region IACObject Member
        /// <summary>
        /// Gets the ACType.
        /// </summary>
        new public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

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
        public override object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            switch (acUrl)
            {
                case Const.CmdClose:
                    if (!VBDockingPanel.CloseWindow())
                    {
                        OnCloseWindow();
                    }
                    break;
            }
            return null;
        }
        #endregion
    }
}
