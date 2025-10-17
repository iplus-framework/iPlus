using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static gip.core.layoutengine.avui.VBProgressBar;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Interaction logic for VBOEEControl.xaml
    /// </summary>
    public partial class VBOEEControl : UserControl
    {
        public VBOEEControl()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<double> AvailabilityOEEProperty = 
            AvaloniaProperty.Register<VBOEEControl, double>(nameof(AvailabilityOEE));
        [Category("VBControl")]
        [Bindable(true)]
        public double AvailabilityOEE
        {
            get { return GetValue(AvailabilityOEEProperty); }
            set { SetValue(AvailabilityOEEProperty, value); }
        }

        public static readonly StyledProperty<double> PerformanceOEEProperty = 
            AvaloniaProperty.Register<VBOEEControl, double>(nameof(PerformanceOEE));
        [Category("VBControl")]
        [Bindable(true)]
        public double PerformanceOEE
        {
            get { return GetValue(PerformanceOEEProperty); }
            set { SetValue(PerformanceOEEProperty, value); }
        }

        public static readonly StyledProperty<double> QualityOEEProperty = 
            AvaloniaProperty.Register<VBOEEControl, double>(nameof(QualityOEE));
        [Category("VBControl")]
        [Bindable(true)]
        public double QualityOEE
        {
            get { return GetValue(QualityOEEProperty); }
            set { SetValue(QualityOEEProperty, value); }
        }

        public static readonly StyledProperty<double> TotalOEEProperty = 
            AvaloniaProperty.Register<VBOEEControl, double>(nameof(TotalOEE));
        [Category("VBControl")]
        [Bindable(true)]
        public double TotalOEE
        {
            get { return GetValue(TotalOEEProperty); }
            set { SetValue(TotalOEEProperty, value); }
        }

        public static readonly StyledProperty<SolidColorBrush> OEETextColorProperty =
            AvaloniaProperty.Register<VBOEEControl, SolidColorBrush>(nameof(OEETextColor), new SolidColorBrush(Colors.White));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush OEETextColor
        {
            get { return GetValue(OEETextColorProperty); }
            set { SetValue(OEETextColorProperty, value); }
        }


        public static readonly StyledProperty<SolidColorBrush> AvailabilityColorProperty =
            AvaloniaProperty.Register<VBOEEControl, SolidColorBrush>(nameof(AvailabilityColor), new SolidColorBrush(Colors.Lime));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush AvailabilityColor
        {
            get { return GetValue(AvailabilityColorProperty); }
            set { SetValue(AvailabilityColorProperty, value); }
        }


        public static readonly StyledProperty<SolidColorBrush> PerformanceColorProperty =
            AvaloniaProperty.Register<VBOEEControl, SolidColorBrush>(nameof(PerformanceColor), new SolidColorBrush(Colors.Red));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush PerformanceColor
        {
            get { return GetValue(PerformanceColorProperty); }
            set { SetValue(PerformanceColorProperty, value); }
        }


        public static readonly StyledProperty<SolidColorBrush> QualityColorProperty =
            AvaloniaProperty.Register<VBOEEControl, SolidColorBrush>(nameof(QualityColor), new SolidColorBrush(Colors.Yellow));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush QualityColor
        {
            get { return GetValue(QualityColorProperty); }
            set { SetValue(QualityColorProperty, value); }
        }


        public static readonly StyledProperty<SolidColorBrush> TotalOEEColorProperty =
            AvaloniaProperty.Register<VBOEEControl, SolidColorBrush>(nameof(TotalOEEColor), new SolidColorBrush(Colors.Gray));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush TotalOEEColor
        {
            get { return GetValue(TotalOEEColorProperty); }
            set { SetValue(TotalOEEColorProperty, value); }
        }
    }
}
