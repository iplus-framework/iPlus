// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACPointStorableBase.cs" company="gip mbh, Oftersheim, Germany">
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
    /// <summary>
    /// Interface for managing references to a ACComponent
    /// </summary>
    /// <typeparam name="T">In most cases T is IACObject and contains ACRef-Instances or internal WPFReferencesToComp-Classes</typeparam>
    public interface IACPointReference<T> : IACPoint<T> where T : IACObject 
    {
        #region Methods for ACRefs to IACComponents/IACObjects
        /// <summary>
        /// Contains a reference?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Contains(T item);

        /// <summary>
        /// Adds a reference
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        void Add(T item);

        /// <summary>
        /// Removes a reference
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Remove(T item);

        /// <summary>
        /// Broadcast a message to all references
        /// </summary>
        /// <param name="acURL"></param>
        /// <param name="acParameter"></param>
        void Broadcast(string acURL, object[] acParameter);
        #endregion

        #region Methods Refs to WPF-Objects
        /// <summary>
        /// WPF-Control that register itself and the bounded object (in most cases a ACComponentProxy-Object) to this Reference-Point
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="boundedObject">ACComponent which is bound via WPF-Binding to the WPF-Control</param>
        void AddWPFRef(int hashOfDepObj, T boundedObject);

        /// <summary>
        /// WPF-Control that removes itself
        /// </summary>
        /// <param name="hashOfDepObj">ashcode of the calling WPF-Control</param>
        /// <returns></returns>
        bool RemoveWPFRef(int hashOfDepObj);

        /// <summary>
        /// Is any WPF-Control bound to this component?
        /// </summary>
        /// <param name="boundedComponent">Component which ist bound via WPF-Binding to a WPFControl</param>
        /// <returns></returns>
        bool HasWPFRefsForComp(IACComponent boundedComponent);

        void CheckWPFRefsAndDetachUnusedProxies();
        #endregion


        #region Properties
        /// <summary>
        /// A strong reference is a normal reference that protects the referred object from collection by the IPlus-GarbageCollector.
        /// Returns if there are any strong references. (Weak references are excluded)
        /// </summary>
        bool HasStrongReferences { get; }

        /// <summary>
        /// A strong reference is a normal reference that protects the referred object from collection by the IPlus-GarbageCollector.
        /// Returns the count of strong references. (Weak references are excluded)
        /// </summary>
        int CountStrongReferences { get; }
        #endregion
    }
}
