// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    //
    // Summary:
    //     Specifies how objects being loaded into the object context are merged with objects
    //     already in the object context.
    public enum MergeOption
    {
        //
        // Summary:
        //     Objects that do not exist in the object context are attached to the context.
        //     If an object is already in the context, the current and original values of object's
        //     properties in the entry are not overwritten with data source values. The state
        //     of the object's entry and state of properties of the object in the entry do not
        //     change. System.Data.Objects.MergeOption.AppendOnly is the default merge option.
        AppendOnly,
        //
        // Summary:
        //     Objects that do not exist in the object context are attached to the context.
        //     If an object is already in the context, the current and original values of object's
        //     properties in the entry are overwritten with data source values. The state of
        //     the object's entry is set to System.Data.EntityState.Unchanged, no properties
        //     are marked as modified.
        OverwriteChanges,
        //
        // Summary:
        //     Objects that do not exist in the object context are attached to the context.
        PreserveChanges,
        //
        // Summary:
        //     Objects are maintained in a System.Data.EntityState.Detached state and are not
        //     tracked in the System.Data.Objects.ObjectStateManager. However, Entity Framework-generated
        //     entities and POCO entities with proxies maintain a reference to the object context
        //     to facilitate loading of related objects.
        NoTracking
    }
}
