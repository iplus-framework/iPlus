// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using DBSyncerUpdate.unit.test.Transfer;
using System;

namespace DBSyncerUpdate.unit.test
{
    public static class Program
    {
        public static void Main()
        {
            TransferCommand cmd = new TransferCommand();
            bool transferSuccess = cmd.DoTransfer();
            Console.WriteLine(transferSuccess);
        }
    }
}
