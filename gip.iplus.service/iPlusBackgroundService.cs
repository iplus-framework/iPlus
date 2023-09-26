using gip.core.autocomponent;
using gip.core.datamodel;
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
        private readonly iPlusService _iPlusService;
        private readonly ILogger<IPlusBackgroundService> _logger;
        
        public IPlusBackgroundService(
            iPlusService iPlusService,
            ILogger<IPlusBackgroundService> logger) =>
            (_iPlusService, _logger) = (iPlusService, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(iPlusService.CurrentDomain_UnhandledException);

                    if (args == null || args.Count() <= 0)
                        args = System.Environment.GetCommandLineArgs();
                    CommandLineHelper cmdHelper = new CommandLineHelper(args);
                    bool WCFOff = args.Contains("/WCFOff");
                    bool simulation = args.Contains("/Simulation");

                    // TODO: Two different Implementaions for Linux or Windows-Platform
                    ACStartUpRoot startUpManager = new ACStartUpRoot(null);
                    // If Linux, then pass null
                    //ACStartUpRoot startUpManager = new ACStartUpRoot(null);

                    String errorMsg = "";
                    // 1. Datenbankverbindung herstellen
                    //await Task.Delay(5000);
                    if (startUpManager.LoginUser(cmdHelper.LoginUser, cmdHelper.LoginPassword, false, false, ref errorMsg, WCFOff, simulation) != 1)
                    {
                        string source = "Vario iPlus Service";
                        if (MyOperatingSystem.isWindows())
                        {
#pragma warning disable CA1416
                            if (!EventLog.SourceExists(source))
                                EventLog.CreateEventSource(source, "Application");
                            EventLog.WriteEntry(source, errorMsg, EventLogEntryType.Information);
#pragma warning restore CA1416
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.

                if (ACRoot.SRoot != null)
                    ACRoot.SRoot.ACDeInit();
                System.Environment.Exit(1);
            }
        }
    }
}
