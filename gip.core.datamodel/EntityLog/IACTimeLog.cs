// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.datamodel
{
    public interface IACTimeLog : INotifyPropertyChanged, IACObject
    {
        DateTime? StartDate
        {
            get;
            set;
        }

        DateTime? EndDate
        {
            get;
            set;
        }
    }
}
