// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿
namespace gip.core.datamodel
{
    /// <summary>
    /// @aagincic: until now we don't support generic on GUI - for case select many acconfig items I write such as class
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACConfigSelected'}de{'ACConfigSelected'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACConfigSelected
    {
        [ACPropertyInfo(1, "ACConfig", "en{'ACConfig'}de{'ACConfig'}")]
        public IACConfig ACConfig { get; set; }


        [ACPropertyInfo(2, "Selected", "en{'Selected'}de{'Ausgewählt'}")]
        public bool Selected { get; set; }
    }
}
