namespace gip.core.reporthandler
{

    /// <summary>
    /// Return value for jet state
    /// part of status response
    /// </summary>
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
