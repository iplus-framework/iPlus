using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;

namespace gip.core.autocomponent
{
    /// <summary>
    /// The PWPointIn class is a special variant of a subscription point that is able to subscribe to the corresponding events of the starting points based on the workflow edges (ACClassWFEdge table). 
    /// In addition, it offers properties to query how many events have already been fired and to return the result AND-/ OR- or EXAND- linked accordingly. 
    /// An EXAND link is a special feature in iPlus, which means that only one output event was received and all other output events were not fired and the group of the workflow node to which they belong is not active.
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWPointIn'}de{'PWPointIn'}", Global.ACKinds.TACClass)]
    public class PWPointIn : PWPointEventSubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public PWPointIn()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public PWPointIn(PWBase parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        public PWPointIn(PWBase parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }
        #endregion

        /// <summary>
        /// Returns true if all Source-Events has fired (AND-Logic)
        /// Or if a OR-Logic was declared in the ACClassWFEdge-Declaration
        /// </summary>
        public bool IsActive 
        {
            get
            {
                bool allActive = true;
                foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                {
                    if (eventSubscrEntry.IsTriggerDirect && eventSubscrEntry.IsActive)
                        return true;
                    if (!eventSubscrEntry.IsActive)
                        allActive = false;
                }
                return allActive;
            }
        }


        /// <summary>
        /// Returns true if all Source-Events has fired (AND-Logic)
        /// </summary>
        /// <value>
        ///   <c>true</c> if all Source-Events has fired and; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveAND
        {
            get
            {
                foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                {
                    if (!eventSubscrEntry.IsActive)
                        return false;
                }
                return true;
            }
        }


        /// <summary>
        /// An EXAND link is a special feature in iPlus, which means that only one output event was received and all other output events were not fired and the group of the workflow node to which they belong is not active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if one Source-Events has fired when other groups are idle ; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveExAND
        {
            get
            {
                int countTotal = 0;
                int countActive = 0;
                int countInActive = 0;
                foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                {
                    countTotal++;
                    if (eventSubscrEntry.IsActive)
                    {
                        countActive++;
                    }
                    else //if (!eventSubscrEntry.IsActive)
                    {
                        PWBase pwBase = eventSubscrEntry.ValueT as PWBase;
                        if (pwBase != null)
                        {
                            PWGroup pwGroup = pwBase as PWGroup;
                            if (pwGroup == null)
                                pwGroup = pwBase.ParentACComponent as PWGroup;
                            if (pwGroup != null && (pwGroup.CurrentACState == ACStateEnum.SMIdle || pwGroup.CurrentACState == ACStateEnum.SMBreakPoint))
                                countInActive++;
                        }
                    }
                }
                return countActive == 1 && ((countInActive + countActive) == countTotal);
            }
        }


        /// <summary>
        /// Returns true if one Source-Events has fired (OR-Logic)
        /// </summary>
        /// <value>
        ///   <c>true</c> if one Source-Events has fired or; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveOR
        {
            get
            {
                foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                {
                    if (eventSubscrEntry.IsActive)
                        return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Count of received events
        /// </summary>
        public int ActiveEdgeCount
        {
            get
            {
                int activeCount = 0;
                foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                {
                    if (eventSubscrEntry.IsActive)
                        activeCount++;
                }
                return activeCount;
            }
        }


        /// <summary>
        /// Count of subscribed events
        /// </summary>
        public int EdgeCount
        {
            get
            {
                return ConnectionList.Count();
            }
        }


        /// <summary>Receiving an event. It sets ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive true.</summary>
        /// <param name="wrapObject"></param>
        public void UpdateActiveState(IACObject wrapObject)
        {
            ACPointEventSubscrWrap<ACComponent> eventSubscrEntry = GetWrapObject(wrapObject);
            if (eventSubscrEntry != null)
            {
                eventSubscrEntry.IsActive = true;
                // Broadcast to clients
                OnLocalStorageListChanged();
            }
        }


        /// <summary>
        /// Resets all ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive properties in the connectionlist to false.
        /// </summary>
        public void ResetActiveStates()
        {
            foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
            {
                eventSubscrEntry.IsActive = false;
            }
            // Broadcast to Clients
            OnLocalStorageListChanged();
        }


        public string DumpStateInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
            {
                sb.AppendLine(String.Format("ACUrl: {0}, ClientPointName: {1}, IsTriggerDirect: {2}, IsActive: {3}", eventSubscrEntry.ACUrl, eventSubscrEntry.ClientPointName, eventSubscrEntry.IsTriggerDirect, eventSubscrEntry.IsActive));
            }
            sb.AppendLine(String.Format("IsActive: {0}, IsActiveAND: {1}, IsActiveOR: {2}", IsActive, IsActiveAND, IsActiveOR));
            return sb.ToString();
        }
    }


    [DataContract]
    internal sealed class PWPointInProxy : PWPointEventSubscrProxy
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public PWPointInProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param name="parent"></param>
        public PWPointInProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }
        #endregion

        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            base.OnPointReceivedRemote(receivedPoint);
        }
    }

}
