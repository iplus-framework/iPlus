using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.ComponentModel;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for displaying objects that implement the IACValue interface.
    /// XAML Attributes: VBContent: Relative or absolute ACUrl to an IACValue
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Objekten, die das Interface IACValue implementieren.
    /// XAML-Attribute: VBContent: Relative oder absolute ACUrl zu einem IACValue
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDynamicContent'}de{'VBDynamicContent'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDynamicContent : VBDesignBase, IVBContent, IACMenuBuilderWPFTree
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBDynamicContent.
        /// </summary>
        public VBDynamicContent()
            : base()
        {
            ShowConfigValue = false;
        }
        #endregion

        #region Control Loaded-Event
        bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        internal override void InitVBControl()
        {
            base.InitVBControl();
            if (_Loaded)
                return;

            // If Binding to Object wich should be presented manually set, then the Binding must be done over VBContent
            BindingExpressionBase boundedValue = BindingOperations.GetBindingExpressionBase(this, VBDynamicContent.DesignVBContentProperty);
            if (boundedValue != null)
            {
            }
            else if (BSOACComponent != null && !string.IsNullOrEmpty(VBContent))
            {
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                if (BSOACComponent.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                {
                    _VBContentPropertyInfo = dcACTypeInfo;
                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = dcPath;
                    binding.Mode = BindingMode.OneWay;
                    this.Bind(VBDynamicContent.DesignVBContentProperty, binding);
                }
                else
                {
                    if (ContextACObject != null && ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {
                        _VBContentPropertyInfo = dcACTypeInfo;
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        binding.Mode = BindingMode.OneWay;
                        this.Bind(VBDynamicContent.DesignVBContentProperty, binding);
                    }
                    else
                    {
                        Binding binding = new Binding();
                        binding.Source = DataContext;
                        binding.Path = VBContent;
                        binding.Mode = BindingMode.OneWay;
                        this.Bind(VBDynamicContent.DesignVBContentProperty, binding);
                    }
                }
                LoadDesign();
            }
            _Loaded = true;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if (_Loaded)
            {
                Content = null;
                if (IsSet(DataContextProperty))
                    DataContext = null;
                this.ClearAllBindings();
                DesignVBContent = null;
            }
            
            base.DeInitVBControl(bso);
            //_Loaded = false;
        }

        #endregion

        #region Dynamic XAML over Dependency-Property
        private bool _bindingToValueTypeSet = false;
        private bool _SettingBinding = false;
        void LoadDesign()
        {
            if (_SettingBinding)
                return; 
            string xaml = "";
            object newDataContextForChild = null;
            if (ShowConfigValue)
            {
                if (DesignVBContent is ACClassProperty)
                {
                    ACClassProperty acValue = DesignVBContent as ACClassProperty;
                    if (acValue != null)
                    {
                        if (!_bindingToValueTypeSet)
                        {
                            _SettingBinding = true;
                            _bindingToValueTypeSet = true;
                            try
                            {
                                Binding binding = new Binding();
                                binding.Source = DataContext;
                                binding.Path = VBContent + ".ValueTypeACClass";
                                binding.Mode = BindingMode.OneWay;
                                this.Bind(VBDynamicContent.ValueTypeProperty, binding);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("VBDynamicContent", "LoadDesign", msg);
                            }
                            _SettingBinding = false;
                        }
                        var design = acValue.ConfigACClass.GetDesign(acValue.ConfigACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                        if (design != null && design.ValueTypeACClass != null)
                        {
                            string controlName = design.ValueTypeACClass.ACIdentifier;
                            if (acValue.ConfigACClass.ACKind == Global.ACKinds.TACLRBaseTypes || acValue.ConfigACClass.ACKind == Global.ACKinds.TACEnum)
                            {
                                string vbContentUrl = VBContent;
                                // If DesignVBContent is Bound over classic binding in XAML
                                if (String.IsNullOrEmpty(this.VBContent))
                                {
                                    newDataContextForChild = DesignVBContent;
                                    vbContentUrl = "ConfigValue";
                                }
                                // Else VBContent was set
                                else if (!vbContentUrl.Contains("\\ConfigValue"))
                                    vbContentUrl += "\\ConfigValue";

                                object newContext = DetermineBindableDataContextForVBControl(vbContentUrl);
                                if (newContext != null)
                                    SetBindingToDataContext(newContext, this);

                                double height = design.VisualHeight;
                                if (height < 0.00001
                                    && (controlName.Contains("TextBlock")
                                        || controlName.Contains("TextBox")
                                        || controlName.Contains("DatePicker")
                                        || controlName.Contains("DateTimePicker")
                                        || controlName.Contains("ComboBox")))
                                    height = 30;

                                if (height > 0.00001)
                                    xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\" Height=\"{2}\"><vb:{0} VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl, height);
                                else
                                    xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\"><vb:{0} VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl);
                            }
                            else
                            {
                                object newContext = GetObjectForVBContent(VBContent);
                                if (newContext != null)
                                    SetBindingToDataContext(newContext, this);
                                //DataContext = BSOACComponent.ACUrlCommand(this.VBContent);
                                //if (DataContext == null && BSOACComponent is IACSelectDependentDlg)
                                //{
                                //    IACSelectDependentDlg selDlg = BSOACComponent as IACSelectDependentDlg;
                                //    if (selDlg != null)
                                //    {
                                //        DataContext = selDlg.CurrentSelection.ACUrlCommand(this.VBContent);
                                //        if (DataContext == null)
                                //            DataContext = selDlg.CurrentSelection;
                                //    }
                                //}
                                xaml = design.XAMLDesign;
                            }
                        }
                    }
                }
            }
            else
            {
                if (DesignVBContent is IACContainer)
                {
                    IACContainer acValue = DesignVBContent as IACContainer;
                    if (acValue != null)
                    {
                        if (!_bindingToValueTypeSet)
                        {
                            _SettingBinding = true;
                            _bindingToValueTypeSet = true;
                            try
                            {
                                Binding binding = new Binding();
                                binding.Source = DataContext;
                                binding.Path = VBContent + ".ValueTypeACClass";
                                binding.Mode = BindingMode.OneWay;
                                this.Bind(VBDynamicContent.ValueTypeProperty, binding);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("VBDynamicContent", "LoadDesign(10)", msg);
                            }
                            _SettingBinding = false;
                        }
                    } 
                    if (acValue != null && acValue.ValueTypeACClass != null)
                    {
                        var design = acValue.ValueTypeACClass.GetDesign(acValue.ValueTypeACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                        if (design != null && design.ValueTypeACClass != null)
                        {
                            string vbContentUrl = VBContent;
                            // If DesignVBContent is Bound over classic binding in XAML
                            if (String.IsNullOrEmpty(this.VBContent))
                            {
                                newDataContextForChild = DesignVBContent;
                                vbContentUrl = Const.Value;
                            }
                            // Else VBContent was set
                            else if (!vbContentUrl.Contains(ACUrlHelper.Delimiter_DirSeperator + Const.Value))
                                    vbContentUrl += ACUrlHelper.Delimiter_DirSeperator + Const.Value;

                            string controlName = design.ValueTypeACClass.ACIdentifier;
                            if (String.IsNullOrWhiteSpace(design.XAMLDesign))
                            {
                                switch (acValue.ValueTypeACClass.ACKind)
                                {
                                    case Global.ACKinds.TACLRBaseTypes:
                                    case Global.ACKinds.TACEnum:
                                        {
                                            object newContext = DetermineBindableDataContextForVBControl(vbContentUrl);
                                            if (newContext != null)
                                                SetBindingToDataContext(newContext, this);

                                            double height = design.VisualHeight;
                                            if (height < 0.00001
                                                && (controlName.Contains("TextBlock")
                                                    || controlName.Contains("TextBox")
                                                    || controlName.Contains("DatePicker")
                                                    || controlName.Contains("DateTimePicker")
                                                    || controlName.Contains("ComboBox")))
                                                height = 30;

                                            //Array enumArray = null;
                                            string vbSource = null;
                                            if (   acValue.ValueTypeACClass.ACKind == Global.ACKinds.TACEnum 
                                                && acValue.ValueTypeACClass.ObjectType != null)
                                            {
                                                //enumArray = Enum.GetValues(acValue.ValueTypeACClass.ObjectType);
                                                if (acValue.ValueTypeACClass.ACValueListForEnum == null)
                                                    vbSource = String.Format("\\!{0}(#{1}\\{1}#)", Const.MN_GetEnumList, acValue.ValueTypeACClass.ObjectType.AssemblyQualifiedName);
                                                else
                                                {
                                                    newDataContextForChild = DesignVBContent;
                                                    vbContentUrl = Const.Value;
                                                }
                                                if (controlName.Contains("TextBox"))
                                                    controlName = typeof(VBComboBox).Name;
                                            }

                                            if (!ShowCaption && typeof(IVBContent).IsAssignableFrom(design.ValueTypeACClass.ObjectType))
                                            {
                                                if (height > 0.00001)
                                                    xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\" Height=\"{2}\"><vb:{0} ShowCaption=\"False\" VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl, height);
                                                else
                                                    xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\"><vb:{0} ShowCaption=\"False\" VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl);
                                            }
                                            else
                                            {
                                                if (height > 0.00001)
                                                {
                                                    if (!String.IsNullOrEmpty(vbSource) && controlName.Contains("ComboBox"))
                                                        xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\" Height=\"{3}\"><vb:{0} VBContent=\"{1}\" VBSource=\"{2}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl, vbSource, height);
                                                    else
                                                        xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\" Height=\"{2}\"><vb:{0} VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl, height);
                                                }
                                                else
                                                {
                                                    if (!String.IsNullOrEmpty(vbSource) && controlName.Contains("ComboBox"))
                                                        xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\"><vb:{0} VBContent=\"{1}\" VBSource=\"{2}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl, vbSource);
                                                    else
                                                        xaml = string.Format("<vb:VBGrid VerticalAlignment=\"Top\"><vb:{0} VBContent=\"{1}\"></vb:{0}></vb:VBGrid>", controlName, vbContentUrl);
                                                }
                                            }

                                        }
                                        break;
                                    case Global.ACKinds.TACDBA:
                                        {
                                            object newContext = DetermineBindableDataContextForVBControl(vbContentUrl);
                                            if (newContext != null)
                                                SetBindingToDataContext(newContext, this);

                                            xaml += string.Format("<vb:{0} ACCaption=\"Value\" VBContent=\"{1}\" VBSource=\"" + Const.ContextDatabase + "\\{2}\"></vb:{0}>", controlName, vbContentUrl, acValue.ValueTypeACClass.ACIdentifier);
                                        }
                                        break;
                                    default:
                                        {
                                            if (newDataContextForChild == null)
                                            {
                                                object newContext = GetObjectForVBContent(vbContentUrl);
                                                if (newContext != null)
                                                    SetBindingToDataContext(newContext, this);
                                            }
                                            else
                                            {
                                                newDataContextForChild = newDataContextForChild.GetValue(Const.Value);
                                            }
                                            xaml = design.XAMLDesign;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                if (newDataContextForChild == null)
                                {
                                    object newContext = GetObjectForVBContent(vbContentUrl);
                                    if (newContext != null)
                                        SetBindingToDataContext(newContext, this);
                                }
                                else
                                {
                                    newDataContextForChild = newDataContextForChild.GetValue(Const.Value);
                                }
                                xaml = design.XAMLDesign;
                            }
                        }
                    }
                }
                else if (VBContentPropertyInfo != null)
                {
                    var design = VBContentPropertyInfo.ValueTypeACClass.GetDesign(VBContentPropertyInfo.ValueTypeACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                    if (design != null && design.ValueTypeACClass != null)
                    {
                        xaml = design.XAMLDesign;
                    }
                    else if (BSOACComponent != null && !String.IsNullOrEmpty(this.VBContent))
                    {
                        object value = BSOACComponent.ACUrlCommand(this.VBContent);
                        if (value != null)
                        {
                            Type type = value.GetType();
                            var valueType = gip.core.datamodel.Database.GlobalDatabase.GetACType(type);
                            if (valueType != null)
                            {
                                design = VBContentPropertyInfo.ValueTypeACClass.GetDesign(valueType, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                                if (design != null && design.ValueTypeACClass != null)
                                {
                                    xaml = design.XAMLDesign;
                                }
                            }
                        }
                    }

                    if (DesignVBContent != null)
                        newDataContextForChild = DesignVBContent;
                    else
                    {
                        object objectToBind = null;
                        if (ContextACObject != null && !String.IsNullOrEmpty(this.VBContent))
                        {
                            objectToBind = ContextACObject.ACUrlCommand(this.VBContent);
                            if (objectToBind != null)
                            {
                                SetBindingToDataContext(objectToBind, this);
                            }
                        }
                        if (objectToBind != null && BSOACComponent != null && !String.IsNullOrEmpty(this.VBContent))
                        {
                            objectToBind = BSOACComponent.ACUrlCommand(this.VBContent);
                            if (objectToBind != null)
                                SetBindingToDataContext(objectToBind, this);
                        }
                    }
                }
                else if (DesignVBContent != null)
                {
                    Type typeOfContent = DesignVBContent.GetType();
                    if (typeOfContent != null)
                    {
                        var valueType = typeOfContent.GetACType();
                        if (valueType != null)
                        {
                            var design = valueType.GetDesign(valueType, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                            if (design != null && design.ValueTypeACClass != null)
                            {
                                xaml = design.XAMLDesign;
                                newDataContextForChild = DesignVBContent;
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(xaml))
            {
                if (string.IsNullOrEmpty(xaml))
                {
                    xaml = "<DockPanel></DockPanel>";
                }
            }
            Visual uiElement = Layoutgenerator.LoadLayout(xaml, ContextACObject, BSOACComponent, VBContent);
            if (newDataContextForChild != null && uiElement is Control)
                SetBindingToDataContext(newDataContextForChild, uiElement as Control);

            Content = uiElement;
        }

        protected object DetermineBindableDataContextForVBControl(string vbContentUrl)
        {
            object newContext = null;
            Control parentFW = Parent as Control;
            if (parentFW != null)
            {
                IACObject parentACObject = parentFW.DataContext as IACObject;
                if (parentACObject != null)
                {
                    IACType acType = null;
                    string acPath = null;
                    object source = null;
                    Global.ControlModes controlMode = Global.ControlModes.Hidden;
                    if (parentACObject.ACUrlBinding(vbContentUrl, ref acType, ref source, ref acPath, ref controlMode))
                        newContext = parentACObject;
                }
            }
            if (newContext == null)
            {
                IACObject parentACObject = parentFW.DataContext as IACObject;
                if (parentACObject != null)
                {
                    IACType acType = null;
                    string acPath = null;
                    object source = null;
                    Global.ControlModes controlMode = Global.ControlModes.Hidden;
                    if (BSOACComponent.ACUrlBinding(vbContentUrl, ref acType, ref source, ref acPath, ref controlMode))
                        newContext = BSOACComponent;
                }
                if (newContext == null && BSOACComponent is IACSelectDependentDlg)
                {
                    IACSelectDependentDlg selDlg = BSOACComponent as IACSelectDependentDlg;
                    if (selDlg != null)
                    {
                        IACType acType = null;
                        string acPath = null;
                        object source = null;
                        Global.ControlModes controlMode = Global.ControlModes.Hidden;
                        if (selDlg.CurrentSelection.ACUrlBinding(vbContentUrl, ref acType, ref source, ref acPath, ref controlMode))
                            newContext = selDlg.CurrentSelection;
                    }
                }
            }
            return newContext;
        }

        protected object GetObjectForVBContent(string vbContentUrl)
        {
            object newContext = null;
            Control parentFW = Parent as Control;
            if (parentFW != null)
            {
                IACObject parentACObject = parentFW.DataContext as IACObject;
                if (parentACObject != null)
                {
                    newContext = parentACObject.ACUrlCommand(vbContentUrl);
                }
            }
            if (newContext == null)
            {
                newContext = BSOACComponent.ACUrlCommand(vbContentUrl);
                if (newContext == null && BSOACComponent is IACSelectDependentDlg)
                {
                    IACSelectDependentDlg selDlg = BSOACComponent as IACSelectDependentDlg;
                    if (selDlg != null)
                    {
                        newContext = selDlg.CurrentSelection.ACUrlCommand(this.VBContent);
                        if (newContext == null)
                            newContext = selDlg.CurrentSelection;
                    }
                }
            }
            return newContext;
        }

        protected void SetBindingToDataContext(object newContext, Control targetElement)
        {
            Binding binding = new Binding();
            binding.Source = newContext;
            targetElement.Bind(Control.DataContextProperty, binding);
        }

        /// <summary>
        /// Gets or sets the DesignVBContent.
        /// </summary>
        public object DesignVBContent
        {
            get 
            {
                return GetValue(DesignVBContentProperty); 
            }
            set
            {
                SetValue(DesignVBContentProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for DesignVBContent.
        /// </summary>
        public static readonly StyledProperty<object> DesignVBContentProperty = AvaloniaProperty.Register<VBDynamicContent, object>(nameof(DesignVBContent));

        /// <summary>
        /// Represents the dependency property for ValueType.
        /// </summary>
        public static readonly StyledProperty<object> ValueTypeProperty = AvaloniaProperty.Register<VBDynamicContent, object>(nameof(ValueType));

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == DesignVBContentProperty)
            {
                LoadDesign();
            }
            else if (change.Property == ValueTypeProperty)
            {
                LoadDesign();
            }
            base.OnPropertyChanged(change);
        }
        #endregion

        #region Overridden IACInteractiveObject Member

        #endregion

        #region IVBContent Member
        #endregion

        #region IACInteractiveObject Member

        /// <summary>
        /// Gets or sets the VBDynamicACComponent.
        /// </summary>
        [Category("VBControl")]
        public string VBDynamicACComponent
        {
            get;
            set;
        }
        #endregion

        #region IACObject Member

        protected string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public override string ACCaption
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

        /// <summary>
        /// Represents the dependency property for ShowConfigValue.
        /// </summary>

        public static readonly StyledProperty<bool> ShowConfigValueProperty = AvaloniaProperty.Register<VBComboBox, bool>(nameof(ShowConfigValue));

        /// <summary>
        /// Detrmines is configuration value shown or hidden.
        /// </summary>
        [Category("VBControl")]
        public bool ShowConfigValue
        {
            get { return (bool)GetValue(ShowConfigValueProperty); }
            set { SetValue(ShowConfigValueProperty, value); }
        }

        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowCaptionProperty = AvaloniaProperty.Register<VBComboBox, bool>(nameof(ShowCaption));        

    }
}
