using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// An attached view model that adapts a ProgressBar control to provide properties
    /// that assist in the creation of a segmented circular template
    /// </summary>
    public class VBProgressBarSegmentedViewModel : VBProgressBarCircularViewModel
    {
        private int _segmentCount = 8;

        /// <summary>
        /// Gets or sets segment count.
        /// </summary>
        public int SegmentCount
        {
            get { return _segmentCount; }
            set
            {
                _segmentCount = value;
                BuildSegments();
                ComputeViewModelProperties();
            }
        }

        /// <summary>
        /// Represents the depnency property for Segments.
        /// </summary>
        public static readonly DependencyProperty SegmentsProperty = DependencyProperty.Register("Segments", typeof(List<SegmentData>), typeof(VBProgressBarCircularViewModel));

        /// <summary>
        /// Gets or sets the collection of segments (SegmentData).
        /// </summary>
        [Category("VBControl")]
        public List<SegmentData> Segments
        {
            get { return (List<SegmentData>)GetValue(SegmentsProperty); }
            set { SetValue(SegmentsProperty, value); }
        }

        /// <summary>
        /// Creates a new instance of VBProgressBarSegmentedViewModel.
        /// </summary>
        public VBProgressBarSegmentedViewModel()
        {
            BuildSegments();
        }

        private void BuildSegments()
        {
            var segments = new List<SegmentData>();
            double endAngle = 360.0 / (double)SegmentCount;
            for (int i = 0; i < SegmentCount; i++)
            {
                double startAngle = (double)i * 360 / (double)SegmentCount;
                segments.Add(new SegmentData(startAngle, endAngle, this));
            }

            Segments = segments;
        }

        /// <summary>
        /// Computes the ViewModel properties.
        /// </summary>
        protected override void ComputeViewModelProperties()
        {
            if (_progressBar == null)
                return;

            double normalValue = _progressBar.Value / (_progressBar.Maximum - _progressBar.Minimum);

            for (int i = 0; i < SegmentCount; i++)
            {
                double startValue = (double)i / (double)SegmentCount;
                double endValue = (double)(i + 1) / (double)SegmentCount;
                double opacity = (normalValue - startValue) / (endValue - startValue); ;
                opacity = Math.Min(1, Math.Max(0, opacity));
                Segments[i].Opacity = opacity;
            }
            base.ComputeViewModelProperties();
        }


        /// <summary>
        /// The data for an individual segment.
        /// </summary>
        public class SegmentData : INotifyPropertyChanged
        {
            /// <summary>
            /// Gets the StartAngle.
            /// </summary>
            public double StartAngle { get; private set; }

            /// <summary>
            /// Gets the WedgeAngle.
            /// </summary>
            public double WedgeAngle { get; private set; }

            private double _opacity;

            /// <summary>
            /// Gets or sets the Opacity.
            /// </summary>
            [Category("VBControl")]
            public double Opacity
            {
                get { return _opacity; }
                set { _opacity = value; OnPropertyChanged("Opacity"); }
            }

            /// <summary>
            /// Gets the parent.
            /// </summary>
            public VBProgressBarSegmentedViewModel Parent { get; private set; }

            /// <summary>
            /// Creates the new instance of SegmentData.
            /// </summary>
            /// <param name="start">The start parameter.</param>
            /// <param name="wedge">The wedge parameter.</param>
            /// <param name="parent">The parent parameter.</param>
            public SegmentData(double start, double wedge,
              VBProgressBarSegmentedViewModel parent)
            {
                StartAngle = start;
                WedgeAngle = wedge;
                Parent = parent;
                Opacity = 0;
            }

            #region INotifyPropertyChanged

            /// <summary>
            /// The PropertyChanged event.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Handles the OnPropertyChanged event.
            /// </summary>
            /// <param name="property">The name of property.</param>
            protected void OnPropertyChanged(string property)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
            }

            #endregion
        }
    }
}
