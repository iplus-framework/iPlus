﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IVBConnector.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface IVBConnector
    /// </summary>
    public interface IVBConnector : IVBContent
    {
        //IACObject ParentACObject { get; }

        /// <summary>
        /// Gets the parent VB control.
        /// </summary>
        /// <value>The parent VB control.</value>
        IVBContent ParentVBControl { get; }
    }
}
