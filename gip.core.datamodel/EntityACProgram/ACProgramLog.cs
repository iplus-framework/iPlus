using System;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACProgramLog-Entries a made for each workflow-node and all invocations to PAProcessFunction's.
    /// It saved also the passed parameters. The Parameters are stored in a so called virtual Method of type ACClassMethod or it's derivation.
    /// ACClassMethod's are serializable and stored in the property XMLConfig.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Programlog iPlus'}de{'Programmlog iPlus'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACUrlPrefix, "en{'ACUrl'}de{'ACUrl'}", "", "", true)]
    [ACPropertyEntity(2, "Message", "en{'Message'}de{'Meldung'}","", "", true)]
    [ACPropertyEntity(3, "StartDate", "en{'Start date'}de{'Start date'}","", "", true)]
    [ACPropertyEntity(4, "EndDate", "en{'End date'}de{'End date'}","", "", true)]
    [ACPropertyEntity(5, "StartDatePlan","en{'Planed start date'}de{'Planed start date'}","", "", true)]
    [ACPropertyEntity(6, "EndDatePlan", "en{'Planed end date'}de{'Planed end date'}","", "", true)]
    [ACPropertyEntity(9999, "ACProgramLog1_ParentACProgramLog", "en{'Parent Program.Log'}de{'Eltern-Programm.Log'}", Const.ContextDatabase + "\\" + ACProgramLog.ClassName, "", true)]
    [ACPropertyEntity(9999, ACProgram.ClassName, "en{'Program'}de{'Programm'}", Const.ContextDatabase + "\\" + ACProgram.ClassName, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProgramLog.ClassName, "en{'Program.log'}de{'Programm.log'}", typeof(ACProgramLog), ACProgramLog.ClassName, Const.ACUrlPrefix, Const.ACUrlPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProgramLog>) })]
    public partial class ACProgramLog
    {
        public const string ClassName = "ACProgramLog";
        #region New/Delete
        /// <summary>
        /// Handling von Sequencenummer wird automatisch bei der Anlage durchgeführt
        /// </summary>
        public static ACProgramLog NewACObject(Database database, IACObject parentACObject)
        {
            ACProgramLog entity = new ACProgramLog();
            entity.ACProgramLogID = Guid.NewGuid();
            if (parentACObject is ACProgram)
                entity.ACProgram = parentACObject as ACProgram;
            entity.DefaultValuesACObject();
            entity.SetInsertAndUpdateInfo(Database.Initials, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        public override string ACCaption
        {
            get
            {
                return this.ACUrl;
            }
        }

        /// <summary>
        /// Returns ACProgram
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACProgram</value>
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        public override IACObject ParentACObject
        {
            get
            {
                return this.ACProgram;
            }
        }

        #endregion

        #region IACObjectEntity Members
        [IgnoreDataMember]
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACUrlPrefix;
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        partial void OnXMLConfigChanging(global::System.String value)
        {
            bRefreshConfig = false;
            if (!(String.IsNullOrEmpty(value) && String.IsNullOrEmpty(XMLConfig)) && value != XMLConfig)
                bRefreshConfig = true;
        }

        partial void OnXMLConfigChanged()
        {
            if (bRefreshConfig)
                ACProperties.Refresh();
        }

        #endregion

        #region Properties

        [ACPropertyInfo(999,"","en{'Duration'}de{'Duration'}")]
        [IgnoreDataMember]
        public TimeSpan Duration
        {
            get
            {
                return TimeSpan.FromSeconds(DurationSec);
            }
            set
            {
                DurationSec = value.TotalSeconds;
            }
        }

        [ACPropertyInfo(999, "", "en{'Duration'}de{'Duration'}")]
        [IgnoreDataMember]
        public TimeSpan DurationPlan
        {
            get
            {
                return TimeSpan.FromSeconds(DurationSecPlan);
            }
            set
            {
                DurationSecPlan = value.TotalSeconds;
            }
        }

        [ACPropertyInfo(999, "", "en{'Duration difference'}de{'Duration difference'}")]
        [IgnoreDataMember]
        public TimeSpan DurationDiff
        {
            get
            {
                return DurationPlan - Duration;
            }
        }

        [ACPropertyInfo(999, "", "en{'Difference start'}de{'Difference start'}")]
        [IgnoreDataMember]
        public TimeSpan StartDateDiff
        {
            get
            {
                if (!StartDate.HasValue)
                    return TimeSpan.Zero;
                return StartDatePlan - StartDate.Value;
            }
        }

        [ACPropertyInfo(999, "", "en{'Diference end'}de{'Diference end'}")]
        [IgnoreDataMember]
        public TimeSpan EndDateDiff
        {
            get
            {
                if (!EndDate.HasValue)
                    return TimeSpan.Zero;
                return EndDatePlan - EndDate.Value;
            }
        }

        [ACPropertyInfo(999)]
        [IgnoreDataMember]
        public ACMethod Value
        {
            get
            {
                return ACConvert.XMLToObject(typeof(ACMethod), XMLConfig, true, Database.GlobalDatabase) as ACMethod;
            }
        }

        #endregion
    }
}
