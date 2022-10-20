// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACClassDesignProvider.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface for objects or components, that can provide a ACClassDesign, that should be used for presenting itsself on the gui
    /// </summary>
    public interface IACClassDesignProvider : IACObject
    {
        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>
        ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "");
    }
}
