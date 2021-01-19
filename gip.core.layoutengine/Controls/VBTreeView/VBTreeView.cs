using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Reflection;
using System.Collections;
using System.Transactions;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The control element for displaying hierarchies. Example for item data template with checkbox => BSOTC3Sync.cs, design Explorer
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Hierarchien. Beispiel für Positionsdatenvorlage mit Checkbox => BSOTC3Sync.cs, Design Explorer
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTreeView'}de{'VBTreeView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTreeView : TreeView, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds = "Items";
        bool _isRoot;
        /// <summary>
        /// Represents the list of a custom control styles.
        /// </summary>
        protected static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TreeViewStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTreeControl/Themes/TreeViewStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TreeViewStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTreeControl/Themes/TreeViewStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTreeView), new FrameworkPropertyMetadata(typeof(VBTreeView)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBTreeView.
        /// </summary>
        public VBTreeView()
        {
            CheckBoxLevel = 0;
            ExpandLevel = 1;
            AutoLoad = true;
            this.AddHandler(TreeView.MouseDownEvent, new MouseButtonEventHandler(VBTreeView_MouseButtonDown), true);
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += VBTreeView_Loaded;
            this.Unloaded += VBTreeView_Unloaded;
            this.SourceUpdated += VBTreeView_SourceUpdated;
            this.TargetUpdated += VBTreeView_TargetUpdated;
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            InitVBControl();
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        #region Additional Dependcy-Properties
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBTreeView));

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
        /// Gets the container for item override.
        /// </summary>
        /// <returns>The new instance of VBTreeViewItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBTreeViewItem();
        }

        /// <summary>
        /// Determines is item overrides it's own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if is override, otherwise false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            //return item is VBTreeView;
            return base.IsItemItsOwnContainerOverride(item);
        }
        #endregion

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly DependencyProperty ACCaptionProperty
            = DependencyProperty.Register(Const.ACCaptionPrefix, typeof(string), typeof(VBTreeView), new PropertyMetadata(new PropertyChangedCallback(OnACCaptionChanged)));

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get { return (string)GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        private static void OnACCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IVBContent)
            {
                VBTreeView control = d as VBTreeView;
                if (control.ContextACObject != null)
                {
                    if (!control._Initialized)
                        return;
                    (control as VBTreeView).ACCaptionTrans = control.Root().Environment.TranslateText(control.ContextACObject, control.ACCaption);
                }
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty ACCaptionTransProperty
            = DependencyProperty.Register("ACCaptionTrans", typeof(string), typeof(VBTreeView));

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        public string ACCaptionTrans
        {
            get { return (string)GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly DependencyProperty ShowCaptionProperty
            = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBTreeView), new PropertyMetadata(true));
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for TreeItemTemplate.
        /// </summary>
        public static readonly DependencyProperty TreeItemTemplateProperty
            = DependencyProperty.Register("TreeItemTemplate", typeof(DataTemplate), typeof(VBTreeView));

        /// <summary>
        /// Gets or sets the data template for tree items in this control. 
        /// </summary>
        [Category("VBControl")]
        public DataTemplate TreeItemTemplate
        {
            get { return (DataTemplate)GetValue(TreeItemTemplateProperty); }
            set { SetValue(TreeItemTemplateProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBTreeView), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Determines is context menu disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return (bool)GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the VBTreeViewExpandMethod
        /// </summary>
        public static readonly DependencyProperty VBTreeViewExpandMethodProperty = DependencyProperty.Register("VBTreeViewExpandMethod", typeof(string), typeof(VBTreeViewItem));

        /// <summary>
        /// Represents the property where you enter the name of BSO's method, which will be invoked on OnVBTreeViewItemExpand event. It's used for VBTreeView lazy loading.
        /// </summary>
        [Category("VBControl")]
        public string VBTreeViewExpandMethod
        {
            get { return (string)GetValue(VBTreeViewExpandMethodProperty); }
            set { SetValue(VBTreeViewExpandMethodProperty, value); }
        }

        /// <summary>
        /// Determines is tree view check(set tick) items to root when is child item checked.
        /// </summary>
        [Category("VBControl")]
        public bool CheckToRoot
        {
            get { return (bool)GetValue(CheckToRootProperty); }
            set { SetValue(CheckToRootProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the CheckToRoot.
        /// </summary>
        public static readonly DependencyProperty CheckToRootProperty =
            DependencyProperty.Register("CheckToRoot", typeof(bool), typeof(VBTreeView), new PropertyMetadata(false));
        

        #region Loaded-Event

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || (ContextACObject == null))
                return;
            if (String.IsNullOrEmpty(VBContent) || String.IsNullOrEmpty(VBSource))
                return;

            _Initialized = true;
            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTreeView", VBContent);
                return;
            }
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;
            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBTreeView", VBSource + " " + VBContent);
                return;
            }

            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            if (VBContent != null && VBContent != "")
            {
                if (Visibility == Visibility.Visible)
                {
                    if (RightControlMode < Global.ControlModes.Disabled)
                    {
                        Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        // Beschriftung
                        if (string.IsNullOrEmpty(ACCaption))
                            ACCaptionTrans = VBContentPropertyInfo.ACCaption;
                        else
                            ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
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
                        if (BSOACComponent != null)
                        {
                            Global.ControlModesInfo controlModeInfo = BSOACComponent.GetControlModes(this);
                            Global.ControlModes controlMode = controlModeInfo.Mode;
                            Enabled = controlMode >= Global.ControlModes.Enabled;
                        }
                    }
                }

            }

            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            Binding binding;
            if (dsSource == null)
            {
                binding = new Binding(VBSource);
            }
            else
            {
                binding = new Binding();
                binding.Source = dsSource;
                binding.Path = new PropertyPath(dsPath);
            }
            binding.Mode = BindingMode.OneWay;
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            this.SetBinding(VBTreeView.TreeDataSourceProperty, binding);

            IACType ciACTypeInfo = null;
            object ciSource = null;
            string ciPath = "";
            Global.ControlModes ciRightControlMode = Global.ControlModes.Hidden;
            if (ContextACObject.ACUrlBinding(VBSource + "ChangeInfo", ref ciACTypeInfo, ref ciSource, ref ciPath, ref ciRightControlMode))
            {
                Binding bindingChangeInfo = new Binding();
                bindingChangeInfo.Source = ciSource;
                bindingChangeInfo.Path = new PropertyPath(ciPath);
                bindingChangeInfo.Mode = BindingMode.OneWay;
                this.SetBinding(VBTreeView.ChangeInfoProperty, bindingChangeInfo);
            }

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBTreeView.ACCompInitStateProperty, binding);
            }

        }

        bool _Loaded = false;
        void VBTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                Binding boundedValue = BindingOperations.GetBinding(this, VBTreeView.TreeDataSourceProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.Source as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBTreeView", "AddWPFRef", exw.Message);
                    }

                    if (this.Items.Count <= 0)
                        FillTree();
                }
            }
            _Loaded = true;
        }

        void VBTreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null)
            {
                if (BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(this.GetHashCode());
            }

            _Loaded = false;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());
            this.Loaded -= VBTreeView_Loaded;
            this.Unloaded -= VBTreeView_Unloaded;
            this.SourceUpdated -= VBTreeView_SourceUpdated;
            this.TargetUpdated -= VBTreeView_TargetUpdated;
            this.RemoveHandler(TreeView.MouseDownEvent, new MouseButtonEventHandler(VBTreeView_MouseButtonDown));
            foreach (TreeViewItem item in Items)
            {
                DeInitVBTreeViewItem(item);
            }
            BindingOperations.ClearBinding(this, VBTreeView.TreeDataSourceProperty);
            BindingOperations.ClearBinding(this, VBTreeView.ChangeInfoProperty);
            //BindingOperations.ClearBinding(this, VBTreeView.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBTreeView.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
            _VBContentPropertyInfo = null;

            ACQueryDefinition = null;
            CurrentTargetVBDataObject = null;
        }


        private void VBTreeView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
        }

        private void VBTreeView_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
        }

        /// <summary>
        /// Deinitializes a tree view item.
        /// </summary>
        /// <param name="item">The tree view item to deinitialize.</param>
        protected virtual void DeInitVBTreeViewItem(TreeViewItem item)
        {
            // Damir TODO: Delete when done with Datatemplate
            if (item == null)
                return;
            if (item.Header != null && item.Header is VBTextBlock)
            {
                VBTextBlock tb = item.Header as VBTextBlock;
                BindingOperations.ClearBinding(tb, VBTextBlock.TextProperty);
            }
            else if (item.Header != null && item.Header is Grid)
            {
                Grid g = item.Header as Grid;
                foreach (UIElement child in g.Children)
                {
                    if (child is VBCheckBox)
                    {
                        VBCheckBox cb = child as VBCheckBox;
                        BindingOperations.ClearBinding(cb, VBCheckBox.IsCheckedProperty);
                        cb.Click -= cb_Click;
                    }
                    else if (child is VBTextBlock)
                    {
                        VBTextBlock tb = child as VBTextBlock;
                        BindingOperations.ClearBinding(tb, VBTextBlock.TextProperty);
                    }
                }
            }
            foreach (TreeViewItem item2 in item.Items)
            {
                DeInitVBTreeViewItem(item2);
            }

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
        #endregion

        #region Event-Handling
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBTreeView));

        /// <summary>
        /// Represents the property where you enter the name of BSO's property which is in tree structure(example: ACClassInfoWithItems) to connect it with tree view.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
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

        /// <summary>
        /// Gets or sets the CheckBox level.
        /// </summary>
        [Category("VBControl")]
        public int CheckBoxLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependeny property for ExpandLevel.
        /// </summary>
        public static readonly DependencyProperty ExpandLevelProperty
            = DependencyProperty.Register("ExpandLevel", typeof(int), typeof(VBTreeView));

        /// <summary>
        /// Gets or sets the expand level. Determines to which level tree view will be pre-expanded.
        /// </summary>
        [Category("VBControl")]
        public int ExpandLevel
        {
            get { return (int)GetValue(ExpandLevelProperty); }
            set { SetValue(ExpandLevelProperty, value); }
        }
        #endregion

        #region IVBSource Members

        /// <summary>
        /// Represents the property where you enter the name of BSO's property which contains the root item of tree structure.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        /// <summary>
        /// Determines which column will be shown. (On example: ACCaption)
        /// </summary>
        [Category("VBControl")]
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        /// <summary>
        /// Determines which columns will be disabled.
        /// </summary>
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VBChilds.
        /// </summary>
        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        /// <summary>
        /// Gets or sets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }

        // TODO: 
        /// <summary>
        /// Gets or sets sort order.
        /// </summary>
        public string SortOrder
        {
            get;
            set;
        }

        private VBTreeViewItem CreateNewTreeItem(IACObject acObject)
        {
            VBTreeViewItem newTreeItem = new VBTreeViewItem(this, ContextACObject);
            //newTreeItem.MouseRightButtonDown += new MouseButtonEventHandler(treeViewItem_MouseRightButtonDown);
            newTreeItem.ContentACObject = acObject;
            if (this.TreeItemTemplate != null)
            {
                newTreeItem.DataContext = acObject;
                //if (this.TreeItemTemplate is DataTemplate)
                //{
                //    VBVisualTreeHelper.FindChildObjectInVisualTree(newTreeItem, typeof(VBCheckBox));
                //}
                //newTreeItem.HeaderTemplate = this.TreeItemTemplate;
                return newTreeItem;
            }

            bool showCheckbox = false;
            IVBDataCheckbox treeEntry = acObject as IVBDataCheckbox;
            if (treeEntry != null)
            {
                showCheckbox = !string.IsNullOrEmpty(treeEntry.DataContentCheckBox);
            }
            object bindingDataObject = acObject;

            string[] dataPath = VBShowColumns.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                bindingDataObject = bindingDataObject.GetType().GetProperty(path).GetValue(bindingDataObject, null);
            }
            Binding binding = new Binding(dataPath.Last());
            binding.Source = bindingDataObject;

            // Damir TODO: With Datatemplate
            if (!showCheckbox)
            {
                //Grid g = new Grid();
                //g.ColumnDefinitions.Add(new ColumnDefinition());

                VBTextBlock tb = new VBTextBlock();
                tb.SetBinding(VBTextBlock.TextProperty, binding);

                //g.Children.Add(tb);

                //ContentControl ctc = new ContentControl();
                //ctc.Content = tb;
                newTreeItem.Header = tb;
            }
            if (showCheckbox)
            {
                if (!_isRoot)
                {
                    Grid g = new Grid();
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30), MinWidth = 30 });
                    g.ColumnDefinitions.Add(new ColumnDefinition());

                    VBCheckBox cb = new VBCheckBox();
                    cb.Width = 30;
                    cb.MinWidth = 30;
                    Binding bindingCheckbox = new Binding();
                    bindingCheckbox.Mode = BindingMode.TwoWay;
                    bindingCheckbox.Source = bindingDataObject;
                    bindingCheckbox.Path = new PropertyPath(treeEntry.DataContentCheckBox);
                    cb.SetBinding(VBCheckBox.IsCheckedProperty, bindingCheckbox);

                    Binding bindingEnabled = new Binding();
                    bindingEnabled.Mode = BindingMode.OneWay;
                    bindingEnabled.Source = bindingDataObject;
                    bindingEnabled.Path = new PropertyPath("IsEnabled");
                    cb.SetBinding(VBCheckBox.IsEnabledProperty, bindingEnabled);
                    
                    cb.SetValue(Grid.RowProperty, 0);
                    cb.SetValue(Grid.ColumnProperty, 0);
                    cb.Click += new RoutedEventHandler(cb_Click);

                    VBTextBlock tb = new VBTextBlock();
                    tb.SetValue(Grid.RowProperty, 0);
                    tb.SetValue(Grid.ColumnProperty, 1);
                    tb.SetBinding(VBTextBlock.TextProperty, binding);

                    g.Children.Add(tb);
                    g.Children.Add(cb);

                    //ContentControl ctc = new ContentControl();
                    //ctc.Content = g;
                    newTreeItem.Header = g;
                }
                _isRoot = false;
            }
            return newTreeItem;
        }

        internal void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            object o = VisualTreeHelper.GetParent(cb);
            while (true)
            {
                if (o is TreeViewItem)
                {
                    break;
                }
                o = VisualTreeHelper.GetParent((DependencyObject)o);
            }
            VBTreeViewItem vbtvi = (VBTreeViewItem)o;
            vbtvi.IsSelected = true;
            if (vbtvi.ContentACObject is IVBDataCheckbox && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                checkChilds(vbtvi, (vbtvi.ContentACObject as IVBDataCheckbox).IsChecked);
                if (CheckToRoot)
                    checkParents(vbtvi);
            }
            vbtvi.BringIntoView();
            //ACObject.ValueChanged(VBContent + "_" + WithCheckbox);
        }

        private void checkParents(VBTreeViewItem vbtvi)
        {
            object o = vbtvi.Parent;
            if (o.GetType() == typeof(VBTreeViewItem))
            {
                checkParents((VBTreeViewItem)o);
                VBTreeViewItem parent = (VBTreeViewItem)o;

                if (parent.ContentACObject is IVBDataCheckbox)
                {
                    string dataContentCheckBox = ((IVBDataCheckbox)parent.ContentACObject).DataContentCheckBox;
                    if (!string.IsNullOrEmpty(dataContentCheckBox))
                    {
                        Type t = parent.ContentACObject.GetType();
                        PropertyInfo pi = t.GetProperty(dataContentCheckBox);
                        pi.SetValue(parent.ContentACObject, true, null);
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void checkChilds(VBTreeViewItem vbtvi, bool isChecked)
        {
            foreach (VBTreeViewItem tvi in vbtvi.Items)
            {
                checkChilds(tvi, isChecked);

                if (tvi.ContentACObject is IVBDataCheckbox)
                {
                    string dataContentCheckBox = ((IVBDataCheckbox)tvi.ContentACObject).DataContentCheckBox;
                    if (!string.IsNullOrEmpty(dataContentCheckBox))
                    {
                        Type t = tvi.ContentACObject.GetType();
                        if (t != null)
                        {
                            PropertyInfo pi = t.GetProperty(dataContentCheckBox);
                            if (pi != null)
                            {
                                pi.SetValue(tvi.ContentACObject, isChecked, null);
                            }
                        }
                    }
                }
            }
        }

        private object GetBSOData(Guid guid)
        {
            if (ContextACObject == null)
                return null;
            Type t = ContextACObject.GetType();
            Object dataObjectRoot = null;
            try
            {
                // Rootelement ermitteln
                dataObjectRoot = t.InvokeMember(VBSource, Global.bfGetProp, null, ContextACObject, null);

                return GetDataFromChilds(dataObjectRoot, guid);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBTreeView", "GetBSOData", msg);
                return null;
            }
        }

        private object GetDataFromChilds(object dataObject, Guid guid)
        {
            if (dataObject is IEnumerable)
            {
                IEnumerable list = dataObject as IEnumerable;
                foreach (var item in list)
                {
                    object result = GetDataFromChilds(item, guid);
                    if (result != null)
                        return result;
                }
                return null;
            }

            Type t2 = dataObject.GetType();

            object actItem = dataObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            t2 = actItem.GetType();
            Object dataChilds = t2.InvokeMember(dataPath.Last(), Global.bfGetProp, null, actItem, null);
            if (dataChilds != null && dataChilds is IEnumerable)
            {
                // Es gibt keinen Rootknoten
                var listValues = dataChilds as IEnumerable;
                foreach (var dataObjectChild in listValues)
                {
                    object result = GetDataFromChilds(dataObjectChild, guid);
                    if (result != null)
                        return result;
                }
            }

            return null;
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTreeView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        ///// <summary>
        ///// Represents the dependency property for ACUrlCmdMessage.
        ///// </summary>
        ////public static readonly DependencyProperty ACUrlCmdMessageProperty =
        ////    DependencyProperty.Register("ACUrlCmdMessage",
        ////        typeof(ACUrlCmdMessage), typeof(VBTreeView),
        ////        new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        ///// <summary>
        ///// Gets or sets the ACUrlCmdMessage.
        ///// </summary>
        ////public ACUrlCmdMessage ACUrlCmdMessage
        ////{
        ////    get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
        ////    set { SetValue(ACUrlCmdMessageProperty, value); }
        ////}

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBTreeView),
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
            VBTreeView thisControl = dependencyObject as VBTreeView;
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
        /// Zielobjekt beim ACDropData
        /// </summary>
        IACInteractiveObject CurrentTargetVBDataObject
        {
            get;
            set;
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if (!(ContextACObject is IACComponent))
                return;
            (ContextACObject as IACComponent).ACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            if (!(ContextACObject is IACComponent))
                return false;
            return (ContextACObject as IACComponent).IsEnabledACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }

        string _DblClick = "";
        /// <summary>
        /// Gets or sets the double click ACMethod name.
        /// </summary>
        public string DblClick
        {
            get
            {
                if (string.IsNullOrEmpty(_DblClick) && (BSOACComponent != null))
                {
                    var query = BSOACComponent.ACClassMethods.Where(c => c.InteractionVBContent == VBContent && c.SortIndex == (short)MISort.Load);
                    if (query.Any())
                    {
                        return query.First().ACIdentifier;
                    }
                }
                return _DblClick;
            }
            set
            {
                _DblClick = value;
            }
        }

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

        /// <summary>
        /// Determines is control enabled or disabled.
        /// </summary>
        public bool Enabled
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

        /// <summary>
        /// Gets or sets DragEnabled.
        /// </summary>
        [Category("VBControl")]
        public DragMode DragEnabled { get; set; }

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
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBTreeView));
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
        #endregion

        #region IDataHandling Members
        /// <summary>
        /// Determines is auto load enabled or disabled.
        /// </summary>
        [Category("VBControl")]
        public bool AutoLoad { get; set; }
        #endregion

        #region private methods

        /// <summary>
        /// Handles the OnContextMenuOpening event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }

        internal void treeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if (((TreeViewItem)sender).IsSelected != true)
                ((TreeViewItem)sender).IsSelected = true;
            if (sender is TreeViewItem)
            {
                if (ContextACObject == null)
                    return;
                Point point = e.GetPosition(sender as TreeViewItem);

                ACActionMenuArgs actionArgs = new ACActionMenuArgs(sender as IACInteractiveObject, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(sender as IACInteractiveObject, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (((TreeViewItem)sender).Parent is TreeView)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                else
                {
                    this.ContextMenu = null;
                }
                e.Handled = true;
            }
            else
                e.Handled = true;
        }

        /// <summary>
        /// Represents the dependency property for TreeDataSource.
        /// </summary>
        public static readonly DependencyProperty TreeDataSourceProperty
            = DependencyProperty.Register("TreeDataSource", typeof(object), typeof(VBTreeView), new PropertyMetadata(new PropertyChangedCallback(TreeDataSourceChanged)));

        /// <summary>
        /// Gets or sets the TreeDataSource.
        /// </summary>
        public object TreeDataSource
        {
            get
            {
                return GetValue(TreeDataSourceProperty);
            }
            set
            {
                SetValue(TreeDataSourceProperty, value);
            }
        }

        private static void TreeDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VBTreeView)d).FillTree(e.NewValue);
        }

        private void FillTree()
        {
            Items.Clear();

            if (ContextACObject == null)
                return;
            Type t = ContextACObject.GetType();
            Object root = ContextACObject.ACUrlCommand(VBSource, null);
            if (root == null)
            {
                root = ContextACObject;
                string[] dataPath = VBSource.Split('\\');

                // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
                for (int i = 0; i < dataPath.Length - 1; i++)
                {
                    string path = dataPath[i];
                    root = root.GetType().GetProperty(path).GetValue(root, null);
                }
                try
                {
                    t = root.GetType();
                    PropertyInfo pi = t.GetProperty(dataPath.Last());
                    root = pi.GetValue(root, null);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBTreeView", "FillTree", msg);
                    return;
                }
            }
            FillTree(root);
        }

        private void FillTree(object root)
        {
            Items.Clear();
            if (root == null)
                return;
            IACObject saveACObject = null;
            if (ContextACObject != null)
                saveACObject = ContextACObject.ACUrlCommand(VBContent, null) as IACObject;
            if (root is IEnumerable)
            {
                // Es gibt keinen Rootknoten
                var listValues = root as IEnumerable;

                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = from c in sortItems
                                 orderby c.Property
                                 select c.Item;
                    foreach (var item in sorted)
                    {

                        _isRoot = CheckBoxLevel > 0;
                        FillChilds(Items, item, 0);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(Items, item as IACObject, 1);
                    }
                }
            }
            else
            {
                // Es gibt einen Rootknoten
                _isRoot = CheckBoxLevel > 0;
                FillChilds(Items, root as IACObject, 0);
            }
            if (Items.Count > 0)
            {
                VBTreeViewItem treeItem = null;
                if (saveACObject != null)
                {
                    treeItem = FindVBTreeViewItemByContent(Items, saveACObject);
                }
                if (treeItem == null)
                    treeItem = (VBTreeViewItem)Items[0];
                treeItem.IsSelected = true;
                treeItem.BringIntoView();
            }
        }

        private VBTreeViewItem FillChilds(ItemCollection itemColletion, IACObject dataObject, int level)
        {
            IVBIsVisible isVisibleItem = dataObject as IVBIsVisible;
            if (isVisibleItem != null && !isVisibleItem.IsVisible)
                return null;
            VBTreeViewItem newTreeItem = CreateNewTreeItem(dataObject);
            if (level < ExpandLevel)
            {
                newTreeItem.IsExpanded = true;
            }
            itemColletion.Add(newTreeItem);

            object actItem = dataObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Type t1 = actItem.GetType();
            PropertyInfo pi = t1.GetProperty(dataPath.Last());
            Object o2 = pi.GetValue(actItem, null);
            if (o2 is IEnumerable)
            {
                // TODO: Sortierung der Elemente
                // Es gibt keinen Rootknoten
                var listValues = o2 as IEnumerable;
                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = from c in sortItems
                                 orderby c.Property
                                 select c.Item;
                    foreach (var item in sorted)
                    {
                        FillChilds(newTreeItem.Items, item, level + 1);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(newTreeItem.Items, item as IACObject, level + 1);
                    }
                }
            }
            return newTreeItem;
        }

        internal VBTreeViewItem FillChildsOnItemExpand(VBTreeViewItem expandedItem)
        {
            expandedItem.IsExpanded = true;
            object actItem = expandedItem.ContentACObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Type t1 = actItem.GetType();
            PropertyInfo pi = t1.GetProperty(dataPath.Last());
            Object o2 = pi.GetValue(actItem, null);
            if (o2 is IEnumerable)
            {
                // TODO: Sortierung der Elemente
                // Es gibt keinen Rootknoten
                var listValues = o2 as IEnumerable;
                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = from c in sortItems
                                 orderby c.Property
                                 select c.Item;
                    foreach (var item in sorted)
                    {
                        FillChilds(expandedItem.Items, item, 1);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(expandedItem.Items, item as IACObject, 1);
                    }
                }
            }
            return expandedItem;
        }

        /// <summary>
        /// Sucht das VBTreeViewItem in dem das acObject gefunden wird
        /// </summary>
        /// <param name="itemColletion"></param>
        /// <param name="acObject"></param>
        /// <returns></returns>
        private VBTreeViewItem FindVBTreeViewItemByContent(ItemCollection itemColletion, IACObject acObject)
        {
            if (acObject is ACClassInfoWithItems)
            {
                foreach (var item in itemColletion)
                {
                    VBTreeViewItem vbTreeViewItem = item as VBTreeViewItem;
                    if (vbTreeViewItem.ContentACObject is ACClassInfoWithItems)
                    {
                        if ((acObject as ACClassInfoWithItems).ValueT == (vbTreeViewItem.ContentACObject as ACClassInfoWithItems).ValueT)
                        {
                            return vbTreeViewItem;
                        }
                    }
                    var result = FindVBTreeViewItemByContent(vbTreeViewItem.Items, acObject);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Invoked on OnSelectedItemChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);
            if (SelectedItem != null)
            {
                VBTreeViewItem vbTreeViewItem = SelectedItem as VBTreeViewItem;
                if (vbTreeViewItem == null)
                    return;
                if (TreeValue != vbTreeViewItem)
                {
                    TreeValue = vbTreeViewItem;
                }
            }
        }

        #endregion

        #region DragAndDrop

        /// <summary>
        /// Represents the dependency property for TreeItemClicked.
        /// </summary>
        public static readonly DependencyProperty TreeItemClickedProperty
                = DependencyProperty.Register("TreeItemClicked", typeof(int), typeof(VBTreeView));

        /// <summary>
        /// Gets or sets the TreeItemClicked.
        /// </summary>
        public int TreeItemClicked
        {
            get
            {
                return (int)GetValue(TreeItemClickedProperty);
            }
            set
            {
                SetValue(TreeItemClickedProperty, value);
            }
        }

        void VBTreeView_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragEnabled == DragMode.Enabled || (Keyboard.IsKeyDown(Key.LeftShift) && DragEnabled == DragMode.EnabledMove))
            {
                System.Windows.Controls.Primitives.Thumb thumb = e?.MouseDevice?.DirectlyOver as System.Windows.Controls.Primitives.Thumb;
                if(thumb != null && thumb.TemplatedParent != null && thumb.TemplatedParent is System.Windows.Controls.Primitives.ScrollBar)
                    return;

                VBTreeViewItem vbTreeViewItem = SelectedItem as VBTreeViewItem;
                if (vbTreeViewItem != null)
                {
                    UpdateValue(true);
                    TreeItemClicked++;
                    VBDragDrop.VBDoDragDrop(vbTreeViewItem/*, acObject, ACComponent, new Point()*/);
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for the TreeValue.
        /// </summary>
        public static readonly DependencyProperty TreeValueProperty
                = DependencyProperty.Register("TreeValue", typeof(VBTreeViewItem), typeof(VBTreeView), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

        /// <summary>
        /// Gets or sets the TreeValue.
        /// </summary>
        public VBTreeViewItem TreeValue
        {
            get
            {
                return GetValue(TreeValueProperty) as VBTreeViewItem;
            }
            set
            {
                SetValue(TreeValueProperty, value);
            }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBTreeView)
            {
                VBTreeView vbTreecontrol = d as VBTreeView;
                if (vbTreecontrol == null || String.IsNullOrEmpty(vbTreecontrol.VBContent))
                    return;
                vbTreecontrol.UpdateValue();
            }
        }

        private void UpdateValue(bool always = false)
        {
            if (ContextACObject == null)
                return;
            if (TreeValue != null)
            {
                if (always || ContextACObject.ACUrlCommand(VBContent, null) != TreeValue.ContentACObject)
                {
                    ContextACObject.ACUrlCommand(VBContent, TreeValue.ContentACObject);
                }
            }
            else
            {
                VBTreeViewItem value = (VBTreeViewItem)SelectedItem;
                if (always || ContextACObject.ACUrlCommand(VBContent, null) != value.ContentACObject)
                {
                    ContextACObject.ACUrlCommand(VBContent, value.ContentACObject);
                }
            }
        }

        /// <summary>
        /// Finds the tree view item in the collection.
        /// </summary>
        /// <param name="itemCollection">The items collection.</param>
        /// <param name="acObject">The acObject.</param>
        /// <returns>TreeViewItem if it was found, otherwise false.</returns>
        public TreeViewItem FindItem(ItemCollection itemCollection, IACObject acObject)
        {
            foreach (var item in itemCollection)
            {
                VBTreeViewItem vbTreeViewItem = item as VBTreeViewItem;

                if (vbTreeViewItem.ContentACObject == acObject)
                    return vbTreeViewItem;
                TreeViewItem treeViewItem = FindItem(vbTreeViewItem.Items, acObject);
                if (treeViewItem != null)
                    return treeViewItem;
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the ChangeInfo.
        /// </summary>
        public ChangeInfo ChangeInfo
        {
            get { return (ChangeInfo)GetValue(ChangeInfoProperty); }
            set
            {
                SetValue(ChangeInfoProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for the ChangeInfo.
        /// </summary>
        public static readonly DependencyProperty ChangeInfoProperty
            = DependencyProperty.Register("ChangeInfo", typeof(ChangeInfo), typeof(VBTreeView), new PropertyMetadata(new PropertyChangedCallback(OnChangedChangeInfo)));

        private static void OnChangedChangeInfo(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is VBTreeView)
            {
                VBTreeView vbTreecontrol = d as VBTreeView;
                if (vbTreecontrol.ChangeInfo == null)
                    return;

                switch (vbTreecontrol.ChangeInfo.ChangeCmd)
                {
                    case Const.CmdDeleteData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            if (treeItem != null)
                            {
                                if (treeItem.Parent is TreeView) // Root-Item
                                {
                                    TreeView treeView = (TreeView)treeItem.Parent;
                                    treeView.Items.Remove(treeItem);
                                }
                                else
                                {
                                    VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                    if (parentTreeItem != null)
                                    {
                                        if (treeItem.Parent is VBTreeViewItem)
                                        {
                                            VBTreeViewItem parentVBTreeViewItem = treeItem.Parent as VBTreeViewItem;
                                            if (parentVBTreeViewItem.ContentACObject is IACContainerWithItems)
                                                ((IACContainerWithItems)parentVBTreeViewItem.ContentACObject).Remove(treeItem.ContentACObject as IACContainerWithItems);
                                        }
                                        parentTreeItem.Items.Remove(treeItem);
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdNewData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            if (treeItem != null)
                            {
                                if (treeItem.Parent is TreeView) // Root-Item
                                {
                                    TreeView treeView = (TreeView)treeItem.Parent;
                                    if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                                    {
                                        VBTreeViewItem newTreeItem = vbTreecontrol.CreateNewTreeItem(vbTreecontrol.ChangeInfo.ChangedObject);
                                        treeView.Items.Add(newTreeItem);
                                        newTreeItem.IsSelected = true;
                                        newTreeItem.BringIntoView();
                                    }
                                }
                                else
                                {
                                    VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                    if (parentTreeItem != null)
                                    {
                                        if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                                        {
                                            VBTreeViewItem newTreeItem = vbTreecontrol.CreateNewTreeItem(vbTreecontrol.ChangeInfo.ChangedObject);
                                            parentTreeItem.Items.Add(newTreeItem);
                                            newTreeItem.IsSelected = true;
                                            newTreeItem.BringIntoView();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdInsertData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            if (treeItem != null && treeItem.Parent is VBTreeViewItem)
                            {
                                VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                if (parentTreeItem != null)
                                {
                                    // Suche der Position zum einfügen
                                    for (int i = 0; i < parentTreeItem.Items.Count; i++)
                                    {
                                        if (parentTreeItem.Items[i] == treeItem)
                                        {
                                            if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                                            {
                                                VBTreeViewItem newTreeItem = vbTreecontrol.CreateNewTreeItem(vbTreecontrol.ChangeInfo.ChangedObject);
                                                parentTreeItem.Items.Insert(i, newTreeItem);
                                                newTreeItem.IsSelected = true;
                                                newTreeItem.BringIntoView();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdAddChildData:
                        {
                            VBTreeViewItem treeItem = null;
                            if (vbTreecontrol.ChangeInfo.ParentObject != null)
                            {
                                treeItem = (VBTreeViewItem)vbTreecontrol.FindItem(vbTreecontrol.Items, vbTreecontrol.ChangeInfo.ParentObject);
                            }
                            else
                            {
                                treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            }
                            if (treeItem != null)
                            {
                                if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                                {
                                    VBTreeViewItem newTreeItem = vbTreecontrol.FillChilds(treeItem.Items, vbTreecontrol.ChangeInfo.ChangedObject, 1);
                                    if (newTreeItem != null)
                                    {
                                        newTreeItem.IsSelected = true;
                                        newTreeItem.BringIntoView();
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdAddChildNoExpand:
                        {
                            VBTreeViewItem treeItem = null;
                            if (vbTreecontrol.ChangeInfo.ParentObject != null)
                                treeItem = (VBTreeViewItem)vbTreecontrol.FindItem(vbTreecontrol.Items, vbTreecontrol.ChangeInfo.ParentObject);
                            else
                                treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            
                            if (treeItem != null)
                                if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                                {
                                    VBTreeViewItem newTreeItem = vbTreecontrol.FillChilds(treeItem.Items, vbTreecontrol.ChangeInfo.ChangedObject, 1);
                                }
                        }
                        break;
                    case Const.CmdUpdateAllData:
                        {
                            VBTreeViewItem treeItem = null;
                            if (vbTreecontrol.ChangeInfo.ChangedObject != null)
                            {
                                treeItem = (VBTreeViewItem)vbTreecontrol.FindItem(vbTreecontrol.Items, vbTreecontrol.ChangeInfo.ChangedObject);
                            }
                            else
                            {
                                treeItem = (VBTreeViewItem)vbTreecontrol.SelectedItem;
                            }
                            if (treeItem != null)
                            {
                                treeItem.IsSelected = true;
                                treeItem.BringIntoView();
                            }
                        }
                        break;
                    default:
                        vbTreecontrol.FillTree();
                        break;
                }
            }
        }

        #region DragAndDrop
        /// <summary>
        /// Handles the OnDragEnter event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.OriginalSource.ToString()); // as UIElement
#endif
            HandleDragOver(this, e);
            base.OnDragEnter(e);
        }

        /// <summary>
        /// Handles the OnDragLeave event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDragLeave(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDragOver(this, e);
            base.OnDragLeave(e);
        }

        /// <summary>
        /// Handles the OnDragOver event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDragOver(this, e);
            base.OnDragOver(e);
        }

        /// <summary>
        /// Handles the OnDrop event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDrop(this, e);
            base.OnDrop(e);
        }

        /// <summary>
        /// Handles the DragEnter event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragEnter(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handles the DragLeave event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragLeave(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handles the DragOver event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragOver(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    HandleDragOver_Move(sender, 0, 0, e);
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, 0, 0, e);
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Move(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentTargetVBDataObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentTargetVBDataObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist

            Global.ElementActionType elementActionType = Global.ElementActionType.Drop;
            if (Keyboard.IsKeyDown(Key.LeftShift) && DragEnabled == DragMode.EnabledMove)
            {
                elementActionType = Global.ElementActionType.Move;
            }

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, elementActionType);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDrop(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Global.ElementActionType elementActionType = Global.ElementActionType.Drop;

            if (e.KeyStates == DragDropKeyStates.ShiftKey && DragEnabled == DragMode.EnabledMove)
                elementActionType = Global.ElementActionType.Move;
            switch (elementActionType)
            {
                case Global.ElementActionType.Move: // Vorhandene Elemente verschieben
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Move);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                case Global.ElementActionType.Drop: // Neue Elemente einfügen
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    return;
            }
        }

        private VBTreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }

            return container as VBTreeViewItem;
        }

        //VBDataObjectTarget GetTargetVBDataObject(UIElement element)
        //{
        //    VBTreeViewItem vbTreeViewItem = GetNearestContainer(element);
        //    if (vbTreeViewItem == null)
        //        return null;

        //    VBDataObjectTarget targetObject = new VBDataObjectTarget();

        //    targetObject.VBControl      = vbTreeViewItem;
        //    targetObject.ACComponent    = vbTreeViewItem.ACComponent;
        //    targetObject.ACObject       = vbTreeViewItem.ACObject;
        //    return targetObject;
        //}
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
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
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
        /// Gets the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<returns>Returns the list of ACMenu items.</returns>
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
        }
        #endregion

        #region IACObject Member

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

        #endregion
    }

    /// <summary>
    /// Represents the class for SortItem.
    /// </summary>
    class SortItem
    {
        /// <summary>
        /// Creates a new instance of Sortitem.
        /// </summary>
        /// <param name="item">The item parameter.</param>
        /// <param name="property">The name of property.</param>
        public SortItem(IACObject item, string property)
        {
            Item = item;

            object actItem = item;

            string[] dataPath = property.Split('.');
            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Property = (string)actItem.GetType().GetProperty(dataPath.Last()).GetValue(actItem, null);
        }

        /// <summary>
        /// Gets or sets the Item.
        /// </summary>
        public IACObject Item
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of property.
        /// </summary>
        public string Property
        {
            get;
            set;
        }

    }
}
