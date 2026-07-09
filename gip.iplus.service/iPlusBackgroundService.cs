// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace gip.iplus.service
{
    internal class IPlusBackgroundService : BackgroundService
    {
        private readonly CommandLineArgs _cmdLineArgs;
        private readonly ILogger<IPlusBackgroundService> _logger;

        public IPlusBackgroundService(ILogger<IPlusBackgroundService> logger, CommandLineArgs cmdArgs)
        {
            _logger = logger;
            _cmdLineArgs = cmdArgs;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            string[] args = _cmdLineArgs.Args;
            try
            {
                AppDomain.CurrentDomain.UnhandledException += iPlusService.CurrentDomain_UnhandledException;

                if (args == null || args.Count() <= 0)
                    args = new string[] { "/U00", "/P00" };
                CommandLineHelper cmdHelper = new CommandLineHelper(args);
                bool wcfOff = args.Contains("/WCFOff");
                bool simulation = args.Contains("/Simulation");
                bool waitToAttachDebugger = args.Contains("/Debug");

                ACStartUpRoot startUpManager = new ACStartUpRoot(null);
                string errorMsg = "";
                _logger.LogInformation("Starting iPlus Service with user {LoginUser}", cmdHelper.LoginUser);

                if (waitToAttachDebugger)
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }

                if (startUpManager.LoginUser(cmdHelper.LoginUser, cmdHelper.LoginPassword, false, false, ref errorMsg, wcfOff, simulation) != 1)
                {
                    _logger.LogError("Starting iPlus Service failed: {Error}", errorMsg);
                    throw new InvalidOperationException($"iPlus service startup failed: {errorMsg}");
                }

                _logger.LogInformation("iPlus Service started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }

            await base.StartAsync(cancellationToken);
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
            AppDomain.CurrentDomain.UnhandledException -= iPlusService.CurrentDomain_UnhandledException;

            if (ACRoot.SRoot != null)
                ACRoot.SRoot.ACDeInit();

            return base.StopAsync(cancellationToken);
        }
    }
}
