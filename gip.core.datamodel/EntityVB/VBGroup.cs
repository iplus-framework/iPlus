using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>Table for declaring groups for the rightmanagement in iPlus.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Group'}de{'Gruppe'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBGroupName", "en{'Group Name'}de{'Gruppenname'}","", "", true)]
    [ACPropertyEntity(2, "Description", "en{'Description'}de{'Beschreibung'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBGroup.ClassName, "en{'Usergroup'}de{'Benutzergruppe'}", typeof(VBGroup), VBGroup.ClassName, "VBGroupName", "VBGroupName", new object[]
        {
            new object[] {Const.QueryPrefix + VBGroupRight.ClassName, "en{'Usergroupright'}de{'Benutzergruppenrechte'}", typeof(VBGroupRight), VBGroupRight.ClassName + "_" + VBGroup.ClassName, VBGroup.ClassName +"\\VBGroupName", VBGroup.ClassName + "\\VBGroupName"}
        }
   )]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBGroup>) })]
    [NotMapped]
    public partial class VBGroup
    {
        public const string ClassName = "VBGroup";

        #region New/Delete
        public static VBGroup NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBGroup entity = new VBGroup();
            entity.VBGroupID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            entity.Description = "";
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
                return VBGroupName;
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
            if (string.IsNullOrEmpty(VBGroupName))
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

        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "VBGroupName";
            }
        }

        #endregion
    }
}
