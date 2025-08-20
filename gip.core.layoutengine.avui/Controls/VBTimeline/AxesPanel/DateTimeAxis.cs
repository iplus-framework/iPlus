using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace gip.core.layoutengine.avui.timeline
{
    public class DateTimeAxis : StackPanel
    {
        #region c'tors

        public DateTimeAxis(DateTimeAxesPanel parent)
        {
            _ParentPanel = parent ?? throw new NullReferenceException();

            SolidColorBrush axisColor = Brushes.Beige;

            if(ControlManager.WpfTheme == eWpfTheme.Aero)
            {
                axisColor = Brushes.Gray;
            }

            DateTimeTextBlock = new TextBlock() { Margin = new Thickness(0,5,0,2), TextAlignment = TextAlignment.Center};
            this.Width = DateTimeAxesPanel.TextBlockWidth;

            Axis = new Line() { Stroke = axisColor, Fill = axisColor, Stretch = Stretch.Fill, HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
            Binding binding = new Binding("ActualHeight");
            binding.Source = parent;
            Axis.SetBinding(Line.Y2Property, binding);
            SetBinding(HeightProperty, binding);

            Children.Add(DateTimeTextBlock);
            Children.Add(Axis);
        }

        #endregion

        #region Properties

        public TextBlock DateTimeTextBlock
        {
            get;
            set;
        }

        private Line Axis
        {
            get;
            set;
        }

        public double AxisHeight
        {
            get;
            set;
        }

        private DateTime _CurrentDateTime;
        public DateTime CurrentDateTime
        {
            get => _CurrentDateTime;
            set
            {
                _CurrentDateTime = value;
                TimeSpan currentTS = _ParentPanel.TickTimeSpan;

                if (currentTS < TimeSpan.FromMinutes(30))
                    DateTimeTextBlock.Text = _CurrentDateTime.ToString("HH:mm:ss");
                else if (currentTS < TimeSpan.FromDays(1))
                    DateTimeTextBlock.Text = _CurrentDateTime.ToString("ddd HH:mm");
                else if (currentTS < TimeSpan.FromDays(30))
                    DateTimeTextBlock.Text = _CurrentDateTime.ToString("dd/MM/yy");
                else if (currentTS < TimeSpan.FromDays(360))
                    DateTimeTextBlock.Text = _CurrentDateTime.ToString("MM/yy");
                else
                    DateTimeTextBlock.Text = _CurrentDateTime.ToString("yyyy");

                ToolTip = _CurrentDateTime.ToString();
            }
        }

        public double DefaultPosition
        {
            get;
            set;
        }

        private DateTimeAxesPanel _ParentPanel;

        #endregion

        #region Methods

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
            BindingOperations.ClearBinding(Axis, Line.Y2Property);
            BindingOperations.ClearBinding(this, HeightProperty);
            Children.Clear();
            Axis = null;
            DateTimeTextBlock = null;
            _ParentPanel = null;
        }

        #endregion
    }
}
