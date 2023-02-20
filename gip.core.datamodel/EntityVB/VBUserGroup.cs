using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>Table for assigning users to a group.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Usergroup'}de{'Benutzergruppe'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBGroup", "en{'Usergroup'}de{'Benutzergruppe'}", Const.ContextDatabase + "\\" + VBGroup.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(9999, "VBUser", "en{'User'}de{'Benutzer'}", Const.ContextDatabase + "\\" + VBUser.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUserGroup.ClassName, "en{'Usergroup'}de{'Benutzergruppe'}", typeof(VBUserGroup), VBUserGroup.ClassName, VBGroup.ClassName + "\\VBGroupName", VBGroup.ClassName + "\\VBGroupName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUserGroup>) })]
    [NotMapped]
    public partial class VBUserGroup : IACObjectEntity
    {
        public const string ClassName = "VBUserGroup";

        #region New/Delete
        public static VBUserGroup NewACObject(Database database, IACObject parentACObject)
        {
            VBUserGroup entity = new VBUserGroup();
            entity.VBUserGroupID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBUser)
            {
                entity.VBUser = parentACObject as VBUser; 
            }
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
                return VBGroup.ACCaption + "->" + VBUser.ACCaption;
            }
        }

        /// <summary>
        /// Returns VBUser
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to VBUser</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return VBUser;
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
            base.EntityCheckAdded(user, context);
            if (VBUser == null || VBGroup == null)
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
                return "VBGroup\\VBGroupName";
            }
        }

        #endregion
    }
}

