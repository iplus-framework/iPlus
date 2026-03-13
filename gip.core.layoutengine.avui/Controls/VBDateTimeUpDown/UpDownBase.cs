using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a Up or Down base control.
    /// </summary>
    [TemplatePart("PART_Spinner", typeof(Spinner))]
    [TemplatePart("PART_TextBox", typeof(TextBox), IsRequired = true)]
    public abstract class UpDownBase : InputBase
    {
        #region Members

        /// <summary>
        /// Name constant for Text template part.
        /// </summary>
        internal const string ElementTextName = "PART_TextBox";

        /// <summary>
        /// Name constant for Spinner template part.
        /// </summary>
        internal const string ElementSpinnerName = "PART_Spinner";

        #endregion //Members

        #region Properties

        public static readonly StyledProperty<object> MaximumProperty =
            AvaloniaProperty.Register<UpDownBase, object>(nameof(Maximum), coerce: (s, v) => ((UpDownBase)s).OnCoerceMaximum(v));

        public object Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly StyledProperty<object> MinimumProperty =
            AvaloniaProperty.Register<UpDownBase, object>(nameof(Minimum), coerce: (s, v) => ((UpDownBase)s).OnCoerceMinimum(v));

        public object Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        protected TextBox TextBox { get; private set; }

        private Spinner _spinner;
        internal Spinner Spinner
        {
            get { return _spinner; }
            private set
            {
                if (_spinner != null)
                    _spinner.Spin -= OnSpinnerSpin;
                    
                _spinner = value;
                
                if (_spinner != null)
                    _spinner.Spin += OnSpinnerSpin;
            }
        }

        #endregion //Properties

        #region Constructors

        public UpDownBase() : base()
        {
        }

        #endregion //Constructors

        #region Base Class Overrides

        /// <summary>
        /// Overrides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            TextBox = e.NameScope.Find<TextBox>(ElementTextName);
            Spinner = e.NameScope.Find<Spinner>(ElementSpinnerName);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        DoIncrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Down:
                    {
                        DoDecrement();
                        e.Handled = true;
                        break;
                    }
                case Key.Enter:
                    {
                        SyncTextAndValueProperties(UpDownBase.TextProperty, TextBox?.Text);
                        break;
                    }
            }
            
            if (!e.Handled)
                base.OnKeyDown(e);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            if (!e.Handled)
            {
                if (e.Delta.Y < 0)
                {
                    DoDecrement();
                }
                else if (0 < e.Delta.Y)
                {
                    DoIncrement();
                }

                e.Handled = true;
            }
        }

        protected override object OnCoerceValue(object value)
        {
            if (value is IComparable val)
            {
                var min = Minimum;
                if (min != null)
                {
                    var coercedMin = Convert.ChangeType(min, val.GetType());
                    if (val.CompareTo(coercedMin) < 0)
                        return coercedMin;
                }

                var max = Maximum;
                if (max != null)
                {
                    var coercedMax = Convert.ChangeType(max, val.GetType());
                    if (val.CompareTo(coercedMax) > 0)
                        return coercedMax;
                }
            }
            return base.OnCoerceValue(value);
        }

        protected virtual object OnCoerceMaximum(object value)
        {
            return value;
        }

        protected virtual object OnCoerceMinimum(object value)
        {
            return value;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        private void OnSpinnerSpin(object sender, SpinEventArgs e)
        {
            OnSpin(e);
        }

        #endregion //Event Handlers

        #region Methods

        protected virtual void OnSpin(SpinEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Direction == SpinDirection.Increase)
                DoIncrement();
            else
                DoDecrement();
        }

        /// <summary>
        /// Performs a decrement if conditions allow it.
        /// </summary>
        private void DoDecrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease)
            {
                if (Value is IComparable val && Minimum != null)
                {
                    var min = Convert.ChangeType(Minimum, val.GetType()) as IComparable;
                    if (val.CompareTo(min) <= 0)
                        return;
                }

                OnDecrement();
            }
        }

        /// <summary>
        /// Performs an increment if conditions allow it.
        /// </summary>
        private void DoIncrement()
        {
            if (Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase)
            {
                if (Value is IComparable val && Maximum != null)
                {
                    var max = Convert.ChangeType(Maximum, val.GetType()) as IComparable;
                    if (val.CompareTo(max) >= 0)
                        return;
                }

                OnIncrement();
            }
        }

        #region Abstract

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Increase.
        /// </summary>
        protected abstract void OnIncrement();

        /// <summary>
        /// Called by OnSpin when the spin direction is SpinDirection.Decrease.
        /// </summary>
        protected abstract void OnDecrement();

        #endregion //Abstract

        #endregion //Methods
    }
}
