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
// <copyright file="IACAttach.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for objects that can be serialized and broadcasted.
    /// If the the encapuslated objects after deserializing must be attached to a parent context (e.g. Entity-Framework Database-Context), then this interface should be used.
    /// </summary>
    public interface IACAttach
    {
        /// <summary>Attaches the deserialized encapuslated objects to the parent context.</summary>
        /// <param name="parentACObject">The parent context. Normally this is a EF-Context (IACEntityObjectContext).</param>
        void AttachTo(IACObject parentACObject);


        /// <summary>Detaches the encapuslated objects from the parent context.</summary>
        /// <param name="detachFromContext">If attached object is a Entity object, then it will be detached from Change-Tracking if this parameter is set to true.</param>
        void Detach(bool detachFromContext = false);

        /// <summary>Gets a value indicating whether the encapuslated objects are attached.</summary>
        /// <value>
        ///   <c>true</c> if the encapuslated objects are attached; otherwise, <c>false</c>.</value>
        bool IsAttached { get; }

        #region Events
        /// <summary>
        /// Occurs when encapuslated objects were detached.
        /// </summary>
        event EventHandler ObjectDetached;

        /// <summary>
        /// Occurs before the deserialized content will be attached to be able to access the encapuslated objects later.
        /// </summary>
        event EventHandler ObjectDetaching;

        /// <summary>
        /// Occurs when encapuslated objects were attached.
        /// </summary>
        event EventHandler ObjectAttached;
        #endregion


    }
}
