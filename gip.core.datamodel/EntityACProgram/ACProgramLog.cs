using System;
using System.ComponentModel.DataAnnotations.Schema;
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
    [ACPropertyEntity(3, "StartDate", "en{'Start time'}de{'Startzeit'}","", "", true)]
    [ACPropertyEntity(4, "EndDate", "en{'End time'}de{'Endezeit'}","", "", true)]
    [ACPropertyEntity(5, "StartDatePlan","en{'Planned start time'}de{'Geplante Startzeit'}","", "", true)]
    [ACPropertyEntity(6, "EndDatePlan", "en{'Planned end time'}de{'Geplante Endezeit'}","", "", true)]
    [ACPropertyEntity(9999, "ACProgramLog1_ParentACProgramLog", "en{'Parent Program.Log'}de{'Eltern-Programm.Log'}", Const.ContextDatabase + "\\" + ACProgramLog.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(9999, ACProgram.ClassName, "en{'Program'}de{'Programm'}", Const.ContextDatabase + "\\" + ACProgram.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProgramLog.ClassName, "en{'Program.log'}de{'Programm.log'}", typeof(ACProgramLog), ACProgramLog.ClassName, Const.ACUrlPrefix, Const.ACUrlPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProgramLog>) })]
    [NotMapped]
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
            if (parentACObject != null && parentACObject is ACProgram)
            {
                //using (ACMonitor.Lock(database.QueryLock_1X000))
                //{
                    entity.ACProgram = parentACObject as ACProgram;
                //}
            }
            entity.DefaultValuesACObject();
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        [NotMapped]
        ACProgramLog _NewParentACProgramLogForQueue;
        [NotMapped]
        public ACProgramLog NewParentACProgramLogForQueue
        {
            get
            {
                return _NewParentACProgramLogForQueue;
            }
            set
            {
                _NewParentACProgramLogForQueue = value;
            }
        }

        [NotMapped]
        ACProgram _NewACProgramForQueue;
        [NotMapped]
        public ACProgram NewACProgramForQueue
        {
            get
            {
                return _NewACProgramForQueue;
            }
            set
            {
                _NewACProgramForQueue = value;
            }
        }

        public void PublishToChangeTrackerInQueue()
        {
            ACProgram = _NewACProgramForQueue;
            ACProgramLog1_ParentACProgramLog = _NewParentACProgramLogForQueue;
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
        [NotMapped]
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
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                var context = this.GetObjectContext();
                if (context != null)
                {
                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        return this.ACProgram;
                    }
                }
                return this.ACProgram;
            }
        }

        #endregion

        #region IACObjectEntity Members
        [IgnoreDataMember]
        [NotMapped]
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

        #region Properties

        [ACPropertyInfo(110,"","en{'Duration'}de{'Dauer'}")]
        [IgnoreDataMember]
        [NotMapped]
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

        [ACPropertyInfo(111, "", "en{'Planned Duration'}de{'Geplante Dauer'}")]
        [IgnoreDataMember]
        [NotMapped]
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

        [ACPropertyInfo(112, "", "en{'Duration difference'}de{'Duration difference'}")]
        [IgnoreDataMember]
        [NotMapped]
        public TimeSpan DurationDiff
        {
            get
            {
                return DurationPlan - Duration;
            }
        }

        [ACPropertyInfo(113, "", "en{'Difference start'}de{'Difference start'}")]
        [IgnoreDataMember]
        [NotMapped]
        public TimeSpan StartDateDiff
        {
            get
            {
                if (!StartDate.HasValue)
                    return TimeSpan.Zero;
                return StartDatePlan - StartDate.Value;
            }
        }

        [ACPropertyInfo(114, "", "en{'Difference end'}de{'Difference end'}")]
        [IgnoreDataMember]
        [NotMapped]
        public TimeSpan EndDateDiff
        {
            get
            {
                if (!EndDate.HasValue)
                    return TimeSpan.Zero;
                return EndDatePlan - EndDate.Value;
            }
        }

        [ACPropertyInfo(100, "ProgramLog", "en{'Start time'}de{'Startzeit'}")]
        [NotMapped]
        public DateTime? StartDateDST
        {
            get
            {
                return StartDate.HasValue ? StartDate.Value.GetDateTimeDSTCorrected() : StartDate;
            }
        }

        [ACPropertyInfo(101, "ProgramLog", "en{'End time'}de{'Endezeit'}")]
        [NotMapped]
        public DateTime? EndDateDST
        {
            get
            {
                return EndDate.HasValue ? EndDate.Value.GetDateTimeDSTCorrected() : EndDate;
            }
        }

        [ACPropertyInfo(102, "ProgramLog", "en{'Planned start time'}de{'Geplante Startzeit'}")]
        [NotMapped]
        public DateTime StartDatePlanDST
        {
            get
            {
                return StartDatePlan.GetDateTimeDSTCorrected();
            }
        }

        [ACPropertyInfo(103, "ProgramLog", "en{'Planned end time'}de{'Geplante Endezeit'}")]
        [NotMapped]
        public DateTime EndDatePlanDST
        {
            get
            {
                return EndDatePlan.GetDateTimeDSTCorrected();
            }
        }


        [ACPropertyInfo(999)]
        [IgnoreDataMember]
        [NotMapped]
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
