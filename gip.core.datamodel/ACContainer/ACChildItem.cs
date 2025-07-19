// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACChildItem'}de{'ACChildItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACChildItem<T> where T : IACComponent
    {
        public ACChildItem(IACComponent component, string instanceName)
        {
            ParentComponent = component;
            InstanceName = instanceName;
        }

        #region properties

        [ACPropertyInfo(9999)]
        public string InstanceName { get; set; }

        public IACComponent ParentComponent { get; set; }

        private T value;
        [ACPropertyInfo(9999)]
        public T Value
        {
            get
            {
                if (value == null)
                {
                    IACComponent item = ParentComponent.FindChildComponents<T>((IACComponent c) => c.ACIdentifier.StartsWith(InstanceName)).FirstOrDefault();
                    if (item == null)
                        item = ParentComponent.ACUrlCommand("?" + InstanceName) as IACComponent;
                    if (item == null)
                        item = ParentComponent.StartComponent(InstanceName, null, new object[] { }) as IACComponent;
                    if (item != null)
                        value = (T)item;
                }
                return value;
            }
        }
        #endregion
    }
}
