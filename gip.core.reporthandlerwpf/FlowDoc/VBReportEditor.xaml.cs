using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Reflection;
using System.Transactions;
using System.ComponentModel;
using gip.core.layoutengine;
using System.Windows.Xps.Packaging;
using System.Windows.Markup;
using gip.core.autocomponent;
using Document.Editor;

namespace gip.core.reporthandlerwpf.Flowdoc
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

        protected override void OnInitialized(EventArgs e)
        {
            Unloaded += new RoutedEventHandler(VBReportEditor_Unloaded);
            Loaded += new RoutedEventHandler(VBReportEditor_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(VBReportEditor_DataContextChanged);
            base.OnInitialized(e);
        }

        void VBReportEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DeInitVBControl();
        }

        protected bool _Loaded = false, _WrongXAML = false;
        protected void VBReportEditor_Loaded(object sender, RoutedEventArgs e)
        {
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

            if (Visibility == Visibility.Visible)
            {
                if (RightControlMode < Global.ControlModes.Disabled)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (VBXMLEditor != null)
                    {
                        VBXMLEditor.VBContent = this.VBContent;
                    }

                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = new PropertyPath(dcPath);
                    binding.NotifyOnSourceUpdated = true;
                    binding.NotifyOnTargetUpdated = true;
                    binding.Mode = BindingMode.TwoWay;

                    this.SetBinding(VBReportEditor.XMLTextProperty, binding);
                    this.TargetUpdated += OnTargetUpdatedOfBinding;
                    this.SourceUpdated += OnSourceUpdatedOfBinding;

                    if (!String.IsNullOrEmpty(VBReportData))
                    {
                        IACType dcACTypeInfo2 = null;
                        object dcSource2 = null;
                        string dcPath2 = "";
                        Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                        if (!ContextACObject.ACUrlBinding(VBReportData, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                        {
                            this.Root().Messages.Error(BSOACComponent, "Error00007", false, "VBReportEditor", VBReportData);
                            return;
                        }

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource2;
                        binding2.Path = new PropertyPath(dcPath2);
                        binding2.NotifyOnSourceUpdated = true;
                        binding2.NotifyOnTargetUpdated = true;
                        binding2.Mode = BindingMode.OneWay;
                        this.SetBinding(VBReportEditor.DesignerReportDataProperty, binding2);
                    }

                    //if (!String.IsNullOrEmpty(VBRefreshDesigner))
                    //{
                    //    IACType dcACTypeInfo2 = null;
                    //    object dcSource2 = null;
                    //    string dcPath2 = "";
                    //    Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                    //    if (!ContextACObject.ACUrlBinding(VBRefreshDesigner, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                    //    {
                    //        this.Root().Messages.Error(BSOACComponent, "Error00007", "VBReportEditor", VBDesignerReportData);
                    //        return;
                    //    }

                    //    Binding binding2 = new Binding();
                    //    binding2.Source = dcSource2;
                    //    binding2.Path = new PropertyPath(dcPath2);
                    //    binding2.NotifyOnSourceUpdated = true;
                    //    binding2.NotifyOnTargetUpdated = true;
                    //    binding2.Mode = BindingMode.OneWay;
                    //    this.SetBinding(VBReportEditor.RefreshDesignerProperty, binding2);
                    //}

                    Binding binding3 = new Binding();
                    binding3.Source = BSOACComponent;
                    binding3.Path = new PropertyPath(Const.ACUrlCmdMessage);
                    binding3.Mode = BindingMode.OneWay;
                    SetBinding(VBReportEditor.ACUrlCmdMessageProperty, binding3);


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
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            RefreshDesignerFromXAML();
        }

        public void OnTargetUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBReportEditor.XMLTextProperty)
            {
                RefreshDesignEditor();
            }
            else if (args.Property == VBReportEditor.DesignerReportDataProperty)
            {
            }
            //else if (args.Property == VBReportEditor.RefreshDesignerProperty)
            //{
            //    RefreshDesignEditor();
            //}
        }

        public void OnSourceUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBReportEditor.XMLTextProperty)
            {
            }
            else if (args.Property == VBReportEditor.DesignerReportDataProperty)
            {
            }
            //else if (args.Property == VBReportEditor.RefreshDesignerProperty)
            //{
            //    //RefreshDesignEditor();
            //}
        }

        public void ApplyConfig()
        {
            if (ucDesigner.Document.Resources.Contains("Config"))
                ucDesigner.Document.Resources["Config"] = ((VBBSOReport)BSOACComponent).CurrentReportConfiguration;
            else
                ucDesigner.Document.Resources.Add("Config", ((VBBSOReport)BSOACComponent).CurrentReportConfiguration);
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

                        using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                            Database.GlobalDatabase.ACSaveChanges();
                        ((VBBSOReport)BSOACComponent).FlowDocumentConfig = ucDesigner.Document;
                        ((VBBSOReport)BSOACComponent).LoadConfig();
                    }
                }
                else // XAMLTab
                {
                    if (_LastActiveTab == DesignTab)
                        SaveToXAML();
                    else if (_LastActiveTab == ConfigurationTab)
                    {
                        if (ucDesigner.Document.Resources.Contains("Config"))
                            ucDesigner.Document.Resources["Config"] = ((VBBSOReport)BSOACComponent).CurrentReportConfiguration;
                        else
                            ucDesigner.Document.Resources.Add("Config", ((VBBSOReport)BSOACComponent).CurrentReportConfiguration);
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
        void VBReportEditor_Unloaded(object sender, RoutedEventArgs e)
        {
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

            this.Loaded -= VBReportEditor_Loaded;
            this.Unloaded -= VBReportEditor_Unloaded;
            this.DataContextChanged -= VBReportEditor_DataContextChanged;

            if (!String.IsNullOrEmpty(VBReportData))
                BindingOperations.ClearBinding(this, VBReportEditor.DesignerReportDataProperty);
            //if (!String.IsNullOrEmpty(VBDesignerReportData))
            //    BindingOperations.ClearBinding(this, VBReportEditor.DesignerReportDataProperty);
            BindingOperations.ClearBinding(this, VBReportEditor.XMLTextProperty);

            if (bso == null && BSOACComponent != null)
            {
                bso = BSOACComponent;
            }
            if (bso != null && _BSOPropertyChangedSubscr)
            {
                bso.PropertyChanged -= BSOACComponent_PropertyChanged;
                _BSOPropertyChangedSubscr = false;
            }
            if (ReadLocalValue(BSOACComponentProperty) != DependencyProperty.UnsetValue)
                BSOACComponent = null;
            if (_LastElementACComponent != null && _LastElementACComponent.ReferencePoint != null)
                _LastElementACComponent.ReferencePoint.Remove(this);
            _LastElementACComponent = null;
            _VBContentPropertyInfo = null;

            _Loaded = false;

            if (_ReportDocument != null)
            {
                _ReportDocument.Dispose();
                _ReportDocument = null;
            }
        }

        FlowDocEditor UCDesigner
        {
            get
            {
                return ucDesigner;
            }
        }

        DocumentViewer UCDocumentViewer
        {
            get
            {
                return ucDocumentViewer;
            }
        }

        //VBXMLEditor _VBXMLEditor = null;
        VBXMLEditor VBXMLEditor
        {
            get
            {
                return ucXMLEditor;
                //if (_VBXMLEditor == null)
                //{
                //    _VBXMLEditor = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, "ucAvalonTextEditor") as VBXMLEditor;
                //}
                //return _VBXMLEditor;
            }
        }

        //public static readonly DependencyProperty PropertyGridViewProperty
        //    = DependencyProperty.Register("PropertyGridView", typeof(PropertyGridView), typeof(VBReportEditor));
        //public VBPropertyGridView PropertyGridView
        //{
        //    get
        //    {
        //        return (VBPropertyGridView)GetValue(PropertyGridViewProperty);
        //    }
        //    set
        //    {
        //        SetValue(PropertyGridViewProperty, value);
        //    }
        //}


        //public static readonly DependencyProperty DesignItemTreeViewProperty
        //    = DependencyProperty.Register("DesignItemTreeView", typeof(DesignItemTreeView), typeof(VBReportEditor));
        //public VBDesignItemTreeView DesignItemTreeView
        //{
        //    get
        //    {
        //        return (VBDesignItemTreeView)GetValue(DesignItemTreeViewProperty);
        //    }
        //    set
        //    {
        //        SetValue(DesignItemTreeViewProperty, value);
        //    }
        //}

        public static readonly DependencyProperty XMLTextProperty
            = DependencyProperty.Register("Text", typeof(string), typeof(VBReportEditor), new PropertyMetadata(new PropertyChangedCallback(XMLTextChanged)));

        public string XMLText
        {
            get
            {
                return (string)GetValue(XMLTextProperty);
            }
            set
            {
                SetValue(XMLTextProperty, value);
            }
        }

        private static void XMLTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty DesignerReportDataProperty
            = DependencyProperty.Register("DesignerReportData", typeof(ReportData), typeof(VBReportEditor));

        public ReportData DesignerReportData
        {
            get
            {
                return (ReportData)GetValue(DesignerReportDataProperty);
            }
            set
            {
                SetValue(DesignerReportDataProperty, value);
            }
        }

        /// <summary>
        /// Determines is the configuration tab visible or not.
        /// </summary>
        public bool HideConfigurationTab
        {
            get { return (bool)GetValue(HideConfigurationTabProperty); }
            set { SetValue(HideConfigurationTabProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for HideConfigurationTab.
        /// </summary>
        public static readonly DependencyProperty HideConfigurationTabProperty =
            DependencyProperty.Register("HideConfigurationTab", typeof(bool), typeof(VBReportEditor), new PropertyMetadata(false));



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

        private ReportDocument _ReportDocument = null;
        public void RefreshDesignerFromXAML()
        {
            if (!IsLoaded)
                return;

            string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.XMLText);
            if (DesignerReportData == null || (_ReportDocument != null && _ReportDocument.XamlData == newXMLText))
                return;

            if (_ReportDocument != null)
            {
                _ReportDocument.Dispose();
                _ReportDocument = null;
            }
            try
            {
                _ReportDocument = new ReportDocument(this.XMLText);
                if (_ReportDocument != null)
                {
                    UCDesigner.Document = _ReportDocument.CreateFlowDocument(true);
                }
            }
            catch (Exception)
            {
                UCDesigner.Document = null;
                _WrongXAML = true;
                // MessageBox.Show(e.Message, "Report Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void RefreshViewerFromXAML()
        {
            if (String.IsNullOrEmpty(this.XMLText) || !IsLoaded)
                return;

            string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.XMLText);
            if (DesignerReportData == null || _ReportDocument != null && _ReportDocument.XamlData == newXMLText)
                return;

            if (_ReportDocument != null)
            {
                _ReportDocument.Dispose();
                _ReportDocument = null;
            }
            try
            {
                _ReportDocument = new ReportDocument(this.XMLText);
                if (_ReportDocument != null)
                {
                    XpsDocument xps = _ReportDocument.CreateXpsDocument(DesignerReportData);
                    if (xps != null)
                        this.UCDocumentViewer.Document = xps.GetFixedDocumentSequence();
                    else
                        this.UCDocumentViewer.Document = null;
                }
            }
            catch (Exception e)
            {
                this.UCDocumentViewer.Document = null;
                _WrongXAML = true;
                MessageBox.Show(e.Message, "Report Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SaveToXAML()
        {
            if ((UCDesigner == null) || (UCDesigner.Document == null))
                return;
            if (_ReportDocument != null && !_WrongXAML)
            {
                string newXaml = _ReportDocument.UpdateXAMLDataFromChangedFlowDoc(UCDesigner.Document);
                if (newXaml != XMLText)
                    XMLText = newXaml;
            }
            _WrongXAML = false;
        }

        #region IDataField Members

        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBReportEditor));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public static readonly DependencyProperty VBReportDataProperty
            = DependencyProperty.Register("VBReportData", typeof(string), typeof(VBReportEditor));

        public string VBReportData
        {
            get { return (string)GetValue(VBReportDataProperty); }
            set { SetValue(VBReportDataProperty, value); }
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBReportEditor));
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

        //public string ToolTip
        //{
        //    get
        //    {
        //        return ucAvalonTextEditor.ToolTip as string;
        //    }
        //    set
        //    {
        //        ucAvalonTextEditor.ToolTip = value;
        //    }
        //}

        private bool Visible
        {
            get
            {
                return Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Visibility = Visibility.Hidden;
                }
            }
        }

        private bool Enabled
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject == null)
                    {
                        IsEnabled = true;
                    }
                    else
                    {
                        IsEnabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                }
                else
                {
                    IsEnabled = false;
                }
                ControlModeChanged();
            }
        }

        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            Enabled = controlMode >= Global.ControlModes.Enabled;
            Visible = controlMode >= Global.ControlModes.Disabled;
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
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
            if (Enabled)
            {
                if (VBContentPropertyInfo != null)
                {
                    if (VBContentPropertyInfo.IsNullable)
                    {
                        ControlMode = Global.ControlModes.Enabled;
                    }
                    else
                    {
                        ControlMode = Global.ControlModes.EnabledRequired;
                    }
                }
                else
                    ControlMode = Global.ControlModes.Disabled;
            }
            else
            {
                ControlMode = Global.ControlModes.Disabled;
            }
        }

        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBReportEditor));

        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }


        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBReportEditor));
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
            get { return (string)GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
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

        public static readonly DependencyProperty ACUrlCmdMessageProperty =
                                   DependencyProperty.Register("ACUrlCmdMessage",
                                   typeof(ACUrlCmdMessage), typeof(VBReportEditor), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBReportEditor thisControl = dependencyObject as VBReportEditor;
            if (thisControl == null)
                return;
            if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
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
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
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
        #endregion

        #region IACMenuBuilder Member
        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            return acMenuItemList;
        }
        #endregion

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
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

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
        }
    }

}
