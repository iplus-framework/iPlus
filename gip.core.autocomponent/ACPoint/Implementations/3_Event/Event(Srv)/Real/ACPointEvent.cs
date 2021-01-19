using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointEvent'}de{'ACPointEvent'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, true)]
    public sealed class ACPointEvent : ACPointNetEventBase<ACComponent>, IACPointEvent
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointEvent()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public ACPointEvent(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        public ACPointEvent(IACComponent parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        /*public static ACPointEvent operator +(ACPointEvent sEvent, ACPointNetEventDelegate AsyncCallbackDelegate, string ClientPointName)
        {
            sEvent.SubscribeEvent(AsyncCallbackDelegate, ClientPointName);
            return sEvent;
        }*/

        public string DumpStateInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ACPointEventWrap<ACComponent> eventSubscrEntry in ConnectionList)
            {
                sb.AppendLine(String.Format("ACUrl: {0}, ClientPointName: {1}", eventSubscrEntry.ACUrl, eventSubscrEntry.ClientPointName));
            }
            return sb.ToString();
        }

    }
}

