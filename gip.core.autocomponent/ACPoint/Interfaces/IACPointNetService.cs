// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACPointNetService<T, W>
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
        /// <summary>
        /// From-Points are Points which act's as a kind of Server-Points, where To-Points can connect with it
        /// If From-Points are Proxies, then all Entries in the Connection-List must be stored and 
        /// sended to the Real-Object on Server side. So all Connections in the local memory are Stored in this 
        /// List "ConnectionListLocal". 
        /// In contrast: the ConnectionList of From-Points is a merged List of all ConnectionListLocal-List 
        /// over all exisiting Proxy-Instances. The ConnectionList contains only Elements which ValidationState is 
        /// declared as "Valid" by the Server-Object.
        /// First, when new Entries on Proxy-Side are done, the Elements has a ValidationState as "NewEntry".
        /// </summary>
        IEnumerable<W> ConnectionListLocal { get; }

        /// <summary>
        /// List of unwrapped Objects in ConnectionListLocal
        /// </summary>
        IEnumerable<T> RefObjectListLocal { get; }

        /// <summary>
        /// Reference to Extension-Class, which implements this interface (workaround for multiple inheritance)
        /// </summary>
        ACPointServiceBase<T, W> ServicePointHelper { get; }

        void InvokeSetMethod(IACPointNetBase point);
    }
}

