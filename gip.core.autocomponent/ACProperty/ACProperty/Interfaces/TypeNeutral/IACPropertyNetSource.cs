using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Interface for Network-capable properties that resides on a "Real-Instance" on server-side. Network-capable properties automatically distribute their values across the network in the event of a change. Network-capable properties offer the possibility of so-called "property binding". The concept of property binding enables you to "connect" a property of component A with another property of component B in order to achieve automatic forwarding of value changes.
    /// A "Source-Property" is a network capable property, that contains or holds a original value. "Target-Properties" can have a binding to this "Source-Property" to receive the value.
    /// </summary>
    public interface IACPropertyNetSource : IACPropertyNetServer
    {
        /// <summary>
        /// Returns a List of "Target-Properties" that have bound it's value to this "Source-Property".
        /// NOT THREAD-SAFE! Use _20035_LockTargets to query the list!
        /// </summary>
        List<IACPropertyNetTarget> Targets { get; }


        /// <summary>
        /// Returns a List of other objects that refer this "Source-Property". e.g. OPC-Items
        /// NOT THREAD-SAFE! Use _20035_LockTargets to query the list!
        /// </summary>
        List<object> AdditionalRefs { get; }


        bool WasInitializedFromInvoker { get; }
    }
}
