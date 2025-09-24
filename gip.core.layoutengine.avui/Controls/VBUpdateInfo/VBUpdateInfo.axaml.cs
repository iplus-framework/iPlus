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
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control elemet to display the installation and update data of a data set.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelemet zur Anzeige der Anlage- und Aktualisierungsdaten eines Datensatzes.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBUpdateInfo'}de{VBUpdateInfo''}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBUpdateInfo : UserControl, IVBContent, IACObject
    {
        string _VBContent;

        /// <summary>
        /// Creates a new instance of the VBUpdateInfo.
        /// </summary>
        public VBUpdateInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }


        #region IDataField Members

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                return _VBContent;
            }
            set
            {
                _VBContent = value;
            }
        }

        /// <summary>
        /// Gets or sets the ACIdentifier.
        /// </summary>
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
            get;set;
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBUpdateInfo), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
        ////        typeof(ACUrlCmdMessage), typeof(VBUpdateInfo),
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
                typeof(ACInitState), typeof(VBUpdateInfo),
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
            VBUpdateInfo thisControl = dependencyObject as VBUpdateInfo;
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

        //public string ToolTip
        //{
        //    get
        //    {
        //        return ucTextblock.ToolTip as string;
        //    }
        //    set
        //    {
        //        ucTextblock.ToolTip = value;
        //    }
        //}


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

        IACType _VBContentProprtyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentProprtyInfo as ACClassProperty;
            }
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
        public string DisabledModes { get; set; }
        #endregion


        bool _Loaded = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Loaded)
                return;
            _Loaded = true;
            RightControlMode = Global.ControlModes.Disabled;
            if (VBContent == null || VBContent == "")
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                Binding binding1 = new Binding();
                binding1.Path = new PropertyPath(VBContent + ".InsertName");
                binding1.Mode = BindingMode.OneWay;
                binding1.NotifyOnSourceUpdated = true;
                binding1.NotifyOnTargetUpdated = true;
                ucInsertName.SetBinding(TextBlock.TextProperty, binding1);

                Binding binding2 = new Binding();
                binding2.Path = new PropertyPath(VBContent + ".InsertDate");
                binding2.Mode = BindingMode.OneWay;
                binding2.NotifyOnSourceUpdated = true;
                binding2.NotifyOnTargetUpdated = true;
                ucInsertDate.SetBinding(TextBlock.TextProperty, binding2);

                Binding binding3 = new Binding();
                binding3.Path = new PropertyPath(VBContent + ".UpdateName");
                binding3.Mode = BindingMode.OneWay;
                binding3.NotifyOnSourceUpdated = true;
                binding3.NotifyOnTargetUpdated = true;
                ucUpdateName.SetBinding(TextBlock.TextProperty, binding3);

                Binding binding4 = new Binding();
                binding4.Path = new PropertyPath(VBContent + ".UpdateDate");
                binding4.Mode = BindingMode.OneWay;
                binding4.NotifyOnSourceUpdated = true;
                binding4.NotifyOnTargetUpdated = true;
                ucUpdateDate.SetBinding(TextBlock.TextProperty, binding4);

                if (BSOACComponent != null)
                {
                    Binding binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBUpdateInfo.ACCompInitStateProperty, binding);
                }

                if (Visibility == Visibility.Visible)
                {
                    if (RightControlMode < Global.ControlModes.Disabled)
                    {
                        Visibility = Visibility.Collapsed;
                    }
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
            if (!_Loaded)
                return;
            _Loaded = false;
            _VBContentProprtyInfo = null;
            
            BindingOperations.ClearBinding(ucInsertName, TextBlock.TextProperty);
            BindingOperations.ClearBinding(ucInsertDate, TextBlock.TextProperty);
            BindingOperations.ClearBinding(ucUpdateName, TextBlock.TextProperty);
            BindingOperations.ClearBinding(ucUpdateDate, TextBlock.TextProperty);
            //BindingOperations.ClearBinding(this, VBUpdateInfo.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBUpdateInfo.ACCompInitStateProperty);
            this.ClearAllBindings();
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
