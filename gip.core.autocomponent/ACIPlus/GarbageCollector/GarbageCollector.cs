// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Threading;

namespace gip.core.autocomponent
{
    public class GarbageCollector : ACDelegateQueue
    {
        #region c'tors
        public GarbageCollector()
            : base("GarbageCollector", 30000)
        {
        }
        #endregion

        #region Properties
        private static GarbageCollector _Instance = null;
        public static GarbageCollector Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GarbageCollector();
                return _Instance;
            }
        }
        #endregion

        #region Methods
        internal void PrepareShutdown()
        {
            _WorkerInterval_ms = 500;
            Add(delegate { int i = 0; i++; });
        }

        protected override bool OnStartQueueProcessing(int countActions)
        {
            //EnterLockDelegateQueue();
            return true;
        }

        protected override void OnQueueProcessed(int countActions)
        {
            //ExitLockDelegateQueue();
        }

        ///// <summary>
        ///// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        ///// </summary>
        //public void EnterLockDelegateQueue()
        //{
        //    ACMonitor.Enter(_LockDelegateQueue);
        //}

        ///// <summary>
        ///// Leaves Critical Section
        ///// </summary>
        //public void ExitLockDelegateQueue()
        //{
        //    ACMonitor.Exit(_LockDelegateQueue);
        //}

        public void AnalyzeAndSweep(ACComponent component)
        {
            if (component == null || !component.IsProxy || component is ApplicationManagerProxy || ACRoot.SRoot.InitState != ACInitState.Initialized)
                return;
            // Component-Manager-Proxies dürfen nicht gestoppt werden
            if (component.ParentACComponent != null && component.ParentACComponent is ACRoot)
                return;
            Add(delegate { FinalizeComponent(component); });
        }

        private void FinalizeComponent(ACComponent component)
        {
            if (component == null || !component.IsProxy)
                return;
            if (component.InitState != ACInitState.Initialized)
                return;
            if (AreComponentAndChildsReferenced(component))
                return;
            component.FinalizeComponent();
        }

        private bool AreComponentAndChildsReferenced(IACComponent component)
        {
            if (component.InitState != ACInitState.Initialized || component.ReferencePoint == null)
                return false;
            if ((component.ReferencePoint as ACPointReference).HasStrongReferences)
                return true;
            foreach (IACComponent child in component.ACComponentChilds)
            {
                bool referenced = AreComponentAndChildsReferenced(child);
                if (referenced)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
