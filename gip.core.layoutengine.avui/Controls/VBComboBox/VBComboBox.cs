using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Transactions;
using System.Reflection;
using System.Diagnostics;
using Avalonia.Controls;
using gip.ext.designer.avui;

namespace gip.core.layoutengine.avui
{
    ///<summary>
    ///Control for displaying a value with list selection.
    ///</summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von einem Wert mit Listenauswahl.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBComboBox'}de{'VBComboBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBComboBox : ComboBox, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ComboBoxStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBComboBox/Themes/ComboBoxStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ComboBoxStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBComboBox/Themes/ComboBoxStyleAero.xaml" },
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


        static VBComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBComboBox), new FrameworkPropertyMetadata(typeof(VBComboBox)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBComboBox.
        /// </summary>
        public VBComboBox()
        {
            VisibilityFilterRow = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            TargetUpdated += new EventHandler<DataTransferEventArgs>(VBComboBox_TargetUpdated);
            SourceUpdated += new EventHandler<DataTransferEventArgs>(VBComboBox_SourceUpdated);
            Loaded += VBComboBox_Loaded;
            Unloaded += VBComboBox_Unloaded;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            _EditableTextBoxSite2 = (GetTemplateChild("PART_EditableTextBox") as TextBox);
            if (_EditableTextBoxSite2 != null)
            {
                _EditableTextBoxSite2.TextChanged += new TextChangedEventHandler(OnEditableTextBoxTextChanged2);
                _EditableTextBoxSite2.LostKeyboardFocus += _EditableTextBoxSite2_LostKeyboardFocus;
            }
            _PART_TakeCount = (GetTemplateChild("PART_TakeCount") as FrameworkElement);
            if (_PART_TakeCount != null)
                _PART_TakeCount.MouseLeftButtonDown += _PART_TakeCount_MouseLeftButtonDown;

            InitVBControl();
        }

        private void _PART_TakeCount_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ACAccess != null && ACAccess.NavACQueryDefinition != null)
            {
                if (e.ClickCount > 1)
                {
                    if (ACAccess.NavACQueryDefinition.TakeCount != 0)
                        ACAccess.NavACQueryDefinition.TakeCount = 0;
                    else if (Database.Root.Environment.AccessDefaultTakeCount > 0)
                        ACAccess.NavACQueryDefinition.TakeCount = Database.Root.Environment.AccessDefaultTakeCount;
                }
                else
                {
                    if (ACAccess.NavACQueryDefinition.TakeCount > 0)
                    {
                        ACAccess.NavACQueryDefinition.TakeCount *= 2;
                        NavSearchOnACAccess(true);
                    }
                    else if (Database.Root.Environment.AccessDefaultTakeCount > 0)
                        ACAccess.NavACQueryDefinition.TakeCount = Database.Root.Environment.AccessDefaultTakeCount;
                }
            }
        }

        /// <summary>
        /// Handles a OnTemplatedChanged.
        /// </summary>
        /// <param name="oldTemplate">The old template.</param>
        /// <param name="newTemplate">The new template.</param>
        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            if (_EditableTextBoxSite2 != null)
            {
                _EditableTextBoxSite2.TextChanged -= new TextChangedEventHandler(OnEditableTextBoxTextChanged2);
                _EditableTextBoxSite2.LostKeyboardFocus -= _EditableTextBoxSite2_LostKeyboardFocus;
            }
            if (_PART_TakeCount != null)
                _PART_TakeCount.MouseLeftButtonDown -= _PART_TakeCount_MouseLeftButtonDown;

            base.OnTemplateChanged(oldTemplate, newTemplate);
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

        #region Additional Dependency Properties
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBComboBox));

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
        /// Represents the dependency property for VisibilityFilterRow.
        /// </summary>
        public static readonly DependencyProperty VisibilityFilterRowProperty
            = DependencyProperty.Register("VisibilityFilterRow", typeof(Visibility), typeof(VBComboBox));

        /// <summary>
        /// Determines is filter row visible or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die Filterzeile sichtbar ist oder nicht.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Visibility VisibilityFilterRow
        {
            get
            {
                return (Visibility)GetValue(VisibilityFilterRowProperty);
            }
            set
            {
                SetValue(VisibilityFilterRowProperty, value);
            }
        }


        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly DependencyProperty ACCaptionProperty
            = DependencyProperty.Register(Const.ACCaptionPrefix, typeof(string), typeof(VBComboBox), new PropertyMetadata(new PropertyChangedCallback(OnACCaptionChanged)));
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return (string)GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        private static void OnACCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBComboBox)
            {
                VBComboBox control = d as VBComboBox;
                //if (!control._Initialized)
                //    return;
                if (control != null)
                {
                    if (control.BSOACComponent != null)
                        control.ACCaptionTrans = control.Root().Root.Environment.TranslateText(control.BSOACComponent, control.ACCaption);
                    else
                        control.ACCaptionTrans = control.ACCaption;
                }
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty ACCaptionTransProperty
            = DependencyProperty.Register("ACCaptionTrans", typeof(string), typeof(VBComboBox));
        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaptionTrans
        {
            get { return (string)GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly DependencyProperty ShowCaptionProperty
            = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBComboBox), new PropertyMetadata(true));
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        /// <summary>
        /// Gets the container for item override
        /// </summary>
        /// <returns>Returns a new VBComboBoxItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBComboBoxItem();
        }

        public static readonly DependencyProperty UpdateSourceTriggerProperty
                                = DependencyProperty.Register("UpdateSource", typeof(System.Windows.Data.UpdateSourceTrigger), typeof(VBComboBox), new PropertyMetadata(System.Windows.Data.UpdateSourceTrigger.Default));
        [Category("VBControl")]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return (UpdateSourceTrigger)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }


        #region Layout

        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly DependencyProperty WidthCaptionProperty
            = DependencyProperty.Register("WidthCaption", typeof(GridLength), typeof(VBComboBox), new PropertyMetadata(new GridLength(15, GridUnitType.Star)));

        /// <summary>
        /// Gets or sets the width of caption.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption
        {
            get { return (GridLength)GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthCaptionMax.
        /// </summary>
        public static readonly DependencyProperty WidthCaptionMaxProperty
            = DependencyProperty.Register("WidthCaptionMax", typeof(double), typeof(VBComboBox), new PropertyMetadata((double)150));
        /// <summary>
        /// Gets or sets the maximum width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die maximale Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaptionMax
        {
            get { return (double)GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContent.
        /// </summary>
        public static readonly DependencyProperty WidthContentProperty
            = DependencyProperty.Register("WidthContent", typeof(GridLength), typeof(VBComboBox), new PropertyMetadata(new GridLength(20, GridUnitType.Star)));
        
        /// <summary>
        /// Gets or sets the width of content.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthContent
        {
            get { return (GridLength)GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContentMax.
        /// </summary>
        public static readonly DependencyProperty WidthContentMaxProperty
            = DependencyProperty.Register("WidthContentMax", typeof(double), typeof(VBComboBox), new PropertyMetadata(Double.PositiveInfinity));


        /// <summary>
        /// Gets or sets the maximum width of content.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return (double)GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for WidthPadding.
        /// </summary>
        public static readonly DependencyProperty WidthPaddingProperty
            = DependencyProperty.Register("WidthPadding", typeof(GridLength), typeof(VBComboBox), new PropertyMetadata(new GridLength(0)));
        
        /// <summary>
        /// Gets or sets the width of padding.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return (GridLength)GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }
        #endregion


        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBComboBox), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
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

        #endregion

        #region Loaded-Event
        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        private TextBox _EditableTextBoxSite2;
        private FrameworkElement _PART_TakeCount;
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

            try
            {
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
                if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                {
                    this.Root().Messages.LogDebug("Error00003", "VBComboBox", VBContent);
                    return;
                }
                _ACTypeInfo = dcACTypeInfo;

                ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, VBComboBox.RightControlModeProperty);
                if ((valueSource == null)
                    || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))
                    || (dcRightControlMode < RightControlMode))
                {
                    RightControlMode = dcRightControlMode;
                }

                if (BSOACComponent != null)
                {
                    Binding binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBComboBox.ACCompInitStateProperty, binding);
                }
                //if (ContextACObject is IACComponent)
                //{
                //    Binding binding = new Binding();
                //    binding.Source = ContextACObject;
                //    binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                //    binding.Mode = BindingMode.OneWay;
                //    SetBinding(VBComboBox.ACUrlCmdMessageProperty, binding);
                //}

                // VBContent muß im XAML gestetzt sein
                System.Diagnostics.Debug.Assert(VBContent != "");

                if (Visibility == Visibility.Visible)
                {
                    if (_ACTypeInfo == null)
                        return;
                    // Beschriftung setzen, falls nicht im XAML definiert
                    if (string.IsNullOrEmpty(ACCaption))
                        ACCaptionTrans = _ACTypeInfo.ACCaption;
                    else
                        ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

                    string acAccess = "";
                    ACClassProperty sourceProperty = null;
                    VBSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
                    IACType dsACTypeInfo = null;
                    object dsSource = null;
                    string dsPath = "";
                    Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

                    if (sourceProperty != null && sourceProperty.IsStatic)
                    {
                        if (!sourceProperty.GetBindingForStaticProperty(ref dsSource, ref dsPath, ref dsRightControlMode))
                        {
                            this.Root().Messages.LogDebug("Error00004", "VBComboBox", VBSource + " " + VBContent);
                            return;
                        }
                        dsACTypeInfo = sourceProperty;
                    }
                    else if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                    {
                        if (!BSOACComponent.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                        {
                            this.Root().Messages.LogDebug("Error00004", "VBComboBox", VBSource + " " + VBContent);
                            //this.Root().Messages.Error(ContextACObject, "Error00004", "VBComboBox", VBSource, VBContent);
                            return;
                        }
                    }

                    if (!String.IsNullOrEmpty(VBAccess))
                        acAccess = VBAccess;
                    // Falls kein Access (IAccess) im BSO definiert ist, dann die globale ACQueryDefinition verwenden
                    if (string.IsNullOrEmpty(acAccess))
                    {
                        if (dsACTypeInfo != null)
                        {
                            _ = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
                        }
                        if (string.IsNullOrEmpty(acAccess))
                        {
                            if (dsACTypeInfo != null && dsACTypeInfo.ValueTypeACClass != null)
                            {
                                ACClass queryType = dsACTypeInfo.ValueTypeACClass.PrimaryNavigationquery();
                                if (queryType != null)
                                {
                                    ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, queryType, queryType.ACIdentifier, true);
                                    if (queryDef != null)
                                    {
                                        IACObjectWithInit initObject = ContextACObject as IACObjectWithInit;
                                        if (initObject == null)
                                            initObject = BSOACComponent;

                                        bool isObjectSet = ((dsPath.StartsWith("Database.") || (dsSource != null && dsSource is IACEntityObjectContext))
                                                        && (dsACTypeInfo is ACClassProperty && (dsACTypeInfo as ACClassProperty).GenericType == typeof(Microsoft.EntityFrameworkCore.DbSet<>).FullName));
                                        if (initObject != null && isObjectSet)
                                            ACAccess = queryDef.NewAccess("VBComboBox", initObject, dsSource as IACObject);
                                        else
                                            ACQueryDefinition = queryDef;
                                    }
                                }
                            }
                            else if (ContextACObject is IACContainer && String.IsNullOrEmpty(VBSource))
                            {
                                IACContainer container = ContextACObject as IACContainer;
                                if (container != null)
                                {
                                    if (container.ValueTypeACClass.ACKind == Global.ACKinds.TACEnum
                                        && container.ValueTypeACClass.ObjectType != null)
                                    {
                                        if (container.ValueTypeACClass.ACValueListForEnum == null)
                                        {
                                            VBSource = String.Format("\\!{0}(#{1}\\{1}#)", Const.MN_GetEnumList, container.ValueTypeACClass.ObjectType.AssemblyQualifiedName);
                                            if (!BSOACComponent.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                                                return;
                                        }
                                        else
                                        {
                                            dsSource = container.ValueTypeACClass.ACValueListForEnum;
                                            if (String.IsNullOrEmpty(dcPath))
                                                dcPath = Const.Value;
                                            if (String.IsNullOrEmpty(VBShowColumns))
                                                VBShowColumns = Const.ACCaptionPrefix;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ACAccess = ContextACObject.ACUrlCommand(acAccess) as IAccess;
                        }
                    }
                    // ansonsten ACQueryDefinition aus BSO verwenden
                    else
                    {
                        ACAccess = ContextACObject.ACUrlCommand(acAccess) as IAccess;
                    }


                    List<ACColumnItem> vbShowColumns = null;
                    if (ACQueryDefinition != null)
                        vbShowColumns = ACQueryDefinition.GetACColumns(this.VBShowColumns);
                    else
                        vbShowColumns = ACQueryDefinition.BuildACColumnsFromVBSource(this.VBShowColumns);

                    if ((vbShowColumns == null || !vbShowColumns.Any()) && (dsACTypeInfo == null || dsACTypeInfo.ObjectType != typeof(string)))
                    {
                        this.Root().Messages.LogDebug("Error00005", "VBComboBox", VBShowColumns + " " + VBContent);
                        //this.Root().Messages.Error(ContextACObject, "Error00005", "VBComboBox", VBShowColumns, VBContent);
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
                        SetBinding(ComboBox.ItemsSourceProperty, binding);
                    }
                    else
                    {
                        // dsSource = Reference auf BSOCompany-Instanz
                        // dsPath = "Database.MDCountryList"
                        // Liefert gefiltertes (ACQueryDefinition.ACFilterColumns) und 
                        // sortiertes (ACQueryDefinition.ACSortColumns) Ergebnis: 

                        // IEnumerable list = BSOCompany.Database.MDCountryList.ACSelect(ACQueryDefinition);
                        // Zukünftig, da die ...List-Eigenschaften in Database entfallen:
                        // IEnumerable list = BSOCompany.Database.MDCountry.ACSelect(ACQueryDefinition);


                        // TODO: Zwischenlösung
                        if (dsPath.StartsWith(Const.ContextDatabase + ".") || (dsSource != null && dsSource is IACEntityObjectContext))
                        {
                            //Type t = dsSource.GetType();
                            //PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                            //var lastPath = dsPath.Substring(9);
                            //var database = pi.GetValue(dsSource, null) as IACObject;
                            //var result = database.ACSelect(ACQueryDefinition, lastPath);
                            //this.ItemsSource = result;
                            if (NavSearchOnACAccess())
                            {
                                ACAccessComposite = new CompositeCollection(2);
                                CollectionContainer container = new CollectionContainer();
                                Binding binding = new Binding();
                                binding.Source = ACAccess;
                                binding.Path = new PropertyPath("NavObjectList");
                                binding.Mode = BindingMode.OneWay;
                                BindingOperations.SetBinding(container, CollectionContainer.CollectionProperty, binding);
                                ACAccessComposite.Add(container);

                                object selectedValue = ContextACObject.ACUrlCommand(VBContent);
                                if (selectedValue != null)
                                {
                                    ACAccessComposite.Add(selectedValue);
                                }

                                binding = new Binding();
                                binding.Source = this;
                                binding.Path = new PropertyPath("ACAccessComposite");
                                SetBinding(ComboBox.ItemsSourceProperty, binding);
                            }
                            else
                            {
                                if (dsSource != null && dsSource is IACEntityObjectContext)
                                {
                                    IQueryable result = (dsSource as IACEntityObjectContext).ACSelect(ACQueryDefinition, dsPath);
                                    Binding binding = new Binding();
                                    binding.Source = result.AsArrayList();
                                    SetBinding(ComboBox.ItemsSourceProperty, binding);
                                    //this.ItemsSource = result;
                                }
                                else
                                {
                                    Type t = dsSource.GetType();
                                    PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                                    var lastPath = dsPath.Substring(9);
                                    var database = pi.GetValue(dsSource, null) as IACObject;
                                    IQueryable result = database.ACSelect(ACQueryDefinition, lastPath);
                                    Binding binding = new Binding();
                                    binding.Source = result.AsArrayList();
                                    SetBinding(ComboBox.ItemsSourceProperty, binding);
                                    //this.ItemsSource = result;
                                }
                            }
                        }
                        else
                        {
                            if (ACAccess != null)
                            {
                                ACAccessComposite = new CompositeCollection(2);
                                CollectionContainer container = new CollectionContainer();
                                Binding binding = new Binding();
                                binding.Source = dsSource;
                                if (!string.IsNullOrEmpty(dsPath))
                                    binding.Path = new PropertyPath(dsPath);
                                binding.Mode = BindingMode.OneWay;
                                BindingOperations.SetBinding(container, CollectionContainer.CollectionProperty, binding);
                                ACAccessComposite.Add(container);

                                object selectedValue = ContextACObject.ACUrlCommand(VBContent);
                                if (selectedValue != null)
                                {
                                    ACAccessComposite.Add(selectedValue);
                                }

                                binding = new Binding();
                                binding.Source = this;
                                binding.Path = new PropertyPath("ACAccessComposite");
                                SetBinding(ComboBox.ItemsSourceProperty, binding);
                            }
                            else
                            {
                                // Listenbereich von Combobox füllen 
                                Binding binding = new Binding();
                                binding.Source = dsSource;
                                if (!string.IsNullOrEmpty(dsPath))
                                {
                                    binding.Path = new PropertyPath(dsPath);
                                }
                                SetBinding(ComboBox.ItemsSourceProperty, binding);
                            }
                        }
                    }

                    string textPath = "";
                    string selectedValuePath = null;
                    bool isACValueItem = false;
                    if (ItemTemplate == null && vbShowColumns != null)
                    {
                        DataTemplate dataTemplate = new DataTemplate();

                        FrameworkElementFactory factorySP = new FrameworkElementFactory(typeof(StackPanel));
                        factorySP.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                        //FrameworkElementFactory factorySP = new FrameworkElementFactory(typeof(Grid));

                        int columnCount = 0;
                        int maxColumnCount = vbShowColumns.Count();
                        if (String.IsNullOrEmpty(VBShowColumns) && maxColumnCount >= 3)
                            maxColumnCount = 3;
                        double maxColWidth = 1000 / maxColumnCount;
                        if (maxColWidth > 250)
                            maxColWidth = 250;
                        else if (maxColWidth < 50)
                            maxColWidth = 50;
                        //for (int i = 0; i < maxColumnCount; i++)
                        //{
                        //    FrameworkElementFactory factoryCol = new FrameworkElementFactory(typeof(ColumnDefinition));
                        //    factoryCol.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
                        //    factoryCol.SetValue(ColumnDefinition.MaxWidthProperty, maxColWidth);
                        //    factoryCol.SetValue(ColumnDefinition.MinWidthProperty, 50.0);
                        //    factorySP.AppendChild(factoryCol);
                        //}

                        foreach (var dataShowColumn in vbShowColumns)
                        {
                            IACType dsColACTypeInfo = dsACTypeInfo;
                            object dsColSource = null;
                            string dsColPath = "";
                            Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;

                            if (dcACTypeInfo.ObjectType.IsEnum)
                            {
                                dsColPath = dataShowColumn.PropertyName;
                            }
                            else if (!dsACTypeInfo.ACType.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                            {
                                if (dsColACTypeInfo.ObjectType == typeof(ACValueItem))
                                {
                                    isACValueItem = true;
                                    FrameworkElementFactory factoryTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                                    Binding binding1 = new Binding();
                                    binding1.Path = new PropertyPath(dataShowColumn.ACIdentifier);
                                    binding1.Mode = BindingMode.OneWay;
                                    factoryTextBlock.SetBinding(TextBlock.TextProperty, binding1);
                                    factoryTextBlock.SetValue(TextBlock.WidthProperty, maxColWidth);
                                    //factoryTextBlock.SetValue(Grid.ColumnProperty, columnCount);
                                    factorySP.AppendChild(factoryTextBlock);
                                    selectedValuePath = Const.Value;
                                    textPath = dataShowColumn.ACIdentifier;
                                    break;
                                }
                                continue;
                            }
                            else
                            {
                                if (dsColACTypeInfo.ValueTypeACClass.ACKind != Global.ACKinds.TACLRBaseTypes)
                                {
                                    if (dsColACTypeInfo.ValueTypeACClass.GetColumns(1).Count > 0)
                                        dsColPath += "." + dsColACTypeInfo.ValueTypeACClass.GetColumns(1).First().ACIdentifier;
                                }
                            }
                            if (columnCount == 0)
                                textPath = dsColPath;
                            if (dsColACTypeInfo != null && dsColACTypeInfo.ObjectType == typeof(bool))
                            {
                                FrameworkElementFactory factoryTextBlock = new FrameworkElementFactory(typeof(CheckBox));

                                Binding binding1 = new Binding();
                                binding1.Path = new PropertyPath(dsColPath);
                                binding1.Mode = BindingMode.OneWay;
                                factoryTextBlock.SetBinding(CheckBox.IsCheckedProperty, binding1);
                                factoryTextBlock.SetValue(CheckBox.IsEnabledProperty, false);
                                factoryTextBlock.SetValue(CheckBox.WidthProperty, maxColWidth);
                                //factoryTextBlock.SetValue(Grid.ColumnProperty, columnCount);

                                factorySP.AppendChild(factoryTextBlock);
                            }
                            else
                            {
                                FrameworkElementFactory factoryTextBlock = new FrameworkElementFactory(typeof(TextBlock));

                                Binding binding1 = new Binding();
                                binding1.Path = new PropertyPath(dsColPath);
                                binding1.Mode = BindingMode.OneWay;
                                factoryTextBlock.SetBinding(TextBlock.TextProperty, binding1);
                                factoryTextBlock.SetValue(TextBlock.WidthProperty, maxColWidth);
                                //factoryTextBlock.SetValue(Grid.ColumnProperty, columnCount);

                                factorySP.AppendChild(factoryTextBlock);
                            }
                            columnCount++;
                            if (columnCount >= maxColumnCount)
                                break;
                        }
                        dataTemplate.VisualTree = factorySP;
                        ItemTemplate = dataTemplate;
                    }
                    else if (vbShowColumns != null && vbShowColumns.Count > 0)
                    {
                        IACType dsColACTypeInfo = dsACTypeInfo;
                        object dsColSource = null;
                        string dsColPath = "";
                        Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
                        var dataShowColumn = vbShowColumns.FirstOrDefault();

                        if (dcACTypeInfo.ObjectType.IsEnum)
                        {
                            dsColPath = dataShowColumn.PropertyName;
                        }
                        if (dsACTypeInfo.ACType.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                        {
                            if (dsColACTypeInfo.ValueTypeACClass.ACKind != Global.ACKinds.TACLRBaseTypes)
                            {
                                if (dsColACTypeInfo.ValueTypeACClass.GetColumns(1).Count > 0)
                                    dsColPath += "." + dsColACTypeInfo.ValueTypeACClass.GetColumns(1).First().ACIdentifier;
                            }
                        }
                        textPath = dsColPath;
                    }

                    string currentTextPath = GetValue(TextSearch.TextPathProperty) as string;
                    if (String.IsNullOrEmpty(currentTextPath))
                        SetValue(TextSearch.TextPathProperty, textPath);

                    // Sonderbehandlung enum, da hier immer eine IEnumerable<ACValueItem> als Datasource generiert wird
                    if (dcACTypeInfo.ObjectType.IsEnum || isACValueItem)
                    {
                        SetValue(ComboBox.SelectedValuePathProperty, Const.Value);

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource;
                        binding2.Path = new PropertyPath(dcPath);
                        binding2.Mode = BindingMode.TwoWay;
                        binding2.NotifyOnSourceUpdated = true;
                        binding2.NotifyOnTargetUpdated = true;
                        binding2.UpdateSourceTrigger = UpdateSourceTrigger;
                        //SetBinding(ComboBox.SelectedItemProperty, binding2);
                        SetBinding(ComboBox.SelectedValueProperty, binding2);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(selectedValuePath))
                            SetValue(ComboBox.SelectedValuePathProperty, selectedValuePath);

                        Binding binding2 = new Binding();
                        binding2.Source = dcSource;
                        binding2.Path = new PropertyPath(dcPath);
                        if (dcACTypeInfo is ACClassProperty)
                            binding2.Mode = (dcACTypeInfo as ACClassProperty).IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                        binding2.NotifyOnSourceUpdated = true;
                        binding2.NotifyOnTargetUpdated = true;
                        binding2.UpdateSourceTrigger = UpdateSourceTrigger;
                        if (!String.IsNullOrEmpty(VBValidation))
                            binding2.ValidationRules.Add(new VBValidationRule(ValidationStep.ConvertedProposedValue, true, ContextACObject, VBContent, VBValidation));

                        //Binding binding3 = new Binding();
                        //binding3.Source = binding2.Source;
                        //binding3.Path = binding2.Path;
                        //binding3.Mode = BindingMode.OneWay;
                        //binding2.FallbackValue = SetBinding(ComboBox.TextProperty, binding3);

                        SetBinding(ComboBox.SelectedItemProperty, binding2);
                    }
                }
                //if (IsEnabled)
                //{
                //    if (RightControlMode < Global.ControlModes.Enabled)
                //    {
                //        IsEnabled = false;
                //    }
                //    else
                //    {
                //        UpdateControlMode();
                //    }
                //}

                if (AutoFocus)
                {
                    Focus();
                    //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBCheckBox", "InitVBControl(10)", msg);
            }
        }

        bool _Loaded = false;
        void VBComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                Binding boundedValue = BindingOperations.GetBinding(this, ComboBox.ItemsSourceProperty);
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
                        this.Root().Messages.LogDebug("VBComboBox", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
                BSOACComponent.RemoveWPFRef(this.GetHashCode());

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
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());

            _Initialized = false;
            this.SourceUpdated -= VBComboBox_SourceUpdated;
            this.TargetUpdated -= VBComboBox_TargetUpdated;
            Loaded -= VBComboBox_Loaded;
            Unloaded -= VBComboBox_Unloaded;
            if (_EditableTextBoxSite2 != null)
            {
                _EditableTextBoxSite2.TextChanged -= new TextChangedEventHandler(OnEditableTextBoxTextChanged2);
                _EditableTextBoxSite2.LostKeyboardFocus -= _EditableTextBoxSite2_LostKeyboardFocus;
            }
            _EditableTextBoxSite2 = null;
            if (_PART_TakeCount != null)
                _PART_TakeCount.MouseLeftButtonDown -= _PART_TakeCount_MouseLeftButtonDown;
            _PART_TakeCount = null;
            _ACTypeInfo = null;
            ACAccess = null;

            this.ClearAllBindings();
            this.ItemsSource = null;
            ACAccessComposite = null;
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

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if ((ContextACObject != null) && e.ChangedButton == MouseButton.Right)
            {
                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                e.Handled = true;
            }
            base.OnPreviewMouseRightButtonDown(e);
        }

        private void VBComboBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
            if (e.Property == ComboBox.SelectedItemProperty)
            {
                if (ACAccessComposite != null)
                {
                    if (_ACAccessCompositeChanging)
                        return;
                    BindingExpression expression = BindingOperations.GetBindingExpression(this, ComboBox.SelectedItemProperty);
                    if (expression == null)
                        return;
                    try
                    {
                        _ACAccessCompositeChanging = true;
                        //object addedItem = expression.ResolvedSource;
                        object addedItem = null;
                        if (expression.ResolvedSource != null)
                            addedItem = expression.ResolvedSource.GetValue(expression.ResolvedSourcePropertyName);
                        object lastSelectedItem = ACAccessComposite.Count > 1 ? ACAccessComposite[1] : null;
                        if (lastSelectedItem != null)
                        {
                            if (lastSelectedItem != addedItem)
                            {
                                ACAccessComposite.Remove(lastSelectedItem);
                                if (addedItem != null)
                                {
                                    ACAccessComposite.Add(addedItem);
                                    expression.UpdateTarget();
                                }
                            }
                        }
                        else if (addedItem != null)
                        {
                            ACAccessComposite.Add(addedItem);
                            expression.UpdateTarget();
                        }
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBCheckBox", "VBComboBox_TargetUpdated", msg);
                    }
                    finally
                    {
                        _ACAccessCompositeChanging = false;
                    }
                }
            }
        }

        private void VBComboBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (ACAccessComposite != null)
            {
                if (_ACAccessCompositeChanging)
                {
                    base.OnSelectionChanged(e);
                    return;
                }
                try
                {
                    _ACAccessCompositeChanging = true;
                    object addedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
                    object removedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                    object lastSelectedItem = ACAccessComposite.Count > 1 ? ACAccessComposite[1] : null;
                    if (lastSelectedItem != null)
                    {
                        if (lastSelectedItem != addedItem)
                        {
                            ACAccessComposite.Remove(lastSelectedItem);
                            if (addedItem != null)
                                ACAccessComposite.Add(addedItem);
                        }
                    }
                    else if (addedItem != null)
                        ACAccessComposite.Add(addedItem);
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBCheckBox", "OnSelectionChanged", msg);
                }
                finally
                {
                    _ACAccessCompositeChanging = false;
                }
            }
            base.OnSelectionChanged(e);
        }

        protected virtual void OnEditableTextBoxTextChanged2(object sender, TextChangedEventArgs e)
        {
            if (!this.IsEditable)
            {
                return;
            }
        }

        void _EditableTextBoxSite2_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!this.IsEditable)
            {
                return;
            }
            if (this.SelectedItem == null
                && this.ACAccessComposite != null
                && this.ACAccessComposite.Count == 1
                && ACAccess != null
                && !String.IsNullOrEmpty(this._EditableTextBoxSite2.Text)
                && ACAccess.NavACQueryDefinition.TakeCount > 0)
            {
                object foundItem = ACAccess.OneTimeSearchFirstOrDefault(this._EditableTextBoxSite2.Text);
                if (foundItem != null)
                {
                    try
                    {
                        _ACAccessCompositeChanging = true;
                        ACAccessComposite.Add(foundItem);
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBCheckBox", "_EditableTextBoxSite2_LostKeyboardFocus", msg);
                    }
                    finally
                    {
                        _ACAccessCompositeChanging = false;
                    }
                    this.SelectedItem = foundItem;
                }
                else
                {
                    this._EditableTextBoxSite2.Text = null;
                }
            }
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.Delete)
                {
                    SelectedIndex = -1;
                    //ACObject.ValueChanged(VBContent);

                }
            }
            else if (e.Key == Key.F3)
            {
                Filter();
            }
            else if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
            base.OnKeyUp(e);
        }

        #endregion


        #region IDataField Members

        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBComboBox));

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
        /// <summary>
        /// Represents the dependency property for VBSource.
        /// </summary>
        public static readonly DependencyProperty VBSourceProperty
            = DependencyProperty.Register("VBSource", typeof(string), typeof(VBComboBox));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get { return (string)GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        private string _DataShowColumns;
        /// <summary>
        /// Determines which properties of bounded object will be shown in VBComboBox. XAML sample: VBShowColumns="PropName1,PropName2,PropName3"
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
        /// Determines which columns will be disabled in VBComboBox.
        /// </summary>
        [Category("VBControl")]
        public string VBDisabledColumns
        {
            get;
            set;
        }

        private string _DataChilds;
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
        /// Represents the dependency property for VBAccess.
        /// </summary>
        public static readonly DependencyProperty VBAccessProperty
            = DependencyProperty.Register("VBAccess", typeof(string), typeof(VBComboBox));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBAccess
        {
            get { return (string)GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
        }

        //ACQueryDefinition _ACQueryDefinition = null;
        ///// <summary>
        ///// Gets the ACQueryDefinition.
        ///// </summary>
        //public ACQueryDefinition ACQueryDefinition
        //{
        //    get
        //    {
        //        if (ACAccess != null)
        //            return ACAccess.NavACQueryDefinition;
        //        return _ACQueryDefinition;
        //    }
        //}

        //private static readonly DependencyPropertyKey ACQueryDefinitionPropertyKey = DependencyProperty.RegisterReadOnly("ACQueryDefinition", typeof(ACQueryDefinition), typeof(VBComboBox), new PropertyMetadata(null));
        //public static readonly DependencyProperty ACQueryDefinitionProperty = ACQueryDefinitionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ACQueryDefinitionProperty = DependencyProperty.Register("ACQueryDefinition", typeof(ACQueryDefinition), typeof(VBComboBox), new PropertyMetadata(null));

        public ACQueryDefinition ACQueryDefinition
        {
            get { return (ACQueryDefinition)GetValue(ACQueryDefinitionProperty); }
            private set { SetValue(ACQueryDefinitionProperty, value); }
        }


        //private static readonly DependencyPropertyKey ACAccessPropertyKey = DependencyProperty.RegisterReadOnly("ACAccess", typeof(IAccess), typeof(VBComboBox), new PropertyMetadata(null));
        //public static readonly DependencyProperty ACAccessProperty = ACAccessPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ACAccessProperty = DependencyProperty.Register("ACAccess", typeof(IAccess), typeof(VBComboBox), new PropertyMetadata(null));
        public IAccess ACAccess
        {
            get { return (IAccess)GetValue(ACAccessProperty); }
            private set
            {
                SetValue(ACAccessProperty, value);
                if (value != null)
                {
                    ACQueryDefinition = value.NavACQueryDefinition;
                    if (_PART_TakeCount != null)
                        _PART_TakeCount.Visibility = Visibility.Visible;
                }
                else if (_PART_TakeCount != null)
                    _PART_TakeCount.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Represents the dependency property for ACAccessComposite.
        /// </summary>
        public static readonly DependencyProperty ACAccessCompositeProperty = DependencyProperty.Register("ACAccessComposite", typeof(CompositeCollection), typeof(VBComboBox));

        /// <summary>
        /// Gets or sets the ACAccessComposite.
        /// </summary>
        public CompositeCollection ACAccessComposite
        {
            get { return (CompositeCollection)GetValue(ACAccessCompositeProperty); }
            set { SetValue(ACAccessCompositeProperty, value); }
        }

        private bool _ACAccessCompositeChanging = false;


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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBComboBox),
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
                typeof(ACInitState), typeof(VBComboBox),
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
            VBComboBox thisControl = dependencyObject as VBComboBox;
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
                List<IACObject> acContentList = new List<IACObject>();
                ACValueItem acTypedValue = new ACValueItem("", SelectedItem, _ACTypeInfo);
                acContentList.Add(acTypedValue);

                if (!String.IsNullOrEmpty(VBContent))
                {
                    int pos1 = VBContent.LastIndexOf('\\');
                    if (pos1 != -1)
                    {
                        acContentList.Add(ContextACObject.ACUrlCommand(VBContent.Substring(0, pos1)) as IACObject);
                    }
                }

                return acContentList;
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
        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
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
            if (controlMode != ControlMode)
                ControlMode = controlMode;

            if (controlMode >= Global.ControlModes.Enabled)
                this.IsTabStop = true;
            else
                this.IsTabStop = false;

            if (controlMode == Global.ControlModes.Collapsed)
            {
                if (this.Visibility != System.Windows.Visibility.Collapsed)
                    this.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (this.Visibility != System.Windows.Visibility.Hidden)
                    this.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                if (this.Visibility != System.Windows.Visibility.Visible)
                    this.Visibility = System.Windows.Visibility.Visible;
                if (controlMode == Global.ControlModes.Disabled)
                {
                    if (!IsReadOnly)
                        IsReadOnly = true;
                    if (IsEnabled)
                        IsEnabled = false;
                }
                else
                {
                    if (IsReadOnly)
                        IsReadOnly = false;
                    if (!IsEnabled)
                        IsEnabled = true;
                }
            }
            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        public bool AutoFocus { get; set; }

        IACType _ACTypeInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
            }
        }


        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBComboBox));
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


        /// <summary>
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly DependencyProperty RightControlModeProperty
            = DependencyProperty.Register("RightControlMode", typeof(Global.ControlModes), typeof(VBComboBox));

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes RightControlMode
        {
            get { return (Global.ControlModes)GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

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

        #region ACMethod
        /// <summary>
        /// Modifies the content.
        /// </summary>
        [ACMethodInteraction("", "en{'Modify'}de{'Bearbeiten'}", 100, false)]
        public virtual void Modify()
        {
            if (!ACContentList.Any())
                return;

            IACObject firstObject = ACContentList.First();
            if (firstObject == null)
                return;

            if (firstObject is ACValueItem)
                firstObject = (firstObject as ACValueItem).Value as IACObject;

            if (firstObject == null)
                return;

            if (firstObject.ACType == null)
                return;


            ACClass acClass = firstObject.ACType.ValueTypeACClass.ManagingBSO;
            if (acClass == null)
            {
                IACObject firstObjectTmp = firstObject;
                while (firstObjectTmp != null)
                {
                    if (firstObjectTmp.ACType != null && firstObjectTmp.ACType.ValueTypeACClass != null)
                        acClass = firstObjectTmp.ACType.ValueTypeACClass.ManagingBSO;
                    if (acClass != null)
                        break;
                    firstObjectTmp = firstObjectTmp.ParentACObject;
                }
            }
            if (acClass == null)
                return;


            if ((ContextACObject.ACType.ACKind == Global.ACKinds.TACBSO) || (ContextACObject.ACType.ACKind == Global.ACKinds.TACBSOGlobal))
            {
                string filterColumn = firstObject.ACType.ValueTypeACClass.ACFilterColumns.Split(',').First();
                ACValueItem acTypedValue = ACContentList.First() as ACValueItem;

                string acUrl = Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + acClass.ACIdentifier;

                if (acTypedValue != null && !string.IsNullOrEmpty(filterColumn))
                {
                    IACObject value = acTypedValue.Value as IACObject;
                    ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(acUrl);
                    acMethod.ParameterValueList["AutoFilter"] = value.ACUrlCommand(filterColumn).ToString();

                    this.Root().RootPageWPF.StartBusinessobject(acUrl, acMethod.ParameterValueList);
                }
                else
                {
                    this.Root().RootPageWPF.StartBusinessobject(acUrl, null);
                }
            }
        }

        /// <summary>
        /// Determines is enabled modify.
        /// </summary>
        /// <returns>Returns true if modify enabled otherwise false.</returns>
        public virtual bool IsEnabledModify()
        {
            if (!ACContentList.Any())
                return false;

            IACObject firstObject = ACContentList.First();
            if (firstObject == null)
                return false;

            if (firstObject is ACValueItem)
                firstObject = (firstObject as ACValueItem).Value as IACObject;

            if (firstObject == null)
                return false;

            if (firstObject.ACType == null)
                return false;

            ACClass acClass = firstObject.ACType.ValueTypeACClass.ManagingBSO;
            if (acClass == null)
                return false;

            if (acClass.GetRight(acClass) != Global.ControlModes.Enabled)
                return false;

            return true;
        }

        /// <summary>
        /// Filters the content.
        /// </summary>
        [ACMethodInteraction("", "en{'Filter'}de{'Filter'}", 101, false)]
        public void Filter()
        {
            if (ACAccess == null)
                return;
            if (ACAccess.ShowACQueryDialog())
            {
                NavSearchOnACAccess(true);
            }
        }

        //protected void NavSerah

        /// <summary>
        /// Determines is enabled filter.
        /// </summary>
        /// <returns>Returns true if filter enabled, otherwise false.</returns>
        public bool IsEnabledFilter()
        {
            return ACAccess != null && BSOACComponent != null;
        }

        private bool NavSearchOnACAccess(bool refreshAccessComposite = false)
        {
            if (ACAccess == null)
                return false;
            bool navSearchExecuted = false;
            try
            {
                IACBSO acComponent = ContextACObject as IACBSO;
                if (acComponent == null)
                    acComponent = BSOACComponent;
                if (acComponent != null)
                    navSearchExecuted = acComponent.ExecuteNavSearch(ACAccess);
                if (!navSearchExecuted)
                    navSearchExecuted = ACAccess.NavSearch();
                if (navSearchExecuted && refreshAccessComposite && ACAccessComposite != null)
                {
                    CollectionContainer container = this.ACAccessComposite[0] as CollectionContainer;
                    if (container != null)
                    {
                        BindingExpression expression2 = BindingOperations.GetBindingExpression(container, CollectionContainer.CollectionProperty);
                        if (expression2 != null)
                            expression2.UpdateTarget();
                    }
                }
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("VBConboBox", "NavSearchOnACAccess", e.Message);
            }
            return navSearchExecuted;
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
