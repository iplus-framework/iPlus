using System;

namespace gip.core.datamodel
{
    #region Events and Delegates
    /// <summary>
    /// Signature of the Setter-Delegate-Methode in Assemblies (for Callback)
    /// </summary>
    /// <param name="value">The value.</param>
    public delegate void ACPropertySetMethod(IACPropertyNetValueEvent value);
    #endregion


    /// <summary>
    /// Interface for Network-capable properties. Network-capable properties automatically distribute their values across the network in the event of a change. Network-capable properties offer the possibility of so-called "property binding". The concept of property binding enables you to "connect" a property of component A with another property of component B in order to achieve automatic forwarding of value changes.
    /// </summary>
    public interface IACPropertyNetBase : IACPropertyBase
    {
        /// <summary>
        /// It's called when a new value has arrived from the real object on serverside to refresh the current value.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        void OnValueEventReceivedRemote(IACPropertyNetValueEvent eventArgs);


        /// <summary>
        /// Setter-Delegat-Methode von Assembly-ACObjekten (Callback)
        /// </summary>
        /// <value>The set method.</value>
        ACPropertySetMethod SetMethod { get; set; }


        /// <summary>
        /// Returns the current value inside a wrapper of type IACPropertyNetValueEvent
        /// </summary>
        /// <returns>
        /// IACPropertyNetValueEvent.
        /// </returns>
        IACPropertyNetValueEvent GetValueAsEvent();


        /// <summary>
        /// PAClassPhysicalBaseProperty values ​​can also be stored in long-term archives. To do this, the update rate must be set in the iPlus development environment. Set a threshold value in the "Log Filter" field so that not every marginal change in value is persisted. This is particularly useful for double and float values. Use this method to query these archived values.
        /// </summary>
        /// <param name="from">Filter time from</param>
        /// <param name="to">Filter time to</param>
        /// <returns>PropertyLogListInfo.</returns>
        PropertyLogListInfo GetArchiveLog(DateTime from, DateTime to);


        /// <summary>
        /// Every time when the Value-Property is set a broadcast to all subscribed components will be performed - whether the value has changed or not.
        /// If this property is false a broadcast happens only if the value has changed.
        /// </summary>
        /// <value><c>true</c> if broadcast should be forced; otherwise, <c>false</c>.</value>
        bool ForceBroadcast { get; set; }


        /// <summary>
        /// The last value changed.This value is passed as a request to the data source. 
        /// If a value change has occurred, the current valid value is set.
        /// If the value change has been rejected or changed, the feedback (possibly with error description) is written to this property.
        /// </summary>
        IACPropertyNetValueEvent CurrentChangeValueRequest { get; }
    }
}
