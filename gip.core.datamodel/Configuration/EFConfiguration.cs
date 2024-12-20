// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Entity-Framework-Configuration
    /// </summary>
    public class EFConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Command-Timeout for Entity-Framework-Queries
        /// </summary>
        [ConfigurationProperty("CommandTimeout", DefaultValue = (short)30, IsRequired = false)]
        public short CommandTimeout
        {
            get
            {
                return (short)this["CommandTimeout"];
            }
            set
            {
                this["CommandTimeout"] = value;
            }
        }

    }
}
