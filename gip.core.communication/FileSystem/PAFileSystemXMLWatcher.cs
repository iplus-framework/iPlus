using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace gip.core.communication
{
    /// <summary>
    /// File-System-Watcher
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'XML Watcher'}de{'XML Überwacher'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFileSystemXMLWatcher : PAFileSystemWatcherBase
    {
        #region c´tors
        public PAFileSystemXMLWatcher(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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
            string rootNode = "";
            bool importerFound = false;
            XmlTextReader xmlReader = null;
            string fileName = eventArgs["FullPath"] as string;

            if (watcherChangeType != WatcherChangeTypes.Deleted)
            {
                try
                {
                    xmlReader = new XmlTextReader(fileName);

                    while (xmlReader.Read())
                    {
                        rootNode = xmlReader.LocalName;
                        if (!String.IsNullOrEmpty(rootNode) && rootNode != "xml" && rootNode != "xml")
                            break;
                    }
                    eventArgs.GetACValue("FileContentInfo").Value = rootNode;

                    if (!String.IsNullOrEmpty(rootNode))
                    {
                        //Test only
                        //List<PAXMLDocImporterBase> testBase = FindChildComponents<PAXMLDocImporterBase>(c => c is PAXMLDocImporterBase, 2);

                        foreach (PAXMLDocImporterBase importer in FindChildComponents<PAXMLDocImporterBase>(c => c is PAXMLDocImporterBase, null, 2))
                        {
                            if (importer.IsImporterForXMLDocType(eventArgs, xmlReader))
                            {
                                xmlReader.Close();
                                importerFound = true;
                                Messages.LogDebug("ACFileSystemXMLWatcher.AnalyzeContentBeforeRaising(): ", "Importer Found", fileName);
                                _FilesInProcess.Add(fileName);
                                DelegateQueue.Add(
                                    delegate()
                                    {
                                        importer.DoImportAndArchive(eventArgs);
                                    }
                                    );
                                break;
                            }
                        }
                    }
                    xmlReader.Close();
                }
                catch (Exception e)
                {
                    if (xmlReader != null)
                        xmlReader.Close();
                    Messages.LogException("ACFileSystemXMLWatcher.AnalyzeContentBeforeRaising(): ", "XmlTextReader", e.Message);
                }
            }
            xmlReader = null;

            if (!importerFound)
            {
                Messages.LogDebug("ACFileSystemXMLWatcher.AnalyzeContentBeforeRaising(): ", "Importer Not Found", fileName);
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
                        Messages.LogException("ACFileSystemXMLWatcher.AnalyzeContentBeforeRaising(): ", "Delete File", e.Message);
                    }
                }
                return false;
            }
            return true;
        }
        #endregion

        #endregion

    }
}
