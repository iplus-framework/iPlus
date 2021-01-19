using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.communication
{
    /// <summary>
    /// Represents the root component for a components which inherts PAExporterERPBase. 
    /// This component is responsible for a direct invoke web service's methods and 
    /// for manage with unsuccssfull web service's method invokes of the PAExporterERPBase components.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WebService ERP Export'}de{'WebService ERP Export'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAExportERPGroup : PAFileCyclicExport
    {
        #region c'tors

        public PAExportERPGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") : 
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            ReconstructFilesToSendList();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        public const string ClassName = "PAExportERPGroup";

        #endregion

        #region Properties

        //Contains file path
        private List<ERPFileItem> _FilesToSendList = new List<ERPFileItem>();

        private bool _IsNowSending = false;

        private ACMonitorObject _OperationLockObject = new ACMonitorObject(10000);

        public ACMonitorObject SendLockObject = new ACMonitorObject(15000);

        public bool IsAvailableDirectSend
        {
            get
            {
                using (ACMonitor.Lock(_OperationLockObject))
                {
                    return !_FilesToSendList.Any();
                }
            }
        }

        [ACPropertyInfo(999, "", "en{'Is web service supports multiple invokes in same time'}de{'Unterstützt der Webdienst mehrere Aufrufe gleichzeitig'}")]
        public bool IsWSSupportsMultiInvokesInSameTime
        {
            get;
            set;
        }

        #endregion

        #region Methods

        protected override void OnStartScheduling()
        {
            ReconstructFilesToSendList();
            base.OnStartScheduling();
        }

        private void ReconstructFilesToSendList()
        {
            using (ACMonitor.Lock(_OperationLockObject))
            {
                _FilesToSendList.Clear();

                List<string> expDirList = new List<string>();

                foreach (PAExporterERPBase exp in FindChildComponents<PAExporterERPBase>())
                {
                    if (string.IsNullOrEmpty(exp.ExportDir) || !Directory.Exists(exp.ExportDir) || exp.ExportOff || expDirList.Contains(exp.ExportDir))
                        continue;

                    expDirList.Add(exp.ExportDir);
                }

                foreach(string expDir in expDirList)
                {
                    foreach (string filePath in Directory.EnumerateFiles(expDir))
                    {
                        if (_FilesToSendList.Any(c => c.FilePath == filePath))
                            continue;

                        ERPFileItem fileItem = new ERPFileItem(filePath);
                        if (string.IsNullOrEmpty(fileItem.ACIdentifier))
                            continue;
                        _FilesToSendList.Add(fileItem);
                    }
                }

                _FilesToSendList = _FilesToSendList.OrderBy(c => c.SendTime).ToList();
            }
        }

        protected override void RunJob(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            if (!_IsNowSending)
            {
                using (ACMonitor.Lock(_OperationLockObject))
                {
                    _IsNowSending = true;
                    try
                    {
                        var filesToSend = _FilesToSendList.ToArray();

                        foreach(ERPFileItem fileItem in filesToSend)
                        {
                            PAExporterERPBase exporter = FindChildComponents<PAExporterERPBase>(c => c.ACIdentifier == fileItem.ACIdentifier).FirstOrDefault();
                            if (exporter != null)
                            {
                                if (exporter.ReSendObject(fileItem))
                                    _FilesToSendList.Remove(fileItem);
                                else
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PAExportERPGroup.RunJob", e);
                    }
                    finally
                    {
                        _IsNowSending = false;
                    }
                }
            }
            base.RunJob(now, lastRun, nextRun);
        }

        /// <summary>
        /// Add object(ERP file item) to the send cache.
        /// </summary>
        /// <param name="erpFileItem">The ERP file item to add.</param>
        public void AddObjectToSendCache(ERPFileItem erpFileItem)
        {
            using (ACMonitor.Lock(_OperationLockObject))
            {
                if (!_FilesToSendList.Any(c => c.FilePath == erpFileItem.FilePath))
                    _FilesToSendList.Add(erpFileItem);
            }
        }

        /// <summary>
        /// Removes item from send cache and delete it from Export directory.
        /// </summary>
        /// <param name="erpFileItem"></param>
        public void RemoveFromSendCache(ERPFileItem erpFileItem)
        {
            using (ACMonitor.Lock(_OperationLockObject))
            {
                if (!_FilesToSendList.Any(c => c.FilePath == erpFileItem.FilePath))
                {
                    _FilesToSendList.Remove(erpFileItem);
                    if (File.Exists(erpFileItem.FilePath))
                        File.Delete(erpFileItem.FilePath);
                }
            }
        }

        /// <summary>
        /// Tries direct invoke on web service's method and return result. This method is only available on the server side.
        /// </summary>
        /// <typeparam name="C">The type of invoke parameter.</typeparam>
        /// <typeparam name="T">The return type of web service's method</typeparam>
        /// <param name="invokeAction">The web service direct invoke method action.</param>
        /// <param name="invokeParam">The web service's method parameters.</param>
        /// <param name="methodName">The web service's method name.</param>
        /// <param name="result">Returns a method's result.</param>
        /// <returns>Null if is inovked successfully, otherwise false.</returns>
        public Msg InvokeDirect<C,T>(Func<C,T> invokeAction, C invokeParam, string methodName, out T result)
        {
            result = default(T);
            try
            {
                if (IsWSSupportsMultiInvokesInSameTime)
                {
                    result = invokeAction(invokeParam);
                    var res = result;
                }
                else
                {
                    using (ACMonitor.Lock(SendLockObject))
                    {
                        result = invokeAction(invokeParam);
                        var res = result;
                    }
                }
                return null;
            }
            catch(Exception e)
            {
                return new Msg(e.Message, this, eMsgLevel.Exception, ClassName, "InvokeDirect", 184);
            }
        }
        #endregion
    }

    public class ERPFileItem
    {
        public ERPFileItem(string filePath)
        {
            FilePath = filePath;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileNameSplited = fileName.Split(new string[] { "__" }, StringSplitOptions.None);

            DateTime dt = DateTime.MinValue;
            if(DateTime.TryParseExact(fileNameSplited[0], "dd_MM_yyyy-HH_mm_ss_fff", null, System.Globalization.DateTimeStyles.None, out dt))
            {
                SendTime = dt;
                ACIdentifier = fileNameSplited[1];
            }
        }

        public ERPFileItem(DateTime sendTime, string acIdentifier, string exportDir)
        {
            SendTime = sendTime;
            ACIdentifier = acIdentifier;
            FilePath = GenerateFilePath(sendTime, acIdentifier, exportDir);
        }

        public string FilePath
        {
            get;
            set;
        }

        public DateTime SendTime
        {
            get;
            set;
        }

        public string ACIdentifier
        {
            get;
            set;
        }

        public static string GenerateFilePath(DateTime dateTime, string exporterACIdentifier, string exportDir)
        {
            string fileName = GenerateFileName(dateTime, exporterACIdentifier);
            return Path.Combine(exportDir, fileName);
        }

        public static string GenerateFileName(DateTime dateTime, string exporterACIdentifier)
        {
            return string.Format("{0}__{1}.xml", dateTime.ToString("dd_MM_yyyy-HH_mm_ss_fff"), exporterACIdentifier);
        }
    }
}
