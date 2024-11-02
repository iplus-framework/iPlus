// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.iplus.service
{
    public class iPlusService
    {
        #region Global Exception-Handler
        // Handle the UI exceptions by showing a dialog box, and asking the user whether
        // or not they wish to abort execution.
        // NOTE: This exception cannot be kept from terminating the application - it can only 
        // log the event, and inform the user about it. 
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
