using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>VBNoConfiguration is used to define range of numbers for each application table. It also saves the last counter for each application table.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Noconfiguration'}de{'Nummernkonfiguration'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "BSONoConfiguration")]
    [ACPropertyEntity(1, "VBNoConfigurationName", "en{'VBNoConfigurationName'}de{'de-VBNoConfigurationName'}","", "", true)]
    [ACPropertyEntity(2, "CurrentCounter", "en{'CurrentCounter'}de{'de-CurrentCounter'}","", "", true)]
    [ACPropertyEntity(3, "CurrentDate", "en{'CurrentDate'}de{'de-CurrentDate'}","", "", true)]
    [ACPropertyEntity(4, "MaxCounter", "en{'MaxCounter'}de{'de-MaxCounter'}","", "", true)]
    [ACPropertyEntity(5, "MinCounter", "en{'MinCounter'}de{'de-MinCounter'}","", "", true)]
    [ACPropertyEntity(6, "UseDate", "en{'UseDate'}de{'de-UseDate'}","", "", true)]
    [ACPropertyEntity(7, "UsedDelimiter", "en{'UsedDelimiter'}de{'de-UsedDelimiter'}","", "", true)]
    [ACPropertyEntity(8, "UsedPrefix", "en{'UsedPrefix'}de{'de-UsedPrefix'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBNoConfiguration.ClassName, "en{'Noconfiguration'}de{'Nummernkonfiguration'}", typeof(VBNoConfiguration), VBNoConfiguration.ClassName, "VBNoConfigurationName", "VBNoConfigurationName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBNoConfiguration>) })]
    public partial class VBNoConfiguration
    {
        public const string ClassName = "VBNoConfiguration";

        #region New/Delete
        public static VBNoConfiguration NewACObject(Database database, IACObject parentACObject)
        {
            VBNoConfiguration entity = new VBNoConfiguration();
            entity.VBNoConfigurationID = Guid.NewGuid();
            entity.UsedPrefix = "";
            entity.UsedDelimiter = "";
            entity.UseDate = false;
            entity.MinCounter = 10000000;
            entity.MaxCounter = 99999999;
            entity.CurrentDate = DateTime.Now;
            entity.CurrentCounter = entity.MinCounter;
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
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                return VBNoConfigurationName;
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
            if (string.IsNullOrEmpty(VBNoConfigurationName))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
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
                return "VBNoConfigurationName";
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
}
