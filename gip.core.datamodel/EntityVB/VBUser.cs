using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>Table for managing iPlus-users</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'User'}de{'Benutzer'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBUserName", "en{'Name'}de{'Name'}","", "", true)]
    [ACPropertyEntity(2, "VBUserNo", "en{'Login'}de{'Anmeldename'}","", "", true)]
    [ACPropertyEntity(3, "Initials", "en{'Initials'}de{'Initialen'}","", "", true)]
    [ACPropertyEntity(4, "AllowChangePW", "en{'Can change password'}de{'Darf Passwort ändern'}","", "", true)]
    [ACPropertyEntity(5, "IsSuperuser", "en{'Has unrestricted rights'}de{'Hat uneingeschränkte Rechte'}","", "", true)]
    [ACPropertyEntity(6, "VBLanguage", "en{'Language'}de{'Sprache'}", Const.ContextDatabase + "\\" + VBLanguage.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(9999, "Password", "en{'Password'}de{'Passwort'}","", "", true)]
    [ACPropertyEntity(9999, "MenuACClassDesign", "en{'Main Menu'}de{'Hauptmenü'}", Const.ContextDatabase + "\\MenuACClassList", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUser.ClassName, "en{'User'}de{'Benutzer'}", typeof(VBUser), VBUser.ClassName, "VBUserName", "VBUserName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUser>) })]
    [NotMapped]
    public partial class VBUser
    {
        [NotMapped]
        public const string ClassName = "VBUser";
        [NotMapped]
        public const string NoColumnName = "VBUserNo";
        [NotMapped]
        public const string FormatNewNo = null;

        #region New/Delete
        public static VBUser NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            VBUser entity = new VBUser();
            entity.VBUserID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            entity.VBUserNo = secondaryKey;
            entity.VBLanguage = VBLanguage.DefaultVBLanguage(database);
            entity.IsSuperuser = false;

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return VBUserNo + " " + VBUserName;
            }
        }

        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        public override IACObject GetChildEntityObject(string className, params string[] filterValues)
        {
            if (filterValues.Any())
            {
                switch (className)
                {
                    case VBUserACClassDesign.ClassName:
                        return this.VBUserACClassDesign_VBUser.Where(c => c.ACClassDesign.ACIdentifier == filterValues[0]).FirstOrDefault();
                    case VBUserACProject.ClassName:
                        return this.VBUserACProject_VBUser.Where(c => c.ACProject.ACProjectName == filterValues[0]).FirstOrDefault();
                    case VBUserGroup.ClassName:
                        return this.VBUserGroup_VBUser.Where(c => c.VBGroup.VBGroupName == filterValues[0]).FirstOrDefault();
                    case VBUserInstance.ClassName:
                        return this.VBUserInstance_VBUser.Where(c => c.ServerIPV6 == filterValues[0]).FirstOrDefault();
                }
            }

            return null;
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
            base.EntityCheckAdded(user, context);
            if (string.IsNullOrEmpty(VBUserName))
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
            return null;
        }

        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "VBUserName";
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            using (var context = new iPlusV4Context())
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
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion

        #region Password-Check
        public bool CheckEnteredPassword(string enteredPassword)
        {
            if (String.IsNullOrEmpty(enteredPassword))
                return false;
            string enryptedPass = ACCrypt.GetMd5Hash(enteredPassword);
            return enryptedPass == this.Password || enteredPassword == this.Password;
        }

        [ACPropertyInfo(9999, "", "en{'Password'}de{'Passwort'}", "", true)]
        [NotMapped]
        public string CryptPassword
        {
            get
            {
                return this.Password;
            }
            set
            {
                this.Password = ACCrypt.GetMd5Hash(value);
                OnPropertyChanged("CryptPassword");
            }
        }
        #endregion
    }
}
