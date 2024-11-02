// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    public interface IScheduledOrder
    {
        int? ScheduledOrder { get;set;}
        bool IsSelected { get;set;}
    }
}
