// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.autocomponent;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPAControlBase'}de{'VBPAControlBase'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public abstract class VBPAControlBase : VBVisualControlBase
    {
        #region Additional Styled Properties

        #region Appearance

        #region Effects
        public static readonly StyledProperty<bool> GlassEffectProperty
            = AvaloniaProperty.Register<VBPAControlBase, bool>(nameof(GlassEffect), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool GlassEffect
        {
            get { return GetValue(GlassEffectProperty); }
            set { SetValue(GlassEffectProperty, value); }
        }
        #endregion // Effects

        #region Brushes

        #region Borders
        public static readonly StyledProperty<ISolidColorBrush> BorderBrushAutoProperty
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(BorderBrushAuto), 
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
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(BorderBrushManual), 
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
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(BorderBrushMaint), 
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush BorderBrushMaint
        {
            get { return GetValue(BorderBrushMaintProperty); }
            set { SetValue(BorderBrushMaintProperty, value); }
        }
        #endregion

        #region Fill
        public static readonly StyledProperty<ISolidColorBrush> FillBrushIdleProperty
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(FillBrushIdle), 
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
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(FillBrushRunning), 
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
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(FillBrushFault), 
                new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush FillBrushFault
        {
            get { return GetValue(FillBrushFaultProperty); }
            set { SetValue(FillBrushFaultProperty, value); }
        }


        public static readonly StyledProperty<ISolidColorBrush> FillBrushInterlockedProperty
            = AvaloniaProperty.Register<VBPAControlBase, ISolidColorBrush>(nameof(FillBrushInterlocked), 
                new SolidColorBrush(Colors.DeepSkyBlue));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ISolidColorBrush FillBrushInterlocked
        {
            get { return GetValue(FillBrushInterlockedProperty); }
            set { SetValue(FillBrushInterlockedProperty, value); }
        }

        #endregion

        #endregion

        #endregion

        #region Binding-Properties

        #region OperatingMode
        public static readonly StyledProperty<Global.OperatingMode> OperatingModeProperty
            = AvaloniaProperty.Register<VBPAControlBase, Global.OperatingMode>(nameof(OperatingMode), Global.OperatingMode.Automatic);
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

        #region IsTriggered
        public static readonly StyledProperty<bool> IsTriggeredProperty
            = AvaloniaProperty.Register<VBPAControlBase, bool>(nameof(IsTriggered));
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IsTriggered
        {
            get { return GetValue(IsTriggeredProperty); }
            set { SetValue(IsTriggeredProperty, value); }
        }
        #endregion

        #region IsInterlocked
        public static readonly StyledProperty<bool> IsInterlockedProperty
            = AvaloniaProperty.Register<VBPAControlBase, bool>(nameof(IsInterlocked));
        /// <summary>
        /// Betriebsart
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IsInterlocked
        {
            get { return GetValue(IsInterlockedProperty); }
            set { SetValue(IsInterlockedProperty, value); }
        }
        #endregion

        #region FaultState
        public static readonly StyledProperty<PANotifyState> FaultStateProperty
            = AvaloniaProperty.Register<VBPAControlBase, PANotifyState>(nameof(FaultState), PANotifyState.Off);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PANotifyState FaultState
        {
            get { return GetValue(FaultStateProperty); }
            set { SetValue(FaultStateProperty, value); }
        }

        #endregion

        #endregion

        #endregion

    }
}
