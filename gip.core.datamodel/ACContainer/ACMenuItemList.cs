// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMenuItemList.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    //[DataContract]
    /// <summary>
    /// Class ACMenuItemList
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMenuItemList'}de{'ACMenuItemList'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMenuItemList : List<ACMenuItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACMenuItemList"/> class.
        /// </summary>
        public ACMenuItemList()
        {
        }
    }
}
