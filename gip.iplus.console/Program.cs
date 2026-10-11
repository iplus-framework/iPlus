using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace gip.iplus.console
{
    public class Program
    {

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            CommandLineHelper cmdHelper = new CommandLineHelper(args);
            bool WCFOff = args.Contains("/WCFOff");
            bool simulation = args.Contains("/Simulation");

            // Default credentials for automated/agent sessions (overridable via /U and /P)
            if (String.IsNullOrEmpty(cmdHelper.LoginUser))
                cmdHelper.LoginUser = "00";
            if (String.IsNullOrEmpty(cmdHelper.LoginPassword))
                cmdHelper.LoginPassword = "00";

            ACStartUpRoot startUpManager = new ACStartUpRoot(null);
            String errorMsg = "";

            if (startUpManager.LoginUser(cmdHelper.LoginUser, cmdHelper.LoginPassword, false, false, ref errorMsg, WCFOff, simulation) != 1)
            {
                Messages.ConsoleMsg("System", "Login Error.");
                Messages.ConsoleMsg("System", errorMsg);
                if (!Console.IsInputRedirected)
                    Console.ReadLine();
                return;
            }

            Messages.ConsoleMsg("System", "Service is ready for requests.  Press any key (or Ctrl+C) to close service.");
            Console.WriteLine();

            WaitForShutdown();

            Messages.ConsoleMsg("System", "Closing service...");
            ACRoot.SRoot.ACDeInit();
            Messages.ConsoleMsg("System", "Service shutdown...");
            if (!Console.IsInputRedirected)
                Console.ReadLine();
        }

        private static ManualResetEvent _ShutdownEvent;
        private static PosixSignalRegistration _SigTermRegistration;

        /// <summary>
        /// Blocks until the user presses a key (interactive console) or the process is
        /// asked to terminate (Ctrl+C / SIGTERM). This makes the console usable
        /// in automated/agent sessions where stdin is redirected or closed.
        /// </summary>
        private static void WaitForShutdown()
        {
            if (!Console.IsInputRedirected)
            {
                Console.ReadLine();
                return;
            }

            // Headless mode: keep the service alive until the process is asked to terminate.
            _ShutdownEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _ShutdownEvent.Set();
            };

            // React to SIGTERM (kill / timeout) as well, not only Ctrl+C (SIGINT).
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                _SigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
                {
                    context.Cancel = true;
                    _ShutdownEvent.Set();
                });
            }

            _ShutdownEvent.WaitOne();
        }

        private static void MainThread(object data)
        {
        }

        #region Global Exception-Handler
        // Handle the UI exceptions by showing a dialog box, and asking the user whether
        // or not they wish to abort execution.
        // NOTE: This exception cannot be kept from terminating the application - it can only 
        // log the event, and inform the user about it. 
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                if (ACRoot.SRoot != null)
                {
                    if (ACRoot.SRoot.Messages != null)
                    {
                        StringBuilder desc = new StringBuilder();
                        StackTrace stackTrace = new StackTrace(ex, true);
                        for (int i = 0; i < stackTrace.FrameCount; i++)
                        {
                            StackFrame sf = stackTrace.GetFrame(i);
                            desc.AppendFormat(" Method: {0}", sf.GetMethod());
                            desc.AppendFormat(" File: {0}", sf.GetFileName());
                            desc.AppendFormat(" Line Number: {0}", sf.GetFileLineNumber());
                            desc.AppendLine();
                        }

                        ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "0", ex.Message);
                        if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
                            ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "0", ex.InnerException.Message);

                        string stackDesc = desc.ToString();
                        if (Database.Root != null)
                            Database.Root.Messages.LogException("App.CurrentDomain_UnhandledException", "Stacktrace", stackDesc);
                    }
                }
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null && Database.Root.InitState == ACInitState.Initialized)
                    Database.Root.Messages.LogException("gip.iplus.console.Program", "CurrentDomain_UnhandledException", msg);
            }
        }
        #endregion
    }
}
