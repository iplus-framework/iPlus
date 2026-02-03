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
using System.Transactions;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a selectable item in <see cref="VBRadioButtonGroup"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert ein auswählbares Element in <see cref="VBRadioButtonGroup"/>
    /// </summary>
    public class VBRadioButtonGroupItem : ListBoxItem
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "RadioButtonItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonGroupStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "RadioButtonItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonGroupStyleAero.xaml" },
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


        static VBRadioButtonGroupItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRadioButtonGroupItem), new FrameworkPropertyMetadata(typeof(VBRadioButtonGroupItem)));
        }

        bool _themeApplied = false;
        public VBRadioButtonGroupItem()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
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
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }

        public static readonly DependencyProperty PushButtonStyleProperty
            = DependencyProperty.Register("PushButtonStyle", typeof(Boolean), typeof(VBRadioButtonGroupItem));
        [Category("VBControl")]
        public Boolean PushButtonStyle
        {
            get { return (Boolean)GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

        }
    }

    /// <summary>
    /// Control for representing a radio button group.
    /// </summary>
    /// <summary>
    /// Steuerelement zur Darstellung einer Schaltergruppe.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBRadioButtonGroup'}de{'VBRadioButtonGroup'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBRadioButtonGroup : ListBox, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "RadioButtonGroupStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonGroupStyleGip.xaml",
                                        hasImplicitStyles = false},
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "RadioButtonGroupStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonGroupStyleAero.xaml",
                                        hasImplicitStyles = false},
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

        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }


        static VBRadioButtonGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRadioButtonGroup), new FrameworkPropertyMetadata(typeof(VBRadioButtonGroup)));
        }

        bool _themeApplied = false;
        public VBRadioButtonGroup()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += VBRadioButtonGroup_Loaded;
            this.Unloaded += VBRadioButtonGroup_Unloaded;
            this.SourceUpdated += VB_SourceUpdated;
            this.TargetUpdated += VB_TargetUpdated;
            ActualizeTheme(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            InitVBControl();
        }

        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        #region Additional Dependenc-Properties
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBRadioButtonGroup));

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

        protected override DependencyObject GetContainerForItemOverride()
        {
            VBRadioButtonGroupItem item = new VBRadioButtonGroupItem();
            item.PushButtonStyle = this.PushButtonStyle;
            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, VBRadioButtonGroup.ItemsMinWidthProperty);
            if (valueSource != null)
            {
                if (valueSource.BaseValueSource == BaseValueSource.Local)
                {
                    item.MinWidth = this.ItemsMinWidth;
                }
            }
            valueSource = DependencyPropertyHelper.GetValueSource(this, VBRadioButtonGroup.ItemsMaxWidthProperty);
            if (valueSource != null)
            {
                if (valueSource.BaseValueSource == BaseValueSource.Local)
                {
                    item.MaxWidth = this.ItemsMaxWidth;
                }
            }
            return item;
        }

        public enum ItemHostType : short
        {
            StackPanel = 0,
            WrapPanel = 1,
            DockPanel = 2,
            Grid = 3,
        }

        public static readonly DependencyProperty ItemsHostTypeProperty
            = DependencyProperty.Register("ItemsHostType", typeof(ItemHostType), typeof(VBRadioButtonGroup), new PropertyMetadata(ItemHostType.StackPanel));
        [Category("VBControl")]
        public ItemHostType ItemsHostType
        {
            get { return (ItemHostType)GetValue(ItemsHostTypeProperty); }
            set { SetValue(ItemsHostTypeProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerVisibilityProperty
            = DependencyProperty.Register("ScrollViewerVisibility", typeof(Visibility), typeof(VBRadioButtonGroup), new PropertyMetadata(Visibility.Visible));
        [Category("VBControl")]
        public Visibility ScrollViewerVisibility
        {
            get { return (Visibility)GetValue(ScrollViewerVisibilityProperty); }
            set { SetValue(ScrollViewerVisibilityProperty, value); }
        }


        public static readonly DependencyProperty PushButtonStyleProperty
            = DependencyProperty.Register("PushButtonStyle", typeof(Boolean), typeof(VBRadioButtonGroup));
        [Category("VBControl")]
        public Boolean PushButtonStyle
        {
            get { return (Boolean)GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty ItemsMinWidthProperty
            = DependencyProperty.Register("ItemsMinWidth", typeof(Double), typeof(VBRadioButtonGroup));
        [Category("VBControl")]
        public Double ItemsMinWidth
        {
            get { return (Double)GetValue(ItemsMinWidthProperty); }
            set { SetValue(ItemsMinWidthProperty, value); }
        }


        public static readonly DependencyProperty ItemsMaxWidthProperty
            = DependencyProperty.Register("ItemsMaxWidth", typeof(Double), typeof(VBRadioButtonGroup));
        [Category("VBControl")]
        public Double ItemsMaxWidth
        {
            get { return (Double)GetValue(ItemsMaxWidthProperty); }
            set { SetValue(ItemsMaxWidthProperty, value); }
        }

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

        public static readonly DependencyProperty ColumnsProperty
            = DependencyProperty.Register("Columns", typeof(int), typeof(VBRadioButtonGroup));
        [Category("VBControl")]
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty
            = DependencyProperty.Register("Rows", typeof(int), typeof(VBRadioButtonGroup));
        [Category("VBControl")]
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly DependencyProperty ACCaptionProperty
            = DependencyProperty.Register(Const.ACCaptionPrefix, typeof(string), typeof(VBRadioButtonGroup), new PropertyMetadata(new PropertyChangedCallback(OnACCaptionChanged)));

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
                VBRadioButtonGroup control = d as VBRadioButtonGroup;
                if (control.ContextACObject != null)
                {
                    if (!control._Initialized)
                        return;
                    (control as VBRadioButtonGroup).ACCaptionTrans = control.Root().Environment.TranslateText(control.ContextACObject, control.ACCaption);
                }
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty ACCaptionTransProperty
            = DependencyProperty.Register("ACCaptionTrans", typeof(string), typeof(VBRadioButtonGroup));

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
            = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBRadioButtonGroup), new PropertyMetadata(true));
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

        public static readonly DependencyProperty HorizontalItemsProperty
            = DependencyProperty.Register("HorizontalItems", typeof(Boolean), typeof(VBRadioButtonGroup));

        [Category("VBControl")]
        public Boolean HorizontalItems
        {
            get
            {
                return (Boolean)GetValue(HorizontalItemsProperty);
            }
            set
            {
                SetValue(HorizontalItemsProperty, value);
            }
        }

        #endregion

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
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBRadioButtonGroup", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            //IACBSOContext bsoContext = VBLogicalTreeHelper.GetBSOContext(this);
            //bsoContext.VBControlList.Add(this);

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (VBContentPropertyInfo == null)
                return;

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                Visibility = Visibility.Collapsed;
            }
            // Beschriftung
            if (string.IsNullOrEmpty(ACCaption))
                ACCaptionTrans = VBContentPropertyInfo.ACCaption;
            else
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

            // Listenbereich füllen (VBSource, VBShowColumns)
            string acAccess = "";
            ACClassProperty sourceProperty = null;
            VBSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBRadioButtonGroup", VBSource + " " + VBContent);
                //this.Root().Messages.ErrorAsync(ContextACObject, "Error00004", "VBRadioButtonGroup", VBSource, VBContent);
                return;
            }

            if (string.IsNullOrEmpty(acAccess))
            {
                ACQueryDefinition = this.Root().Queries.CreateQueryByClass(null, dcACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), dsACTypeInfo.ValueTypeACClass.ACIdentifier, true);
            }
            else
            {
                IAccess access = ContextACObject.ACUrlCommand(acAccess) as IAccess;
                ACQueryDefinition = access.NavACQueryDefinition;
            }

            List<ACColumnItem> vbShowColumns = ACQueryDefinition != null ? ACQueryDefinition.ACColumns : null; // dsACTypeInfo.MyACColumns(9999, VBShowColumns);

            if (vbShowColumns == null || !vbShowColumns.Any())
            {
                this.Root().Messages.LogDebug("Error00005", "VBRadioButtonGroup", VBShowColumns + " " + VBContent);
                //this.Root().Messages.ErrorAsync(ContextACObject, "Error00005", "VBRadioButtonGroup", VBShowColumns, VBContent);
                return;
            }

            IACType dsColACTypeInfo = null;
            object dsColSource = null;
            string dsColPath = "";
            Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
            if (!dsACTypeInfo.ACUrlBinding(vbShowColumns[0].PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
            {
                return;
            }

            if (dsACTypeInfo is ACClassMethod)
            {
                ObjectDataProvider odp = new ObjectDataProvider();
                odp.ObjectInstance = dsSource;
                string param = VBSource.Substring(VBSource.IndexOf('(') + 1, VBSource.LastIndexOf(')') - (VBSource.IndexOf('(') + 1));
                if (param.StartsWith("#") && param.EndsWith("#"))
                {
                    odp.MethodParameters.Add(param.Substring(1, param.Length - 2));
                }
                else
                {
                    string[] paramList = param.Split(',');
                    foreach (var param1 in paramList)
                    {
                        odp.MethodParameters.Add(param1.Replace("\"", ""));
                    }
                }
                odp.MethodName = dsPath.Substring(1);  // "!" bei Methodenname entfernen
                Binding binding = new Binding();
                binding.Source = odp;
                SetBinding(ListBox.ItemsSourceProperty, binding);
            }
            else
            {
                Binding binding = new Binding();
                binding.Source = dsSource;
                if (!string.IsNullOrEmpty(dsPath))
                {
                    binding.Path = new PropertyPath(dsPath);
                }
                SetBinding(ListBox.ItemsSourceProperty, binding);
            }
            // TODO: Unterstützt bisher nur eine Spalte
            if (ItemTemplate == null)
                DisplayMemberPath = dsColPath;

            if (dcACTypeInfo.ObjectType.IsEnum)
            {
                SetValue(ListBox.SelectedValuePathProperty, Const.Value);

                Binding binding2 = new Binding();
                binding2.Source = dcSource;
                binding2.Path = new PropertyPath(dcPath);
                binding2.Mode = BindingMode.TwoWay;
                binding2.NotifyOnSourceUpdated = true;
                binding2.NotifyOnTargetUpdated = true;
                //SetBinding(ComboBox.SelectedItemProperty, binding2);
                SetBinding(ListBox.SelectedValueProperty, binding2);
            }
            else
            {
                // Gebundene Spalte setzen (VBContent)
                Binding binding2 = new Binding();
                binding2.Source = dcSource;
                binding2.Path = new PropertyPath(dcPath);
                if (VBContentPropertyInfo != null)
                    binding2.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                binding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SetBinding(ListBox.SelectedValueProperty, binding2);
            }

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBRadioButtonGroup.ACCompInitStateProperty, binding);
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
        }

        bool _Loaded = false;
        void VBRadioButtonGroup_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                Binding boundedValue = BindingOperations.GetBinding(this, ListBox.ItemsSourceProperty);
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
                        this.Root().Messages.LogDebug("VBRadioButtonGroup", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBRadioButtonGroup_Unloaded(object sender, RoutedEventArgs e)
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
            _VBContentPropertyInfo = null;
            this.Loaded -= VBRadioButtonGroup_Loaded;
            this.Unloaded -= VBRadioButtonGroup_Unloaded;
            this.SourceUpdated -= VB_SourceUpdated;
            this.TargetUpdated -= VB_TargetUpdated;

            BindingOperations.ClearBinding(this, ListBox.ItemsSourceProperty);
            BindingOperations.ClearBinding(this, ListBox.SelectedValueProperty);
            //BindingOperations.ClearBinding(this, VBRadioButtonGroup.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBRadioButtonGroup.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
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

        #region Methods
        private IACObject GetNearestContainer(UIElement element)
        {
            if (element == null)
                return null;

            //if (element is DataGridCellsPresenter)
            //{
            //    DataGridCellsPresenter presenter = element as DataGridCellsPresenter;
            //    return presenter.Item as IACObjectWithBinding;
            //}
            //else //if (element is Border)
            {
                int count = 0;
                while (element != null && count < 10)
                {
                    element = VisualTreeHelper.GetParent(element) as UIElement;

                    if ((element != null) && (element is VBRadioButtonGroupItem))
                    {
                        VBRadioButtonGroupItem vbListBoxItem = element as VBRadioButtonGroupItem;
                        return vbListBoxItem.Content as IACObject;
                    }
                    count++;
                }
            }
            return null;
        }
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBRadioButtonGroup));

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

        #endregion

        #region IVBSource Members

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

        public string VBDisabledColumns
        {
            get;
            set;
        }

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

        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBRadioButtonGroup), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
        ////        typeof(ACUrlCmdMessage), typeof(VBRadioButtonGroup),
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
                typeof(ACInitState), typeof(VBRadioButtonGroup),
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
            VBRadioButtonGroup thisControl = dependencyObject as VBRadioButtonGroup;
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

        List<IACObject> _ACContentList = new List<IACObject>();
        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return _ACContentList;
            }
        }

        private void UpdateACContentList(IACObject acObject)
        {
            _ACContentList.Clear();
            // TODO Norbert:
            if (acObject != null)
            {
                _ACContentList.Add(acObject);

                int pos1 = VBContent.LastIndexOf('\\');
                if ((pos1 != -1) && (ContextACObject != null))
                {
                    _ACContentList.Add(ContextACObject.ACUrlCommand(VBContent.Substring(0, pos1)) as IACObject);
                }
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

        /// <summary>
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBRadioButtonGroup), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
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

        void VB_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            e.Handled = true;
            UpdateControlMode();
        }

        void VB_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
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
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBRadioButtonGroup));
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

        #region DragAndDrop
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion
    }
}
