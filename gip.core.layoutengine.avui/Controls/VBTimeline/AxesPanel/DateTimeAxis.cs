using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.Converters;
using System;

namespace gip.core.layoutengine.avui.timeline
{
    public class DateTimeAxis : StackPanel
    {
        #region c'tors

        static DateTimeAxis()
        {
            // Register styled properties
        }

        public DateTimeAxis(DateTimeAxesPanel parent)
        {
            _ParentPanel = parent ?? throw new NullReferenceException();

            IBrush axisColor = Brushes.Beige;

            if (ControlManager.WpfTheme == eWpfTheme.Light)
            {
                axisColor = Brushes.Gray;
            }

            DateTimeTextBlock = new TextBlock() { Margin = new Thickness(0, 5, 0, 2), TextAlignment = TextAlignment.Center, FontSize = 10 };
            this.Width = DateTimeAxesPanel.TextBlockWidth;

            Axis = new Line() { Stroke = axisColor, Fill = axisColor, Stretch = Stretch.Fill, HorizontalAlignment = HorizontalAlignment.Center };

            // Avalonia has no ActualHeight CLR property (WPF) - use Bounds.Height.
            Binding binding = new Binding("Bounds.Height");
            binding.Source = parent;
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Bindings.Add(binding);
            // Add a static binding with value 0
            Binding staticBinding = new Binding();
            staticBinding.Source = 0;
            multiBinding.Bindings.Add(staticBinding); // X - Value
            multiBinding.Bindings.Add(binding); // Y - Value
            PointConverter pointConverter = new PointConverter();
            multiBinding.Converter = pointConverter;
            Axis.Bind(Line.EndPointProperty, multiBinding);
            Bind(HeightProperty, binding);

            Children.Add(DateTimeTextBlock);
            Children.Add(Axis);
        }

        #endregion

        #region Properties

        public static readonly StyledProperty<TextBlock> DateTimeTextBlockProperty =
            AvaloniaProperty.Register<DateTimeAxis, TextBlock>(nameof(DateTimeTextBlock));

        public TextBlock DateTimeTextBlock
        {
            get { return GetValue(DateTimeTextBlockProperty); }
            set { SetValue(DateTimeTextBlockProperty, value); }
        }

        public static readonly StyledProperty<Line> AxisProperty =
            AvaloniaProperty.Register<DateTimeAxis, Line>(nameof(Axis));

        private Line Axis
        {
            get { return GetValue(AxisProperty); }
            set { SetValue(AxisProperty, value); }
        }

        public static readonly StyledProperty<double> AxisHeightProperty =
            AvaloniaProperty.Register<DateTimeAxis, double>(nameof(AxisHeight), 0.0);

        public double AxisHeight
        {
            get { return GetValue(AxisHeightProperty); }
            set { SetValue(AxisHeightProperty, value); }
        }

        public static readonly StyledProperty<DateTime> CurrentDateTimeProperty =
            AvaloniaProperty.Register<DateTimeAxis, DateTime>(nameof(CurrentDateTime), DateTime.Now);

        public DateTime CurrentDateTime
        {
            get { return GetValue(CurrentDateTimeProperty); }
            set 
            { 
                SetValue(CurrentDateTimeProperty, value);
                UpdateDateTimeDisplay(value);
            }
        }

        public static readonly StyledProperty<double> DefaultPositionProperty =
            AvaloniaProperty.Register<DateTimeAxis, double>(nameof(DefaultPosition), 0.0);

        public double DefaultPosition
        {
            get { return GetValue(DefaultPositionProperty); }
            set { SetValue(DefaultPositionProperty, value); }
        }

        private DateTimeAxesPanel _ParentPanel;

        #endregion

        #region Methods

        private void UpdateDateTimeDisplay(DateTime dateTime)
        {
            if (_ParentPanel == null || DateTimeTextBlock == null)
                return;

            // Respect the user's regional settings: use the current culture for
            // all formats. Day/month names ("ddd") are culture-aware anyway; for
            // pure numeric dates use the culture's short-date pattern instead of
            // the hard-coded English "dd/MM/yy" / "MM/yy".
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            TimeSpan currentTS = _ParentPanel.TickTimeSpan;

            if (currentTS < TimeSpan.FromMinutes(30))
                DateTimeTextBlock.Text = dateTime.ToString("HH:mm:ss", culture);
            else if (currentTS < TimeSpan.FromDays(1))
                DateTimeTextBlock.Text = dateTime.ToString("ddd HH:mm", culture);
            else if (currentTS < TimeSpan.FromDays(30))
                DateTimeTextBlock.Text = dateTime.ToString(culture.DateTimeFormat.ShortDatePattern);
            else if (currentTS < TimeSpan.FromDays(360))
                DateTimeTextBlock.Text = dateTime.ToString(culture.DateTimeFormat.YearMonthPattern);
            else
                DateTimeTextBlock.Text = dateTime.ToString("yyyy", culture);

            ToolTip.SetTip(this, dateTime.ToString(culture));
        }

        public void SetPosition(double offset, bool isDefault = false)
        {
            if (isDefault)
                DefaultPosition = offset;
            Canvas.SetLeft(this, offset);
        }

        public override string ToString()
        {
            return DateTimeTextBlock != null ? DateTimeTextBlock.Text : base.ToString();
        }

        public void DeInitControl()
        {
            this.ClearAllBindings();
            Children.Clear();
            Axis = null;
            DateTimeTextBlock = null;
            _ParentPanel = null;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == CurrentDateTimeProperty)
            {
                UpdateDateTimeDisplay((DateTime)change.NewValue);
            }
        }

        #endregion
    }
}
