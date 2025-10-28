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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBSensor'}de{'VBSensor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBSensor : VBPAControlBase
    {
        #region c'tors
        public VBSensor() : base()
        {
        }

        #endregion

        #region Styled Properties

        #region Appearance

        #region Sensor-Type
        [ACClassInfo(Const.PackName_VarioSystem, "en{'SensorTypes'}de{'SensorTypes'}", Global.ACKinds.TACEnum)]
        public enum SensorTypes : short
        {
            Rectangle = 0,
            Circle = 1,
        }

        public static readonly StyledProperty<SensorTypes> SensorTypeProperty
            = AvaloniaProperty.Register<VBSensor, SensorTypes>(nameof(SensorType), SensorTypes.Rectangle);
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SensorTypes SensorType
        {
            get { return GetValue(SensorTypeProperty); }
            set { SetValue(SensorTypeProperty, value); }
        }
        #endregion

        #region Sensor-Role
        public static readonly StyledProperty<PAESensorRole> SensorRoleProperty
            = AvaloniaProperty.Register<VBSensor, PAESensorRole>(nameof(SensorRole), PAESensorRole.None);
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PAESensorRole SensorRole
        {
            get { return GetValue(SensorRoleProperty); }
            set { SetValue(SensorRoleProperty, value); }
        }
        #endregion

        #endregion

        #endregion

    }
}
