using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointClientACObject'}de{'ACPointClientACObject'}", Global.ACKinds.TACClass)]
    public sealed class ACPointClientACObject : ACPointClientObject<ACComponent>
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointClientACObject()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointClientACObject(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ACPointClientACObject" /> class.</summary>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="maxCapacity">The maximum capacity.</param>
        public ACPointClientACObject(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion
    }
}

