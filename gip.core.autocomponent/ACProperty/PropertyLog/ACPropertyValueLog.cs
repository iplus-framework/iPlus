// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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
using FASTER.core;
using gip.core.datamodel;
using System.Globalization;

namespace gip.core.autocomponent
{
    public enum ACPropertyLogStorageBackend
    {
        PersistentDictionary = 0,
        FasterLog = 1
    }

    public static class ACPropertyValueLogSettings
    {
        public static ACPropertyLogStorageBackend StorageBackend { get; set; } = ACPropertyLogStorageBackend.PersistentDictionary;
    }

    public class ACPropertyValueLog<T>
    {
        public static ACPropertyLogStorageBackend StorageBackend
        {
            get => ACPropertyValueLogSettings.StorageBackend;
            set => ACPropertyValueLogSettings.StorageBackend = value;
        }

        private static string FileNameExtension
        {
            get
            {
                switch (StorageBackend)
                {
                    case ACPropertyLogStorageBackend.FasterLog:
                        return ".flg";
                    case ACPropertyLogStorageBackend.PersistentDictionary:
                    default:
                        return ".edb";
                }
            }
        }
        
        public ACPropertyValueLog(ACPropertyNetServerBase<T> property2Log)
        {
            _property2Log = property2Log;
        }

        Global.MaxRefreshRates? _LogRefreshRate;
        public Global.MaxRefreshRates LogRefreshRate
        {
            get
            {
                if (_LogRefreshRate.HasValue)
                    return _LogRefreshRate.Value;
                return (_property2Log.ACType as ACClassProperty).LogRefreshRate;
            }
            set
            {
                _LogRefreshRate = value;
            }
        }

