using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public interface IRouteItemIDProvider : IACComponent
    {
        string RouteItemID { get; }
        int RouteItemIDAsNum { get; }
    }

    public interface IRoutableModule : IRouteItemIDProvider
    {
        IACContainerTNet<BitAccessForAllocatedByWay> AllocatedByWay { get; }

        void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff);

        void SimulateAllocationState(RouteItem item, bool switchOff);
    }
}
