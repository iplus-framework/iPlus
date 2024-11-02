// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    /// <summary>
    /// Entity items supports default image (Grid preview)
    /// </summary>
    public interface  IImageInfo: IACObject
    {
        string DefaultImage { get; set; }
        string DefaultThumbImage { get; set; }
    }
}
