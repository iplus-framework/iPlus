// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    /// <summary>
    /// General format for dialog result
    /// </summary>
    public class VBDialogResult
    {
        public IACObject ReturnValue { get; set; }

        public eMsgButton SelectedCommand { get; set; }
    }
}
