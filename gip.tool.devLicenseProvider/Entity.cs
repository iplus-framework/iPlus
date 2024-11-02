// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.devLicenseProvider
{
    public partial class License
    {
        public string LicenseNoView
        {
            get
            {
                return string.Format("L{0}", LicenseNo.ToString("D4"));
            }
            set
            {
            }
        }
    }

    public partial class Customer
    {
        public string CustomerNoView
        {
            get
            {
                return string.Format("C{0}", CustomerNo.ToString("D4"));
            }
            set
            {
            }
        }
    }
}
