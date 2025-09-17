// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Layout;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Interaction logic for GridUnitSelector.xaml
	/// </summary>
    public partial class GridUnitSelector : UserControl
    {
        GridRailAdorner rail;
        
        // Cache the controls for performance
        private RadioButton _fixedButton;
        private RadioButton _starButton;
        private RadioButton _autoButton;

        public GridUnitSelector()
        {
            InitializeComponent();
        }

        public GridUnitSelector(GridRailAdorner rail) : this()
        {
            this.rail = rail;
            
            // Wire up event handlers after the control is loaded
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Cache control references and wire up event handlers
            _fixedButton = this.FindControl<RadioButton>("fixed");
            _starButton = this.FindControl<RadioButton>("star");
            _autoButton = this.FindControl<RadioButton>("auto");
            
            if (_fixedButton != null)
                _fixedButton.Click += FixedChecked;
            if (_starButton != null)
                _starButton.Click += StarChecked;
            if (_autoButton != null)
                _autoButton.Click += AutoChecked;
        }

        void FixedChecked(object sender, RoutedEventArgs e)
        {
            this.rail?.SetGridLengthUnit(Unit);
        }

        void StarChecked(object sender, RoutedEventArgs e)
        {
            this.rail?.SetGridLengthUnit(Unit);
        }

        void AutoChecked(object sender, RoutedEventArgs e)
        {
            this.rail?.SetGridLengthUnit(Unit);
        }

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<GridUnitSelector, Orientation>(nameof(Orientation));

        public Orientation Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public DesignItem SelectedItem { get; set; }

        public GridUnitType Unit
        {
            get
            {
                // Use cached references if available, otherwise find controls
                var autoButton = _autoButton ?? this.FindControl<RadioButton>("auto");
                var starButton = _starButton ?? this.FindControl<RadioButton>("star");
                
                if (autoButton?.IsChecked == true)
                    return GridUnitType.Auto;
                if (starButton?.IsChecked == true)
                    return GridUnitType.Star;

                return GridUnitType.Pixel;
            }
            set
            {
                // Use cached references if available, otherwise find controls
                var autoButton = _autoButton ?? this.FindControl<RadioButton>("auto");
                var starButton = _starButton ?? this.FindControl<RadioButton>("star");
                var fixedButton = _fixedButton ?? this.FindControl<RadioButton>("fixed");
                
                switch (value)
                {
                    case GridUnitType.Auto:
                        if (autoButton != null)
                            autoButton.IsChecked = true;
                        break;
                    case GridUnitType.Star:
                        if (starButton != null)
                            starButton.IsChecked = true;
                        break;
                    default:
                        if (fixedButton != null)
                            fixedButton.IsChecked = true;
                        break;
                }
            }

        }
        
        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            this.IsVisible = false;
        }
    }

}
