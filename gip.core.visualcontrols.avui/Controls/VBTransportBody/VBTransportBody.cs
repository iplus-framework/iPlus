// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.processapplication;
using gip.core.autocomponent;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTransportBody'}de{'VBTransportBody'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTransportBody : VBVisualControlBase
    {
        #region c'tors
        public VBTransportBody() : base()
        {
        }

        #endregion

        #region Additional Styled Properties

        #region OperatingMode
        public static readonly StyledProperty<Global.OperatingMode> OperatingModeProperty
            = AvaloniaProperty.Register<VBTransportBody, Global.OperatingMode>(nameof(OperatingMode), Global.OperatingMode.Automatic);
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.OperatingMode OperatingMode
        {
            get { return GetValue(OperatingModeProperty); }
            set { SetValue(OperatingModeProperty, value); }
        }
        #endregion

        #region RunState
        public static readonly StyledProperty<bool> RunStateProperty
            = AvaloniaProperty.Register<VBTransportBody, bool>(nameof(RunState), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool RunState
        {
            get { return GetValue(RunStateProperty); }
            set { SetValue(RunStateProperty, value); }
        }

        #endregion

        #region ReqRunState
        public static readonly StyledProperty<bool> ReqRunStateProperty
            = AvaloniaProperty.Register<VBTransportBody, bool>(nameof(ReqRunState), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ReqRunState
        {
            get { return GetValue(ReqRunStateProperty); }
            set { SetValue(ReqRunStateProperty, value); }
        }
        #endregion


        #region DirectionLeft
        public static readonly StyledProperty<bool> DirectionLeftProperty
            = AvaloniaProperty.Register<VBTransportBody, bool>(nameof(DirectionLeft));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool DirectionLeft
        {
            get { return GetValue(DirectionLeftProperty); }
            set { SetValue(DirectionLeftProperty, value); }
        }
        #endregion

        #region ReqDirectionLeft
        public static readonly StyledProperty<bool> ReqDirectionLeftProperty
            = AvaloniaProperty.Register<VBTransportBody, bool>(nameof(ReqDirectionLeft));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ReqDirectionLeft
        {
            get { return GetValue(ReqDirectionLeftProperty); }
            set { SetValue(ReqDirectionLeftProperty, value); }
        }
        #endregion

        #region SpeedFast
        public static readonly StyledProperty<bool> SpeedFastProperty
            = AvaloniaProperty.Register<VBTransportBody, bool>(nameof(SpeedFast));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool SpeedFast
        {
            get { return GetValue(SpeedFastProperty); }
            set { SetValue(SpeedFastProperty, value); }
        }
        #endregion

        #region FaultState
        public static readonly StyledProperty<PANotifyState> FaultStateProperty
            = AvaloniaProperty.Register<VBTransportBody, PANotifyState>(nameof(FaultState), PANotifyState.Off);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PANotifyState FaultState
        {
            get { return GetValue(FaultStateProperty); }
            set { SetValue(FaultStateProperty, value); }
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == RunStateProperty || 
                change.Property == ReqRunStateProperty || 
                change.Property == FaultStateProperty)
            {
                OnRunStateChanged(change);
            }
        }

        protected void OnRunStateChanged(AvaloniaPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Brushes

        public static readonly StyledProperty<ISolidColorBrush> BorderBrushAutoProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(BorderBrushAuto), 
                new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush BorderBrushAuto
        {
            get { return GetValue(BorderBrushAutoProperty); }
            set { SetValue(BorderBrushAutoProperty, value); }
        }

        public static readonly StyledProperty<ISolidColorBrush> BorderBrushManualProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(BorderBrushManual), 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush BorderBrushManual
        {
            get { return GetValue(BorderBrushManualProperty); }
            set { SetValue(BorderBrushManualProperty, value); }
        }

        public static readonly StyledProperty<ISolidColorBrush> BorderBrushMaintProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(BorderBrushMaint), 
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush BorderBrushMaint
        {
            get { return GetValue(BorderBrushMaintProperty); }
            set { SetValue(BorderBrushMaintProperty, value); }
        }


        public static readonly StyledProperty<ISolidColorBrush> FillBrushIdleProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(FillBrushIdle), 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush FillBrushIdle
        {
            get { return GetValue(FillBrushIdleProperty); }
            set { SetValue(FillBrushIdleProperty, value); }
        }


        public static readonly StyledProperty<ISolidColorBrush> FillBrushRunningProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(FillBrushRunning), 
                new SolidColorBrush(Color.FromArgb(255, 50, 255, 0)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush FillBrushRunning
        {
            get { return GetValue(FillBrushRunningProperty); }
            set { SetValue(FillBrushRunningProperty, value); }
        }

        public static readonly StyledProperty<ISolidColorBrush> FillBrushFaultProperty
            = AvaloniaProperty.Register<VBTransportBody, ISolidColorBrush>(nameof(FillBrushFault), 
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush FillBrushFault
        {
            get { return GetValue(FillBrushFaultProperty); }
            set { SetValue(FillBrushFaultProperty, value); }
        }

        #endregion

    }
}