        private T _LastLogValue = default(T);
        private DateTime _LastLogTime = DateTime.MinValue;
        internal void LogCurrentValue()
        {
            if (PropertyLogStore == null)
            {
                if (IsRefreshCycleElapsed && ApplyFilter_IsLogable)
                {
                    _LastLogTime = DateTime.Now;
                    _LastLogValue = _property2Log.ValueT;
                }
                return;
            }

            using (ACMonitor.Lock(_property2Log._20015_LockValue))
            {
                if (_LastLogTime == DateTime.MinValue)
                {
                    try
                    {
                        if (PropertyLogStore.Any())
                        {
                            KeyValuePair<DateTime, T> lastEntry = PropertyLogStore.Last();
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
                    AddValue(_property2Log.ValueT, dtNow);
                    if ((dtNow - _LastSaveTime).TotalMinutes >= 5)
                    {
                        SaveChanges();
                    }
                    _LastLogTime = dtNow;
                    _LastLogValue = _property2Log.ValueT;
                }
            }
        }

        public void AddValue(T value, DateTime stamp)
        {
            if (PropertyLogStore != null)
                PropertyLogStore.Set(stamp, value);
        }

        private const string C_DT_FormatPrefix = "yyyyMM";

        internal PropertyLogListInfo GetValues(DateTime from, DateTime to)
        {
            PropertyLogListInfo pLInfo = null;
            DateTime now = DateTime.Now;
            if (  PropertyLogStore == null
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
                            var dirInfos = dir.GetDirectories("*." + _property2Log.ACIdentifier + FileNameExtension, SearchOption.TopDirectoryOnly);
                            if (dirInfos != null && dirInfos.Any())
                            {
                                foreach (var fileDict in dirInfos)
                                {
                                    DateTime fileMonth;
                                    if (!DateTime.TryParseExact(fileDict.Name.Substring(0,6), C_DT_FormatPrefix, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileMonth))
                                        continue;

                                    //if (   (from >= fileDict.CreationTime
                                    //        && from < fileDict.CreationTime.AddMonths(1))
                                    //    || (to >= fileDict.CreationTime
                                    //        && to < fileDict.CreationTime.AddMonths(1))
                                    //    || (from < fileDict.CreationTime && to > fileDict.CreationTime.AddMonths(1)))
                                    if (   (from.Month == fileMonth.Month && from.Year == fileMonth.Year)
                                        || (to.Month == fileMonth.Month && to.Year == fileMonth.Year))
                                    {
                                        if (PropertyLogStore.StoragePath == fileDict.FullName)
                                            continue;
                                        using (IPropertyLogStorage<T> oldStore = CreateStorage(fileDict.FullName))
                                        {
                                            if (pLInfo == null)
                                            {
                                                pLInfo = new PropertyLogListInfo((_property2Log.ACType as ACClassProperty).LogRefreshRate,
                                                       oldStore.QueryRange(from, to)
                                                            .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value })
                                                            .ToList());
                                            }
                                            else
                                            {
                                                (pLInfo.PropertyLogList as List<PropertyLogItem>).AddRange(oldStore.QueryRange(from, to)
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
                                                       PropertyLogStore.QueryRange(from, to)
                                                            .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value })
                                                            .ToList());
                    }
                    else
                    {
                        (pLInfo.PropertyLogList as List<PropertyLogItem>).AddRange(PropertyLogStore.QueryRange(from, to)
                                    .Select(c => new PropertyLogItem() { Time = c.Key, Value = c.Value }));
                    }
                }
            }
            pLInfo.ACCaption = _property2Log.ACCaption;
            return pLInfo;
        }

        public bool IsRefreshCycleElapsed
        {
            get
            {
                if (_LastLogTime == DateTime.MinValue)
                    return true;
                TimeSpan diff = DateTime.Now - _LastLogTime;
                return diff.IsRefreshRateCycleElapsed(LogRefreshRate);
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

        public bool ApplyFilter_IsLogable
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
        public void SaveChanges(bool isDeInit = false)
        {
            if (isDeInit || _PropertyLogStore == null)
                return;

            using (ACMonitor.Lock(_property2Log._20015_LockValue))
            {
                PropertyLogStore.Flush();
            }
            _LastSaveTime = DateTime.Now;
        }

        private bool _LogDeletionChecked = false;
        private DateTime _PropertyLogFileStamp = DateTime.MinValue;
        private IPropertyLogStorage<T> _PropertyLogStore = null;
        public IPropertyLogStorage<T> PropertyLogStore
        {
            get
            {
                DateTime dtNow = DateTime.Now;

                using (ACMonitor.Lock(_property2Log._20015_LockValue))
                {
                    if ((_PropertyLogStore == null)
                        || (_PropertyLogFileStamp == DateTime.MinValue)
                        || (_PropertyLogFileStamp.Year != dtNow.Year)
                        || (_PropertyLogFileStamp.Month != dtNow.Month))
                    {
                        string fileName = GetFileName(dtNow);
                        if (!String.IsNullOrEmpty(fileName))
                        {
                            _PropertyLogFileStamp = DateTime.MinValue;
                            if (_PropertyLogStore != null)
                            {
                                _PropertyLogStore.Flush();
                                _PropertyLogStore.Dispose();
                            }
                            _PropertyLogStore = null;
                            try
                            {
                                _PropertyLogStore = CreateStorage(fileName);
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
                return _PropertyLogStore;
            }
        }

        private IPropertyLogStorage<T> CreateStorage(string fileName)
        {
            if (StorageBackend == ACPropertyLogStorageBackend.FasterLog)
                return new FasterLogStorage<T>(fileName);
            return new PersistentDictionaryStorage<T>(fileName);
        }

        private string GetFileName(DateTime stamp)
        {
            if (String.IsNullOrEmpty(PathOfDirectory))
                return "";
            return Path.Combine(PathOfDirectory, stamp.ToString(C_DT_FormatPrefix) + "." + _property2Log.ACIdentifier + FileNameExtension);
        }

        private void DeleteOldDictionaries(DateTime stamp)
        {
            _LogDeletionChecked = true;
#if DEBUG
            return;
#else
            if (String.IsNullOrEmpty(PathOfDirectory))
                return;

            DateTime deleteDate = stamp.AddMonths(DeletePropLogAfterXMonths * -1);
            try
            {
                var dir = new DirectoryInfo(PathOfDirectory);
                if (dir != null)
                {
                    var dirInfos = dir.GetDirectories("*." + _property2Log.ACIdentifier + FileNameExtension, SearchOption.TopDirectoryOnly);
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
#endif
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

    public interface IPropertyLogStorage<T> : IDisposable
    {
        string StoragePath { get; }
        bool Any();
        KeyValuePair<DateTime, T> Last();
        IEnumerable<KeyValuePair<DateTime, T>> QueryRange(DateTime from, DateTime to);
        void Set(DateTime stamp, T value);
        void Flush();
    }

    internal sealed class PersistentDictionaryStorage<T> : IPropertyLogStorage<T>
    {
        private readonly PersistentDictionary<DateTime, T> _store;

        public PersistentDictionaryStorage(string storagePath)
        {
            _store = new PersistentDictionary<DateTime, T>(storagePath);
        }

        public string StoragePath => _store.DatabasePath;

        public bool Any() => _store.Any();

        public KeyValuePair<DateTime, T> Last() => _store.Last();

        public IEnumerable<KeyValuePair<DateTime, T>> QueryRange(DateTime from, DateTime to)
        {
            return _store.Where(c => c.Key >= from && c.Key <= to);
        }

        public void Set(DateTime stamp, T value)
        {
            _store[stamp] = value;
        }

        public void Flush()
        {
            _store.Flush();
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }

    internal sealed class FasterLogStorage<T> : IPropertyLogStorage<T>
    {
        private readonly SortedDictionary<DateTime, T> _entries = new SortedDictionary<DateTime, T>();
        private readonly FasterLogSettings _settings;
        private readonly FasterLog _log;

        public FasterLogStorage(string storagePath)
        {
            StoragePath = storagePath;
            Directory.CreateDirectory(StoragePath);

            _settings = new FasterLogSettings(StoragePath);
            _log = new FasterLog(_settings);
            LoadExistingEntries();
        }

        public string StoragePath { get; private set; }

        public bool Any() => _entries.Any();

        public KeyValuePair<DateTime, T> Last() => _entries.Last();

        public IEnumerable<KeyValuePair<DateTime, T>> QueryRange(DateTime from, DateTime to)
        {
            return _entries.Where(c => c.Key >= from && c.Key <= to);
        }

        public void Set(DateTime stamp, T value)
        {
            byte[] data = LogRecordSerializer<T>.Serialize(stamp, value);
            if (!_log.TryEnqueue(data, out _))
                _log.Enqueue(data);

            _entries[stamp] = value;
        }

        public void Flush()
        {
            _log.Commit(true);
        }

        public void Dispose()
        {
            _log.Dispose();
            _settings.Dispose();
        }

        private void LoadExistingEntries()
        {
            using (FasterLogScanIterator iter = _log.Scan(_log.BeginAddress, long.MaxValue))
            {
                byte[] result;
                int length;
                while (iter.GetNext(out result, out length, out _))
                {
                    if (LogRecordSerializer<T>.TryDeserialize(result, length, out DateTime stamp, out T value))
                        _entries[stamp] = value;
                }
            }
        }
    }

    internal static class LogRecordSerializer<T>
    {
        public static byte[] Serialize(DateTime stamp, T value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, true))
            {
                writer.Write(stamp.Ticks);
                bool hasValue = value != null;
                writer.Write(hasValue);
                if (hasValue)
                {
                    string serialized = SerializeValue(value);
                    writer.Write(serialized ?? String.Empty);
                }
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static bool TryDeserialize(byte[] data, int length, out DateTime stamp, out T value)
        {
            stamp = DateTime.MinValue;
            value = default(T);

            try
            {
                using (MemoryStream ms = new MemoryStream(data, 0, length))
                using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8, true))
                {
                    stamp = new DateTime(reader.ReadInt64());
                    bool hasValue = reader.ReadBoolean();
                    if (!hasValue)
                        return true;

                    string serialized = reader.ReadString();
                    value = DeserializeValue(serialized);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string SerializeValue(T value)
        {
            if (value == null)
                return null;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (targetType == typeof(string))
                return value as string;
            if (targetType == typeof(DateTime))
                return ((DateTime)(object)value).ToString("O", CultureInfo.InvariantCulture);
            if (targetType == typeof(TimeSpan))
                return ((TimeSpan)(object)value).ToString("c", CultureInfo.InvariantCulture);
            if (targetType == typeof(Guid))
                return ((Guid)(object)value).ToString("D", CultureInfo.InvariantCulture);
            if (targetType.IsEnum)
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (targetType.IsPrimitive || targetType == typeof(decimal))
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            XmlSerializer serializer = new XmlSerializer(targetType);
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                serializer.Serialize(sw, value);
                return sw.ToString();
            }
        }

        private static T DeserializeValue(string serialized)
        {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object value;

            if (targetType == typeof(string))
                value = serialized;
            else if (targetType == typeof(DateTime))
                value = DateTime.ParseExact(serialized, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            else if (targetType == typeof(TimeSpan))
                value = TimeSpan.ParseExact(serialized, "c", CultureInfo.InvariantCulture);
            else if (targetType == typeof(Guid))
                value = Guid.Parse(serialized);
            else if (targetType.IsEnum)
                value = Enum.Parse(targetType, serialized, true);
            else if (targetType.IsPrimitive || targetType == typeof(decimal))
                value = Convert.ChangeType(serialized, targetType, CultureInfo.InvariantCulture);
            else
            {
                XmlSerializer serializer = new XmlSerializer(targetType);
                using (StringReader sr = new StringReader(serialized))
                {
                    value = serializer.Deserialize(sr);
                }
            }

            return (T)value;
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
            rootPath = pAppManager.ArchivePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            //subPath = forComponent.ACUrl.Replace(pAppManager.ACUrl, "");
            subPath = forComponent.GetACUrl() ?? String.Empty;
            subPath = subPath.TrimStart('\\', '/');
            subPath = subPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
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
            string archivePath = String.IsNullOrEmpty(subPath) ? rootPath : Path.Combine(rootPath, subPath);
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
