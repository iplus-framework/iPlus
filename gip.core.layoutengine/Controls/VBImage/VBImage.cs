using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using gip.core.datamodel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Transactions;
using System.Windows.Media.Imaging;
using System.IO;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control element for displaying an image.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung eines Image
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBImage'}de{'VBImage'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBImage : Image, IVBContent, IACObject
    {
        /// <summary>
        /// Creates a new instance of VBImage.
        /// </summary>
        public VBImage()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);
            base.OnInitialized(e);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }

        #region Loaded Event

        bool _Initialized = false;

        /// <summary>
        /// Handles OnLoaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
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
            if (ContextACObject == null || String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBImage", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                Visibility = Visibility.Collapsed;
            }
            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = new PropertyPath(dcPath);
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.Mode = BindingMode.OneWay;
            SetBinding(VBImage.ACClassDesignProperty, binding);

            binding = new Binding();
            binding.Source = dcSource;
            dcPath += ".DesignBinary";
            binding.Path = new PropertyPath(dcPath);
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.Mode = BindingMode.OneWay;
            SetBinding(VBImage.DesignBinaryProperty, binding);


            //if (BSOACComponent != null)
            //{
            //    binding = new Binding();
            //    binding.Source = BSOACComponent;
            //    binding.Path = new PropertyPath(Const.InitState);
            //    binding.Mode = BindingMode.OneWay;
            //    SetBinding(VBImage.ACCompInitStateProperty, binding);
            //}

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                {
                }
                else
                {
                    UpdateControlMode();
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
            _VBContentPropertyInfo = null;
            this.Loaded -= OnLoaded;
            
            BindingOperations.ClearBinding(this, VBImage.ACClassDesignProperty);
            //BindingOperations.ClearBinding(this, VBImage.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        //protected void InitStateChanged()
        //{
        //    if (BSOACComponent != null &&
        //        (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
        //        DeInitVBControl(BSOACComponent);
        //}
        #endregion

        #region IACInteractiveObject Member
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
        /// Represents the dependecy property for ACClassDesign.
        /// </summary>
        public static readonly DependencyProperty ACClassDesignProperty
            = DependencyProperty.Register("ACClassDesign", typeof(ACClassDesign), typeof(VBImage), new PropertyMetadata(new PropertyChangedCallback(OnACClassDesignChanged)));

        //private ACClassDesign _LastACClassDesign;
        /// <summary>
        /// Gets or sets the ACClassDesign.
        /// </summary>
        public ACClassDesign ACClassDesign
        {
            get { return (ACClassDesign)GetValue(ACClassDesignProperty); }
            set { SetValue(ACClassDesignProperty, value); }
        }

        /// <summary>
        /// Represents the DesignBinary.
        /// </summary>
        public static readonly DependencyProperty DesignBinaryProperty
            = DependencyProperty.Register("DesignBinary", typeof(object), typeof(VBImage), new PropertyMetadata(new PropertyChangedCallback(OnACClassDesignChanged)));

        /// <summary>
        /// Gets or sets the DesignBinary.
        /// </summary>
        public object DesignBinary
        {
            get { return (object)GetValue(DesignBinaryProperty); }
            set { SetValue(DesignBinaryProperty, value); }
        }


        private static void OnACClassDesignChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is VBImage)
            {
                VBImage vbImage = d as VBImage;
                vbImage.UpdateDesign();
            }
        }

        /// <summary>
        /// Updates the design.
        /// </summary>
        public void UpdateDesign()
        {
            if (ContextACObject == null)
                return;
            if (ACClassDesign == null)
                this.Source = null;
            else
                UpdateImage();
        }

        private void UpdateImage()
        {
            if (ACClassDesign == null)
                return;
            if ((ACClassDesign.ACKind == Global.ACKinds.DSBitmapResource) && (ACClassDesign.DesignBinary != null))
            {
                BitmapImage bitmapImage = null;
                try
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(ACClassDesign.DesignBinary);
                    bitmapImage.EndInit();
                }
                catch (Exception e)
                {
                    this.Root().Messages.LogException("VBImage", "UpdateImage()", VBContent);
                    this.Root().Messages.LogException("VBImage", "UpdateImage()", String.Format("Can't create icon for {0}. Invalid Binary", ACClassDesign.GetACUrl()));
                    bitmapImage = new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine;component/Images/QuestionMark.JPG", UriKind.Absolute));
                }
                this.Source = bitmapImage;
            }
            else
                this.Source = null;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBImage));

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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
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
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBImage), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        ///// <summary>
        ///// Represents the dependency property for ACCompInitState.
        ///// </summary>
        ////public static readonly DependencyProperty ACCompInitStateProperty =
        ////    DependencyProperty.Register("ACCompInitState",
        ////        typeof(ACInitState), typeof(VBImage),
        ////        new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        ///// <summary>
        ///// Gets or sets the ACCompInitState.
        ///// </summary>
        ////public ACInitState ACCompInitState
        ////{
        ////    get { return (ACInitState)GetValue(ACCompInitStateProperty); }
        ////    set { SetValue(ACCompInitStateProperty, value); }
        ////}

        ////private static void OnDepPropChanged(DependencyObject dependencyObject,
        ////       DependencyPropertyChangedEventArgs args)
        ////{
        ////    VBImage thisControl = dependencyObject as VBImage;
        ////    if (args.Property == ACCompInitStateProperty)
        ////        thisControl.InitStateChanged();
        ////}

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
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBImage));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return (Global.ControlModes)GetValue(ControlModeProperty); }
            set { SetValue(ControlModeProperty, value); }
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
            if (Enabled)
            {
                ControlMode = Global.ControlModes.Enabled;
            }
            else
            {
                ControlMode = Global.ControlModes.Disabled;
            }
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
                IsEnabled = value;
            }
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
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBImage));
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
    }
}
