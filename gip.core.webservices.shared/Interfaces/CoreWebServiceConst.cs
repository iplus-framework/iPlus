// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.webservices
{
    public static class CoreWebServiceConst
    {
        public const string UriLogin = "Login/{userName}";
        public const string UriLogin_F = "Login/{0}";

        public const string UriLogout = "Logout/{sessionID}";
        public const string UriLogout_F = "Logout/{0}";

        public const string UriACClass_BarcodeID = "ACClasses/barcode/{barcodeID}";
        public const string UriACClass_BarcodeID_F = "ACClasses/barcode/{0}";

        public const string UriDumpPerfLog = "DumpPerfLog";


        public const string EmptyParam = "*";

        public static Guid? DecodeSessionIdFromCookieHeader(string cookieHeader)
        {
            if (String.IsNullOrEmpty(cookieHeader))
                return null;
            string[] values = cookieHeader.Split(';');
            if (values == null || !values.Any())
                return null;
            foreach (string kvp in values)
            {
                string[] kvpArr = kvp.Split('=');
                if (kvpArr == null || kvpArr.Count() != 2)
                    continue;
                Guid guid;
                if (Guid.TryParse(kvpArr[1], out guid))
                    return guid;
            }
            return null;
        }

        public static string BuildSessionIdForCookieHeader(Guid guid)
        {
            return String.Format("SessionID={0}", guid.ToString());
        }
    }
}
