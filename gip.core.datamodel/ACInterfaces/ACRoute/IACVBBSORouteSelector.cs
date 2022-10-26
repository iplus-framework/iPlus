using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IACVBBSORouteSelector
    {
        void GetAvailableRoutes(IEnumerable<ACClass> startComponents, IEnumerable<ACClass> endComponents);
        void EditRoutes(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated);

        IEnumerable<Route> RouteResult
        {
            get;
        }
    }
}
