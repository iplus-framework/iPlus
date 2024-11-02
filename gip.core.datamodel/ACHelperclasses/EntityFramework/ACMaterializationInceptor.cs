// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{
    public class ACMaterializationInterceptor : IMaterializationInterceptor
    {
        public ACMaterializationInterceptor()
        {
        }

        public InterceptionResult<object> CreatingInstance(
        MaterializationInterceptionData materializationData,
        InterceptionResult<object> result)
        {
            return result;
        }

        public object CreatedInstance(MaterializationInterceptionData materializationData, object entity)
        {
            VBEntityObject entityObject = entity as VBEntityObject;
            if (entityObject == null)
                return entity;
            entityObject.OnObjectMaterialized(materializationData.Context as IACEntityObjectContext);
            return entityObject;
        }

        public InterceptionResult InitializingInstance(
            MaterializationInterceptionData materializationData,
            object entity,
            InterceptionResult result)
        {
            return result;
        }

        public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
        {
            return entity;
        }
    }
}
