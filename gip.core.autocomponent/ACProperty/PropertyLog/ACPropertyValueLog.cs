using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using Microsoft.Isam.Esent.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACPropertyValueLog<T>
    {
        public ACPropertyValueLog(ACPropertyNetServerBase<T> property2Log)
        {
            _property2Log = property2Log;
        }

        private T _LastLogValue = default(T);
        private DateTime _LastLogTime = DateTime.MinValue;
        internal void LogCurrentValue()
        {
            if (PropertyLogDict == null)
                return;

            using (ACMonitor.Lock(_property2Log._20015_LockValue))
            {
                if (_LastLogTime == DateTime.MinValue)
                {
                    try
                    {
                        if (PropertyLogDict.Any())
                        {
                            KeyValuePair<DateTime, T> lastEntry = PropertyLogDict.Last();
                            _LastLogTime = lastEntry.Key;
                            _LastLogValue = lastEntry.Value;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPropertyLog<T>", "LogCurrentValue", msg);
                    }
                }

                if (IsRefreshCycleElapsed && ApplyFilter_IsLogable)
                {
                    DateTime dtNow = DateTime.Now;
                    PropertyLogDict[dtNow] = _property2Log.ValueT;
                    if ((dtNow - _LastSaveTime).TotalMinutes >= 5)
                    {
                        SaveChanges();
                    }
                    _LastLogTime = dtNow;
                    _LastLogValue = _property2Log.ValueT;
                }
            }
        }

        internal PropertyLogListInfo GetValues(DateTime from, DateTime to)
        {
            PropertyLogListInfo pLInfo = null;
            DateTime now = DateTime.Now;
            if (  PropertyLogDict == null
                || from > now
                || to < from)
            {
                pLInfo = new PropertyLogListInfo((_property2Log.ACType as ACClassProperty).LogRefreshRate, new List<PropertyLogItem>());
            }
            else
            {
                // Look in older Archives first
                if (from.Month != now.Month)
                {
                    try
                    {
                        var dir = new DirectoryInfo(PathOfDirectory);
                        if (dir != null)
                        {
                            var dirInfos = dir.GetDirectories("*." + _property2Log.ACIdentifier + ".edb", SearchOption.TopDirectoryOnly);
                            if (dirInfos != null && dirInfos.Any())
                            {
                                foreach (var fileDict in dirInfos)
                                {
                                    if (   (from >= fileDict.CreationTime
                                            && from < fileDict.CreationTime.AddMonths(1))
                                        || (to >= fileDict.CreationTime
                                            && to < fileDict.CreationTime.AddMonths(1))
                                        || (from < fileDict.CreationTime && to > fileDict.CreationTime.AddMonths(1)))
                                    {
                                        if (PropertyLogDict.DatabasePath == fileDict.FullName)
                                            continue;
                                        using (PersistentDictionary<DateTime, T> oldDict = new PersistentDictionary<DateTime, T>(fileDict.FullName))
                                        {
                                            if (pLInfo == null)
                                            {
                                                pLInfo = new PropertyLogListInfo((_property2Log.ACType as ACClassProperty).LogRefreshRate,
                                                       oldDict.Where(c => c.Key >= from && c.Key <= to)
                                                            .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value })
                                                            .ToList());
                                            }
                                            else
                                            {
                                                (pLInfo.PropertyLogList as List<PropertyLogItem>).AddRange(oldDict.Where(c => c.Key >= from && c.Key <= to)
                                                            .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value }));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPropertyLog<T>", "GetValues", msg);
                    }
                }
                using (ACMonitor.Lock(_property2Log._20015_LockValue))
                {
                    if (pLInfo == null)
                    {
                        pLInfo = new PropertyLogListInfo((_property2Log.ACType as ACClassProperty).LogRefreshRate,
                                                       PropertyLogDict.Where(c => c.Key >= from && c.Key <= to)
                                                            .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value })
                                                            .ToList());
                    }
                    else
                    {
                        (pLInfo.PropertyLogList as List<PropertyLogItem>).AddRange(PropertyLogDict.Where(c => c.Key >= from && c.Key <= to)
                                    .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value }));
                    }
                }
            }
            pLInfo.ACCaption = _property2Log.ACCaption;
            return pLInfo;
        }

        private bool IsRefreshCycleElapsed
        {
            get
            {
                if (_LastLogTime == DateTime.MinValue)
                    return true;
                TimeSpan diff = DateTime.Now - _LastLogTime;
                return diff.IsRefreshRateCycleElapsed((_property2Log.ACType as ACClassProperty).LogRefreshRate);
            }
        }

        private Nullable<bool> _IsFilterableType;
        private bool IsFilterableType
        {
            get
            {
                if (_IsFilterableType.HasValue)
                    return _IsFilterableType.Value;

                Type typeT = typeof(T);
                if ((typeT == typeof(SByte))
                    || (typeT == typeof(Int16))
                    || (typeT == typeof(Int32))
                    || (typeT == typeof(Int64))
                    || (typeT == typeof(UInt16))
                    || (typeT == typeof(UInt32))
                    || (typeT == typeof(UInt64))
                    || (typeT == typeof(Single))
                    || (typeT == typeof(Double))
                    || (typeT == typeof(Decimal)))
                    _IsFilterableType = true;
                if (!_IsFilterableType.HasValue)
                    return false;
                if (!(_property2Log.ACType as ACClassProperty).LogFilter.HasValue)
                    return _IsFilterableType.Value;

                //_LogFilter = (T) Convert.ChangeType((_property2Log.ACTypeInfo as ACClassProperty).LogFilter.Value, typeT);
                return _IsFilterableType.Value;
            }
        }

        private bool ApplyFilter_IsLogable
        {
            get
            {
                if (!IsFilterableType)
                    return true;
                if (_property2Log.ValueT == null)
                    return false;
                if (_LastLogValue == null)
                    return true;
                if (!_property2Log.PropertyInfo.LogFilter.HasValue)
                    return true;
                try
                {
                    Type typeDouble = typeof(Double);
                    Double valueT = (Double)Convert.ChangeType(_property2Log.ValueT, typeDouble);
                    Double valueTLast = (Double)Convert.ChangeType(_LastLogValue, typeDouble);

                    if ((valueT >= valueTLast + _property2Log.PropertyInfo.LogFilter.Value)
                        || (valueT <= valueTLast - _property2Log.PropertyInfo.LogFilter.Value))
                    {
                        return true;
                    }
                }
                catch (OverflowException oe)
                {
                    string msg = oe.Message;
                    if (oe.InnerException != null && oe.InnerException.Message != null)
                        msg += " Inner:" + oe.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyLog<T>", "ApplyFilter_IsLogable", msg);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyLog<T>", "ApplyFilter_IsLoagable(10)", msg);
                }
                return false;
            }
        }


        private DateTime _LastSaveTime = DateTime.Now;
        internal void SaveChanges(bool isDeInit = false)
        {
            if (isDeInit && _PropertyLogDict == null)
                return;
            if (PropertyLogDict == null)
                return;

            using (ACMonitor.Lock(_property2Log._20015_LockValue))
            {
                PropertyLogDict.Flush();
            }
            _LastSaveTime = DateTime.Now;
        }

        private bool _LogDeletionChecked = false;
        private DateTime _PropertyLogFileStamp = DateTime.MinValue;
        private PersistentDictionary<DateTime, T> _PropertyLogDict = null;
        public PersistentDictionary<DateTime, T> PropertyLogDict
        {
            get
            {
                DateTime dtNow = DateTime.Now;

                using (ACMonitor.Lock(_property2Log._20015_LockValue))
                {
                    if ((_PropertyLogDict == null)
                        || (_PropertyLogFileStamp == DateTime.MinValue)
                        || (_PropertyLogFileStamp.Year != dtNow.Year)
                        || (_PropertyLogFileStamp.Month != dtNow.Month))
                    {
                        string fileName = GetFileName(dtNow);
                        if (!String.IsNullOrEmpty(fileName))
                        {
                            _PropertyLogFileStamp = DateTime.MinValue;
                            if (_PropertyLogDict != null)
                                _PropertyLogDict.Flush();
                            _PropertyLogDict = null;
                            try
                            {
                                _PropertyLogDict = new PersistentDictionary<DateTime, T>(fileName);
                                _PropertyLogFileStamp = dtNow;
                            }
                            catch (Exception e)
                            {
                                _PropertyLogFileStamp = DateTime.MinValue;
                                if (ACComponent != null)
                                    ACComponent.Messages.LogException(_property2Log.GetACUrl(), "ACProperty2File.PropertyLogDict", e.Message);
                            }
                        }
                        DeleteOldDictionaries(dtNow);
                    }
                    else if (!_LogDeletionChecked)
                        DeleteOldDictionaries(dtNow);
                }
                return _PropertyLogDict;
            }
        }

        private string GetFileName(DateTime stamp)
        {
            if (String.IsNullOrEmpty(PathOfDirectory))
                return "";
            return PathOfDirectory + "\\" + stamp.ToString("yyyyMM") + "." + _property2Log.ACIdentifier + ".edb";
        }

        private void DeleteOldDictionaries(DateTime stamp)
        {
            _LogDeletionChecked = true;
            if (String.IsNullOrEmpty(PathOfDirectory))
                return;
            DateTime deleteDate = stamp.AddMonths(DeletePropLogAfterXMonths * -1);
            try
            {
                var dir = new DirectoryInfo(PathOfDirectory);
                if (dir != null)
                {
                    var dirInfos = dir.GetDirectories("*." + _property2Log.ACIdentifier + ".edb", SearchOption.TopDirectoryOnly);
                    if (dirInfos != null && dirInfos.Any())
                    {
                        var dirsToDelete = dirInfos.Where(c => c.CreationTime < deleteDate).ToArray();
                        foreach (var dirToDelete in dirsToDelete)
                        {
                            dirToDelete.Delete(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPropertyLog<T>", "DeleteOldDictionaries", msg);
            }
        }

        private static int? _DeletePropLogAfterXMonths;
        private static int DeletePropLogAfterXMonths
        {
            get
            {
                if (_DeletePropLogAfterXMonths.HasValue)
                    return _DeletePropLogAfterXMonths.Value;
                else
                {
                    LoggingConfiguration loggingConfiguration = (LoggingConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Logging/LoggingConfiguration");
                    if (loggingConfiguration != null)
                        _DeletePropLogAfterXMonths = loggingConfiguration.DeletePropLogAfterXMonths;
                    if (!_DeletePropLogAfterXMonths.HasValue || _DeletePropLogAfterXMonths.Value <= 0)
                        _DeletePropLogAfterXMonths = 3;
                }
                return _DeletePropLogAfterXMonths.Value;
            }
        }

        private ACPropertyNetServerBase<T> _property2Log = null;
        private IACComponent ACComponent
        {
            get
            {
                return _property2Log.ACRef.ValueT;
            }
        }

        private string _PathOfDirectory = null;
        public string PathOfDirectory
        {
            get
            {
                if (!String.IsNullOrEmpty(_PathOfDirectory))
                    return _PathOfDirectory;
                if (_property2Log == null || _PathOfDirectory == "")
                    return "";
                try
                {
                    if (_property2Log.ACRef.ValueT != null)
                        _PathOfDirectory = _property2Log.ACRef.ValueT.FindAndCreateArchivePath();
                }
                catch (Exception e)
                {
                    if (ACComponent != null)
                        ACComponent.Messages.LogException(_property2Log.GetACUrl(), "ACProperty2File.PathOfDirectory", e.Message);
                    _PathOfDirectory = ""; // Doesn't query ay more, otherwise every time this exception occur the logfile increases unnecessary
                }
                return _PathOfDirectory;
            }
        }

    }

    public static class ACUrl2ArchiveDirectoryManager
    {
        public static void GetArchivePath(this IACComponent forComponent, ref string rootPath, ref string subPath)
        {
            if (forComponent == null)
                throw new ArgumentNullException("forComponent");
            ApplicationManager pAppManager = forComponent.FindParentComponent<ApplicationManager>(c => c is ApplicationManager);
            if (pAppManager == null)
                throw new InvalidOperationException("ApplicationManager not found");
            if (String.IsNullOrEmpty(pAppManager.ArchivePath))
                throw new InvalidOperationException("ArchivePath not set in ApplicationManager");
            rootPath = pAppManager.ArchivePath;
            if (rootPath.Last() == '\\')
                rootPath = rootPath.Substring(0, rootPath.Length - 1);
            //subPath = forComponent.ACUrl.Replace(pAppManager.ACUrl, "");
            subPath = forComponent.GetACUrl();
            if (subPath.First() != '\\')
                subPath = "\\" + subPath;
        }

        public static string FindAndCreateArchivePath(this IACComponent forComponent)
        {
            string rootPath = "";
            string subPath = "";
            try
            {
                GetArchivePath(forComponent, ref rootPath, ref subPath);
            }
            catch (Exception e)
            {
                if (forComponent != null)
                    forComponent.Messages.LogException("ACUrl2ArchiveDirectoryManager.FindAndCreateArchivePath", forComponent.GetACUrl(), e.Message);
            }
            if (!Directory.Exists(rootPath))
                throw new InvalidOperationException("RootPath for Archive doesn't exist");
            string archivePath = rootPath + subPath;
            if (!Directory.Exists(archivePath))
            {
                try
                {
                    DirectoryInfo dirInfo = Directory.CreateDirectory(archivePath);
                }
                catch (Exception e)
                {
                    if (forComponent != null)
                        forComponent.Messages.LogException("ACUrl2ArchiveDirectoryManager.FindAndCreateArchivePath", forComponent.GetACUrl(), e.Message);
                }
            }
            return archivePath;
            /*string nextPath = rootPath;
            string[] acURLDirs = subPath.Split('\\');
            foreach (string urlDir in acURLDirs)
            {
                if (String.IsNullOrEmpty(urlDir))
                    continue;
                nextPath += "\\" + urlDir;
                if (!Directory.Exists(nextPath))
                {
                    Directory.CreateDirectory(nextPath);
                }
            }*/
        }

    }
}
