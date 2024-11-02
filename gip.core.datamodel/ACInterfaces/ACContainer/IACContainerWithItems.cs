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
// <copyright file="IACContainerWithItems.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>A container that enables to build tree structures. A container is a object that encapsulates a value which is from type IACObject.
    /// It contains a Items-Property for childs and a ParentContainer-Property for referencing the parent container.
    /// Use this interface if you want to create a lightweight tree-based-model for presenting it on the GUI (Instead of using the heavyweight IACComponents).
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACContainer" />
    public interface IACContainerWithItems : IACContainer
    {
        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        IEnumerable<IACContainerWithItems> Items { get; }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        void Add(IACContainerWithItems child);


        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        bool Remove(IACContainerWithItems child);


        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        IACContainerWithItems ParentContainer { get; }


        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        IACContainerWithItems RootContainer { get; }
    }


    /// <summary>A generic container that enables to build tree structures. A container is a object that encapsulates a value which is from type IACObject.
    /// It contains a generic ItemsT-Property for childs and a generic ParentContainerT-Property for referencing the parent container.
    /// Use this interface if you want to create a lightweight tree-based-model for presenting it on the GUI (Instead of using the heavyweight IACComponents).</summary>
    /// <typeparam name="T">Generic type of the container</typeparam>
    /// <typeparam name="S">Generic type for ValueT-Property</typeparam>
    /// <seealso cref="gip.core.datamodel.IACContainerWithItems" />
    /// <seealso cref="gip.core.datamodel.IACContainerT{S}" />
    /// <seealso cref="gip.core.datamodel.IVBIsVisible" />
    public interface IACContainerWithItemsT<T, S> : IACContainerWithItems, IACContainerT<S>, IVBIsVisible where T : IACContainerWithItems
    {
        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        IEnumerable<T> ItemsT { get; }


        /// <summary>Visible Container-Childs</summary>
        /// <value>Visible Container-Childs</value>
        IEnumerable<T> VisibleItemsT { get; }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        void Add(T child);


        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        bool Remove(T child);


        /// <summary>Gets the parent container T.</summary>
        /// <value>The parent container T.</value>
        T ParentContainerT { get; }


        /// <summary>Gets the root container T.</summary>
        /// <value>The root container T.</value>
        T RootContainerT { get; }
    }
}
