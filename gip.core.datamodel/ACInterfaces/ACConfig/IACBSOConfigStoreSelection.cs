// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface that derives IACConfigStoreSelection and it's intended only for Business-Objects.
    /// It helps to determine how many subworkflows the user has currently navigated.
    /// </summary>
    public interface IACBSOConfigStoreSelection : IACConfigStoreSelection
    {
        void AddVisitedMethods(ACClassMethod acClassMethod);

        List<ACClassMethod> VisitedMethods { get; }
    }
}
