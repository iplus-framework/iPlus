// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Transactions;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Draws border, background or both around another element
    /// </summary>
    /// <summary xml:lang="de">
    /// Zeichnet Rahmen, Hintergrund oder beides um ein anderes Element herum
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBorder'}de{'VBBorder'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBBorder : Border
    {
    }
}
