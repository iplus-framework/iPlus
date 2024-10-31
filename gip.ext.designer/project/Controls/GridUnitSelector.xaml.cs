﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using gip.ext.design;

namespace gip.ext.designer.Controls
{
	/// <summary>
	/// Interaction logic for GridUnitSelector.xaml
	/// </summary>
    public partial class GridUnitSelector : UserControl
    {
        GridRailAdorner rail;

        public GridUnitSelector(GridRailAdorner rail)
        {
            InitializeComponent();

            this.rail = rail;
        }

        void FixedChecked(object sender, RoutedEventArgs e)
        {
            this.rail.SetGridLengthUnit(Unit);
        }

        void StarChecked(object sender, RoutedEventArgs e)
        {
            this.rail.SetGridLengthUnit(Unit);
        }

        void AutoChecked(object sender, RoutedEventArgs e)
        {
            this.rail.SetGridLengthUnit(Unit);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GridUnitSelector),
                                        new FrameworkPropertyMetadata());

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public DesignItem SelectedItem { get; set; }

        public GridUnitType Unit
        {
            get
            {
                if (auto.IsChecked == true)
                    return GridUnitType.Auto;
                if (star.IsChecked == true)
                    return GridUnitType.Star;

                return GridUnitType.Pixel;
            }
            set
            {
                switch (value)
                {
                    case GridUnitType.Auto:
                        auto.IsChecked = true;
                        break;
                    case GridUnitType.Star:
                        star.IsChecked = true;
                        break;
                    default:
                        @fixed.IsChecked = true;
                        break;
                }
            }

        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.Visibility = Visibility.Hidden;
        }
    }

}
