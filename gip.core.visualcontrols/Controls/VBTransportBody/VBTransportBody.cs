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
using gip.core.processapplication;
using gip.core.autocomponent;
namespace gip.core.visualcontrols
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTransportBody'}de{'VBTransportBody'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTransportBody : VBVisualControlBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TransportBodyStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBTransportBody/Themes/TransportBodyStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TransportBodyStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBTransportBody/Themes/TransportBodyStyleGip.xaml" },
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

        static VBTransportBody()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTransportBody), new FrameworkPropertyMetadata(typeof(VBTransportBody)));
        }

        bool _themeApplied = false;
        public VBTransportBody()
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

        #region OperatingMode
        public static readonly DependencyProperty OperatingModeProperty
            = DependencyProperty.Register("OperatingMode", typeof(Global.OperatingMode), typeof(VBTransportBody), new PropertyMetadata(Global.OperatingMode.Automatic));
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

        #region RunState
        public static readonly DependencyProperty RunStateProperty
            = DependencyProperty.Register("RunState", typeof(Boolean), typeof(VBTransportBody), new PropertyMetadata(false, new PropertyChangedCallback(OnRunStateChanged)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean RunState
        {
            get { return (Boolean)GetValue(RunStateProperty); }
            set { SetValue(RunStateProperty, value); }
        }

        #endregion

        #region ReqRunState
        public static readonly DependencyProperty ReqRunStateProperty
            = DependencyProperty.Register("ReqRunState", typeof(Boolean), typeof(VBTransportBody), new PropertyMetadata(false, new PropertyChangedCallback(OnRunStateChanged)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqRunState
        {
            get { return (Boolean)GetValue(ReqRunStateProperty); }
            set { SetValue(ReqRunStateProperty, value); }
        }
        #endregion


        #region DirectionLeft
        public static readonly DependencyProperty DirectionLeftProperty
            = DependencyProperty.Register("DirectionLeft", typeof(Boolean), typeof(VBTransportBody));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean DirectionLeft
        {
            get { return (Boolean)GetValue(DirectionLeftProperty); }
            set { SetValue(DirectionLeftProperty, value); }
        }
        #endregion

        #region ReqDirectionLeft
        public static readonly DependencyProperty ReqDirectionLeftProperty
            = DependencyProperty.Register("ReqDirectionLeft", typeof(Boolean), typeof(VBTransportBody));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqDirectionLeft
        {
            get { return (Boolean)GetValue(ReqDirectionLeftProperty); }
            set { SetValue(ReqDirectionLeftProperty, value); }
        }
        #endregion

        #region SpeedFast
        public static readonly DependencyProperty SpeedFastProperty
            = DependencyProperty.Register("SpeedFast", typeof(Boolean), typeof(VBTransportBody));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean SpeedFast
        {
            get { return (Boolean)GetValue(SpeedFastProperty); }
            set { SetValue(SpeedFastProperty, value); }
        }
        #endregion

        #region FaultState
        public static readonly DependencyProperty FaultStateProperty
            = DependencyProperty.Register("FaultState", typeof(PANotifyState), typeof(VBTransportBody), new PropertyMetadata(PANotifyState.Off, new PropertyChangedCallback(OnRunStateChanged)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PANotifyState FaultState
        {
            get { return (PANotifyState)GetValue(FaultStateProperty); }
            set { SetValue(FaultStateProperty, value); }
        }

        #endregion

        private static void OnRunStateChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is VBTransportBody)
            {
                VBTransportBody control = d as VBTransportBody;
                control.OnRunStateChanged(e);
            }
        }

        protected void OnRunStateChanged(DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Brushes

        public static readonly DependencyProperty BorderBrushAutoProperty
            = DependencyProperty.Register("BorderBrushAuto", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 200, G = 200, B = 200 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushAuto
        {
            get { return (SolidColorBrush)GetValue(BorderBrushAutoProperty); }
            set { SetValue(BorderBrushAutoProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushManualProperty
            = DependencyProperty.Register("BorderBrushManual", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 0 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushManual
        {
            get { return (SolidColorBrush)GetValue(BorderBrushManualProperty); }
            set { SetValue(BorderBrushManualProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushMaintProperty
            = DependencyProperty.Register("BorderBrushMaint", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushMaint
        {
            get { return (SolidColorBrush)GetValue(BorderBrushMaintProperty); }
            set { SetValue(BorderBrushMaintProperty, value); }
        }


        public static readonly DependencyProperty FillBrushIdleProperty
            = DependencyProperty.Register("FillBrushIdle", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 255 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushIdle
        {
            get { return (SolidColorBrush)GetValue(FillBrushIdleProperty); }
            set { SetValue(FillBrushIdleProperty, value); }
        }


        public static readonly DependencyProperty FillBrushRunningProperty
            = DependencyProperty.Register("FillBrushRunning", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 50, G = 255, B = 0 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushRunning
        {
            get { return (SolidColorBrush)GetValue(FillBrushRunningProperty); }
            set { SetValue(FillBrushRunningProperty, value); }
        }

        public static readonly DependencyProperty FillBrushFaultProperty
            = DependencyProperty.Register("FillBrushFault", typeof(SolidColorBrush), typeof(VBTransportBody), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushFault
        {
            get { return (SolidColorBrush)GetValue(FillBrushFaultProperty); }
            set { SetValue(FillBrushFaultProperty, value); }
        }

        #endregion

    }
}
