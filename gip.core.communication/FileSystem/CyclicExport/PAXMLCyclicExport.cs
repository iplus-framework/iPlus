// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;


namespace gip.core.communication
{
    /// <summary>
    /// Exporter for Property-Logs (Root-Node)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Cyclic XML-Export'}de{'Zyklischer XML-Export'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAXMLCyclicExport : PAFileCyclicExport
    {
        #region c´tors
        public PAXMLCyclicExport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        #region overidden methods
        protected override void RunJob(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            //base.RunJob(now, lastRun, nextRun);
            var delegateQueue = DelegateQueue;
            if (InWorkerThread && delegateQueue != null)
            {
                if (LimitActionsInQueue <= 0 || delegateQueue.DelegateQueueCount < LimitActionsInQueue)
                    delegateQueue.Add(() => 
                    {
                        DoExport(now, lastRun, nextRun);
                        MoveToNetFolders(now, lastRun, nextRun);
                    });
            }
            else
            {
                DoExport(now, lastRun, nextRun);
                MoveToNetFolders(now, lastRun, nextRun);
            }
        }

        private void MoveToNetFolders(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            if (this.InitState != ACInitState.Initialized || !this.Root.Initialized)
                return;

            try
            {
                foreach (PAXMLDocExporterBase exporter in FindChildComponents<PAXMLDocExporterBase>(c => c is PAXMLDocExporterBase))
                {
                    if (!String.IsNullOrWhiteSpace(exporter.MoveToNetDir)
                        && !String.IsNullOrWhiteSpace(exporter.ExportDir))
                    {
                        DirectoryInfo sourceDir = new DirectoryInfo(exporter.ExportDir);
                        if (sourceDir.Exists)
                        {
                            FileInfo[] sourceFiles = sourceDir.GetFiles();
                            if (sourceFiles.Any())
                            {
                                DirectoryInfo targetDir = new DirectoryInfo(exporter.MoveToNetDir);
                                if (targetDir.Exists)
                                {
                                    foreach (FileInfo sourceFile in sourceFiles)
                                    {
                                        string targetPath = System.IO.Path.Combine(exporter.MoveToNetDir, sourceFile.Name);
                                        sourceFile.MoveTo(targetPath);
                                    }
                                }
                                else
                                {
                                    string message = String.Format("Networkfolder {0} coudn't be open!", exporter.MoveToNetDir);
                                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                                    Msg msg = new Msg(message, this, eMsgLevel.Exception, "PAXMLCyclicExport", "MoveToNetFolders", 1020);
                                    ErrorText.ValueT = msg.Message;
                                    Messages.LogException(this.GetACUrl(), "MoveToNetFolders(1020)", msg.Message);
                                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                                }
                            }
                        }
                        else
                        {
                            string message = String.Format("Exportfolder {0} coudn't be open!", exporter.ExportDir);
                            IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                            Msg msg = new Msg(message, this, eMsgLevel.Exception, "PAXMLCyclicExport", "MoveToNetFolders", 1010);
                            ErrorText.ValueT = msg.Message;
                            Messages.LogException(this.GetACUrl(), "MoveToNetFolders(1010)", msg.Message);
                            OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                if (e.InnerException != null)
                    message += System.Environment.NewLine + e.InnerException.Message;
                IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                Msg msg = new Msg(message, this, eMsgLevel.Exception, "PAXMLCyclicExport", "MoveToNetFolders", 1900);
                ErrorText.ValueT = msg.Message;
                Messages.LogException(this.GetACUrl(), "MoveToNetFolders(1900)", msg.Message);
                OnNewAlarmOccurred(IsExportingAlarm, msg, true);
            }
        }
        #endregion
        
        #endregion
    }
}
