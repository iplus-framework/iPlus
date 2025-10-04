using Avalonia.Controls;
using Avalonia.Data;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Avalonia.Interactivity;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
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
        /// Represents the dependecy property for ACClassDesign.
        /// </summary>
        public static readonly StyledProperty<ACClassDesign> ACClassDesignProperty;

        /// <summary>
        /// Represents the DesignBinary.
        /// </summary>
        public static readonly StyledProperty<object> DesignBinaryProperty;

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty;

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty;

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty;

        /// <summary>
        /// Represents the dependency property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty;

        static VBImage()
        {
            ACClassDesignProperty = AvaloniaProperty.Register<VBImage, ACClassDesign>(nameof(ACClassDesign));
            DesignBinaryProperty = AvaloniaProperty.Register<VBImage, object>(nameof(DesignBinary));
            VBContentProperty = AvaloniaProperty.Register<VBImage, string>(nameof(VBContent));
            BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBImage>();
            ControlModeProperty = AvaloniaProperty.Register<VBImage, Global.ControlModes>(nameof(ControlMode));
            DisabledModesProperty = AvaloniaProperty.Register<VBImage, string>(nameof(DisabledModes));

            ACClassDesignProperty.Changed.AddClassHandler<VBImage>((x, e) => x.UpdateDesign());
            DesignBinaryProperty.Changed.AddClassHandler<VBImage>((x, e) => x.UpdateDesign());
        }

        /// <summary>
        /// Creates a new instance of VBImage.
        /// </summary>
        public VBImage()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            this.Loaded += OnLoaded;
            base.OnInitialized();
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
                IsVisible = false;
            }
            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = dcPath;
            // Note: NotifyOnSourceUpdated and NotifyOnTargetUpdated don't exist in Avalonia
            binding.Mode = BindingMode.OneWay;
            this.Bind(VBImage.ACClassDesignProperty, binding);

            binding = new Binding();
            binding.Source = dcSource;
            dcPath += ".DesignBinary";
            binding.Path = dcPath;
            // Note: NotifyOnSourceUpdated and NotifyOnTargetUpdated don't exist in Avalonia
            binding.Mode = BindingMode.OneWay;
            this.Bind(VBImage.DesignBinaryProperty, binding);

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
            Source = null;
            
            this.ClearValue(VBImage.ACClassDesignProperty);
            this.ClearAllBindings();
        }

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
        /// Gets or sets the ACClassDesign.
        /// </summary>
        public ACClassDesign ACClassDesign
        {
            get { return GetValue(ACClassDesignProperty); }
            set { SetValue(ACClassDesignProperty, value); }
        }

        /// <summary>
        /// Gets or sets the DesignBinary.
        /// </summary>
        public object DesignBinary
        {
            get { return GetValue(DesignBinaryProperty); }
            set { SetValue(DesignBinaryProperty, value); }
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
                Bitmap bitmapImage = null;
                try
                {
                    using (var stream = new MemoryStream(ACClassDesign.DesignBinary))
                    {
                        bitmapImage = new Bitmap(stream);
                        this.Source = bitmapImage;
                    }
                }
                catch (Exception e)
                {
                    this.Root().Messages.LogException("VBImage", "UpdateImage()", e);
                    this.Root().Messages.LogException("VBImage", "UpdateImage()", String.Format("Can't create icon for {0} at VBContent {1}. Invalid Binary", ACClassDesign.GetACUrl(), VBContent));
                    bitmapImage = new Bitmap(AssetLoader.Open(new Uri("avares://gip.core.layoutengine.avui/Images/QuestionMark.JPG")));
                    this.Source = bitmapImage;
                }
            }
            else
                this.Source = null;
        }

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
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return GetValue(ControlModeProperty); }
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
            IsVisible = controlMode >= Global.ControlModes.Disabled;
        }

        /// <summary>
        /// Gets or sets the DisabledModes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die DisabledModes.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }
    }
}
