// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.datamodel;
using System.Windows.Controls.Primitives;
using gip.core.layoutengine;

namespace gip.core.visualcontrols
{
    [TemplatePart(Name = "PART_TickBar", Type = typeof(FrameworkElement))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFillLevel'}de{'VBFillLevel'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFillLevel : VBProgressBar
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList2 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "FillLevelStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBFillLevel/Themes/FillLevelStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "FillLevelStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBFillLevel/Themes/FillLevelStyleGip.xaml" },
        };

        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList2;
            }
        }

        static VBFillLevel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBFillLevel), new FrameworkPropertyMetadata(typeof(VBFillLevel)));
            VBProgressBar.ProgressBarStyleProperty.OverrideMetadata(typeof(VBFillLevel), new PropertyMetadata(ProgressBarStyles.PerformantBar));
        }
        #endregion

        #region Additional Dependency-Properties

        public static readonly DependencyProperty TickFrequencyProperty
            = DependencyProperty.Register("TickFrequency", typeof(double), typeof(VBFillLevel), new PropertyMetadata((double)10.0));
        /// <summary>
        /// Der Abstand zwischen Teilstrichen.Der Standardwert ist (1.0).
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }


        public static readonly DependencyProperty ShowTickBarProperty
            = DependencyProperty.Register("ShowTickBar", typeof(Boolean), typeof(VBFillLevel));
        /// <summary>
        /// Füllstandsstriche
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ShowTickBar
        {
            get { return (Boolean)GetValue(ShowTickBarProperty); }
            set { SetValue(ShowTickBarProperty, value); }
        }
        #endregion

    }
}
