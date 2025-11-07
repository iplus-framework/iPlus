using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using gip.ext.designer.avui;
using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace gip.core.layoutengine.avui
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
        protected override void OnInitialized()
        {
            DataContextChanged += VBDesignEditor_DataContextChanged;
            base.OnInitialized();
        }

        void VBDesignEditor_DataContextChanged(object sender, EventArgs e)
        {
            DeInitVBControl();
        }

        private static ACClass _ClassOfBSOiPlusStudio;
        private static ACClass ClassOfBSOiPlusStudio
        {
            get
            {
                if (_ClassOfBSOiPlusStudio != null)
                    return _ClassOfBSOiPlusStudio;
                _ClassOfBSOiPlusStudio = Database.GlobalDatabase.GetACType("BSOiPlusStudio");
                return _ClassOfBSOiPlusStudio;
            }
        }

        private static Type TypeOfBSOiPlusStudio
        {
            get
            {
                if (ClassOfBSOiPlusStudio == null)
                    return null;
                return ClassOfBSOiPlusStudio.ObjectType;
            }
        }


        private static ACClass _ClassOfBSOVisualisationStudio;

        private static ACClass ClassOfBSOVisualisationStudio
        {
            get
            {
                if (_ClassOfBSOVisualisationStudio != null)
                    return _ClassOfBSOVisualisationStudio;
                _ClassOfBSOVisualisationStudio = Database.GlobalDatabase.GetACType("BSOVisualisationStudio");
                return _ClassOfBSOVisualisationStudio;
            }
        }

        private static Type TypeOfBSOVisualisationStudio
        {
            get 
            {
                if (ClassOfBSOVisualisationStudio == null)
                    return null;
                return ClassOfBSOVisualisationStudio.ObjectType;
            }
        }

        protected bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
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

            // Check if RightControlMode is locally set
            if (!this.IsSet(VBDesignEditor.RightControlModeProperty)
                || RightControlMode < dcRightControlMode)
            {
                RightControlMode = dcRightControlMode;
            }

            // VBContent muß im XAML gestettet sein
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

                    var designManager = GetDesignManager(false);
                    if ((designManager == null) && (dcSource is IACComponentDesignManager))
                        designManager = dcSource as IACComponentDesignManager;
                    if (designManager != null)
                    {
                        if (!designManager.ShowXMLEditor)
                        {
                            if (ucTabControl != null && ucTabControl.Items != null)
                            {
                                foreach (var item in ucTabControl.Items)
                                {
                                    if (item is VBTabItem vbTabItem && vbTabItem.Name == "XAMLTab")
                                    {
                                        ucTabControl.Items.Add(vbTabItem);
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

                    var binding = new Binding
                    {
                        Source = dcSource,
                        Path = dcPath,
                        Mode = BindingMode.TwoWay
                    };

                    this.Bind(VBDesignEditor.XMLTextProperty, binding);

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

                        var binding2 = new Binding
                        {
                            Source = dcSource2,
                            Path = dcPath2,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBDesignEditor.DesignerDataContextProperty, binding2);
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

                        var binding2 = new Binding
                        {
                            Source = dcSource2,
                            Path = dcPath2,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBDesignEditor.RefreshDesignerProperty, binding2);
                    }

                    if (BSOACComponent != null)
                    {
                        var initStateBinding = new Binding
                        {
                            Source = BSOACComponent,
                            Path = Const.InitState,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBDesignEditor.ACCompInitStateProperty, initStateBinding);
                    }               
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

        public void OnTargetUpdatedOfBinding(Object sender, AvaloniaPropertyChangedEventArgs args)
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

        //public void OnSourceUpdatedOfBinding(Object sender, AvaloniaPropertyChangedEventArgs args)
        //{
        //    if (args.Property == VBDesignEditor.XMLTextProperty)
        //    {
        //    }
        //    else if (args.Property == VBDesignEditor.DesignerDataContextProperty)
        //    {
        //    }
        //    else if (args.Property == VBDesignEditor.RefreshDesignerProperty)
        //    {
        //        //RefreshDesignEditor();
        //    }
        //}

        object _LastActiveTab = null;
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
        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
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

            this.DataContextChanged -= VBDesignEditor_DataContextChanged;

            // Clear bindings using Avalonia's method
            //if (!String.IsNullOrEmpty(VBRefreshDesigner))
            //    this[VBDesignEditor.RefreshDesignerProperty] = AvaloniaProperty.UnsetValue;
            //if (!String.IsNullOrEmpty(VBDesignerDataContext))
            //    this[VBDesignEditor.DesignerDataContextProperty] = AvaloniaProperty.UnsetValue;
            //this[VBDesignEditor.XMLTextProperty] = AvaloniaProperty.UnsetValue;
            //this[VBDesignEditor.ACUrlCmdMessageProperty] = AvaloniaProperty.UnsetValue;
            //this[VBDesignEditor.ACCompInitStateProperty] = AvaloniaProperty.UnsetValue;
            this.ClearAllBindings();

            if (GetValue(BSOACComponentProperty) != AvaloniaProperty.UnsetValue)
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

        public static readonly StyledProperty<VBPropertyGridView> PropertyGridViewProperty =
            AvaloniaProperty.Register<VBDesignEditor, VBPropertyGridView>(nameof(PropertyGridView));
        public VBPropertyGridView PropertyGridView
        {
            get
            {
                return GetValue(PropertyGridViewProperty);
            }
            set
            {
                SetValue(PropertyGridViewProperty, value);
            }
        }


        public static readonly StyledProperty<VBDesignItemTreeView> DesignItemTreeViewProperty =
            AvaloniaProperty.Register<VBDesignEditor, VBDesignItemTreeView>(nameof(DesignItemTreeView));
        public VBDesignItemTreeView DesignItemTreeView
        {
            get
            {
                return GetValue(DesignItemTreeViewProperty);
            }
            set
            {
                SetValue(DesignItemTreeViewProperty, value);
            }
        }

        public static readonly StyledProperty<string> XMLTextProperty =
            AvaloniaProperty.Register<VBDesignEditor, string>(nameof(XMLText));

        public string XMLText
        {
            get
            {
                return GetValue(XMLTextProperty);
            }
            set
            {
                SetValue(XMLTextProperty, value);
            }
        }

        public static readonly StyledProperty<IACObject> DesignerDataContextProperty =
            AvaloniaProperty.Register<VBDesignEditor, IACObject>(nameof(DesignerDataContext));

        public IACObject DesignerDataContext
        {
            get
            {
                return GetValue(DesignerDataContextProperty);
            }
            set
            {
                SetValue(DesignerDataContextProperty, value);
            }
        }


        public static readonly StyledProperty<int> RefreshDesignerProperty =
            AvaloniaProperty.Register<VBDesignEditor, int>(nameof(RefreshDesigner));

        public int RefreshDesigner
        {
            get
            {
                return GetValue(RefreshDesignerProperty);
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

            VBDesign vbDesign = this.GetVBDesign();
            ACClassDesign acClassDesign = null;

            if (DataContext != null && TypeOfBSOiPlusStudio.IsAssignableFrom(DataContext.GetType()))
            {
                IACComponent component = this.DataContext as IACComponent;
                if (component != null)
                    acClassDesign = component.ACUrlCommand("CurrentACClassDesign") as ACClassDesign;
            }
            else if (DataContext != null && TypeOfBSOVisualisationStudio.IsAssignableFrom(DataContext.GetType()))
            {
                IACComponent component = this.DataContext as IACComponent;
                if (component != null)
                    acClassDesign = component.ACUrlCommand("CurrentVisualisation") as ACClassDesign;
            }
            if (acClassDesign == null && vbDesign != null)
            {
                acClassDesign = vbDesign.ACClassDesign;
            }

            if (acClassDesign != null)
            {
                tbInfoDesignFullName.Text = acClassDesign.ACIdentifier;
                tbInfoClassName.Text = acClassDesign.ACClass?.ACIdentifier;
                tblCaption.Text = acClassDesign.ACCaption;
                tblCaptionTranslation.Text = acClassDesign.ACCaptionTranslation;
                tbInsertDate.Text = acClassDesign.InsertDate.ToString();
                tbUpdateDate.Text = acClassDesign.UpdateDate.ToString();
                tbInfoACUrl.Text = acClassDesign.GetACUrl();
            }
            else
            {
                IACObject contentItem = vbDesign.ACContentList.FirstOrDefault();
                if (contentItem != null)
                {
                    if (contentItem.ParentACObject != null)
                        tbInfoClassName.Text = contentItem.ParentACObject.ACIdentifier;
                    tbInfoDesignFullName.Text = contentItem.ACIdentifier;
                    tbInfoACUrl.Text = contentItem.GetACUrl();
                }
                tblCaption.Text = vbDesign.ACCaption;
                tblCaptionTranslation.Text = vbDesign.ACCaption;
                tbInsertDate.Text = "";
                tbUpdateDate.Text = "";
                tbInfoACUrl.Text = "";
            }
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

                try
                {
                    if (TypeOfBSOiPlusStudio.IsAssignableFrom(DataContext.GetType()))
                    {
                        if (DesignerDataContext != null)
                        {
                            if (DesignerDataContext is ACClass)
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
                                if (TypeOfBSOVisualisationStudio.IsAssignableFrom(acClass.ObjectType))
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
                                && (c.Component is Control) 
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
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDesignEditor, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBDesignEditor, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBDesignEditor, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            VBDesignEditor thisControl = this;
            if (change.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
            else if (change.Property == XMLTextProperty)
            {
                OnTargetUpdatedOfBinding(change.Sender, change);
            }
            else if (change.Property == DesignerDataContextProperty)
            {
                OnTargetUpdatedOfBinding(change.Sender, change);
            }
            else if (change.Property == RefreshDesignerProperty)
            {
                OnTargetUpdatedOfBinding(change.Sender, change);
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
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDesignEditor>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
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
        
        private bool Visible
        {
            get
            {
                return IsVisible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        IsVisible = true;
                    }
                }
                else
                {
                    IsVisible = false;
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
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBDesignEditor, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get
            {
                return GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }


        /// <summary>
        /// Represents the dependency property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBDesignEditor, string>(nameof(DisabledModes));
        /// <summary>
        /// Gets or sets the DisabledModes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die DisabledModes.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        public IACComponentDesignManager GetDesignManager(bool create = false)
        {
            VBDesign vbDesign = this.GetVBDesign();
                
            if (BSOACComponent is IACComponentDesignManager)
                return BSOACComponent as IACComponentDesignManager;
            else
            {
                if (BSOACComponent?.ACComponentChilds != null)
                {
                    foreach (IACComponent child in BSOACComponent.ACComponentChilds)
                    {
                        if (child is IACComponentDesignManager)
                        {
                            return child as IACComponentDesignManager;
                        }
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
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBDesignEditor, Global.ControlModes>(nameof(RightControlMode));

        public Global.ControlModes RightControlMode
        {
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

    }

}
