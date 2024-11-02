// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.installerAndUpdater
{
    public enum RollbackAvailablity : short
    {
        Search = 0,
        No = 10,
        Yes = 20
    }

    public enum AppMode : short
    {
        Install = 0,
        Update = 10,
        Rollback = 20
    }

    public enum CloseState : short
    {
        WithMessage = 0,
        WithoutMessage = 10,
        WithoutMessageFinish = 20
    }

    public enum RollbackType : short
    {
        Disabled = 1,
        EnabledManual = 2,
        EnabledAutomatic = 3
    }

    public enum SqlConfigurationState
    {
        NoConnection = 0,
        UserNotSysAdmin = 10,
        AllOk = 20
    }

    public enum SqlBrowserServiceState
    {
        ServiceRunning = 0,
        ServiceNotRunning = 10,
        ServiceNotExist = 20
    }

    public enum SqlServerLocation
    {
        Local = 0,
        Remote = 10
    }
}
