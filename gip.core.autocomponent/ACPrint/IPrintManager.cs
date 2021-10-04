using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IPrintManager : IACComponent
    {
        Msg Print(PAOrderInfo pAOrderInfo, int copyCount);

        PrinterInfo GetPrinterInfo(PAOrderInfo pAOrderInfo);

        string PrintServers { get;set; }

    }
}
