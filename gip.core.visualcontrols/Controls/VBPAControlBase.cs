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
using gip.core.autocomponent;

namespace gip.core.visualcontrols
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPAControlBase'}de{'VBPAControlBase'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public abstract class VBPAControlBase : VBVisualControlBase
    {
        #region Additional Dependency-Properties

        #region Appearance

        #region Effects
        public static readonly DependencyProperty GlassEffectProperty
            = DependencyProperty.Register("GlassEffect", typeof(Visibility), typeof(VBPAControlBase), new PropertyMetadata(Visibility.Hidden));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Visibility GlassEffect
        {
            get { return (Visibility)GetValue(GlassEffectProperty); }
            set { SetValue(GlassEffectProperty, value); }
        }
        #endregion // Effects

        #region Brushes

        #region Borders
        public static readonly DependencyProperty BorderBrushAutoProperty
            = DependencyProperty.Register("BorderBrushAuto", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 200, G = 200, B = 200 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushAuto
        {
            get { return (SolidColorBrush)GetValue(BorderBrushAutoProperty); }
            set { SetValue(BorderBrushAutoProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushManualProperty
            = DependencyProperty.Register("BorderBrushManual", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 0 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushManual
        {
            get { return (SolidColorBrush)GetValue(BorderBrushManualProperty); }
            set { SetValue(BorderBrushManualProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushMaintProperty
            = DependencyProperty.Register("BorderBrushMaint", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 })));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush BorderBrushMaint
        {
            get { return (SolidColorBrush)GetValue(BorderBrushMaintProperty); }
            set { SetValue(BorderBrushMaintProperty, value); }
        }
        #endregion

        #region Fill
        public static readonly DependencyProperty FillBrushIdleProperty
            = DependencyProperty.Register("FillBrushIdle", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 255, B = 255 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushIdle
        {
            get { return (SolidColorBrush)GetValue(FillBrushIdleProperty); }
            set { SetValue(FillBrushIdleProperty, value); }
        }


        public static readonly DependencyProperty FillBrushRunningProperty
            = DependencyProperty.Register("FillBrushRunning", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 50, G = 255, B = 0 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushRunning
        {
            get { return (SolidColorBrush)GetValue(FillBrushRunningProperty); }
            set { SetValue(FillBrushRunningProperty, value); }
        }

        public static readonly DependencyProperty FillBrushFaultProperty
            = DependencyProperty.Register("FillBrushFault", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 })));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushFault
        {
            get { return (SolidColorBrush)GetValue(FillBrushFaultProperty); }
            set { SetValue(FillBrushFaultProperty, value); }
        }


        public static readonly DependencyProperty FillBrushInterlockedProperty
            = DependencyProperty.Register("FillBrushInterlocked", typeof(SolidColorBrush), typeof(VBPAControlBase), new PropertyMetadata(Brushes.DeepSkyBlue));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush FillBrushInterlocked
        {
            get { return (SolidColorBrush)GetValue(FillBrushInterlockedProperty); }
            set { SetValue(FillBrushInterlockedProperty, value); }
        }

        #endregion

        #endregion

        #endregion

        #region Binding-Properties

        #region OperatingMode
        public static readonly DependencyProperty OperatingModeProperty
            = DependencyProperty.Register("OperatingMode", typeof(Global.OperatingMode), typeof(VBPAControlBase), new PropertyMetadata(Global.OperatingMode.Automatic));
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

        #region IsTriggered
        public static readonly DependencyProperty IsTriggeredProperty
            = DependencyProperty.Register("IsTriggered", typeof(Boolean), typeof(VBPAControlBase));
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean IsTriggered
        {
            get { return (Boolean)GetValue(IsTriggeredProperty); }
            set { SetValue(IsTriggeredProperty, value); }
        }
        #endregion

        #region IsInterlocked
        public static readonly DependencyProperty IsInterlockedProperty
            = DependencyProperty.Register("IsInterlocked", typeof(Boolean), typeof(VBPAControlBase));
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean IsInterlocked
        {
            get { return (Boolean)GetValue(IsInterlockedProperty); }
            set { SetValue(IsInterlockedProperty, value); }
        }
        #endregion

        #region FaultState
        public static readonly DependencyProperty FaultStateProperty
            = DependencyProperty.Register("FaultState", typeof(PANotifyState), typeof(VBPAControlBase), new PropertyMetadata(PANotifyState.Off));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PANotifyState FaultState
        {
            get { return (PANotifyState)GetValue(FaultStateProperty); }
            set { SetValue(FaultStateProperty, value); }
        }

        #endregion

        #endregion

        #endregion

    }
}
