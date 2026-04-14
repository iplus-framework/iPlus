using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        // Tracks whether the binding is in design/declaration mode fallback.
        // When true, ProvideValue skips target.Bind() to prevent Avalonia's stricter
        // binding engine from publishing type-conversion errors (e.g. string "Root" → bool)
        // that would abort XAML rendering.
        private bool _isDesignFallback = false;

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
                // Do not permanently resolve here using global context.
                // In Avalonia, markup extensions can be created before the final target DataContext
                // is stable (especially in nested templates). ProvideValue re-resolves against the
                // concrete target control context to avoid stale bindings.
                _isDesignFallback = false;
            }
        }

        private bool TryResolveBinding(IACObject context, out object dcSource, out string dcPath, out BindingMode resolvedMode)
        {
            dcSource = null;
            dcPath = "";
            resolvedMode = this.Mode;

            if (context == null || String.IsNullOrEmpty(_VBContent))
                return false;

            if (!context.ACUrlBinding(_VBContent, ref _dcACTypeInfo, ref dcSource, ref dcPath, ref _dcRightControlMode))
                return false;

            if (_dcACTypeInfo != null)
            {
                bool isInput = true;
                if (_dcACTypeInfo is ACClassProperty)
                    isInput = (_dcACTypeInfo as ACClassProperty).IsInput;
                if (this.Mode == BindingMode.Default)
                    resolvedMode = isInput ? BindingMode.TwoWay : BindingMode.OneWay;
            }

            if (Layoutgenerator.CurrentBSO != null)
                Layoutgenerator.CurrentBSO.AddWPFRef(this.GetHashCode(), dcSource as IACObject);

            return true;
        }

        private static string ResolveTypeName(object value)
        {
            return value == null ? "<null>" : value.GetType().Name;
        }

        private static readonly ConditionalWeakTable<AvaloniaObject, HashSet<AvaloniaProperty>> _attachedRebindTargets =
            new ConditionalWeakTable<AvaloniaObject, HashSet<AvaloniaProperty>>();

        private Binding CreateResolvedBinding(object dcSource, string dcPath, BindingMode resolvedMode)
        {
            return new Binding
            {
                Source = dcSource,
                Path = dcPath,
                Mode = resolvedMode,
                Priority = this.Priority,
                Delay = this.Delay,
                FallbackValue = this.FallbackValue,
                StringFormat = this.StringFormat,
                Converter = this.Converter,
                ConverterCulture = this.ConverterCulture,
                ConverterParameter = this.ConverterParameter,
                TargetNullValue = this.TargetNullValue,
                UpdateSourceTrigger = this.UpdateSourceTrigger
            };
        }

        private void EnsureDataContextRebind(StyledElement target, AvaloniaProperty dp)
        {
            HashSet<AvaloniaProperty> attachedProperties = _attachedRebindTargets.GetOrCreateValue(target);
            lock (attachedProperties)
            {
                if (attachedProperties.Contains(dp))
                    return;
                attachedProperties.Add(dp);
            }

            target.PropertyChanged += (s, e) =>
            {
                if (e.Property != StyledElement.DataContextProperty)
                    return;

                IACObject nextContext = e.NewValue as IACObject;
                if (nextContext == null)
                    return;

                if (!TryResolveBinding(nextContext, out object nextSource, out string nextPath, out BindingMode nextMode))
                {
                    if (dp == ToolTip.TipProperty)
                        target.SetValue(dp, null);
                    return;
                }

                var resolvedBinding = CreateResolvedBinding(nextSource, nextPath, nextMode);
                target.Bind(dp, resolvedBinding);

                // FOR TRACING:
                // Database.Root.Messages.LogDebug(
                //     "VBBindingExt",
                //     "ProvideValue",
                //     $"VBBinding DataContext Rebind OK Target={target.GetType().Name}:{target.Name}; Property={dp.Name}; VBContent={_VBContent}; Context={ResolveTypeName(nextContext)}; Source={ResolveTypeName(nextSource)}; Path={nextPath}; Mode={nextMode}");
            };
        }

        private void TryDeferredRebind(StyledElement target, AvaloniaProperty dp)
        {
            EventHandler<AvaloniaPropertyChangedEventArgs> handler = null;
            handler = (s, e) =>
            {
                if (e.Property != StyledElement.DataContextProperty)
                    return;

                IACObject nextContext = e.NewValue as IACObject;
                if (nextContext == null)
                    return;

                if (!TryResolveBinding(nextContext, out object nextSource, out string nextPath, out BindingMode nextMode))
                    return;

                target.PropertyChanged -= handler;

                var resolvedBinding = CreateResolvedBinding(nextSource, nextPath, nextMode);
                target.Bind(dp, resolvedBinding);

                string targetName = target.Name;
                string targetType = target.GetType().Name;
                // FOR TRACING:
                // Database.Root.Messages.LogDebug("VBBindingExt", "ProvideValue", $"VBBinding Deferred Rebind OK Target={targetType}:{targetName}; Property={dp.Name}; VBContent={_VBContent}; Context={ResolveTypeName(nextContext)}; Source={ResolveTypeName(nextSource)}; Path={nextPath}; Mode={nextMode}");
            };

            target.PropertyChanged += handler;
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

        public object ProvideValue(IServiceProvider provider)
        {
            if (provider == null)
                return this;

            IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null)
                return this;

            AvaloniaObject target = service.TargetObject as AvaloniaObject;
            AvaloniaProperty dp = service.TargetProperty as AvaloniaProperty;
            
            if (target == null || dp == null)
                return this;

            // Re-resolve against the real target context at apply time.
            // This avoids stale global-context bindings during nested XAML loading.
            IACObject context = null;
            StyledElement styledTarget = target as StyledElement;
            if (styledTarget != null)
                context = styledTarget.DataContext as IACObject;

            if (context == null)
                context = BindToBSO ? Layoutgenerator.CurrentBSO : Layoutgenerator.CurrentDataContext;

            object dcSource;
            string dcPath;
            BindingMode resolvedMode;
            _isDesignFallback = !TryResolveBinding(context, out dcSource, out dcPath, out resolvedMode);

            string targetName = (target as StyledElement)?.Name;
            string targetType = target.GetType().Name;
            string dpName = dp.Name;
            string contextType = ResolveTypeName(context);

            // In design/declaration mode the binding resolution failed and there is no
            // meaningful source to bind to.  Skip applying the binding so that Avalonia's
            // stricter engine does not publish type-conversion BindingErrors (e.g. converting
            // the string "Root" to bool) that can abort XAML rendering downstream.
            if (_isDesignFallback)
            {
                // FOR TRACING:
                // Database.Root.Messages.LogInfo("VBBindingExt", "ProvideValue", $"VBBinding Resolve FAIL Target={targetType}:{targetName}; Property={dpName}; VBContent={_VBContent}; Context={contextType}");

                if (styledTarget != null)
                {
                    EnsureDataContextRebind(styledTarget, dp);
                    TryDeferredRebind(styledTarget, dp);
                }

                // Prevent stale tooltip values from previous template/data context phases.
                if (dp == ToolTip.TipProperty)
                    return null;

                return target.GetValue(dp);
            }

            // Create a concrete binding per target instance so shared template bindings
            // do not leak Source/Path state between controls.
            var resolvedBinding = CreateResolvedBinding(dcSource, dcPath, resolvedMode);

            if (styledTarget != null)
                EnsureDataContextRebind(styledTarget, dp);

            // FOR TRACING:
            // Database.Root.Messages.LogInfo("VBBindingExt", "ProvideValue", $"VBBinding Resolve OK Target={targetType}:{targetName}; Property={dpName}; VBContent={_VBContent}; Context={contextType}; Source={ResolveTypeName(dcSource)}; Path={dcPath}; Mode={resolvedMode}");

            return resolvedBinding;
        }
    }

    //[MarkupExtensionReturnType(typeof(object))]
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

        //[DefaultValue("")]
        //public string BindingGroupName
        //{
        //    get { return binding.BindingGroupName; }
        //    set { binding.BindingGroupName = value; }
        //}

        /// <summary>Gets or sets the value to use when the binding is unable to return a value.</summary>
        /// <returns>The default value is <see cref="F:System.Windows.AvaloniaProperty.UnsetValue"></see>.</returns>
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

        [DefaultValue(0)]
        public int Delay
        {
            get { return binding.Delay; }
            set { binding.Delay = value; }
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

        /// <summary>Gets or sets a value that indicates the direction of the data flow in the binding.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Data.BindingMode"></see> values. The default value is <see cref="F:System.Windows.Data.BindingMode.Default"></see>, which returns the default binding mode value of the target dependency property. However, the default value varies for each dependency property. In general, user-editable control properties, such as those of text boxes and check boxes, default to two-way bindings, whereas most other properties default to one-way bindings.A programmatic way to determine whether a dependency property binds one-way or two-way by default is to get the property metadata of the property using <see cref="M:System.Windows.AvaloniaProperty.GetMetadata(System.Type)"></see> and then check the Boolean value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.BindsTwoWayByDefault"></see> property.</returns>
        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get { return binding.Mode; }
            set { binding.Mode = value; }
        }

        /// <summary>
        /// Gets or sets the binding priority.
        /// </summary>
        [DefaultValue(BindingPriority.LocalValue)]
        public BindingPriority Priority
        {
            get { return binding.Priority; }
            set { binding.Priority = value; }
        }


        /// <summary>Gets or sets the path to the binding source property.</summary>
        /// <returns>The path to the binding source. The default value is null.</returns>
        public string Path
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

        // public WeakReference DefaultAnchor
        // {
        //     get { return binding.DefaultAnchor; }
        //     set { binding.DefaultAnchor = value; }
        // }

        // public WeakReference<INameScope> NameScope
        // {
        //     get { return binding.NameScope; }
        //     set { binding.NameScope = value; }
        // }


        public Func<string, string, Type> TypeResolver
        {
            get { return binding.TypeResolver; }
            set { binding.TypeResolver = value; }
        }

        /// <summary>Gets or sets a value that determines the timing of binding source updates.</summary>
        /// <returns>One of the <see cref="T:System.Windows.Data.UpdateSourceTrigger"></see> values. The default value is <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default"></see>, which returns the default <see cref="T:System.Windows.Data.UpdateSourceTrigger"></see> value of the target dependency property. However, the default value for most dependency properties is <see cref="F:System.Windows.Data.UpdateSourceTrigger.PropertyChanged"></see>, while the <see cref="P:System.Windows.Controls.TextBox.Text"></see> property has a default value of <see cref="F:System.Windows.Data.UpdateSourceTrigger.LostFocus"></see>.A programmatic way to determine the default <see cref="P:System.Windows.Data.Binding.UpdateSourceTrigger"></see> value of a dependency property is to get the property metadata of the property using <see cref="M:System.Windows.AvaloniaProperty.GetMetadata(System.Type)"></see> and then check the value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.DefaultUpdateSourceTrigger"></see> property.</returns>
        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return binding.UpdateSourceTrigger; }
            set { binding.UpdateSourceTrigger = value; }
        }


        /// <summary>Gets a collection of rules that check the validity of the user input.</summary>
        /// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationRule"></see> objects.</returns>
        /// https://github.com/reactiveui/ReactiveUI.Validation/tree/aa09a2fd606acbf4bf6ec2163ad3db54d5644027
        //public Collection<ValidationRule> ValidationRules
        //{
        //    get { return binding.ValidationRules; }
        //}
        #endregion

        public override object ProvideValue(IServiceProvider provider)
        {
            AvaloniaObject target;
            AvaloniaProperty dp;
            TryGetTargetItems(provider, out target, out dp);

            if (!_VBInitialized && !String.IsNullOrEmpty(_VBContent) && (Layoutgenerator.CurrentDataContext != null))
            {
                IACObject dataContext = Layoutgenerator.CurrentDataContext;
                if (target != null)
                {
                    try
                    {
                        dataContext = target.GetValue(StyledElement.DataContextProperty) as IACObject;
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
                    this.Path = dcPath;
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
                    // Design/declaration mode: ACUrlBinding could not resolve the path.
                    // Skip applying the binding entirely — Avalonia's stricter engine would
                    // publish a BindingError when it tries to convert the string "Root"
                    // (Root.ACIdentifier) to non-string target property types (e.g. bool),
                    // which can abort XAML rendering.  Leave the property at its default value.
                    _VBInitialized = true;
                    return target.GetValue(dp);
                }
            }

            _VBInitialized = true;
            target.Bind(dp, binding);
            return target.GetValue(dp);
            //return binding.ProvideValue(provider);
        }

        protected virtual bool TryGetTargetItems(IServiceProvider provider, out AvaloniaObject target, out AvaloniaProperty dp)
        {
            target = null;
            dp = null;
            if (provider == null)
                return false;

            //create a binding and assign it to the target
            IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null) return false;

            //we need dependency objects / properties
            target = service.TargetObject as AvaloniaObject;
            dp = service.TargetProperty as AvaloniaProperty;
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
