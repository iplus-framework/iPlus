// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.shared;

namespace gip.core.layoutenginewpf
{
    /// <summary>
    /// VBTabitemRoot ist ein spezialisiertes VBTabitem, welches als Haupttabitem verwendet wird.
    /// Nur beim VBTabitemRoot wird bei der aktualisierung der Daten zwischen 
    /// cvtFixPanel und cvtPinPanel unterschieden. Hier ist das cvtPinPanel immer das Suchgrid
    /// </summary>
    public class VBTabitemRoot : VBTabitem
    {
        public VBTabitemRoot(IBSO bso, IExecute execute, string fixLayouts, string pinLayouts, string defaultLayout, string caption)
        {
            RootPanel = true;
            CloseAble = true;
            BSO = bso;
            Execute = execute;
            Caption = caption;
            Header = caption;
            FixLayouts = fixLayouts;
            PinLayouts = pinLayouts;
            DefaultLayout = defaultLayout;
        }
    }
}
