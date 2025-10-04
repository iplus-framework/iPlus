using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a input base control.
    /// </summary>
    public abstract class InputBase : TemplatedControl
    {
        #region Members

        /// <summary>
        /// Flags if the Text and Value properties are in the process of being sync'd
        /// </summary>
        private bool _isSyncingTextAndValueProperties;
        private bool _isInitialized;

        #endregion //Members

        #region Properties

        public virtual object PreviousValue { get; internal set; }

        #region IsEditable

        public static readonly StyledProperty<bool> IsEditableProperty = 
            AvaloniaProperty.Register<InputBase, bool>(nameof(IsEditable), true);
        
        public bool IsEditable
        {
            get { return GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        #endregion //IsEditable

        #region Text

        public static readonly StyledProperty<string> TextProperty = 
            AvaloniaProperty.Register<InputBase, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        
        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == TextProperty)
            {
                OnTextChanged(change.GetOldValue<string>(), change.GetNewValue<string>());
                if (_isInitialized)
                    SyncTextAndValueProperties(change.Property, change.NewValue);
            }
            else if (change.Property == ValueProperty)
            {
                var oldValue = change.OldValue;
                var newValue = change.NewValue;
                
                if (!Equals(oldValue, newValue))
                {
                    PreviousValue = oldValue;
                    OnValueChanged(oldValue, newValue);

                    if (_isInitialized)
                        SyncTextAndValueProperties(change.Property, newValue);
                }
            }
            else if (change.Property == ValueTypeProperty)
            {
                OnValueTypeChanged(change.GetOldValue<Type>(), change.GetNewValue<Type>());
            }
        }

        protected virtual void OnTextChanged(string previousValue, string currentValue)
        {

        }

        #endregion //Text

        #region Value

        public static readonly StyledProperty<object> ValueProperty = 
            AvaloniaProperty.Register<InputBase, object>(Const.Value, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, 
                coerce: OnCoerceValuePropertyCallback);
        
        public virtual object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            var args = new RoutedEventArgs<object>(ValueChangedEvent, this, oldValue, newValue);
            RaiseEvent(args);
        }

        private static object OnCoerceValuePropertyCallback(AvaloniaObject d, object baseValue)
        {
            if (d is InputBase inputBase)
                return inputBase.OnCoerceValue(baseValue);
            else
                return baseValue;
        }

        protected virtual object OnCoerceValue(object value)
        {
            return value;
        }

        #endregion //Value

        #region ValueType

        public static readonly StyledProperty<Type> ValueTypeProperty = 
            AvaloniaProperty.Register<InputBase, Type>(nameof(ValueType), typeof(String));
        
        public Type ValueType
        {
            get { return GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        protected virtual void OnValueTypeChanged(Type oldValue, Type newType)
        {
            if (_isInitialized)
                SyncTextAndValueProperties(TextProperty, Text);
        }

        #endregion //ValueType

        #endregion //Properties

        #region Base Class Overrides

        /// <summary>
        /// The event handler for Initialized event.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (!_isInitialized)
            {
                _isInitialized = true;
                SyncTextAndValueProperties(ValueProperty, Value);
            }
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent<RoutedEventArgs<object>> ValueChangedEvent = 
            RoutedEvent.Register<InputBase, RoutedEventArgs<object>>(nameof(ValueChanged), RoutingStrategies.Bubble);
        
        public event EventHandler<RoutedEventArgs<object>> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion //Events

        #region Methods

        protected void SyncTextAndValueProperties(AvaloniaProperty p, object newValue)
        {
            //prevents recursive syncing properties
            if (_isSyncingTextAndValueProperties)
                return;

            _isSyncingTextAndValueProperties = true;

            //this only occurs when the user typed in the value
            if (InputBase.TextProperty == p)
            {
                SetValue(InputBase.ValueProperty, ConvertTextToValue(newValue?.ToString()));
            }

            SetValue(InputBase.TextProperty, ConvertValueToText(newValue));

            _isSyncingTextAndValueProperties = false;
        }

        #endregion //Methods

        #region Abstract

        protected abstract object ConvertTextToValue(string text);

        protected abstract string ConvertValueToText(object value);

        #endregion //Abstract
    }

    /// <summary>
    /// Custom RoutedEventArgs for value changed events that includes old and new values
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    public class RoutedEventArgs<T> : RoutedEventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public RoutedEventArgs(RoutedEvent routedEvent, Interactive source, T oldValue, T newValue) 
            : base(routedEvent, source)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
