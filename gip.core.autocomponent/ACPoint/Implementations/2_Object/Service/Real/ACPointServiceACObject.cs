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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointServiceACObject'}de{'ACPointServiceACObject'}", Global.ACKinds.TACClass)]
    public sealed class ACPointServiceACObject : ACPointServiceObject<ACComponent>
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointServiceACObject()
            : this(null, (ACClassProperty) null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointServiceACObject(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointServiceACObject" /> class.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        public ACPointServiceACObject(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }
        #endregion
    }
}

