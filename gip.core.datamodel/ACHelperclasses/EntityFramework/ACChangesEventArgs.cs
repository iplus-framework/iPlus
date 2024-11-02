// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 11-07-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-26-2012
// ***********************************************************************
// <copyright file="ACMemberIndexer.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Extended EventArgs that are raised when ACSaveChanges is called on a EF-Database-Context.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ACChangesEventArgs : EventArgs
    {
        public enum ACChangesType
        {
            ACSaveChanges,
            ACUndoChanges
        }

        public ACChangesEventArgs(ACChangesType type, bool succeeded)
        {
            ChangeType = type;
            Succeeded = succeeded;
        }

        public ACChangesType ChangeType
        {
            get;
            protected set;
        }

        public bool Succeeded
        {
            get;
            protected set;
        }
    }

    public delegate void ACChangesEventHandler(object sender, ACChangesEventArgs e);
}
