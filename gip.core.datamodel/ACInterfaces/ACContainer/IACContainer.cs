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
// <copyright file="IACContainer.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// A Container is a object that encapsulates a value which is from type IACObject.
    /// It's also possible to store a value that is a not an IACObject. 
    /// In this case the Property ValueTypeACClass is null, because it's an unkown type for iPlus.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACContainer'}de{'IACContainer'}", Global.ACKinds.TACInterface)]
    public interface IACContainer : IACObjectBase
    {
        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        object Value { get; set; }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        ACClass ValueTypeACClass { get; }
    }
}
