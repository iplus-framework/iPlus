// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using gip.iplus.service;
using Microsoft.Extensions.Logging;

namespace gip.iplus.service
{
    public class CommandLineArgs
    {
        public string[] Args { get; set; }
    }

    static class Program
    {
        public static void Main(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            if (MyOperatingSystem.isWindows())
            {
                hostBuilder.UseWindowsService();
            }
            else if (MyOperatingSystem.isLinux())
            {
                hostBuilder.UseSystemd();
            }
            hostBuilder.ConfigureServices(services => 
                { 
                    services.AddSingleton(new CommandLineArgs { Args = args }); 
                    services.AddHostedService<IPlusBackgroundService>(); 
                });

            IHost host = hostBuilder.Build();
            host.Run();
        }
    }
}