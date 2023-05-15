using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static gip.core.layoutengine.VBProgressBar;

namespace gip.core.layoutengine
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public static readonly DependencyProperty AvailabilityOEEProperty = DependencyProperty.Register("AvailabilityOEE", typeof(double), typeof(VBOEEControl));
        [Category("VBControl")]
        [Bindable(true)]
        public double AvailabilityOEE
        {
            get { return (double)GetValue(AvailabilityOEEProperty); }
            set { SetValue(AvailabilityOEEProperty, value); }
        }

        public static readonly DependencyProperty PerformanceOEEProperty = DependencyProperty.Register("PerformanceOEE", typeof(double), typeof(VBOEEControl));
        [Category("VBControl")]
        [Bindable(true)]
        public double PerformanceOEE
        {
            get { return (double)GetValue(PerformanceOEEProperty); }
            set { SetValue(PerformanceOEEProperty, value); }
        }

        public static readonly DependencyProperty QualityOEEProperty = DependencyProperty.Register("QualityOEE", typeof(double), typeof(VBOEEControl));
        [Category("VBControl")]
        [Bindable(true)]
        public double QualityOEE
        {
            get { return (double)GetValue(QualityOEEProperty); }
            set { SetValue(QualityOEEProperty, value); }
        }

        public static readonly DependencyProperty TotalOEEProperty = DependencyProperty.Register("TotalOEE", typeof(double), typeof(VBOEEControl));
        [Category("VBControl")]
        [Bindable(true)]
        public double TotalOEE
        {
            get { return (double)GetValue(TotalOEEProperty); }
            set { SetValue(TotalOEEProperty, value); }
        }

        public static readonly DependencyProperty OEETextColorProperty
            = DependencyProperty.Register("OEETextColor", typeof(SolidColorBrush), typeof(VBOEEControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush OEETextColor
        {
            get { return (SolidColorBrush)GetValue(OEETextColorProperty); }
            set { SetValue(OEETextColorProperty, value); }
        }


        public static readonly DependencyProperty AvailabilityColorProperty
            = DependencyProperty.Register("AvailabilityColor", typeof(SolidColorBrush), typeof(VBOEEControl), new PropertyMetadata(new SolidColorBrush(Colors.Lime)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush AvailabilityColor
        {
            get { return (SolidColorBrush)GetValue(AvailabilityColorProperty); }
            set { SetValue(AvailabilityColorProperty, value); }
        }


        public static readonly DependencyProperty PerformanceColorProperty
            = DependencyProperty.Register("PerformanceColor", typeof(SolidColorBrush), typeof(VBOEEControl), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush PerformanceColor
        {
            get { return (SolidColorBrush)GetValue(PerformanceColorProperty); }
            set { SetValue(PerformanceColorProperty, value); }
        }


        public static readonly DependencyProperty QualityColorProperty
            = DependencyProperty.Register("QualityColor", typeof(SolidColorBrush), typeof(VBOEEControl), new PropertyMetadata(new SolidColorBrush(Colors.Yellow)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush QualityColor
        {
            get { return (SolidColorBrush)GetValue(QualityColorProperty); }
            set { SetValue(QualityColorProperty, value); }
        }


        public static readonly DependencyProperty TotalOEEColorProperty
            = DependencyProperty.Register("TotalOEEColor", typeof(SolidColorBrush), typeof(VBOEEControl), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush TotalOEEColor
        {
            get { return (SolidColorBrush)GetValue(TotalOEEColorProperty); }
            set { SetValue(TotalOEEColorProperty, value); }
        }



    }
}
