using gip.core.datamodel;
using System.Collections.Generic;

namespace gip.core.autocomponent
{
    public interface IPrintManager : IACComponent
    {
        Msg Print(PAOrderInfo pAOrderInfo, int copyCount);

        PrinterInfo GetPrinterInfo(PAOrderInfo pAOrderInfo);

        List<PrinterInfo> GetPrintServers();

    }
}
