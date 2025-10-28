// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using Avalonia;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBOperatingMode'}de{'VBOperatingMode'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBOperatingMode : VBVisualControlBase
    {
        #region c'tors
        public VBOperatingMode() : base()
        {
        }
        #endregion

        #region Additional Dependency-Properties

        #region OperatingMode-Type

        public static readonly StyledProperty<Global.OperatingMode> OperatingModeProperty = AvaloniaProperty.Register<VBOperatingMode, Global.OperatingMode>(nameof(OperatingMode), Global.OperatingMode.Automatic);
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
