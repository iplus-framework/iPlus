using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Alarmlog'}de{'Alarmlog'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "")]
    [ACPropertyEntity(1, "MessageLevelIndex", "en{'MessageLevelIndex'}de{'de-MessageLevelIndex'}", typeof(eMsgLevel), "", "", true)]
    [ACPropertyEntity(2, Const.ACIdentifierPrefix, "en{'Messagekey'}de{'Fehlerschlüssel'}", "", "", true)]
    [ACPropertyEntity(3, "Message", "en{'Message'}de{'Meldung'}","", "", true)]
    [ACPropertyEntity(5, "TimeStampOccurred", "en{'Occurred'}de{'Aufgetaucht'}","", "", true)]
    [ACPropertyEntity(6, "TimeStampAcknowledged", "en{'Acknowledged'}de{'Quittiert'}","", "", true)]
    [ACPropertyEntity(7, "AcknowledgedBy", "en{'Acknowledged by'}de{'Quittiert von'}","", "", true)]
    [ACPropertyEntity(8, Const.EntityXMLConfig, "en{'Data'}de{'Daten'}")]
    [ACPropertyEntity(9, "Column", "en{'Hashcode Class+Method'}de{'Hashcode Klasse+Methode'}", "", "", true)]
    [ACPropertyEntity(10, "Row", "en{'Row-ID in Method'}de{'Zeilen-ID in Methode'}", "", "", true)]
    [ACPropertyEntity(11, "TranslID", "en{'TranslationID'}de{'ÜbersetzungsID'}", "", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + MsgAlarmLog.ClassName, "en{'AlarmLog'}de{'AlarmLog'}", typeof(MsgAlarmLog), MsgAlarmLog.ClassName, "Source", "Source,TimeStampOccurred")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<MsgAlarmLog>) })]
    public partial class MsgAlarmLog
    {
        public const string ClassName = "MsgAlarmLog";

        #region New/Delete
        public static MsgAlarmLog NewACObject(Database database, IACObject parentACObject)
        {
            MsgAlarmLog entity = new MsgAlarmLog();
            entity.MsgAlarmLogID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.XMLConfig = "";
            entity.TimeStampOccurred = DateTime.MinValue;
            entity.TimeStampAcknowledged = DateTime.MinValue;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        public static MsgAlarmLog NewMsgAlarmLog(Database database, Msg msgToCopy)
        {
            MsgAlarmLog entity = new MsgAlarmLog();
            entity.MsgAlarmLogID = msgToCopy.MsgId;

            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                entity.DefaultValuesACObject();
            }
            entity.XMLConfig = "";
            entity.CopyFromMsg(msgToCopy);
            return entity;
        }

        public void CopyFromMsg(Msg msgToCopy)
        {
            ACIdentifier = msgToCopy.ACIdentifier;
            MessageLevel = msgToCopy.MessageLevel;
            Message = msgToCopy.Message;
            TranslID = msgToCopy.TranslID;
            TimeStampOccurred = (msgToCopy.TimeStampOccurred == null || msgToCopy.TimeStampOccurred.Year < 1900) ? DateTime.Now : msgToCopy.TimeStampOccurred;
            TimeStampAcknowledged = (msgToCopy.TimeStampAcknowledged == null || msgToCopy.TimeStampAcknowledged.Year < 1900) ? DateTime.Now : msgToCopy.TimeStampAcknowledged;
            if (msgToCopy.AcknowledgedBy != null)
                AcknowledgedBy = msgToCopy.AcknowledgedBy;
            //XMLConfig = msgToCopy.XMLConfig;
            Row = msgToCopy.Row;
            Column = msgToCopy.Column;
            if (msgToCopy.SourceComponent != null && msgToCopy.SourceComponent.ValueT != null)
                ACClassID = msgToCopy.SourceComponent.ValueT.ComponentClass.ACClassID;
        }

#endregion

        #region Own Member
        public eMsgLevel MessageLevel
        {
            get
            {
                return (eMsgLevel)this.MessageLevelIndex;
            }
            set
            {
                MessageLevelIndex = (short)value;
            }
        }

        protected string _Source = "";
        [ACPropertyInfo(4, "", "en{'Source'}de{'Quelle'}")]
        public string Source
        {
            get
            {
                if(string.IsNullOrEmpty(_Source))
                {
                    if (ACClass != null && ACClass.ACProject.ACProjectType != Global.ACProjectTypes.ClassLibrary)
                        _Source = ACClass.ACUrlComponent;
                    else if (ACProgramLog != null)
                        _Source = ACProgramLog.ACUrl;
                }
                return _Source;
            }
        }

        #endregion

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(11, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                return Translator.GetACPropertyCaption(ACClass != null ? ACClass.ACUrlComponent : "", ACIdentifier);
            }
        }

        [ACPropertyInfo(12, "", "en{'Caption Component'}de{'Bezeichnung Komponente'}")]
        public String ACCaptionComponent
        {
            get
            {
                return Translator.GetACComponentCaption(ACClass != null ? ACClass.ACUrlComponent : "");
            }
        }


        [ACPropertyInfo(13, "", "en{'Comment Component'}de{'Kommentar Komponente'}")]
        public String ACCommentComponent
        {
            get
            {
                return Translator.GetACComponentComment(ACClass != null ? ACClass.ACUrlComponent : "");
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
            if (/*/string.IsNullOrEmpty(Source) || */string.IsNullOrEmpty(ACIdentifier) || (TimeStampOccurred <= DateTime.MinValue) )
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Source, ACIdentifier, TimeStampOccurred",
                    Message = "Source, ACIdentifier, TimeStampOccurred is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Source, ACIdentifier, TimeStampOccurred"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        static public string KeyACIdentifier
        {
            get
            {
                return "Source,ACIdentifier,TimeStampOccurred";
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
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Alarmlog Statistic'}de{'Alarmlog Statistic'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, false, true)]
    public class MsgAlarmLogStatistic : MsgAlarmLog
    {
        public MsgAlarmLogStatistic(IEnumerable<MsgAlarmLog> alarms)
        {
            var alarmLog = alarms.FirstOrDefault();
            if (alarmLog != null)
            {
                this.Message = alarmLog.Message;
                this.ACIdentifier = alarmLog.ACIdentifier;
                this.TranslID = alarmLog.TranslID;
                this._Source = alarmLog.Source;
            }
            _Alarms = alarms;
            if (_Alarms != null)
            {
                double totalSeconds = _Alarms.Where(a => a.TimeStampAcknowledged > a.TimeStampOccurred).Sum(c => (c.TimeStampAcknowledged - c.TimeStampOccurred).TotalSeconds);
                if (totalSeconds > 0)
                {
                    if (TimeSpan.MaxValue.TotalSeconds > totalSeconds)
                        TotalDuration = string.Format("{0:d\\.hh\\:mm\\:ss}", TimeSpan.FromSeconds(totalSeconds));
                    else
                        TotalDuration = string.Format("{0:d\\.hh\\:mm\\:ss}", TimeSpan.MaxValue);
                }
            }
            OnPropertyChanged("Alarms");
            OnPropertyChanged("TotalDuration");
        }

        [ACPropertyInfo(999,"", "en{'Duration'}de{'Dauer'}")]
        public string TotalDuration
        {
            get;
            set;
        }

        [ACPropertyInfo(999,"", "en{'Alarms Count'}de{'Anzahl der Alarme'}")]
        public int AlarmsCount
        {
            get => Alarms != null ? Alarms.Count() : 0;
        }

        private IEnumerable<MsgAlarmLog> _Alarms;
        [ACPropertyInfo(999, "", "en{'Alarms'}de{'Alarms'}")]
        public IEnumerable<MsgAlarmLog> Alarms
        {
            get => _Alarms;
            set
            {
            }
        }
    }
}




