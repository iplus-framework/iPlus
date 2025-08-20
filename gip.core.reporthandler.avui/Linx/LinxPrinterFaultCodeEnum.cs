// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;

namespace gip.core.reporthandler.avui
{

    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxPrinterFaultCodeEnum'}de{'LinxPrinterFaultCodeEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public enum LinxPrinterFaultCodeEnum : byte
    {
        None = 01,
        Print_head_temperature = 01,
        Deflector_voltage = 02,
        EHT_trip = 02,
        Charge = 03,
        Phase_failure = 03,
        Time_of_Flight = 04,
        _300V_power_supply = 05,
        Temperature_Deflector = 06,
        Hardware_safety_trip = 06,
        Ink_tank_empty = 07,
        Ink_Overflow = 08,
        Internal_Spillage = 08,
        Phase = 09,
        Other = 10,
        Solvent_tank_empty = 10,
        Jet_Misaligned = 11,
        Pressure_Limit = 12,
        Viscosity = 13,
        Low_Temperature = 33,
        High_Temperature = 34,
        Line_Pressure = 36,
        Reservoir_Pressure = 36,
        Ink_tank_empty1 = 37,
    }
}
