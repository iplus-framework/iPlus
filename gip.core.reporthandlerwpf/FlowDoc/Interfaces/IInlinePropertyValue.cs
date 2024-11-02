// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Interface for property values
    /// </summary>
    public interface IInlinePropertyValue : IInlineValue, IDictRef
    {
        /// <summary>
        /// ACUrl to Property
        /// </summary>
        string VBContent { get; set; }
    }
}
