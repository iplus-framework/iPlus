// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;

namespace gip.core.reporthandlerwpf
{

    /// <summary>
    /// Return value for jet state
    /// part of status response
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxJetStateEnum'}de{'LinxJetStateEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public enum LinxJetStateEnum : byte
    {
        Jet_Running = 0x00,
        Jet_Startup = 0x01,
        Jet_Shutdown = 0x02,
        Jet_Stopped = 0x03,
        Fault = 0x04,
        Warming_Up = 0x05
    }
}
