// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using Document.Editor;

namespace gip.core.reporthandler.avui
{
    /// <summary>
    /// Editor zum bearbeiten von XAML
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBReportEditor'}de{'VBReportEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBReportEditor : UserControl, IVBContent, IACMenuBuilder, IACObject
    {
        public VBReportEditor()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            DataContextChanged += VBReportEditor_DataContextChanged;
            base.OnInitialized();
        }

        private void VBReportEditor_DataContextChanged(object sender, EventArgs e)
        {
            DeInitVBControl();
        }

        protected bool _Loaded = false, _WrongXAML = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (_Loaded)
            {
                RefreshDesignEditor();
                return;
            }

            if (!(ContextACObject is IACComponent) || String.IsNullOrEmpty(VBContent))
                return;

            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent.ReferencePoint != null)
                elementACComponent.ReferencePoint.Add(this);
            _LastElementACComponent = elementACComponent;

            _Loaded = true;
            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBReportEditor", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (IsVisible)
            {
                if (RightControlMode < Global.ControlModes.Disabled)
                {
                    IsVisible = false;
                }
                else
                {
                    if (VBXMLEditor != null)
                    {
                        VBXMLEditor.VBContent = this.VBContent;
                    }

                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = dcPath;
                    binding.Mode = BindingMode.TwoWay;

                    this.Bind(VBReportEditor.XMLTextProperty, binding);

                    if (!String.IsNullOrEmpty(VBReportData))
                    {
                        IACType dcACTypeInfo2 = null;
                        object dcSource2 = null;
                        string dcPath2 = "";
                        Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                        if (!ContextACObject.ACUrlBinding(VBReportData, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                        {
                            this.Root().Messages.ErrorAsync(BSOACComponent, "Error00007", false, "VBReportEditor", VBReportData);
                            return;
                        }

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource2;
                        binding2.Path = dcPath2;
                        binding2.Mode = BindingMode.OneWay;
                        this.Bind(VBReportEditor.DesignerReportDataProperty, binding2);
                    }

                    Binding binding3 = new Binding();
                    binding3.Source = BSOACComponent;
                    binding3.Path = Const.ACUrlCmdMessage;
                    binding3.Mode = BindingMode.OneWay;
                    Bind(VBReportEditor.ACUrlCmdMessageProperty, binding3);


                }
            }

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                {
                    IsEnabled = false;
                }
                else
                {
                    UpdateControlMode();
                }
            }
            if (AutoFocus)
            {
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            RefreshDesignerFromXAML();
        }

        //public void OnTargetUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        //{
        //    if (args.Property == VBReportEditor.XMLTextProperty)
        //    {
        //        RefreshDesignEditor();
        //    }
        //    else if (args.Property == VBReportEditor.DesignerReportDataProperty)
        //    {
        //    }
        //}

        //public void OnSourceUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        //{
        //    if (args.Property == VBReportEditor.XMLTextProperty)
        //    {
        //    }
        //    else if (args.Property == VBReportEditor.DesignerReportDataProperty)
        //    {
        //    }
        //}

        public void ApplyConfig()
        {
            //if (ucDesigner.Document.Resources.Contains("Config"))
            //    ucDesigner.Document.Resources["Config"] = ((VBBSOReport)BSOACComponent).CurrentReportConfiguration;
            //else
            //    ucDesigner.Document.Resources.Add("Config", ((VBBSOReport)BSOACComponent).CurrentReportConfiguration);
            SaveToXAML();
        }

        VBTabItem _LastActiveTab = null;
        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (sender != this)
            //return;
            if (e.AddedItems.Count > 0)
            {
                VBTabItem vbTabitemAdded = e.AddedItems[0] as VBTabItem;
                if (vbTabitemAdded == null || _LastActiveTab == vbTabitemAdded)
                    return;
                if (vbTabitemAdded == DesignTab)
                {
                    RefreshDesignerFromXAML();
                }
                else if (vbTabitemAdded == PreviewTab)
                {
                    if (_LastActiveTab == DesignTab)
                        SaveToXAML();
                    RefreshViewerFromXAML();
                }
                else if (vbTabitemAdded == ConfigurationTab)
                {
                    if (ConfigurationTab.BSOACComponent != null && BSOACComponent is VBBSOReport)
                    {
                        //using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                        //    Database.GlobalDatabase.ACSaveChanges();
                        //((VBBSOReport)BSOACComponent).FlowDocumentConfig = ucDesigner.Document;
                        //((VBBSOReport)BSOACComponent).LoadConfig();
                    }
                }
                else // XAMLTab
                {
                    if (_LastActiveTab == DesignTab)
                        SaveToXAML();
                    else if (_LastActiveTab == ConfigurationTab)
                    {
                        //if (ucDesigner.Document.Resources.Contains("Config"))
                        //    ucDesigner.Document.Resources["Config"] = ((VBBSOReport)BSOACComponent).CurrentReportConfiguration;
                        //else
                        //    ucDesigner.Document.Resources.Add("Config", ((VBBSOReport)BSOACComponent).CurrentReportConfiguration);
                        SaveToXAML();
                    }
                }
                _LastActiveTab = vbTabitemAdded;
            }
            //else
            //_LastActiveTab = null;
        }

        bool _BSOPropertyChangedSubscr = false;
        void BSOACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InitState")
            {
                IACComponent bso = sender as IACComponent;
                if ((bso != null)
                    && (bso.InitState == ACInitState.Destructed || bso.InitState == ACInitState.DisposedToPool))
                    DeInitVBControl(true, bso);
            }
        }

        private IACComponent _LastElementACComponent = null;
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            if (ContextACObject is IACComponent)
                _LastElementACComponent = ContextACObject as IACComponent;
            DeInitVBControl();
        }

        internal virtual void DeInitVBControl(bool force = false, IACComponent bso = null)
        {
            if (!_Loaded)
                return;
            VBDesign parent = this.GetVBDesign();
            if (parent != null && DataContext != null && !force)
                return;

            this.DataContextChanged -= VBReportEditor_DataContextChanged;

            if (!String.IsNullOrEmpty(VBReportData))
                this.ClearBinding(VBReportEditor.DesignerReportDataProperty);
            //if (!String.IsNullOrEmpty(VBDesignerReportData))
            //    BindingOperations.ClearBinding(this, VBReportEditor.DesignerReportDataProperty);
            this.ClearBinding(VBReportEditor.XMLTextProperty);

            if (bso == null && BSOACComponent != null)
            {
                bso = BSOACComponent;
            }
            if (bso != null && _BSOPropertyChangedSubscr)
            {
                bso.PropertyChanged -= BSOACComponent_PropertyChanged;
                _BSOPropertyChangedSubscr = false;
            }
            if (IsSet(BSOACComponentProperty))
                BSOACComponent = null;
            if (_LastElementACComponent != null && _LastElementACComponent.ReferencePoint != null)
                _LastElementACComponent.ReferencePoint.Remove(this);
            _LastElementACComponent = null;
            _VBContentPropertyInfo = null;

            _Loaded = false;

            //if (_ReportDocument != null)
            //{
            //    _ReportDocument.Dispose();
            //    _ReportDocument = null;
            //}
        }

        //FlowDocEditor UCDesigner
        //{
        //    get
        //    {
        //        return ucDesigner;
        //    }
        //}

        //DocumentViewer UCDocumentViewer
        //{
        //    get
        //    {
        //        return ucDocumentViewer;
        //    }
        //}

        VBXMLEditor VBXMLEditor
        {
            get
            {
                return ucXMLEditor;
            }
        }

        public static readonly StyledProperty<string> XMLTextProperty =
            AvaloniaProperty.Register<VBReportEditor, string>(nameof(XMLText));

        public string XMLText
        {
            get => GetValue(XMLTextProperty);
            set => SetValue(XMLTextProperty, value);
        }

        public static readonly StyledProperty<ReportData> DesignerReportDataProperty =
            AvaloniaProperty.Register<VBReportEditor, ReportData>(nameof(DesignerReportData));

        public ReportData DesignerReportData
        {
            get => GetValue(DesignerReportDataProperty);
            set => SetValue(DesignerReportDataProperty, value);
        }

        /// <summary>
        /// Determines is the configuration tab visible or not.
        /// </summary>
        public bool HideConfigurationTab
        {
            get => GetValue(HideConfigurationTabProperty);
            set => SetValue(HideConfigurationTabProperty, value);
        }

        /// <summary>
        /// Represents the dependency property for HideConfigurationTab.
        /// </summary>
        public static readonly StyledProperty<bool> HideConfigurationTabProperty =
            AvaloniaProperty.Register<VBReportEditor, bool>(nameof(HideConfigurationTab), defaultValue: false);



        //public static readonly DependencyProperty RefreshDesignerProperty
        //    = DependencyProperty.Register("RefreshDesigner", typeof(int), typeof(VBReportEditor));

        //public int RefreshDesigner
        //{
        //    get
        //    {
        //        return (int)GetValue(RefreshDesignerProperty);
        //    }
        //    set
        //    {
        //        SetValue(RefreshDesignerProperty, value);
        //    }
        //}

        public void RefreshDesignEditor()
        {
            VBTabItem vbTabitemAdded = this.ucTabControl.SelectedItem as VBTabItem;
            if (vbTabitemAdded == DesignTab)
                RefreshDesignerFromXAML();
        }

        //private ReportDocument _ReportDocument = null;
        public void RefreshDesignerFromXAML()
        {
            if (!IsLoaded)
                return;

            //string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.XMLText);
            //if (DesignerReportData == null || (_ReportDocument != null && _ReportDocument.XamlData == newXMLText))
            //    return;

            //if (_ReportDocument != null)
            //{
            //    _ReportDocument.Dispose();
            //    _ReportDocument = null;
            //}
            //try
            //{
            //    _ReportDocument = new ReportDocument(this.XMLText);
            //    if (_ReportDocument != null)
            //    {
            //        //UCDesigner.Document = _ReportDocument.CreateFlowDocument(true);
            //    }
            //}
            //catch (Exception)
            //{
            //    //UCDesigner.Document = null;
            //    _WrongXAML = true;
            //}
        }

        public void RefreshViewerFromXAML()
        {
            //if (String.IsNullOrEmpty(this.XMLText) || !IsLoaded)
            //    return;

            //string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.XMLText);
            //if (DesignerReportData == null || _ReportDocument != null && _ReportDocument.XamlData == newXMLText)
            //    return;

            //if (_ReportDocument != null)
            //{
            //    _ReportDocument.Dispose();
            //    _ReportDocument = null;
            //}
            //try
            //{
            //    _ReportDocument = new ReportDocument(this.XMLText);
            //    if (_ReportDocument != null)
            //    {
            //        XpsDocument xps = _ReportDocument.CreateXpsDocument(DesignerReportData);
            //        if (xps != null)
            //            this.UCDocumentViewer.Document = xps.GetFixedDocumentSequence();
            //        else
            //            this.UCDocumentViewer.Document = null;
            //    }
            //}
            //catch (Exception)
            //{
            //    this.UCDocumentViewer.Document = null;
            //    _WrongXAML = true;
            //    MessageBox.Show(e.Message, "Report Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        public void SaveToXAML()
        {
            throw new NotImplementedException();
            //if ((UCDesigner == null) || (UCDesigner.Document == null))
            //    return;
            //if (_ReportDocument != null && !_WrongXAML)
            //{
            //    string newXaml = _ReportDocument.UpdateXAMLDataFromChangedFlowDoc(UCDesigner.Document);
            //    if (newXaml != XMLText)
            //        XMLText = newXaml;
            //}
            //_WrongXAML = false;
        }

        #region IDataField Members

        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBReportEditor, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
        }

        public static readonly StyledProperty<string> VBReportDataProperty =
            AvaloniaProperty.Register<VBReportEditor, string>(nameof(VBReportData));

        public string VBReportData
        {
            get => GetValue(VBReportDataProperty);
            set => SetValue(VBReportDataProperty, value);
        }


        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get; set;
        }
        #endregion

        #region IDataContent Members
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBReportEditor>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
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

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                    }
                    break;
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            return this.ReflectIsEnabledACUrlCommand(acCommand.GetACUrl(), null);
                        }
                    }
                    break;
            }
            return false;
        }

        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            //Enabled = controlMode >= Global.ControlModes.Enabled;
            //Visible = controlMode >= Global.ControlModes.Disabled;
        }


        public bool AutoFocus { get; set; }

        IACType _VBContentPropertyInfo = null;
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentPropertyInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
            //if (Enabled)
            //{
            //    if (VBContentPropertyInfo != null)
            //    {
            //        if (VBContentPropertyInfo.IsNullable)
            //        {
            //            ControlMode = Global.ControlModes.Enabled;
            //        }
            //        else
            //        {
            //            ControlMode = Global.ControlModes.EnabledRequired;
            //        }
            //    }
            //    else
            //        ControlMode = Global.ControlModes.Disabled;
            //}
            //else
            //{
            //    ControlMode = Global.ControlModes.Disabled;
            //}
        }

        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBReportEditor, Global.ControlModes>(nameof(ControlMode));

        public Global.ControlModes ControlMode
        {
            get => GetValue(ControlModeProperty);
            set => SetValue(ControlModeProperty, value);
        }


        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBReportEditor, string>(nameof(DisabledModes));
        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get => GetValue(DisabledModesProperty);
            set => SetValue(DisabledModesProperty, value);
        }

        public IACComponentDesignManager GetDesignManager(bool create = false)
        {
            VBDesign vbDesign = this.GetVBDesign();
            if (BSOACComponent is IACComponentDesignManager)
                return BSOACComponent as IACComponentDesignManager;
            else
            {
                foreach (IACComponent child in BSOACComponent.ACComponentChilds)
                {
                    if (child is IACComponentDesignManager)
                    {
                        return child as IACComponentDesignManager;
                    }
                }
            }
            if (vbDesign == null)
                return null;
            return vbDesign.GetDesignManager(create);
        }
        #endregion

        #region IVBContent

        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBReportEditor, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get => GetValue(ACUrlCmdMessageProperty);
            set => SetValue(ACUrlCmdMessageProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            else if (change.Property == ControlModeProperty)
            {
                if (String.IsNullOrEmpty(VBContent) && ContextACObject == null)
                    UpdateControlMode();
            }
            else if (change.Property == ACUrlCmdMessageProperty)
            {
                OnACUrlMessageReceived();
            }
        }

        public void OnACUrlMessageReceived()
        {
            if (!this.IsLoaded)
                return;
            var acUrlMessage = ACUrlCmdMessage;
            if (acUrlMessage == null
                || acUrlMessage.ACParameter == null
                || !acUrlMessage.ACParameter.Any()
                || !(acUrlMessage.ACParameter[0] is IACComponent)
                || acUrlMessage.TargetVBContent != this.VBContent)
                return;
            switch (acUrlMessage.ACUrl)
            {
                case Const.CmdApplyConfig:
                    ApplyConfig();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region ControlMode
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }
        #endregion

        #region IACObject
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if ((acParameter != null) && (acParameter[0] != null))
            {
                switch (acUrl)
                {
                    case Const.EventDeInit:
                        if (ContextACObject is IACComponent && ContextACObject == acParameter[0] && (ContextACObject as IACComponent).ReferencePoint != null)
                            (ContextACObject as IACComponent).ReferencePoint.Remove(this);
                        break;
                }
            }
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
        }

        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }
        #endregion

        #region IACMenuBuilder Member
        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            return acMenuItemList;
        }
        #endregion

        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public void DeInitVBControl(IACComponent bso)
        {
        }
    }

}
