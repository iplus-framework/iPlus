namespace gip.core.reporthandler
{

    /// <summary>
    /// Returned byte describe advanced print error
    /// </summary>
    public enum LinxExtendedPrintErrorEnum : short
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
