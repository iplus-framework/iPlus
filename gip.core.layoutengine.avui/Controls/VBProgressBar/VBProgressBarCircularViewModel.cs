using System.Linq;
using gip.ext.designer.avui;
using System.ComponentModel;
using System;
using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia;
using Avalonia.Interactivity;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// An attached view model that adapts a ProgressBar control to provide properties
    /// that assist in the creation of a circular template
    /// </summary>
    public class VBProgressBarCircularViewModel : Control
    {
        #region Attach attached property

        /// <summary>
        /// Represents the attached property for Attach.
        /// </summary>
        public static readonly AttachedProperty<VBProgressBarCircularViewModel> AttachProperty =
            AvaloniaProperty.RegisterAttached<VBProgressBarCircularViewModel, Control, VBProgressBarCircularViewModel>("Attach");

        /// <summary>
        /// Gets the Attach.
        /// </summary>
        /// <param name="d">The avalonia object.</param>
        /// <returns>The VBProgressBarCircularViewModel object.</returns>
        public static VBProgressBarCircularViewModel GetAttach(AvaloniaObject d)
        {
            return d.GetValue(AttachProperty);
        }

        /// <summary>
        /// Set the Attach.
        /// </summary>
        /// <param name="d">The avalonia object.</param>
        /// <param name="value">The VBProgressBarCircularViewModel object.</param>
        public static void SetAttach(AvaloniaObject d, VBProgressBarCircularViewModel value)
        {
            d.SetValue(AttachProperty, value);
        }

        static VBProgressBarCircularViewModel()
        {
            AttachProperty.Changed.Subscribe(OnAttachChanged);
        }

        private static void OnAttachChanged(AvaloniaPropertyChangedEventArgs e)
        {
            // set the view model as the DataContext for the rest of the template
            Control targetElement = e.Sender as Control;
            VBProgressBarCircularViewModel viewModel = e.NewValue as VBProgressBarCircularViewModel;
            if (targetElement != null && viewModel != null)
            {
                targetElement.DataContext = viewModel;

                // handle the loaded event
                targetElement.Loaded += Element_Loaded;
            }
        }

        /// <summary>
        /// Handle the Loaded event of the element to which this view model is attached
        /// in order to enable the attached
        /// view model to bind to properties of the parent element
        /// </summary>
        static void Element_Loaded(object sender, RoutedEventArgs e)
        {
            Control targetElement = sender as Control;
            VBProgressBarCircularViewModel attachedModel = GetAttach(targetElement);

            // find the ProgressBar using visual tree traversal
            var progressBar = targetElement.GetVisualAncestors().OfType<ProgressBar>().FirstOrDefault();
            if (progressBar != null)
            {
                attachedModel.SetProgressBar(progressBar);
            }
        }

        #endregion

        #region fields

        protected ProgressBar _progressBar;

        #endregion

        #region properties

        /// <summary>
        /// Represents the styled property for Percent.
        /// </summary>
        public static readonly StyledProperty<double> PercentProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(Percent));

        /// <summary>
        /// Gets or sets the Percent.
        /// </summary>
        [Category("VBControl")]
        public double Percent
        {
            get { return GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Diameter.
        /// </summary>
        public static readonly StyledProperty<double> DiameterProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(Diameter));

        /// <summary>
        /// Gets or sets the Diameter.
        /// </summary>
        [Category("VBControl")]
        public double Diameter
        {
            get { return GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Radius.
        /// </summary>
        public static readonly StyledProperty<double> RadiusProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(Radius));

        /// <summary>
        /// Gets or sets the Radius.
        /// </summary>
        [Category("VBControl")]
        public double Radius
        {
            get { return GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for InnerRadius.
        /// </summary>
        public static readonly StyledProperty<double> InnerRadiusProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(InnerRadius));

        /// <summary>
        /// Gets or sets the InnerRadius.
        /// </summary>
        [Category("VBControl")]
        public double InnerRadius
        {
            get { return GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CentreX.
        /// </summary>
        public static readonly StyledProperty<double> CentreXProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(CentreX));

        /// <summary>
        /// Gets or sets the CentreX.
        /// </summary>
        [Category("VBControl")]
        public double CentreX
        {
            get { return GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CentreY.
        /// </summary>
        public static readonly StyledProperty<double> CentreYProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(CentreY));

        /// <summary>
        /// Gets or sets the CentreY.
        /// </summary>
        [Category("VBControl")]
        public double CentreY
        {
            get { return GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Angle.
        /// </summary>
        public static readonly StyledProperty<double> AngleProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(Angle));

        /// <summary>
        /// Gets or sets the Angle.
        /// </summary>
        [Category("VBControl")]
        public double Angle
        {
            get { return GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for HoleSizeFactor.
        /// </summary>
        public static readonly StyledProperty<double> HoleSizeFactorProperty = 
            AvaloniaProperty.Register<VBProgressBarCircularViewModel, double>(nameof(HoleSizeFactor));

        /// <summary>
        /// Gets or sets the HoleSizeFactor.
        /// </summary>
        [Category("VBControl")]
        public double HoleSizeFactor
        {
            get { return GetValue(HoleSizeFactorProperty); }
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
            CentreX = _progressBar.Bounds.Width / 2;
            CentreY = _progressBar.Bounds.Height / 2;
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
            
            // Subscribe to property changes using Avalonia's property change notifications
            _progressBar.GetObservable(ProgressBar.ValueProperty).Subscribe(_ => ComputeViewModelProperties());
            _progressBar.GetObservable(ProgressBar.MaximumProperty).Subscribe(_ => ComputeViewModelProperties());
            _progressBar.GetObservable(ProgressBar.MinimumProperty).Subscribe(_ => ComputeViewModelProperties());

            ComputeViewModelProperties();
        }
    }
}
