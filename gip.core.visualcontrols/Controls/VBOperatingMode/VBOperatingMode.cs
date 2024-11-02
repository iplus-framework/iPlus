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
using System.Windows.Media.Animation;
using gip.core.datamodel;
using gip.core.layoutengine;

namespace gip.core.visualcontrols
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBOperatingMode'}de{'VBOperatingMode'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBOperatingMode : VBVisualControlBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "OperatingModeStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBOperatingMode/Themes/OperatingModeStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "OperatingModeStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBOperatingMode/Themes/OperatingModeStyleGip.xaml" },
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

        static VBOperatingMode()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBOperatingMode), new FrameworkPropertyMetadata(typeof(VBOperatingMode)));
        }

        bool _themeApplied = false;
        public VBOperatingMode()
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

        #region OperatingMode-Type

        public static readonly DependencyProperty OperatingModeProperty
            = DependencyProperty.Register("OperatingMode", typeof(Global.OperatingMode), typeof(VBOperatingMode), new PropertyMetadata(Global.OperatingMode.Automatic));
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.OperatingMode OperatingMode
        {
            get { return (Global.OperatingMode)GetValue(OperatingModeProperty); }
            set { SetValue(OperatingModeProperty, value); }
        }
        #endregion

        #endregion

    }
}
