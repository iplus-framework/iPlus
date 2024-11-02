// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IVBMediaControllerProxy
    {
        string OpenFileDialog(bool isFolderPicker, string initialDirectory, bool useExisting, string defaultExtension = null, Dictionary<string, string> filters = null);
        byte[] ResizeImage(string fileName, int maxWidth, int maxHeight, string quality = "Medium");
        Bitmap CreateLicenseImage(VBLicense CurrentVBLicense);
    }

    public interface IVBMediaControllerService
    {
        IVBMediaControllerProxy GetMediaControllerProxy(IACComponent component);
        void RemoveMediaControllerProxy(IACComponent component);
        IEnumerable<string> GetWindowsPrinters();
    }
}
