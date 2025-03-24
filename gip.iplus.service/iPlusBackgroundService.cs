// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.iplus.service
{
    internal class IPlusBackgroundService : BackgroundService
    {
        private readonly CommandLineArgs _CmdLineArgs;
        private readonly ILogger<IPlusBackgroundService> _logger;

        public IPlusBackgroundService(ILogger<IPlusBackgroundService> logger, CommandLineArgs cmdArgs)
        {
            _logger = logger;
            _CmdLineArgs = cmdArgs;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            string[] args = _CmdLineArgs.Args;
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(iPlusService.CurrentDomain_UnhandledException);

                if (args == null || args.Count() <= 0)
                    args = new string[] { "/U00", "/P00" };
                CommandLineHelper cmdHelper = new CommandLineHelper(args);
                bool WCFOff = args.Contains("/WCFOff");
                bool simulation = args.Contains("/Simulation");

                ACStartUpRoot startUpManager = new ACStartUpRoot(null);
                String errorMsg = "";
                _logger.LogInformation(String.Format("Starting iPlus Service with user {0}", cmdHelper.LoginUser));
                if (startUpManager.LoginUser(cmdHelper.LoginUser, cmdHelper.LoginPassword, false, false, ref errorMsg, WCFOff, simulation) != 1)
                    _logger.LogInformation(String.Format("Starting iPlus-Service failed {0}", errorMsg));
                _logger.LogInformation("iPlus Service started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex, ex.StackTrace);
            }
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (ACRoot.SRoot != null)
                ACRoot.SRoot.ACDeInit();
            return base.StopAsync(cancellationToken);
        }
    }
}
