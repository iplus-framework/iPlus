using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.IO;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.iplus.service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (args == null || args.Count() <= 0)
                args = System.Environment.GetCommandLineArgs();
            CommandLineHelper cmdHelper = new CommandLineHelper(args);
            bool WCFOff = args.Contains("/WCFOff");
            bool simulation = args.Contains("/Simulation");

            ACStartUpRoot startUpManager = new ACStartUpRoot();

            String errorMsg = "";
            // 1. Datenbankverbindung herstellen
            if (startUpManager.LoginUser(cmdHelper.LoginUser, cmdHelper.LoginPassword, false, false, ref errorMsg, WCFOff, simulation) != 1)
            {
                string source = "Vario iPlus Service";
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, "Application");
                EventLog.WriteEntry(source, errorMsg, EventLogEntryType.Information);
                return;
            }
        }

        protected override void OnStop()
        {
            if (ACRoot.SRoot != null)
                ACRoot.SRoot.ACDeInit();
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
                        ACRoot.SRoot.Messages.LogException("App.CurrentDomain_UnhandledException", "Stacktrace", stackDesc);
                    }
                }
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null && Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                    Database.Root.Messages.LogException("gip.iplus.service.Service1", "CurrentDomain_UnhandledException", msg);
            }
        }
        #endregion

    }
}
