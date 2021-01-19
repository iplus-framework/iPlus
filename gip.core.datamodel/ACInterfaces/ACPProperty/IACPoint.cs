// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACPoint.cs" company="gip mbh, Oftersheim, Germany">
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
    /// An IACPoint is used to abstractly describe the relationships between any object. It is called a Connectonpoint because in graph theory objects are represented as points and relationships as lines. Using this abstract interface, IPlus is able to graphically represent any relationships between objects (e.g. workflows, visualizations, routes...). The relationships are stored in the IEnumerable&lt;T&gt; ConnectionList property.
    /// </summary>
    /// <typeparam name="T">Type of objects that are stored in the ConnectionList-Property</typeparam>
    /// <seealso cref="gip.core.datamodel.IACPointBase" />
    public interface IACPoint<T> : IACPointBase where T : IACObject 
    {
        /// <summary>
        /// List of relations to other objects.
        /// It's use to describe the relationships to other objects.
        /// </summary>
        IEnumerable<T> ConnectionList { get; }

        /// <summary>
        /// A readable string that returns informations about the relationships
        /// </summary>
        string ConnectionListInfo { get; }
    }
}
