using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IACVBBSORouteSelector
    {
        void ShowAvailableRoutes(IEnumerable<ACClass> startComponents, IEnumerable<ACClass> endComponents, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true);
        void ShowAvailableRoutes(IEnumerable<Tuple<ACClass, ACClassProperty>> startPoints, IEnumerable<Tuple<ACClass, ACClassProperty>> endPoints, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true);
        void EditRoutes(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated);

        IEnumerable<Route> RouteResult
        {
            get;
        }
    }
}
