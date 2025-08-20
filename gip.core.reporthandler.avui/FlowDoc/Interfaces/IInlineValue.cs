// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Interface for values
    /// </summary>
    public interface IInlineValue
    {
        /// <summary>
        /// Gets or sets the value format
        /// </summary>
        string StringFormat { get; set; }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        object Value { get; set; }

        int MaxLength { get; set; }
    }
}
