// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Base-Interface for tables which are filled by binding to a datasource
    /// </summary>
    public interface ITableRowData : IDictRef
    {
        /// <summary>
        /// ACUrl to DataSource
        /// </summary>
        string VBSource { get; set; }
    }
}
