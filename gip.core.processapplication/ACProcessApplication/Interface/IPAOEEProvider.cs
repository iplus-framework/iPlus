// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO.Ports;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.processapplication
{
    public interface IPAOEEProvider : IACComponent
    {
        IACContainerTNet<AvailabilityState> AvailabilityState { get; }
        IACContainerTNet<Global.OperatingMode> OperatingMode { get; }
        IACContainerTNet<Boolean> Allocated { get; }
    }
}
