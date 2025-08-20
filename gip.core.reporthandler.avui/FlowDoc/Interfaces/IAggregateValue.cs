// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Interface for values to be used as aggregate values
    /// </summary>
    public interface IAggregateValue
    {
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        string AggregateGroup { get; set; }
    }
}
