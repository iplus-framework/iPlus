using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem,"en{'ProgramLogWrapper'}de{'ProgramLogWrapper'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, true)]
    public class ProgramLogWrapper : IACObject
    {
        public ProgramLogWrapper()
        {

        }

        private ACProgramLog _ACProgramLog;
        [ACPropertyInfo(999,"ProgramLog","en{'ProgramLogWrapper'}de{'ProgramLogWrapper'}")]
        public ACProgramLog ACProgramLog
        {
            get
            {
                return _ACProgramLog;
            }
            set 
            {
                _ACProgramLog = value;
            }
        }

        [ACPropertyInfo(100, "ProgramLog", "en{'Start time'}de{'Startzeit'}")]
        public DateTime? StartDate
        {
            get
            {
                return ACProgramLog.StartDateDST;
            }
        }

        [ACPropertyInfo(101, "ProgramLog", "en{'End time'}de{'Endezeit'}")]
        public DateTime? EndDate
        {
            get
            {
                return ACProgramLog.EndDateDST;
            }
        }

        [ACPropertyInfo(102, "ProgramLog", "en{'Planned start time'}de{'Geplante Startzeit'}")]
        public DateTime StartDatePlan
        {
            get
            {
                return ACProgramLog.StartDatePlanDST;
            }
        }

        [ACPropertyInfo(103, "ProgramLog", "en{'Planned end time'}de{'Geplante Endezeit'}")]
        public DateTime EndDatePlan
        {
            get
            {
                return ACProgramLog.EndDatePlanDST;
            }
        }

        [ACPropertyInfo(110, "", "en{'Duration'}de{'Dauer'}")]
        public TimeSpan Duration
        {
            get
            {
                return ACProgramLog.Duration;
            }
        }

        [ACPropertyInfo(111, "", "en{'Planned Duration'}de{'Geplante Dauer'}")]
        [IgnoreDataMember]
        public TimeSpan DurationPlan
        {
            get
            {
                return ACProgramLog.DurationPlan;
            }
        }

        [ACPropertyInfo(112, "", "en{'Duration difference'}de{'Duration difference'}")]
        public TimeSpan DurationDiff
        {
            get
            {
                return ACProgramLog.DurationDiff;
            }
        }

        [ACPropertyInfo(113, "", "en{'Difference start'}de{'Difference start'}")]
        public TimeSpan StartDateDiff
        {
            get
            {
                return ACProgramLog.StartDateDiff;
            }
        }

        [ACPropertyInfo(114, "", "en{'Difference end'}de{'Difference end'}")]
        [IgnoreDataMember]
        public TimeSpan EndDateDiff
        {
            get
            {
                return ACProgramLog.EndDateDiff;
            }
        }

        private List<ProgramLogWrapper> _Items;
        public List<ProgramLogWrapper> Items
        {
            get
            {
                if (_Items == null)
                    _Items = new List<ProgramLogWrapper>();
                return _Items;
            }
        }

        private int _DisplayOrder = 0;
        public int DisplayOrder
        {
            get 
            {
                return _DisplayOrder;
            }
            set 
            {
                _DisplayOrder = value;
            }
        }

        public bool IsShowInSearch = false;
        public bool IsShowInFilter = false;
        public bool ChildAlarm = false;

        private List<Global.TimelineItemStatus> _Status = new List<Global.TimelineItemStatus>();

        public virtual List<Global.TimelineItemStatus> Status
        {
            get
            {
                _Status.Clear();
                if (ACProgramLog.Duration.TotalSeconds == 0)
                    _Status.Add(Global.TimelineItemStatus.Duration);

                if (ACProgramLog.MsgAlarmLog_ACProgramLog.Any())
                    _Status.Add(Global.TimelineItemStatus.Alarm);

                if (ChildAlarm)
                    _Status.Add(Global.TimelineItemStatus.ChildAlarm);

                if (_Status.Any())
                    return _Status;
                else
                    _Status.Add(Global.TimelineItemStatus.OK);

                return _Status;
            }

        }

        public virtual TimelineItemType Type
        {
            get 
            {
                if (ACProgramLog.ACClassID != null)
                {

                    using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                    {
                        ACClass acclass = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACClassID == ACProgramLog.ACClassID);
                        if (acclass.IsDerivedClassFrom("PWProcessFunction") /*&& ACProgramLog.ACProgramLog1_ParentACProgramLog == null*/)
                            return TimelineItemType.Batch;
                    }
                }
                return TimelineItemType.Normal;
            }
        }

        #region IACObject members

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.ACProgramLog.ACIdentifier; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return this.ReflectGetACContentList(); }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

#endregion
    }

    //public enum Global.TimelineItemStatus : short
    //{
    //    OK = 0,
    //    Duration = 1,
    //    Alarm = 2,
    //    ChildAlarm = 3,
    //    //AlarmChildAlarm = 4,
    //    //DurationAlarm = 5,
    //    //DurationChildAlarm = 6,
    //    //DurationAlarmChildAlarm = 7
    //}

    public enum TimelineItemType : short 
    { 
        Normal = 0,
        Batch = 1
    }
}
