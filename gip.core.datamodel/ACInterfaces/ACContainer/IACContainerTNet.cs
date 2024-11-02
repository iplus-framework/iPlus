// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    /// <summary>Interface for implementing network capable properties which values (IACContainer&lt;T&gt;.ValueT) will be broadcasted over the network.</summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <seealso cref="gip.core.datamodel.IACContainerT{T}" />
    /// <seealso cref="gip.core.datamodel.IACPropertyNetBase" />
    public interface IACContainerTNet<T> : IACContainerT<T>, IACPropertyNetBase
    {
        /// <summary>The "Unrequested" value is the last valid value sent from the real object to the proxy object. If a value change of the ValueT property has occurred, the "Unrequested" value remains unchanged until the value change in real object has occurred and the new accepted value has been returned to the proxy object over the network.</summary>
        /// <value>The last valid value on server-side (real object)</value>
        T ValueTUnrequested { get; }

        /// <summary>A change request is started whenever the ValueT-Property was changed. The change request will be sent to the server (real object) and the reponse send back. If the server refused the chsnge request the message-text can be read here.</summary>
        /// <value>Messagetext if change request was refused on server-side.</value>
        string Message { get; }
    }
}
