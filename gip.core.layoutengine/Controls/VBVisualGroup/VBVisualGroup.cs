using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;
using System.Transactions;
using gip.ext.design.Extensions;
using gip.ext.designer.Extensions;
using System.ComponentModel;

namespace gip.core.layoutengine
{

    /// <summary>
    /// Represents the canvas in group.
    /// </summary>
    [ExtensionDisabled(typeof(PanelMove))]
    [ExtensionDisabled(typeof(PanelSelectionHandler))]
    [ExtensionDisabled(typeof(CanvasPlacementSupport))]
    [ExtensionDisabled(typeof(SelectedElementRectangleExtension))]
    [ExtensionDisabled(typeof(SizeDisplayExtension))]
    [ExtensionDisabled(typeof(MarginHandleExtension))]
    [ExtensionDisabled(typeof(InPlaceEditorExtension))]
    [ExtensionDisabled(typeof(ResizeThumbExtension))]
    [ExtensionDisabled(typeof(ShapeDrawingHandler))]
    [ExtensionDisabled(typeof(VBQuickOperationMenuExtension))]
    [ExtensionDisabled(typeof(TopLeftContainerDragHandle))]
    [ExtensionDisabled(typeof(PanelInstanceFactory))]
    [ExtensionDisabled(typeof(BorderForInvisibleControl))]
    public class VBCanvasInGroup : Canvas
    {
    }


    /// <summary>
    /// Control element for displaying IACObject´s in graphical form with further content.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von IACObject´s in grafischer Form mit weiteren Inhalten.
    /// </summary>
    [TemplatePart(Name = "PART_SelectedDecorator", Type = typeof(Control))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBVisualGroup'}de{'VBVisualGroup'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBVisualGroup : GroupBox, IVBContent, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "VBVisualGroupStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBVisualGroup/Themes/VBVisualGroupStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "VBVisualGroupStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBVisualGroup/Themes/VBVisualGroupStyleAero.xaml" },
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

        static VBVisualGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBVisualGroup), new FrameworkPropertyMetadata(typeof(VBVisualGroup)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBVisualGroup.
        /// </summary>
        public VBVisualGroup()
        {
            RightControlMode = Global.ControlModes.Enabled;
            Loaded += new RoutedEventHandler(OnVBContentControl_Loaded);
            VBDesignBase.IsSelectableEnum isSelectable = VBDesignBase.GetIsSelectable(this);
            if (isSelectable == VBDesignBase.IsSelectableEnum.Unset)
                VBDesignBase.SetIsSelectable(this, VBDesignBase.IsSelectableEnum.True);
        }

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
            IACObject acObject = ParentACObject.ACUrlCommand(VBContent, null) as IACObject;
            ContentACObject = acObject;
            string styleName = "";

            if (ContentACObject != null && ContentACObject.ACType != null)
            {
                ACClassDesign acClassDesign = null;
                IACClassDesignProvider designProvider = ContentACObject as IACClassDesignProvider;
                if (designProvider != null)
                    acClassDesign = designProvider.GetDesign(Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
                else
                    acClassDesign = ContentACObject.ACType.GetDesign(ContentACObject, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
                if (acClassDesign != null)
                {
                    try
                    {
                        var resource = Layoutgenerator.LoadResource(acClassDesign.XMLDesign, ContentACObject, BSOACComponent);

                        if (this.TryFindResource(acClassDesign.ACIdentifier) == null)
                            this.Resources.MergedDictionaries.Add(resource);

                        styleName = acClassDesign.ACIdentifier;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBVisualGroup", "OnApplyTemplate", msg);
                    }
                }
            }

            base.OnApplyTemplate();
            if (!_themeApplied && !String.IsNullOrEmpty(styleName))
                ActualizeTheme(false, styleName);
            if (ParentACObject != null)
                ContentACObject = ParentACObject.ACUrlCommand(VBContent, null) as IACObject;
            UpdateACClassDesign();
            InitVBControl();
        }

        /// <summary>
        /// Actualizes the theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        /// <param name="styleName">The style name.</param>
        public void ActualizeTheme(bool bInitializingCall, string styleName)
        {
            if (!string.IsNullOrEmpty(styleName))
            {
                var resource1 = this.TryFindResource(styleName);
                this.Style = resource1 as Style;
            }

            //_themeApplied = ControlManager.ActualizeTheme(this, MyStyleInfoList, bInitializingCall, false, styleName);
        }
        #endregion

        #region Loaded-Event

        private IACComponent _LastElementACComponent = null;
        private IACComponent LastElementACComponent
        {
            get
            {
                return _LastElementACComponent;
            }
            set
            {
                //if (_LastElementACComponent != null)
                //{
                //    if (_LastElementACComponent != value && _LastElementACComponent.ReferencePoint != null)
                //        _LastElementACComponent.ReferencePoint.Remove(this);
                //    else
                //        return;
                //}
                //_LastElementACComponent = value;
                //if (_LastElementACComponent != null && _LastElementACComponent.ReferencePoint != null)
                //{
                //    _LastElementACComponent.ReferencePoint.Add(this);
                //}
                if (_LastElementACComponent != null && BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(GetHashCode());

                _LastElementACComponent = value;

                try
                {
                    if (_LastElementACComponent != null && BSOACComponent != null)
                        BSOACComponent.AddWPFRef(GetHashCode(), _LastElementACComponent);
                }
                catch (Exception exw)
                {
                    this.Root().Messages.LogDebug("VBVisualGroup", "AddWPFRef", exw.Message);
                }

            }
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
            Loaded -= OnVBContentControl_Loaded;
            _Initialized = false;
            _ACClassDesign = null;
            _ACObject = null;
            _VBContentPropertyInfo = null;
            BindingOperations.ClearBinding(this, VBVisualGroup.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);

            _PART_Canvas = null;
            DataContext = null;
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
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;

        /// <summary>
        /// Handles on loaded event and runs VBControl initialization.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnVBContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
        }

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized)
                return;
            _Initialized = true;

            if (ParentACObject == null)
                return;

            if (string.IsNullOrEmpty(VBContent))
                return;

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBVisualGroup.ACCompInitStateProperty, binding);
            }

