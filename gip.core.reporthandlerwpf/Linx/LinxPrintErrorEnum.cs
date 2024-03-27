using gip.core.datamodel;

namespace gip.core.reporthandlerwpf
{
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxPrintErrorEnum'}de{'LinxPrintErrorEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public enum LinxPrintErrorEnum: byte
    {
        No_TOF_adjustments = 00,
        Viscosity_Temperature = 00,
        Jet_shutdown_incomplete = 01,
        Over_speed_print_go = 02,
        Ink_low = 03,
        Solvent_low = 04,
        Print_go_Remote_data = 05,
        Service_time = 06,
        Print_head_cover_off = 07,
        Print_head_not_fitted = 08,
        Bad_print_head_code = 08,
        New_print_head_fitted = 09,
        Charge_calibration_range = 10,
        Line_calibration_error = 10,
        Print_Quality = 11,
        Safety_override_link_fitted = 11,
        Low_pressure = 12,
        Vacuum_pressure = 12,
        Modulation = 13,
        Short_Diverter_Delay = 13,
        Over_speed_variable_data = 14,
        Default_language = 15,
        Memory_failure = 16,
        Memory_corrupt = 17,
        No_message_in_memory = 18,
        OverSpeed, _Print_Verification = 18,
        TOF_under_range = 19,
        Purge_pad_replacement = 19,
        TOF_over_range = 20,
        Remote_alarm = 20,

        Default_keycodes = 21,
        Storage_corrupt = 22,
        No_Aux_Board_Fitted = 23,
        Print_Go_Old_pattern = 24,
        Parallel_IO_init = 25,
        Msg_change_in_Print_delay = 26,
        Print_Go_after_schedule_end = 27,
        Incompatibile_Aux_photocell_mode = 28,
        Invalid_parallel_input = 29,
        Long_Diverter_Delay = 30,
        Extended_errors_present = 31
    }
}
