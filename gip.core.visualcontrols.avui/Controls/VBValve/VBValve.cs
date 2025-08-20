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
using gip.core.layoutengine.avui;
using gip.core.processapplication;
using gip.core.autocomponent;


namespace gip.core.visualcontrols.avui
{
    [TemplatePart(Name = "PART_TickBar", Type = typeof(FrameworkElement))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBValve'}de{'VBValve'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBValve : VBPAControlBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ValveStyleGip", 
                                         styleUri = "/gip.core.visualcontrols.avui;Component/Visualisierung/VBValve/Themes/ValveStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ValveStyleGip", 
                                         styleUri = "/gip.core.visualcontrols.avui;Component/Visualisierung/VBValve/Themes/ValveStyleGip.xaml" },
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

        static VBValve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBValve), new FrameworkPropertyMetadata(typeof(VBValve)));
        }

        bool _themeApplied = false;
        public VBValve()
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

        #region Dependency-Properties

        #region Appearance

        #region Valve-Type
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ValveTypes'}de{'ValveTypes'}", Global.ACKinds.TACEnum)]
        public enum ValveTypes : short
        {
            OneWayValve = 0,
            TwoWayValve = 1,
            ThreeWayValve = 2,
            OneWaySlide = 3,
            OneWayFlap = 4,
            TwoWayFlap = 5,
            DiverterLeft = 6,
            DiverterRight = 7,
            MixingValve = 8,
            AnalogValve = 9,
            SimpleRect = 10,
            TwoWayValveDivertR = 11,
            TwoWayValveDivertL = 12,
            ThreeWayValve3Flange = 13,
            DiverterLeftInv = 14,
            DiverterRightInv = 15,
        }

        public static readonly DependencyProperty ValveTypeProperty
            = DependencyProperty.Register("ValveType", typeof(ValveTypes), typeof(VBValve), new PropertyMetadata(ValveTypes.OneWayValve));
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ValveTypes ValveType
        {
            get { return (ValveTypes)GetValue(ValveTypeProperty); }
            set { SetValue(ValveTypeProperty, value); }
        }
        #endregion // Valve-Type

        #endregion // Dependency-Properties

        #region Binding-Properties

        #region Pos1
        public static readonly DependencyProperty Pos1Property
            = DependencyProperty.Register("Pos1", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean Pos1
        {
            get { return (Boolean)GetValue(Pos1Property); }
            set { SetValue(Pos1Property, value); }
        }

        #endregion

        #region ReqPos1
        public static readonly DependencyProperty ReqPos1Property
            = DependencyProperty.Register("ReqPos1", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqPos1
        {
            get { return (Boolean)GetValue(ReqPos1Property); }
            set { SetValue(ReqPos1Property, value); }
        }
        #endregion

        #region Pos2
        public static readonly DependencyProperty Pos2Property
            = DependencyProperty.Register("Pos2", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean Pos2
        {
            get { return (Boolean)GetValue(Pos2Property); }
            set { SetValue(Pos2Property, value); }
        }

        #endregion

        #region ReqPos2
        public static readonly DependencyProperty ReqPos2Property
            = DependencyProperty.Register("ReqPos2", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqPos2
        {
            get { return (Boolean)GetValue(ReqPos2Property); }
            set { SetValue(ReqPos2Property, value); }
        }
        #endregion

        #region Pos3
        public static readonly DependencyProperty Pos3Property
            = DependencyProperty.Register("Pos3", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean Pos3
        {
            get { return (Boolean)GetValue(Pos3Property); }
            set { SetValue(Pos3Property, value); }
        }

        #endregion

        #region ReqPos3
        public static readonly DependencyProperty ReqPos3Property
            = DependencyProperty.Register("ReqPos3", typeof(Boolean), typeof(VBValve), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqPos3
        {
            get { return (Boolean)GetValue(ReqPos3Property); }
            set { SetValue(ReqPos3Property, value); }
        }
        #endregion

        #endregion

        #endregion

    }
}
