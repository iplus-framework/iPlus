using gip.core.datamodel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Excel Watcher'}de{'Excel Überwacher'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFileSystemExcelWatcher : PAFileSystemWatcherBase
    {
        #region c´tors
        public PAFileSystemExcelWatcher(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _DelegateQueue = new ACDelegateQueue(this.GetACUrl());
            _DelegateQueue.StartWorkerThread();
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_DelegateQueue != null)
            {
                _DelegateQueue.StopWorkerThread();
                _DelegateQueue = null;
            }
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties

        [ACPropertyInfo(true, 300, "Configuration", "en{'DeleteUnknownFile'}de{'DeleteUnknownFile'}", "", true)]
        [DefaultValue(true)]
        public bool DeleteUnknownFile
        {
            get;
            set;
        }

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {
                return _DelegateQueue;
            }
        }

        #endregion

        #region Methods

        #region Overridden Methods
        protected override bool AnalyzeContentBeforeRaising(ACEventArgs eventArgs)
        {
            WatcherChangeTypes watcherChangeType = (WatcherChangeTypes)eventArgs["WatcherChangeType"];
            bool importerFound = false;
            string fileName = eventArgs["FullPath"] as string;

            if (watcherChangeType != WatcherChangeTypes.Deleted && IsImporterForExcelDocType(eventArgs, fileName))
            {
                try
                {
                    foreach (PAExcelDocImporterBase importer in FindChildComponents<PAExcelDocImporterBase>(c => c is PAExcelDocImporterBase, null, 2))
                    {
                        if (importer.IsImporterForExcelDocType(eventArgs, fileName))
                        {
                            importerFound = true;
                            Messages.LogDebug("ACFileSystemExcelWatcher.AnalyzeContentBeforeRaising(): ", "Importer Found", fileName);
                            _FilesInProcess.Add(fileName);
                            DelegateQueue.Add(
                                delegate ()
                                {
                                    importer.DoImportAndArchive(eventArgs);
                                }
                                );
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException("ACFileSystemExcelWatcher.AnalyzeContentBeforeRaising(): ", "Excel Importer", e.Message);
                }
            }

            if (!importerFound)
            {
                if (!String.IsNullOrWhiteSpace(ForwardDir))
                    ForwardFile(fileName, ForwardDir);

                Messages.LogDebug("ACFileSystemExcelWatcher.AnalyzeContentBeforeRaising(): ", "Importer Not Found", fileName);
                if (!DeleteUnknownFile)
                {
                    // Verschiebe in Mülleimer
                    string movePath = FindAndCreateTrashPath(fileName);
                    MoveOrDeleteFile(true, movePath, fileName);
                }
                else
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception e)
                    {
                        Messages.LogException("ACFileSystemExcelWatcher.AnalyzeContentBeforeRaising(): ", "Delete File", e.Message);
                    }
                }
                return false;
            }
            return true;
        }

        public virtual bool IsImporterForExcelDocType(ACEventArgs eventArgs, string fileName)
        {
            return File.Exists(fileName) && new string[] { ".xlsx", ".xls" }.Contains(System.IO.Path.GetExtension(fileName).ToLower());
        }

        #endregion

        #endregion
    }
}
