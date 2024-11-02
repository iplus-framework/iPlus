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
using System.Windows.Controls.Primitives;
using gip.core.datamodel;
using gip.core.layoutengine;

namespace gip.core.visualcontrols
{
    [TemplatePart(Name = "PART_TickBar", Type = typeof(FrameworkElement))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBSlideValve'}de{'VBSlideValve'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBSlideValve : Control
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "SlideValveStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBSlideValve/Themes/SlideValveStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "SlideValveStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBSlideValve/Themes/SlideValveStyleGip.xaml" },
        };

        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBSlideValve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBSlideValve), new FrameworkPropertyMetadata(typeof(VBSlideValve)));
        }

        bool _themeApplied = false;
        public VBSlideValve()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }

        #endregion

        #region Additional Dependency-Properties

        public static readonly DependencyProperty IsOpenProperty
            = DependencyProperty.Register("IsOpen", typeof(Boolean), typeof(VBSlideValve));
        /// <summary>
        /// Füllstandsstriche
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean IsOpen
        {
            get { return (Boolean)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion

    }
}
