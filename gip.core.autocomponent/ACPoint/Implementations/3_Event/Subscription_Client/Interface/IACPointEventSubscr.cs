using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointEventSubscr<T> : IACPointNet<T, ACPointEventSubscrWrap<T>>, IACPointNetClient
        where T : ACComponent
    {
        #region Subscribe
        /// <summary>Subscribes an Event at atACObject, for CallbackMethods, which are defined in a assembly</summary>
        /// <param name="atACComponent"></param>
        /// <param name="eventName">Name of event</param>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, ACPointNetEventDelegate AsyncCallbackDelegate);

        /// <summary>Subscribes an Event from atACObject, for CallbackMethods, which are defined by script</summary>
        /// <param name="atACComponent"></param>
        /// <param name="eventName">Name of even</param>
        /// <param name="asyncCallbackDelegateName">Name of Event-Handler-CallBack-Delegate of this</param>
        /// <returns>
        ///   <br />
        /// </returns>
        ACPointEventSubscrWrap<T> SubscribeEvent(IACObject atACComponent, string eventName, string asyncCallbackDelegateName);

        /// <summary>Subscribes all events of atACObject</summary>
        /// <param name="atACComponent"></param>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool SubscribeAllEvents(IACComponent atACComponent, ACPointNetEventDelegate AsyncCallbackDelegate);
        #endregion

        #region Unsubscribe
        /// <summary>Unsubscribes this Event-Subscription at the Event (eventName) at a ACObject (atACUrl)
        /// (Removes automatically the corresponding EventHandler-Delegate at IACPointEvent)</summary>
        /// <param name="atACComponent"></param>
        /// <param name="eventName">Name of event</param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool UnSubscribeEvent(IACComponent atACComponent, string eventName);

        /// <summary>Unsubscribes all subscribed Events of this Event-Subscription
        /// at a ACObject (removes automatically all EventHandler-Delegates at IACPointEvent)</summary>
        /// <param name="atACComponent"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool UnSubscribeAllEvents(IACComponent atACComponent);

        bool UnSubscribeAllEvents();
        #endregion

        #region Contains
        /// <summary>Check if this Event-Subscription subscribes the Event (eventName) at a ACObject (atACUrl)</summary>
        /// <param name="atACUrl"></param>
        /// <param name="eventName"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool Contains(string atACUrl, string eventName);

        /// <summary>Check if this Event-Subscription subscribes the Event for this EventHandler-Delegate</summary>
        /// <param name="atACComponent"></param>
        /// <param name="AsyncCallbackDelegate"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool Contains(IACComponent atACComponent, ACPointNetEventDelegate AsyncCallbackDelegate);
        #endregion
    }

    public interface IACPointEventSubscr : IACPointEventSubscr<ACComponent>
    {
    }
}

