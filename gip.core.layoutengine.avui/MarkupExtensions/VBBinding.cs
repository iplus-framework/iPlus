using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Xaml;
using System.Windows.Data;
using gip.core.datamodel;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace gip.core.layoutengine.avui
{
    public class VBBinding : Binding
    {
        public VBBinding()
            : base()
        {
        }

        public VBBinding(string path)
            : base(path)
        {
        }

        ~VBBinding()
        {
            if (Layoutgenerator.Root != null && Layoutgenerator.Root.Businessobjects != null)
                Layoutgenerator.Root.Businessobjects.FindAndRemoveWPFRef(this.GetHashCode());
        }

        public bool BindToBSO
        {
            get;
            set;
        }

        private string _VBContent;
        public string VBContent
        {
            get
            {
                return _VBContent;
            }

            set
            {
                _VBContent = value;
                IACObject context = null;
                if (BindToBSO)
                    context = Layoutgenerator.CurrentBSO;
                else
                    context = Layoutgenerator.CurrentDataContext;

                if ((context != null) && !String.IsNullOrEmpty(_VBContent))
                {
                    object dcSource = null;
                    string dcPath = "";
                    if (context.ACUrlBinding(_VBContent, ref _dcACTypeInfo, ref dcSource, ref dcPath, ref _dcRightControlMode))
                    {
                        this.Source = dcSource;
                        this.Path = new PropertyPath(dcPath);
                        this.NotifyOnSourceUpdated = true;
                        this.NotifyOnTargetUpdated = true;
                        if (_dcACTypeInfo != null)
                        {
                            bool isInput = true;
                            if (_dcACTypeInfo is ACClassProperty)
                                isInput = (_dcACTypeInfo as ACClassProperty).IsInput;
                            if (this.Mode == BindingMode.Default)
                                this.Mode = isInput ? BindingMode.TwoWay : BindingMode.OneWay;
                        }
                        if (Layoutgenerator.CurrentBSO != null)
                            Layoutgenerator.CurrentBSO.AddWPFRef(this.GetHashCode(), dcSource as IACObject);
                    }
                    else
                    {
                        if (Layoutgenerator.Root != null)
                        {
                            this.Source = Layoutgenerator.Root;
                            this.Path = new PropertyPath(Const.ACIdentifierPrefix);
                            this.Mode = BindingMode.OneWay;
                        }
                        else
                        {
                            this.Source = Layoutgenerator.CurrentDataContext;
                            this.Path = new PropertyPath(Const.ACIdentifierPrefix);
                            this.Mode = BindingMode.OneWayToSource;
                        }
                    }
                }
            }
        }

        private IACType _dcACTypeInfo;
        public IACType dcACTypeInfo
        {
            get
            {
                return _dcACTypeInfo;
            }
        }

        private Global.ControlModes _dcRightControlMode = Global.ControlModes.Hidden;
        public Global.ControlModes dcRightControlMode
        {
            get
            {
                return _dcRightControlMode;
            }
        }
    }

    [MarkupExtensionReturnType(typeof(object))]
    public class VBBindingExt : MarkupExtension
    {
        private Binding binding = new Binding();

        public VBBindingExt()
        {
            if (binding == null)
                binding = new Binding();
        }

        public VBBindingExt(string path)
        {
            binding = new Binding(path);
        }


        #region BindingBase

        [DefaultValue(null)]
        public string StringFormat
        {
            get { return binding.StringFormat; }
            set { binding.StringFormat = value; }
        }

        [DefaultValue("")]
        public string BindingGroupName
        {
            get { return binding.BindingGroupName; }
            set { binding.BindingGroupName = value; }
        }

        /// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"></see> property on instances of this class.</summary>
        /// <returns>true if the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"></see> property value should be serialized; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeFallbackValue()
        {
            return (binding.ShouldSerializeFallbackValue());
        }

        /// <summary>Gets or sets the value to use when the binding is unable to return a value.</summary>
        /// <returns>The default value is <see cref="F:System.Windows.DependencyProperty.UnsetValue"></see>.</returns>
        [DefaultValue(null)]
        public object FallbackValue
        {
            get { return binding.FallbackValue; }
            set { binding.FallbackValue = value; }
        }

        #endregion

        #region Binding

        [Browsable(false)]
        public Binding Binding
        {
            get { return binding; }
            set { binding = value; }
        }

        /// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.Path"></see> property should be persisted.</summary>
        /// <returns>true if the property value has changed from its default; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializePath()
        {
            return binding.ShouldSerializePath();
        }
        /// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.Source"></see> property should be persisted.</summary>
        /// <returns>true if the property value has changed from its default; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSource()
        {
            return binding.ShouldSerializeSource();
        }
        /// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.ValidationRules"></see> property should be persisted.</summary>
        /// <returns>true if the property value has changed from its default; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeValidationRules()
        {
            return binding.ShouldSerializeValidationRules();
        }

        /// <summary>
        /// This member is not is not intended to be used directly from your code.
        /// </summary>
        [DefaultValue((string)null)]
        public object AsyncState
        {
            get { return binding.AsyncState; }
            set { binding.AsyncState = value; }
        }

        /// <summary>Gets or sets a value that indicates whether to evaluate the <see cref="P:System.Windows.Data.Binding.Path"></see> relative to the data item or the <see cref="T:System.Windows.Data.DataSourceProvider"></see> object.</summary>
        /// <returns>false to evaluate the path relative to the data item itself; otherwise, true. The default value is false.</returns>
        [DefaultValue(false)]
        public bool BindsDirectlyToSource
        {
            get { return binding.BindsDirectlyToSource; }
            set { binding.BindsDirectlyToSource = value; }
        }

        /// <summary>Gets or sets the converter to use.</summary>
        /// <returns>A value of type <see cref="T:System.Windows.Data.IValueConverter"></see>. The default value is null.</returns>
        [DefaultValue(null)]
        public IValueConverter Converter
        {
            get { return binding.Converter; }
            set { binding.Converter = value; }
        }

        [DefaultValue(null)]
        public object TargetNullValue
        {
            get { return binding.TargetNullValue; }
            set { binding.TargetNullValue = value; }
        }

        /// <summary>Gets or sets the culture in which to evaluate the converter.</summary>
        /// <returns>The default value is null.</returns>
        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture
        {
            get { return binding.ConverterCulture; }
            set { binding.ConverterCulture = value; }
        }

        /// <summary>Gets or sets the parameter to pass to the <see cref="P:System.Windows.Data.Binding.Converter"></see>.</summary>
        /// <returns>The parameter to pass to the <see cref="P:System.Windows.Data.Binding.Converter"></see>. The default value is null.</returns>
        [DefaultValue(null)]
        public object ConverterParameter
        {
            get { return binding.ConverterParameter; }
            set { binding.ConverterParameter = value; }
        }

        /// <summary>Gets or sets the name of the element to use as the binding source object.</summary>
        /// <returns>The value of the Name property or x:Name Attribute of the element of interest. You can refer only to elements that are created in code if they are registered to the appropriate <see cref="T:System.Windows.NameScope"></see> through RegisterName. For more information, see WPF Namescopes.The default value is null.</returns>
        [DefaultValue((string)null)]
        public string ElementName
        {
            get { return binding.ElementName; }
            set { binding.ElementName = value; }
        }

        /// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Data.Binding"></see> should get and set values asynchronously.</summary>
        /// <returns>The default value is null.</returns>
        [DefaultValue(false)]
        public bool IsAsync
        {
            get { return binding.IsAsync; }
            set { binding.IsAsync = value; }
        }

        /// <summary>Gets or sets a value that indicates the direction of the data flow in the binding.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Data.BindingMode"></see> values. The default value is <see cref="F:System.Windows.Data.BindingMode.Default"></see>, which returns the default binding mode value of the target dependency property. However, the default value varies for each dependency property. In general, user-editable control properties, such as those of text boxes and check boxes, default to two-way bindings, whereas most other properties default to one-way bindings.A programmatic way to determine whether a dependency property binds one-way or two-way by default is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)"></see> and then check the Boolean value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.BindsTwoWayByDefault"></see> property.</returns>
        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get { return binding.Mode; }
            set { binding.Mode = value; }
        }

        /// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Data.Binding.SourceUpdated"></see> event when a value is transferred from the binding target to the binding source.</summary>
        /// <returns>true if the <see cref="E:System.Windows.Data.Binding.SourceUpdated"></see> event should be raised when the binding source value is updated; otherwise, false. The default value is false.</returns>
        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated
        {
            get { return binding.NotifyOnSourceUpdated; }
            set { binding.NotifyOnSourceUpdated = value; }
        }

        /// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Data.Binding.TargetUpdated"></see> event when a value is transferred from the binding source to the binding target.</summary>
        /// <returns>true if the <see cref="E:System.Windows.Data.Binding.TargetUpdated"></see> event should be raised when the binding target value is updated; otherwise, false. The default value is false.</returns>
        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated
        {
            get { return binding.NotifyOnTargetUpdated; }
            set { binding.NotifyOnTargetUpdated = value; }
        }

        /// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Controls.Validation.Error"></see> attached event on the bound object.</summary>
        /// <returns>true if the <see cref="E:System.Windows.Controls.Validation.Error"></see> attached event should be raised on the bound object when there is a validation error during source updates; otherwise, false. The default value is false.</returns>
        [DefaultValue(false)]
        public bool NotifyOnValidationError
        {
            get { return binding.NotifyOnValidationError; }
            set { binding.NotifyOnValidationError = value; }
        }

        /// <summary>Gets or sets the path to the binding source property.</summary>
        /// <returns>The path to the binding source. The default value is null.</returns>
        public PropertyPath Path
        {
            get { return binding.Path; }
            set { binding.Path = value; }
        }

        /// <summary>Gets or sets the binding source by specifying its location relative to the position of the binding target.</summary>
        /// <returns>A <see cref="T:System.Windows.Data.RelativeSource"></see> object specifying the relative location of the binding source to use. The default value is null.</returns>
        [DefaultValue((string)null)]
        public RelativeSource RelativeSource
        {
            get { return binding.RelativeSource; }
            set { binding.RelativeSource = value; }
        }

        /// <summary>Gets or sets the object to use as the binding source.</summary>
        /// <returns>The object to use as the binding source.</returns>
        public object Source
        {
            get { return binding.Source; }
            set { binding.Source = value; }
        }

        /// <summary>Gets or sets a handler you can use to provide custom logic for handling exceptions that the binding engine encounters during the update of the binding source value. This is only applicable if you have associated an <see cref="T:System.Windows.Controls.ExceptionValidationRule"></see> with your binding.</summary>
        /// <returns>A method that provides custom logic for handling exceptions that the binding engine encounters during the update of the binding source value.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
        {
            get { return binding.UpdateSourceExceptionFilter; }
            set { binding.UpdateSourceExceptionFilter = value; }
        }

        /// <summary>Gets or sets a value that determines the timing of binding source updates.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Data.UpdateSourceTrigger"></see> values. The default value is <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default"></see>, which returns the default <see cref="T:System.Windows.Data.UpdateSourceTrigger"></see> value of the target dependency property. However, the default value for most dependency properties is <see cref="F:System.Windows.Data.UpdateSourceTrigger.PropertyChanged"></see>, while the <see cref="P:System.Windows.Controls.TextBox.Text"></see> property has a default value of <see cref="F:System.Windows.Data.UpdateSourceTrigger.LostFocus"></see>.A programmatic way to determine the default <see cref="P:System.Windows.Data.Binding.UpdateSourceTrigger"></see> value of a dependency property is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)"></see> and then check the value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.DefaultUpdateSourceTrigger"></see> property.</returns>
        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return binding.UpdateSourceTrigger; }
            set { binding.UpdateSourceTrigger = value; }
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get { return binding.ValidatesOnDataErrors; }
            set { binding.ValidatesOnDataErrors = value; }
        }

        [DefaultValue(false)]
        public bool ValidatesOnExceptions
        {
            get { return binding.ValidatesOnExceptions; }
            set { binding.ValidatesOnExceptions = value; }
        }

        /// <summary>Gets or sets an XPath query that returns the value on the XML binding source to use.</summary>
        /// <returns>The XPath query. The default value is null.</returns>
        [DefaultValue((string)null)]
        public string XPath
        {
            get { return binding.XPath; }
            set { binding.XPath = value; }
        }

        /// <summary>Gets a collection of rules that check the validity of the user input.</summary>
        /// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationRule"></see> objects.</returns>
        public Collection<ValidationRule> ValidationRules
        {
            get { return binding.ValidationRules; }
        }
        #endregion

        public override object ProvideValue(IServiceProvider provider)
        {
            DependencyObject target;
            DependencyProperty dp;
            TryGetTargetItems(provider, out target, out dp);

            if (!_VBInitialized && !String.IsNullOrEmpty(_VBContent) && (Layoutgenerator.CurrentDataContext != null))
            {
                IACObject dataContext = Layoutgenerator.CurrentDataContext;
                if (target != null)
                {
                    try
                    {
                        dataContext = target.GetValue(FrameworkElement.DataContextProperty) as IACObject;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBBindingExt", "ProvideValue", msg);
                    }
                }
                if (target == null)
                    return this;


                if (dataContext == null)
                    dataContext = Layoutgenerator.CurrentDataContext;


                object dcSource = null;
                string dcPath = "";
                if (dataContext.ACUrlBinding(_VBContent, ref _dcACTypeInfo, ref dcSource, ref dcPath, ref _dcRightControlMode))
                {
                    this.Source = dcSource;
                    this.Path = new PropertyPath(dcPath);
                    this.NotifyOnSourceUpdated = true;
                    this.NotifyOnTargetUpdated = true;
                    if (_dcACTypeInfo != null)
                    {
                        bool isInput = true;
                        if (_dcACTypeInfo is ACClassProperty)
                            isInput = (_dcACTypeInfo as ACClassProperty).IsInput;
                        if (this.Mode == BindingMode.Default)
                        {
                            this.Mode = isInput ? BindingMode.TwoWay : BindingMode.OneWay;
                        }
                    }
                }
                else
                {
                    if (Layoutgenerator.Root != null)
                    {
                        this.Source = Layoutgenerator.Root;
                        this.Path = new PropertyPath(Const.ACIdentifierPrefix);
                        this.Mode = BindingMode.OneWay;
                    }
                    else
                    {
                        this.Source = Layoutgenerator.CurrentDataContext;
                        this.Path = new PropertyPath(Const.ACIdentifierPrefix);
                        this.Mode = BindingMode.OneWayToSource;
                    }
                }
            }

            _VBInitialized = true;
            return binding.ProvideValue(provider);
        }

        protected virtual bool TryGetTargetItems(IServiceProvider provider, out DependencyObject target, out DependencyProperty dp)
        {
            target = null;
            dp = null;
            if (provider == null)
                return false;

            //create a binding and assign it to the target
            IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null) return false;

            //we need dependency objects / properties
            target = service.TargetObject as DependencyObject;
            dp = service.TargetProperty as DependencyProperty;
            return target != null && dp != null;
        }

        private bool _VBInitialized = false;

        private string _VBContent;
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

        private IACType _dcACTypeInfo;
        public IACType dcACTypeInfo
        {
            get
            {
                return _dcACTypeInfo;
            }
        }

        private Global.ControlModes _dcRightControlMode = Global.ControlModes.Hidden;
        public Global.ControlModes dcRightControlMode
        {
            get
            {
                return _dcRightControlMode;
            }
        }
    }
}
