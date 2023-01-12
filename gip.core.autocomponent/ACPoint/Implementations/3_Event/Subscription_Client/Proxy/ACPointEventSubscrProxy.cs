using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    internal sealed class ACPointEventSubscrProxy : ACPointNetEventSubscrProxy<ACComponent>, IACPointEventSubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEventSubscrProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointEventSubscrProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }
        #endregion
    }
}
