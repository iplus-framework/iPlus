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
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia;
using System.Collections;
using System.Collections.ObjectModel;
using gip.ext.designer.avui;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;

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
        // Pseudo classes for Avalonia to replace x:Null comparison
        public static readonly StyledProperty<bool> HasSelectedValueProperty =
            AvaloniaProperty.Register<VBComboBox, bool>(nameof(HasSelectedValue), false);

        public bool HasSelectedValue
        {
            get { return GetValue(HasSelectedValueProperty); }
            private set { SetValue(HasSelectedValueProperty, value); }
        }

        // Define pseudo classes - using string-based pseudo classes for Avalonia
        private const string HasValuePseudoClass = ":has-value";
        private const string NoValuePseudoClass = ":no-value";

        public VBComboBox()
        {
            VisibilityFilterRow = false;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.GetObservable(SelectedItemProperty).Subscribe(_ => OnSelectedItemChanged());
            this.GetObservable(SelectedValueProperty).Subscribe(_ => OnSelectedValueChanged());
        }

        private void OnSelectedValueChanged()
        {
            var selectedValue = SelectedValue;
            var hasValue = selectedValue != null;
            HasSelectedValue = hasValue;
            
            // Update pseudo classes
            PseudoClasses.Set(HasValuePseudoClass, hasValue);
            PseudoClasses.Set(NoValuePseudoClass, !hasValue);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _EditableTextBoxSite2 = e.NameScope.Find("PART_EditableTextBox") as TextBox;
            if (_EditableTextBoxSite2 != null)
            {
                _EditableTextBoxSite2.GetObservable(TextBox.TextProperty).Subscribe(_ => OnEditableTextBoxTextChanged2());
                _EditableTextBoxSite2.LostFocus += _EditableTextBoxSite2_LostFocus;
            }
            _PART_TakeCount = e.NameScope.Find("PART_TakeCount") as Control;
            if (_PART_TakeCount != null)
                _PART_TakeCount.PointerPressed += _PART_TakeCount_PointerPressed;

            InitVBControl();
        }

        private void _PART_TakeCount_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (ACAccess != null && ACAccess.NavACQueryDefinition != null)
            {
                var clickCount = e.ClickCount;
                if (clickCount > 1)
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

        #region Additional Styled Properties

        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty
            = AvaloniaProperty.Register<VBComboBox, Global.ControlModes>(nameof(ControlMode));

        public Global.ControlModes ControlMode
        {
            get { return GetValue(ControlModeProperty); }
            set { SetValue(ControlModeProperty, value); }
        }

        public static readonly StyledProperty<bool> VisibilityFilterRowProperty
            = AvaloniaProperty.Register<VBComboBox, bool>(nameof(VisibilityFilterRow));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool VisibilityFilterRow
        {
            get { return GetValue(VisibilityFilterRowProperty); }
            set { SetValue(VisibilityFilterRowProperty, value); }
        }

        public static readonly StyledProperty<string> ACCaptionProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(ACCaption));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        public static readonly StyledProperty<string> ACCaptionTransProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(ACCaptionTrans));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaptionTrans
        {
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowCaptionProperty
            = AvaloniaProperty.Register<VBComboBox, bool>(nameof(ShowCaption), true);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowCaption
        {
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new ComboBoxItem(); // Use Avalonia's ComboBoxItem directly
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<ComboBoxItem>(item, out recycleKey);
        }

        public static readonly StyledProperty<UpdateSourceTrigger> UpdateSourceTriggerProperty
            = AvaloniaProperty.Register<VBComboBox, UpdateSourceTrigger>(nameof(UpdateSourceTrigger), UpdateSourceTrigger.Default);

        [Category("VBControl")]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        #region Layout

        public static readonly StyledProperty<GridLength> WidthCaptionProperty
            = AvaloniaProperty.Register<VBComboBox, GridLength>(nameof(WidthCaption), new GridLength(15, GridUnitType.Star));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption
        {
            get { return GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        public static readonly StyledProperty<double> WidthCaptionMaxProperty
            = AvaloniaProperty.Register<VBComboBox, double>(nameof(WidthCaptionMax), 150.0);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaptionMax
        {
            get { return GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        public static readonly StyledProperty<GridLength> WidthContentProperty
            = AvaloniaProperty.Register<VBComboBox, GridLength>(nameof(WidthContent), new GridLength(20, GridUnitType.Star));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthContent
        {
            get { return GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        public static readonly StyledProperty<double> WidthContentMaxProperty
            = AvaloniaProperty.Register<VBComboBox, double>(nameof(WidthContentMax), Double.PositiveInfinity);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }

        public static readonly StyledProperty<GridLength> WidthPaddingProperty
            = AvaloniaProperty.Register<VBComboBox, GridLength>(nameof(WidthPadding), new GridLength(0));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }
        #endregion

        public static readonly AttachedProperty<bool> DisableContextMenuProperty =
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBComboBox>();

        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        #endregion

        #region Loaded-Event

        protected bool _Initialized = false;
        private TextBox _EditableTextBoxSite2;
        private Control _PART_TakeCount;
        bool _Loaded = false;

        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (DisableContextMenu)
                ContextFlyout = null;

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

                // Set RightControlMode if not locally set
                if (dcRightControlMode < RightControlMode)
                {
                    RightControlMode = dcRightControlMode;
                }

                if (BSOACComponent != null)
                {
                    var binding = new Binding
                    {
                        Source = BSOACComponent,
                        Path = Const.InitState,
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(ACCompInitStateProperty, binding);
                }

                System.Diagnostics.Debug.Assert(VBContent != "");

                if (IsVisible)
                {
                    if (_ACTypeInfo == null)
                        return;
                    // Set caption if not defined in XAML
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
                            return;
                        }
                    }

                    if (!String.IsNullOrEmpty(VBAccess))
                        acAccess = VBAccess;
                    // If no Access (IAccess) is defined in BSO, use global ACQueryDefinition
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
                    // Otherwise use ACQueryDefinition from BSO
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
                        this.Bind(ComboBox.ItemsSourceProperty, binding);
                    }
                    else
                    {
                        if (dsPath.StartsWith(Const.ContextDatabase + ".") || (dsSource != null && dsSource is IACEntityObjectContext))
                        {
                            if (NavSearchOnACAccess())
                            {
                                ACAccessComposite = new ObservableCollection<object>();
                                
                                var binding = new Binding
                                {
                                    Source = ACAccess,
                                    Path = "NavObjectList",
                                    Mode = BindingMode.OneWay
                                };
                                // In Avalonia, we handle composite collections differently
                                // For now, just use direct binding to ACAccess.NavObjectList
                                this.Bind(ItemsSourceProperty, binding);

                                object selectedValue = ContextACObject.ACUrlCommand(VBContent);
                                if (selectedValue != null)
                                {
                                    ACAccessComposite.Add(selectedValue);
                                }
                            }
                            else
                            {
                                if (dsSource != null && dsSource is IACEntityObjectContext)
                                {
                                    IQueryable result = (dsSource as IACEntityObjectContext).ACSelect(ACQueryDefinition, dsPath);
                                    var binding = new Binding
                                    {
                                        Source = result.AsArrayList()
                                    };
                                    this.Bind(ItemsSourceProperty, binding);
                                }
                                else
                                {
                                    Type t = dsSource.GetType();
                                    PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                                    var lastPath = dsPath.Substring(9);
                                    var database = pi.GetValue(dsSource, null) as IACObject;
                                    IQueryable result = database.ACSelect(ACQueryDefinition, lastPath);
                                    var binding = new Binding
                                    {
                                        Source = result.AsArrayList()
                                    };
                                    this.Bind(ItemsSourceProperty, binding);
                                }
                            }
                        }
                        else
                        {
                            if (ACAccess != null)
                            {
                                ACAccessComposite = new ObservableCollection<object>();
                                
                                var binding = new Binding
                                {
                                    Source = dsSource,
                                    Mode = BindingMode.OneWay
                                };
                                if (!string.IsNullOrEmpty(dsPath))
                                    binding.Path = dsPath;
                                // For Avalonia, bind directly to the source
                                this.Bind(ItemsSourceProperty, binding);

                                object selectedValue = ContextACObject.ACUrlCommand(VBContent);
                                if (selectedValue != null)
                                {
                                    ACAccessComposite.Add(selectedValue);
                                }
                            }
                            else
                            {
                                // Fill list area of combobox
                                var binding = new Binding
                                {
                                    Source = dsSource
                                };
                                if (!string.IsNullOrEmpty(dsPath))
                                {
                                    binding.Path = dsPath;
                                }
                                this.Bind(ItemsSourceProperty, binding);
                            }
                        }
                    }

                    string textPath = "";
                    string selectedValuePath = null;
                    bool isACValueItem = false;
                    if (ItemTemplate == null && vbShowColumns != null)
                    {
                        DataTemplate dataTemplate = new DataTemplate();

                        var template = new FuncDataTemplate<object>((value, namescope) =>
                        {
                            StackPanel factorySP = new StackPanel();
                            factorySP.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                            int columnCount = 0;
                            int maxColumnCount = vbShowColumns.Count();
                            if (String.IsNullOrEmpty(VBShowColumns) && maxColumnCount >= 3)
                                maxColumnCount = 3;
                            double maxColWidth = 1000 / maxColumnCount;
                            if (maxColWidth > 250)
                                maxColWidth = 250;
                            else if (maxColWidth < 50)
                                maxColWidth = 50;

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
                                        TextBlock factoryTextBlock = new TextBlock();
                                        Binding binding1 = new Binding();
                                        binding1.Path = dataShowColumn.ACIdentifier;
                                        binding1.Mode = BindingMode.OneWay;
                                        factoryTextBlock.Bind(TextBlock.TextProperty, binding1);
                                        factoryTextBlock.SetValue(TextBlock.WidthProperty, maxColWidth);
                                        //factoryTextBlock.SetValue(Grid.ColumnProperty, columnCount);
                                        factorySP.Children.Add(factoryTextBlock);
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
                                    CheckBox factoryTextBlock = new CheckBox();

                                    Binding binding1 = new Binding();
                                    binding1.Path = dsColPath;
                                    binding1.Mode = BindingMode.OneWay;
                                    factoryTextBlock.Bind(CheckBox.IsCheckedProperty, binding1);
                                    factoryTextBlock.SetValue(CheckBox.IsEnabledProperty, false);
                                    factoryTextBlock.SetValue(CheckBox.WidthProperty, maxColWidth);
                                    factorySP.Children.Add(factoryTextBlock);
                                }
                                else
                                {
                                    TextBlock factoryTextBlock = new TextBlock();

                                    Binding binding1 = new Binding();
                                    binding1.Path = dsColPath;
                                    binding1.Mode = BindingMode.OneWay;
                                    factoryTextBlock.Bind(TextBlock.TextProperty, binding1);
                                    factoryTextBlock.SetValue(TextBlock.WidthProperty, maxColWidth);
                                    factorySP.Children.Add(factoryTextBlock);
                                }
                                columnCount++;
                                if (columnCount >= maxColumnCount)
                                    break;
                            }
                            return factorySP;
                        } 
                        );
                        ItemTemplate = dataTemplate;
                    }
                    // Handle display columns
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

                    string currentTextPath = GetValue(TextSearch.TextProperty) as string;
                    if (String.IsNullOrEmpty(currentTextPath))
                        SetValue(TextSearch.TextProperty, textPath);

                    // Special handling for enum, as here always an IEnumerable<ACValueItem> is generated as data source
                    if (dcACTypeInfo.ObjectType.IsEnum || isACValueItem)
                    {
                        this.SelectedValueBinding = new Binding(Const.Value);

                        var binding2 = new Binding
                        {
                            Source = dcSource,
                            Path = dcPath,
                            Mode = BindingMode.TwoWay
                        };
                        this.Bind(SelectedValueProperty, binding2);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(selectedValuePath))
                            this.SelectedValueBinding = new Binding(selectedValuePath);

                        var binding2 = new Binding
                        {
                            Source = dcSource,
                            Path = dcPath,
                            Mode = (dcACTypeInfo is ACClassProperty acProp && acProp.IsInput) ? BindingMode.TwoWay : BindingMode.OneWay
                        };

                        this.Bind(SelectedItemProperty, binding2);
                    }
                }

                if (AutoFocus)
                {
                    Focus();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBComboBox", "InitVBControl(10)", msg);
            }
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                var boundedValue = this.GetBindingObservable(ItemsSourceProperty);
                // Handle bound object reference for WPF compatibility
            }
            _Loaded = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
                BSOACComponent.RemoveWPFRef(this.GetHashCode());

            _Loaded = false;
        }

        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());

            _Initialized = false;
            if (_EditableTextBoxSite2 != null)
            {
                _EditableTextBoxSite2.LostFocus -= _EditableTextBoxSite2_LostFocus;
            }
            _EditableTextBoxSite2 = null;
            if (_PART_TakeCount != null)
                _PART_TakeCount.PointerPressed -= _PART_TakeCount_PointerPressed;
            _PART_TakeCount = null;
            _ACTypeInfo = null;
            ACAccess = null;

            this.ClearAllBindings();
            ItemsSource = null;
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

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if ((ContextACObject != null) && e.InitialPressMouseButton == MouseButton.Right)
            {
                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open(this);
                }
                e.Handled = true;
            }
            base.OnPointerReleased(e);
        }

        private void OnSelectedItemChanged()
        {
            UpdateControlMode();
            if (ACAccessComposite != null)
            {
                if (_ACAccessCompositeChanging)
                    return;
                
                try
                {
                    _ACAccessCompositeChanging = true;
                    object addedItem = SelectedItem;
                    object lastSelectedItem = ACAccessComposite.Count > 1 ? ACAccessComposite[1] : null;
                    if (lastSelectedItem != null)
                    {
                        if (lastSelectedItem != addedItem)
                        {
                            ACAccessComposite.Remove(lastSelectedItem);
                            if (addedItem != null)
                            {
                                ACAccessComposite.Add(addedItem);
                            }
                        }
                    }
                    else if (addedItem != null)
                    {
                        ACAccessComposite.Add(addedItem);
                    }
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBComboBox", "OnSelectedItemChanged", msg);
                }
                finally
                {
                    _ACAccessCompositeChanging = false;
                }
            }
        }

        protected virtual void OnEditableTextBoxTextChanged2()
        {
            // In Avalonia ComboBox, IsEditable property doesn't exist the same way
            // We'll check if we can edit by checking if _EditableTextBoxSite2 is not null
            if (_EditableTextBoxSite2 == null)
            {
                return;
            }
        }

        void _EditableTextBoxSite2_LostFocus(object sender, RoutedEventArgs e)
        {
            // In Avalonia ComboBox, IsEditable property doesn't exist the same way
            if (_EditableTextBoxSite2 == null)
            {
                return;
            }
            if (SelectedItem == null
                && ACAccessComposite != null
                && ACAccessComposite.Count == 1
                && ACAccess != null
                && !String.IsNullOrEmpty(_EditableTextBoxSite2.Text)
                && ACAccess.NavACQueryDefinition.TakeCount > 0)
            {
                object foundItem = ACAccess.OneTimeSearchFirstOrDefault(_EditableTextBoxSite2.Text);
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
                            datamodel.Database.Root.Messages.LogException("VBComboBox", "_EditableTextBoxSite2_LostFocus", msg);
                    }
                    finally
                    {
                        _ACAccessCompositeChanging = false;
                    }
                    SelectedItem = foundItem;
                }
                else
                {
                    _EditableTextBoxSite2.Text = null;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                if (e.Key == Key.Delete)
                {
                    SelectedIndex = -1;
                }
            }
            else if (e.Key == Key.F3)
            {
                Filter();
            }
            else if (e.Key == Key.Enter)
            {
                var next = KeyboardNavigationHandler.GetNext(this, NavigationDirection.Next);
                if (next != null)
                {
                    next.Focus();
                }
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }

        #endregion

        #region IDataField Members

        public static readonly StyledProperty<string> VBContentProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(VBContent));

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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return Name; }
            set { Name = value; }
        }
        #endregion

        #region IVBSource Members

        public static readonly StyledProperty<string> VBSourceProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(VBSource));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get { return GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        private string _DataShowColumns;
        /// <summary>
        /// Determines which properties of bounded object will be shown in VBComboBox. XAML sample: VBShowColumns="PropName1,PropName2,PropName3"
        /// </summary>
        [Category("VBControl")]
        public string VBShowColumns
        {
            get { return _DataShowColumns; }
            set { _DataShowColumns = value; }
        }

        /// <summary>
        /// Determines which columns will be disabled in VBComboBox.
        /// </summary>
        [Category("VBControl")]
        public string VBDisabledColumns { get; set; }

        private string _DataChilds;

        public string VBChilds
        {
            get { return _DataChilds; }
            set { _DataChilds = value; }
        }

        public static readonly StyledProperty<string> VBAccessProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(VBAccess));

        [Category("VBControl")]
        public string VBAccess
        {
            get { return GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
        }

        public static readonly StyledProperty<ACQueryDefinition> ACQueryDefinitionProperty = 
            AvaloniaProperty.Register<VBComboBox, ACQueryDefinition>(nameof(ACQueryDefinition));

        public ACQueryDefinition ACQueryDefinition
        {
            get { return GetValue(ACQueryDefinitionProperty); }
            private set { SetValue(ACQueryDefinitionProperty, value); }
        }

        public static readonly StyledProperty<IAccess> ACAccessProperty = 
            AvaloniaProperty.Register<VBComboBox, IAccess>(nameof(ACAccess));

        public IAccess ACAccess
        {
            get { return GetValue(ACAccessProperty); }
            private set
            {
                SetValue(ACAccessProperty, value);
                if (value != null)
                {
                    ACQueryDefinition = value.NavACQueryDefinition;
                    if (_PART_TakeCount != null)
                        _PART_TakeCount.IsVisible = true;
                }
                else if (_PART_TakeCount != null)
                    _PART_TakeCount.IsVisible = false;
            }
        }

        public static readonly StyledProperty<ObservableCollection<object>> ACAccessCompositeProperty = 
            AvaloniaProperty.Register<VBComboBox, ObservableCollection<object>>(nameof(ACAccessComposite));

        public ObservableCollection<object> ACAccessComposite
        {
            get { return GetValue(ACAccessCompositeProperty); }
            set { SetValue(ACAccessCompositeProperty, value); }
        }

        private bool _ACAccessCompositeChanging = false;

        #endregion

        #region IDataContent Members

        public IACObject ContextACObject
        {
            get { return DataContext as IACObject; }
        }

        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBComboBox>();

        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBComboBox, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBComboBox, ACInitState>(nameof(ACCompInitState));

        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            else if (change.Property == ACCaptionProperty)
            {
                if (BSOACComponent != null)
                    ACCaptionTrans = this.Root().Environment.TranslateText(BSOACComponent, ACCaption);
                else
                    ACCaptionTrans = ACCaption;
            }
        }

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

        public static readonly AttachedProperty<string> VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner<VBComboBox>();

        public string VBValidation
        {
            get { return GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

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
                IsTabStop = true;
            else
                IsTabStop = false;

            if (controlMode == Global.ControlModes.Collapsed)
            {
                if (IsVisible)
                    IsVisible = false;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (IsVisible)
                    IsVisible = false;
            }
            else
            {
                if (!IsVisible)
                    IsVisible = true;
                if (controlMode == Global.ControlModes.Disabled)
                {
                    if (IsEnabled)
                        IsEnabled = false;
                }
                else
                {
                    if (!IsEnabled)
                        IsEnabled = true;
                }
            }
            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
        }

        public bool AutoFocus { get; set; }

        IACType _ACTypeInfo = null;

        public ACClassProperty VBContentPropertyInfo
        {
            get { return _ACTypeInfo as ACClassProperty; }
        }

        public static readonly StyledProperty<string> DisabledModesProperty
            = AvaloniaProperty.Register<VBComboBox, string>(nameof(DisabledModes));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }
        #endregion

        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty
            = AvaloniaProperty.Register<VBComboBox, Global.ControlModes>(nameof(RightControlMode));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes RightControlMode
        {
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        #region IACObject

        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        public IACObject ParentACObject
        {
            get { return Parent as IACObject; }
        }
        #endregion

        #region IACMenuBuilder Member

        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

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
                    // In Avalonia, we would handle refresh differently
                    // For now, just force a refresh of the items source
                    var binding = this.GetBindingObservable(ItemsSourceProperty);
                }
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("VBComboBox", "NavSearchOnACAccess", e.Message);
            }
            return navSearchExecuted;
        }

        #endregion

        #region IACObject Member

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
        #endregion
    }
}
