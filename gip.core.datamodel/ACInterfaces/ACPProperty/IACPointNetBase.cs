// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACPointNet.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace gip.core.datamodel
{
    #region Events and Delegates
    /// <summary>
    /// Signatur der Setter-Delegat-Methode in Assembly-ACComponent(für den Callback)
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>FALSE:
    /// Signals, that Standard-Implementation in Point should takes effect
    /// e.g. Auto-Dequeueing the List
    /// TRUE:
    /// Signals, that Component has handled</returns>
    public delegate bool ACPointSetMethod(IACPointNetBase point);
    #endregion

    /// <summary>
    /// Interface IACPointNetBase
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACPointNetBase'}de{'IACPointNetBase'}",  Global.ACKinds.TACInterface)]
    public interface IACPointNetBase : IACPointBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether [point changed for broadcast].
        /// </summary>
        /// <value><c>true</c> if [point changed for broadcast]; otherwise, <c>false</c>.</value>
        bool PointChangedForBroadcast { get; set; }

        /// <summary>
        /// If set, then Method-Invocations on Points are called synchronous.
        /// The invoking Thread is blocked until the answer from the server comes back.
        /// </summary>
        /// <value><c>true</c> if [synchronous mode]; otherwise, <c>false</c>.</value>
        bool SynchronousMode { get; set; }

        /// <summary>
        /// Methodenaufruf, der dem Server-Objekt signalisiert, dass dieser Wert auf Client-Seite/Netzwerk
        /// abboniert wird. Muss bei Client-Points aufgerufen werden. Für Service-Points wird dies indirekt ausgelöst,
        /// indem in die Local-Storage-List ein Wert eingetragen ist.
        /// </summary>
        void Subscribe(bool force = true);

        /// <summary>
        /// Used from Framework
        /// </summary>
        void ReSubscribe();

        /// <summary>
        /// Used from Framework
        /// </summary>
        void UnSubscribe();

        /// <summary>
        /// Method, which rebuilds Object after Deserialization.
        /// If called on Server-Side, Sequence-Numer on Wrap-Objects is set
        /// </summary>
        /// <param name="parentSubscrObject">If Deserialization on Server-Side, ACPSubscriptionACObject ist set</param>
        void RebuildAfterDeserialization(object parentSubscrObject);

        /// <summary>
        /// Called from Framework when changed Point arrives from remote Side
        /// </summary>
        /// <param name="receivedPoint">The received point.</param>
        void OnPointReceivedRemote(IACPointNetBase receivedPoint);

        /// <summary>
        /// Setter-Delegat-Methode von Assembly-ACObjekten (Callback)
        /// </summary>
        /// <value>The set method.</value>
        ACPointSetMethod SetMethod { get; set; }

        /// <summary>
        /// Copies the data of wrap object.
        /// </summary>
        /// <param name="cloneOrOriginal">The clone or original.</param>
        void CopyDataOfWrapObject(object cloneOrOriginal);

        /// <summary>
        /// Gets a value indicating whether this instance is persistable.
        /// </summary>
        /// <value><c>true</c> if this instance is persistable; otherwise, <c>false</c>.</value>
        bool IsPersistable { get; }

        /// <summary>
        /// Determines whether [contains] [the specified clone or original].
        /// </summary>
        /// <param name="cloneOrOriginal">The clone or original.</param>
        /// <returns><c>true</c> if [contains] [the specified clone or original]; otherwise, <c>false</c>.</returns>
        bool Contains(object cloneOrOriginal);
        /// <summary>
        /// Removes the specified clone or original.
        /// </summary>
        /// <param name="cloneOrOriginal">The clone or original.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool Remove(object cloneOrOriginal);

        /// <summary>
        /// Occurs when [all entries removed].
        /// </summary>
        event EventHandler AllEntriesRemoved;
    }
}