            IACObject acObject = ParentACObject.ACUrlCommand(VBContent, null) as IACObject;
            ContentACObject = acObject;

            if (acObject is IACObject)
            {
                DataContext = acObject;
            }
            LoadDesign();

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                {
                    IsEnabled = false;
                }
                else
                {
                    Enabled = true; // ParentACObject.GetControlModes(this) >= Global.ControlModes.Enabled;
                }
            }
            //FrameworkElement x = VBVisualTreeHelper.FindChildVBContentObjectInVisualTree((this.Parent as FrameworkElement).Parent as FrameworkElement, "HeaderGrid");
        }

        #endregion

        #region IVBContent Member

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
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
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
        /// Determines is this instance selected or not.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsSelected.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected",
                                       typeof(bool),
                                       typeof(VBVisualGroup),
                                       new FrameworkPropertyMetadata(false));


        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBVisualGroup), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
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

        #region IACInteractiveObject Member

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBVisualGroup));

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
        /// Gets or sets the name of VBDesign.
        /// </summary>
        [Category("VBControl")]
        public string VBDesignName
        {
            get;
            set;
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
                return parentFrameworkElement != null ? parentFrameworkElement.DataContext as IACObject : null;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBVisualGroup), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBVisualGroup),
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
            VBVisualGroup thisControl = dependencyObject as VBVisualGroup;
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
                    }
                    break;
            }
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

        #region IACObject Member

        protected string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
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
        /// Gets the content ACComponent;
        /// </summary>
        public IACComponent ContentACComponent
        {
            get
            {
                return ContentACObject as IACComponent;
            }
        }

        protected IACObject _ACObject;
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
                if ((_ACObject != null) && (DataContext != _ACObject))
                {
                    DataContext = _ACObject;
                    if (_ACObject is IACComponent)
                        LastElementACComponent = _ACObject as IACComponent;
                }
                else if (_ACObject == null)
                    LastElementACComponent = null;
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

        #region Dynamic XAML over Dependency-Property

        /// <summary>
        /// Represents the dependency property for DataValue.
        /// </summary>
        public static readonly DependencyProperty DataValueProperty
            = DependencyProperty.Register("DataValue", typeof(object), typeof(VBVisualGroup), new PropertyMetadata(new PropertyChangedCallback(VBVisualNode_DataContextChanged)));

        /// <summary>
        /// Gets or sets data value.
        /// </summary>
        [Category("VBControl")]
        private object DataValue
        {
            get { return (object)GetValue(DataValueProperty); }
            set { SetValue(DataValueProperty, value); }
        }

        static void VBVisualNode_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is VBVisualGroup)
            {
                VBVisualGroup vbVisual = sender as VBVisualGroup;
                IACObject acObject = vbVisual.ParentACObject.ACUrlCommand(vbVisual.VBContent, null) as IACObject;
                vbVisual.ContentACObject = acObject;

                vbVisual.LoadDesign();
            }
        }
        #endregion

        #region private Members
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
        #endregion

        #region Layout from ACClassDesign
        ACClassDesign _ACClassDesign;
        /// <summary>
        /// Gets or sets the ACClassDesign.
        /// </summary>
        public ACClassDesign ACClassDesign
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

        /// <summary>
        /// Updates the ACClassDesign.
        /// </summary>
        public void UpdateACClassDesign()
        {
            if (ContextACObject == null)
                return;
            ACClassDesign = null;
            if (ContentACObject != null && ContentACObject.ACType != null)
            {
                IACClassDesignProvider designProvider = ContentACObject as IACClassDesignProvider;
                if (designProvider != null)
                    ACClassDesign = designProvider.GetDesign(Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
                else
                    ACClassDesign = ContentACObject.ACType.GetDesign(ContentACObject, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout, VBDesignName);
            }
            if (ACClassDesign == null)
            {
                ACClassDesign = BSOACComponent.Database.ContextIPlus.ACClassDesign.Where(c => c.ACIdentifier == "UnknowMainlayout").First();
            }
        }

        private Canvas _PART_Canvas;
        /// <summary>
        /// Gets the PART Canvas.
        /// </summary>
        public Canvas PART_Canvas
        {
            get
            {
                if (_PART_Canvas == null)
                {
                    _PART_Canvas = VBVisualTreeHelper.FindObjectInVisualTree(this, "PART_Canvas") as Canvas;
                }
                return _PART_Canvas;
            }
        }

        void LoadDesign()
        {
            ACClassDesign = null;
            UpdateACClassDesign();
        }
        #endregion

        #region Mouse-Events
        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }

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

            if (vbDesign != null
                && vbDesign.IsDesignerActive
                && (vbDesign.GetDesignManager() == null
                    || (!vbDesign.GetDesignManager().ACIdentifier.StartsWith("VBDesignerWorkflowMethod")
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
                ToolTip = null;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            VBVisualGroup vbVisualGroup = this.GetVBDesign()?.SelectedVBControl as VBVisualGroup;
            if (vbVisualGroup == null || vbVisualGroup != this)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            Point p = e.GetPosition(vbVisualGroup);
            HitTestResult result = VisualTreeHelper.HitTest(vbVisualGroup, p);

            if (result == null || vbVisualGroup.ContextACObject == null)
            {
                base.OnMouseDoubleClick(e);
                return;
            }

            string methodName = "";
            using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
            {
                ACClass currentACClass = vbVisualGroup.ContextACObject.ACType as ACClass;
                methodName = currentACClass?.Methods.FirstOrDefault(c => c.ExecuteByDoubleClick && c.IsStatic && c.IsInteraction)?.ACIdentifier;
            }

            if (!string.IsNullOrEmpty(methodName))
            {
                vbVisualGroup.ContextACObject.ACUrlCommand("!" + methodName);
                e.Handled = true;
                return;
            }

            vbVisualGroup?.ControldialogOn();
            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// Opens the control dialog for this instance.
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
        /// Determines is enabled open control dialog.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledControldialogOn()
        {
            if (ContentACObject == null || !(ContentACObject is IACComponent) || ContentACObject.ACType == null)
                return false;

            ACClassDesign design = null;
            IACClassDesignProvider designProvider = ContentACObject as IACClassDesignProvider;
            if (designProvider != null)
                design = designProvider.GetDesign(Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
            else
                design = ContentACObject.ACType.GetDesign(ContentACObject, Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
            return design != null;
        }


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

        /// <summary>
        /// Overrides ToString method.
        /// </summary>
        /// <returns>VBVisualGroup: Name, VBContent</returns>
        public override string ToString()
        {
            return string.Format("VBVisualGroup: {0} {1}", this.Name, this.VBContent);
        }
    }
}
