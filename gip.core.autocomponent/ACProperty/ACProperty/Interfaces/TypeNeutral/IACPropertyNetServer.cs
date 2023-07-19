using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Event-Args that are sended when a value inside a IACPropertyNetServer-Instance has changed and must be broadcasted.
    /// </summary>
    /// <seealso cref="System.ComponentModel.PropertyChangedEventArgs" />
    public class ACPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public ACPropertyChangedEventArgs(string propertyName, IACPropertyNetValueEvent valueEvent)
            : base(propertyName)
        {
            _ValueEvent = valueEvent;
        }

        private IACPropertyNetValueEvent _ValueEvent;
        public IACPropertyNetValueEvent ValueEvent
        {
            get
            {
                return _ValueEvent;
            }
        }
    }


    /// <summary>
    /// ACPropertyChangedEventHandler is invoked twice: Before a broadcast will take place or afterwards.
    /// </summary>
    public enum ACPropertyChangedPhase : short
    {
        /// <summary>
        /// Before Broadcast and Persistance. ValueT of Property has still the old value.
        /// </summary>
        BeforeBroadcast = 0,

        /// <summary>
        /// After Broadcast and Persistance. ValueT of Property has now the new value.
        /// </summary>
        AfterBroadcast = 1
    }


    /// <summary>
    /// Delegate for subscribing ValueUpdatedOnReceival-Event on a IACPropertyNetServer
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ACPropertyChangedEventArgs"/> instance containing the event data.</param>
    /// <param name="phase">The phase.</param>
    public delegate void ACPropertyChangedEventHandler(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase);


    /// <summary>
    /// Signature of the Delegate-Method for the Callback of Script-Setter-Methods
    /// </summary>
    /// <param name="value"></param>
    /// <param name="callingProperty"></param>
    public delegate void ACPropertyNetSetMethodScript(IACPropertyNetValueEvent value, IACPropertyNetBase callingProperty);


    /// <summary>
    /// Interface for Network-capable properties that resides on a "Real-Instance" on server-side. Network-capable properties automatically distribute their values across the network in the event of a change. Network-capable properties offer the possibility of so-called "property binding". The concept of property binding enables you to "connect" a property of component A with another property of component B in order to achieve automatic forwarding of value changes.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACPropertyNetServer'}de{'IACPropertyNetServer'}", Global.ACKinds.TACInterface)]
    public interface IACPropertyNetServer : IACPropertyNetBase
    {
        /// <summary>
        /// Occurs when the Value has changed of a Server-Property. This event is raised twice: Before a broadcast will take place or afterwards.
        /// </summary>
        event ACPropertyChangedEventHandler ValueUpdatedOnReceival;


        /// <summary>
        /// Delegate-Method for the Callback of a Script-Setter-Method
        /// </summary>
        ACPropertyNetSetMethodScript NetSetMethodOfScript { get; set; }


        /// <summary>
        /// Method for changing the ACValueT-Property on Serverside to pass additional individual parameters
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="forceSend"></param>
        /// <param name="invokerInfo">Additional individual parameters</param>
        void ChangeValueServer(object newValue, bool forceSend, object invokerInfo = null);

        bool BackupValue(bool resetAndClear = false);

        bool RestoreBackupedValue();
    }
}
