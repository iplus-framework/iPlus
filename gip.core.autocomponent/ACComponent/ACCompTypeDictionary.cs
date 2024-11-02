// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACCompTypeDictionary : Dictionary<Type, List<ACRef<ACComponent>>>
    {
        private object _Lock = new object();
        public IEnumerable<T> GetComponentsOfType<T>(bool findInheritedTypes=false) where T : ACComponent
        {
            lock (_Lock)
            {
                if (!findInheritedTypes)
                {
                    List<ACRef<ACComponent>> refComps;
                    if (TryGetValue(typeof(T), out refComps))
                    {
                        return refComps.ToArray().Select(c => (T)c.ValueT);
                    }
                }
                else
                {
                    Type baseType = typeof(T);
                    var query = this.Where(c => baseType.IsAssignableFrom(c.Key)).SelectMany(c => c.Value).ToArray();
                    if (query != null && query.Any())
                    {
                        return query.Select(c => (T)c.ValueT);
                    }
                }
            }
            return null;
        }

        public IEnumerable<T> GetComponentsOfType<T>(Type derivationsFrom) where T : ACComponent
        {
            lock (_Lock)
            {
                var query = this.Where(c => derivationsFrom.IsAssignableFrom(c.Key)).SelectMany(c => c.Value).ToArray();
                if (query != null && query.Any())
                {
                    return query.Select(c => (T)c.ValueT);
                }
            }
            return null;
        }


        public void AddComponent(ACComponent acComponent)
        {
            if (acComponent == null)
                return;
            Type type = acComponent.GetType();

            lock (_Lock)
            {
                List<ACRef<ACComponent>> refComps;
                if (!TryGetValue(type, out refComps))
                {
                    refComps = new List<ACRef<ACComponent>>();
                    Add(type, refComps);
                }
                ACRef<ACComponent> refComp = new ACRef<ACComponent>(acComponent, false);
                refComps.Add(refComp);
            }
        }

        public void RemoveComponent(ACComponent acComponent)
        {
            if (acComponent == null)
                return;
            Type type = acComponent.GetType();

            lock (_Lock)
            {
                List<ACRef<ACComponent>> refComps;
                if (TryGetValue(type, out refComps))
                {
                    ACRef<ACComponent> refComp = refComps.Where(c => c.ValueT == acComponent).FirstOrDefault();
                    if (refComp != null)
                    {
                        refComp.Detach();
                        refComps.Remove(refComp);
                    }
                }
            }
        }

        public void DetachAll()
        {
            lock (_Lock)
            {
                foreach (List<ACRef<ACComponent>> refComps in this.Values)
                {
                    foreach (ACRef<ACComponent> refComp in refComps)
                    {
                        refComp.Detach();
                    }
                }
            }
        }
    }
}
