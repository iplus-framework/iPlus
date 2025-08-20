using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LinqToVisualTree;
using System.Windows.Data;
using System.ComponentModel;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// An attached view model that adapts a ProgressBar control to provide properties
    /// that assist in the creation of a circular template
    /// </summary>
    public class VBProgressBarCircularViewModel : FrameworkElement
    {
        #region Attach attached property

        /// <summary>
        /// Represents the dependency property for Attach.
        /// </summary>
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(object), typeof(VBProgressBarCircularViewModel),
                new PropertyMetadata(null, new PropertyChangedCallback(OnAttachChanged)));

        /// <summary>
        /// Gets the Attach.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>The VBProgressBarCircularViewModel object.</returns>
        public static VBProgressBarCircularViewModel GetAttach(DependencyObject d)
        {
            return (VBProgressBarCircularViewModel)d.GetValue(AttachProperty);
        }

        /// <summary>
        /// Set the Attach.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">The VBProgressBarCircularViewModel object.</param>
        public static void SetAttach(DependencyObject d, VBProgressBarCircularViewModel value)
        {
            d.SetValue(AttachProperty, value);
        }

        /// <summary>
        /// Change handler for the Attach property
        /// </summary>
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // set the view model as the DataContext for the rest of the template
            FrameworkElement targetElement = d as FrameworkElement;
            VBProgressBarCircularViewModel viewModel = e.NewValue as VBProgressBarCircularViewModel;
            targetElement.DataContext = viewModel;

            // handle the loaded event
            targetElement.Loaded += new RoutedEventHandler(Element_Loaded);
        }

        /// <summary>
        /// Handle the Loaded event of the element to which this view model is attached
        /// in order to enable the attached
        /// view model to bind to properties of the parent element
        /// </summary>
        static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement targetElement = sender as FrameworkElement;
            VBProgressBarCircularViewModel attachedModel = GetAttach(targetElement);

            // find the ProgressBar and associated it with the view model
            var progressBar = targetElement.Ancestors<ProgressBar>().Single() as ProgressBar;
            attachedModel.SetProgressBar(progressBar);
        }

        #endregion

        #region fields

        //private double _angle;

        //private double _centreX;

        //private double _centreY;

        //private double _radius;

        //private double _innerRadius;

        //private double _diameter;

        //private double _percent;

        //private double _holeSizeFactor = 0.0;

        protected ProgressBar _progressBar;

        #endregion

        #region properties

        /// <summary>
        /// Represents the dependency property for Percent.
        /// </summary>
        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register("Percent", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the Percent.
        /// </summary>
        [Category("VBControl")]
        public double Percent
        {
            get { return (double)GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Diameter.
        /// </summary>
        public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register("Diameter", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the Diameter.
        /// </summary>
        [Category("VBControl")]
        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Radius.
        /// </summary>
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the
        /// </summary>
        [Category("VBControl")]
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for InnerRadius.
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the
        /// </summary>
        [Category("VBControl")]
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CentreX.
        /// </summary>
        public static readonly DependencyProperty CentreXProperty = DependencyProperty.Register("CentreX", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the CentreX.
        /// </summary>
        [Category("VBControl")]
        public double CentreX
        {
            get { return (double)GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CentreY.
        /// </summary>
        public static readonly DependencyProperty CentreYProperty = DependencyProperty.Register("CentreY", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the CentreY.
        /// </summary>
        [Category("VBControl")]
        public double CentreY
        {
            get { return (double)GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Angle.
        /// </summary>
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the Angle.
        /// </summary>
        [Category("VBControl")]
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for HoleSizeFactor.
        /// </summary>
        public static readonly DependencyProperty HoleSizeFactorProperty = DependencyProperty.Register("HoleSizeFactor", typeof(double), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the HoleSize.
        /// </summary>
        [Category("VBControl")]
        public double HoleSizeFactor
        {
            get { return (double)GetValue(HoleSizeFactorProperty); }
            set { SetValue(HoleSizeFactorProperty, value); }
        }

        #endregion

        /// <summary>
        /// Re-computes the various properties that the elements in the template bind to.
        /// </summary>
        protected virtual void ComputeViewModelProperties()
        {
            if (_progressBar == null)
                return;

            Angle = (_progressBar.Value - _progressBar.Minimum) * 360 / (_progressBar.Maximum - _progressBar.Minimum);
            CentreX = _progressBar.ActualWidth / 2;
            CentreY = _progressBar.ActualHeight / 2;
            Radius = Math.Min(CentreX, CentreY);
            Diameter = Radius * 2;
            InnerRadius = Radius * HoleSizeFactor;
            Percent = Angle / 360;
        }

        /// <summary>
        /// Add handlers for the updates on various properties of the ProgressBar
        /// </summary>
        private void SetProgressBar(ProgressBar progressBar)
        {
            if (_progressBar != null)
                return;
            _progressBar = progressBar;
            _progressBar.SizeChanged += (s, e) => ComputeViewModelProperties();
            
            //RegisterForNotification(Const.Value, progressBar, (d, e) => ComputeViewModelProperties());
            DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(ProgressBar.ValueProperty, typeof(ProgressBar));
            desc.AddValueChanged(progressBar, new EventHandler(ProgressBarDepPropChanged));  

            //RegisterForNotification("Maximum", progressBar, (d, e) => ComputeViewModelProperties());
            desc = DependencyPropertyDescriptor.FromProperty(ProgressBar.MaximumProperty, typeof(ProgressBar));
            desc.AddValueChanged(progressBar, new EventHandler(ProgressBarDepPropChanged));
            
            //RegisterForNotification("Minimum", progressBar, (d, e) => ComputeViewModelProperties());
            desc = DependencyPropertyDescriptor.FromProperty(ProgressBar.MinimumProperty, typeof(ProgressBar));
            desc.AddValueChanged(progressBar, new EventHandler(ProgressBarDepPropChanged));

            ComputeViewModelProperties();
        }

        private void ProgressBarDepPropChanged(object sender, EventArgs e)
        {
            ComputeViewModelProperties();
        }

        /// Add a handler for a DP change
        /// see: http://amazedsaint.blogspot.com/2009/12/silverlight-listening-to-dependency.html
        private void RegisterForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
        {
            //Bind to a dependency property  
            Binding b = new Binding(propertyName) { Source = element };
            var prop = System.Windows.DependencyProperty.RegisterAttached(
                "ListenAttached" + propertyName,
                typeof(object),
                typeof(UserControl),
                new PropertyMetadata(callback));

            element.SetBinding(prop, b);
        }
    }
}
