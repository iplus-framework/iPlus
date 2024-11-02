// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public static class ACCleanAssemblyListExt
    {
        public static void AddACCleanItem(this List<ACCleanAssembly> list, ACCleanItem aCCleanItem)
        {
            ACCleanAssembly aCCleanAssembly = list.FirstOrDefault(c => c.AssemblyName == aCCleanItem.AssemblyName);
            if(aCCleanAssembly == null)
            {
                aCCleanAssembly = new ACCleanAssembly(aCCleanItem.AssemblyName);
                list.Add(aCCleanAssembly);
            }

            if (!aCCleanAssembly.CleanItemList.Any(c => c.FullClassName == aCCleanItem.FullClassName))
                aCCleanAssembly.CleanItemList.Add(aCCleanItem);
        }
    }
}
