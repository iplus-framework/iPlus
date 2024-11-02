// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    internal class DerivationCache : Dictionary<Guid, DerivationDict>
    {

        public object _DerivationCacheLockObject = new object();

        public bool? IsDerived(Guid baseClassID, Guid derivedClassID)
        {
            DerivationDict result;
            lock (_DerivationCacheLockObject)
            {
                if (TryGetValue(baseClassID, out result))
                {
                    return result.IsDerived(derivedClassID);
                }
                else
                    return null;
            }
        }

        public void RegisterDerivedClass(Guid baseClassID, Guid derivedClassID, bool isDerived)
        {
            lock(_DerivationCacheLockObject)
            {
                DerivationDict result;
                if(!TryGetValue(baseClassID, out result))
                {
                    result = new DerivationDict();
                    Add(baseClassID, result);
                }
                result.RegisterDerivedClass(derivedClassID, isDerived);
            }
        }
    }

    internal class DerivationDict : Dictionary<Guid, bool>
    {
        public bool? IsDerived(Guid derivedClassID)
        {
            bool result;
            if(TryGetValue(derivedClassID, out result))
            {
                return result;
            }
            return null;
        }

        public void RegisterDerivedClass(Guid derivedClassID, bool isDerived)
        {
            bool result;
            if(!TryGetValue(derivedClassID, out result))
            {
                Add(derivedClassID, isDerived);
            }
        }
    }
}
