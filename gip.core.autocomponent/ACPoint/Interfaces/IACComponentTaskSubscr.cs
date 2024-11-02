// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// IACComponentTaskSubscr is a counterpart for the IACComponentTaskExec.
    /// Interface, that defines a standardized mechanism, how workflow-classes must be implemented to be able to invoke asynchronous methods on PAProcessModules.
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACComponent" />
    public interface IACComponentTaskSubscr : IACComponent
    {
        /// <summary>
        /// Asynchronous subscription-point, that stores a List of invocations.
        /// </summary>
        /// <value>
        /// The task subscription point.
        /// </value>
        ACPointAsyncRMISubscr TaskSubscriptionPoint { get; }

        /// <summary>
        /// Returns the pointer to the TaskCallback-Method
        /// </summary>
        /// <value>
        /// The task callback delegate.
        /// </value>
        ACPointNetEventDelegate TaskCallbackDelegate { get; }

        /// <summary>
        /// Callback-Method with the signature of ACPointNetEventDelegate to receive Callbacks from IACComponentTaskExec.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ACEventArgs"/> instance containing the event data.</param>
        /// <param name="wrapObject">The wrap object.</param>
        void TaskCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject);
    }
}
