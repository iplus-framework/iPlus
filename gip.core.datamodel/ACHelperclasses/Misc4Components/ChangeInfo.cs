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
// <copyright file="ChangeInfo.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ChangeInfo
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ChangeInfo'}de{'ChangeInfo'}", Global.ACKinds.TACClass)]
    public class ChangeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeInfo"/> class.
        /// </summary>
        /// <param name="parentObject">The parent object.</param>
        /// <param name="changedObject">The changed object.</param>
        /// <param name="changeCmd">The change CMD.</param>
        public ChangeInfo(IACObject parentObject, IACObject changedObject, string changeCmd)
        {
            ParentObject = parentObject;
            ChangedObject = changedObject;
            ChangeCmd = changeCmd;
        }
        /// <summary>
        /// Gets or sets the parent object.
        /// </summary>
        /// <value>The parent object.</value>
        public IACObject ParentObject { get; set; }
        /// <summary>
        /// Gets or sets the changed object.
        /// </summary>
        /// <value>The changed object.</value>
        public IACObject ChangedObject { get; set; }
        /// <summary>
        /// Gets or sets the change CMD.
        /// </summary>
        /// <value>The change CMD.</value>
        public string ChangeCmd { get; set; }
    }
}
