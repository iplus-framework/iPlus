// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    public interface IACBSOACProgramProvider : IACComponent
    {
        bool IsEnabledACProgram { get; set; }

        string WorkflowACUrl { get; }

        IEnumerable<ACProgram> GetACProgram();

        string GetProdOrderProgramNo();

        DateTime GetProdOrderInsertDate();
    }
}
