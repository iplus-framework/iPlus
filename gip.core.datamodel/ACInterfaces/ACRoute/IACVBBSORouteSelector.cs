// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IACVBBSORouteSelector
    {
        void ShowAvailableRoutes(IEnumerable<ACClass> startComponents, IEnumerable<ACClass> endComponents, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true, ACClass preselectedStart = null);
        void ShowAvailableRoutes(IEnumerable<Tuple<ACClass, ACClassProperty>> startPoints, IEnumerable<Tuple<ACClass, ACClassProperty>> endPoints, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true);
        void EditRoutes(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated);
        void EditRoutesWithAttach(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated);
        void ShowRoute(Route route);

        IEnumerable<Route> RouteResult
        {
            get;
        }
    }
}
