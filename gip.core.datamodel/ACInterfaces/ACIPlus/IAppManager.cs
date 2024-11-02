// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IAppManager : IACComponent
    {
        string RoutingServiceACUrl
        {
            get;
        }

        ACDelegateQueue ApplicationQueue
        {
            get;
        }

        //string[] FindMatchingUrls(FindMatchingUrlsParam queryParam);

        Dictionary<string, object> GetACComponentACMemberValues(Dictionary<string, string> acUrl_AcMemberIdentifiers);
    }
}
