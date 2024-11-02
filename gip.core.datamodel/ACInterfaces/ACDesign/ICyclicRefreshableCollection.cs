// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    // ***********************************************************************
    // Assembly         : gip.core.datamodel
    // Author           : aagincic
    // Created          : 26-01-2023
    //
    // Last Modified By : aagincic
    // Last Modified On : 26-01-2023
    public interface ICyclicRefreshableCollection
    {
        /// <summary>
        /// Enable refresh already GUI loaded elements
        /// </summary>
        void Refresh();
    }
}
