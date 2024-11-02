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
    internal sealed class ACPointTaskProxy : ACPointAsyncRMIProxy
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointTaskProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointTaskProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }
        #endregion

        #region Method's
        public override bool Persist(bool withLock)
        {
            // Persistierung durch Seralisierung der LocalStorage-Liste
            // return base.Persist(withLock);
            // Persistierung per Tabelle:
            return ACPointTask.PersistToACClassTask(this, withLock);
        }

        internal override bool ReStoreFromDB()
        {
            // Restore durch De-Seralisierung der LocalStorage-Liste
            // return base.ReStoreFromDB();
            // Restore per Tabelle:
            return ACPointTask.ReStoreFromACClassTask(this);
        }
        #endregion

    }
}

