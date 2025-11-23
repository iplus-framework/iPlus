using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    /// <summary> 
    /// Contains a list of selectable items.
    /// </summary>
    /// <summary>
    /// Enthält eine Liste der auswählbaren Elemente.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBListBox'}de{'VBListBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBListBox : ListBox, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        string _DataShowColumns;
        string _DataChilds;
        private DispatcherTimer _cyclickDataRefreshDispTimer;

        public VBListBox()
        {
            // Set up drag and drop event handlers for Avalonia
            if (DragEnabled == DragMode.Enabled)
            {
                DragDrop.SetAllowDrop(this, true);
                AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AddHandler(DragDrop.DragOverEvent, OnDragOver);
                AddHandler(DragDrop.DropEvent, OnDrop);
                AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            }
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.PointerPressed += new EventHandler<PointerPressedEventArgs>(VBListBox_PointerPressed);
            this.DoubleTapped += VBListBox_DoubleTapped;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        #endregion

        #region Additional Dependenc-Properties
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBListBox, Global.ControlModes>(nameof(ControlMode));
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

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new VBListBoxItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBListBoxItem>(item, out recycleKey);
        }

        /// <summary>
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBListBox, Global.ControlModes>(nameof(RightControlMode));

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
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBListBox, string>(Const.ACCaptionPrefix);
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBListBox, string>(nameof(ACCaptionTrans));
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
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBListBox, bool>(nameof(ShowCaption), true);
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
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        [Category("VBControl")]
        [ACPropertyInfo(999)]
        public bool IsEnabledScrollIntoView
        {
            get { return GetValue(IsEnabledScrollIntoViewProperty); }
            set { SetValue(IsEnabledScrollIntoViewProperty, value); }
        }

        public static readonly StyledProperty<bool> IsEnabledScrollIntoViewProperty =
            AvaloniaProperty.Register<VBListBox, bool>(nameof(IsEnabledScrollIntoView), false);


        #region xx = > CyclicDataRefresh

        /// <summary>
        /// Determines is cyclic refresh enabled or disabled.The value (integer) in this property determines the interval of cyclic execution in miliseconds.   
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Aktualisier aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        [Category("VBControl")]
        public int CyclicDataRefresh
        {
            get { return GetValue(CyclicDataRefreshProperty); }
            set { SetValue(CyclicDataRefreshProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CyclicDataRefresh.
        /// </summary>
        public static readonly StyledProperty<int> CyclicDataRefreshProperty = 
            AvaloniaProperty.Register<VBListBox, int>(nameof(CyclicDataRefresh));

        #endregion

        #region Layout
        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaptionProperty =
            AvaloniaProperty.Register<VBListBox, GridLength>(nameof(WidthCaption), new GridLength(15, GridUnitType.Star));
        /// <summary>
        /// Gets or sets the width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption
        {
            get { return GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthCaptionMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthCaptionMaxProperty =
            AvaloniaProperty.Register<VBListBox, double>(nameof(WidthCaptionMax), 150.0);
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
            get { return GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContent.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthContentProperty =
            AvaloniaProperty.Register<VBListBox, GridLength>(nameof(WidthContent), new GridLength(20, GridUnitType.Star));
        /// <summary>
        /// Gets or sets the width of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die maximale Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthContent
        {
            get { return GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContentMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthContentMaxProperty =
            AvaloniaProperty.Register<VBListBox, double>(nameof(WidthContentMax), Double.PositiveInfinity);
        /// <summary>
        /// Gets or sets the maximum width of content.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for WidthPadding.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthPaddingProperty =
            AvaloniaProperty.Register<VBListBox, GridLength>(nameof(WidthPadding), new GridLength(0));
        /// <summary>
        /// Gets or sets the width of padding.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }
        #endregion

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
            if (DisableContextMenu)
                ContextFlyout = null;

            _Initialized = true;
            LoadCyclicDataRefreshProperty();
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBListBox", VBContent);
                return;
            }
            _ACTypeInfo = dcACTypeInfo;

            // Check if RightControlMode is locally set
            if (!this.IsSet(VBListBox.RightControlModeProperty) || dcRightControlMode < RightControlMode)
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
                this.Bind(VBListBox.ACCompInitStateProperty, binding);
            }

            // VBContent muß im XAML gestettet sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (_ACTypeInfo == null)
                return;

            // Beschriftung
            if (string.IsNullOrEmpty(ACCaption))
                ACCaptionTrans = _ACTypeInfo.ACCaption;
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
                this.Root().Messages.LogDebug("Error00004", "VBListBox", VBSource + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00004", "VBListBox", VBSource, VBContent);
                return;
            }

            if (!String.IsNullOrEmpty(VBAccess))
                acAccess = VBAccess;
            if (string.IsNullOrEmpty(acAccess))
            {
                ACClass navQueryClass = dsACTypeInfo.ValueTypeACClass.PrimaryNavigationquery();
                if (navQueryClass != null)
                {
                    ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, navQueryClass, navQueryClass.ACIdentifier, true);
                    if (queryDef != null)
                    {
                        IACObjectWithInit initObject = ContextACObject as IACObjectWithInit;
                        if (initObject == null)
                            initObject = BSOACComponent;
                        bool isObjectSet = ((dsPath.StartsWith("Database.") || (dsSource != null && dsSource is IACEntityObjectContext))
                                            && (dsACTypeInfo is ACClassProperty && (dsACTypeInfo as ACClassProperty).GenericType == typeof(Microsoft.EntityFrameworkCore.DbSet<>).FullName));
                        if (initObject != null && isObjectSet)
                            _ACAccess = queryDef.NewAccess("VBListBox", initObject, dsSource as IACObject);
                        else
                            _ACQueryDefinition = queryDef;
                    }
                }
            }
            else
            {
                _ACAccess = ContextACObject.ACUrlCommand(acAccess) as IAccess;
            }

            List<ACColumnItem> vbShowColumns = null;
            if (ACQueryDefinition != null)
                vbShowColumns = ACQueryDefinition.GetACColumns(this.VBShowColumns);

            if ((vbShowColumns == null || !vbShowColumns.Any()) 
                && (dsACTypeInfo == null || dsACTypeInfo.ObjectType != typeof(string)) 
                && ItemTemplate == null)
            {
                this.Root().Messages.LogDebug("Error00005", "VBListBox", VBShowColumns + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00005", "VBListBox", VBShowColumns, VBContent);
                return;
            }

            IACType dsColACTypeInfo = dsACTypeInfo;
            object dsColSource = null;
            string dsColPath = "";
            Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
            if (vbShowColumns != null && vbShowColumns.Any())
            {
                if (!dsACTypeInfo.ACUrlBinding(vbShowColumns[0].PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                {
                    return;
                }
                if (dsColACTypeInfo.ValueTypeACClass.ACKind != Global.ACKinds.TACLRBaseTypes)
                {
                    dsColPath += "." + dsColACTypeInfo.ValueTypeACClass.GetColumns(1).First().ACIdentifier;
                }
            }

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
                    var binding = new Binding
                    {
                        Source = ACAccess,
                        Path = "NavObjectList",
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(ItemsSourceProperty, binding);
                }
                else
                {
                    if (dsSource != null && dsSource is IACEntityObjectContext)
                    {
                        var result = (dsSource as IACEntityObjectContext).ACSelect(ACQueryDefinition, dsPath);
                        var binding = new Binding
                        {
                            Source = result.AsArrayList()
                        };
                        this.Bind(ItemsSourceProperty, binding);
                    }
                    else
                    {
                        Type t = dsSource.GetType();
                        PropertyInfo pi = t.GetProperty(Const.ContextDatabase + ".");

                        var lastPath = dsPath.Substring(9);
                        var database = pi.GetValue(dsSource, null) as IACObject;
                        var result = database.ACSelect(ACQueryDefinition, lastPath);
                        this.ItemsSource = result.AsArrayList();
                    }
                }
            }
            else
            {
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
            // TODO: Supported bisher nur eine Spalte
            if (ItemTemplate == null)
                DisplayMemberBinding = new Binding(dsColPath);

            // Gebundene Spalte setzen (VBContent)
            var binding2 = new Binding
            {
                Source = dcSource,
                Path = dcPath,
                Mode = VBContentPropertyInfo != null && VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
            };
            this.Bind(SelectedValueProperty, binding2);

            if (AutoFocus)
            {
                Focus();
            }
        }

        private void ObservableColl_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsEnabledScrollIntoView)
                (VBVisualTreeHelper.FindChildObjectInVisualTree(this, typeof(ScrollViewer)) as ScrollViewer)?.ScrollToEnd();
        }

        private void LoadCyclicDataRefreshProperty()
        {
            if (this.IsSet(CyclicDataRefreshProperty))
            {
                if (CyclicDataRefresh > 0)
                {
                    _cyclickDataRefreshDispTimer = new DispatcherTimer();
                    _cyclickDataRefreshDispTimer.Tick += new EventHandler(cyclickDataRefreshDispTimer_CanExecute);
                    _cyclickDataRefreshDispTimer.Interval = new TimeSpan(0, 0, 0, 0, CyclicDataRefresh);
                    _cyclickDataRefreshDispTimer.Start();
                }
            }
        }

        private void cyclickDataRefreshDispTimer_CanExecute(object sender, EventArgs e)
        {
            if (ItemsSource != null && ItemsSource is ICyclicRefreshableCollection)
            {
                (ItemsSource as ICyclicRefreshableCollection).Refresh();
            }
        }

        bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                var boundedValue = BindingOperations.GetBindingExpressionBase(this, ItemsSourceProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.GetSource() as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBListBox", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
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
            _ACTypeInfo = null;
            _ACAccess = null;

            this.ClearAllBindings();
            this.ItemsSource = null;
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
        /// Handles the OnPointerReleased event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                if (DisableContextMenu)
                {
                    e.Handled = true;
                    return;
                }
                var uiElement = e.Source as Visual;
                if ((ContextACObject != null) && (uiElement != null))
                {
                    UpdateACContentList(GetNearestContainer(uiElement));
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
                        ContextMenu.Open();
                    }
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        void VBListBox_DoubleTapped(object sender, TappedEventArgs e)
        {
            OnDoubleTapped(e);
        }

        protected void OnDoubleTapped(TappedEventArgs e)
        {
            if (DblClick != null && DblClick != "")
            {
                if (SelectedItem == null)
                {
                    // TODO: Erste Zelle markieren, ucGrid.SelectedGridItem darf dann nicht NULL sein
                    //SetSelectedItemToValue((DataGridCellInfo)ucGrid.SelectedItem);
                }
                //SetSelectedItemToValue((DataGridCellInfo)ucGrid.SelectedItem);

                ContextACObject.ACUrlCommand(DblClick);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
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
            base.OnKeyUp(e);
        }

        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (IsEnabledScrollIntoView && SelectionMode == SelectionMode.Single && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0];
                if (item != null)
                {
                    // In Avalonia, we can try to scroll the item into view
                    try
                    {
                        // Get the index of the item and scroll to it
                        var index = Items.IndexOf(item);
                        if (index >= 0)
                        {
                            // Try to bring the item into view using ScrollViewer
                            var scrollViewer = this.FindDescendantOfType<ScrollViewer>();
                            if (scrollViewer != null)
                            {
                                // Calculate the position to scroll to based on item index
                                var itemHeight = 25; // Approximate item height
                                var targetOffset = index * itemHeight;
                                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetOffset);
                            }
                        }
                    }
                    catch
                    {
                        // If scrolling fails, just ignore
                    }
                }
            }
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
            else if (change.Property == RightControlModeProperty)
                UpdateControlMode();
            else if (change.Property == ACCaptionProperty)
            {
                if (ContextACObject != null)
                {
                    if (!_Initialized)
                        return;
                    ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                }
            }
            else if (change.Property == DragEnabledProperty)
            {
                // Update drag and drop handlers when DragEnabled property changes
                UpdateDragDropHandlers();
            }
            else if (change.Property == SelectionProperty || change.Property == SelectedItemProperty)
            {
                // Handle selection changes manually since Avalonia doesn't have OnSelectionChanged
                var oldItems = change.OldValue as IList ?? new List<object>();
                var newItems = change.NewValue as IList ?? (change.NewValue != null ? new List<object> { change.NewValue } : new List<object>());
                var selectionChangedArgs = new SelectionChangedEventArgs(SelectionChangedEvent, oldItems, newItems);
                OnSelectionChanged(selectionChangedArgs);
            }
            VB_TargetUpdated(null, change);
        }

        private void UpdateDragDropHandlers()
        {
            if (DragEnabled == DragMode.Enabled)
            {
                DragDrop.SetAllowDrop(this, true);
                AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AddHandler(DragDrop.DragOverEvent, OnDragOver);
                AddHandler(DragDrop.DropEvent, OnDrop);
                AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            }
            else
            {
                DragDrop.SetAllowDrop(this, false);
                RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
                RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
                RemoveHandler(DragDrop.DropEvent, OnDrop);
                RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            }
        }

        void VB_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        void VB_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        #endregion

        #region Methods
        private IACObject GetNearestContainer(Visual element)
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
                    element = element.GetVisualParent();

                    if ((element != null) && (element is VBListBoxItem))
                    {
                        VBListBoxItem vbListBoxItem = element as VBListBoxItem;
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
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBListBox, string>(nameof(VBContent));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's Selected property. Selected property must be marked with [ACPropertySelected(...)] attribute.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
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
        public static readonly StyledProperty<string> VBSourceProperty =
            AvaloniaProperty.Register<VBListBox, string>(nameof(VBSource));

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

        /// <summary>
        /// Determines which property of bounded object will be shown in VBListBox. XAML sample: VBShowColumns="PropName1"
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
        /// Determines which columns will be disabled in VBListBox.
        /// </summary>
        [Category("VBControl")]
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
        /// Represents the dependency property for VBAccess.
        /// </summary>
        public static readonly StyledProperty<string> VBAccessProperty =
            AvaloniaProperty.Register<VBListBox, string>(nameof(VBAccess));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBAccess
        {
            get { return GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
        }


        ACQueryDefinition _ACQueryDefinition = null;
        /// <summary>
        /// Gets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get
            {
                if (ACAccess != null)
                    return ACAccess.NavACQueryDefinition;
                return _ACQueryDefinition;
            }
        }

        IAccess _ACAccess = null;
        /// <summary>
        /// Gets the ACAccess.
        /// </summary>
        public IAccess ACAccess
        {
            get
            {
                return _ACAccess;
            }
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
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBListBox>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBListBox, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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
        /// Represents the property in which you enter the name of method that is invoked on mouse double click.
        /// </summary>
        [Category("VBControl")]
        public string DblClick
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly AttachedProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBListBox>();
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        [Category("VBControl")]
        public string VBValidation
        {
            get { return GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly AttachedProperty<bool> DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBListBox>();
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
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
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
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else
            {
                if (!this.IsVisible)
                    this.IsVisible = true;
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

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        /// <summary>
        /// Enable or disable drag.
        /// </summary>
        [Category("VBControl")]
        public DragMode DragEnabled { get; set; }

        /// <summary>
        /// Represents the dependency property for DragEnabled.
        /// </summary>
        public static readonly StyledProperty<DragMode> DragEnabledProperty =
            AvaloniaProperty.Register<VBListBox, DragMode>(nameof(DragEnabled));

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
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBListBox, string>(nameof(DisabledModes));
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
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }
        #endregion

        #region DragAndDrop

        void VBListBox_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (DragEnabled == DragMode.Enabled)
            {
                object dragItem = SelectedItem;
                if (dragItem != null)
                {
                    // Create a data object for the drag operation
                    var dataObject = new DataObject();
                    dataObject.Set("VBListBox", this);
                    
                    // Start the drag operation using Avalonia's DragDrop
                    var task = DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        void OnDragEnter(object sender, DragEventArgs e)
        {
            ProcessDrag(e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            ProcessDrag(e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }

            IACInteractiveObject item = VBDragDrop.GetDropObject(e);
            if (item != null && BSOACComponent != null && e.Source != null && e.Source is IACInteractiveObject)
            {
                ACActionArgs args = new ACActionArgs(item, 0, 0, Global.ElementActionType.Drop);
                BSOACComponent.ACActionToTarget(e.Source as IACInteractiveObject, args);
            }
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            // Handle drag leave if needed
        }

        void ProcessDrag(DragEventArgs e)
        {
            //e.DragEffects = DragDropEffects.None;
            
            //if (this.GetVBDesign().IsDesignerActive)
            //{
            //    return;
            //}

            //// Check if we can accept the drag data
            //if (e.Data.Contains("VBListBox"))
            //{
            //    e.DragEffects = e.KeyModifiers.HasFlag(KeyModifiers.Control) 
            //        ? DragDropEffects.Copy 
            //        : DragDropEffects.Move;
            //}
            //else
            //{
            //    // Try to get drop object from VBDragDrop
            //    try
            //    {
            //        var dropObject = VBDragDrop.GetDropObject(e);
            //        if (dropObject != null)
            //        {
            //            e.DragEffects = e.KeyModifiers.HasFlag(KeyModifiers.Control) 
            //                ? DragDropEffects.Copy 
            //                : DragDropEffects.Move;
            //        }
            //    }
            //    catch
            //    {
            //        // If VBDragDrop.GetDropObject fails, just leave DragEffects as None
            //    }
            //}
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

        #region Interaction Methods
        /// <summary>
        /// Opens the filter dialog.
        /// </summary>
        [ACMethodInteraction("", "en{'Filter'}de{'Filter'}", 101, false)]
        public void Filter()
        {
            if (ACAccess == null)
                return;
            if (ACAccess.ShowACQueryDialog())
            {
                if (NavSearchOnACAccess())
                {
                    //BindingExpression expression = this.GetBindingExpression(ItemsSourceProperty);
                    //if (expression != null)
                    //    expression.UpdateTarget();
                    //else
                    //    ItemsSource = ACAccess.NavObjectList;
                }
            }
        }

        /// <summary>
        /// Determines is enabled Filter or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledFilter()
        {
            return ACAccess != null && BSOACComponent != null;
        }

        private bool NavSearchOnACAccess()
        {
            if (ACAccess == null)
                return false;
            bool navSearchExecuted = false;
            IACBSO acComponent = ContextACObject as IACBSO;
            if (acComponent == null)
                acComponent = BSOACComponent;
            if (acComponent != null)
                navSearchExecuted = acComponent.ExecuteNavSearch(ACAccess);
            if (!navSearchExecuted)
                navSearchExecuted = ACAccess.NavSearch();
            return navSearchExecuted;
        }
        #endregion
    }
}
