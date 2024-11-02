// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Interface for property values
    /// </summary>
    public interface IDictRef
    {
        /// <summary>
        /// Gets or sets the Key of ReportData-Dictionary.
        /// Behind a key could be primitive Data, which is directly set in the ReportData-Dictionary
        /// or a DataTable. In this case the TableName is the key
        /// or a IACObject. In this case you should use the ACIdentifier of the IACObject
        /// or a ACComponent. In this case you should use the .net-Runtime-Typname of the class
        /// or a ACQueryDefinition. In this case you should user the ChildACUrl-Member of the Root-Object
        /// </summary>
        string DictKey { get; set; }
    }
}
