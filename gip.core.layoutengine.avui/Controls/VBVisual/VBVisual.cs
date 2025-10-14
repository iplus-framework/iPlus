using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.VisualControlAnalyser;
using System.Collections;
using System.Transactions;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using gip.ext.designer.avui.Services;
using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the placement behavior for VBVisual.
    /// </summary>
    [ExtensionFor(typeof(VBVisual), OverrideExtension = typeof(DefaultPlacementBehavior))]
    public class VBVisualPlacementBehavior : DefaultPlacementBehavior
    {
        static VBVisualPlacementBehavior()
        {
            _contentControlsNotAllowedToAdd.Add(typeof(VBVisual));
        }

        /// <summary>
        /// Ends the placement operation.
        /// </summary>
        /// <param name="operation">The placement operation parameter.</param>
        public override void EndPlacement(PlacementOperation operation)
        {
            base.EndPlacement(operation);
        }

    }

    /// <summary>
    /// Control element for displaying IACObject´s in graphical form. It works only with ACClassDesign´s with ACKindIndex == ACKinds.DSVisual.
    /// XAML attributes:
    ///  -VBContent: Relative or absolute ACUrl to any IACObject
    ///  -VBDesignName: Optional specification on an ACClassDesign.ACIdentifier 
    ///  -VBDynamicContent: true=VBContent can change at runtime (used in BSOiPlusStudio "TabOverview") false(Defualt)=Contents are only defined once
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von IACObject´s in grafischer Form. Es arbeitet nur mit ACClassDesign´s mit ACKindIndex == ACKinds.DSVisual verwendet werden.
    /// XAML-Attribute:
    /// -VBContent:         Relative oder absolute ACUrl zu einem beliebigen IACObject
    /// -VBDesignName:      Optionale Angabe auf eines ACClassDesign.ACIdentifier 
    /// -VBDynamicContent:  true=VBContent kann sich zur Laufzeit ändern (Verwendung im BSOiPlusStudio "TabOverview") false(Defualt)=Inhalt wird nur einmalig festgelegt
    /// </summary>
    [TemplatePart(Name = "PART_SelectedDecorator", Type = typeof(Control))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBVisual'}de{'VBVisual'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBVisual : ContentControl, IVBContent, IACMenuBuilderWPFTree, IACObject // , IVBClassDesign
    {
        #region c'tors

        /// <summary>
        /// Creates a new instace of the VBVisual.
        /// </summary>
        public VBVisual()
            : base()
        {
            RightControlMode = Global.ControlModes.Enabled;
            Loaded += OnVBVisual_Loaded;
            Unloaded += VBVisual_Unloaded;
            VBDesignBase.IsSelectableEnum isSelectable = VBDesignBase.GetIsSelectable(this);
            if (isSelectable == VBDesignBase.IsSelectableEnum.Unset)
                VBDesignBase.SetIsSelectable(this, VBDesignBase.IsSelectableEnum.True);
        }
        #endregion

        #region Properties

        #region Public XAML Properties
        /// <summary>
        /// Determines is content dynamic or static.
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(10)]
        public bool VBDynamicContent
        {
            get;
            set;
        }


        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(11)]
        public string DisabledModes
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBVisual), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Determines is context menu disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(12)]
        public bool DisableContextMenu
        {
            get { return (bool)GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty = DependencyProperty.Register("VBContent", typeof(string), typeof(VBVisual), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Represents the name of ContentACObject.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [ACPropertyInfo(13)]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }
        
        /// <summary>
        /// Represents the dependency property for VBDesignName.
        /// </summary>
        public static readonly DependencyProperty VBDesignNameProperty = DependencyProperty.Register("VBDesignName", typeof(string), typeof(VBVisual), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the name of VBDesign.
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(14)]
        public string VBDesignName
        {
            get { return (string)GetValue(VBDesignNameProperty); }
            set { SetValue(VBDesignNameProperty, value); }
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(15)]
        public bool AutoFocus { get; set; }

        protected string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [ACPropertyInfo(16)]
        public virtual string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                if (ContextACObject != null)
                    return ContextACObject.ACCaption;
                return "";
            }
            set
            {
                _ACCaption = value;
            }
        }

        #endregion

        #region ACComponent-Init-State
        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty = DependencyProperty.Register("ACCompInitState", typeof(ACInitState), typeof(VBVisual), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }
        #endregion

        #region Dynamic XAML over Dependency-Property
        /// <summary>
        /// Represents the dependency property for DataValue.
        /// </summary>
        public static readonly DependencyProperty DataValueProperty
            = DependencyProperty.Register("DataValue", typeof(object), typeof(VBVisual), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        [Category("VBControl")]
        private object DataValue
        {
            get { return (object)GetValue(DataValueProperty); }
            set { SetValue(DataValueProperty, value); }
        }
        #endregion

        #region DataContext

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBVisual), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (ContentACObject != null)
                    return ContentACObject;
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                FrameworkElement parentFrameworkElement = this.Parent as FrameworkElement;
                if (parentFrameworkElement == null)
                    return null;
                return parentFrameworkElement.DataContext as IACObject;
            }
        }

        /// <summary>
        /// Gets the parent ACComponent.
        /// </summary>
        public IACComponent ParentACComponent
        {
            get
            {
                FrameworkElement parentFrameworkElement = this.Parent as FrameworkElement;
                if (parentFrameworkElement == null)
                    return null;
                return parentFrameworkElement.DataContext as IACComponent;
            }
        }

        #endregion

        #region IACObject-Properties

        /// <summary>
        /// Gets the content ACComponent.
        /// </summary>
        public IACComponent ContentACComponent
        {
            get
            {
                return ContentACObject as IACComponent;
            }
        }

        protected IACObject _ACObject;
        ///// <summary>
        ///// Objekt, das innerhalb des VBVisuals dargestellt wird
        ///// </summary>
        /// <summary>
        /// Gets or sets the content ACObject.
        /// </summary>
        public IACObject ContentACObject
        {
            get
            {
                return _ACObject;
            }
            set
            {
                _ACObject = value;
                if (_ACObject != null)
                {
                    if (DataContext != _ACObject)
                    {
                        BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
                        Binding binding = new Binding();
                        binding.Source = _ACObject;
                        this.SetBinding(FrameworkElement.DataContextProperty, binding);
                    }
                    if (_ACObject is IACComponent)
                        LastElementACComponent = _ACObject as IACComponent;
                }
                else if (_ACObject == null)
                    LastElementACComponent = null;
            }
        }

        private bool _WPFRefAdded = false;
        private IACComponent _LastElementACComponent = null;
        private IACComponent LastElementACComponent
        {
            get
            {
                return _LastElementACComponent;
            }
            set
            {
                if (_LastElementACComponent != null && BSOACComponent != null)
                {
                    try
                    {
                        BSOACComponent.RemoveWPFRef(GetHashCode());
                        _WPFRefAdded = false;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBVisual", "LastElementACComponent", msg);
                    }
                }

                _LastElementACComponent = value;

                if (_LastElementACComponent != null && BSOACComponent != null)
                {
                    if (_LastElementACComponent.InitState == ACInitState.Initializing || _LastElementACComponent.InitState == ACInitState.Initialized || _LastElementACComponent.InitState == ACInitState.Constructed)
                    {
                        try
                        {
                            BSOACComponent.AddWPFRef(GetHashCode(), _LastElementACComponent);
                            _WPFRefAdded = true;
                        }
                        catch (Exception exw)
                        {
                            this.Root().Messages.LogDebug("VBVisual", "AddWPFRef", exw.Message);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public virtual IEnumerable<IACObject> ACContentList
        {
            get
            {
                if (ContentACObject != null)
                {
                    List<IACObject> acContentList = new List<IACObject>();
                    acContentList.Add(ContentACObject);
                    if (ContentACObject is IACComponent)
                    {
                        IACComponent acComponent = ContentACObject as IACComponent;
                        if (acComponent.Content != null)
                            acContentList.Add(acComponent.Content);
                    }
                    return acContentList;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the parent ACElement.
        /// </summary>
        public IACInteractiveObject ParentACElement
        {
            get { return this.GetVBDesign(); }
        }

        /// <summary>
        /// Gets or sets the property from ContentACObject.
        /// </summary>
        /// <param name="property">The property parameter.</param>
        /// <returns>The property value as string.</returns>
        public string this[string property]
        {
            get
            {

                if (ContentACObject is IACEntityProperty)
                {
                    return (string)((IACEntityProperty)ContentACObject)[property];
                }
                return null;
            }
            set
            {
                if (ContentACObject is IACEntityProperty)
                {
                    ((IACEntityProperty)ContentACObject)[property] = value;
                }
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

        IACType _VBContentValueType = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentValueType as ACClassProperty;
            }
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

        #endregion

        #region Private and Protected Members
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
            }
        }

        ACClassDesign _ACClassDesign;
        /// <summary>
        /// Gets or sets the ACClassDesign.
        /// </summary>
        protected ACClassDesign ACClassDesign
        {
            get
            {
                return _ACClassDesign;
            }
            set
            {
                _ACClassDesign = value;
            }
        }
        #endregion

        #region Drag and Drop
        /// <summary>
        /// Property to Enable Drag and Drop Behaviour.
        /// </summary>
        public static readonly DependencyProperty DragEnabledProperty = ContentPropertyHandler.DragEnabledProperty.AddOwner(typeof(VBVisual), new FrameworkPropertyMetadata(DragMode.Disabled, FrameworkPropertyMetadataOptions.Inherits));
        public DragMode DragEnabled
        {
            get { return (DragMode)GetValue(DragEnabledProperty); }
            set { SetValue(DragEnabledProperty, value); }
        }


        /// <summary>
        /// Invokes OnDragOver event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDragOver(e);
                return;
            }
            //HandleDragOver(e.Source, 0, 0, e);
            //HandleDragOver(this, 0, 0, e);
            base.OnDragOver(e);
        }

        /// <summary>
        /// Invokes OnDrop event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDrop(e);
                return;
            }
            //HandleDrop(e.Source, 0, 0, e);
            HandleDrop(this, 0, 0, e);
            base.OnDrop(e);
        }

        /// <summary>
        /// Handles the Drop event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="x">The vertical position from left.</param>
        /// <param name="y">The horizontal position from top.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDrop(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            //UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    {
                        if (e.KeyStates != DragDropKeyStates.ControlKey)
                        {
                            e.Effects = DragDropEffects.None;
                            return;
                        }
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
                        ACAction(actionArgs);
                        e.Handled = true;
                        return;
                    }
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    return;
            }
        }

        /// <summary>
        /// Handles the DragOver event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="x">The vertical position from left.</param>
        /// <param name="y">The horizontal position from top.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragOver(object sender, double x, double y, DragEventArgs e)
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
                    HandleDragOver_Move(sender, x, y, e);
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, x, y, e);
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
            //UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das verschieben erlaubt ist
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);

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
            //UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
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
        
        #endregion

        #region Additional Dependency Prop

        /// <summary>
        /// Represents the name for RegisterVBDecorator.
        /// </summary>
        public static DependencyProperty RegisterVBDecoratorProperty
            = VBDesignBase.RegisterVBDecoratorProperty.AddOwner(typeof(VBVisual), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets or sets the VBDecorator.
        /// </summary>
        [Category("VBControl")]
        public string RegisterVBDecorator
        {
            get { return (string)GetValue(RegisterVBDecoratorProperty); }
            set { SetValue(RegisterVBDecoratorProperty, value); }
        }

        #endregion


        private static void OnDepPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            VBVisual thisControl = dependencyObject as VBVisual;
            if (thisControl == null)
                return;
            if (args.Property == DataValueProperty)
            {
                if (thisControl.ParentACObject != null)
                {
                    IACObject acObject = null;
                    if (thisControl.VBContent == "this")
                        acObject = thisControl.ParentACObject;
                    else
                        acObject = thisControl.ParentACObject.ACUrlCommand(thisControl.VBContent, null) as IACObject;
                    thisControl.ContentACObject = acObject;

                    thisControl.LoadDesign();
                }
            }
            else if (args.Property == VBDesignNameProperty)
            {
                if (thisControl._VBInitialized)
                    thisControl.LoadDesign();
            }
            else if (args.Property == ACCompInitStateProperty)
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
            else if (args.Property == VBContentProperty)
            {
                if (thisControl._VBInitialized && !String.IsNullOrEmpty(thisControl.VBContent) && thisControl.DataContext != null && !thisControl._VBContentBindedLate)
                {
                    thisControl._VBContentBindedLate = true;
                    thisControl._VBInitialized = false;
                    thisControl.InitVBControl();
                    //thisControl.LoadDesign();
                }
            }
        }
        #endregion

        #region Methods

        #region Lifecycle

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            IsHitTestVisible = true;
            Focusable = true;
            
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ParentACObject != null)
            {
                if (VBContent == "this")
                    ContentACObject = ParentACObject;
                else
                    ContentACObject = ParentACObject.ACUrlCommand(VBContent, null) as IACObject;
            }
            UpdateACClassDesign();
            InitVBControl();
           
        }


        protected bool _VBInitialized = false;
        protected bool _VBContentBindedLate = false;
        protected bool _VBLoaded = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_VBInitialized)
            {
                InsertVBVisual();
                return;
            }
            if (this.DataContext == null)
            {
                _VBInitialized = true;
                return;
            }
            if (string.IsNullOrEmpty(VBContent) && !VBDynamicContent)
            {
                if (!String.IsNullOrEmpty(VBDesignName))
                {
                    if (ParentACComponent != null)
                    {
                        VBContent = "this";
                    }
                    else
                    {
                        if ((ParentACObject != null) && (ParentACObject is ACClass))
                        {
                            VBContent = "this";
                        }
                    }
                }
                else
                    return;
            }

            if (string.IsNullOrEmpty(VBContent))
                return;

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBVisual.ACCompInitStateProperty, binding);
            }

            // Falls Anzeige des VBVisuals in der "Toten"-Welt
            if ((ParentACComponent == null) && (ParentACObject != null))
            {
                if (ParentACObject is ACClass)
                {
                    IACType acObject = null;
                    if (VBContent == "this")
                        acObject = ParentACObject as ACClass;
                    else
                        acObject = (ParentACObject as ACClass).GetTypeByACUrlComponent(VBContent);
                    ContentACObject = acObject;
                    // IACType cannot be a IACInteractiveObject
                    //if (acObject is IACInteractiveObject && (_ACObject != acObject || DataContext != _ACObject))
                    //{
                    //    BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
                    //    Binding binding = new Binding();
                    //    binding.Source = acObject;
                    //    this.SetBinding(FrameworkElement.DataContextProperty, binding);
                    //}

                    LoadDesign();
                    RightControlMode = Global.ControlModes.Enabled;
                }
                else
                {
                    if (!VBDynamicContent)
                    {
                        IACObject acObject = null;

                        if (VBContent == "this")
                        {
                            acObject = ParentACObject;
                        }
                        else
                        {
                            acObject = ParentACObject.ACUrlCommand(VBContent, null) as IACObject;
                        }

                        ContentACObject = acObject;
                        if (acObject is IACObject && (_ACObject != acObject || DataContext != _ACObject))
                        {
                            BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
                            Binding binding = new Binding();
                            binding.Source = acObject;
                            this.SetBinding(FrameworkElement.DataContextProperty, binding);
                            //DataContext = acObject;
                            if (ContentACObject is IACComponent)
                                LastElementACComponent = ContentACObject as IACComponent;

                        }
                        LoadDesign();
                    }
                    else
                    {
                        IACType dcACTypeInfo = null;
                        object dcSource = null;
                        string dcPath = "";
                        Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                        if (!ParentACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                        {
                            this.Root().Messages.LogDebug("Error00003", "VBVisual", VBContent);
                            return;
                        }

                        RightControlMode = dcRightControlMode;

                        if (Visibility == Visibility.Visible)
                        {
                            if (RightControlMode < Global.ControlModes.Disabled)
                            {
                                Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Binding binding = new Binding();
                                binding.Source = dcSource;
                                if (!string.IsNullOrEmpty(dcPath))
                                    binding.Path = new PropertyPath(dcPath);
                                binding.Mode = BindingMode.OneWay;
                                binding.NotifyOnSourceUpdated = true;
                                binding.NotifyOnTargetUpdated = true;
                                this.SetBinding(VBVisual.DataValueProperty, binding);
                            }
                        }
                    }
                }

                //if (!(ParentACObject is IVBVisualWF))
                //{
                //    return;
                //}
            }
            else if (ParentACComponent != null)
            {
                // Abzeige des VBVisual in der "Lebenden"-Welt
                if (!VBDynamicContent)
                {
                    IACObject acObject = null;

                    if (VBContent == "this")
                    {
                        acObject = ParentACComponent;
                    }
                    else
                    {
                        acObject = ParentACComponent.ACUrlCommand(VBContent, null) as IACObject;
                    }

                    ContentACObject = acObject;
                    if (acObject is IACObject && (_ACObject != acObject || DataContext != _ACObject))
                    {
                        BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
                        Binding binding = new Binding();
                        binding.Source = acObject;
                        this.SetBinding(FrameworkElement.DataContextProperty, binding);
                        //DataContext = acObject;
                    }
                    LoadDesign();
                }
                else
                {
                    IACType dcACTypeInfo = null;
                    object dcSource = null;
                    string dcPath = "";
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                    if (!ParentACComponent.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {
                        this.Root().Messages.LogDebug("Error00003", "VBVisual", VBContent);
                        return;
                    }

                    RightControlMode = dcRightControlMode;

                    if (Visibility == Visibility.Visible)
                    {
                        if (RightControlMode < Global.ControlModes.Disabled)
                        {
                            Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Binding binding = new Binding();
                            binding.Source = dcSource;
                            if (!string.IsNullOrEmpty(dcPath))
                                binding.Path = new PropertyPath(dcPath);
                            binding.Mode = BindingMode.OneWay;
                            binding.NotifyOnSourceUpdated = true;
                            binding.NotifyOnTargetUpdated = true;
                            this.SetBinding(VBVisual.DataValueProperty, binding);
                        }
                    }
                }
            }
            else if (BSOACComponent != null)
            {
                // Abzeige des VBVisual in der "Lebenden"-Welt
                if (!VBDynamicContent)
                {
                    IACObject acObject = null;

                    // Workaround for VBFunctionFlipItem: (Sometimes it happens, that VBContent ist not set properly through Binding to ACUrl of PWNodeProxy-Object
                    if (VBContent == "\\" && ContextACObject != null && ContextACObject is IACComponent)
                    {
                        acObject = ContextACObject;
                    }
                    else if (VBContent == "this")
                    {
                        acObject = BSOACComponent;
                    }
                    else
                    {
                        acObject = BSOACComponent.ACUrlCommand(VBContent, null) as IACObject;
                    }

                    ContentACObject = acObject;
                    if (acObject is IACObject && (_ACObject != acObject || DataContext != _ACObject))
                    {
                        BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
                        Binding binding = new Binding();
                        binding.Source = acObject;
                        this.SetBinding(FrameworkElement.DataContextProperty, binding);
                        //DataContext = acObject;
                    }
                    LoadDesign();
                }
                else
                {
                    IACType dcACTypeInfo = null;
                    object dcSource = null;
                    string dcPath = "";
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                    if (!BSOACComponent.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {
                        this.Root().Messages.LogDebug("Error00003", "VBVisual", VBContent);
                        return;
                    }

                    RightControlMode = dcRightControlMode;

                    if (Visibility == Visibility.Visible)
                    {
                        if (RightControlMode < Global.ControlModes.Disabled)
                        {
                            Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Binding binding = new Binding();
                            binding.Source = dcSource;
                            if (!string.IsNullOrEmpty(dcPath))
                                binding.Path = new PropertyPath(dcPath);
                            binding.Mode = BindingMode.OneWay;
                            binding.NotifyOnSourceUpdated = true;
                            binding.NotifyOnTargetUpdated = true;
                            this.SetBinding(VBVisual.DataValueProperty, binding);
                        }
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
                    Enabled = true;// ParentACObject.GetControlModes(this) >= Global.ControlModes.Enabled;
                }
            }

            _VBInitialized = true;
            InsertVBVisual();
        }

        /// <summary>
        /// Handles OnVBVisualLoaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnVBVisual_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_VBLoaded)
                return;

            if (!_WPFRefAdded && _LastElementACComponent != null && BSOACComponent != null)
            {
                if (_LastElementACComponent.InitState == ACInitState.Initializing || _LastElementACComponent.InitState == ACInitState.Initialized || _LastElementACComponent.InitState == ACInitState.Constructed)
                {
                    try
                    {
                        BSOACComponent.AddWPFRef(GetHashCode(), _LastElementACComponent);
                        _WPFRefAdded = true;
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBVisual", "AddWPFRef", exw.Message);
                    }
                }
            }

            _VBLoaded = true;
        }

        void VBVisual_Unloaded(object sender, RoutedEventArgs e)
        {
            
            if (!_VBLoaded)
                return;

            RemoveVBVisual();

            if (_WPFRefAdded && BSOACComponent != null)
            {
                try
                {
                    BSOACComponent.RemoveWPFRef(GetHashCode());
                    _WPFRefAdded = false;
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBVisual", "VBVisual_Unloaded", msg);
                }
            }

            _VBLoaded = false;
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

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_VBInitialized)
                return;
            if (_WPFRefAdded && bso != null && bso is IACBSO)
            {
                try
                {
                    (bso as IACBSO).RemoveWPFRef(GetHashCode());
                    _WPFRefAdded = false;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBVisual", "DeInitVBControl", msg);
                }
            }

            LastElementACComponent = null;
            Loaded -= OnVBVisual_Loaded;
            Unloaded -= VBVisual_Unloaded;
            _VBInitialized = false;
            _ACClassDesign = null;
            _ACObject = null;
            _VBContentValueType = null;
            BindingOperations.ClearBinding(this, VBVisual.ACCompInitStateProperty);
            BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
            this.ClearAllBindings();
            DataContext = null;
            Content = null;
        }

        /// <summary>
        /// Inserts the VBVisual.
        /// </summary>
        public void InsertVBVisual()
        {
            if (RegisterVBDecorator != null && RegisterVBDecorator.Equals("VBAdorderDecoratorMaterialWF"))
            {
                VBAdornerDecoratorIACObject decorator = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBAdornerDecoratorIACObject)) as VBAdornerDecoratorIACObject;
                if (decorator != null)
                    decorator.RegisterVBVisual(this);
            }
        }

        public void RemoveVBVisual()
        {
            if (RegisterVBDecorator != null && RegisterVBDecorator.Equals("VBAdorderDecoratorMaterialWF"))
            {
                VBAdornerDecoratorIACObject decorator = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBAdornerDecoratorIACObject)) as VBAdornerDecoratorIACObject;
                if (decorator != null)
                    decorator.UnRegisterVBVisual(this);
            }
        }

        #endregion

        
        #region Layout from ACClassDesign

        /// <summary>
        /// Updates the ACClassDesign.
        /// </summary>
        public void UpdateACClassDesign()
        {
            ACClassDesign = null;
            if (ContentACObject != null)
            {
                IACClassDesignProvider designProvider = ContentACObject as IACClassDesignProvider;
                if (designProvider != null)
                    ACClassDesign = designProvider.GetDesign(Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
                else
                    ACClassDesign = ContentACObject.ACType.GetDesign(ContentACObject, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
            }
            if (ACClassDesign == null && BSOACComponent != null)
            {
                ACClassDesign = BSOACComponent.Database.ContextIPlus.ACClassDesign.Where(c => c.ACIdentifier == Const.UnknownDesign && c.ACClass.ACIdentifier == Const.UnknownClass).FirstOrDefault();
            }
        }

        void LoadDesign()
        {
            ACClassDesign = null;
            UpdateACClassDesign();

            Content = null;
            UIElement uiElement = null;
            //var x = ACClassDesign.ValueTypeACClass.ACIdentifier;

            if (ACClassDesign != null && ACClassDesign.BAMLDesign != null && ACClassDesign.IsDesignCompiled)
                uiElement = Layoutgenerator.LoadLayout(ACClassDesign, ContextACObject == null ? ContentACObject : ContextACObject, BSOACComponent, ACClassDesign.ACIdentifier);
            else if (ACClassDesign != null && !string.IsNullOrEmpty(ACClassDesign.XMLDesign) && !ACClassDesign.IsResourceStyle)
                uiElement = Layoutgenerator.LoadLayout(ACClassDesign.XMLDesign, ContextACObject == null ? ContentACObject : ContextACObject, BSOACComponent, ACClassDesign.ACIdentifier);
            else
            {
                // Im Fehlerfall
                ContentControl contentControl = new ContentControl();
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Icons/Design.xaml", UriKind.Relative);
                contentControl.Style = (Style)dict["IconDesignStyleGip"];
                uiElement = contentControl;
            }
            Content = uiElement;
            OnDesignLoaded();
        }

        /// <summary>
        /// Handles OnDesignLoaded.
        /// </summary>
        protected virtual void OnDesignLoaded()
        {
        }
        #endregion


        #region Mouse-Events

        /// <summary>
        /// Handles the OnContextMenuOpening event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (DisableContextMenu)
                e.Handled = true;
            else
                base.OnContextMenuOpening(e);
        }

        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            VBDesign vbDesign = this.GetVBDesign();

            if (   vbDesign != null 
                && vbDesign.IsDesignerActive 
                && (   vbDesign.GetDesignManager() == null 
                    || (   !vbDesign.GetDesignManager().ACIdentifier.StartsWith("VBDesignerWorkflowMethod")
                        && !vbDesign.GetDesignManager().ACIdentifier.StartsWith("VBDesignerMaterialWF")
                       )
                   )
               )
            {
                return;
            }
            Point point = e.GetPosition(this);
            ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
            //BSOACComponent.ParentACComponent.ACAction(actionArgs);
            BSOACComponent.ACAction(actionArgs);
            if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
            {
                VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                this.ContextMenu = vbContextMenu;
                //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                if (vbContextMenu.PlacementTarget == null)
                    vbContextMenu.PlacementTarget = this;
                ContextMenu.IsOpen = true;
                e.Handled = true;
            }
            base.OnMouseRightButtonDown(e);
        }

        bool internalTooltip = false;
        /// <summary>
        /// Handles the OnMouseEnter event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, FrameworkElement.ToolTipProperty);
            if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            {
                internalTooltip = true;
                if (ContentACObject != null)
                {
                    ToolTip = VBContent + "\n" + ContentACObject.ACCaption;
                }
                else
                {
                    ToolTip = VBContent;
                }
            }
            base.OnMouseEnter(e);
        }

        /// <summary>
        /// Handles the OnMouseLeave event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (internalTooltip)
                ToolTip = DependencyProperty.UnsetValue;
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Handles the OnMouseDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseDown(PointerEventArgs e)
        {
            if (DragEnabled == DragMode.Enabled && this.ContentACObject is IACObject)
                VBDragDrop.VBDoDragDrop(e, this);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            VBVisual vbVisual = this.GetVBDesign()?.SelectedVBControl as VBVisual;
            if(vbVisual == null || vbVisual != this)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            Point p = e.GetPosition(vbVisual);
            HitTestResult result = VisualTreeHelper.HitTest(vbVisual, p);

            if (result == null || vbVisual.ContextACObject == null)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            string methodName = "";
            using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
            {
                ACClass currentACClass = vbVisual.ContextACObject.ACType as ACClass;
                methodName = currentACClass?.Methods.FirstOrDefault(c => c.ExecuteByDoubleClick && c.IsStatic && c.IsInteraction)?.ACIdentifier;
            }

            if (!string.IsNullOrEmpty(methodName))
            {
                vbVisual.ContextACObject.ACUrlCommand("!" + methodName);
                e.Handled = true;
                return;
            }
            
            vbVisual?.ControldialogOn();
            e.Handled = true;
        }

        #endregion


        #region IACInteractiveObject Methods
        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public virtual void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                        return;
                    }
                    break;
            }
            BSOACComponent.ACActionToTarget(this, actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public virtual bool IsEnabledACAction(ACActionArgs actionArgs)
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
        #endregion


        #region IACMenuBuilder Methods
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

            if (ContextACObject is IACInteractiveObject)
            {

                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    if (Database.Root.Environment.License.MayUserDevelop)
                    {
                        ACClass acClass = Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == "BSOiPlusStudio").First();
                        if (acClass.GetRight(acClass) == Global.ControlModes.Enabled)
                        {
                            ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                            if (ContextACObject.ACType != null)
                            {
                                ACValueItem category = Global.ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == (short)Global.ContextMenuCategory.Utilities);
                                ACMenuItem parent = new ACMenuItem(null, category.ACCaption, category.Value.ToString(), category.SortIndex, null, true);

                                acMethod.ParameterValueList["AutoLoad"] = ContextACObject.ACType.GetACUrl();
                                acMenuItemList.Add(new ACMenuItem("Show " + ContextACObject.ACType.ACIdentifier + " in iPlus Development Environment", Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, (short)MISort.IPlusStudio, acMethod.ParameterValueList, parent.ACUrl));
                            }
                        }
                    }
                }
            }
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


        #region IACObject Methods
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


        #region Context-Menu Methods
        /// <summary>
        /// Opens the control dialog.
        /// </summary>
        [ACMethodInteraction("", "en{'Controldialog'}de{'Steuerungsdialog'}", (short)MISort.ControldialogOn, false)]
        public void ControldialogOn()
        {
            VBDesignBase vbDesign = this.GetVBDesignBase();
            if (vbDesign != null)
            {
                string acInstance = ACUrlHelper.ExtractInstanceName(vbDesign.VBBSOSelectionManager.ACIdentifier);
                IACComponent controlDialog;
                if (string.IsNullOrEmpty(acInstance))
                    controlDialog = vbDesign.GetACComponentByKey(BSOACComponent, "VBBSOControlDialog");
                else
                    controlDialog = vbDesign.GetACComponentByACIdentifier(BSOACComponent, "VBBSOControlDialog(" + acInstance + ")");

                if (controlDialog != null)
                    controlDialog.ACUrlCommand("!ShowSelectionDialog", this);
            }
        }

        /// <summary>
        /// Determines is enabled to open control dialog.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledControldialogOn()
        {
            if (ContentACObject == null || !(ContentACObject is IACClassDesignProvider) || ContentACObject.ACType == null)
                return false;

            ACClassDesign design = null;
            IACClassDesignProvider designProvider = ContentACObject as IACClassDesignProvider;
            if (designProvider != null)
                design = designProvider.GetDesign(Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
            else
                design = ContentACObject.ACType.GetDesign(ContentACObject, Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
            return design != null;
        }
#endregion


#region Misc
        /// <summary>
        /// Overrides ToString method.
        /// </summary>
        /// <returns>VBVisual: Name, VBContent</returns>
        public override string ToString()
        {
            return string.Format("VBVisual: {0} {1}", this.Name, this.VBContent);
        }
#endregion

#endregion
    }
}
