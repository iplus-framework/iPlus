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
// <copyright file="ACObjectItemList.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACObjectItemList
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACObjectItemList'}de{'ACObjectItemList'}", Global.ACKinds.TACClass)]
    public class ACObjectItemList : ObservableCollection<ACObjectItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectItemList"/> class.
        /// </summary>
        public ACObjectItemList()
        {
        }

        public ACObjectItemList(List<ACObjectItem> iacObjectList):base(iacObjectList)
        {

        }
    }
}
