using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Xml;
using System.Collections;
using gip.core.layoutengine.avui.VisualControlAnalyser;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using gip.ext.designer.avui.Services;
using gip.ext.design.avui;
using Avalonia.Controls;
using gip.ext.designer.avui;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Provides <see cref="IPlacementBehavior"/> for Canvas and VBCanvas
    /// </summary>
    /// <summary xml:lang="de">
    /// Liefert <see cref="IPlacementBehavior"/>  für Canvas und VBCanvas
    /// </summary>
    [ExtensionFor(typeof(VBCanvas), OverrideExtension = typeof(CanvasPlacementSupport))]
    [ExtensionFor(typeof(Canvas), OverrideExtension = typeof(CanvasPlacementSupport))]
    public class VBCanvasPlacementBehavior : CanvasPlacementSupport
    {
        /// <summary>
        /// Begins the placement.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
        public override void BeginPlacement(PlacementOperation operation)
        {
            bool _isFromNode = IsFromNode();
            VBCanvas rootCanvas = this.Context.RootItem.View as VBCanvas;
            if (rootCanvas != null)
            {
                foreach (var item in rootCanvas.Children)
                {
                    if (item is VBEdge)
                    {
                        ((VBEdge)item).RedrawVBEdge(_isFromNode);
                    }
                }
            }

            if (_isFromNode)
            {
                if (this.Context.RootItem.ContentProperty.IsCollection)
                {
                    var designElements = this.Context.RootItem.ContentProperty.CollectionElements.Where(c => c.ComponentType == typeof(VBEdge));

                    PointCollection points = new PointCollection();
                    points.Clear();

                    foreach (DesignItem item in designElements)
                    {
                        DesignItemProperty property = item.Properties[VBEdge.PointsProperty];
                        if (property != null)
                        {
                            property.SetValue(points);
                            property.SetValueOnInstance(points);
                        }
                    }
                }
            }
            base.BeginPlacement(operation);
        }

        /// <summary>
        /// Determines can or can't place child items.
        /// </summary>
        /// <param name="childItems">The child items.</param>
        /// <param name="type">The placement type.</param>
        /// <param name="position">The placement position.</param>
        /// <returns></returns>
        public override bool CanPlace(ICollection<DesignItem> childItems, PlacementType type, PlacementAlignment position)
        {
            foreach (DesignItem item in childItems)
            {
                if (item.ComponentType == typeof(VBEdge))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Ends the placement.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
        public override void EndPlacement(PlacementOperation operation)
        {
            VBCanvas rootCanvas = this.Context.RootItem.View as VBCanvas;
            if (rootCanvas != null)
            {
                bool isFromNode = IsFromNode();
                foreach (var item in rootCanvas.Children)
                {
                    if (item is IEdge)
                    {
                        ((IEdge)item).RedrawVBEdge(isFromNode);
                    }
                }
            }
            base.EndPlacement(operation);
        }

        /// <summary>
        /// Determines is from node or not.
        /// </summary>
        /// <returns>Returns true is from node, otherwise false.</returns>
        public bool IsFromNode()
        {
            if (this.Services.Tool.CurrentTool is DrawingToolEditPoints)
                return false;
            else
                return true;
        }
        //public override void SetPosition(PlacementInformation info)
        //{
        //    //base.SetPosition(info);
        //    info.Item.Properties[FrameworkElement.MarginProperty].Reset();

        //    UIElement child = info.Item.View;
        //    Rect newPosition = info.Bounds;

        //    if (newPosition.Left != GetLeft(child))
        //    {
        //        info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(newPosition.Left);
        //    }
        //    if (newPosition.Top != GetTop(child))
        //    {
        //        info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(newPosition.Top);
        //    }
        //}

            /// <summary>
            /// Determines can or can't leave container.
            /// </summary>
            /// <param name="operation">The placement operation.</param>
            /// <returns>Returns true if can leave container, otherwise false.</returns>
        public override bool CanLeaveContainer(PlacementOperation operation)
        {
            if (operation.Type == PlacementType.Move)
                return false;
            return true;
        }

        /// <summary>
        /// Determines can or can't move the vector.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
        /// <param name="vector">The vector.</param>
        /// <returns>Returns true if can move vector, otherwise false.</returns>
        public override bool CanMoveVector(PlacementOperation operation, Vector vector)
        {
            return true;
        }
    }

    ///// <summary>
    ///// Verhindern, das Canvas innerhalb einer VBVisualGroup markiert werden kann
    ///// </summary>
    //[ExtensionFor(typeof(Canvas), OverrideExtension = typeof(PanelSelectionHandler))]
    //public class CanvasSelectionHandler : BehaviorExtension, IHandlePointerToolMouseDown
    //{
    //    public CanvasSelectionHandler()
    //        : base()
    //    {
    //    }

    //    protected override void OnInitialized()
    //    {
    //        base.OnInitialized();
    //        this.ExtendedItem.AddBehavior(typeof(IHandlePointerToolMouseDown), this);
    //    }

    //    public void HandleSelectionMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result)
    //    {
    //        if (e.ChangedButton == MouseButton.Left && MouseGestureBase.IsOnlyButtonPressed(e, MouseButton.Left))
    //        {
    //            e.Handled = true;
    //            if (result.ModelHit.Parent == null || !(result.ModelHit.Parent.ComponentType == typeof(VBVisualGroup)))
    //                new RangeSelectionGesture(result.ModelHit).Start(designPanel, e);
    //        }
    //    }
    //}

    /// <summary>
    /// Defines an area within which you can explicitly position child elements by using coordinates that are relative to the VBCanvas.
    /// </summary>
    /// <summary xml:lang="de">
    /// Definiert einen Bereich, in dem Sie untergeordnete Elemente explizit positionieren können, indem Sie Koordinaten verwenden, die relativ zur VBCanvas sind.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBCanvas'}de{'VBCanvas'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBCanvas : Canvas, IVBContent, IACObject
    {
        #region c'tors
        static VBCanvas()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        /// <summary>
        /// Creates a new instance of VBCanvas.
        /// </summary>
        public VBCanvas()
        {
            RightControlMode = Global.ControlModes.Enabled;
            this.Loaded += new RoutedEventHandler(VBCanvas_Loaded);
            this.Unloaded += new RoutedEventHandler(VBCanvas_Unloaded);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized)
                return;

            if (!string.IsNullOrEmpty(VBContent))
            {
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                if (ContextACObject != null && ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                {
                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = new PropertyPath(dcPath);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBCanvas.ACUpdateControlModeProperty, binding);
                }
            }
            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBCanvas.ACCompInitStateProperty, binding);
            }

            _Initialized = true;
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
            this.Loaded -= VBCanvas_Loaded;
            this.Unloaded -= VBCanvas_Unloaded;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }
            this.ClearAllBindings();        
        }

        #endregion

        #region Event Override
        void VBCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_dispTimer != null)
            {
                if (!_dispTimer.IsEnabled)
                    _dispTimer.Start();
            }
            else
            {
                if (ReadLocalValue(CanExecuteCyclicProperty) != DependencyProperty.UnsetValue)
                {
                    _dispTimer = new DispatcherTimer();
                    _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                    _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                    _dispTimer.Start();
                }
            }
        }

        void VBCanvas_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }
        }

        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }
        #endregion

        #region IVBContent Members

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
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
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
            Visibility = controlMode >= Global.ControlModes.Disabled ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBCanvas), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUpdateControlMode.
        /// </summary>
        public static readonly DependencyProperty ACUpdateControlModeProperty =
            DependencyProperty.Register("ACUpdateControlMode",
                typeof(object), typeof(VBCanvas),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the AC update control mode.
        /// </summary>
        public object ACUpdateControlMode
        {
            get { return GetValue(ACUpdateControlModeProperty); }
            set { SetValue(ACUpdateControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBCanvas),
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
            VBCanvas thisControl = dependencyObject as VBCanvas;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == ACUpdateControlModeProperty)
                thisControl.UpdateControlMode();
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
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
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
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
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
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }


        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly DependencyProperty CanExecuteCyclicProperty = 
            ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner(typeof(VBCanvas), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Determines is cyclic execution enabled or disabled.The value (integer) in this property determines the interval of cyclic execution in miliseconds.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Ausführung aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        [Category("VBControl")]
        public int CanExecuteCyclic
        {
            get { return (int)GetValue(CanExecuteCyclicProperty); }
            set { SetValue(CanExecuteCyclicProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBCanvas), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));

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
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;

        /// <summary>
        /// Gets or sets the string format for the control.
        /// </summary>
        /// <summary>
        /// Liest oder setzt das Stringformat für das Steuerelement.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }


        public static readonly DependencyProperty AnimationOffProperty = ContentPropertyHandler.AnimationOffProperty.AddOwner(typeof(VBCanvas), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        [Category("VBControl")]
        public bool AnimationOff
        {
            get { return (bool)GetValue(AnimationOffProperty); }
            set { SetValue(AnimationOffProperty, value); }
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
            }
        }

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
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBCanvas));

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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
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
                return this.Parent as IACObject;
            }
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

        #region CanExecute Dispatcher
        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion
    }
}
