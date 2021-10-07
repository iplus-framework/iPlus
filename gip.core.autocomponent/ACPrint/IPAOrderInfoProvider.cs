using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IPAOrderInfoProvider : IACComponent
    {
        PAOrderInfo GetOrderInfo();
    }
}
