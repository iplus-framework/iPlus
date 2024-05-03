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
using gip.core.datamodel;
using gip.ext.designer;
using gip.core.layoutengine.Helperclasses;
using System.Windows.Markup;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Basis class for Content Controls
    /// </summary>
    /// <summary>
    /// Basisklasse für Inhaltssteuerelemente
    /// </summary>
    [ContentProperty("InstanceInfoList")]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignBase'}de{'VBDesignBase'}", Global.ACKinds.TACVBControl)]
    public abstract class VBDesignBase : ContentControl, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
        #region c'tors
        static VBDesignBase()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        public VBDesignBase()
            : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += new RoutedEventHandler(VBDesignBase_Loaded);
            this.Unloaded += new RoutedEventHandler(VBDesignBase_Unloaded);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }

        protected bool _LoadedBase = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        internal virtual void InitVBControl()
        {
            if (!_LoadedBase && BSOACComponent != null)
            {
                foreach (VBInstanceInfo instanceInfo in InstanceInfoList)
                {
                    IACComponent subACComponent = null;
                    if (instanceInfo.ACIdentifier == "..")
                    {
                        subACComponent = BSOACComponent != null ? BSOACComponent.ParentACComponent : null;
                    }
                    else
                    {
                        subACComponent = BSOACComponent != null ? BSOACComponent.GetChildComponent(instanceInfo.ACIdentifier, true) : null;
                        if (subACComponent == null && instanceInfo.AutoStart)
                        {
                            subACComponent = BSOACComponent != null ? BSOACComponent.StartComponent(instanceInfo.ACIdentifier, null, instanceInfo.BuildStartParameter()) as IACComponent : null;
                        }
                    }
                    if (subACComponent != null)
                    {
                        if (instanceInfo.SetAsDataContext)
                        {
                            Binding binding = new Binding();
                            binding.Source = subACComponent;
                            this.SetBinding(FrameworkElement.DataContextProperty, binding);
                        }
                        if (instanceInfo.SetAsBSOACComponet)
                        {
                            Binding binding = new Binding();
                            binding.Source = subACComponent;
                            this.SetBinding(VBDesignBase.BSOACComponentProperty, binding);
                        }
                    }
                    else
                    {
                        if (instanceInfo.SetAsDataContext)
                            this.DataContext = null;
                        if (instanceInfo.SetAsBSOACComponet)
                            this.BSOACComponent = null;
                    }
                }

                if (!string.IsNullOrEmpty(BSOIsActivated))
                {
                    Binding binding = new Binding();
                    binding.Path = new PropertyPath(BSOIsActivated);
                    binding.Converter = new ConverterVisibilityBool();
                    SetBinding(VisibilityProperty, binding);
                }

                if (BSOACComponent != null)
                {
                    Binding binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBDesignBase.ACCompInitStateProperty, binding);

                    binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBDesignBase.ACUrlCmdMessageProperty, binding);
                }
                _LoadedBase = true;
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
            if (!_LoadedBase)
                return;
            _LoadedBase = false;
            this.Loaded -= VBDesignBase_Loaded;
            this.Unloaded -= VBDesignBase_Unloaded;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }

            BindingOperations.ClearBinding(this, VBDesignBase.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDesignBase.ACCompInitStateProperty);
            BindingOperations.ClearBinding(this, VBDesignBase.MsgFromSelMngrProperty);
            BindingOperations.ClearAllBindings(this);

            _SelectionManager = null;
            _VBContentPropertyInfo = null;
            SelectedVBControl = null;
            adornVBControlManagerList = null;
            this.Content = null;
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


        protected virtual void VBDesignBase_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }

            if (!string.IsNullOrEmpty(VBContent) && ContextACObject != null && ContextACObject is IACComponent)
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.VBDesignUnloaded));
        }

        protected virtual void VBDesignBase_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl(); // Aufruf leider notwendig, weil im VBDockingPanelTabbedDoc.container_VBDesignLoaded erst bei Loaded-Event der TabItem-Name refreshed wird
            //InstanceSelectionManager();

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

            if(!string.IsNullOrEmpty(VBContent) && ContextACObject != null && ContextACObject is IACComponent)
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.VBDesignLoaded));
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (!string.IsNullOrEmpty(VBContent) && ContextACObject != null && ContextACObject is IACComponent)
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.VBDesignChanged));
        }

        #endregion

        #region InstanceInfo
        public VBInstanceInfoList _InstanceInfoList = new VBInstanceInfoList();
        /// <summary>
        /// Gets or sets the list of instance infos.
        /// </summary>
        public VBInstanceInfoList InstanceInfoList
        {
            get
            {
                return _InstanceInfoList;
            }
            set
            {
                _InstanceInfoList = value;
            }
        }

        /// <summary>
        /// Checks if is instance info with associated key exists in InstaceInfoList.
        /// </summary>
        /// <param name="key">The instance info key parameter.</param>
        /// <returns>True if is exist, otherwise false.</returns>
        public bool ContainsInstanceInfoForKey(string key)
        {
            if (this.InstanceInfoList.Count <= 0)
                return false;
            return (this.InstanceInfoList.Where(c => c.Key == key).Any());
        }

        /// <summary>
        /// Checks if is instance info with associated acIdentifier exists in InstaceInfoList.
        /// </summary>
        /// <param name="acIdentifier">The instance info acIdentifier parameter.</param>
        /// <returns>True if is exist, otherwise false.</returns>
        public bool ContainsInstanceInfoForACIdentifier(string acIdentifier)
        {
            if (this.InstanceInfoList.Count <= 0)
                return false;
            return (this.InstanceInfoList.Where(c => c.ACIdentifier == acIdentifier).Any());
        }

        /// <summary>
        /// Finds VBDesign with instance info by key.
        /// </summary>
        /// <param name="bsoACComponent">The bsoACComponent parameter.</param>
        /// <param name="key">The instance info key parameter.</param>
        /// <returns>The VBDesignBase object if is found, otherwise null.</returns>
        protected VBDesignBase FindVBDesignWithInstanceInfoByKey(IACComponent bsoACComponent, string key)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = null;
            if (this.ContainsInstanceInfoForKey(key) && this.BSOACComponent == bsoACComponent)
            {
                vbDesignBaseWithInstanceInfo = this;
                return vbDesignBaseWithInstanceInfo;
            }
            var queryParentDesigns = this.GetVisualAncestors().OfType<VBDesignBase>();
            if (queryParentDesigns.Any())
            {
                foreach (VBDesignBase parentVBDesign in queryParentDesigns)
                {
                    if (parentVBDesign.ContainsInstanceInfoForKey(key) && this.BSOACComponent == bsoACComponent)
                    {
                        vbDesignBaseWithInstanceInfo = parentVBDesign;
                        return vbDesignBaseWithInstanceInfo;
                    }
                }
            }
            if (this.BSOACComponent == bsoACComponent)
                return this;
            return null;
        }

        /// <summary>
        /// Find VBDesign with instance info by ACIdentifier.
        /// </summary>
        /// <param name="bsoACComponent">The bsoACComponent parameter.</param>
        /// <param name="acIdentifier">The instance info acIdentifier parameter.</param>
        /// <returns>The VBDesignBase object if is found, otherwise null.</returns>
        protected VBDesignBase FindVBDesignWithInstanceInfoByACIdentifier(IACComponent bsoACComponent, string acIdentifier)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = null;
            if (this.ContainsInstanceInfoForACIdentifier(acIdentifier) && this.BSOACComponent == bsoACComponent)
            {
                vbDesignBaseWithInstanceInfo = this;
                return vbDesignBaseWithInstanceInfo;
            }
            var queryParentDesigns = this.GetVisualAncestors().OfType<VBDesignBase>();
            if (queryParentDesigns.Any())
            {
                foreach (VBDesignBase parentVBDesign in queryParentDesigns)
                {
                    if (parentVBDesign.ContainsInstanceInfoForACIdentifier(acIdentifier) && this.BSOACComponent == bsoACComponent)
                    {
                        vbDesignBaseWithInstanceInfo = parentVBDesign;
                        return vbDesignBaseWithInstanceInfo;
                    }
                }
            }
            if (this.BSOACComponent == bsoACComponent)
                return this;
            return null;
        }

        /// <summary>
        /// Gets the ACComponent by key.
        /// </summary>
        /// <param name="bsoACComponent">The bsoACComponent parameter.</param>
        /// <param name="key">The instance info key.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="findOnly">The findOnly parameter.</param>
        /// <returns>The ACComponent object if is found or started, otherwise null.</returns>
        public IACComponent GetACComponentByKey(IACComponent bsoACComponent, string key, object[] parameter = null, bool findOnly = false)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = FindVBDesignWithInstanceInfoByKey(bsoACComponent, key);
            if (vbDesignBaseWithInstanceInfo == null)
                return null;

            VBInstanceInfo instanceInfo = vbDesignBaseWithInstanceInfo.InstanceInfoList.GetInstanceInfoByKey(key);
            string instanceACName;
            if (instanceInfo == null)
                instanceACName = key;
            else
                instanceACName = instanceInfo.ACIdentifier;

            IACComponent subACComponent = bsoACComponent.GetChildComponent(instanceACName, true);
            if (subACComponent == null && !findOnly)
            {
                subACComponent = bsoACComponent.StartComponent(instanceACName, null, parameter) as IACComponent;
            }
            return subACComponent;
        }

        /// <summary>
        /// Gets the ACComponent by ACIdentifier.
        /// </summary>
        /// <param name="bsoACComponent">The bsoACComponent parameter.</param>
        /// <param name="acIdentifier">The instance info acIdentifier parameter.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="findOnly">The findOnly parameter.</param>
        /// <returns>The ACComponent object if is found or started, otherwise null.</returns>
        public IACComponent GetACComponentByACIdentifier(IACComponent bsoACComponent, string acIdentifier, object[] parameter = null, bool findOnly = false)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = FindVBDesignWithInstanceInfoByACIdentifier(bsoACComponent, acIdentifier);
            if (vbDesignBaseWithInstanceInfo == null)
                return null;

            VBInstanceInfo instanceInfo = vbDesignBaseWithInstanceInfo.InstanceInfoList.GetInstanceInfoByACIdentifier(acIdentifier);
            string instanceACName;
            if (instanceInfo == null)
                instanceACName = acIdentifier;
            else
                instanceACName = instanceInfo.ACIdentifier;

            IACComponent subACComponent = bsoACComponent.GetChildComponent(instanceACName, true);
            if (subACComponent == null && !findOnly)
            {
                subACComponent = bsoACComponent.StartComponent(instanceACName, null, parameter) as IACComponent;
            }
            return subACComponent;
        }
        #endregion

        #region IVBContent Member

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
                typeof(ACUrlCmdMessage), typeof(VBDesignBase),
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
        /// Represents the dependeny property for MsgFromSelMngr.
        /// </summary>
        public static readonly DependencyProperty MsgFromSelMngrProperty =
            DependencyProperty.Register("MsgFromSelMngr",
                typeof(ACUrlCmdMessage), typeof(VBDesignBase),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage from selection manager.
        /// </summary>
        public ACUrlCmdMessage MsgFromSelMngr
        {
            get { return (ACUrlCmdMessage)GetValue(MsgFromSelMngrProperty); }
            set { SetValue(MsgFromSelMngrProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBDesignBase),
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
            VBDesignBase thisControl = dependencyObject as VBDesignBase;
            if (thisControl == null)
                return;
            if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (args.Property == MsgFromSelMngrProperty)
                thisControl.OnMsgFromSelMngrReceived();
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
        }


        private IACComponent _SelectionManager;
        /// <summary>
        /// Gets the selection manager.
        /// </summary>
        public IACComponent VBBSOSelectionManager
        {
            get
            {
                if (_SelectionManager == null)
                    InstanceSelectionManager();
                return _SelectionManager;
            }
        }

        private void InstanceSelectionManager()
        {
            if (_SelectionManager == null)
                _SelectionManager = GetACComponentByACIdentifier(this.BSOACComponent, Const.SelectionManagerCDesign_ClassName);

            if (_SelectionManager != null)
            {
                if (BindingOperations.GetBindingExpression(this, VBDesignBase.MsgFromSelMngrProperty) == null)
                {
                    Binding binding = new Binding();
                    binding.Source = _SelectionManager;
                    binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBDesignBase.MsgFromSelMngrProperty, binding);
                }
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return ACUrlHelper.BuildACNameForGUI(this, Name);
            }

            set
            {
                Name = value;
            }
        }

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly DependencyProperty CanExecuteCyclicProperty = ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Determines is cyclic execution enabled or disabled.The value (integer) in this property determines the interval of cyclic execution in miliseconds.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Ausführung aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        public int CanExecuteCyclic
        {
            get { return (int)GetValue(CanExecuteCyclicProperty); }
            set { SetValue(CanExecuteCyclicProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
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
        /// Property to Enable Drag and Drop Behaviour.
        /// </summary>
        public static readonly DependencyProperty DragEnabledProperty = ContentPropertyHandler.DragEnabledProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata(DragMode.Disabled, FrameworkPropertyMetadataOptions.Inherits));
        public DragMode DragEnabled
        {
            get { return (DragMode)GetValue(DragEnabledProperty); }
            set { SetValue(DragEnabledProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;
        /// <summary>
        /// Gets or sets the string format for the control.
        /// </summary>
        /// <summary xml:lang="de">
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


        public static readonly DependencyProperty AnimationOffProperty = ContentPropertyHandler.AnimationOffProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        [Category("VBControl")]
        public bool AnimationOff
        {
            get { return (bool)GetValue(AnimationOffProperty); }
            set { SetValue(AnimationOffProperty, value); }
        }



        protected IACType _VBContentPropertyInfo = null;
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
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDesignBase));

        /// <summary>
        /// Represents the property in which you enter the name of VBDesign.
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
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public virtual void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public virtual bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public virtual string ACCaption
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
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public virtual IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
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
            return true;
        }

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public virtual void OnACUrlMessageReceived()
            {
                if (!this.IsLoaded)
                    return;
                var acUrlMessage = ACUrlCmdMessage;
                if (acUrlMessage == null
                    || acUrlMessage.ACParameter == null
                    || !acUrlMessage.ACParameter.Any()
                    || !(acUrlMessage.ACParameter[0] is IACComponent)
                    || acUrlMessage.TargetVBContent != this.VBContent)
                    return;
                byte[] result = null;
                switch (acUrlMessage.ACUrl)
                {
                    case Const.CmdPrintScreenToImage:
                        result = PrintScreenToImage(acUrlMessage.ACParameter);
                        if (result != null)
                        {
                            (acUrlMessage.ACParameter[0] as IACComponent).ACUrlCommand(Const.CmdPrintScreenToImage, result);
                        }
                        break;
                    case Const.CmdPrintScreenToIcon:
                        result = PrintScreenToIcon(acUrlMessage.ACParameter);
                        if (result != null)
                        {
                            (acUrlMessage.ACParameter[0] as IACComponent).ACUrlCommand(Const.CmdPrintScreenToIcon, result);
                        }
                        break;
                    case Const.CmdPrintScreenToClipboard:
                        PrintScreenToClipboard(CurrentScreenToBitmap());
                        break;
                    case Const.CmdExportDesignToFile:
                        Bitmap bmp = CurrentScreenToBitmap();
                        string fileName = acUrlMessage.ACParameter[1].ToString();
                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        Graphics g = Graphics.FromImage(bmp);
                        Bitmap bmpNew = new Bitmap(bmp);
                        g.DrawImage(bmpNew, new System.Drawing.Point(0, 0));
                        g.Dispose();
                        bmp.Dispose();

                        //code to manipulate bmpNew goes here.

                        bmpNew.Save(fileName);


                        break;
                    case Const.CmdInitSelectionManager:
                    {
                        _= VBBSOSelectionManager;
                    }
                        break;
                    case Const.CmdDesignModeOff:
                        if(this is VBDesign)
                        {
                            VBDesign vbDesign = this as VBDesign;
                            vbDesign.DesignModeOff();
                        }
                        break;
                    default:
                        break;
                }
            }

        /// <summary>
        /// Handles the OnMsgFromSelMngrReceived callback.
        /// </summary>
        public void OnMsgFromSelMngrReceived()
        {
            if (!this.IsLoaded)
                return;
            var acUrlMessage = MsgFromSelMngr;
            if (acUrlMessage == null
                || acUrlMessage.ACParameter == null
                || !acUrlMessage.ACParameter.Any()
                || acUrlMessage.ACParameter.Count() < 2
                || acUrlMessage.ACParameter[0] != _SelectionManager)
                return;
            switch (acUrlMessage.ACUrl)
            {
                case Const.CmdHighlightVBControl:
                    HighlightVBControl(acUrlMessage.ACParameter[1] as IVBContent, (bool)(acUrlMessage.ACParameter[2]));
                    break;
                case Const.CmdHighlightContentACObject:
                    HighlightContentACObject(acUrlMessage.ACParameter[1] as IACObject, (bool)(acUrlMessage.ACParameter[2]));
                    break;
                default:
                    break;
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
                return Parent as IACObject;
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
        #endregion

        #region Selection

        #region Dependency-Properties

        /// <summary>
        /// Represents the dependency property for ShowAdornerLayer.
        /// </summary>
        public static readonly DependencyProperty ShowAdornerLayerProperty
            = DependencyProperty.Register("ShowAdornerLayer", typeof(bool), typeof(VBDesignBase), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Determines is adorner layer shown or hidden.
        /// </summary>
        [Category("VBControl")]
        public bool ShowAdornerLayer
        {
            get { return (bool)GetValue(ShowAdornerLayerProperty); }
            set { SetValue(ShowAdornerLayerProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsDesignerActive.
        /// </summary>
        public static readonly DependencyProperty IsDesignerActiveProperty
            = DependencyProperty.Register("IsDesignerActive", typeof(bool), typeof(VBDesignBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Determines is designer active.
        /// </summary>
        public bool IsDesignerActive
        {
            get { return (bool)GetValue(IsDesignerActiveProperty); }
            set { SetValue(IsDesignerActiveProperty, value); }
        }

        #endregion

      #region Mouse-Event
        //private static VBDesignBase _LastVBDesignBaseWithInfo = null;
        IVBContent _LastClickedControl;

        /// <summary>
        /// Handles the OnPreviewMouseDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (!IsDesignerActive && e.OriginalSource is DependencyObject)
            {
                DependencyObject clickedElement = e.OriginalSource as DependencyObject;
                if (e.OriginalSource is Adorner)
                {
                    if (AdornVBControlManagerList != null)
                    {
                        foreach (AdornerVBControlManager adManager in AdornVBControlManagerList)
                        {
                            AdornerHitTestResult adornerHitResult = adManager.AdornerLayerOfDesign.AdornerHitTest(e.GetPosition(this));
                            if (adornerHitResult != null)
                                clickedElement = adornerHitResult.VisualHit;
                        }
                    }
                    clickedElement = (e.OriginalSource as Adorner).AdornedElement;
                    HitTestResult hitResult = VisualTreeHelper.HitTest(clickedElement as UIElement, e.GetPosition(clickedElement as UIElement));
                    if (hitResult != null)
                        clickedElement = hitResult.VisualHit;
                }
                var ancestors = clickedElement.GetVisualAncestors();
                VBDesignBase vbDesign = ancestors.OfType<VBDesignBase>().FirstOrDefault();

                // Das OnPreviewMouseDown wird getunnelt (oben -> unten am Element-Tree)
                // Falls das durch zuerst gefundene FirstOrDefault() vbDesign dieses Design ist, dann ist er selbst das letzte vbDesign, 
                // das eine Instanz-Definition für den Selection-Manager besitzen kann
                if (vbDesign == this)
                {
                    IVBContent selectableElement = null;
                    var vbContentAncestors = ancestors.OfType<IVBContent>();
                    foreach (IVBContent elementInVisualTree in vbContentAncestors)
                    {
                        if (elementInVisualTree == this)
                            break;
                        if (elementInVisualTree is UIElement)
                        {
                            if (GetIsSelectable(elementInVisualTree as UIElement) == IsSelectableEnum.True)
                            {
                                selectableElement = elementInVisualTree;
                                break;
                            }

                            if (elementInVisualTree is VBCheckBox)
                            {
                                VBCheckBox checkBox = elementInVisualTree as VBCheckBox;
                                if (checkBox != null && checkBox.PushButtonStyle)
                                {
                                    selectableElement = null;
                                    break;
                                }
                            }
                        }
                    }

                    if (selectableElement != null)
                        SetSelectionAtManager(selectableElement);
                }
                // Sonst sammle weiter Instanz-Informationen
                else
                {
                    //if (this.ContainsSelectionManagerInstanceInfo)
                    //_LastVBDesignBaseWithInfo = this;
                }
            }
            base.OnPreviewMouseDown(e);
        }

        private void SetSelectionAtManager(IVBContent controlToSelect)
        {
            _LastClickedControl = controlToSelect;
            if ((controlToSelect is VBVisual || controlToSelect is VBVisualGroup || controlToSelect is VBGraphItem) && _SelectionManager == null)
                InstanceSelectionManager();
            if (_SelectionManager != null)
                _SelectionManager.ACUrlCommand("!OnVBControlClicked", _LastClickedControl, Keyboard.IsKeyDown(Key.LeftCtrl));
        }
        #endregion

        #region Attached-Property IsSelectableProperty

        /// <summary>
        /// Represents the enumeration for IsSelectable.
        /// </summary>
        public enum IsSelectableEnum : short
        {
            Unset = 0,
            True = 1,
            False = 2,
        }

        /// <summary>
        /// Represents the dependency property for IsSelectable.
        /// </summary>
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.RegisterAttached("IsSelectable",
                                                                    typeof(IsSelectableEnum),
                                                                    typeof(VBDesignBase),
                                                                    new FrameworkPropertyMetadata(IsSelectableEnum.Unset,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Sets the IsSelectable.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="value">The value parameter.</param>
        [AttachedPropertyBrowsableForChildren]
        public static void SetIsSelectable(UIElement element, IsSelectableEnum value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDesignBase.IsSelectableProperty, value);
        }

        /// <summary>
        /// Gets the IsSelectable.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <returns>The IsSelectable enumeration item.</returns>
        [AttachedPropertyBrowsableForChildren]
        public static IsSelectableEnum GetIsSelectable(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (IsSelectableEnum)element.GetValue(VBDesignBase.IsSelectableProperty);
        }
        #endregion

        #region Dependency-Property SelectedVBControl
        /// <summary>
        /// Gets or sets the SelectedVBControl.
        /// </summary>
        public IVBContent SelectedVBControl
        {
            get { return (IVBContent)GetValue(SelectedVBControlProperty); }
            set { SetValue(SelectedVBControlProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for SelectedVBControl.
        /// </summary>
        public static readonly DependencyProperty SelectedVBControlProperty =
          DependencyProperty.Register("SelectedVBControl",
                                       typeof(IVBContent),
                                       typeof(VBDesignBase),
                                       new PropertyMetadata(new PropertyChangedCallback(OnSelectedVBControlChanged)));

        private static void OnSelectedVBControlChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is VBDesignBase)
            {
                VBDesignBase vbDesign = d as VBDesignBase;
                vbDesign.OnSelectedVBControlChanged();
            }
        }
        #endregion

        #region Selection-Methods
        /// <summary>
        /// Highlights the VBControl.
        /// </summary>
        /// <param name="vbControlToSelect">The vbControl to select(highlight).</param>
        /// <param name="isMultiSelect">Determines is multi selection or single selection.</param>
        [ACMethodInfo("", "en{'HighlightVBControl'}de{'HighlightVBControl'}", 9999)]
        public void HighlightVBControl(IVBContent vbControlToSelect, bool isMultiSelect)
        {
            if (vbControlToSelect == null)
            {
                AdornVBControl(null, isMultiSelect);
                return;
            }

            // Falls geklicktes Control, das hervorgehoben werden soll
            if (_LastClickedControl == vbControlToSelect)
            {
                AdornVBControl(_LastClickedControl, isMultiSelect);
            }
            // Sonst suche im Tree nach Control
            else
            {
                if (vbControlToSelect is DependencyObject)
                {
                    if (VBLogicalTreeHelper.IsChildObjectInLogicalTree(this, vbControlToSelect as DependencyObject, typeof(VBDesignBase)))
                    {
                        AdornVBControl(vbControlToSelect, isMultiSelect);
                        return;
                    }
                }
                // Nicht gefunden, dann wird dieses Control nicht in diesem VBDesign verwaltet sondern in einem anderen
                AdornVBControl(null, isMultiSelect);
            }
        }

        /// <summary>
        /// Method to Select Visual-Control over ACComponent
        /// </summary>
        /// <param name="acObjectToSelect">The acObject to select(highlight).</param>
        [ACMethodInfo("", "en{'HighlightContentACObject'}de{'HighlightContentACObject'}", 9999)]
        public void HighlightContentACObject(IACObject acObjectToSelect, bool highlightParentIfNotFound)
        {
            if (acObjectToSelect == null)
            {
                SetSelectionAtManager(null);
                AdornVBControl(null, false);
                return;
            }

            if (_LastClickedControl != null)
            {
                if (HasVBControlContent(_LastClickedControl, acObjectToSelect))
                {
                    HighlightVBControl(_LastClickedControl, false);
                    return;
                }
            }
            IVBContent foundContent = FindContentACObjectInLogicalTree(this, acObjectToSelect);
            if (foundContent == null && highlightParentIfNotFound)
            {
                IACObject parentACObject = acObjectToSelect.ParentACObject;
                while (parentACObject != null)
                {
                    foundContent = FindContentACObjectInLogicalTree(this, parentACObject);
                    if (foundContent != null)
                        break;
                    parentACObject = parentACObject.ParentACObject;
                }
            }
            // Falls dieses VBDesign, das selecktierte Element beeinhaltet, dann setze selektiertes Element auf dem Selection-Manager
            // dieser wiederum ruft die Methode HighlightVBControl() auf
            if (foundContent != null)
                SetSelectionAtManager(foundContent);
            // Nicht gefunden, dann wird dieses Control nicht in diesem VBDesign verwaltet sondern in einem anderen
            else
                AdornVBControl(null, false);
        }

        //private static IVBContent FindContentACObjectInLogicalTree(IVBContent vbControlStart, IACObject acObjectContent)
        //{
        //    if ((vbControlStart == null) || (acObjectContent == null))
        //        return null;
        //    if (HasVBControlContent(vbControlStart, acObjectContent))
        //        return vbControlStart;
        //    if (!(vbControlStart is DependencyObject))
        //        return null;
        //    foreach (object childObj in LogicalTreeHelper.GetChildren(vbControlStart as DependencyObject))
        //    {
        //        if (childObj is IVBContent)
        //        {
        //            IVBContent found = FindContentACObjectInLogicalTree(childObj as IVBContent, acObjectContent);
        //            if (found != null)
        //                return found;
        //        }
        //        else if (childObj is DependencyObject)
        //        {
        //            DependencyObject foundDepObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(childObj as DependencyObject, typeof(IVBContent));
        //            if (foundDepObj != null)
        //            {
        //                IVBContent found = FindContentACObjectInLogicalTree(foundDepObj as IVBContent, acObjectContent);
        //                if (found != null)
        //                    return found;
        //            }
        //        }
        //        else
        //            continue;
        //    }
        //    return null;
        //}

        private static IVBContent FindContentACObjectInLogicalTree(DependencyObject depObjStart, IACObject acObjectContent)
        {
            if ((depObjStart == null) || (acObjectContent == null))
                return null;
            IVBContent vbControlStart = depObjStart as IVBContent;
            if (vbControlStart != null)
            {
                if (HasVBControlContent(vbControlStart, acObjectContent))
                    return vbControlStart;
            }
            if (!(depObjStart is DependencyObject))
                return null;
            foreach (object childObj in LogicalTreeHelper.GetChildren(depObjStart as DependencyObject))
            {
                DependencyObject childDepObj = childObj as DependencyObject;
                if (childDepObj != null)
                {
                    IVBContent found = FindContentACObjectInLogicalTree(childDepObj, acObjectContent);
                    if (found != null)
                        return found;
                }
                //if (childObj is IVBContent)
                //{
                //    IVBContent found = FindContentACObjectInLogicalTree(childObj as IVBContent, acObjectContent);
                //    if (found != null)
                //        return found;
                //}
                //else if (childObj is DependencyObject)
                //{
                //    DependencyObject foundDepObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(childObj as DependencyObject, typeof(IVBContent));
                //    if (foundDepObj != null)
                //    {
                //        IVBContent found = FindContentACObjectInLogicalTree(foundDepObj as IVBContent, acObjectContent);
                //        if (found != null)
                //            return found;
                //    }
                //}
                else
                    continue;
            }
            return null;
        }

        private static bool HasVBControlContent(IVBContent vbControl, IACObject acObjectContent)
        {
            if ((vbControl == null) || (acObjectContent == null))
                return false;
            if (vbControl.ACContentList == null)
                return false;
            if (vbControl.ACContentList.Where(c => c == acObjectContent).Any())
                return true;
            return false;
        }

        /// <summary>
        /// Create screen shot and resize image to 370x250
        /// </summary>
        /// <returns>Image in byte array</returns>
        public byte[] PrintScreenToImage(object[] parameters)
        {
            return PrintScreenAndRezize(370, 250, parameters);
        }


        public byte[] PrintScreenToIcon(object[] parameters)
        {
            return PrintScreenAndRezize(32, 32, parameters);
        }

        private byte[] PrintScreenAndRezize(double width, double height, object[] parameters)
        {
            string vbContentChild = null;
            if (parameters != null && parameters.Count() > 1)
                vbContentChild = parameters[1] as string;
            UIElement uiElement = this.Content as UIElement;

            Canvas canvas = null;
            byte[] arr = new byte[] { 0 };
            if (!String.IsNullOrEmpty(vbContentChild))
            {
                UIElement childFound = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(uiElement, vbContentChild) as UIElement;
                if (childFound == null)
                    return arr;
                uiElement = childFound;
            }

            if (uiElement is Canvas)
                canvas = uiElement as Canvas;
            else if (uiElement is ScrollViewer)
            {
                ScrollViewer scrollViewer = uiElement as ScrollViewer;
                if (scrollViewer.Content is Canvas)
                    canvas = scrollViewer.Content as Canvas;
            }
            else if (uiElement is VBVisual)
            {
                VBVisual vbVisual = uiElement as VBVisual;
                canvas = VBVisualTreeHelper.FindChildObjectInVisualTree(vbVisual.Content as UIElement, typeof(Canvas)) as Canvas;
            }


            if (canvas != null && !Double.IsNaN(canvas.Width) && !Double.IsNaN(canvas.Height))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    canvas.Arrange(new Rect(canvas.RenderSize));
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(canvas);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    TransformedBitmap targetBitmap = new TransformedBitmap(rtb, new ScaleTransform(width / canvas.Width, height / canvas.Height));
                    encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                    encoder.Save(stream);
                    arr = stream.ToArray();
                    double widthPoint = (((FrameworkElement)canvas.Parent).ActualWidth - canvas.Width) / 2;
                    double heighPoint = (((FrameworkElement)canvas.Parent).ActualHeight - canvas.Height) / 2;
                    if (widthPoint > 0 && heighPoint > 0)
                        canvas.Arrange(new Rect(new System.Windows.Point(widthPoint, heighPoint), canvas.RenderSize));
                }
            }
            return arr;
        }

        /// <summary>
        /// Create screen shot and resize image to 370x250
        /// </summary>
        /// <returns>Image in byte array</returns>
        private Bitmap CurrentScreenToBitmap()
        {
            Bitmap bitmap = null;

            UIElement uiElement = this.Content as UIElement;
            ScrollViewer scrollViewer = null;
            VBCanvas vbCanvas = null;
            byte[] arr = new byte[] { 0 };

            if (uiElement is VBCanvas)
                vbCanvas = uiElement as VBCanvas;

            else if (uiElement is ScrollViewer)
                scrollViewer = uiElement as ScrollViewer;

            if (scrollViewer != null && scrollViewer.Content is VBCanvas)
                vbCanvas = scrollViewer.Content as VBCanvas;

            if (vbCanvas != null && !Double.IsNaN(vbCanvas.Width) && !Double.IsNaN(vbCanvas.Height))
            {

                vbCanvas.Arrange(new Rect(vbCanvas.RenderSize));
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)vbCanvas.Width, (int)vbCanvas.Height, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(vbCanvas);
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));
                    encoder.Save(stream);
                    bitmap = new Bitmap(stream);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Prints screen to the clipboard.
        /// </summary>
        /// <param name="bitmap">The bitmap parameter.</param>
        public void PrintScreenToClipboard(Bitmap bitmap)
        {
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            System.Windows.Clipboard.SetImage(bitmapSource);
        }

        #endregion

        #region Adorner-Layer
        protected List<AdornerVBControlManager> adornVBControlManagerList;
        /// <summary>
        /// Gets the list of AdornVBControlManager.
        /// </summary>
        public List<AdornerVBControlManager> AdornVBControlManagerList
        {
            get
            {
                if (adornVBControlManagerList == null)
                    adornVBControlManagerList = new List<AdornerVBControlManager>();
                return adornVBControlManagerList;
            }
        }

        public System.Windows.Media.Color Invert(System.Windows.Media.Color color)
        {
            return System.Windows.Media.Color.FromRgb((byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
        }

        /// <summary>
        /// Adorns the VBControl.
        /// </summary>
        /// <param name="vbControlToAdorn">The vbControl to adorn.</param>
        /// <param name="isMultiSelect">Determines is multi selection or single selection.</param>
        protected void AdornVBControl(IVBContent vbControlToAdorn, bool isMultiSelect)
        {
            //this.Background
            if (ShowAdornerLayer && !IsDesignerActive)
            {
                System.Windows.Media.Color selectionColor = System.Windows.Media.Colors.Red;
                System.Windows.Media.Color selectionColor2 = System.Windows.Media.Colors.White;
                //DependencyObject controlAsVisual = vbControlToAdorn as DependencyObject;
                //if (controlAsVisual != null)
                //{
                //    DependencyObject depObj = VisualTreeHelper.GetParent(controlAsVisual);
                //    Panel panel = depObj as Panel;
                //    if (panel != null)
                //    {
                //        //GradientBrush brush2;
                //        System.Windows.Media.Color bgColor = System.Windows.Media.Colors.Red;
                //        SolidColorBrush brush = panel.Background as SolidColorBrush;
                //        if (brush != null)
                //            selectionColor = Invert(brush.Color);
                //        else
                //        {
                //            GradientBrush brush2 = panel.Background as GradientBrush;
                //            if (brush2 != null)
                //                selectionColor = Invert(brush2.GradientStops.FirstOrDefault().Color);
                //        }
                //    }
                //    else
                //    {
                //        Control parentControl = depObj as Control;
                //        //VBVisualTreeHelper.FindParentObjectInVisualTree(VisualTreeHelper.GetParent(controlAsVisual), typeof(Control)) as Control;
                //        if (parentControl != null)
                //        {
                //            //GradientBrush brush2;
                //            System.Windows.Media.Color bgColor = System.Windows.Media.Colors.Red;
                //            SolidColorBrush brush = parentControl.Background as SolidColorBrush;
                //            if (brush != null)
                //                selectionColor = Invert(brush.Color);
                //            else
                //            {
                //                GradientBrush brush2 = parentControl.Background as GradientBrush;
                //                if (brush2 != null)
                //                    selectionColor = Invert(brush2.GradientStops.FirstOrDefault().Color);
                //            }
                //        }
                //    }
                //}

                if (!isMultiSelect)
                {
                    foreach (var item in AdornVBControlManagerList)
                        item.RemoveAdornerFromElement();
                    AdornVBControlManagerList.Clear();
                }

                AdornerVBControlManager adornManagerForControl = AdornVBControlManagerList.FirstOrDefault(x => x.ControlToAdorn == vbControlToAdorn);
                if (adornManagerForControl != null)
                {
                    adornManagerForControl.RemoveAdornerFromElement();
                    adornManagerForControl.AddAdornerToElement(selectionColor);
                }
                else
                {
                    adornManagerForControl = new AdornerVBControlManager(vbControlToAdorn, selectionColor);
                    AdornVBControlManagerList.Add(adornManagerForControl);
                }

                foreach (var item in AdornVBControlManagerList)
                {
                    if (item.ControlToAdorn != vbControlToAdorn && item.LastUsedColor == selectionColor)
                    {
                        item.RemoveAdornerFromElement();
                        item.AddAdornerToElement(selectionColor2);
                    }
                }
            }
            SelectedVBControl = vbControlToAdorn;
        }

        protected void OnSelectedVBControlChanged()
        {
        }
        #endregion

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
            return this.ReflectGetMenu(this);
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            // Ende der WPF-GetMenu-Rekursion, wenn hier angelagt
            ACMenuItemList acMenuList = this.ReflectGetMenu(this);
            if ((acMenuList != null) && (acMenuList.Count > 0))
            {
                foreach (var acMenu in acMenuList)
                    acMenuItemList.Add(acMenu);
                //acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
            }
        }
        #endregion

        #region CanExecute Dispatcher
        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Additional Dependency Prop

        /// <summary>
        /// Represents the dependency property for RegisterVBDecorator.
        /// </summary>
        public static DependencyProperty RegisterVBDecoratorProperty
            = ContentPropertyHandler.RegisterVBDecoratorProperty.AddOwner(typeof(VBDesignBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets or sets the RegisterVBDecorator.
        /// </summary>
        [Category("VBControl")]
        public string RegisterVBDecorator
        {
            get { return (string)GetValue(RegisterVBDecoratorProperty); }
            set { SetValue(RegisterVBDecoratorProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for BSOIsActivated.
        /// </summary>
        public static readonly DependencyProperty BSOIsActivatedProperty =
            DependencyProperty.Register("BSOIsActivated", typeof(string), typeof(VBDesignBase));


        /// <summary>
        /// This property is used for VBDesign dynamic visibility. In this property we set name of bool property defined in BSO which is responsible for visibility BSO and VBDesign.
        /// If bool property in BSO is true, BSO and VBDesign are visible, otherwise BSO and VBDesign are hidden.
        /// </summary>
        [Category("VBControl")]
        public string BSOIsActivated
        {
            get { return (string)GetValue(BSOIsActivatedProperty); }
            set { SetValue(BSOIsActivatedProperty, value); }
        }

        #endregion

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
    }

}
