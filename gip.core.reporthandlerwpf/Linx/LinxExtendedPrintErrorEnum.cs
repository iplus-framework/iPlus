// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;

namespace gip.core.reporthandlerwpf
{

    /// <summary>
    /// Returned byte describe advanced print error
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxExtendedPrintErrorEnum'}de{'LinxExtendedPrintErrorEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public enum LinxExtendedPrintErrorEnum : byte
    {
        Cover_ovrerride_active = 0x00,
        Power_override_active = 0x01,
        Gutter_override_active = 0x02,
        Gate_array_test_mode = 0x03,
        Valid_UNIC_chip_not_found = 0x04,
        Message_memory_full = 0x05,
        Message_name_exist = 0x06
    }
}
