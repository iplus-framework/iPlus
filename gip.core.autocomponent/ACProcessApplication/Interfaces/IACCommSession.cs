// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACCommSession : IACComponent
    {
        IACContainerTNet<bool> IsConnected { get; set; }

        bool InitSession();
        bool IsEnabledInitSession();

        bool DeInitSession();
        bool IsEnabledDeInitSession();

        bool Connect();
        bool IsEnabledConnect();

        bool DisConnect();
        bool IsEnabledDisConnect();
    }
}
