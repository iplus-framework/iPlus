// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using gip.core.datamodel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ApplicationManagerProxy'}de{'ApplicationManagerProxy'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.NotStorable, true, false)]
    public class ApplicationManagerProxy : ACComponentProxy, IAppManager
    {
        #region c´tors
        public ApplicationManagerProxy(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ACUrlRoutingService = new ACPropertyConfigValue<string>(this, "ACUrlRoutingService", ACRoutingService.DefaultServiceACUrl);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _ApplicationQueue = new ACDelegateQueue(this.GetACUrl() + ";AppQueue");
                _ApplicationQueue.StartWorkerThread();
            }

            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool result = await base.ACDeInit(deleteACClassTask);
            if (_ApplicationQueue != null)
            {
                _ApplicationQueue.StopWorkerThread();
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ApplicationQueue = null;
                }
            }
            return result;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Overriden: Returns the Database-Context for Application-Managers (RootDbOpQueue.AppContext)
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                return RootDbOpQueue.AppContext;
            }
        }

        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        private ACDelegateQueue _ApplicationQueue;
        public ACDelegateQueue ApplicationQueue
        {
            get
            {
                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _ApplicationQueue;
                }
            }
        }

        #endregion

        #region Config
        private ACPropertyConfigValue<string> _ACUrlRoutingService;
        [ACPropertyConfig("en{'Routing service ACUrl'}de{'Routing service ACUrl'}")]
        public virtual string ACUrlRoutingService
        {
            get
            {
                return _ACUrlRoutingService.ValueT;
            }
        }

        public string RoutingServiceACUrl { get { return ACUrlRoutingService; } }

        #endregion

        #region methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                //case gip.core.autocomponent.ApplicationManager.MN_FindMatchingUrls:
                //    result = FindMatchingUrls(acParameter[0] as FindMatchingUrlsParam);
                //    return true;
                case gip.core.autocomponent.ApplicationManager.MN_GetACComponentACMemberValues:
                    result = GetACComponentACMemberValues(acParameter[0] as Dictionary<string, string>);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        //public string[] FindMatchingUrls(FindMatchingUrlsParam queryParam)
        //{
        //    if (queryParam == null)
        //        return null;
        //    byte[] data;
        //    using (var stream = new MemoryStream())
        //    {
        //        BinaryFormatter bf = new BinaryFormatter();
        //        bf.Serialize(stream, queryParam);
        //        stream.Flush();
        //        data = stream.ToArray();
        //    }
        //    return RMInvoker.ExecuteMethod(gip.core.autocomponent.ApplicationManager.MN_FindMatchingUrlsSerial, new Object[] { data }) as string[];
        //}

        public Dictionary<string, object> GetACComponentACMemberValues(Dictionary<string, string> acUrl_AcMemberIdentifiers)
        {
            return RMInvoker.ExecuteMethod(gip.core.autocomponent.ApplicationManager.MN_GetACComponentACMemberValues, new Object[] { acUrl_AcMemberIdentifiers }) as Dictionary<string, object>;
        }

        #endregion
    }
}
