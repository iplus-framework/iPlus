// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using gip.core.media;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Logfilereader Line'}de{'Logfilereader Zeile'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class LogfileReaderLine
    {
        [ACPropertyInfo(1, "", "en{'Time'}de{'Uhrzeit'}")]
        public DateTime DateTime { get; set; }

        [ACPropertyInfo(2, "", "en{'MessageLevel'}de{'MessageLevel'}")]
        public eMsgLevel MessageLevel { get; set; }

        [ACPropertyInfo(3, "", "en{'Quelle'}de{'Source'}")]
        public string Source { get; set; }

        [ACPropertyInfo(4, "", "en{'Name'}de{'Name'}")]
        public string ACName { get; set; }

        [ACPropertyInfo(5, "", "en{'Message'}de{'Meldung'}")]
        public string Message { get; set; }

        public string OriginalLine { get; set; }

        public override string ToString()
        {
            return $"{DateTime:yyyy-MM-dd HH:mm:ss.ffff} {MessageLevel} {Source} {ACName} {Message}";
        }
    }
}