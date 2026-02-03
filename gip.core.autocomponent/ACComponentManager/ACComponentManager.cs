// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Basisklasse zur Verwaltung untergeordneter ACComponent-Instanzen
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Objectmanager'}de{'Objektmanager'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class ACComponentManager : PAClassAlarmingBase
    {
        #region c´tors

        public ACComponentManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }

        protected static IACEntityObjectContext _CommonManagerContext;
        /// <summary>
        /// Returns a seperate and shared Database-Context "StaticACComponentManager".
        /// Because Businessobjects also inherit from this class all BSO's get this shared database context.
        /// If some custom BSO's needs its own context, then they have to override this property.
        /// Application-Managers that also inherit this class should override this property an use their own context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_CommonManagerContext == null)
                    _CommonManagerContext = ACObjectContextManager.GetOrCreateContext<Database>("StaticACComponentManager");
                return _CommonManagerContext;
            }
        }
        #endregion
    }
}
