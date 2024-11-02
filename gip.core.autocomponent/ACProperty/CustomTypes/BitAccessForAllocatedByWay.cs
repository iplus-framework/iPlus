// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess AllocatedByWay'}de{'Bitzugriff AllocatedByWay'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACPropertyEntity(100, "Bit00", "en{'Reserved (Bit00/)'}de{'Reserviert (Bit00/)'}")]
    [ACPropertyEntity(110, "Bit01", "en{'Allocated by way (Bit01/)'}de{'Belegt durch Wegesteuerung (Bit01/)'}")]
    public class BitAccessForAllocatedByWay : BitAccessForByte
    {
        #region c'tors
        public BitAccessForAllocatedByWay()
        {
        }

        public BitAccessForAllocatedByWay(IACType acValueType)
            : base(acValueType)
        {
        }
        #endregion

        #region CustomizedBits

        public bool Bit00_Reserved
        {
            get
            {
                return Bit00;
            }
            set
            {
                Bit00 = value;
            }
        }

        public bool Bit01_Allocated
        {
            get
            {
                return Bit01;
            }
            set
            {
                Bit01 = value;
                if (value)
                    Bit00_Reserved = false;
            }
        }

        #endregion  
    }
}
