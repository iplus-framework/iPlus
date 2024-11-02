// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gip.core.tcAgent
{
    partial class ServiceController : ServiceBase
    {
        public ServiceController()
        {
            InitializeComponent();
        }

        AdsVariobatchAgent _adsVariobatchAgent;

        protected override void OnStart(string[] args)
        {
            _adsVariobatchAgent = new AdsVariobatchAgent();
        }

        protected override void OnStop()
        {
            if (_adsVariobatchAgent != null)
                _adsVariobatchAgent.StopAdsAgent();
        }

        //[Conditional("DEBUG_SERVICE")]
        //private static void DebugMode()
        //{
        //    Debugger.Break();
        //}
    }
}
