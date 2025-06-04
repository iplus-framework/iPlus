// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Interface for Workflow-classes that are derivations of PWBaseExecutable, that enables the usage of overridable configuration stores.
    /// When users changes the configuration on client-side, the configuration of all affected workflow-nodes must be reloaded afterwards.
    /// Thefore the root-node PWProcessFunction calls the method ClearMyConfiguration() of all childs to reset their local cache.
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACComponentPWNode" />
    public interface IACMyConfigCache : IACComponentPWNode
    {
        bool IsConfigurationLoaded { get;  }
        ACMethod MyConfiguration { get; }
        void ClearMyConfiguration();
    }
}
