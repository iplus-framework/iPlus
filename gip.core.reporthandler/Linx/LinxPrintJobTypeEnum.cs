namespace gip.core.reporthandler
{
    public enum LinxPrintJobTypeEnum : byte
    {
        ControlCmd = 1,
        PrintRemote = 2,
        CheckStatus = 3,
        RasterData = 4,
        DeleteReport = 5,
        DownloadReport = 6,
        LoadReport = 7,
    }
}
