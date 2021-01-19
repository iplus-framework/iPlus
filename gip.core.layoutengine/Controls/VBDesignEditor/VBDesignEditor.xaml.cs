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
using gip.ext.design;
using gip.ext.designer;
using gip.ext.designer.PropertyGrid;
using gip.ext.designer.Xaml;
using gip.ext.xamldom;
using gip.ext.designer.Services;
using System.Reflection;
using System.Transactions;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Editor for editing XAML.
    /// </summary>
    /// <summary>
    /// Editor zum bearbeiten von XAML.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignEditor'}de{'VBDesignEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBDesignEditor : UserControl, IVBContent, IACMenuBuilder, IACObject
    {
        public VBDesignEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            Unloaded += new RoutedEventHandler(VBDesignEditor_Unloaded);
            Loaded += new RoutedEventHandler(VBDesignEditor_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(VBDesignEditor_DataContextChanged);
            base.OnInitialized(e);
        }

        void VBDesignEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DeInitVBControl();
        }

        protected bool _Loaded = false;
        protected void VBDesignEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //DesignTab.PART_TabItemBorder.Visibility = System.Windows.Visibility.Collapsed;
            //XAMLTab.Visibility = System.Windows.Visibility.Hidden;

            if (_SelectionManager != null && _SelectionManager.ReferencePoint != null)
            {
                if (!_SelectionManager.ReferencePoint.Contains(this))
                    _SelectionManager.ReferencePoint.Add(this);
            }

            if (_Loaded)
                return;

            if (!(ContextACObject is IACComponent) || String.IsNullOrEmpty(VBContent))
                return;

            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent.ReferencePoint == null)
                return;
            elementACComponent.ReferencePoint.Add(this);
            _LastElementACComponent = elementACComponent;

            _Loaded = true;
            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBDesignEditor", VBContent);
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

                    var designManager = GetDesignManager(false);
                    if ((designManager == null) && (dcSource is IACComponentDesignManager))
                        designManager = dcSource as IACComponentDesignManager;
                    if (designManager != null)
                    {
                        if (!designManager.ShowXMLEditor)
                        {
                            foreach (var item in ucTabControl.Items)
                            {
                                if (item is VBTabItem)
                                {
                                    VBTabItem vbTabItem = item as VBTabItem;
                                    if (vbTabItem.Name == "XAMLTab")
                                    {
                                        ucTabControl.Items.Remove(vbTabItem);
                                        break;
                                    }
                                }
                            }
                        }
                        if (PropertyGridView == null)
                        {
                            if (designManager.PropertyWindowVisible)
                            {
                                designManager.ShowPropertyWindow();
                            }
                        }
                        else
                        {
                            if (PropertyGridView.BSOACComponent != designManager)
                                PropertyGridView.BSOACComponent = designManager;
                        }

                        if (DesignItemTreeView == null)
                        {
                            if (designManager.LogicalTreeWindowVisible)
                            {
                                designManager.ShowLogicalTreeWindow();
                            }
                        }
                        else
                        {
                            if (DesignItemTreeView.BSOACComponent != designManager)
                                DesignItemTreeView.BSOACComponent = designManager;
                        }

                        BSOACComponent = designManager;
                    }

                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = new PropertyPath(dcPath);
                    binding.NotifyOnSourceUpdated = true;
                    binding.NotifyOnTargetUpdated = true;
                    binding.Mode = BindingMode.TwoWay;

                    this.SetBinding(VBDesignEditor.XMLTextProperty, binding);
                    this.TargetUpdated += OnTargetUpdatedOfBinding;
                    this.SourceUpdated += OnSourceUpdatedOfBinding;

                    if (!String.IsNullOrEmpty(VBDesignerDataContext))
                    {
                        IACType dcACTypeInfo2 = null;
                        object dcSource2 = null;
                        string dcPath2 = "";
                        Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                        if (!ContextACObject.ACUrlBinding(VBDesignerDataContext, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                        {
                            this.Root().Messages.Error(BSOACComponent, "Error00007", false, "VBDesignEditor", VBDesignerDataContext);
                            return;
                        }

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource2;
                        binding2.Path = new PropertyPath(dcPath2);
                        binding2.NotifyOnSourceUpdated = true;
                        binding2.NotifyOnTargetUpdated = true;
                        binding2.Mode = BindingMode.OneWay;
                        this.SetBinding(VBDesignEditor.DesignerDataContextProperty, binding2);
                    }

                    if (!String.IsNullOrEmpty(VBRefreshDesigner))
                    {
                        IACType dcACTypeInfo2 = null;
                        object dcSource2 = null;
                        string dcPath2 = "";
                        Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                        if (!ContextACObject.ACUrlBinding(VBRefreshDesigner, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                        {
                            this.Root().Messages.Error(BSOACComponent, "Error00007", false, "VBDesignEditor", VBDesignerDataContext);
                            return;
                        }

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource2;
                        binding2.Path = new PropertyPath(dcPath2);
                        binding2.NotifyOnSourceUpdated = true;
                        binding2.NotifyOnTargetUpdated = true;
                        binding2.Mode = BindingMode.OneWay;
                        this.SetBinding(VBDesignEditor.RefreshDesignerProperty, binding2);
                    }

                    if (BSOACComponent != null)
                    {
                        binding = new Binding();
                        binding.Source = BSOACComponent;
                        binding.Path = new PropertyPath(Const.InitState);
                        binding.Mode = BindingMode.OneWay;
                        SetBinding(VBDesignEditor.ACCompInitStateProperty, binding);
                    }

                    //if (ContextACObject is IACComponent)
                    //{
                    //    Binding binding = new Binding();
                    //    binding.Source = ContextACObject;
                    //    binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                    //    binding.Mode = BindingMode.OneWay;
                    //    SetBinding(VBDesignEditor.ACUrlCmdMessageProperty, binding);
                    //}
                
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

            DesignSurface.OnDeleteItem += DesignSurface_OnDeleteItem;

            RefreshViewFromXAML();
        }

        void DesignSurface_OnDeleteItem(object sender, EventArgs e)
        {
            if (BSOACComponent != null && BSOACComponent is IACComponentDesignManager)
                if (!((IACComponentDesignManager)BSOACComponent).DeleteItem(DesignContext.Services.Selection.SelectedItems.FirstOrDefault()))
                    ModelTools.DeleteComponents(DesignContext.Services.Selection.SelectedItems);
        }

        public void OnTargetUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBDesignEditor.XMLTextProperty)
            {
                RefreshDesignEditor();
            }
            else if (args.Property == VBDesignEditor.DesignerDataContextProperty)
            {
            }
            else if (args.Property == VBDesignEditor.RefreshDesignerProperty)
            {
                RefreshDesignEditor();
            }
        }

        public void OnSourceUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBDesignEditor.XMLTextProperty)
            {
            }
            else if (args.Property == VBDesignEditor.DesignerDataContextProperty)
            {
            }
            else if (args.Property == VBDesignEditor.RefreshDesignerProperty)
            {
                //RefreshDesignEditor();
            }
        }


        VBTabItem _LastActiveTab = null;
        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                VBTabItem vbTabitemAdded = e.AddedItems[0] as VBTabItem;
                if (_LastActiveTab == vbTabitemAdded)
                    return;
                if (vbTabitemAdded == DesignTab)
                {
                    RefreshViewFromXAML();
                }
                else
                {
                    SaveToXAML();
                }
                _LastActiveTab = vbTabitemAdded;
            }
            else
                _LastActiveTab = null;
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        private IACComponent _LastElementACComponent = null;
        void VBDesignEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_SelectionManager != null && _SelectionManager.ReferencePoint != null)
                _SelectionManager.ReferencePoint.Remove(this);
            if (ContextACObject is IACComponent)
                _LastElementACComponent = ContextACObject as IACComponent;
            DeInitVBControl();
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
            DeInitVBControl(true,bso);
        }

        internal virtual void DeInitVBControl(bool force = false, IACComponent bso=null)
        {
            if (!_Loaded)
                return;
            VBDesign parent = this.GetVBDesign();
            if (parent != null && DataContext != null && !force)
                return;

            this.Loaded -= VBDesignEditor_Loaded;
            this.Unloaded -= VBDesignEditor_Unloaded;
            this.DataContextChanged -= VBDesignEditor_DataContextChanged;

            if (!String.IsNullOrEmpty(VBRefreshDesigner))
                BindingOperations.ClearBinding(this, VBDesignEditor.RefreshDesignerProperty);
            if (!String.IsNullOrEmpty(VBDesignerDataContext))
                BindingOperations.ClearBinding(this, VBDesignEditor.DesignerDataContextProperty);
            BindingOperations.ClearBinding(this, VBDesignEditor.XMLTextProperty);
            BindingOperations.ClearBinding(this, VBDesignEditor.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDesignEditor.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);

            if (ReadLocalValue(BSOACComponentProperty) != DependencyProperty.UnsetValue)
                BSOACComponent = null;
            if (_LastElementACComponent != null && _LastElementACComponent.ReferencePoint != null)
                _LastElementACComponent.ReferencePoint.Remove(this);
            _LastElementACComponent = null;
            _VBContentPropertyInfo = null;
            
            DesignSurface.OnDeleteItem -= DesignSurface_OnDeleteItem;

            _Loaded = false;
        }

        
        DesignSurface _DesignSurface = null;
        public DesignSurface DesignSurface
        {
            get 
            { 
                if (_DesignSurface == null)
                {
                    _DesignSurface = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, "ucWpfDesignSurface") as DesignSurface;
                }
                return _DesignSurface; 
            }
        }

        public DesignContext DesignContext
        {
            get { return DesignSurface.DesignContext; }
        }


        VBXMLEditor _VBXMLEditor = null;
        VBXMLEditor VBXMLEditor 
        {
            get
            {
                if (_VBXMLEditor == null)
                {
                    _VBXMLEditor = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, "ucAvalonTextEditor") as VBXMLEditor;
                }
                return _VBXMLEditor;
            }
        }

        public static readonly DependencyProperty PropertyGridViewProperty
            = DependencyProperty.Register("PropertyGridView", typeof(PropertyGridView), typeof(VBDesignEditor));
        public VBPropertyGridView PropertyGridView
        {
            get
            {
                return (VBPropertyGridView)GetValue(PropertyGridViewProperty);
            }
            set
            {
                SetValue(PropertyGridViewProperty, value);
            }
        }


        public static readonly DependencyProperty DesignItemTreeViewProperty
            = DependencyProperty.Register("DesignItemTreeView", typeof(DesignItemTreeView), typeof(VBDesignEditor));
        public VBDesignItemTreeView DesignItemTreeView
        {
            get
            {
                return (VBDesignItemTreeView)GetValue(DesignItemTreeViewProperty);
            }
            set
            {
                SetValue(DesignItemTreeViewProperty, value);
            }
        }

        public static readonly DependencyProperty XMLTextProperty
            = DependencyProperty.Register("Text", typeof(string), typeof(VBDesignEditor), new PropertyMetadata(new PropertyChangedCallback(XMLTextChanged)));

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


        public static readonly DependencyProperty DesignerDataContextProperty
            = DependencyProperty.Register("DesignerDataContext", typeof(IACObject), typeof(VBDesignEditor));

        public IACObject DesignerDataContext
        {
            get
            {
                return (IACObject)GetValue(DesignerDataContextProperty);
            }
            set
            {
                SetValue(DesignerDataContextProperty, value);
            }
        }


        public static readonly DependencyProperty RefreshDesignerProperty
            = DependencyProperty.Register("RefreshDesigner", typeof(int), typeof(VBDesignEditor));

        public int RefreshDesigner
        {
            get
            {
                return (int)GetValue(RefreshDesignerProperty);
            }
            set
            {
                SetValue(RefreshDesignerProperty, value);
            }
        }

        public void RefreshDesignEditor()
        {
            VBTabItem vbTabitemAdded = this.ucTabControl.SelectedItem as VBTabItem;
            if (vbTabitemAdded == DesignTab)
                RefreshViewFromXAML();
        }


        private string _LastLoadedXAMLInView;
        public void RefreshViewFromXAML()
        {
            if (String.IsNullOrEmpty(this.XMLText) || !IsLoaded)
                return;
            object newContext = null;
            if (DesignerDataContext != null)
                newContext = DesignerDataContext;
            else
                newContext = this.DataContext;

            string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.XMLText);
            if ((_LastLoadedXAMLInView == newXMLText) && (DesignSurface.DataContext == newContext))
                return;
            using (StringReader sr = new StringReader(Layoutgenerator.CheckOrUpdateNamespaceInLayout(newXMLText)))
            using (XmlTextReader r = new XmlTextReader(sr))
            {
                _LastLoadedXAMLInView = newXMLText;
                XamlLoadSettings settings = new XamlLoadSettings();

                foreach (Assembly assembly in ACxmlnsResolver.Assemblies)
                {
                    settings.DesignerAssemblies.Add(assembly);
                    settings.TypeFinder.RegisterAssembly(assembly,true);
                }
                //foreach (KeyValuePair<String, ACxmlnsInfo> kvp in ACxmlnsResolver.NamespacesDict)
                //{
                //    if (kvp.Value.Assembly != null)
                //    {
                //        settings.TypeFinder.RegisterAssembly(kvp.Value.Assembly, kvp.Value.XMLNameSpace, kvp.Key);
                //    }
                //}
                //Type controlType = typeof(VBDesignEditor);
                //settings.DesignerAssemblies.Add(controlType.Assembly);
                //KeyValuePair<string, string> kvp = Layoutgenerator.GetNamespaceInfo(controlType);
                //if (String.IsNullOrEmpty(kvp.Key))
                //settings.TypeFinder.RegisterAssembly(controlType.Assembly);
                //else
                    //settings.TypeFinder.RegisterAssembly(controlType.Assembly, controlType.Namespace, kvp.Key);
                //foreach (Assembly assembly in DesignerAssemblies)
                //{
                //    settings.DesignerAssemblies.Add(assembly);
                //    settings.TypeFinder.RegisterAssembly(assembly);
                //}
                /*settings.CustomServiceRegisterFunctions.Add(
                    delegate(XamlDesignContext context)
                    {
                        //context.Services.AddService(typeof(IUriContext), new FileUriContext(this.PrimaryFile));
                        //context.Services.AddService(typeof(IPropertyDescriptionService), new PropertyDescriptionService(this.PrimaryFile));
                        //context.Services.AddService(typeof(IEventHandlerService), new CSharpEventHandlerService(this));
                        //context.Services.AddService(typeof(ITopLevelWindowService), new WpfAndWinFormsTopLevelWindowService());
                        //context.Services.AddService(typeof(ChooseClassServiceBase), new IdeChooseClassService());
                    });*/
                //settings.TypeFinder = MyTypeFinder.Create(this.PrimaryFile);
                try
                {
                    if (this.DataContext.GetType().Name == "BSOiPlusStudio")
                    {
                        if (DesignerDataContext != null)
                        {
                            if (DesignerDataContext.GetType().Name == ACClass.ClassName)
                            {
                                ACClass acClass = DesignerDataContext as ACClass;
                                switch (acClass.ACKind)
                                {
                                    case Global.ACKinds.TPAModule:
                                    case Global.ACKinds.TPAProcessModule:
                                    case Global.ACKinds.TPAProcessFunction:
                                    case Global.ACKinds.TPABGModule:
                                        DesignSurface.DataContext = DesignerDataContext;
                                        break;
                                    default:
                                        DesignSurface.DataContext = null;
                                        break;
                                }
                                if (acClass.ACIdentifier == "BSOVisualisationStudio")
                                {
                                    IACComponent component = this.DataContext as IACComponent;
                                    if (component != null)
                                    {
                                        ACClassDesign currentACClassDesign = component.ACUrlCommand("CurrentACClassDesign") as ACClassDesign;
                                        if (currentACClassDesign != null && currentACClassDesign.ACUsageIndex == (short)Global.ACUsages.DUVisualisation)
                                            DesignSurface.DataContext = ContextACObject;
                                    }
                                }
                            }
                            else
                            {
                                DesignSurface.DataContext = DesignerDataContext;
                            }
                        }
                        else
                            DesignSurface.DataContext = this.DataContext;
                    }
                    else
                    {
                        if (DesignerDataContext != null)
                            DesignSurface.DataContext = DesignerDataContext;
                        else
                            DesignSurface.DataContext = this.DataContext;
                    }
                    Layoutgenerator.CurrentDataContext = DesignSurface.DataContext as IACObject;
                    DesignSurface.LoadDesigner(r, settings);

                    //DesignSurface.ContextMenuOpening += (sender, e) => MenuService.ShowContextMenu(e.OriginalSource as UIElement, designer, "/AddIns/WpfDesign/Designer/ContextMenu");

                    /*if (outline != null && designer.DesignContext != null && designer.DesignContext.RootItem != null)
                    {
                        outline.Root = OutlineNode.Create(designer.DesignContext.RootItem);
                    }*/

                    if (PropertyGridView != null)
                        PropertyGridView.PropertyGrid.SelectedItems = null;
                    if (DesignItemTreeView != null)
                        DesignItemTreeView.RootItem =  DesignSurface.DesignContext.RootItem;
                    DesignSurface.DesignContext.Services.Selection.SelectionChanged += OnSelectionChanged;
                    DesignSurface.DesignContext.Services.GetService<UndoService>().UndoStackChanged += OnUndoStackChanged;

                    //DesignSurface.DesignContext.RootItem.View.UpdateLayout();
                    IACComponentDesignManager designManager = GetDesignManager();
                    if (designManager != null)
                        designManager.OnDesignerLoaded(this, false);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDesignEditor", "RefreshViewFromXAML", msg);

                    //this.UserContent = new WpfDocumentError();
                }
            }
        }

        private void RegisterAssemblies(XamlLoadSettings settings, Type controlType)
        {
        }

        public void SaveToXAML()
        {
            if ((DesignSurface.DesignContext != null) && ObjectsInDesignViewChanged)
            {
                if (DesignSurface.DesignContext.Services.Component.DesignItems != null)
                {
                    List<DesignItem> itemsToCheck = DesignSurface.DesignContext.Services.Component.DesignItems.Where(c => c.View != null && c.View is IVBContent && String.IsNullOrEmpty(c.Name)).ToList();
                    foreach (DesignItem itemToCheck in itemsToCheck)
                    {
                        String vbXName = (itemToCheck.View as IVBContent).GetVBContentAsXName();
                        if (!String.IsNullOrEmpty(vbXName))
                        {
                            vbXName = String.Format("{0}_{1}", vbXName, DesignSurface.DesignContext.Services.Component.DesignItems.Where(c => (c.Component != null) 
                                && (c.Component is FrameworkElement) 
                                && !String.IsNullOrEmpty(c.Name) 
                                && c.Name.StartsWith(vbXName)).Count());
                            itemToCheck.Name = vbXName;
                        }
                    }
                }

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                //settings.IndentChars = "\t";
                settings.NewLineOnAttributes = false;
                StringWriter stringWrite = new StringWriter();
                using (XmlWriter xmlWriter = XmlTextWriter.Create(stringWrite, settings))
                {
                    DesignSurface.SaveDesigner(xmlWriter);
                    XMLText = stringWrite.ToString();
                    //XMLText = stringWrite.ToString().FormatXML();
                    IACComponentDesignManager designManager = GetDesignManager();
                    if (designManager != null)
                        designManager.OnDesignerLoaded(this, true);
                }
                ObjectsInDesignViewChanged = false;
            }
            else
            {
                /*if (_stream.CanRead)
                {
                    _stream.Position = 0;
                    using (var reader = new StreamReader(_stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(reader.ReadToEnd());
                    }
                }*/
            }

        }

        public void OnCloseAndSaveChanges()
        {
            if (ObjectsInDesignViewChanged)
                SaveToXAML();

            // Damir: Norbert deine Änderung vom 23.05. hatte zur Folge, dass die Methode designPanel_DragOver von einem falschen DesignManagerControlTool aufgerufen worden ist
            //if (DesignSurface != null)
            //DesignSurface.UnloadDesigner(true);

            if (PropertyGridView != null)
            {
                PropertyGridView.PropertyGrid.SelectedItems = null;
                PropertyGridView = null;
            }
            if (DesignItemTreeView != null)
            {
                DesignItemTreeView.RootItem = null;
                DesignItemTreeView = null;
            }
        }

        void OnSelectionChanged(object sender, DesignItemCollectionEventArgs e)
        {
            if (PropertyGridView != null)
                PropertyGridView.PropertyGrid.SelectedItems = DesignContext.Services.Selection.SelectedItems;

            if (BSOACComponent is IACComponentDesignManager)
            {
                if (DesignContext.Services.Selection.SelectedItems.Any())
                {
                    if (VBBSOSelectionManager != null)
                    {
                        var component = DesignContext.Services.Selection.SelectedItems.First().Component;
                        if (component is IVBContent)
                        {
                            VBBSOSelectionManager.ACUrlCommand("!OnVBControlClicked", component, false);
                            //(BSOACComponent as IACComponentDesignManager).SelectedVBControl = component as IVBContent;
                        }
                        else
                            VBBSOSelectionManager.ACUrlCommand("!OnVBControlClicked", null, false);
                    }
                }
            }
        }

        private IACComponent _SelectionManager;
        public IACComponent VBBSOSelectionManager
        {
            get
            {
                if (_SelectionManager != null)
                    return _SelectionManager;
                if (BSOACComponent != null)
                {
                    if (BSOACComponent is IACComponentDesignManager)
                    {
                        VBDesign vbDesign = this.GetVBDesign();
                        if (vbDesign != null)
                        {
                            _SelectionManager = vbDesign.VBBSOSelectionManager;
                            if (_SelectionManager != null && _SelectionManager.ReferencePoint != null)
                                _SelectionManager.ReferencePoint.Add(this);
                        }
                    }
                }
                return _SelectionManager;
            }
        }


        public bool ObjectsInDesignViewChanged
        {
            get;
            set;
        }

        void OnUndoStackChanged(object sender, EventArgs e)
        {
            if (!ObjectsInDesignViewChanged)
            {
                this.XMLText = this.XMLText;
            }
            ObjectsInDesignViewChanged = true;
        }

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDesignEditor));

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

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBDesignEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBDesignEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBDesignEditor thisControl = dependencyObject as VBDesignEditor;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }

        private string _VBDesignerDataContext;
        public string VBDesignerDataContext
        {
            get
            {
                return _VBDesignerDataContext;
            }
            set
            {
                _VBDesignerDataContext = value;
            }
        }

        private string _VBRefreshDesigner;
        public string VBRefreshDesigner
        {
            get
            {
                return _VBRefreshDesigner;
            }
            set
            {
                _VBRefreshDesigner = value;
            }
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
            get;set;
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBDesignEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
                if ( value == true )
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

        /// <summary>
        /// Updates a control mode.
        /// </summary>
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
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        IACType _VBContentPropertyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
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

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBDesignEditor));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
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
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBDesignEditor));
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
            var contentItem = vbDesign.ACContentList.FirstOrDefault();
            if (contentItem.ParentACObject != null)
                tbInfoClassName.Text = contentItem.ParentACObject.ACIdentifier;
            tblCaption.Text = vbDesign.ACCaption;
            tblCaptionTranslation.Text = vbDesign.ACClassDesign != null ? vbDesign.ACClassDesign.ACCaptionTranslation : vbDesign.ACCaption;
            tbInfoDesignFullName.Text = contentItem.ACIdentifier;
            tbInfoACUrl.Text = contentItem.GetACUrl();
            var acClassDesign = Database.GlobalDatabase.ACClassDesign.FirstOrDefault(c => c.ACIdentifier == contentItem.ACIdentifier);
            if (acClassDesign != null)
            {
                tbInsertDate.Text = acClassDesign.InsertDate.ToShortDateString();
                tbUpdateDate.Text = acClassDesign.UpdateDate.ToShortDateString();
            }
                
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

        #region ControlMode
        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
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
                        if ((_SelectionManager != null) && (_SelectionManager == acParameter[0]) && _SelectionManager.ReferencePoint != null)
                        {
                            _SelectionManager.ReferencePoint.Remove(this);
                            _SelectionManager = null;
                        }
                        break;
                }
            }
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
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
        /// <summary>
        /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
        /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method.
        /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern.
        /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <returns>List of menu entries</returns>
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

    }

    /*public class MyTypeFinder : XamlTypeFinder
    {
        OpenedFile file;
        readonly TypeResolutionService typeResolutionService = new TypeResolutionService();

        public static MyTypeFinder Create(OpenedFile file)
        {
            MyTypeFinder f = new MyTypeFinder();
            f.file = file;
            f.ImportFrom(CreateWpfTypeFinder());
            return f;
        }

        public override Assembly LoadAssembly(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                IProjectContent pc = GetProjectContent(file);
                if (pc != null)
                {
                    return this.typeResolutionService.LoadAssembly(pc);
                }
                return null;
            }
            else
            {
                Assembly assembly = FindAssemblyInProjectReferences(name);
                if (assembly != null)
                {
                    return assembly;
                }
                return base.LoadAssembly(name);
            }
        }

        Assembly FindAssemblyInProjectReferences(string name)
        {
            IProjectContent pc = GetProjectContent(file);
            if (pc != null)
            {
                return FindAssemblyInProjectReferences(pc, name);
            }
            return null;
        }

        Assembly FindAssemblyInProjectReferences(IProjectContent pc, string name)
        {
            foreach (IProjectContent referencedProjectContent in pc.ReferencedContents)
            {
                if (name == referencedProjectContent.AssemblyName)
                {
                    return this.typeResolutionService.LoadAssembly(referencedProjectContent);
                }
            }
            return null;
        }

        public override XamlTypeFinder Clone()
        {
            MyTypeFinder copy = new MyTypeFinder();
            copy.file = this.file;
            copy.ImportFrom(this);
            return copy;
        }

        internal static IProjectContent GetProjectContent(OpenedFile file)
        {
            if (ProjectService.OpenSolution != null && file != null)
            {
                IProject p = ProjectService.OpenSolution.FindProjectContainingFile(file.FileName);
                if (p != null)
                {
                    return ParserService.GetProjectContent(p);
                }
            }
            return ParserService.DefaultProjectContent;
        }
    }*/

}
