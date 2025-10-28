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
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBMotor'}de{'VBMotor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBMotor : VBPAControlBase
    {
        #region c'tors
        public VBMotor()
        {
        }

        protected override void OnInitialized()
        {
            TileViewport = new RelativeRect(0, 0, 0.2, 1, RelativeUnit.Relative);
            base.OnInitialized();
        }


        #endregion

        #region Styled Properties

        #region Appearance

        #region Motor-Type
        [ACClassInfo(Const.PackName_VarioSystem, "en{'MotorTypes'}de{'MotorTypes'}", Global.ACKinds.TACEnum)]
        public enum MotorTypes : short
        {
            Motor = 0,
            StarFeeder = 1,
            Screw = 2,
            Chain = 3,
            Conveyor = 4,
            ElevatorBody = 5,
            ElevTopLeft = 6,
            ElevTopRight = 7,
            ElevBottomLeft = 8,
            ElevBottomRight = 9,
            Pump = 10,
            PumpMembrane = 11,
            PumpGear = 12,
            Ventilator = 13,
            Sieve = 14,
            ScrewVert = 15,
            Filter = 16,
            Mixer = 17,
        }

        public static readonly StyledProperty<MotorTypes> MotorTypeProperty
            = AvaloniaProperty.Register<VBMotor, MotorTypes>(nameof(MotorType), MotorTypes.Motor);
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public MotorTypes MotorType
        {
            get { return GetValue(MotorTypeProperty); }
            set { SetValue(MotorTypeProperty, value); }
        }
        #endregion

        #region TileView
        public static readonly StyledProperty<RelativeRect> TileViewportProperty
            = AvaloniaProperty.Register<VBMotor, RelativeRect>(nameof(TileViewport), new RelativeRect(0, 0, 0.2, 1, RelativeUnit.Relative));
        public RelativeRect TileViewport
        {
            get { return GetValue(TileViewportProperty); }
            set { SetValue(TileViewportProperty, value); }
        }

        public static readonly StyledProperty<RelativeRect> TileViewportVertProperty
            = AvaloniaProperty.Register<VBMotor, RelativeRect>(nameof(TileViewportVert), new RelativeRect(0, 0, 0.2, 1, RelativeUnit.Relative));
        public RelativeRect TileViewportVert
        {
            get { return GetValue(TileViewportVertProperty); }
            set { SetValue(TileViewportVertProperty, value); }
        }
        #endregion

        #region Effects
        public static readonly StyledProperty<bool> RotorProperty
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(Rotor), false);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool Rotor
        {
            get { return GetValue(RotorProperty); }
            set { SetValue(RotorProperty, value); }
        }
        #endregion // Effects

        #endregion

        #region Binding-Properties

        #region RunState
        public static readonly StyledProperty<bool> RunStateProperty
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(RunState), false);

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
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(ReqRunState), false);

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
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(DirectionLeft));

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
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(ReqDirectionLeft));

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
            = AvaloniaProperty.Register<VBMotor, bool>(nameof(SpeedFast));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool SpeedFast
        {
            get { return GetValue(SpeedFastProperty); }
            set { SetValue(SpeedFastProperty, value); }
        }
        #endregion

        #endregion

        #endregion

        #region Methods

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size result = base.ArrangeOverride(arrangeBounds);

            double tileX = 0.2;
            if ((result.Width > 0) && (result.Height > 0))
            {
                double factor = result.Height / result.Width;
                if (factor < 1)
                    tileX = factor;
                else
                    tileX = 1;
            }

            if (tileX > 1)
                tileX = 1;
            else if (tileX < 0.01)
                tileX = 0.01;
            //tileX = 1 - tileX;
            TileViewport = new RelativeRect(0, 0, tileX, 1, RelativeUnit.Relative);

            double tileY = 0.2;
            if ((result.Width > 0) && (result.Height > 0))
            {
                double factor = result.Width / result.Height;
                if (factor < 1)
                    tileY = factor;
                else
                    tileY = 1;
            }

            if (tileY > 1)
                tileY = 1;
            else if (tileY < 0.01)
                tileY = 0.01;

            TileViewportVert = new RelativeRect(0, 0, tileY, 1, RelativeUnit.Relative);
            return result;
        }

        #endregion

    }
}
