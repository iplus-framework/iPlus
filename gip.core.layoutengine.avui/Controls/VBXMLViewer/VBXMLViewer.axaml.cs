using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Transactions;
using System.Xml;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Controls.Primitives;
using gip.core.layoutengine.avui.Helperclasses;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control element for displaying XML.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelemnt zur Anzeige von XML.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBXMLViewer'}de{'VBXMLViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBXMLViewer : UserControl, IVBContent, IACObject
    {
        /// <summary>
        /// Creates a new instance of VBXMLViewer.
        /// </summary>
        public VBXMLViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }



        bool _Initialized = false;
        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;

            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", VBContent);
                return;
            }
            _ACTypeInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            if (String.IsNullOrEmpty(VBContent))
                return;

            ContentXML = ContextACObject.ACUrlCommand(VBContent) as XmlDocument;

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = dcPath;
            binding.Mode = BindingMode.OneWay;

            this.Bind(VBXMLViewer.ContentXMLProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBXMLViewer.ACCompInitStateProperty, binding);
            }

            _XMLEditor.ChangeSyntaxHighlighting();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
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
            _ACTypeInfo = null;

            this.ClearAllBindings();
        }

        /// <summary>
        /// Invoked on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        public static readonly StyledProperty<XmlDocument> ContentXMLProperty = AvaloniaProperty.Register<VBXMLViewer, XmlDocument>(nameof(ContentXML));
        [Category("VBControl")]
        public XmlDocument ContentXML
        {
            get
            {
                return (XmlDocument)GetValue(ContentXMLProperty);
            }
            set
            {
                SetValue(ContentXMLProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBXMLViewer>();
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
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = AvaloniaProperty.Register<VBXMLViewer, ACInitState>(nameof(ACCompInitState));
        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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
            else if (change.Property == ACCaptionProperty)
            {
                if (!_Initialized)
                    return;
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
            }
            else if (change.Property == ContentXMLProperty)
            {
                if (change.NewValue != null && change.NewValue is XmlDocument xmlDoc)
                {
                    _XMLEditor.Text = xmlDoc.InnerText;

                    // Avalonia doesn't support XPath for Binding to XMLDataProvider and displaying XML with a Hierachical datatemplate
                    // Maybe in Future there will be a control for it
                    //this.Bind(VBTextEditor.VBTextProperty, binding);
                    //BindingOperations.ClearBinding(vbContentControl._XMLEditor, TreeView.ItemsSourceProperty);
                    //XmlDataProvider provider = new XmlDataProvider();
                    //provider.Document = e.NewValue as XmlDocument;
                    //Binding binding = new Binding();
                    //binding.Source = provider;
                    //binding.XPath = "child::node()";
                    //vbContentControl._XMLEditor.SetBinding(TreeView.ItemsSourceProperty, binding);
                }
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
                if (VBContentPropertyInfo != null)
                {
                    if (VBContentPropertyInfo.IsNullable)
                    {
                        ControlMode = Global.ControlModes.Enabled;
                    }
                    else
                    {
                        ControlMode = Global.ControlModes.EnabledRequired;
                    }
                }
                else
                    ControlMode = Global.ControlModes.Disabled;
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
                return this.IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject == null)
                    {
                        this.IsEnabled = true;
                    }
                    else
                    {
                        this.IsEnabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                }
                else
                {
                    this.IsEnabled = false;
                }
                ControlModeChanged();
                this.IsTabStop = this.IsEnabled;
            }
        }

        /// <summary>
        /// Represents the dependency property for the VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBXMLViewer, string>(nameof(VBContent));

        /// <summary>
        /// Represents the property where you enter the name of BSO's property which contains XML content or path to file with XML content.
        /// Set VBContentIsXML property to true if you set this property to XML content.
        /// Set VBContentIsXML property to false if you set this property to file path with XML content.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
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
        /// Represents the styled property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBXMLViewer, string>(Const.ACCaptionPrefix);

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get => GetValue(ACCaptionProperty);
            set => SetValue(ACCaptionProperty, value);
        }

        /// <summary>
        /// Represents the styled property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBXMLViewer, string>(nameof(ACCaptionTrans));

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        public string ACCaptionTrans
        {
            get => GetValue(ACCaptionTransProperty);
            set => SetValue(ACCaptionTransProperty, value);
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
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
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
            get { return Parent as IACObject; }
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


        #region Additional Dependency Properties

        #region ControlMode
        /// <summary>
        /// Represents the styled property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBXMLViewer, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get => GetValue(ControlModeProperty);
            set => SetValue(ControlModeProperty, value);
        }
        #endregion
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
