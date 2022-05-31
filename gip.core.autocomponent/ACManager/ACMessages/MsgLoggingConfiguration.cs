using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;


namespace gip.core.autocomponent
{
    /// <summary>
    /// Diese Klasse wird zum einlesen der Loggingkonfiguration aus der
    /// Web.config verwendet. 
    /// 
    /// Grundlage für neue Konfigurationsklassen ist die .NET-Basisklasse "ConfigurationSection"
    /// und die entsprechenden Metatags. Siehe die unten stehenden Klassen.
    /// 
    /// Zugriff aus der Anwendung:
    /// gip.mes.client2008.LoggingConfiguration loggingConfiguration =
    ///             (gip.mes.client2008.LoggingConfiguration)System.Configuration.ConfigurationManager.GetSection(
    ///             "Logging/LoggingConfiguration");
    ///
    /// 
    /// Folgende Einträge sind hierfür in der Web.config vor zu nehmen:
    /// 1. &lt;configSections&gt;
    /// Hier werden die neue von uns definierte Konfigurationsgruppe registriert
    /// (Derzeit muß die LoggingConfiguration.cs im gleichen Verzeichnis wie die Web.config liegen)
    /// 2. &lt;Logging&gt;
    /// In diesem Bereich wird die eigentliche Konfiguration eingetragen. Zu deren Bedeutung wird
    /// in der Klasse Logging.cs näher eingegangen
    /// 
    ///&lt;configuration&gt;
    ///    &lt;configSections&gt;
    ///      &lt;sectionGroup name="Logging"&gt;
    ///        &lt;section
    ///          name="LoggingConfiguration"
    ///          type="gip.mes.client2008.LoggingConfiguration, gip.mes.client2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
    ///          allowLocation="true"
    ///          allowDefinition="Everywhere"
    ///      /&gt;
    ///      &lt;/sectionGroup&gt;
    ///    &lt;/configSections&gt;
    ///  &lt;Logging&gt;
    ///    &lt;LoggingConfiguration&gt;
    ///      &lt;LogFiles&gt;
    ///        &lt;addLogFile FileType="DebugLog" FileName="DebugLog%Date%.log" MaxSizeMB="10"/&gt;
    ///        &lt;addLogFile FileType="UserLog" FileName="UserLog%Date%.log" MaxSizeMB="10"/&gt;
    ///        &lt;addLogFile FileType="BookingLog" FileName="Booking%Date%.log" MaxSizeMB="10"/&gt;
    ///      &lt;/LogFiles&gt;
    ///      &lt;LoggingTypes&gt;
    ///        &lt;addLoggingType FileType="DebugLog" MessageType="Debug" Source="default"&gt;&lt;/addLoggingType&gt;
    ///        &lt;addLoggingType FileType="DebugLog" MessageType="Command" Source="default"&gt;&lt;/addLoggingType&gt;
    ///        &lt;addLoggingType FileType="UserLog" MessageType="Info" Source="default"&gt;&lt;/addLoggingType&gt;
    ///        &lt;addLoggingType FileType="UserLog" MessageType="Warning" Source="default"&gt;&lt;/addLoggingType&gt;
    ///        &lt;addLoggingType FileType="UserLog" MessageType="Error" Source="default"&gt;&lt;/addLoggingType&gt;
    ///        &lt;addLoggingType FileType="BookingLog" MessageType="default" Source="BookingResultMessage"&gt;&lt;/addLoggingType&gt;
    ///      &lt;/LoggingTypes&gt;
    ///    &lt;/LoggingConfiguration&gt;
    ///  &lt;/Logging&gt;
    ///&lt;/configuration&gt;
    /// </summary>
    /// 


    public class LoggingConfiguration : ConfigurationSection
    {
        public LoggingConfiguration()
        {
        }

        public LoggingConfiguration(String attribVal)
        {
        }

        [ConfigurationProperty("LogFiles", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(LogFileCollection),
            AddItemName = "addLogFile",
            ClearItemsName = "clearLogFiles",
            RemoveItemName = "RemoveLogFile")]
        public LogFileCollection LogFiles
        {

            get
            {
                LogFileCollection LogFileCollection =
                (LogFileCollection)base["LogFiles"];
                return LogFileCollection;
            }
        }


        [ConfigurationProperty("LoggingTypes", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(LoggingTypeCollection),
            AddItemName = "addLoggingType",
            ClearItemsName = "clearLoggingTypes",
            RemoveItemName = "RemoveLoggingType")]
        public LoggingTypeCollection LoggingTypes
        {
            get
            {
                LoggingTypeCollection LoggingTypeCollection =
                (LoggingTypeCollection)base["LoggingTypes"];
                return LoggingTypeCollection;
            }
        }


        [ConfigurationProperty("UseSimpleMonitor", DefaultValue = false, IsRequired = false)]
        public bool UseSimpleMonitor
        {
            get
            {
                return (bool)this["UseSimpleMonitor"];
            }
            set
            {
                this["UseSimpleMonitor"] = value;
            }
        }


        [ConfigurationProperty("ValidateLockHierarchies", DefaultValue = false, IsRequired = false)]
        public bool ValidateLockHierarchies
        {
            get
            {
                return (bool)this["ValidateLockHierarchies"];
            }
            set
            {
                this["ValidateLockHierarchies"] = value;
            }
        }


