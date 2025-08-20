// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.reporthandler.avui
{
    public enum LinxPrintJobTypeEnum : byte
    {
        ControlCmd = 1,
        PrintRemote = 2,
        CheckStatus = 3,
        RasterData = 4,
        DeleteReport = 5,
        DownloadReport = 6,
        LoadReport = 7,
    }
}
