using Avalonia.Controls;
using Avalonia.Data;
using Avalonia;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control element for displaying a name with a colored background.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung einer Bezeichnung mit farblichem Hintergrund.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBHeader'}de{'VBHeader'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBHeader : TemplatedControl, IVBContent, IACObject
    {
        #region c'tors

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty;

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty;

        static VBHeader()
        {
            ACCaptionProperty = AvaloniaProperty.Register<VBHeader, string>(Const.ACCaptionPrefix);
            ACCaptionTransProperty = AvaloniaProperty.Register<VBHeader, string>(nameof(ACCaptionTrans));
            
            ACCaptionProperty.Changed.AddClassHandler<VBHeader>((x, e) => x.OnACCaptionChanged(e));
            ACCompInitStateProperty.Changed.AddClassHandler<VBHeader>((x, e) => x.InitStateChanged());
            BSOACComponentProperty.Changed.AddClassHandler<VBHeader>((x, e) => x.OnBSOACComponentChanged(e));
        }

        public VBHeader() : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        #endregion

        #region IDataField Members

        string _VBContent;
        /// <summary>
        /// Gets or sets the name of property which is intended for header(caption) in this control. Use this property when you want show caption from a BSO.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
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
        /// Enable or disable translation from parent design.
        /// </summary>
        [Category("VBControl")]
        public bool TranslateFromParentDesign
        {
            get;
            set;
        }

        #region Caption

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        private void OnACCaptionChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (!_Initialized)
                return;
            if (ACCaption != null && ACCaption.Contains("{'") && ACCaption.Contains("'}"))
                ACCaptionTrans = Translator.GetTranslation("VBTabItem", ACCaption);
            else
            {
                IACObject translationContext = ContextACObject;
                if (TranslateFromParentDesign)
                {
                    VBDesign vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesign)) as VBDesign;
                    if (vbDesign != null)
                    {
                        ACClassDesign acClassDesign = vbDesign.ContentACObject as ACClassDesign;
                        if (acClassDesign != null)
                            translationContext = acClassDesign.ACClass;
                    }
                }

                ACCaptionTrans = this.Root().Environment.TranslateText(translationContext, ACCaption);
            }
        }

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        public string ACCaptionTrans
        {
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }
        #endregion
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
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBHeader>();
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
            AvaloniaProperty.Register<VBHeader, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private void OnBSOACComponentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == null && e.OldValue != null && !String.IsNullOrEmpty(this.VBContent))
            {
                IACBSO bso = e.OldValue as IACBSO;
                if (bso != null)
                    this.DeInitVBControl(bso);
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
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }
        #endregion


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
            _Initialized = true;

            if (ContextACObject == null)
            {
                ACCaptionTrans = ACCaption;
                return;
            }
            if (string.IsNullOrEmpty(VBContent))
            {
                if (!string.IsNullOrEmpty(ACCaption))
                {
                    if (ACCaption.Contains("{'") && ACCaption.Contains("'}"))
                        ACCaptionTrans = Translator.GetTranslation(ACIdentifier, ACCaption, this.Root().Environment.VBLanguageCode);
                    else
                    {
                        IACObject translationContext = ContextACObject;
                        if (TranslateFromParentDesign)
                        {
                            VBDesign vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesign)) as VBDesign;
                            if (vbDesign != null)
                            {
                                ACClassDesign acClassDesign = vbDesign.ContentACObject as ACClassDesign;
                                if (acClassDesign != null)
                                    translationContext = acClassDesign.ACClass;
                            }
                        }

                        ACCaptionTrans = this.Root().Environment.TranslateText(translationContext, ACCaption);
                    }
                }
                return;
            }

            RightControlMode = Global.ControlModes.Disabled;

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                IsVisible = false;
            }
            //ucTextblock.Text = PropertySchema.DataTypeCaption;
            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBHeader", VBContent);
                return;
            }

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = dcPath;
            binding.Mode = BindingMode.OneWay;
            // Note: NotifyOnSourceUpdated and NotifyOnTargetUpdated don't exist in Avalonia
            this.Bind(VBHeader.ACCaptionTransProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBHeader.ACCompInitStateProperty, binding);
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
                // In Avalonia, we can't use BindingOperations.GetBinding in the same way
                // This logic would need to be adapted if the WPF reference tracking is needed
                try
                {
                    // Simplified logic - the original WPF reference tracking may need alternative implementation
                    if (BSOACComponent != null)
                        BSOACComponent.AddWPFRef(this.GetHashCode(), ContextACObject);
                }
                catch (Exception exw)
                {
                    this.Root().Messages.LogDebug("VBHeader", "AddWPFRef", exw.Message);
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
                BSOACComponent.RemoveWPFRef(this.GetHashCode());

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
            _Initialized = false;
            _VBContentPropertyInfo = null;

            this.ClearValue(VBHeader.ACCaptionTransProperty);
            this.ClearValue(VBHeader.ACCompInitStateProperty);
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
    }
}
