// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SQLInstanceInfo'}de{'SQLInstanceInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACFSItemChanges
    {

        public ACFSItemChanges()
        {

        }

        public ACFSItemChanges(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue != null ? oldValue.ToString() : "";
            NewValue = newValue != null ? newValue.ToString() : "";
        }

        [ACPropertyInfo(1, "PropertyName", "en{'Name'}de{'Name'}")]
        public string PropertyName { get; set; }

        [ACPropertyInfo(2, "OldValue", "en{'Current value'}de{'Aktueller Wert'}")]
        public string OldValue { get; set; }

        [ACPropertyInfo(3, "NewValue", "en{'New value'}de{'Neuer Wert'}")]
        public string NewValue { get; set; }
    }
}
