using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

    [TemplatePart(Name = "PART_SelectedDecorator", Type = typeof(Control))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBVisual'}de{'VBVisual'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBVisual : ContentControl, IVBContent, IACMenuBuilderWPFTree, IACObject // , IVBClassDesign
    {
        #region c'tors

        public VBVisual()
            : base()
        {
            RightControlMode = Global.ControlModes.Enabled;
            this.Loaded += OnVBVisual_Loaded;
            this.Unloaded += VBVisual_Unloaded;
            this.DoubleTapped += VBVisual_DoubleTapped;
            VBDesignBase.IsSelectableEnum isSelectable = VBDesignBase.GetIsSelectable(this);
            if (isSelectable == VBDesignBase.IsSelectableEnum.Unset)
                VBDesignBase.SetIsSelectable(this, VBDesignBase.IsSelectableEnum.True);
            
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
        #endregion

        #region Properties

        #region Public XAML Properties
        [Category("VBControl")]
        [ACPropertyInfo(10)]
        public bool VBDynamicContent
        {
            get;
            set;
        }


        [Category("VBControl")]
        [ACPropertyInfo(11)]
        public string DisabledModes
        {
            get;
            set;
        }


        public static readonly AttachedProperty<bool> DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBVisual>();

        [Category("VBControl")]
        [ACPropertyInfo(12)]
        public bool DisableContextMenu
        {
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        public static readonly StyledProperty<string> VBContentProperty = 
            AvaloniaProperty.Register<VBVisual, string>(nameof(VBContent));

        [Category("VBControl")]
        [ACPropertyInfo(13)]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }
        
        public static readonly StyledProperty<string> VBDesignNameProperty = 
            AvaloniaProperty.Register<VBVisual, string>(nameof(VBDesignName));

        [Category("VBControl")]
        [ACPropertyInfo(14)]
        public string VBDesignName
        {
            get { return GetValue(VBDesignNameProperty); }
            set { SetValue(VBDesignNameProperty, value); }
        }

        [Category("VBControl")]
        [ACPropertyInfo(15)]
        public bool AutoFocus { get; set; }

        protected string _ACCaption;
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
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = 
            AvaloniaProperty.Register<VBVisual, ACInitState>(nameof(ACCompInitState));
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }
        #endregion

        #region Dynamic XAML over Dependency-Property
        public static readonly StyledProperty<object> DataValueProperty =
            AvaloniaProperty.Register<VBVisual, object>(nameof(DataValue));

        [Category("VBControl")]
        private object DataValue
        {
            get { return GetValue(DataValueProperty); }
            set { SetValue(DataValueProperty, value); }
        }
        #endregion

        #region DataContext

        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBVisual>();
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public IACObject ContextACObject
        {
            get
            {
                if (ContentACObject != null)
                    return ContentACObject;
                return DataContext as IACObject;
            }
        }

        public IACObject ParentACObject
        {
            get
            {
                Control parentControl = this.Parent as Control;
                if (parentControl == null)
                    return null;
                return parentControl.DataContext as IACObject;
            }
        }

        public IACComponent ParentACComponent
        {
            get
            {
                Control parentControl = this.Parent as Control;
                if (parentControl == null)
                    return null;
                return parentControl.DataContext as IACComponent;
            }
        }

        #endregion

        #region IACObject-Properties

        public IACComponent ContentACComponent
        {
            get
            {
                return ContentACObject as IACComponent;
            }
        }

        protected IACObject _ACObject;
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
                        this.ClearBinding(Control.DataContextProperty);
                        var binding = new Binding
                        {
                            Source = _ACObject
                        };
                        this.Bind(Control.DataContextProperty, binding);
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

        public IACInteractiveObject ParentACElement
        {
            get { return this.GetVBDesign(); }
        }

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
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentValueType as ACClassProperty;
            }
        }

        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

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
        public static readonly AttachedProperty<DragMode> DragEnabledProperty = 
            ContentPropertyHandler.DragEnabledProperty.AddOwner<VBVisual>();
        public DragMode DragEnabled
        {
            get { return GetValue(DragEnabledProperty); }
            set { SetValue(DragEnabledProperty, value); }
        }


        void OnDragEnter(object sender, DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            HandleDragOver(this, 0, 0, e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            HandleDragOver(this, 0, 0, e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            HandleDrop(this, 0, 0, e);
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            // Handle drag leave if needed
        }

        public void HandleDrop(object sender, double x, double y, DragEventArgs e)
        {
            var uiElement = e.Source as Visual;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            switch (e.DragEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    {
                        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
                        {
                            e.DragEffects = DragDropEffects.None;
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
                    e.DragEffects = DragDropEffects.None;
                    return;
            }
        }

        public void HandleDragOver(object sender, double x, double y, DragEventArgs e)
        {
            var uiElement = e.Source as Visual;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.DragEffects)
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
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Move(object sender, double x, double y, DragEventArgs e)
        {
            var uiElement = e.Source as Visual;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            var uiElement = e.Source as Visual;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
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
        
        #endregion

        #region Additional Dependency Prop

        public static readonly AttachedProperty<string> RegisterVBDecoratorProperty =
            VBDesignBase.RegisterVBDecoratorProperty.AddOwner<VBVisual>();

        [Category("VBControl")]
        public string RegisterVBDecorator
        {
            get { return GetValue(RegisterVBDecoratorProperty); }
            set { SetValue(RegisterVBDecoratorProperty, value); }
        }

        #endregion


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            VBVisual thisControl = this;
            if (change.Property == DataValueProperty)
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
            else if (change.Property == VBDesignNameProperty)
            {
                if (thisControl._VBInitialized)
                    thisControl.LoadDesign();
            }
            else if (change.Property == ACCompInitStateProperty)
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
            else if (change.Property == VBContentProperty)
            {
                if (thisControl._VBInitialized && !String.IsNullOrEmpty(thisControl.VBContent) && thisControl.DataContext != null && !thisControl._VBContentBindedLate)
                {
                    thisControl._VBContentBindedLate = true;
                    thisControl._VBInitialized = false;
                    thisControl.InitVBControl();
                    //thisControl.LoadDesign();
                }
            }
            else if (change.Property == DragEnabledProperty)
            {
                // Update drag and drop handlers when DragEnabled property changes
                UpdateDragDropHandlers();
            }
        }
        #endregion

        #region Methods

        #region Lifecycle

        protected override void OnInitialized()
        {
            base.OnInitialized();
            IsHitTestVisible = true;
            Focusable = true;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
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

        protected virtual void InitVBControl()
        {
            if (_VBInitialized)
            {
                InsertVBVisual();
                return;
            }
            if (DisableContextMenu)
                ContextFlyout = null;

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
                var binding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBVisual.ACCompInitStateProperty, binding);
            }

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
                            this.ClearBinding(Control.DataContextProperty);
                            var binding = new Binding
                            {
                                Source = acObject
                            };
                            this.Bind(Control.DataContextProperty, binding);
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

                        if (IsVisible)
                        {
                            if (RightControlMode < Global.ControlModes.Disabled)
                            {
                                IsVisible = false;
                            }
                            else
                            {
                                var binding = new Binding
                                {
                                    Source = dcSource,
                                    Mode = BindingMode.OneWay
                                };
                                if (!string.IsNullOrEmpty(dcPath))
                                    binding.Path = dcPath;
                                this.Bind(VBVisual.DataValueProperty, binding);
                            }
                        }
                    }
                }
            }
            else if (ParentACComponent != null)
            {
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
                        this.ClearBinding(Control.DataContextProperty);
                        var binding = new Binding
                        {
                            Source = acObject
                        };
                        this.Bind(Control.DataContextProperty, binding);
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

                    if (IsVisible)
                    {
                        if (RightControlMode < Global.ControlModes.Disabled)
                        {
                            IsVisible = false;
                        }
                        else
                        {
                            var binding = new Binding
                            {
                                Source = dcSource,
                                Mode = BindingMode.OneWay
                            };
                            if (!string.IsNullOrEmpty(dcPath))
                                binding.Path = dcPath;
                            this.Bind(VBVisual.DataValueProperty, binding);
                        }
                    }
                }
            }
            else if (BSOACComponent != null)
            {
                if (!VBDynamicContent)
                {
                    IACObject acObject = null;

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
                        this.ClearBinding(Control.DataContextProperty);
                        var binding = new Binding
                        {
                            Source = acObject
                        };
                        this.Bind(Control.DataContextProperty, binding);
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

                    if (IsVisible)
                    {
                        if (RightControlMode < Global.ControlModes.Disabled)
                        {
                            IsVisible = false;
                        }
                        else
                        {
                            var binding = new Binding
                            {
                                Source = dcSource,
                                Mode = BindingMode.OneWay
                            };
                            if (!string.IsNullOrEmpty(dcPath))
                                binding.Path = dcPath;
                            this.Bind(VBVisual.DataValueProperty, binding);
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
                    Enabled = true;
                }
            }

            _VBInitialized = true;
            InsertVBVisual();
        }

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

        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

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
            this.Loaded -= OnVBVisual_Loaded;
            this.Unloaded -= VBVisual_Unloaded;
            _VBInitialized = false;
            _ACClassDesign = null;
            _ACObject = null;
            _VBContentValueType = null;
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
            Visual uiElement = null;
            //var x = ACClassDesign.ValueTypeACClass.ACIdentifier;

            if (ACClassDesign != null && ACClassDesign.BAMLDesign != null && ACClassDesign.IsDesignCompiled)
                uiElement = Layoutgenerator.LoadLayout(ACClassDesign, ContextACObject == null ? ContentACObject : ContextACObject, BSOACComponent, ACClassDesign.ACIdentifier);
            else if (ACClassDesign != null && !string.IsNullOrEmpty(ACClassDesign.XAMLDesign) && !ACClassDesign.IsResourceStyle)
                uiElement = Layoutgenerator.LoadLayout(ACClassDesign.XAMLDesign, ContextACObject == null ? ContentACObject : ContextACObject, BSOACComponent, ACClassDesign.ACIdentifier);
            else
            {
                // Im Fehlerfall
                ContentControl contentControl = new ContentControl();
                uiElement = contentControl;
            }
            Content = uiElement;
            OnDesignLoaded();
        }

        protected virtual void OnDesignLoaded()
        {
        }
        #endregion


        #region Mouse-Events

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
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
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open();
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        bool internalTooltip = false;
        protected override void OnPointerEntered(PointerEventArgs e)
        {
            if (!this.IsSet(ToolTip.TipProperty))
            {
                internalTooltip = true;
                if (ContentACObject != null)
                {
                    ToolTip.SetTip(this, VBContent + "\n" + ContentACObject.ACCaption);
                }
                else
                {
                    ToolTip.SetTip(this, VBContent);
                }
            }
            base.OnPointerEntered(e);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            if (internalTooltip)
                ToolTip.SetTip(this, null);
            base.OnPointerExited(e);
        }


        void VBVisual_DoubleTapped(object sender, TappedEventArgs e)
        {
            VBVisual vbVisual = this.GetVBDesign()?.SelectedVBControl as VBVisual;
            if(vbVisual == null || vbVisual != this)
            {
                return;
            }

            Point p = e.GetPosition(vbVisual);
            var result = vbVisual.InputHitTest(p);

            if (result == null || vbVisual.ContextACObject == null)
            {
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

        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
        }
        #endregion


        #region IACObject Methods
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

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
