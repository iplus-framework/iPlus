// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="IACTask.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Enum PointProcessingState
    /// </summary>
    public enum PointProcessingState : short
    {
        /// <summary>
        /// The new entry
        /// </summary>
        NewEntry = 1,
        /// <summary>
        /// The accepted
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// The rejected
        /// </summary>
        Rejected = 3,
        /// <summary>
        /// The deleted
        /// </summary>
        Deleted = 4, // Completed
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACPointEntry'}de{'IACPointEntry'}", Global.ACKinds.TACInterface)]
    public interface IACPointEntry : IACObject
    {
        /// <summary>
        /// Gets the request ID.
        /// </summary>
        /// <value>The request ID.</value>
        [ACPropertyInfo(9999)]
        Guid RequestID { get; }

        /// <summary>
        /// Gets the sequence no.
        /// </summary>
        /// <value>The sequence no.</value>
        [ACPropertyInfo(1, "", "en{'SequenceNo'}de{'Sequenz-Nr.'}")]
        ulong SequenceNo { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        PointProcessingState State { get; }
    }

    /// <summary>
    /// Interface IACTask
    /// </summary>
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACTask", "en{'ACPointAsyncRMIWrap'}de{'ACPointAsyncRMIWrap'}", typeof(IACTask), "ACPointAsyncRMIWrap", "ACIdentifier,ACCaption", Const.ACIdentifierPrefix)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACTask'}de{'IACTask'}", Global.ACKinds.TACInterface)]
    public interface IACTask : IACPointEntry
    {
        /// <summary>
        /// Gets the workflow context.
        /// </summary>
        /// <value>The workflow context.</value>
        [ACPropertyInfo(9999)]
        IACWorkflowContext WorkflowContext { get; }

        /// <summary>
        /// Gets a value indicating whether [auto remove].
        /// </summary>
        /// <value><c>true</c> if [auto remove]; otherwise, <c>false</c>.</value>
        bool AutoRemove { get; }

        /// <summary>
        /// Gets the AC method.
        /// </summary>
        /// <value>The AC method.</value>
        ACMethod ACMethod { get; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        ACValueList Parameter { get; }

        /// <summary>
        /// Gets a value indicating whether [callback is pending].
        /// </summary>
        /// <value><c>true</c> if [callback is pending]; otherwise, <c>false</c>.</value>
        bool CallbackIsPending { get; }

        /// <summary>
        /// Gets the in process.
        /// </summary>
        /// <value>The in process.</value>
        [ACPropertyInfo(2, "", "en{'InProcess'}de{'Aktiv'}")]
        Boolean InProcess { get; }

        /// <summary>
        /// Gets the executing instance.
        /// </summary>
        /// <value>The executing instance.</value>
        [ACPropertyInfo(9999)]
        ACRef<IACComponent> ExecutingInstance { get; }
    }
}

