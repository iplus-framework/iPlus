namespace gip.core.reporthandler
{
    /// <summary>
    /// Return value for printer state
    /// </summary>
    public enum LinxPrintStateEnum : short
    {
        Printing = 0x00,
        Undefined = 0x01,
        Idle = 0x02,
        Generating_Pixels = 0x03,
        Waiting = 0x04,
        Last = 0x05,
        Printing_Generating_Pixels = 0x06
    }
}
