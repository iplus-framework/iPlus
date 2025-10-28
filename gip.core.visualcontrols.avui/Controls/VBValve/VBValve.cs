// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.processapplication;
using gip.core.autocomponent;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBValve'}de{'VBValve'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBValve : VBPAControlBase
    {
        #region c'tors
        public VBValve() : base()
        {
        }
        #endregion

        #region Styled Properties

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

        public static readonly StyledProperty<ValveTypes> ValveTypeProperty
            = AvaloniaProperty.Register<VBValve, ValveTypes>(nameof(ValveType), ValveTypes.OneWayValve);
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public ValveTypes ValveType
        {
            get { return GetValue(ValveTypeProperty); }
            set { SetValue(ValveTypeProperty, value); }
        }
        #endregion // Valve-Type

        #endregion // Styled Properties

        #region Binding-Properties

        #region Pos1
        public static readonly StyledProperty<bool> Pos1Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(Pos1), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool Pos1
        {
            get { return GetValue(Pos1Property); }
            set { SetValue(Pos1Property, value); }
        }

        #endregion

        #region ReqPos1
        public static readonly StyledProperty<bool> ReqPos1Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(ReqPos1), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ReqPos1
        {
            get { return GetValue(ReqPos1Property); }
            set { SetValue(ReqPos1Property, value); }
        }
        #endregion

        #region Pos2
        public static readonly StyledProperty<bool> Pos2Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(Pos2), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool Pos2
        {
            get { return GetValue(Pos2Property); }
            set { SetValue(Pos2Property, value); }
        }

        #endregion

        #region ReqPos2
        public static readonly StyledProperty<bool> ReqPos2Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(ReqPos2), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ReqPos2
        {
            get { return GetValue(ReqPos2Property); }
            set { SetValue(ReqPos2Property, value); }
        }
        #endregion

        #region Pos3
        public static readonly StyledProperty<bool> Pos3Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(Pos3), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool Pos3
        {
            get { return GetValue(Pos3Property); }
            set { SetValue(Pos3Property, value); }
        }

        #endregion

        #region ReqPos3
        public static readonly StyledProperty<bool> ReqPos3Property
            = AvaloniaProperty.Register<VBValve, bool>(nameof(ReqPos3), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ReqPos3
        {
            get { return GetValue(ReqPos3Property); }
            set { SetValue(ReqPos3Property, value); }
        }
        #endregion

        #endregion

        #endregion

    }
}
