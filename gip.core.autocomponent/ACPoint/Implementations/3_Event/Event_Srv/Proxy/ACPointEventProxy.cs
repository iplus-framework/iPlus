// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    internal sealed class ACPointEventProxy : ACPointNetEventProxy<ACComponent>, IACPointEvent
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEventProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointEventProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }
        #endregion

        /*public static ACPointEventProxy operator +(ACPointEventProxy sEvent, ACPointNetEventDelegate AsyncCallbackDelegate, string ClientPointName)
        {
            sEvent.SubscribeEvent(AsyncCallbackDelegate, ClientPointName);
            return sEvent;
        }*/

    }
}

