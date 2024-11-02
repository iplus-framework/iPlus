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
    /// Interface, that defines a standardized mechanism, how workflow-classes has to invoke asynchronous methods on PAProcessModules.
    /// Each asynchronous method is represented through a PAProcessFunction, that is configured as a child under a PAProcessModule.
    /// PAProcessFunctions are started by passing a ACMethod (virtual method) to its Start()-Method. 
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACComponent" />
    public interface IACComponentTaskExec : IACComponent
    {
        /// <summary>
        /// Asynchronous Point, that stores a List of invocations for its PAProcessFunction-Childs.
        /// Workflow-Classes must access this property and add a Task to start the Processfunction.
        /// </summary>
        /// <value>
        /// The task invocation point.
        /// </value>
        ACPointTask TaskInvocationPoint { get; }


        bool ActivateTask(ACMethod acMethod, bool executeMethod, IACComponent executingInstance = null);

        bool CallbackTask(ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted);

        bool CallbackTask(IACTask task, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted);

        IACTask GetTaskOfACMethod(ACMethod acMethod);

        bool CallbackCurrentTask(ACMethodEventArgs result);
    }
}