        [ConfigurationProperty("DeletePropLogAfterXMonths", DefaultValue = 3, IsRequired = false)]
        public int DeletePropLogAfterXMonths
        {
            get
            {
                return (int)this["DeletePropLogAfterXMonths"];
            }
            set
            {
                this["DeletePropLogAfterXMonths"] = value;
            }
        }

        [ConfigurationProperty("Path", DefaultValue = "", IsRequired = false)]
        public string Path
        {
            get
            {
                return (string)this["Path"];
            }
            set
            {
                this["Path"] = value;
            }
        }

        public string LogFilePath
        {
            get
            {
                if (String.IsNullOrEmpty(Path))
                    return System.IO.Path.GetTempPath();
                if (Path.Contains("\\"))
                    return Path;
                System.Environment.SpecialFolder specialFolder;
                if (Enum.TryParse<System.Environment.SpecialFolder>(Path, out specialFolder))
                    return System.Environment.GetFolderPath(specialFolder);
                return System.IO.Path.GetTempPath();
            }
        }
    }


    public class LogFileCollection : ConfigurationElementCollection
    {
        public LogFileCollection()
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LogFileElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((LogFileElement)element).FileType;
        }

        public LogFileElement this[int index]
        {
            get
            {
                return (LogFileElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public LogFileElement this[string FileType]
        {
            get
            {
                return (LogFileElement)BaseGet(FileType);
            }
        }

        public int IndexOf(LogFileElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(LogFileElement url)
        {
            BaseAdd(url);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(LogFileElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.FileType);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string FileType)
        {
            BaseRemove(FileType);
        }

        public void Clear()
        {
            BaseClear();
        }
    }


    public class LogFileElement : ConfigurationElement
    {
        public LogFileElement(String FileType)
        {
            this.FileType = FileType;
        }

        public LogFileElement()
        {
        }

        [ConfigurationProperty("FileType", DefaultValue = "",
            IsRequired = true, IsKey = true)]
        public string FileType
        {
            get
            {
                return (string)this["FileType"];
            }
            set
            {
                this["FileType"] = value;
            }
        }

        [ConfigurationProperty("FileName", DefaultValue = "",
            IsRequired = true)]
        public string FileName
        {
            get
            {
                return (string)this["FileName"];
            }
            set
            {
                this["FileName"] = value;
            }
        }

        [ConfigurationProperty("MaxSizeMB", DefaultValue = (int)0, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 8080, ExcludeRange = false)]
        public int MaxSizeMB
        {
            get
            {
                return (int)this["MaxSizeMB"];
            }
            set
            {
                this["MaxSizeMB"] = value;
            }
        }

        [ConfigurationProperty("ArchiveAfterDays", DefaultValue = (int)0, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 120, ExcludeRange = false)]
        public int ArchiveAfterDays
        {
            get 
            {
                return (int)this["ArchiveAfterDays"];
            }
            set
            {
                this["ArchiveAfterDays"] = value;
            }
        }

    }


    public class LoggingTypeCollection : ConfigurationElementCollection
    {
        public LoggingTypeCollection()
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggingTypeElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((LoggingTypeElement)element).FileType;
        }

        public LoggingTypeElement this[int index]
        {
            get
            {
                return (LoggingTypeElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public LoggingTypeElement this[string FileType]
        {
            get
            {
                return (LoggingTypeElement)BaseGet(FileType);
            }
        }

        public int IndexOf(LoggingTypeElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(LoggingTypeElement url)
        {
            BaseAdd(url);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(LoggingTypeElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.FileType);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string FileType)
        {
            BaseRemove(FileType);
        }

        public void Clear()
        {
            BaseClear();
        }
    }


    public class LoggingTypeElement : ConfigurationElement
    {
        public LoggingTypeElement(String FileType)
        {
            this.FileType = FileType;
        }

        public LoggingTypeElement()
        {
        }

        [ConfigurationProperty("FileType", DefaultValue = "", IsRequired = true, IsKey = true)]
        public string FileType
        {
            get
            {
                return (string)this["FileType"];
            }
            set
            {
                this["FileType"] = value;
            }
        }

        [ConfigurationProperty("MessageType", DefaultValue = "", IsRequired = true)]
        public string MessageType
        {
            get
            {
                return (string)this["MessageType"];
            }
            set
            {
                this["MessageType"] = value;
            }
        }

        [ConfigurationProperty("Source", DefaultValue = "", IsRequired = true)]
        public string Source
        {
            get
            {
                return (string)this["Source"];
            }
            set
            {
                this["Source"] = value;
            }
        }

        [ConfigurationProperty("ACName", DefaultValue = "", IsRequired = false)]
        public string ACName
        {
            get
            {
                return (string)this["ACName"];
            }
            set
            {
                this["ACName"] = value;
            }
        }

        [ConfigurationProperty("Smtp", DefaultValue = "", IsRequired = false)]
        public string Smtp
        {
            get
            {
                return (string)this["Smtp"];
            }
            set
            {
                this["Smtp"] = value;
            }   
        }


        [ConfigurationProperty("DumpThreadID", DefaultValue = false, IsRequired = false)]
        public bool DumpThreadID
        {
            get
            {
                return (bool)this["DumpThreadID"];
            }
            set
            {
                this["DumpThreadID"] = value;
            }
        }
    }
}

