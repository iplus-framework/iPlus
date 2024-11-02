// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ACChildInfo : Attribute
    {
        /// <summary>
        /// </summary>
        public ACChildInfo(string instanceName, Type type)
        {
            InstanceName = instanceName;
            Type = type;
        }

        #region Properties
        public string InstanceName { get; set; }
        public Type Type { get; set; }
        #endregion
    }
}
