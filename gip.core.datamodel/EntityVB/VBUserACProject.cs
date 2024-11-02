// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>Table that stores which application projects are assigned to a user.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Userproject'}de{'Benutzerprojekt'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, ACProject.ClassName, "en{'Project'}de{'Projekt'}", Const.ContextDatabase + "\\" + ACProject.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(2, "IsClient", "en{'Client'}de{'Client'}","", "", true)]
    [ACPropertyEntity(3, "IsServer", "en{'Server'}de{'Server'}","", "", true)]
    [ACPropertyEntity(9999, "VBUser", "en{'User'}de{'Benutzer'}", Const.ContextDatabase + "\\VBUserList", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUserACProject.ClassName, "en{'Userproject'}de{'Benutzerprojekt'}", typeof(VBUserACProject), VBUserACProject.ClassName, ACProject.ClassName + "\\ACProjectName", ACProject.ClassName + "\\ACProjectName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUserACProject>) })]
    [NotMapped]
    public partial class VBUserACProject : IACObjectEntity
    {
        public const string ClassName = "VBUserACProject";

        #region New/Delete
        public static VBUserACProject NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBUserACProject entity = new VBUserACProject();
            entity.VBUserACProjectID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBUser)
            {
                entity.VBUser = parentACObject as VBUser;
            }
            entity.IsClient = true;
            entity.IsServer = false;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        public static VBUserACProject NewVBUserACProject(Database database, VBUser vbUser, ACProject acProject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBUserACProject entity = VBUserACProject.NewACObject(database, vbUser);
            if (acProject != null)
                entity.ACProject = acProject;
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
                return VBUser.ACCaption + "->" + ACProject.ACCaption;
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
            if (VBUser == null || ACProject == null)
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
                return "ACProject\\ACProjectName";
            }
        }

        #endregion
    }
}
