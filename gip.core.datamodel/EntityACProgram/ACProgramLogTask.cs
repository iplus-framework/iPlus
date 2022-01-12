using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACProgramLogTask is used to store asynchronous method invocations. 
    /// The entries in this table are read into the Connection-List of asynchronous points (ACPointAsyncRMI).
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ProgramLog-Task'}de{'ProgramLog-Task'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]

    [ACPropertyEntity(1, "ACClassMethodXAML", "en{'Method XAML'}de{'Methoden XAML'}","", "", true)]
    [ACPropertyEntity(9999, "ACProgramLog", "en{'Program.Log'}de{'ACProgramm.Log'}", Const.ContextDatabase + "\\" + ACProgramLog.ClassName, "", true)]

    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProgramLogTask.ClassName, "en{'ProgramLogTask'}de{'ProgramLogTask'}", typeof(ACProgramLogTask), ACProgramLogTask.ClassName, "", "")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProgramLogTask>) })]
    public partial class ACProgramLogTask
    {
        public const string ClassName = "ACProgramLogTask";

        #region New/Delete
        /// <summary>
        /// Handling von Sequencenummer wird automatisch bei der Anlage durchgeführt
        /// </summary>
        public static ACProgramLogTask NewACObject(Database database, IACObject parentACObject)
        {
            ACProgramLogTask entity = new ACProgramLogTask();
            entity.ACProgramLogTaskID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.SetInsertAndUpdateInfo(database.UserName, database);
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
        public override string ACCaption
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Returns ACProgramLog
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACProgramLog</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                return this.ACProgramLog;
            }
        }

        #endregion

        #region IACObjectEntity Members


        static public string KeyACIdentifier
        {
            get
            {
                return "";
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
        #endregion
    }
}
