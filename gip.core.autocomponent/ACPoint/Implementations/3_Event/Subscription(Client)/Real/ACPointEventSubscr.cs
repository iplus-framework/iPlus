using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointEventSubscr'}de{'ACPointEventSubscr'}", Global.ACKinds.TACClass)]
    public sealed class ACPointEventSubscr : ACPointNetEventSubscrBase<ACComponent>, IACPointEventSubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEventSubscr()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointEventSubscr(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointEventSubscr" /> class.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        public ACPointEventSubscr(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion
    }
}

