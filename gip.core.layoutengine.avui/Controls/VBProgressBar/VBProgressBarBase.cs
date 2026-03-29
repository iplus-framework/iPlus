using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using System;
using Avalonia.VisualTree;
using Avalonia.Data.Converters;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// A control used to indicate the progress of an operation.
    /// </summary>
    [TemplatePart("PART_Indicator", typeof(Border), IsRequired = true)]
    [PseudoClasses(":vertical", ":horizontal", ":indeterminate")]
    public class VBProgressBarBase : RangeBase
    {
        /// <summary>
        /// Provides calculated values for use with the <see cref="VBProgressBarBase"/>'s control theme or template.
        /// </summary>
        /// <remarks>
        /// This class is NOT intended for general use outside of control templates.
        /// </remarks>
        public class VBProgressBarBaseTemplateSettings : AvaloniaObject
        {
            private double _container2Width;
            private double _containerWidth;
            private double _containerAnimationStartPosition;
            private double _containerAnimationEndPosition;
            private double _container2AnimationStartPosition;
            private double _container2AnimationEndPosition;
            private double _indeterminateStartingOffset;
            private double _indeterminateEndingOffset;

            /// <summary>
            /// Defines the <see cref="ContainerAnimationStartPosition"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double>
                ContainerAnimationStartPositionProperty =
                    AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                        nameof(ContainerAnimationStartPosition),
                        p => p.ContainerAnimationStartPosition,
                        (p, o) => p.ContainerAnimationStartPosition = o);

            /// <summary>
            /// Defines the <see cref="ContainerAnimationEndPosition"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double>
                ContainerAnimationEndPositionProperty =
                    AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                        nameof(ContainerAnimationEndPosition),
                        p => p.ContainerAnimationEndPosition,
                        (p, o) => p.ContainerAnimationEndPosition = o);

            /// <summary>
            /// Defines the <see cref="Container2AnimationStartPosition"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double>
                Container2AnimationStartPositionProperty =
                    AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                        nameof(Container2AnimationStartPosition),
                        p => p.Container2AnimationStartPosition,
                        (p, o) => p.Container2AnimationStartPosition = o);

            /// <summary>
            /// Defines the <see cref="Container2AnimationEndPosition"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double>
                Container2AnimationEndPositionProperty =
                    AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                        nameof(Container2AnimationEndPosition),
                        p => p.Container2AnimationEndPosition,
                        (p, o) => p.Container2AnimationEndPosition = o);

            /// <summary>
            /// Defines the <see cref="Container2Width"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double> Container2WidthProperty =
                AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                    nameof(Container2Width),
                    p => p.Container2Width,
                    (p, o) => p.Container2Width = o);

            /// <summary>
            /// Defines the <see cref="ContainerWidth"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double> ContainerWidthProperty =
                AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                    nameof(ContainerWidth),
                    p => p.ContainerWidth,
                    (p, o) => p.ContainerWidth = o);

            /// <summary>
            /// Defines the <see cref="IndeterminateStartingOffset"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double> IndeterminateStartingOffsetProperty =
                AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                    nameof(IndeterminateStartingOffset),
                    p => p.IndeterminateStartingOffset,
                    (p, o) => p.IndeterminateStartingOffset = o);
            
            /// <summary>
            /// Defines the <see cref="IndeterminateEndingOffset"/> property.
            /// </summary>
            public static readonly DirectProperty<VBProgressBarBaseTemplateSettings, double> IndeterminateEndingOffsetProperty =
                AvaloniaProperty.RegisterDirect<VBProgressBarBaseTemplateSettings, double>(
                    nameof(IndeterminateEndingOffset),
                    p => p.IndeterminateEndingOffset,
                    (p, o) => p.IndeterminateEndingOffset = o);

            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the first indeterminate indicator's width.
            /// </summary>
            public double ContainerWidth
            {
                get => _containerWidth;
                set => SetAndRaise(ContainerWidthProperty, ref _containerWidth, value);
            }
            
            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the second indeterminate indicator's width.
            /// </summary>
            public double Container2Width
            {
                get => _container2Width;
                set => SetAndRaise(Container2WidthProperty, ref _container2Width, value);
            }

            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the first indeterminate indicator's start position when animated.
            /// </summary>
            public double ContainerAnimationStartPosition
            {
                get => _containerAnimationStartPosition;
                set => SetAndRaise(ContainerAnimationStartPositionProperty, ref _containerAnimationStartPosition,
                    value);
            }

            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the first indeterminate indicator's end position when animated.
            /// </summary>
            public double ContainerAnimationEndPosition
            {
                get => _containerAnimationEndPosition;
                set => SetAndRaise(ContainerAnimationEndPositionProperty, ref _containerAnimationEndPosition, value);
            }

            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the second indeterminate indicator's start position when animated.
            /// </summary>
            public double Container2AnimationStartPosition
            {
                get => _container2AnimationStartPosition;
                set => SetAndRaise(Container2AnimationStartPositionProperty, ref _container2AnimationStartPosition,
                    value);
            }
            
            /// <summary>
            /// Used by Avalonia.Themes.Fluent to define the second indeterminate indicator's end position when animated.
            /// </summary>
            public double Container2AnimationEndPosition
            {
                get => _container2AnimationEndPosition;
                set => SetAndRaise(Container2AnimationEndPositionProperty, ref _container2AnimationEndPosition, value);
            }

            /// <summary>
            /// Used by Avalonia.Themes.Simple  to define the starting point of its indeterminate animation.
            /// </summary>
            public double IndeterminateStartingOffset
            {
                get => _indeterminateStartingOffset;
                set => SetAndRaise(IndeterminateStartingOffsetProperty, ref _indeterminateStartingOffset, value);
            }
            
            /// <summary>
            /// Used by Avalonia.Themes.Simple to define the ending point of its indeterminate animation.
            /// </summary>
            public double IndeterminateEndingOffset
            {
                get => _indeterminateEndingOffset;
                set => SetAndRaise(IndeterminateEndingOffsetProperty, ref _indeterminateEndingOffset, value);
            }
        }

        private double _percentage;
        private Border _indicator;
        private IDisposable _trackSizeChangedListener;

        /// <summary>
        /// Defines the <see cref="IsIndeterminate"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<VBProgressBarBase, bool>(nameof(IsIndeterminate));

        /// <summary>
        /// Defines the <see cref="ShowProgressText"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowProgressTextProperty =
            AvaloniaProperty.Register<VBProgressBarBase, bool>(nameof(ShowProgressText));

        /// <summary>
        /// Defines the <see cref="ProgressTextFormat"/> property.
        /// </summary>
        public static readonly StyledProperty<string> ProgressTextFormatProperty =
            AvaloniaProperty.Register<VBProgressBarBase, string>(nameof(ProgressTextFormat), "{1:0}%");

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<VBProgressBarBase, Orientation>(nameof(Orientation));

        /// <summary>
        /// Defines the <see cref="Percentage"/> property.
        /// </summary>
        public static readonly DirectProperty<VBProgressBarBase, double> PercentageProperty =
            AvaloniaProperty.RegisterDirect<VBProgressBarBase, double>(
                nameof(Percentage),
                o => o.Percentage);

        /// <summary>
        /// Gets the overall percentage complete of the progress 
        /// </summary>
        /// <remarks>
        /// This read-only property is automatically calculated using the current <see cref="RangeBase.Value"/> and
        /// the effective range (<see cref="RangeBase.Maximum"/> - <see cref="RangeBase.Minimum"/>).
        /// </remarks>
        public double Percentage
        {
            get => _percentage;
            private set => SetAndRaise(PercentageProperty, ref _percentage, value);
        }

        static VBProgressBarBase()
        {
            ValueProperty.OverrideMetadata<VBProgressBarBase>(new(defaultBindingMode: BindingMode.OneWay));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VBProgressBarBase"/> class.
        /// </summary>
        public VBProgressBarBase()
        {
            UpdatePseudoClasses(IsIndeterminate, Orientation);
        }

        /// <summary>
        /// Gets or sets the TemplateSettings for the <see cref="VBProgressBarBase"/>.
        /// </summary>
        public VBProgressBarBaseTemplateSettings TemplateSettings { get; } = new VBProgressBarBaseTemplateSettings();

        /// <summary>
        /// Gets or sets a value indicating whether the progress bar shows the actual value or a generic,
        /// continues progress indicator (indeterminate state).
        /// </summary>
        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether progress text will be shown.
        /// </summary>
        public bool ShowProgressText
        {
            get => GetValue(ShowProgressTextProperty);
            set => SetValue(ShowProgressTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the format string applied to the internally calculated progress text before it is shown.
        /// </summary>
        public string ProgressTextFormat
        {
            get => GetValue(ProgressTextFormatProperty);
            set => SetValue(ProgressTextFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation of the <see cref="VBProgressBarBase"/>.
        /// </summary>
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateIndicator();
            return result;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty ||
                change.Property == MinimumProperty ||
                change.Property == MaximumProperty ||
                change.Property == IsIndeterminateProperty ||
                change.Property == OrientationProperty)
            {
                UpdateIndicator();
            }

            if (change.Property == IsIndeterminateProperty)
            {
                UpdatePseudoClasses(change.GetNewValue<bool>(), null);
            }
            else if (change.Property == OrientationProperty)
            {
                UpdatePseudoClasses(null, change.GetNewValue<Orientation>());
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            // dispose any previous track size listener
            _trackSizeChangedListener?.Dispose();

            _indicator = e.NameScope.Get<Border>("PART_Indicator");

            // listen to size changes of the indicators track (parent) and update the indicator there. 
            _trackSizeChangedListener = _indicator.Parent?.GetPropertyChangedObservable(BoundsProperty)
                .Subscribe(_ => UpdateIndicator());

            UpdateIndicator();
        }


        protected virtual void UpdateIndicator()
        {
            if (_indicator == null) 
                return;
            // Gets the size of the parent indicator container
            //var barSize = Bounds.Size;
            var barSize = _indicator.GetVisualParent()?.Bounds.Size ?? Bounds.Size;

            var percent = IsIndeterminate || Math.Abs(Maximum - Minimum) < double.Epsilon ?
                1.0 :
                (Value - Minimum) / (Maximum - Minimum);


            var width = (barSize.Width  - _indicator.Margin.Left -
                        _indicator.Margin.Right) * percent;
            _indicator.Width  = width > 0 ? width : 0;      // <‑‑ grows horizontally
            _indicator.Height = double.NaN;                 // no height growth

            Percentage = percent * 100.0;
        }

        private void UpdatePseudoClasses(
            bool? isIndeterminate,
            Orientation? o)
        {
            if (isIndeterminate.HasValue)
            {
                PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
            }

            if (!o.HasValue) return;
            PseudoClasses.Set(":vertical", o == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
        }
    }

    public class OrientationToHorizontal : IValueConverter
    {
        private static OrientationToHorizontal _Current;

        public static OrientationToHorizontal Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new OrientationToHorizontal();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((Orientation)value) == Orientation.Horizontal ? "Stretch" : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class OrientationToVertical : IValueConverter
    {
        private static OrientationToVertical _Current;

        public static OrientationToVertical Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new OrientationToVertical();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((Orientation)value) == Orientation.Vertical ? "Stretch" : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
