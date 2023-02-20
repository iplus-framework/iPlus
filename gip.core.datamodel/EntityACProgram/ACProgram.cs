using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// Each new invocation of a workflow is stored into ACProgram. If Workflows invokes subworkflow a new ACProgram will not be created. Instead the ACProgramLog refrences to a parent ACProgramLog.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Program'}de{'Programm'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ProgramNo", "en{'ProgramNo'}de{'Programmnummer'}", "", "", true)]
    [ACPropertyEntity(2, "ProgramName", "en{'Programname'}de{'Programmname'}", "", "", true)]
    [ACPropertyEntity(9999, "ACProgramTypeIndex", "en{'Programtype'}de{'Programmart'}", typeof(Global.ACProgramTypes), "", "", true)]
    [ACPropertyEntity(9999, "PlannedStartDate", "en{'Planned Starttime'}de{'Gepl.Startzeit'}", "", "", true)]
    [ACPropertyEntity(9999, "Comment", "en{'Comment'}de{'Bemerkung'}", "", "", true)]
    [ACPropertyEntity(9999, "WorkflowTypeACClass", "en{'Programtype'}de{'Programmart'}", Const.ContextDatabase + "\\WorkflowTypeMethodACClassList", "", true)]
    [ACPropertyEntity(9999, "ProgramACClassMethod", "en{'Method'}de{'Methode'}", "", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProgram.ClassName, "en{'Program'}de{'Programm'}", typeof(ACProgram), ACProgram.ClassName, "ProgramNo", "ProgramNo", new object[]
        {
                new object[] {Const.QueryPrefix + ACProgramConfig.ClassName, "en{'Program.config'}de{'Programm.config'}", typeof(ACProgramConfig), ACProgramConfig.ClassName + "_" + ACProgram.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                new object[] {Const.QueryPrefix + ACProgramLog.ClassName, "en{'Program.log'}de{'Programm.log'}", typeof(ACProgramLog), ACProgramLog.ClassName + "_" + ACProgram.ClassName, Const.ACUrlPrefix, Const.ACUrlPrefix}
        })
    ]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProgram>) })]
    [NotMapped]
    public partial class ACProgram : IACObject, IACWorkflowContext, IACConfigStore, IACClassEntity
    {
        public const string ClassName = "ACProgram";
        public const string NoColumnName = "ProgramNo";
        public const string FormatNewNo = "P{0}";
        public readonly ACMonitorObject _10020_LockValue = new ACMonitorObject(10020);

        #region new/Delete
        public static ACProgram NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACProgram entity = new ACProgram();
            entity.ACProgramID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.ProgramNo = secondaryKey;
            entity.ProgramName = "";
            entity.PlannedStartDate = DateTime.Now;
            entity.Comment = "";
            entity.ACProgramType = Global.ACProgramTypes.Precompiled;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return ProgramNo + " " + ProgramName;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (string.IsNullOrEmpty(ProgramNo))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ProgramNo",
                    Message = "ProgramNo is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ProgramNo"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ProgramNo";
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig)
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion

        #region properties
        [NotMapped]
        public Global.ACProgramTypes ACProgramType
        {
            get
            {
                return (Global.ACProgramTypes)ACProgramTypeIndex;
            }
            set
            {
                ACProgramTypeIndex = (short)value;
            }
        }
        #endregion

        #region IACConfigStore

        private string configStoreName;
        [NotMapped]
        public string ConfigStoreName
        {
            get
            {
                if (configStoreName == null)
                {
                    ACClassInfo acClassInfo = (ACClassInfo)GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0];
                    configStoreName = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
                }
                return configStoreName;
            }
        }
        /// <summary>
        /// ACConfigKeyACUrl returns the relative Url to the "main table" in group a group of semantically related tables.
        /// This property is used when NewACConfig() is called. NewACConfig() creates a new IACConfig-Instance and set the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        [NotMapped]
        public string ACConfigKeyACUrl
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and adds a new IACConfig-Entry to ConfigItemsSource.
        /// The implementing class creates a new entity object an add it to its "own Configuration-Table".
        /// It sets automatically the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        /// <param name="acObject">Optional: Reference to another Entity-Object that should be related for this new configuration entry.</param>
        /// <param name="valueTypeACClass">The iPlus-Type of the "Value"-Property.</param>
        /// <param name="localConfigACUrl">localConfigACUrl</param>
        /// <returns>IACConfig as a new entry</returns>
        public IACConfig NewACConfig(IACObjectEntity acObject = null, gip.core.datamodel.ACClass valueTypeACClass = null, string localConfigACUrl = null)
        {
            var database = Database != null ? Database : this.GetObjectContext<Database>();
            ACProgramConfig acConfig = null;
            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                acConfig = ACProgramConfig.NewACObject(this.GetObjectContext<Database>(), this);
                acConfig.KeyACUrl = ACConfigKeyACUrl;
                acConfig.LocalConfigACUrl = localConfigACUrl;
                acConfig.ValueTypeACClass = valueTypeACClass;
                ACProgramConfig_ACProgram.Add(acConfig);
            }
            ACConfigListCache.Add(acConfig);
            return acConfig;
        }

        /// <summary>Removes a configuration from ConfigurationEntries and the database-context.</summary>
        /// <param name="acObject">Entry as IACConfig</param>
        public void RemoveACConfig(IACConfig acObject)
        {
            ACProgramConfig acConfig = acObject as ACProgramConfig;
            if (acConfig == null)
                return;
            if (ACConfigListCache.Contains(acConfig))
                ACConfigListCache.Remove(acConfig);
            var database = Database != null ? Database : this.GetObjectContext<Database>();
            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                ACProgramConfig_ACProgram.Remove(acConfig);
                if (acConfig.EntityState != EntityState.Detached)
                    acConfig.DeleteACObject(database, false);
            }
        }

        /// <summary>
        /// Deletes all IACConfig-Entries in the Database-Context as well as in ConfigurationEntries.
        /// </summary>
        public void DeleteAllConfig()
        {
            if (!ConfigurationEntries.Any())
                return;
            ClearCacheOfConfigurationEntries();

            List<IACConfig> list = ConfigurationEntries.ToList();
            var database = Database != null ? Database : this.GetObjectContext<Database>();
            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                foreach (var acConfig in list)
                {
                    (acConfig as ACProgramConfig).DeleteACObject(database, false);
                }
            }
            ClearCacheOfConfigurationEntries();
        }

        [NotMapped]
        public decimal OverridingOrder { get; set; }

        /// <summary>
        /// A thread-safe and cached list of Configuration-Values of type IACConfig.
        /// </summary>
        [NotMapped]
        public IEnumerable<IACConfig> ConfigurationEntries
        {
            get
            {
                return ACConfigListCache;
            }
        }

        [NotMapped]
        private SafeList<IACConfig> _ACConfigListCache;
        [NotMapped]
        private SafeList<IACConfig> ACConfigListCache
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_ACConfigListCache != null)
                        return _ACConfigListCache;
                }
                SafeList<IACConfig> newSafeList = new SafeList<IACConfig>();
                try
                {
                    List<ACProgramConfig> configList = new List<ACProgramConfig>();
                    var database = Database != null ? Database : this.GetObjectContext<Database>();
                    using (ACMonitor.Lock(database.QueryLock_1X000))
                    {
                        if (ACProgramConfig_ACProgram_IsLoaded)
                        {
                            ACProgramConfig_ACProgram.AutoRefresh(ACProgramConfig_ACProgramReference, this);
                            ACProgramConfig_ACProgram.AutoLoad(ACProgramConfig_ACProgramReference, this);
                        }
                        configList = ACProgramConfig_ACProgram.ToList();
                    }
                    newSafeList = new SafeList<IACConfig>(configList.Select(x => (IACConfig)x));
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACProgram", "ACConfigListCache", msg);
                }
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    _ACConfigListCache = newSafeList;
                    return _ACConfigListCache;
                }
            }
        }

        /// <summary>Clears the cache of configuration entries. (ConfigurationEntries)
        /// Re-accessing the ConfigurationEntries property rereads all configuration entries from the database.</summary>
        public void ClearCacheOfConfigurationEntries()
        {
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = null;
            }
        }

        /// <summary>
        /// Checks if cached configuration entries are loaded from database successfully
        /// </summary>
        public bool ValidateConfigurationEntriesWithDB(ConfigEntriesValidationMode mode = ConfigEntriesValidationMode.AnyCheck)
        {
            if (mode == ConfigEntriesValidationMode.AnyCheck)
            {
                if (ConfigurationEntries.Any())
                    return true;
            }
            using (Database database = new Database())
            {
                var query = database.ACProgramConfig.Where(c => c.ACProgramID == ACProgramID);
                if (mode == ConfigEntriesValidationMode.AnyCheck)
                {
                    if (query.Any())
                        return false;
                }
                else if (mode == ConfigEntriesValidationMode.CompareCount
                         || mode == ConfigEntriesValidationMode.CompareContent)
                    return query.Count() == ConfigurationEntries.Count();
            }
            return true;
        }

        [NotMapped]
        public Database Database
        {
            get
            {
                return Context as Database;
            }
        }

#endregion

#region IACWorkflow
        [NotMapped]
        public IACWorkflowNode RootWFNode
        {
            get
            {
                if (ProgramACClassMethod == null)
                    return null;
                return ProgramACClassMethod.RootWFNode;
            }
        }

        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        [NotMapped]
        public String XMLDesign
        {
            get
            {
                return ProgramACClassMethod.XMLDesign;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
#endregion

#region Applicationdata
        [ACPropertyInfo(9999, "en{'Applicationdata'}de{'Anwendungsdaten'}", "gip.core.autocomponent.ApplicationdataConfig", "en{'Applicationdata'}de{'Anwendungsdaten'}")]
        [NotMapped]
        public ACProgramConfig Applicationdata
        {
            get
            {
                string keyACUrl = ".\\ACClassProperty(Applicationdata)";

                using (ACMonitor.Lock(Database != null ? Database.QueryLock_1X000 : Database.GlobalDatabase.QueryLock_1X000))
                {
                    return ACProgramConfig_ACProgram.Where(c => c.KeyACUrl == keyACUrl).FirstOrDefault();
                }
            }
        }
#endregion

    }
}
