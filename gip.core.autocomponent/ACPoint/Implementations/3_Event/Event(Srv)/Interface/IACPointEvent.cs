using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public delegate void ACPointNetEventDelegate(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject);

    public interface IACPointEvent<T> : IACPointNet<T, ACPointEventWrap<T>>, IACPointNetService<T, ACPointEventWrap<T>>
        where T : ACComponent 
    {
        #region Subscribe without Client-Point-entry
        /// <summary>
        /// Subscribes this Event by Adding an Event-Handler-Delegate "of the Client-Object"
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate);

        /// <summary>Subscribes this Event, for CallbackMethods, which are defined by script</summary>
        /// <param name="fromACComponent"></param>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        ACPointEventWrap<T> SubscribeEvent(IACComponent fromACComponent, string asyncCallbackDelegateName);
        #endregion

        #region Subscribe with Client-Point-Entry
        /// <summary>
        /// Subscribes this Event by Adding an Event-Handler-Delegate "of the Client-Object". 
        /// This Method is Called over the IACPointEventSubscr-Interface.
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <param name="clientEntry"></param>
        /// <returns></returns>
        ACPointEventWrap<T> SubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate, ACPointEventSubscrWrap<T> clientEntry);

        /// <summary>
        /// Subscribes this Event, for CallbackMethods, which are defined by script
        /// This Method is Called over the IACPointEventSubscr-Interface.
        /// </summary>
        /// <param name="asyncCallbackDelegateName"></param>
        /// <param name="clientEntry"></param>
        /// <returns></returns>
        ACPointEventWrap<T> SubscribeEvent(string asyncCallbackDelegateName, ACPointEventSubscrWrap<T> clientEntry);
        #endregion

        #region Unsubscribe
        /// <summary>
        /// Unsubscribes this Event by Removing ACObject over the Event-Handler-Delegate
        /// </summary>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns></returns>
        bool UnSubscribeEvent(ACPointNetEventDelegate AsyncCallbackDelegate);

        /// <summary>Unsubscribes a ACObject from this Event</summary>
        /// <param name="fromACComponent"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool UnSubscribeEvent(IACComponent fromACComponent);
        #endregion

        #region Contains
        /// <summary>
        /// Check if ACObject is subscribed at this Event
        /// </summary>
        /// <param name="acUrl"></param>
        /// <returns></returns>
        bool Contains(string acUrl);

        bool Contains(ACPointNetEventDelegate AsyncCallbackDelegate);
        #endregion

        #region Raise Event
        /// <summary>
        /// Works only if Real Object is Owner
        /// </summary>
        void Raise(ACEventArgs Result);

        //ACEventArgs GetNewEventArgs();

        event EventHandler RaisingStarted;
        event EventHandler RaisingCompleted;
        #endregion
    }

    public interface IACPointEvent : IACPointEvent<ACComponent>
    {
    }
}

