using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace gip.iplus.service
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// 
        /// 1.) DIENST-REGISTRIERUNG
        /// -----------------------------
        /// Den Dienst registriert man mit der Anwendung InstallUtil.exe
        /// Die InstallUtil.exe befindet sich im Verzeichnis der .NET-Frameworkinstallation
        /// z.B. C:\Windows\Microsoft.NET\Framework\v4.0.30319
        /// 
        /// So erfolgt der Aufruf:
        /// C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\gip.variobatch.service.exe
        /// 
        /// Um den Dienst wieder zu deinstallieren muss /U als Option angegegen werden
        /// C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil /U E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\gip.variobatch.service.exe
        /// 
        /// ACHTUNG: Die Eingabeaufforderung muss mit Administrationsrechten ausgeführt werden!!!
        /// 
        /// 2.) BENUTZER UND PASSWORT
        /// ------------------------------
        /// Damit der Dienst mit dem richtigen iPlus-Benutzer hochfährt muss mit regedit (Registry Editor) die Start-Parameter mit angegeben werden.
        /// Unter dem Schlüssel [HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\iPlus]
        /// den Parameter ImagePath mit /U und /P erweitern.
        /// Beispiel: Will man den User 00 mit Passwort 00 verwenden, dann heißt der String im ImagePath:
        /// "E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\gip.variobatch.service.exe" /U00 /P00
        /// 
        /// 3.) CONFIG-DATEI
        /// -----------------------------
        /// Wenn der Dienst gestartet wird, dann sind die Umgebungsvariablen nicht gesetzt in der die Anwendung läuft.
        /// Daher müssen alle Pfade in der Config-Datei absolut sein. 
        /// z.B. beim Connection-String muss der Pfad zu den Metadateien .msl, .csdl, ssdl, auch absolut sein:
        /// <add name="iPlusV4_Entities" connectionString="metadata=E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\VarioBatch.csdl|E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\VarioBatch.ssdl|E:\Devel\VarioBatch2008\VarioiplusV3\bin\Debug\VarioBatch.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=SRV-SQL;Initial Catalog=JoseraBDEV3;Persist Security Info=True;User ID=gip;Password=netspirit;MultipleActiveResultSets=True&quot;" providerName="System.Data.EntityClient" />
        /// 
        /// </summary>
        static void Main()
        {
   //         ServiceBase[] ServicesToRun;
   //         ServicesToRun = new ServiceBase[] 
			//{ 
			//	new Service1() 
			//};
   //         ServiceBase.Run(ServicesToRun);

            using IHost host = Host.CreateDefaultBuilder()
            .UseWindowsService(options =>
            {
                options.ServiceName = ".iPlus Service";
            })
            .ConfigureServices(services =>
            {
                LoggerProviderOptions.RegisterProviderOptions<
                    EventLogSettings, EventLogLoggerProvider>(services);

                services.AddSingleton<Service1>();
                services.AddHostedService<IPlusBackgroundService>();
            })
            .ConfigureLogging((context, logging) =>
            {
                // See: https://github.com/dotnet/runtime/issues/47303
                logging.AddConfiguration(
                    context.Configuration.GetSection("Logging"));
            })
            .Build();

            //await host.RunAsync();
        }
    }
}
